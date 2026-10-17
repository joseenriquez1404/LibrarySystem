using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Service
{
    public static class DbUpdateExceptionExtensions
    {
        // 2601 y 2627: violación de índice o restricción única en SQL Server.
        public static bool IsUniqueViolation(this DbUpdateException exception) =>
            exception.InnerException is SqlException { Number: 2601 or 2627 };

        // 547: violación de llave foránea (por ejemplo, borrar algo que otra tabla referencia).
        public static bool IsForeignKeyViolation(this DbUpdateException exception) =>
            exception.InnerException is SqlException { Number: 547 };
    }
}
