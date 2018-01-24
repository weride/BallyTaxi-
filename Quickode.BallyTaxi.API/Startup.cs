using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Quickode.BallyTaxi.API.Startup))]
[assembly: log4net.Config.XmlConfigurator(ConfigFile = "Web.config", Watch = true)]
namespace Quickode.BallyTaxi.API
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
              new System.Threading.Tasks.Task(() => {
                  Domain.Services.OrderService.HandlePendingOrders();
              }).Start();
            //Domain.Services.OrderService.HandlePendingOrders();
        }
    }
}
