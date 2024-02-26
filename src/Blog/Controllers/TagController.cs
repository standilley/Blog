using Blog.Data;
using Blog.Extensions;
using Blog.Models;
using Blog.ViewModels;
using Blog.ViewModels.Tags;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Blog.Controllers
{
    [ApiController]
    public class TagController : ControllerBase
    {
        [HttpGet("v1/tags")]
        public async Task<IActionResult> GetAsync(
            [FromServices] BlogDataContext context)
        {
            try
            {
                var tags = await context.Tags
                .AsNoTracking()
                .ToListAsync();

                if (tags == null || tags.Count == 0)
                {
                    return NotFound(new ResultViewModel<string>("Nenhuma tag encontrada"));
                }
                return Ok(new ResultViewModel<IList<Tag>>(tags));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<string>("01XE70 - Falha interna no servidor"));
            }
        }

        [HttpGet("v1/tags/{id:int}")]
        public async Task<IActionResult> GetAsync(
            [FromRoute] int id,
            [FromServices] BlogDataContext context)
        {
            if (id <= 0)
                return BadRequest(new ResultViewModel<User>("ID inválido"));
            try
            {
                var tag = await context.Tags
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

                if (tag == null)
                {
                    return NotFound(new ResultViewModel<string>("Nenhuma tag encontrada"));
                }
                return Ok(new ResultViewModel<Tag>(tag));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<string>("01XE70 - Falha interna no servidor"));
            }
        }
        [HttpPost("v1/tags")]
        public async Task<IActionResult> PostAsync(
            [FromBody] EditorTagViewModel model,
            [FromServices] BlogDataContext context)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ResultViewModel<Category>(ModelState.GetErrors()));
            var tag = new Tag
            {
                Id = 0,
                Name = model.Name,
                Slug = model.Slug.ToLower(),
            };
            try
            {
                await context.Tags.AddAsync(tag);
                await context.SaveChangesAsync();
                return Created($"v1/tags/{tag.Id}", new ResultViewModel<Tag>(tag));
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new ResultViewModel<Category>("05XE19 - Não foi possível incluir a categoria"));
            }
            catch
            {
                return StatusCode(500, new ResultViewModel<Category>("05XE11 - Falha interna no servidor"));
            }
        }
        [HttpPut("v1/tags/{id:int}")]
        public async Task<IActionResult> PutAsync(
            [FromRoute] int id,
            [FromBody] EditorTagViewModel model,
            [FromServices] BlogDataContext context)
        {
            if (id <= 0)
                return BadRequest(new ResultViewModel<Post>("ID inválido"));
            if (!ModelState.IsValid)
                return BadRequest(new ResultViewModel<Category>(ModelState.GetErrors()));
            
            try
            {
                var tag = await context.Tags
                    .FirstOrDefaultAsync(x => x.Id == id);
                if (tag == null)
                    return NotFound(new ResultViewModel<Tag>("Conteúdo não encontrado"));
                tag.Name = model.Name;
                tag.Slug = model.Slug.ToLower();

                context.Tags.Update(tag);
                await context.SaveChangesAsync();
                return Ok( new ResultViewModel<Tag>(tag));
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new ResultViewModel<Category>("05XE29 - Não foi possível incluir a categoria"));
            }
            catch
            {
                return StatusCode(500, new ResultViewModel<Category>("05XE21 - Falha interna no servidor"));
            }
        }
        [HttpDelete("v1/tags/{id:int}")]
        public async Task<IActionResult> DeleteAsync(
            [FromRoute] int id,
            [FromServices] BlogDataContext context)
        {
            if (id <= 0)
                return BadRequest(new ResultViewModel<User>("ID inválido"));
            try
            {
                var tag = await context.Tags
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
                if (tag == null)
                    return NotFound(new ResultViewModel<string>("Nenhuma tag encontrada"));
                context.Tags.Remove(tag);
                await context.SaveChangesAsync();
                return Ok(new ResultViewModel<Tag>(tag));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<string>("01XE70 - Falha interna no servidor"));
            }
        }
    }
}
