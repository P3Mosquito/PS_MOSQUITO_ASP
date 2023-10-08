using System.ComponentModel.DataAnnotations;

namespace ps_mosquito_asp.Models
{
    public class UserModel
    {
        [Key]
        public int id { get; set; }
        public string? name { get; set; }
        public string? lastname { get; set; }
        public string? ci { get; set; }
        public string? role { get; set; }
        public string? position { get; set; }
        public string? email { get; set; }
        public string? password { get; set; }
        
    }
}
