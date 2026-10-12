---
name: dotnet-api-models
description: Cómo escribir entidades (Models), enums y AppDbContext con EF Core en las APIs .NET del usuario. Úsala al crear o modificar una entidad, enum, configuración de EF Core, índice o migración.
---

# Models y AppDbContext

## Entidad (`Models/<Entidad>.cs`)

POCO simple, propiedades `{ get; set; }`, strings con `= string.Empty`, enums con default explícito. `Id` es `int`. Sin data annotations: la configuración va en `OnModelCreating`.

```csharp
namespace LoginRegisterAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public Roles Role { get; set; } = Roles.User;
    }
}
```

La contraseña guardada es siempre el hash (`IPasswordHasher<T>` de Identity, sin el stack completo de Identity).

## Enum (`Models/Roles.cs`)

Un archivo por enum, nombre en plural, miembros en PascalCase, uno por línea.

```csharp
namespace LoginRegisterAPI.Models
{
    public enum Roles
    {
        User,
        Editor,
        Admin
    }
}
```

## AppDbContext (`Data/AppDbContext.cs`)

- Primary constructor `(DbContextOptions<AppDbContext> options) : DbContext(options)`.
- `DbSet` como expression-bodied: `public DbSet<User> Users => Set<User>();`
- Configuración fluida en `OnModelCreating`, llamando primero a `base.OnModelCreating(modelBuilder)` y guardando `var user = modelBuilder.Entity<User>();`.
- Enums se guardan como string: `.HasConversion<string>().HasDefaultValue(Roles.User)`.
- Longitudes con `.HasMaxLength(n)`; unicidad con `.HasIndex(...).IsUnique()`.

```csharp
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        var user = modelBuilder.Entity<User>();

        user.Property(u => u.Role)
            .HasConversion<string>()
            .HasDefaultValue(Roles.User);

        user.Property(u => u.Username)
            .HasMaxLength(50);

        user.HasIndex(u => u.Username)
            .IsUnique();
    }
}
```

## Migraciones

Un cambio de esquema = una migración con nombre descriptivo en PascalCase (`AddUserRole`, `AddUniqueUsers`):
`dotnet ef migrations add <Nombre> --project <Proyecto>` y `dotnet ef database update --project <Proyecto>`.
Conexión: `DefaultConnection` en `appsettings.json`, registrada con `UseSqlServer`.
