using System.ComponentModel.DataAnnotations;

namespace ps_mosquito_asp.Models
{
    public class UserModel
    {
        public string? id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(30, ErrorMessage = "El nombre no puede tener más de 30 caracteres.")]
        [RegularExpression(@"^[a-zA-Z\s]*$", ErrorMessage = "El nombre solo puede contener letras.")]
        public string? name { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(40, ErrorMessage = "El apellido no puede tener más de 40 caracteres.")]
        [RegularExpression(@"^[a-zA-Z\s]*$", ErrorMessage = "El nombre solo puede contener letras.")]
        public string? lastname { get; set; }

        [Required(ErrorMessage = "El número de CI es obligatorio.")]
        [RegularExpression(@"^\d{8}[A-Za-z]{0,2}$", ErrorMessage = "El campo debe tener 8 dígitos y puede tener hasta 2 letras adicionales.")]
        public string? ci { get; set; }

        [Required(ErrorMessage = "El rol es obligatorio.")]
        public string? role { get; set; }

        [Required(ErrorMessage = "El cargo es obligatorio.")]
        [StringLength(30, ErrorMessage = "El cargo no puede tener más de 30 caracteres.")]
        [RegularExpression(@"^[a-zA-Z\s]*$", ErrorMessage = "El cargo solo puede contener letras.")]
        public string? position { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
        public string? email { get; set; }

        //[Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 50 caracteres.")]
        //[CustomPasswordValidation(ErrorMessage = "La contraseña no cumple con los requisitos específicos.")]
        public string? password { get; set; }

        // Aquí puedes agregar más propiedades y validaciones según sea necesario
    }
}
