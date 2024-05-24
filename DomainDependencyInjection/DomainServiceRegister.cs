using Domain.Data;
using Lamar;

namespace DomainDependencyInjection
{
    public class DomainServiceRegister
    {
        public static ServiceRegistry GetRegister()
        {
            ServiceRegistry registry = new();
            
            registry.For<IUser>().Use<IUser>();

            //registry.For<Empresas>().Use<Empresas>();

            return registry;
        }
    }
}