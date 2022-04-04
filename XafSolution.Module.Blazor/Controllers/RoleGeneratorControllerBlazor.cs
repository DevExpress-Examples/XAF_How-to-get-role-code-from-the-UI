using DevExpress.ExpressApp.Blazor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XafSolution.Module.Controllers;

namespace XafSolution.Module.Blazor.Controllers {
    public class RoleGeneratorControllerBlazor : RoleGeneratorController {
        protected override void SaveFile(string updaterCode) {
            //Place your save logic here
            File.WriteAllText(@"C:\Updater.cs", updaterCode);
        }
        protected override bool IsEnableRoleGeneratorAction() {
            IConfiguration configuration = ((BlazorApplication)Application).ServiceProvider.GetRequiredService<IConfiguration>();
            string enableRoleGeneratorActionString = configuration.GetSection("EnableRoleGeneratorAction").Value;
            return enableRoleGeneratorActionString == null ? false : bool.Parse(enableRoleGeneratorActionString);
        }
    }
}
