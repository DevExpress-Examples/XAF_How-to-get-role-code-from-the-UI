using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using XafSolution.Module.Controllers;

namespace XafSolution.Module.Win.Controllers {
    public class RoleGeneratorControllerWin : RoleGeneratorController {
        protected override void SaveFile(string updaterCode) {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "cs|*.cs";
            if(saveFileDialog.ShowDialog() == DialogResult.OK) {
                using(StreamWriter file = new StreamWriter(saveFileDialog.FileName, false)) {
                    file.Write(updaterCode);
                }
            }
        }
        protected override bool IsEnableRoleGeneratorAction() {
            string enableRoleGeneratorActionString = ConfigurationManager.AppSettings["EnableRoleGeneratorAction"];
            return enableRoleGeneratorActionString == null ? false : bool.Parse(enableRoleGeneratorActionString);
        }
    }
}
