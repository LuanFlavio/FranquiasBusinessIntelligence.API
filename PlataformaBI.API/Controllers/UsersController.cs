
using DataAccess;
using Domain.Data;
using FranquiasBusinessIntelligence.API.Services.Session;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace PlataformaBI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowOrigin")]
    public class UsersController : FranquiasBIController
    {
        private readonly FranquiasBIDbContext _context;
        private readonly ConcurrentDictionary<string, Session> sessions;

        public UsersController(FranquiasBIDbContext context, ConcurrentDictionary<string, Session> sessions)
            : base(sessions)
        {
            _context = context;
            this.sessions = sessions;
        }

        /// <summary>
        /// Retorna os dados do usuários autenticado
        /// </summary>
        [HttpGet]
        public IActionResult Get()
        {
            if (!UserAuthenticated)
                return Unauthorized();

            this.Session.UpdateLastRequest();

            //Usuarios usuario = this.Session.usuarioLogado;
            IUser usuario = _context.User.FirstOrDefault(user => user.ID == Session.usuarioLogado.ID);

            usuario.Senha = "";

            if (usuario == null)
            {
                return NoContent();
            }

            return Ok(usuario);
        }

        /// <summary>
        /// Retorna os dados de todos usuários (disponível apenas para admin)
        /// </summary>
        [HttpGet("Todos")]
        public IActionResult GetAll()
        {
            if (!UserAuthenticated)
                return Unauthorized();

            if (Session.usuarioLogado.Perfil != "Administrador")
                return Unauthorized();

            var usuarios = _context.User
                .Select(x => new IUserDTO
                {
                    Id = x.ID,
                    Name = x.Nome,
                    Email = x.Email,
                    Profile = x.Perfil,
                    Company = _context.Company.Where(y => y.CNPJ == x.Empresa).FirstOrDefault()
                })
                .ToArray();

            if (usuarios == null)
                 return NoContent();

            return Ok(usuarios);
        }

        /// <summary>
        /// Cadastra um usuário
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] IUser user)
        {
            if (!UserAuthenticated)
                return Unauthorized();

            user.ID = 0;

            this.Session.UpdateLastRequest();

            if (!ValidaUsuario(user))
                return BadRequest("Faltou informação");

            IUser usuario = InsertAsync(user).Result;

            if (!ValidaUsuario(usuario))
            {
                return BadRequest("Usuário já cadastrado");
            }

            return Ok(usuario);
        }

        /// <summary>
        /// Altera o usuário cujo ID for passado (não pode alterar o email)
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] IUser value)
        {
            if (!UserAuthenticated)
                return Unauthorized();

            this.Session.UpdateLastRequest();

            if (id != value.ID)
                return BadRequest("Id passado na URL não compatível com o id passado no objeto");

            if (!ValidaUsuario(value))
                return BadRequest("Faltou informação");

            var usuario = await UpdateAsync(value);

            if (!ValidaUsuario(usuario))
                return BadRequest("Usuário não encontrado");

            return Ok(usuario);
        }

        /// <summary>
        /// Exclui o usuário cujo ID for passado
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!UserAuthenticated)
                return Unauthorized();

            this.Session.UpdateLastRequest();

            var delete = await DeleteAsync(id);

            if (!delete)
                return BadRequest("Usuário não encotrado");

            return Ok("Sucesso");
        }

        [NonAction]
        private bool ValidaUsuario(IUser user)
        {
            this.Session.UpdateLastRequest();

            if (
                user == null ||
                user.Nome == null ||
                user.Email == null ||
                user.Senha == null ||
                user.Perfil == null
            //user.Empresa == 0
            )
            {
                return false;
            }
            return true;
        }

        [NonAction]
        public async Task<bool> ExistsAsync(IUser value)
        {
            return await _context.User.AnyAsync(x => x.ID == value.ID && x.Email.ToLower() == value.Email.ToLower());
        }

        [NonAction]
        public async Task<bool> ExistsAsync(string email)
        {
            return await _context.User.AnyAsync(x => x.Email.ToLower() == email.ToLower());
        }

        [NonAction]
        public async Task<IUser> InsertAsync(IUser value)
        {
            if (await ExistsAsync(value.Email))
                return new IUser();

            var entityEntry = await _context.User.AddAsync(value);

            await _context.SaveChangesAsync();

            _context.ChangeTracker.Clear();

            return entityEntry.Entity;
        }

        [NonAction]
        public async Task<IUser> UpdateAsync(IUser value)
        {
            if (!await ExistsAsync(value))
                return new IUser();

            var entityEntry = _context.Update(value);

            _context.Entry(value).Property(x => x.Senha).IsModified = false;

            await _context.SaveChangesAsync();

            _context.ChangeTracker.Clear();

            return entityEntry.Entity;
        }

        [NonAction]
        public async Task<bool> DeleteAsync(int value)
        {
            IUser usuario = _context.User.FirstOrDefault(x => x.ID == value);

            if (usuario is null)
                return false;

            _context.User.Remove(usuario);

            await this._context.SaveChangesAsync();

            _context.ChangeTracker.Clear();

            return true;
        }

        /*
        [HttpGet("IEmail/{CNPJ}")]
        public IActionResult EnviarEmail(string CNPJ)
        {
            Empresas empresas = _context.empresas.FirstOrDefault(p => p.CNPJ.Equals(GetCNPJ(CNPJ)));


            if (empresas == null)
            {
                return NoContent();
            }
            empresas.Senha = CriptoSenha.MD5Senha(empresas.CNPJ);
            IEmail email = _context.email.FirstOrDefault(p => p.ativo);

            EnvioEmail envioEmail = new EnvioEmail(email);
            var a = envioEmail.enviarEmail(empresas);
            return Ok(empresas);
        }

        [HttpPost]
        public IActionResult Login(string cnpj, string senha)
        {
            Empresas empresas = _context.empresas.FirstOrDefault(p => p.CNPJ == GetCNPJ(cnpj));
            if (empresas == null || senha == null)
            {
                return NoContent();
            }

            var Senha = CriptoSenha.MD5Senha(empresas.CNPJ);
            if (empresas.Senha.Equals(senha.ToUpper()))
            {
                return Ok(empresas);
            }
            else
            {
                return NoContent();
            }

        }
        */
    }
}
