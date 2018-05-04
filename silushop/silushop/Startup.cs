using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(silushop.Startup))]
namespace silushop
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
