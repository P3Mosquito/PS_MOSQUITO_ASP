using System.ComponentModel.DataAnnotations;

namespace ps_mosquito_asp.Models
{
    public class LoginModel
    {
        [Key]
        public int id { get; set; }
        public string? password { get; set; }
        public string? email { get; set; }

        public string? role { get; set; }

    }
}
