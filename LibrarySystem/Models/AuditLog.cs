namespace LibrarySystem.Models
{
    // Solo se inserta, nunca se edita.
    public class AuditLog
    {
        public int Id { get; set; }
        public int ActorId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Entity { get; set; } = string.Empty;
        public string? Before { get; set; }
        public string? After { get; set; }
        public DateTime Date { get; set; }
    }
}
