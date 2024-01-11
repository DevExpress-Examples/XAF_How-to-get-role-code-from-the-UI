<!-- default badges list -->
[![](https://img.shields.io/badge/Open_in_DevExpress_Support_Center-FF7200?style=flat-square&logo=DevExpress&logoColor=white)](https://supportcenter.devexpress.com/ticket/details/T868197)
[![](https://img.shields.io/badge/📖_How_to_use_DevExpress_Examples-e9f6fc?style=flat-square)](https://docs.devexpress.com/GeneralInformation/403183)
<!-- default badges end -->

# XAF - Generate Database Updater Code for Security Roles Created in Application UI in Development Environment

XAF developers often create initial security roles in the administrative UI at runtime in non-production databases of test environments. The visual approach is often faster than writing code manually, especially for [complex permissions with criteria](https://docs.devexpress.com/eXpressAppFramework/113366/data-security-and-safety/security-system#architecture). If you create initial roles at runtime in test databases, at some point, you may need to transfer the data to production databases on customer sites. 

[ModuleUpdater](https://docs.devexpress.com/eXpressAppFramework/DevExpress.ExpressApp.Updating.ModuleUpdater) API and [DBUpdater](https://docs.devexpress.com/eXpressAppFramework/113239/deployment/deployment-tutorial/application-update#update-a-database-dbupdater-tool) are standard means to seed initial data in databases with XAF.

This example demonstrates how to automatically create `ModuleUpdater` code for the required roles created by XAF developers at runtime in test databases. You can easily copy and paste this code into your `ModuleUpdater` descendant and use the standard `DBUpdater` tool to seed data in production databases. We intentionally skip user creation code, because user names are often unknown at early stages and it is much easier to create and link users to predefined roles later in production environment.

![image](https://user-images.githubusercontent.com/14300209/77691659-62c48a00-6fb6-11ea-9d52-d273a30c137d.png)

If this solution does not work for you, you can dump the required data records from the development database to an XML file and then load this XML file from an application using the production database or transfer the data using the built-in RDBMS capabilities. For more information, see the following Support Center ticket: [Security - Best Practices for Export/Import Role Permissions at runtime (without releasing a new application version to clients)](https://supportcenter.devexpress.com/ticket/details/t951640/security-best-practices-for-export-import-role-permissions-at-runtime-without-releasing). Please note, that this solution is applicable only if you use XPO.

> **Note**  
> You can find the solution for .NET Framework, WebForms, and VB.NET in the [20.1.3 branch](https://github.com/DevExpress-Examples/XAF_How-to-get-role-code-from-the-UI/tree/20.1.3+).

## Implementation Details

1. In the Solution Explorer, include [RoleGenerator.csproj](CS/EFCore/GenerateRoleEF/RoleGenerator/RoleGenerator.csproj) into your XAF solution.
2. In the *YourSolutionName.Module* project, add a reference to the *RoleGenerator* project.
3. Add the following files to your XAF solution projects:
	- *YourSolutionName.Module*: [RoleGeneratorController.cs](CS/EFCore/GenerateRoleEF/GenerateRoleEF.Module/Controllers/RoleGeneratorController.cs)
	- *YourSolutionName.Win*: [RoleGeneratorControllerWin.cs](CS/EFCore/GenerateRoleEF/GenerateRoleEF.Win/Controllers/RoleGeneratorControllerWin.cs)
	- *YourSolutionName.Blazor.Server*: [RoleGeneratorControllerBlazor.cs](CS/EFCore/GenerateRoleEF/GenerateRoleEF.Blazor.Server/Controllers/RoleGeneratorControllerBlazor.cs)
4. Modify the [CS/EFCore/GenerateRoleEF/GenerateRoleEF.Win/App.config](CS/EFCore/GenerateRoleEF/GenerateRoleEF.Win/App.config) and [YourSolutionName.Blazor.Server/appsettings.json](CS/EFCore/GenerateRoleEF/GenerateRoleEF.Blazor.Server/appsettings.json) files to add the `EnableRoleGeneratorAction` key.
	   
	``` xml
	<appSettings>
	    ...
	  <add key="EnableRoleGeneratorAction" value="True" />
	</appSettings>
	```
 	
	``` json
	"EnableRoleGeneratorAction": "True",
	```

5. Run the *YourSolutionName.Win* or *YourSolutionName.Blazor.Server* project, select the roles in the `Role` List View, and click the `Generate Role` Action (in the WinForms project, you can find this Action in the **Tools** menu).
 
	**ASP.NET Core Blazor**

	![image](https://github.com/DevExpress-Examples/XAF_How-to-get-role-code-from-the-UI/assets/14300209/a74120d5-a917-4631-aadd-00a926429faa)

	**Windows Forms**

	![image](https://user-images.githubusercontent.com/14300209/77691778-8e477480-6fb6-11ea-9364-a56a90357070.png)

6. Save the generated file. It contains code that creates initial roles based on the data stored in your test database. To use this file in your XAF solution, consider one of the following techniques:
	- Modify the existing *YourSolutionName.Module/DatabaseUpdate/Updater.xx* file based on the `CreateUsersRole` method code copied from the generated *Updater.xx* file.
	- Include the generated *Updater.xx* file into the *YourSolutionName.Module/DatabaseUpdate* folder and modify the [YourSolutionName/Module.cs](CS/EFCore/GenerateRoleEF/GenerateRoleEF.Module/Module.cs) file to use this new `RoleUpdater` class as follows:
 
	``` csharp
	// C#
	using System;
	using DevExpress.ExpressApp;
	using System.Collections.Generic;
	using DevExpress.ExpressApp.Updating;
	
	namespace YourSolutionName.Module {
	  public sealed partial class YourSolutionNameModule : ModuleBase {
	    public override IEnumerable<ModuleUpdater> GetModuleUpdaters(IObjectSpace objectSpace, Version versionFromDB) {
	      ModuleUpdater updater = new DatabaseUpdate.Updater(objectSpace, versionFromDB);
	        ModuleUpdater roleUpdater = new RoleUpdater(objectSpace, versionFromDB);
	    	return new ModuleUpdater[] { updater, roleUpdater };
	//...
	```

	> **Note**  
 	> In the ASP.NET Core Blazor application, the file is saved to the Documents folder by default. You can change this behavior by overriding the `RoleGeneratorControllerBlazor.SaveFile` method.

### Customization for Custom Security Roles

If you have a custom security role class (for example, `ExtendedSecurityRole` implemented in the following examples: [Implement a Custom Security System User Based on an Existing Business Class](https://docs.devexpress.com/eXpressAppFramework/113452/task-based-help/security/how-to-implement-a-custom-security-system-user-based-on-an-existing-business-class), [Implement Custom Security Objects (Users, Roles, Operation Permissions)](https://docs.devexpress.com/eXpressAppFramework/113384/task-based-help/security/how-to-implement-custom-security-objects-users-roles-operation-permissions)) and this class has custom properties, you need to include these properties in the generated code. To do this, handle the `RoleGenerator.CustomizeCodeLines` event in the `RoleGeneratorController` class added at Step 2:

``` csharp
// C#
using System.Collections.Generic;
using System.Linq;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using RoleGeneratorSpace;

namespace XafSolution.Module.Controllers {
    public abstract class RoleGeneratorController : ViewController<ListView> {
	//...
        protected void RoleGeneratorAction_Execute(object sender, SimpleActionExecuteEventArgs e) {
            RoleGenerator roleGenerator = new RoleGenerator(roleType);
            roleGenerator.CustomizeCodeLines += RoleGenerator_CustomizeCodeLines;
            IEnumerable<IPermissionPolicyRole> roleList = e.SelectedObjects.OfType<IPermissionPolicyRole>();
            string updaterCode = roleGenerator.GetUpdaterCode(roleList);
            SaveFile(updaterCode);
        }

        private void RoleGenerator_CustomizeCodeLines(object sender, CustomizeCodeLinesEventArg e) {
            ExtendedSecurityRole exRole = e.Role as ExtendedSecurityRole;
            if(exRole != null) {
                e.CustomCodeLines.Add(string.Format("role.CanExport = {0};", exRole.CanExport.ToString().ToLowerInvariant()));
            }
        }
    }
}

```
> [!WARNING]
> We created this example for demonstration purposes and it is not intended to address all possible usage scenarios with it.
> If this example does not have certain functionality or you want to change its behavior, you can extend this example as needed. Please note that this can be a complex task that requires good knowledge of XAF: [UI Customization Categories by Skill Level](https://www.devexpress.com/products/net/application_framework/xaf-considerations-for-newcomers.xml#ui-customization-categories). You will likely need to research how our components work under the hood. Refer to the following help topic for more information: [Debug DevExpress .NET Source Code with PDB Symbols](https://docs.devexpress.com/GeneralInformation/403656/support-debug-troubleshooting/debug-controls-with-debug-symbols).
> We are unable to help with such tasks as custom programming is outside our Support Service scope: [Technical Support Scope](https://www.devexpress.com/products/net/application_framework/xaf-considerations-for-newcomers.xml#support).

## Files to Review

 - [RoleGeneratorController.cs](CS/EFCore/GenerateRoleEF/GenerateRoleEF.Module/Controllers/RoleGeneratorController.cs)
 - [RoleGeneratorControllerWin.cs](CS/EFCore/GenerateRoleEF/GenerateRoleEF.Win/Controllers/RoleGeneratorControllerWin.cs)
 - [RoleGeneratorControllerBlazor.cs](CS/EFCore/GenerateRoleEF/GenerateRoleEF.Blazor.Server/Controllers/RoleGeneratorControllerBlazor.cs)
 - [RoleGenerator](CS/EFCore/GenerateRoleEF/RoleGenerator)
