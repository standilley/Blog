using System.ComponentModel.DataAnnotations;

namespace Blog.ViewModels.Accounts
{
    public class EditAccountViewModel
    {
        [Required(ErrorMessage = "O Nome é obrigatório")]
        public string Name { get; set; }
        [Required(ErrorMessage = "O Email é obrigatório")]
        [EmailAddress(ErrorMessage = "O E-mail é invalido")]// validate e-mail
        public string Email { get; set; }
    }
}
