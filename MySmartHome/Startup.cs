using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MySmartHome.Startup))]
namespace MySmartHome
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
