using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.Service
{
    public class RateLimitSettings
    {
        public const string SectionName = "RateLimiting";

        [Range(1, 1000)]
        public int LoginPermitLimit { get; set; }

        [Range(1, 1440)]
        public int LoginWindowMinutes { get; set; }
    }
}
