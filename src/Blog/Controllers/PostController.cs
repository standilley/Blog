using Blog.Data;
using Blog.Extensions;
using Blog.Models;
using Blog.ViewModels;
using Blog.ViewModels.Posts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;

namespace Blog.Controllers
{
    [ApiController]
    public class PostController : ControllerBase
    {
        [HttpGet("v1/posts")]
        public async Task<IActionResult> GetAsync(
            [FromServices] BlogDataContext context,
            [FromQuery] int page = 0,
            [FromQuery] int pageSize = 25)
        {
            try
            {
                var count = await context.Posts.AsNoTracking().CountAsync();
                var posts = await context
                    .Posts
                    .AsNoTracking()
                    .Include(x => x.Category)
                    .Include(x => x.Author)
                    .Select(x => new ListPostsViewModel
                    {
                        Id = x.Id,
                        Title = x.Title,
                        Slug = x.Slug,
                        LastUpdateDate = x.LastUpdateDate,
                        Category = x.Category.Name,
                        Author = $"{x.Author.Name} ({x.Author.Email})"
                    })
                    .Skip(page * pageSize)
                    .Take(pageSize)
                    .OrderByDescending(x => x.LastUpdateDate)
                    .ToListAsync();
                return Ok(new ResultViewModel<dynamic>(new
                {
                    total = count,
                    page,
                    pageSize,
                    posts
                }));
            }
            catch
            {
                return StatusCode(500, new ResultViewModel<List<Post>>("05X04 - Falha interna no Servidor"));
            }
        }
        [HttpGet("v1/posts/{id:int}")]
        public async Task<IActionResult> DetailsAsync(
            [FromServices]BlogDataContext context,
            [FromRoute] int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new ResultViewModel<Post>("ID inválido"));
                var post = await context
                    .Posts
                    .AsNoTracking()
                    .Include(x => x.Author)
                    .ThenInclude(x => x.Roles)
                    .Include(x => x.Category)
                    .FirstOrDefaultAsync(x => x.Id == id);
                if (post == null)
                    return NotFound(new ResultViewModel<Post>("Conteúdo não encontrado"));
                return Ok(new ResultViewModel<Post>(post));
            }
            catch(Exception ex)
            {
                return StatusCode(500, new ResultViewModel<List<Post>>("05X04 - Falha interna no Servidor"));
            }
        }
        [HttpGet("v1/posts/category/{category}")]
        public async Task<IActionResult> GetByCategoryAsync(
            [FromRoute] string category,
            [FromServices] BlogDataContext context,
            [FromQuery] int page = 0,
            [FromQuery] int pageSize = 25)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(category))
                    return BadRequest(new ResultViewModel<List<Post>>("Categoria inválida"));
                
                category = category.ToLower();

                var posts = await context.Posts
                    .AsNoTracking()
                    .Include(x => x.Author)
                    .Include(x => x.Category)
                    .Where(x => x.Category.Name.ToLower().Contains(category))
                    .Select(x => new ListPostsViewModel
                    {
                        Id = x.Id,
                        Title = x.Title,
                        Slug = x.Slug,
                        LastUpdateDate = x.LastUpdateDate,
                        Category = x.Category.Name,
                        Author = $"{x.Author.Name} ({x.Author.Email})"
                    })
                    .Skip(page * pageSize)
                    .Take(pageSize)
                    .OrderByDescending(x => x.LastUpdateDate)
                    .ToListAsync();

                if (posts == null || !posts.Any())
                    return NotFound(new ResultViewModel<List<Post>>("Nenhuma publicação foi encontrada para esta categoria"));

                var count = await context.Posts
                    .AsNoTracking()
                    .CountAsync(x => x.Category.Name.ToLower().Contains(category));

                return Ok(new ResultViewModel<dynamic>(new
                {
                    total = count,
                    page,
                    pageSize,
                    posts
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<List<Post>>("05X04 - Falha interna no Servidor"));
            }
        }

        [HttpPost("v1/posts")]
        public async Task<IActionResult> PostAsync(
        [FromBody] CreatePostViewModel model,
        [FromServices] BlogDataContext context)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ResultViewModel<Post>(ModelState.GetErrors()));
            try
            {
                if (string.IsNullOrWhiteSpace(model.Category))
                    return BadRequest(new ResultViewModel<Post>("Categoria inválida"));

                var category = await context.Categories
                    .FirstOrDefaultAsync(x => x.Name.ToLower() == model.Category.ToLower());

                if (category == null)
                    return NotFound(new ResultViewModel<Category>("Categoria não encontrada"));

                if (string.IsNullOrWhiteSpace(model.Author))
                    return BadRequest(new ResultViewModel<Post>("Autor inválido"));

                var author = await context.Users
                    .FirstOrDefaultAsync(x => x.Name.ToLower() == model.Author.ToLower());

                if (author == null)
                    return NotFound(new ResultViewModel<User>("Autor não encontrado"));

                var post = new Post
                {
                    Title = model.Title,
                    Summary = model.Summary,
                    Body = model.Body,
                    Slug = model.Title.ToLower()
                                  .Replace(oldValue: "@", newValue: "-")
                                  .Replace(oldValue: ".", newValue: "-")
                                  .Replace(oldValue: " ", newValue: "-"),
                    CreateDate = DateTime.Now.ToUniversalTime(),
                    Category = category, 
                    Author = author
                };
                await context.Posts.AddAsync(post);
                await context.SaveChangesAsync();
                return Created($"v1/posts/{post.Id}", new ResultViewModel<Post>(post));
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new ResultViewModel<Post>("05XE15 - Falha, publicação idêntica já cadastrada."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<Post>("05XE10 - Falha interna no servidor"));
            }
        }
        [HttpPut("v1/posts{id:int}")]
        public async Task<IActionResult> PostAsync(
            [FromRoute] int id,
            [FromBody] EditPostViewModel model,
            [FromServices] BlogDataContext context)
        {
            if (id <= 0)
                return BadRequest(new ResultViewModel<Post>("ID inválido"));
            if (!ModelState.IsValid)
                return BadRequest(new ResultViewModel<Post>(ModelState.GetErrors()));
            try
            {
                var post = await context.Posts
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (post == null)
                    return NotFound(new ResultViewModel<Post>("Publicação não encontrado"));

                post.Title = model.Title;
                post.Summary = model.Summary;
                post.Body = model.Body;
                post.Slug = model.Slug.ToLower();
                post.LastUpdateDate = DateTime.Now.ToUniversalTime();
  
                context.Posts.Update(post);
                await context.SaveChangesAsync();
                return Ok(new ResultViewModel<Post>(post));
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(500, new ResultViewModel<Post>("09XE3 - Concorrência de atualização detectada. Tente novamente."));
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new ResultViewModel<Post>("05XE19 - Erro ao atualizar o post no banco de dados."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<Post>("05XE30 - Falha interna no servidor"));
            }
        }
        [HttpDelete("v1/posts{id:int}")]
        public async Task<IActionResult> DeleteAsync(
           [FromRoute] int id,
           [FromServices] BlogDataContext context)
        {
            if (id <= 0)
                return BadRequest(new ResultViewModel<Post>("ID inválido"));
            try
            {
                var post = await context
                    .Posts
                    .FirstOrDefaultAsync(x => x.Id == id);
                if (post == null)
                    return NotFound(new ResultViewModel<Post>("Publicação não encontrado"));

                context.Posts.Remove(post);
                await context.SaveChangesAsync();
                return Ok(new ResultViewModel<Post>(post));
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new ResultViewModel<Post>("05XE18 - Erro ao remover publicação no banco de dados."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<Post>("05XE31 - Falha interna no servidor"));
            }
        }

    }
}
