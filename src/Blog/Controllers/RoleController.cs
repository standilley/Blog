using Blog.Data;
using Blog.Extensions;
using Blog.Models;
using Blog.ViewModels.Tags;
using Blog.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Blog.ViewModels.Roles;

namespace Blog.Controllers
{
    [ApiController]
    public class RoleController : ControllerBase
    {

        [HttpGet("v1/roles")]
        public async Task<IActionResult> GetAsync(
            [FromServices] BlogDataContext context)
        {
            try
            {
                var roles = await context.Roles
                .AsNoTracking()
                .ToListAsync();

                if (roles == null || roles.Count == 0)
                {
                    return NotFound(new ResultViewModel<string>("Nenhum Perfil encontrado"));
                }
                return Ok(new ResultViewModel<IList<Role>>(roles));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<string>("01XE70 - Falha interna no servidor"));
            }
        }

        [HttpGet("v1/roles/{id:int}")]
        public async Task<IActionResult> GetAsync(
            [FromRoute] int id,
            [FromServices] BlogDataContext context)
        {
            if (id <= 0)
                return BadRequest(new ResultViewModel<User>("ID inválido"));
            try
            {
                var role = await context.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

                if (role == null)
                {
                    return NotFound(new ResultViewModel<string>("Nenhum Perfil encontrado"));
                }
                return Ok(new ResultViewModel<Role>(role));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<string>("01XE70 - Falha interna no servidor"));
            }
        }
        [HttpPost("v1/roles")]
        public async Task<IActionResult> PostAsync(
            [FromBody] EditorRoleViewModel model,
            [FromServices] BlogDataContext context)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ResultViewModel<Category>(ModelState.GetErrors()));
            var role = new Role
            {
                Id = 0,
                Name = model.Name,
                Slug = model.Slug.ToLower(),
            };
            try
            {
                await context.Roles.AddAsync(role);
                await context.SaveChangesAsync();
                return Created($"v1/tags/{role.Id}", new ResultViewModel<Role>(role));
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new ResultViewModel<Category>("05XE17 - Não foi possível incluir a categoria"));
            }
            catch
            {
                return StatusCode(500, new ResultViewModel<Category>("05XE91 - Falha interna no servidor"));
            }
        }
        [HttpPut("v1/roles/{id:int}")]
        public async Task<IActionResult> PutAsync(
            [FromRoute] int id,
            [FromBody] EditorRoleViewModel model,
            [FromServices] BlogDataContext context)
        {
            if (id <= 0)
                return BadRequest(new ResultViewModel<Post>("ID inválido"));
            if (!ModelState.IsValid)
                return BadRequest(new ResultViewModel<Category>(ModelState.GetErrors()));

            try
            {
                var role = await context.Roles
                    .FirstOrDefaultAsync(x => x.Id == id);
                if (role == null)
                    return NotFound(new ResultViewModel<Tag>("Conteúdo não encontrado"));
                role.Name = model.Name;
                role.Slug = model.Slug.ToLower();

                context.Roles.Update(role);
                await context.SaveChangesAsync();
                return Ok(new ResultViewModel<Role>(role));
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new ResultViewModel<Category>("05XE79 - Não foi possível incluir a categoria"));
            }
            catch
            {
                return StatusCode(500, new ResultViewModel<Category>("05XE41 - Falha interna no servidor"));
            }
        }
        [HttpDelete("v1/roles/{id:int}")]
        public async Task<IActionResult> DeleteAsync(
            [FromRoute] int id,
            [FromServices] BlogDataContext context)
        {
            if (id <= 0)
                return BadRequest(new ResultViewModel<User>("ID inválido"));
            try
            {
                var role = await context.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
                if (role == null)
                    return NotFound(new ResultViewModel<string>("Nenhum Perfil encontrado"));
                context.Roles.Remove(role);
                await context.SaveChangesAsync();
                return Ok(new ResultViewModel<Role>(role));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<string>("01XE80 - Falha interna no servidor"));
            }
        }
    }
}
