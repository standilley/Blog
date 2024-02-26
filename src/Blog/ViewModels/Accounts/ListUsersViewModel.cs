using Blog.Models;
using System.Text.Json.Serialization;

namespace Blog.ViewModels.Accounts
{
    public class ListUsersViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Slug { get; set; }
        public string Bio { get; set; }
    }
}
