using Domain.Data;
using DomainDependencyInjection;
using Lamar;
using System.Collections.Concurrent;
using System.Timers;
using Timer = System.Timers.Timer;

namespace FranquiasBusinessIntelligence.API.Services.Session
{
    public class Session : IDisposable
    {
        private const byte MinutesToCheckSession = 1;
        private const byte MinutesToDie = 120; // x minutos
        private const ushort ConstMinuteToMilliSeconds = 60000;

        private readonly IUser user;
        private readonly ConcurrentDictionary<string, Session> sessions;
        private readonly Timer sessionTimer;
        private readonly string token;
        private readonly Container container;

        private DateTime lastRequest;

        public Session(ConcurrentDictionary<string, Session> sessions, IUser usuario)
            : this(sessions)
        {
            user = usuario;

            container = new Container(opt =>
            {
                opt.Include(DomainServiceRegister.GetRegister());
                opt.For<Session>().Use(this);
            });

            sessionTimer.Start();

            if (!sessions.TryAdd(token, this))
            {
                Dispose();
                throw new InvalidOperationException("Exception to add session on sessions");
            }
        }

        protected Session(ConcurrentDictionary<string, Session> sessions)
        {
            this.sessions = sessions;
            token = Guid.NewGuid().ToString();
            sessionTimer = new Timer(MinutesToCheckSession * ConstMinuteToMilliSeconds);
            lastRequest = DateTime.Now;
            sessionTimer.Elapsed += SessionTimeOutCheck;
        }

        public IUser usuarioLogado => user;

        public string Token => token;

        public DateTime LastRequest => lastRequest;

        public Container Container
        {
            get
            {
                UpdateLastRequest(DateTime.Now);
                return container;
            }
        }

        public void UpdateLastRequest(DateTime? date = null)
        {
            lastRequest = date ?? DateTime.Now;
        }

        public void Dispose()
        {
            sessions.Remove(token, out _);
            sessionTimer.Stop();
            sessionTimer.Dispose();
            container.Dispose();
        }

        private void SessionTimeOutCheck(object? sender, ElapsedEventArgs e)
        {
            if ((DateTime.Now - lastRequest).Minutes >= MinutesToDie)
                Dispose();
        }
    }
}