using System.ComponentModel.DataAnnotations;

namespace Blog.ViewModels.Accounts
{
    public class UpdatePasswordViewModel
    {
        [Required(ErrorMessage = "A senha é obrigatória")]
        [StringLength(
            100,
            MinimumLength = 8,
           ErrorMessage = "A senha deve ter pelo menos 8 caracteres")]
        [RegularExpression(
           @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$",
           ErrorMessage = "A senha deve conter pelo menos uma letra maiúscula, uma letra minúscula, um número e um caractere especial")]
        public string OldPassword { get; set; }
        [Required(ErrorMessage = "A senha é obrigatória")]
        [StringLength(
            100,
            MinimumLength = 8, 
            ErrorMessage = "A senha deve ter pelo menos 8 caracteres")]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$",
            ErrorMessage = "A senha deve conter pelo menos uma letra maiúscula, uma letra minúscula, um número e um caractere especial")]
        public string NewPassword { get; set; }
        [Required(ErrorMessage = "A senha é obrigatória")]
        [StringLength(
            100,
            MinimumLength = 8,
            ErrorMessage = "A senha deve ter pelo menos 8 caracteres")]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$",
            ErrorMessage = "A senha deve conter pelo menos uma letra maiúscula, uma letra minúscula, um número e um caractere especial")]
        public string ConfirmNewPassword { get; set; }
    }
}
