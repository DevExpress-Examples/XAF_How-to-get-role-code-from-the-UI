using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.Base;
using RoleGeneratorSpace;

namespace XafSolution.Module.Controllers {
    public abstract class RoleGeneratorController : ViewController<ListView> {
        private Type roleType;
        private IContainer components = null;
        protected SimpleAction roleGeneratorAction;
        public RoleGeneratorController() {
            InitializeComponent();
            TargetObjectType = typeof(IPermissionPolicyRole);
        }
        private void InitializeComponent() {
            components = new Container();
            roleGeneratorAction = new SimpleAction(components);
            roleGeneratorAction.Caption = "Generate Role";
            roleGeneratorAction.Category = "Tools";
            roleGeneratorAction.ConfirmationMessage = null;
            roleGeneratorAction.Id = "RoleGeneratorAction";
            roleGeneratorAction.ImageName = "Action_Export";
            roleGeneratorAction.ToolTip = null;
            roleGeneratorAction.Execute += new SimpleActionExecuteEventHandler(RoleGeneratorAction_Execute);
            Actions.Add(roleGeneratorAction);
        }
        protected override void OnActivated() {
            base.OnActivated();
            SecurityStrategy security = Application.GetSecurityStrategy();
            roleType = ((SecurityStrategyComplex)(Application.Security)).RoleType;
            roleGeneratorAction.Active["ActionExecuted"] = IsEnableRoleGeneratorAction() && security.CanRead(roleType);
        }
        protected void RoleGeneratorAction_Execute(object sender, SimpleActionExecuteEventArgs e) {
            RoleGenerator roleGenerator = new RoleGenerator(roleType);
            IEnumerable<IPermissionPolicyRole> roleList = e.SelectedObjects.OfType<IPermissionPolicyRole>();
            string updaterCode = roleGenerator.GetUpdaterCode(roleList);
            SaveFile(updaterCode);
        }
        protected abstract void SaveFile(string updaterCode);
        private bool IsEnableRoleGeneratorAction() {
            string enableRoleGeneratorActionString = ConfigurationManager.AppSettings["EnableRoleGeneratorAction"];
            return enableRoleGeneratorActionString == null ? false : bool.Parse(enableRoleGeneratorActionString);
        }
    }
}
