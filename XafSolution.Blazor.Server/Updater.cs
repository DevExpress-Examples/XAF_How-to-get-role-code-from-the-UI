using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Updating;
using DevExpress.Persistent.Base;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using XafSolution.Module.BusinessObjects;
using DevExpress.Persistent.BaseImpl;

namespace YourNameSpace {
	public class RoleUpdater : ModuleUpdater {
		public RoleUpdater(IObjectSpace objectSpace, Version currentDBVersion) :
			base(objectSpace, currentDBVersion) {
		}
		public override void UpdateDatabaseAfterUpdateSchema() {
			base.UpdateDatabaseAfterUpdateSchema();
			CreateDefaultRole();
			ObjectSpace.CommitChanges();
		}
		public void CreateDefaultRole() {
			PermissionPolicyRole role = ObjectSpace.FindObject<PermissionPolicyRole>(new BinaryOperator("Name", "Default"));
			if(role == null) {
				role = ObjectSpace.CreateObject<PermissionPolicyRole>();
				role.Name = "Default";
				role.PermissionPolicy = SecurityPermissionPolicy.DenyAllByDefault;
				role.AddTypePermission<PermissionPolicyRole>(SecurityOperations.Read, SecurityPermissionState.Deny);
				role.AddTypePermission<Employee>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
				role.AddObjectPermission<Employee>(SecurityOperations.Delete, "Contains([Department.Title], 'Development')", SecurityPermissionState.Allow);
				role.AddMemberPermission<Employee>(SecurityOperations.Write, "LastName", "Not Contains([Department.Title], 'Development')", SecurityPermissionState.Deny);
				role.AddTypePermission<ModelDifferenceAspect>("Read;Write;Create", SecurityPermissionState.Allow);
				role.AddTypePermission<Department>(SecurityOperations.Read, SecurityPermissionState.Deny);
				role.AddObjectPermission<Department>(SecurityOperations.Read, "Contains([Title], 'Development')", SecurityPermissionState.Allow);
				role.AddObjectPermission<ApplicationUser>(SecurityOperations.Read, "[Oid] = CurrentUserId()", SecurityPermissionState.Allow);
				role.AddMemberPermission<ApplicationUser>(SecurityOperations.Write, "StoredPassword", "[Oid] = CurrentUserId()", SecurityPermissionState.Allow);
				role.AddMemberPermission<ApplicationUser>(SecurityOperations.Write, "ChangePasswordOnFirstLogon", "[Oid] = CurrentUserId()", SecurityPermissionState.Allow);
				role.AddTypePermission<ModelDifference>("Read;Write;Create", SecurityPermissionState.Allow);
				role.AddNavigationPermission(@"Application/NavigationItems/Items/Default/Items/Employee_ListView", SecurityPermissionState.Allow);
				role.AddNavigationPermission(@"Application/NavigationItems/Items/Default/Items/MyDetails", SecurityPermissionState.Allow);
				role.AddNavigationPermission(@"Application/NavigationItems/Items/Default/Items/Department_ListView", SecurityPermissionState.Allow);
				role.AddActionPermission("RoleGeneratorAction");
			}
		}
	}
}

