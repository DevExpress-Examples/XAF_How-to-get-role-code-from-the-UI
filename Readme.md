# How to generate database updater code for security users and roles from the XAF application UI in a development environment

## Scenario
XAF developers often create initial security users and roles in the application UI (at runtime) using non-production databases and test environments. The administrative UI may be faster than writing C# or VB.NET code especially for complex permissions with criteria ([check screenshots](https://docs.devexpress.com/eXpressAppFramework/113366/concepts/security-system)). If you create initial users and roles at runtime with test databases, you need to eventually transfer this initial data to production databases on customer sites.

## Solution
[ModuleUpdater](https://docs.devexpress.com/eXpressAppFramework/DevExpress.ExpressApp.Updating.ModuleUpdater) API and [DBUpdater](https://docs.devexpress.com/eXpressAppFramework/113239/deployment/deployment-tutorial/application-update#update-database-via-the-dbupdater-tool) are standard means to seed initial data in the database with XAF.
We will demonstrate how to automatically create ModuleUpdater C# or VB.NET code for required users and roles created by XAF developers at runtime with test databases. XAF developers then can easly copy and paste this ready code into their ModuleUpdater descendant and use the standard DBUpdater tool to seed data in production databases.
![](images/result.png)

---

## Implementation Steps

**Step 1.** Include the [RoleGenerator.csproj](CS/RoleGenerator/RoleGenerator.csproj) or [RoleGenerator.vbproj](VB/RoleGenerator/RoleGenerator.vbproj) project to your XAF solution and add the *RoleGenerator* reference to the *YourSolutionName.Module* project.
 
**Step 2.** Include the following files into your XAF solution projects
 - *YourSolutionName.Module*: [RoleGeneratorController.cs](CS/XafSolution.Module/Controllers/RoleGeneratorController.cs) or [RoleGeneratorController.vb](VB/XafSolution.Module/Controllers/RoleGeneratorController.vb);
 - *YourSolutionName.Module.Win*: [RoleGeneratorControllerWin.cs](CS/XafSolution.Module.Win/Controllers/RoleGeneratorControllerWin.cs) or [RoleGeneratorControllerWin.vb](VB/XafSolution.Module.Win/Controllers/RoleGeneratorControllerWin.vb);
 - *YourSolutionName.Module.Web*: [RoleGeneratorControllerWeb.cs](CS/XafSolution.Module.Web/Controllers/RoleGeneratorControllerWeb.cs) or [RoleGeneratorControllerWeb.vb](VB/XafSolution.Module.Web/Controllers/RoleGeneratorControllerWeb.vb).
 
**Step 3.** Modify the [YourSolutionName.Win/App.config](XafSolution.Win/app.config) and [YourSolutionName.Web/Web.config](XafSolution.Web/Web.config) files to add the `EnableRoleGeneratorAction` key under the `appSettings` section.
``` xml
<appSettings>
    ...
  <add key="EnableRoleGeneratorAction" value="True" />
</appSettings>
```
**Step 4.** Run the *YourSolutionName.Win* or *YourSolutionName.Web* project, select roles in the `Role` ListView, and click the `Generate Role` Action (in the WinForms project, this Action is in the `Tools` menu).
 
WinForms:
    ![](images/win.jpg)
   
ASP.NET WebForms:
    ![](images/web.jpg)
    
**Step 5.** Save the generated file - it contains the code that creates initial users and roles based on data stored in your test database. To use this file in your XAF solution, consider one of the two strategies:
 - Modify the existing *YourSolutionName.Module/DatabaseUpdate/Updater.xx* file based on the CreateUsersRole method code copied from the generated Updater.xx file.
 - Include the generated Updater.xx file into the *YourSolutionName.Module/DatabaseUpdate* folder and modify the [YourSolutionName/Module.cs](CS/XafSolution.Module/Module.cs)/[YourSolutionName/Module.vb](VB/XafSolution.Module/Module.vb) file to use this new RoleUpdater class as follows:
 
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
If you do not use PermissionPolicyRole and have a custom security role class ([example 1](https://docs.devexpress.com/eXpressAppFramework/113452/task-based-help/security/how-to-implement-a-custom-security-system-user-based-on-an-existing-business-class), [example 2](https://docs.devexpress.com/eXpressAppFramework/113384/task-based-help/security/how-to-implement-custom-security-objects-users-roles-operation-permissions)), modify the `GetCodeLinesFromRole` method inside [RoleGenerator](RoleGenerator/RoleGenerator.cs) and add necessary code lines in `codeLines` list.

``` csharp
// C#
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.Base;
using System;
using System.Collections.Generic;

namespace RoleGeneratorSpace {
  public class RoleGenerator {
    private List<string> GetCodeLinesFromRole(IPermissionPolicyRole role) {
      List<string> codeLines = new List<string>();
      if(role != null) {
        codeLines.Add($"{variableName}.Name = \"{role.Name}\";");
        codeLines.Add($"{variableName}.PermissionPolicy = SecurityPermissionPolicy.{role.PermissionPolicy.ToString()};");
        if(role.IsAdministrative) {
          codeLines.Add($"{variableName}.IsAdministrative = true;");
        }
        if(role.CanEditModel) {
          codeLines.Add($"{variableName}.CanEditModel = true;");
        }
        //place your custom code here
        //codeLines.Add("your custom code line"); Dennis: добавьте сюда конкретный пример кода для случая с EmployeeRole.
        foreach(IPermissionPolicyTypePermissionObject typePermissionObject in role.TypePermissions) {
          codeLines.AddRange(GetCodeLinesFromTypePermissionObject(typePermissionObject));
        }
        if(role is INavigationPermissions navigationPermissionsRole) {
          foreach(IPermissionPolicyNavigationPermissionObject navigationPermissionObject in navigationPermissionsRole.NavigationPermissions) {
            string codeLine = GetCodeLine(navigationPermissionObject);
            if(codeLine != string.Empty) {
              codeLines.Add(codeLine);
            }
          }
        }
      }
      return codeLines;
    }
//...
```

``` vb
' VB.NET
Imports DevExpress.ExpressApp.Security
Imports DevExpress.Persistent.Base
Imports System
Imports System.Collections.Generic

Namespace RoleGeneratorSpace
  Public Class RoleGenerator {
    Private Function GetCodeLinesFromRole(ByVal role As IPermissionPolicyRole) As List(Of String)
       Dim codeLines As New List(Of String)()
       If role IsNot Nothing Then
           codeLines.Add($"{variableName}.Name = ""{role.Name}""")
           codeLines.Add($"{variableName}.PermissionPolicy = SecurityPermissionPolicy.{role.PermissionPolicy.ToString()}")
           If role.IsAdministrative Then
               codeLines.Add($"{variableName}.IsAdministrative = true")
           End If
           If role.CanEditModel Then
               codeLines.Add($"{variableName}.CanEditModel = true")
           End If
           'place your custom code here
           'codeLines.Add("your custom code line") Dennis: добавьте сюда конкретный пример кода для случая с EmployeeRole.
           For Each typePermissionObject As IPermissionPolicyTypePermissionObject In role.TypePermissions
               codeLines.AddRange(GetCodeLinesFromTypePermissionObject(typePermissionObject))
           Next typePermissionObject
           If TypeOf role Is INavigationPermissions Then
               Dim navigationPermissionsRole As INavigationPermissions = CType(role, INavigationPermissions)
               For Each navigationPermissionObject As IPermissionPolicyNavigationPermissionObject In navigationPermissionsRole.NavigationPermissions
                   Dim codeLine As String = GetCodeLine(navigationPermissionObject)
                   If codeLine <> String.Empty Then
                       codeLines.Add(codeLine)
                   End If
               Next navigationPermissionObject
           End If
       End If
       Return codeLines
   End Function
'...
```
