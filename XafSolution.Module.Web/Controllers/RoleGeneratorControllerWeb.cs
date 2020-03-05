using DevExpress.ExpressApp.Web.SystemModule;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using XafSolution.Module.Controllers;

namespace XafSolution.Module.Web.Controllers {
	public class RoleGeneratorControllerWeb : RoleGeneratorController {
        protected override void OnActivated() {
            base.OnActivated();
            roleGeneratorAction.Model.SetValue("IsPostBackRequired", true);
        }
        private MemoryStream GetStreamFromString(string targetString) {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(targetString);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
        protected override void SaveFile(string updaterCode) {
            using(MemoryStream stream = GetStreamFromString(updaterCode)) {
                HttpContext.Current.Response.ClearHeaders();
                ResponseWriter.WriteFileToResponse(stream, "RoleUpdater.cs");
            }
        }
        protected override bool IsEnableRoleGeneratorAction() {
            string enableRoleGeneratorActionString = WebConfigurationManager.AppSettings["EnableRoleGeneratorAction"];
            return enableRoleGeneratorActionString == null ? false : bool.Parse(enableRoleGeneratorActionString);
        }
    }
}
