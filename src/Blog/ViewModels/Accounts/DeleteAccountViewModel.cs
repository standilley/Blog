using System.ComponentModel.DataAnnotations;

namespace Blog.ViewModels.Accounts
{
    public class DeleteAccountViewModel
    {
        [Required(ErrorMessage = "A senha é obrigatória")]
        [StringLength(
            100,
            MinimumLength = 8,
            ErrorMessage = "A senha deve ter pelo menos 8 caracteres")]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$",
            ErrorMessage = "A senha deve conter pelo menos uma letra maiúscula, uma letra minúscula, um número e um caractere especial")]
        public string Password { get; set; }
    }
}
