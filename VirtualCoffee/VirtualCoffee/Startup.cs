using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(VirtualCoffee.Startup))]
namespace VirtualCoffee
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
