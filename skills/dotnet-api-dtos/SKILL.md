---
name: dotnet-api-dtos
description: Cómo escribir DTOs (Request/Response) en las APIs .NET del usuario. Úsala al crear o modificar cualquier DTO, validación de entrada o forma de respuesta de un endpoint.
---

# DTOs (`Dtos/`)

Clases simples (no records), una por archivo, namespace `<Proyecto>.Dtos`, propiedades `{ get; set; }`, strings con `= string.Empty`.

## Nombres
- Entrada: `<Entidad>Request` (crear/registrar) y `<Entidad><Accion>Request` cuando la validación difiere (`UserLoginRequest`).
- Salida: `<Entidad>Response`.
- Nunca exponer la entidad directamente ni campos sensibles (hash de contraseña).

## Request: validación con data annotations
Anotaciones en la misma línea separadas por coma, encima de cada propiedad. Sin línea en blanco entre propiedades.

- **Registro/creación**: reglas estrictas (`MinLength` y `MaxLength` acordes al esquema de BD).
- **Login/consulta**: solo `Required` + `MaxLength` generoso, para no revelar reglas de formato ni rechazar credenciales válidas antiguas.

```csharp
using System.ComponentModel.DataAnnotations;

namespace LoginRegisterAPI.Dtos
{
    public class UserRequest
    {
        [Required, MinLength(3), MaxLength(50)]
        public string Username { get; set; } = string.Empty;
        [Required, MinLength(8), MaxLength(100)]
        public string Password { get; set; } = string.Empty;
    }
}
```

```csharp
public class UserLoginRequest
{
    [Required, MaxLength(100)]
    public string Username { get; set; } = string.Empty;
    [Required, MaxLength(200)]
    public string Password { get; set; } = string.Empty;
}
```

`MaxLength` del registro debe coincidir con `HasMaxLength` del `AppDbContext`.

## Response
Solo datos seguros de exponer. Enums como `string` (se llena con `.ToString()`), sin anotaciones, sin `using`.

```csharp
namespace LoginRegisterAPI.Dtos
{
    public class UserResponse
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
```

## Respuestas compuestas
Para combinar datos (ej. login) se usa un objeto anónimo en el controlador, no un DTO extra: `Ok(new { user = response, token })`.
