using System.Collections.Concurrent;
using FranquiasBusinessIntelligence.API.Services.Session;
using Lamar;
using Microsoft.AspNetCore.Mvc;

namespace PlataformaBI.API.Controllers
{
    ///
    public class FranquiasBIController : ControllerBase
    {
        private const string HeaderTokenName = "api-fbi-token";

        private readonly ConcurrentDictionary<string, Session> sessions;

        ///
        public FranquiasBIController(ConcurrentDictionary<string, Session> sessions)
        {
            this.sessions = sessions;
        }

        ///
        public bool UserAuthenticated
        {
            get
            {
                string st = this.GetToken();
                return this.sessions.Any(x => x.Key.Equals(st));
            }
        }

        ///
        public Session Session => this.sessions[this.GetToken()];

        ///
        public Container Container => this.Session.Container;

        private string GetToken()
        {
            if (!Request.Headers.Keys.Contains(HeaderTokenName))
                return string.Empty;

            return Request.Headers[HeaderTokenName];
        }
    }
}
