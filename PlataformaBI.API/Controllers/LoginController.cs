using DataAccess;
using Domain.Data;
using FranquiasBusinessIntelligence.API.Services.Session;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace PlataformaBI.API.Controllers
{
    ///
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowOrigin")]
    public class LoginController : FranquiasBIController
    {
        private readonly FranquiasBIDbContext _context;
        private readonly ConcurrentDictionary<string, Session> sessions;

        ///
        public LoginController(FranquiasBIDbContext context, ConcurrentDictionary<string, Session> sessions)
            : base(sessions)
        {
            _context = context;
            this.sessions = sessions;
        }

        /// <summary>
        /// Verifica se há uma sessão ativa
        /// </summary>
        [HttpGet("sessionExists")]
        public IActionResult GetSessionExists()
        {
            if (!UserAuthenticated)
                return Unauthorized();

            return Ok();
        }

        /// <summary>
        /// Retorna o token da sessão do usuário a partir do envio das credenciais corretas
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] UsuariosLogin user)
        {
            if (user.Email == null || user.Password == null)
                return BadRequest();

            //senha = CriptoSenha.MD5Senha(senha);
            IUser? usuarioLogado = await _context.User.FirstOrDefaultAsync(p => p.Email.Equals(user.Email) && p.Senha.Equals(user.Password));

            if (usuarioLogado == null)
                return BadRequest();

            var sessionExists = sessions.Values.FirstOrDefault(x => x.usuarioLogado.ID == usuarioLogado.ID);

            if (sessionExists is null)
            {
                sessionExists = new Session(this.sessions, usuarioLogado);
            }
            else
            {
                sessionExists.UpdateLastRequest();
            }

            HttpContext.Response.Headers.Add("api-fbi-token", sessionExists.Token);

            var userToReturn = sessionExists.usuarioLogado;
            userToReturn.Senha = string.Empty;

            return Ok(userToReturn);
        }

        /// <summary>
        /// Realiza o logout
        /// </summary>
        [HttpPost("logoutSession")]
        public void Logout()
        {
            if (Session.Token != null)
                Session.Dispose();
        }
    }
}