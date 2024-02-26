using System.ComponentModel.DataAnnotations;

namespace Blog.ViewModels.Roles
{
    public class EditorRoleViewModel
    {
        [Required(ErrorMessage = "O Nome é obrigatório")]
        [StringLength(
            40,
            MinimumLength = 2,
            ErrorMessage = "O Nome deve conter entre 2 e 40 caracteres")]
        public string Name { get; set; }
        [Required(ErrorMessage = "O Slug é obrigatória")]
        [StringLength(
            40,
            MinimumLength = 2,
            ErrorMessage = "O Slug deve conter entre 2 e 40 caracteres")]
        public string Slug { get; set; }
    }
}
