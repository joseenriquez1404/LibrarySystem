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
- OpenAPI con `Microsoft.AspNetCore.OpenApi`, expuesto en Development.
- Pendiente de elegir o agregar: base de datos y su proveedor, migraciones, JWT, hashing (bcrypt o argon2), framework de tests, Docker.

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

El código es un esqueleto inicial y todavía no cumple `TODO.txt`:

- `Models/Enums/Roles.cs` usa `User`, `Librarian`, `Admin`. Está bien mapeado al rol "lector", pero los nombres en español del TODO son solo de documentación; no los cambies sin preguntar.
- `Models/User.cs` guarda `Password` en texto plano. Debe guardar solo `PasswordHash`, y el campo `Name` no está en el TODO (`email`, `password_hash`, `rol`, `activo`).
- `Models/Loan.cs` no tiene `ReturnDate` ni la relación con ejemplar (`ejemplar_id`); hoy referencia `BookId`. Falta la entidad **Ejemplar** y la fecha de devolución.
- `Models/Book.cs` no tiene ISBN, año, categoría, ni relación muchos a muchos con autores (`Authors` es `List<string>`).
- Faltan entidades **Reserva**, **Multa** y **Auditoría**.
- `Controllers/` existe vacío.
- No hay repositorio Git, ni `.gitignore`, ni `.env.example`, ni proyecto de tests.
- `LibrarySystem.http` apunta a `/weatherforecast/`, que no existe; es plantilla sin reemplazar.

## Convenciones

- Idioma: nombres de dominio y mensajes de usuario en español, igual que `TODO.txt`. Nombres de código como clases y métodos también en español para mantener consistencia con el dominio.
- Nunca loguear contraseñas ni tokens.
- Configuración sensible por variables de entorno. No commitear secretos ni `appsettings` con credenciales reales.
- Todo listado tiene paginación y ordenamiento con tamaño máximo de página.
- Formato de error uniforme: código, mensaje y detalle.
- 401 cuando falta o expiró el token; 403 cuando el rol no alcanza. No mezclarlos.
- Commits pequeños.

## Comandos

```
dotnet build
dotnet run
```

Tests, Docker y CI todavía no existen; agrégalos en las fases 5 y 6 y actualiza esta sección cuando existan.
