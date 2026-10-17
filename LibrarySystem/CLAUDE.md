# CLAUDE.md

API REST de biblioteca y préstamos en ASP.NET Core (.NET 10). Es un proyecto de aprendizaje con objetivo de portafolio.

## Fuente de verdad

- `TODO.txt` define el alcance, las reglas de negocio, las fases y la definición de terminado. Si algo de este archivo contradice `TODO.txt`, manda `TODO.txt`.
- Antes de implementar una fase, relee su sección en `TODO.txt`.
- No agregues extras (sección "Extras opcionales") hasta cumplir la definición de terminado.

## Alcance

- Dentro: API REST con JWT y 3 roles, catálogo, préstamos, devoluciones, renovaciones, multas, reservas, tests, Docker y documentación.
- Fuera: frontend (se prueba con Swagger, Postman o `.http`), pago real de multas, notificaciones push, códigos de barras físicos.

## Stack

- .NET 10, ASP.NET Core con controladores, `Nullable` y `ImplicitUsings` activados.
- OpenAPI con `Microsoft.AspNetCore.OpenApi` y Scalar como interfaz, expuestos en Development.
- SQL Server con Entity Framework Core 10 (`Microsoft.EntityFrameworkCore.SqlServer`, `.Design` y `.Tools`). `dotnet-ef` es herramienta local del repo (`dotnet-tools.json`).
- JWT con `Microsoft.AspNetCore.Authentication.JwtBearer`. Solo access token (60 minutos por defecto); el refresh token se aplazó.
- Hashing temporal con `PasswordHasher` de ASP.NET Core (en `AuthService` y en el seed).
- Pendiente de elegir o agregar: hashing definitivo (bcrypt o argon2), framework de tests, Docker.

## Arquitectura

Cuatro capas, con dependencias hacia adentro:

1. **Dominio**: entidades, enums, reglas y excepciones de dominio. No importa frameworks ni base de datos.
2. **Aplicación**: casos de uso y las interfaces de repositorio.
3. **Infraestructura**: implementaciones de repositorios, base de datos, JWT y hashing.
4. **Presentación**: controladores, DTOs y mapeo de errores a HTTP.

Reglas:
- Los repositorios se declaran como interfaces en dominio o aplicación y se implementan en infraestructura.
- Los casos de uso reciben sus dependencias por inyección.
- Cada cambio de estado de préstamos, reservas, multas o ejemplares ocurre dentro de una transacción.
- Los montos y plazos salen de configuración (`appsettings`), no de constantes en el código.
- Las excepciones de dominio (por ejemplo `LimitePrestamosExcedido`) se traducen a códigos HTTP en un solo lugar.

Orden de trabajo recomendado (de `TODO.txt`): empezar por entidades, reglas y casos de uso con repositorios en memoria y tests unitarios, y después conectar base de datos y framework.

## Roles

| Rol | Alcance |
| --- | --- |
| Lector (`User` en el código actual) | Consultar catálogo, reservar, ver y renovar solo sus préstamos y multas |
| Bibliotecario (`Librarian`) | Lo del lector sobre cualquier usuario, más catálogo, préstamos, devoluciones y cobro de multas |
| Admin (`Admin`) | Lo anterior, más usuarios y roles, perdonar multas y ver auditoría |

Solo el admin cambia roles, y nadie cambia su propio rol.

## Reglas de negocio

Cada regla necesita al menos un test que falle si se rompe.

1. Máximo 3 préstamos activos por lector.
2. Plazo de 14 días, con hasta 2 renovaciones de 7 días.
3. No se renueva si el préstamo está vencido o si otro lector tiene reserva activa del libro.
4. Multa de 5 pesos por día de retraso, con tope de 100 pesos por préstamo.
5. Un lector con multas pendientes o préstamos vencidos no puede pedir préstamos nuevos.
6. Solo se reserva un libro sin ejemplares disponibles; la cola es por orden de llegada.
7. Una reserva expira 3 días después de que el ejemplar queda disponible para ese lector.
8. Un ejemplar tiene como máximo un préstamo activo a la vez.
9. Un libro con ejemplares prestados o con historial no se borra; se da de baja lógicamente.

## Estado actual del código

Modelos de dominio y acceso a datos implementados (compilan sin warnings):

- `Models/Enums/`: `Roles` (`User`, `Librarian`, `Admin`), `LoanStatus` (`Active`, `Expired`, `Returned`), `CopyStatus` (`Available`, `Loaned`, `Lost`, `Damaged`), `ReservationStatus` (`Active`, `Fulfilled`, `Cancelled`, `Expired`), `FineStatus` (`Pending`, `Paid`, `Forgiven`).
- `Models/User.cs`: `Name`, `Email`, `PasswordHash`, `Active`, `Role`.
- `Models/Book.cs`: `Title`, `Description`, `ISBN`, `Year`, `CategoryId`/`Category`, `Authors` (`List<Author>`), `Active`.
- `Models/Author.cs` y `Models/Category.cs`: entidades con `Id` y `Name`.
- `Models/Copy.cs` (ejemplar): `BookId`, `Code`, `Status`, `Active`.
- `Models/Loan.cs`: `UserId`, `CopyId`, `LoanDate`, `DueDate`, `ReturnDate` (nullable), `Renewals`, `Status`.
- `Models/Reservation.cs`: `UserId`, `BookId`, `Date`, `Position`, `ExpirationDate` (nullable, para la regla 7), `Status`.
- `Models/Fine.cs`: `LoanId`, `Amount` (decimal), `Status`.
- `Models/AuditLog.cs`: `ActorId`, `Action`, `Entity`, `Before`, `After`, `Date`. Solo se inserta.
- `Data/AppDbContext.cs`: un `DbSet` por entidad, relaciones con `Restrict`, enums como texto, índices únicos en `User.Email`, `Book.ISBN` y `Copy.Code`.
- `Data/DbSeeder.cs`: usuarios por rol, categorías, autores, libros y ejemplares. Corre en Development y solo inserta si `Users` está vacía. Contraseñas de prueba: ver el archivo.
- `Controllers/HealthController.cs`: `GET /health`, 200 si hay conexión a la base y 503 si no.
- `Migrations/`: `InitialCreate`, `AddReservationFineAuditLog` y `UniqueCategoryName`, ya aplicadas en la base `Library`.
- `Service/`: `IAuthService`/`AuthService` (registro de lectores y login), `ITokenService`/`TokenService` (crea el JWT con `JsonWebTokenHandler`), `IUserService`/`UserService` (consultar, cambiar rol y activar o desactivar; solo admin, nadie cambia su propio rol, crear usuarios, cambiar el rol y activar o desactivar van a `AuditLogs`) y `JwtSettings`.
- `Dtos/`: `RegisterRequest` y `LoginRequest` con validaciones por atributos, `AuthResponse`, `UserResponse` (nunca expone el hash) y `PagedResult` (sin uso todavía).
- `Exceptions/`: excepciones de los servicios (`InvalidInputException`, `ConflictException`, `InvalidCredentialsException`, `NotFoundException`, `ForbiddenException`) y `ApiExceptionHandler`, el único lugar que las traduce a 400, 401, 403, 404 y 409 con un campo `code`. Los mensajes están en inglés.
- `Controllers/AuthController.cs`: `POST /api/auth/register` (201) y `POST /api/auth/login` (200).
- `Controllers/AuditController.cs` (`GET /api/audit`, solo admin): últimos registros de `AuditLogs`, filtros `entity` y `actorId`, `limit` de 1 a 500 (100 por defecto) mientras no haya paginación.
- `Controllers/CategoryController.cs` (`/api/categories`) y `Controllers/AuthorController.cs` (`/api/authors`): lectura para cualquier usuario autenticado; alta, edición y borrado para bibliotecario y admin. El nombre de categoría es único (409); borrar una categoría o un autor con libros devuelve 409. Listados sin paginación por ahora.
- `Controllers/UserController.cs` (`/api/users`, requiere token): `GET me` para cualquier usuario; `GET`, `GET {id}`, `POST`, `PUT {id}/role` y `PUT {id}/active` solo para admin. No hay `DELETE`; los usuarios se desactivan.
- `Program.cs` configura JwtBearer (en `OnTokenValidated` rechaza con 401 los tokens de usuarios desactivados, borrados o con el rol cambiado), valida `JwtSettings` al arrancar, aplica migraciones y corre el seed solo en Development.
- JWT: `Issuer`, `Audience` y `AccessTokenMinutes` están en `appsettings.json`; solo `Key` cambia por entorno (`appsettings.Development.json` o `Jwt__Key`). La app no arranca si falta la clave o mide menos de 32 caracteres.

Pendiente:

- Decidir si `Copy` se renombra a `Ejemplar` y `Book`/`Author`/`Category` a español. Hoy hay nombres mezclados.
- Relación `Book`–`Category`: se asume una categoría por libro.
- Separar las cuatro capas (hoy todo vive en `Models/`, `Data/`, `Service/` y `Controllers/`), repositorios y casos de uso. Los servicios usan `AppDbContext` directamente.
- Fase 2 incompleta: faltan los permisos por recurso y cambiar el hashing a bcrypt o argon2. El refresh token se aplazó.
- Paginación aplazada por decisión del dueño del proyecto: `UserService.GetAllAsync` devuelve todos los usuarios. `TODO.txt` la pide en todos los listados; se retoma después de la fase 2.
- Al leer el id del usuario en un controlador, usar `ClaimTypes.NameIdentifier` (JwtBearer renombra `sub`); el rol sí funciona con `[Authorize(Roles = "...")]`.
- No hay proyecto de tests.
- `LibrarySystem.http` apunta a `/weatherforecast/`, que no existe; es plantilla sin reemplazar.

Git: ramas `master` (principal) y `modelos-dominio`, con remoto `origin`. `.gitignore` y `.env.example` están en la raíz del repo.

## Convenciones

- Idioma: mensajes de usuario y términos de dominio en español, igual que `TODO.txt`. Los nombres de código de los modelos actuales están en inglés; mantén la convención que quede decidida en "Pendiente".
- Nunca loguear contraseñas ni tokens.
- Configuración sensible por variables de entorno. No commitear secretos ni `appsettings` con credenciales reales.
- La paginación y el ordenamiento de los listados se aplazan hasta después de la fase 2; por ahora los listados devuelven todo.
- Formato de error uniforme: código, mensaje y detalle.
- 401 cuando falta o expiró el token; 403 cuando el rol no alcanza. No mezclarlos.
- Commits pequeños.

## Comandos

```
dotnet build
dotnet run
dotnet tool restore
dotnet dotnet-ef migrations add <Nombre> --project LibrarySystem/LibrarySystem.csproj
dotnet dotnet-ef database update --project LibrarySystem/LibrarySystem.csproj
```

Los comandos `dotnet-ef` se corren desde la raíz del repo, donde está `dotnet-tools.json`. La cadena de conexión de desarrollo está en `appsettings.Development.json`; en otros entornos viene de `ConnectionStrings__DefaultConnection` (ver `.env.example`).

Tests, Docker y CI todavía no existen; agrégalos en las fases 5 y 6 y actualiza esta sección cuando existan.
