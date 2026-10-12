---
name: dotnet-api-structure
description: Estructura y convenciones base de proyectos ASP.NET Core Web API (controllers + EF Core + JWT) del usuario. Úsala al crear un proyecto/API .NET nuevo, agregar una feature o entidad nueva, o cuando el código de un proyecto .NET no siga el estilo habitual. Complementa dotnet-api-models, dotnet-api-dtos, dotnet-api-services y dotnet-api-controllers.
---

# Estructura de una API .NET (estilo del usuario)

Un solo proyecto, carpetas por capa (no por feature). Namespace raíz = nombre del proyecto.

```
<Proyecto>/
  Controllers/   <Feature>Controller.cs
  Data/          AppDbContext.cs
  Dtos/          <Entidad>Request.cs, <Entidad>Response.cs, ...
  Models/        <Entidad>.cs, enums (Roles.cs)
  Services/      I<Servicio>.cs + <Servicio>.cs
  Migrations/
  Program.cs
```

Flujo: `Controller -> IServicio (Servicio) -> AppDbContext`. El controlador mapea entidad -> Response; el servicio devuelve entidades (o Responses en listados con `Select`).

Reglas globales:
- Namespaces con bloque `namespace X { ... }` (NO file-scoped), indentación de 4 espacios.
- Namespaces por carpeta: `<Proyecto>.Controllers`, `.Data`, `.Dtos`, `.Models`, `.Services`.
- Primary constructors para DI en controladores, servicios y DbContext.
- Nullable habilitado: `string.Empty` como default en strings; `?` solo en retornos que significan fallo.
- Sin comentarios en el código salvo que la razón no sea obvia.
- Mensajes de error de la API en inglés ("Username already exists.").
- Mensajes de commit en inglés, imperativo ("Add protected /me endpoint").
- Secretos (`Jwt:Key`) en user secrets, nunca en `appsettings.json` commiteado.
- Documentación OpenAPI nativa (`AddOpenApi` + `/openapi/v1.json`) con Scalar (`/scalar`) en Development; esquema `Bearer` registrado con un document transformer y preseleccionado en Scalar.
- Mantener un archivo `<Proyecto>.http` con ejemplos reales de cada endpoint, y un README/CLAUDE.md con arquitectura y comandos.

Para cada pieza concreta usa la skill correspondiente: `dotnet-api-models`, `dotnet-api-dtos`, `dotnet-api-services`, `dotnet-api-controllers`.
