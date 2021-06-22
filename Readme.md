# How to generate database updater code for security roles created via the application UI in a development environment

## Scenario
XAF developers often create initial security roles via the administrative UI (at runtime) in non-production databases of their test environments. The visual approach is often faster than writing C# or VB.NET code manually, especially for complex permissions with criteria ([check screenshots](https://docs.devexpress.com/eXpressAppFramework/113366/concepts/security-system)). If you create initial roles at runtime with test databases, you need to eventually transfer this data to production databases on customer sites.

## Solution
[ModuleUpdater](https://docs.devexpress.com/eXpressAppFramework/DevExpress.ExpressApp.Updating.ModuleUpdater) API and [DBUpdater](https://docs.devexpress.com/eXpressAppFramework/113239/deployment/deployment-tutorial/application-update#update-a-database-dbupdater-tool) are standard means to seed initial data in databases with XAF.
We will demonstrate how to automatically create ModuleUpdater C# or VB.NET code for required roles created by XAF developers at runtime with test databases. XAF developers can easily copy and paste this ready code into their ModuleUpdater descendant and use the standard DBUpdater tool to seed data in production databases. We intentionally skip user creation code, because user names are often unknown at early stages and it is much easier to create and link users to predefined roles later in production environment.
![image](https://user-images.githubusercontent.com/14300209/77691659-62c48a00-6fb6-11ea-9d52-d273a30c137d.png)

>If this solution does not work for you, you can dump required data records from the development database to an XML file and then load this XML file from an application using the production database or transfer data using the built-in RDBMS capabilities. For more information, please review [Security - Best Practices for Export/Import Role Permissions at runtime (without releasing a new application version to clients)](https://supportcenter.devexpress.com/ticket/details/t951640/security-best-practices-for-export-import-role-permissions-at-runtime-without-releasing).

---

## Implementation Steps

**Step 1.** In the Solution Explorer, include [RoleGenerator.csproj](CS/RoleGenerator/RoleGenerator.csproj) or [RoleGenerator.vbproj](VB/RoleGenerator/RoleGenerator.vbproj) into your XAF solution and then reference this *RoleGenerator* project from the *YourSolutionName.Module* one.
 
**Step 2.** Reference the *System.Configuration.dll* assembly from the *YourSolutionName.Module* project and add the following files to your XAF solution projects
 - *YourSolutionName.Module*: [RoleGeneratorController.cs](CS/XafSolution.Module/Controllers/RoleGeneratorController.cs) or [RoleGeneratorController.vb](VB/XafSolution.Module/Controllers/RoleGeneratorController.vb);
 - *YourSolutionName.Module.Win*: [RoleGeneratorControllerWin.cs](CS/XafSolution.Module.Win/Controllers/RoleGeneratorControllerWin.cs) or [RoleGeneratorControllerWin.vb](VB/XafSolution.Module.Win/Controllers/RoleGeneratorControllerWin.vb);
 - *YourSolutionName.Module.Web*: [RoleGeneratorControllerWeb.cs](CS/XafSolution.Module.Web/Controllers/RoleGeneratorControllerWeb.cs) or [RoleGeneratorControllerWeb.vb](VB/XafSolution.Module.Web/Controllers/RoleGeneratorControllerWeb.vb).

**Step 3.** Modify the [YourSolutionName.Win/App.config](CS/XafSolution.Win/App.config) and [YourSolutionName.Web/Web.config](CS/XafSolution.Web/Web.config) files to add the `EnableRoleGeneratorAction` key under the `appSettings` section.
``` xml
<appSettings>
    ...
  <add key="EnableRoleGeneratorAction" value="True" />
</appSettings>
```
**Step 4.** Run the *YourSolutionName.Win* or *YourSolutionName.Web* project, select roles in the `Role` ListView, and click the `Generate Role` Action (in the WinForms project, this Action is under the `Tools` menu).
 
WinForms:

![image](https://user-images.githubusercontent.com/14300209/77691778-8e477480-6fb6-11ea-9364-a56a90357070.png)
   
ASP.NET WebForms:

![image](https://user-images.githubusercontent.com/14300209/77691846-a5866200-6fb6-11ea-8bb7-30146c2e0ced.png)
    
**Step 5.** Save the generated file and research its content. It contains code that creates initial roles based on data stored in your test database. To use this file in your XAF solution, consider one of these two strategies:
 - Modify the existing *YourSolutionName.Module/DatabaseUpdate/Updater.xx* file based on the `CreateUsersRole` method code copied from the generated *Updater.xx* file.
 - Include the generated *Updater.xx* file into the *YourSolutionName.Module/DatabaseUpdate* folder and modify the [YourSolutionName/Module.cs](CS/XafSolution.Module/Module.cs) or [YourSolutionName/Module.vb](VB/XafSolution.Module/Module.vb) file to use this new `RoleUpdater` class as follows:
 
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
``` vb
' VB.NET
Imports System
Imports DevExpress.ExpressApp
Imports System.Collections.Generic
Imports DevExpress.ExpressApp.Updating

Namespace YourSolutionName.Module {
  Public NotInheritable Partial Class YourSolutionNameModule
   Inherits ModuleBase
    Public Overrides Function GetModuleUpdaters(ByVal objectSpace As IObjectSpace, ByVal versionFromDB As Version) As IEnumerable(Of ModuleUpdater)
        Dim updater As ModuleUpdater = New DatabaseUpdate.Updater(objectSpace, versionFromDB)
        Dim roleUpdater As ModuleUpdater = New DatabaseUpdate.RoleUpdater(objectSpace, versionFromDB)
        Return New ModuleUpdater() {updater, roleUpdater}
    End Function
'...
```

## Customization for Custom Security Roles
If you have a custom security role class (for instance, `ExtendedSecurityRole` as in these examples: [one](https://docs.devexpress.com/eXpressAppFramework/113452/task-based-help/security/how-to-implement-a-custom-security-system-user-based-on-an-existing-business-class), [two](https://docs.devexpress.com/eXpressAppFramework/113384/task-based-help/security/how-to-implement-custom-security-objects-users-roles-operation-permissions)) and this class has custom properties, you need to include these properties into the generated code. To do this, handle the `RoleGenerator.CustomizeCodeLines` event in the `RoleGeneratorController` class added at Step 2:

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

``` vb
' VB.NET
Imports System
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Linq
Imports DevExpress.ExpressApp
Imports DevExpress.ExpressApp.Actions
Imports DevExpress.ExpressApp.Security
Imports DevExpress.Persistent.Base
Imports RoleGeneratorSpace
Imports XafSolution.Security

Namespace XafSolution.Module.Controllers
    Public MustInherit Class RoleGeneratorController
        Inherits ViewController(Of ListView)
'...
        Protected Sub RoleGeneratorAction_Execute(ByVal sender As Object, ByVal e As SimpleActionExecuteEventArgs) Handles roleGeneratorAction.Execute
            Dim roleGenerator As New RoleGenerator(roleType)
            AddHandler roleGenerator.CustomizeCodeLines, AddressOf RoleGenerator_CustomizeCodeLines
            Dim roleList As IEnumerable(Of IPermissionPolicyRole) = e.SelectedObjects.OfType(Of IPermissionPolicyRole)()
            Dim updaterCode As String = roleGenerator.GetUpdaterCode(roleList)
            SaveFile(updaterCode)
        End Sub
		
        Private Sub RoleGenerator_CustomizeCodeLines(ByVal sender As Object, ByVal e As CustomizeCodeLinesEventArg)
            Dim exRole As ExtendedSecurityRoleTS = TryCast(e.Role, ExtendedSecurityRoleTS)
            If exRole IsNot Nothing Then
                e.CustomCodeLines.Add(String.Format("role.CanExport = {0}", exRole.CanExport))
            End If
        End Sub
      
    End Class
End Namespace

'...
```

> Note: If you have a .NET Core project, use the following way to generate roles' code:
> - create a separate .NET Framework project that is connected to the same database and references the same business classes;
> - use the solution from this example with this newly created project to generate roles' code.




