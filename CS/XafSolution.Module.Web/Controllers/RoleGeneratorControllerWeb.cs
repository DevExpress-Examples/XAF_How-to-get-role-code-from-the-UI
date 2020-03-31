using DevExpress.ExpressApp.Web.SystemModule;
using System;
using System.IO;
using System.Linq;
using System.Web;
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
    }
}
