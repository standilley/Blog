using System.ComponentModel.DataAnnotations;

namespace Blog.ViewModels.Accounts
{
    public class BioRegisterViewModel
    {
        [Required(ErrorMessage = "A biografia é obrigatório")]
        [StringLength(
                1000,
                MinimumLength = 3,
                ErrorMessage = "A biografia deve conter entre 3 e 1000 caracteres")]
        public string Bio { get; set; }
    }
}
