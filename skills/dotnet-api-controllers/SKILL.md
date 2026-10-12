---
name: dotnet-api-controllers
description: Cómo escribir controladores (ControllerBase, rutas, códigos de respuesta, [Authorize]) en las APIs .NET del usuario. Úsala al crear o modificar un controlador o endpoint.
---

# Controllers (`Controllers/`)

Un controlador por feature, nombre `<Feature>Controller`, namespace `<Proyecto>.Controllers`.

## Forma
- Atributos en este orden: `[Route("api/[controller]")]`, `[ApiController]`.
- Hereda de `ControllerBase` (no `Controller`).
- Primary constructor con servicios como parámetros sin guion bajo: `AuthController(IAuthUsers authUsersService, ITokenService tokenService)`.
- Acciones `async Task<IActionResult>` (sync `IActionResult` si no hay await). Nombre del método en PascalCase descriptivo (`RegisterUser`, `LoginUser`, `GetAllUsers`); ruta en minúsculas en el atributo: `[HttpPost("register")]`.
- El DTO llega como parámetro directo (sin `[FromBody]`); `[ApiController]` valida las data annotations y responde 400 solo.
- Sin lógica de negocio: delega al servicio.

## Mapeo de resultados
| Servicio devuelve | Respuesta |
|---|---|
| excepción de dominio | no se captura en el controlador; `ApiExceptionHandler` la traduce (400/401/403/404/409) |
| entidad | mapear a `<X>Response` manualmente y `Ok(response)` |
| lista de Responses | `Ok(users)` |

Sin `try/catch` ni `if (x == null)` para fallos de negocio: el servicio lanza y el handler responde. Registro nuevo: `StatusCode(StatusCodes.Status201Created, response)`.

El mapeo entidad -> Response se escribe inline con inicializador de objeto, enum con `.ToString()`:

```csharp
var response = new UserResponse
{
    Id = registeredUser.Id,
    Username = registeredUser.Username,
    Role = registeredUser.Role.ToString()
};
return Ok(response);
```

Respuestas compuestas con objeto anónimo: `Ok(new { user = response, token })`.

## Autorización
- `[Authorize]` encima de `[HttpGet(...)]` en endpoints protegidos; `[Authorize(Roles = "Admin")]` para roles.
- Claims del usuario actual: `User.FindFirstValue(ClaimTypes.NameIdentifier)`, `User.Identity?.Name`, `User.FindFirstValue(ClaimTypes.Role)` (depende del mapeo inbound por defecto: `sub` -> NameIdentifier, `unique_name` -> Name).

## Rate limiting
`[EnableRateLimiting("login")]` encima de `[HttpPost("login")]` (usings: `Microsoft.AspNetCore.RateLimiting`). Se puede aplicar la misma política o una más laxa a `register`. La política y el 429 se definen en `Program.cs` (ver `dotnet-api-services`); el controlador no maneja el rechazo.

## Ejemplo
```csharp
[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthUsers authUsersService, ITokenService tokenService) : ControllerBase
{
    [EnableRateLimiting("login")]
    [HttpPost("login")]
    public async Task<IActionResult> LoginUser(UserLoginRequest loginRequest)
    {
        var loggedInUser = await authUsersService.LoginUserAsync(loginRequest);
        var token = tokenService.CreateToken(loggedInUser);
        var response = new UserResponse
        {
            Id = loggedInUser.Id,
            Username = loggedInUser.Username,
            Role = loggedInUser.Role.ToString()
        };
        return Ok(new { user = response, token });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await authUsersService.GetAllUsersAsync();
        return Ok(users);
    }
}
```

Al agregar un endpoint, añade también su ejemplo en `<Proyecto>.http`.
