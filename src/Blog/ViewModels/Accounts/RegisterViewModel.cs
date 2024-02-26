using System.ComponentModel.DataAnnotations;

namespace Blog.ViewModels.Accounts
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "O Nome é obrigatório")]
        public string Name { get; set; }
        [Required(ErrorMessage = "O Email é obrigatório")]
        [EmailAddress(ErrorMessage = "O E-mail é invalido")]// validate e-mail
        public string Email { get; set; }
        [Required(ErrorMessage = "A senha é obrigatória")]
        [StringLength(
            100,
            MinimumLength = 8,
            ErrorMessage = "A senha deve ter pelo menos 8 caracteres")]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$",
            ErrorMessage = "A senha deve conter pelo menos uma letra maiúscula, uma letra minúscula, um número e um caractere especial")]
        public string Password { get; set; }
        [Required(ErrorMessage = "A senha é obrigatória")]
        [StringLength(
           100,
           MinimumLength = 8,
           ErrorMessage = "A senha deve ter pelo menos 8 caracteres")]
        [RegularExpression(
           @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$",
           ErrorMessage = "A senha deve conter pelo menos uma letra maiúscula, uma letra minúscula, um número e um caractere especial")]
        public string ConfirmPassword { get; set; }
    }
}
