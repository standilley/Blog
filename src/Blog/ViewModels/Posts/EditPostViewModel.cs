using System.ComponentModel.DataAnnotations;

namespace Blog.ViewModels.Posts
{
    public class EditPostViewModel
    {
        [Required(ErrorMessage = "O Título é obrigatório")]
        [StringLength(
            100,
            MinimumLength = 3,
            ErrorMessage = "O Título deve conter entre 3 e 100 caracteres")]
        public string Title { get; set; }
        [Required(ErrorMessage = "O Resumo é obrigatório")]
        [StringLength(
            200,
            MinimumLength = 10,
            ErrorMessage = "O Resumo deve conter entre 3 e 100 caracteres")]
        public string Summary { get; set; }
        [Required(ErrorMessage = "O corpo da publicação é obrigatório")]
        [StringLength(
                1000,
                MinimumLength = 10,
                ErrorMessage = "O corpo da publicação deve conter entre 10 e 1000 caracteres")]
        public string Body { get; set; }
        [Required(ErrorMessage = "O Slug é obrigatória")]
        [StringLength(
                40,
                MinimumLength = 3,
                ErrorMessage = "O Slug deve conter entre 3 e 40 caracteres")]
        public string Slug { get; set; }
        public DateTime LastUpdateDate { get; set; }
    }
}
