---
name: dotnet-api-services
description: Cómo escribir servicios (interfaz + implementación), su registro en Program.cs y la autenticación JWT en las APIs .NET del usuario. Úsala al crear o modificar lógica de negocio, acceso a datos con EF Core, un TokenService o el registro de DI.
---

# Services (`Services/`)

Un par de archivos por servicio: `I<Nombre>.cs` y `<Nombre>.cs`, namespace `<Proyecto>.Services`. Nombre de la clase sin sufijo obligatorio (`AuthUsers`), salvo infraestructura (`TokenService`).

## Interfaz
Métodos `Task<...>` con sufijo `Async`, parámetros con nombre del DTO (`userRequest`, `loginRequest`). En interfaces de infraestructura puede ir `public` en los miembros (`ITokenService`).

```csharp
public interface IAuthUsers
{
    Task<User> RegisterUserAsync(UserRequest userRequest);
    Task<User> LoginUserAsync(UserLoginRequest loginRequest);
    Task<List<UserResponse>> GetAllUsersAsync();
}
```

## Implementación
- Primary constructor con dependencias como `_context`, `_hasher` (con guion bajo en servicios): `public class AuthUsers(AppDbContext _context, IPasswordHasher<User> _hasher) : IAuthUsers`.
- **Los fallos son excepciones de dominio**, nunca `null`: `ConflictException` (409, ya existe), `InvalidCredentialsException` (401, login con usuario inexistente, desactivado o contraseña incorrecta; mismo error en los tres casos), `NotFoundException` (404), `ForbiddenException` (403), `InvalidInputException` (400). Viven en `Exceptions/` con mensaje en inglés. El servicio solo las lanza; `ApiExceptionHandler` (`IExceptionHandler`) es el único lugar que las traduce a HTTP, con `ProblemDetails` y un campo `code`. Se registra con `AddExceptionHandler<ApiExceptionHandler>()`, `AddProblemDetails()` y `app.UseExceptionHandler()`.
- Los servicios devuelven la entidad sin nulabilidad (`User`); el mapeo a Response lo hace el controlador. Excepción: listados, donde se proyecta con `.Select(... new <X>Response {...}).ToListAsync()`.
- Helpers privados al final de la clase (`UserExistsAsync`).
- Guardas con llaves y `throw` dentro: `if (user is null) { throw new NotFoundException("User not found."); }`. Si el índice único puede fallar por concurrencia, captura `DbUpdateException` y lanza `ConflictException`.
- Contraseñas: `_hasher.HashPassword(newUser, plain)` / `VerifyHashedPassword(...) == PasswordVerificationResult.Failed`.
- Persistencia: `_context.X.Add(...)` y `await _context.SaveChangesAsync()`.

```csharp
public async Task<User> RegisterUserAsync(UserRequest user)
{
    var existingUser = await UserExistsAsync(user.Username);
    if (existingUser != null)
    {
        throw new ConflictException("Username already exists.");
    }

    var newUser = new User { Username = user.Username };
    newUser.Password = _hasher.HashPassword(newUser, user.Password);

    _context.Users.Add(newUser);
    await _context.SaveChangesAsync();

    return newUser;
}
```

## TokenService (JWT)
Configuración tipada con `JwtSettings` (clase con `SectionName = "Jwt"` y propiedades `Key`, `Issuer`, `Audience`, `AccessTokenMinutes`, validadas con data annotations: `Key` con `MinLength(32)`, `AccessTokenMinutes` con `Range(1, 1440)`). Nunca leer claves con `IConfiguration` ni dejar el plazo fijo en código.

- `TokenService` recibe `IOptions<JwtSettings> options` en el primary constructor y guarda `options.Value` en un campo privado `jwt`.
- Firma HMAC-SHA256 con `jwt.Key`; claims `sub` (id), `email`, `unique_name` (nombre) y `ClaimTypes.Role`; expira en `jwt.AccessTokenMinutes` minutos; se emite con `JsonWebTokenHandler().CreateToken(descriptor)`.
- `CreateToken(User)` devuelve `(string Token, DateTime ExpiresAt)`.
- En `Program.cs`: `AddOptions<JwtSettings>().BindConfiguration(JwtSettings.SectionName)` con `ValidateDataAnnotations()` y `ValidateOnStart()`; los `TokenValidationParameters` usan una instancia leída de la misma sección (`GetSection(...).Get<JwtSettings>()`). Cualquier cambio al emitir debe coincidir en la validación.
- `Issuer`, `Audience` y `AccessTokenMinutes` van en `appsettings.json`; solo `Key` cambia por entorno (`appsettings.Development.json` o `Jwt__Key`).

## Rate limiting
Middleware nativo `Microsoft.AspNetCore.RateLimiting`, sin paquetes extra. Los límites salen de configuración, nunca del código.

- `RateLimitSettings` (en `Services/`): `SectionName = "RateLimiting"`, propiedades `LoginPermitLimit` (`Range(1, 1000)`) y `LoginWindowMinutes` (`Range(1, 1440)`). Se registra igual que `JwtSettings`: `AddOptions<RateLimitSettings>().BindConfiguration(...).ValidateDataAnnotations().ValidateOnStart()`.
- `appsettings.json`: `"RateLimiting": { "LoginPermitLimit": 5, "LoginWindowMinutes": 1 }`.
- `Program.cs`: `AddRateLimiter` con `RejectionStatusCode = 429` y la política `"login"`, `RateLimitPartition.GetFixedWindowLimiter` particionada por `RemoteIpAddress` (`?? "unknown"`), `QueueLimit = 0`.
- `OnRejected`: añade el header `Retry-After` (de `MetadataName.RetryAfter`) y escribe un `ProblemDetails` con `Title` y `Extensions["code"] = "too_many_requests"`, mismo formato que `ApiExceptionHandler`.
- Pipeline: `app.UseRateLimiter()` antes de `UseAuthentication`.
- Detrás de proxy o Docker hace falta `UseForwardedHeaders`; si no, todos los clientes comparten la IP del proxy y se bloquean entre sí.
- Debe existir un test que haga más intentos que el límite y compruebe el 429.

## Registro en Program.cs
Todo scoped, agrupado después de OpenAPI y antes de `AddDbContext`:

```csharp
builder.Services.AddScoped<IAuthUsers, AuthUsers>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

Orden del pipeline: `UseHttpsRedirection` -> `UseAuthentication` -> `UseAuthorization` -> `MapControllers`.
