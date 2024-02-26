using Blog.Data;
using Blog.Extensions;
using Blog.Models;
using Blog.Services;
using Blog.ViewModels;
using Blog.ViewModels.Accounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Writers;
using SecureIdentity.Password;
using System.Text.RegularExpressions;

namespace Blog.Controllers
{
    [ApiController]
    public class AccountController : ControllerBase
    {
        [HttpPost("v1/accounts/")]
        public async Task<IActionResult> PostAsync(
            [FromBody] RegisterViewModel model,
            [FromServices] EmailService emailService,
            [FromServices] BlogDataContext context)
        {
            if(!ModelState.IsValid)      
               return BadRequest(new ResultViewModel<string>(ModelState.GetErrors()));           
            var user = new User
            {
                Name = model.Name,
                Email = model.Email,
                Slug = model.Email.ToLower()
                                  .Replace(oldValue: "@", newValue: "-")
                                  .Replace(oldValue: ".", newValue: "-")
            };
            if (model.Password != model.ConfirmPassword)
                return BadRequest(new ResultViewModel<string>("As senhas não coincidem"));
            if (!string.IsNullOrEmpty(model.Password))
            {
                user.PasswordHash = PasswordHasher.Hash(model.Password);
            }
            try
            {
                await context.Users.AddAsync(user);
                await context.SaveChangesAsync();

                emailService.Send(
                    user.Name,
                    user.Email,
                    "Bem vindo ao blog",
                    $"Mais do que um blog, uma comunidade! Aprenda, compartilhe e conecte-se com outros estudantes");

                return Ok(new ResultViewModel<dynamic>(new
                {   id = user.Id,
                    user = user.Email,
                    message = "Sua conta foi criada com sucesso."
                }));
            }
            catch (DbUpdateException)
            {
                return StatusCode(400, new ResultViewModel<string>("05X99 - Este E-mail já está cadastrado"));
            }
            catch
            {
                return StatusCode(500, new ResultViewModel<string>("05X04 - Falha interna no servidor"));
            }
        }
        [HttpPost("v1/accounts/login")]
        public async Task<IActionResult> LoginAsync(
            [FromBody] LoginViewModel model,
            [FromServices] BlogDataContext context,
            [FromServices] TokenServices tokenService)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ResultViewModel<string>(ModelState.GetErrors()));
            var user = await context
                .Users
                .AsNoTracking()
                .Include(x=> x.Roles)
                .FirstOrDefaultAsync(x => x.Email == model.Email);
            if (user == null)
                return StatusCode(401, new ResultViewModel<string>("Usuário ou senha inválida"));
            if (!PasswordHasher.Verify(user.PasswordHash, model.Password))
                return StatusCode(401, new ResultViewModel<string>("Usuário ou senha inválida"));
            try
            {
                var token = tokenService.GenerateToken(user);
                return Ok(new ResultViewModel<string>(token, null ));
            }
            catch
            {
                return StatusCode(500, new ResultViewModel<string>("05X04 - Falha interna no servidor"));
            }
        }
        [HttpPost("v1/accounts/bio/{id:int}")]
        public async Task<IActionResult> PostBioAsync(
            [FromRoute] int id,
            [FromBody] BioRegisterViewModel model,
            [FromServices] BlogDataContext context)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ResultViewModel<string>(ModelState.GetErrors()));
            if (id <= 0)
                return BadRequest(new ResultViewModel<User>("ID inválido"));
            try
            {
                var user = await context
                    .Users
                    .FirstOrDefaultAsync(x => x.Id == id);
                if (user == null)
                    return BadRequest(new ResultViewModel<User>("Usuário não encontrado"));
                user.Bio = model.Bio;
                await context.SaveChangesAsync();
                return Ok(new ResultViewModel<User>(user));
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new ResultViewModel<Post>("05XE65 - Não foi possível incluir a biografia"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<Post>("05XE40 - Falha interna no servidor"));
            }
        }
        [Authorize]
        [HttpPost("v1/accounts/upload-image")]
        public async Task<IActionResult> UploadImageAsync(
            [FromBody] UploadImageViewModel model,
            [FromServices] BlogDataContext context)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ResultViewModel<string>(ModelState.GetErrors()));
            var fileName = $"{Guid.NewGuid().ToString()}.jpg";
            var data = new Regex(@"^data:image/[a-z]+;base64,")
                .Replace(model.Base64Image, "");
            var bytes = Convert.FromBase64String(data);
            try
            {
                await System.IO.File.WriteAllBytesAsync($"wwwroot/images/{fileName}", bytes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<string>("05X04 - Falha interna no Servidor"));
            }
            var user = await context
                .Users
                .FirstOrDefaultAsync(x => x.Email == User.Identity.Name);
            if (user == null)
                return NotFound(new ResultViewModel<User>("Usuário não encontrado"));
            user.Image = $"https://localhost:0000/images/{fileName}";
            try
            {
                context.Users.Update(user);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<string>("05X00 - Falha interna no Servidor"));
            }
            return Ok(new ResultViewModel<string>("Imagem alterada com sucesso!", null));
        }
        [HttpGet("v1/accounts")]
        public async Task<IActionResult> GetAsync(
            [FromServices] BlogDataContext context)
            //[FromQuery] int page = 1
            //[FromQuery] int pageSize = 0)
        {
            try
            { 
                var counts = await context.Users.AsNoTracking().CountAsync();
                var users = await context
                    .Users
                    .AsNoTracking()
                    .Select(x => new ListUsersViewModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Email = x.Email,
                        Slug = x.Slug,
                        Bio = x.Bio,
                    })
                    //.Skip(page* pageSize)
                    //.Take(page)
                    .OrderByDescending(x=> x.Id)
                    .ToListAsync();
                var result = new
                {
                    total = counts,
                    //page,
                    //pageSize,
                    users
                };
                return Ok(new ResultViewModel<object>(result));
            }
            catch
            {
                return StatusCode(500, new ResultViewModel<List<Post>>("05X04 - Falha interna no Servidor"));
            }
        }
        [HttpGet("v1/accounts/{id:int}")]
        public async Task<IActionResult> GetByIdAsync(
            [FromRoute] int id,
            [FromServices] BlogDataContext context)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new ResultViewModel<User>("ID inválido"));

                var user = await context.Users
                    .AsNoTracking()
                    .OrderByDescending(x => x.Id)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (user == null)
                    return NotFound(new ResultViewModel<User>("Usuário não encontrado"));

                return Ok(new ResultViewModel<User>(user));
            }
            catch
            {
                return StatusCode(500, new ResultViewModel<List<Post>>("05X04 - Falha interna no Servidor"));
            }
        }
        [HttpGet("v1/accounts/{identifier}")]
        public async Task<IActionResult> GetByUserAsync(
            [FromRoute] string identifier,
            [FromServices] BlogDataContext context)
        {
            int userId;
            if (int.TryParse(identifier, out userId))
            {
                // Se o identificador puder ser convertido para int, então é um ID de usuário
                if (userId <= 0)
                    return BadRequest(new ResultViewModel<User>("ID inválido"));
                try
                {
                    var userById = await context.Users
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == userId);
                    if (userById == null)
                        return NotFound(new ResultViewModel<User>("Usuário não encontrado"));

                    return Ok(new ResultViewModel<User>(userById));
                }
                catch
                {
                    return StatusCode(500, new ResultViewModel<List<Post>>("05X04 - Falha interna no Servidor"));
                }
            }
            else
            {
                // Se o identificador não for um número inteiro, então é um nome de usuário
                if (string.IsNullOrWhiteSpace(identifier))
                    return BadRequest(new ResultViewModel<User>("Nome de usuário inválido"));
                try
                {
                    var userByName = await context.Users
                        .AsNoTracking()
                        .Where(x => x.Name.Contains(identifier.ToLower()))
                        .ToListAsync();
                        
                    if (userByName == null || !userByName.Any())
                        return NotFound(new ResultViewModel<User>("Usuário não encontrado"));

                    return Ok(new ResultViewModel<List<User>>(userByName));
                }
                catch
                {
                    return StatusCode(500, new ResultViewModel<List<Post>>("05X44 - Falha interna no Servidor"));
                }
            }
        }
        [HttpGet("v1/accounts/posts/{id:int}")]
        public async Task<IActionResult> GetPostsByIdAsync(
            [FromRoute] int id,
            [FromServices] BlogDataContext context)
        {
            if (id <= 0)
                return BadRequest(new ResultViewModel<string>("ID inválido"));
            try
            {
                var user = await context
                    .Users
                    .AsNoTracking()
                    .Include(x => x.Posts)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (user == null)
                    return NotFound(new ResultViewModel<string>("Usuário não encontrado"));

                return Ok(new ResultViewModel<IList<Post>>(user.Posts));
            }
            catch
            {
                return StatusCode(500, new ResultViewModel<string>("05X04 - Falha interna no Servidor"));
            }
        }

        [HttpPut("v1/accounts/{id:int}")]
        public async Task<IActionResult> PutAsync(
            [FromRoute] int id,
            [FromBody] EditAccountViewModel model,
            [FromServices] EmailService emailService,
            [FromServices] BlogDataContext context)
        {
            if (id <= 0)
                return BadRequest(new ResultViewModel<User>("ID inválido"));

            if (!ModelState.IsValid)
                return BadRequest(new ResultViewModel<string>(ModelState.GetErrors()));
            var user = await context.Users.FirstOrDefaultAsync(x => x.Id == id);
            if (user == null)
                return NotFound(new ResultViewModel<User>("Usuário não encontrado"));
            user.Name = model.Name;
            user.Email = model.Email.ToLower();
            user.Slug = model.Email.ToLower()
                .Replace(oldValue: "@", newValue: "-")                              
                .Replace(oldValue: ".", newValue: "-");
            try
            {
                context.Users.Update(user);
                await context.SaveChangesAsync();

                emailService.Send(
                    user.Name,
                    user.Email,
                    "Dados atualizados",
                    $"{user.Name}, Seus dados foram atualizados com sucesso.");

                return Ok(new ResultViewModel<User>(user));
            }
            catch (DbUpdateException)
            {
                return StatusCode(400, new ResultViewModel<string>("05X99 - Falha ao tentar atuallizar seu cadastrado"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<string>("05X04 - Falha interna no servidor"));
            }
        }
        [HttpPut("v1/accounts/password/{id:int}")]
        public async Task<IActionResult> PutPasswordAsync(
            [FromRoute] int id,
            [FromBody] UpdatePasswordViewModel model,
            [FromServices] EmailService emailService,
            [FromServices] BlogDataContext context)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ResultViewModel<string>(ModelState.GetErrors()));
            if (id <= 0)
                return BadRequest(new ResultViewModel<User>("ID inválido"));
            var user = await context.Users.FirstOrDefaultAsync(x => x.Id == id);
            
            if (user == null)
                return NotFound(new ResultViewModel<User>("Usuário não encontrado"));

            if (!PasswordHasher.Verify(user.PasswordHash, model.OldPassword))
                return StatusCode(401, new ResultViewModel<string>("Senha incorreta"));

            if (model.NewPassword != model.ConfirmNewPassword)
                return BadRequest(new ResultViewModel<string>("As novas senhas não coincidem"));

            user.PasswordHash = PasswordHasher.Hash(model.NewPassword);
            try
            {
                context.Users.Update(user);
                await context.SaveChangesAsync();

                emailService.Send(
                    user.Name,
                    user.Email,
                    "Senha Atualizada",
                    "Sua senha foi atualizada com sucesso");
                return Ok(new ResultViewModel<string>("Senha atualizada com sucesso"));
            }
            catch (DbUpdateException)
            {
                return StatusCode(400, new ResultViewModel<string>("Não foi possível atualizar sua senha"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<string>("Falha interna no servidor"));
            }
        }
        [HttpDelete("v1/accounts/{id:int}")]
        public async Task<IActionResult> DeleteAsync(
            [FromRoute] int id,
            [FromBody] DeleteAccountViewModel model,
            [FromServices] EmailService emailService,
            [FromServices] BlogDataContext context)
        {

            if (!ModelState.IsValid)
                return BadRequest(new ResultViewModel<string>(ModelState.GetErrors()));
            if (id <= 0)
                return BadRequest(new ResultViewModel<User>("ID inválido"));
            var user = await context
                .Users
                .FirstOrDefaultAsync(x => x.Id == id);
            if (user == null)
                return StatusCode(401, new ResultViewModel<string>("Usuário não encontrado"));
            if (!PasswordHasher.Verify(user.PasswordHash, model.Password))
                return StatusCode(401, new ResultViewModel<string>("Senha inválida"));
            try
            {
                context.Users.Remove(user);
                await context.SaveChangesAsync();

                emailService.Send(
                    user.Name,
                    user.Email,
                    "Conta Deletetada",
                    "Sua conta foi excluída com sucesso");

                return Ok(new ResultViewModel<dynamic>(new
                {
                    user.Name,
                    user.Email,
                    message = "Sua conta foi removida com sucesso"
                }));
            }
            catch (DbUpdateException)
            {
                return StatusCode(400, new ResultViewModel<string>("07XE7 - Não foi possível excluir sua conta"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<string>("Falha interna no servidor"));
            }
        }
    }
}
