using System.ComponentModel.DataAnnotations;

namespace ps_mosquito_asp.Models
{
    public class UserModelReport
    {
        public string? id { get; set; }
        public string? name { get; set; }
        public string? lastname { get; set; }
        public string? taskName { get; set; }
        public string? ci { get; set; }
        public string? role { get; set; }
        public string? position { get; set; }
        [Required]
        public string? email { get; set; }
        [Required]
        public string? password { get; set; }
    }
}
