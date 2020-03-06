# How to get role code from the UI

This article demonstrates how to get role code from the UI.
![](images/result.png)

### Problem
You can create roles in the UI to test them, but it's difficult to write code that creates these roles in the Updater. 
You can use the approach described below to create an Updater file from the UI.


### How does it work?
 **1.** Add the [RoleGenerator.csproj](CS/RoleGenerator/RoleGenerator.csproj)/[RoleGenerator.vbproj](VB/RoleGenerator/RoleGenerator.vbproj) project to your solution.
 
 **2.** Add a reference to the RoleGenerator project to your Module project.
 
 **3.** Add the [RoleGeneratorController.cs](CS/XafSolution.Module/Controllers/RoleGeneratorController.cs)/[RoleGeneratorController.vb](VB/XafSolution.Module/Controllers/RoleGeneratorController.vb) class to your Module project.
 
 **4.** Add the [RoleGeneratorControllerWin.cs](CS/XafSolution.Module.Win/Controllers/RoleGeneratorControllerWin.cs)/[RoleGeneratorControllerWin.vb](VB/XafSolution.Module.Win/Controllers/RoleGeneratorControllerWin.vb) controller to your Module.Win project and the [RoleGeneratorControllerWeb.cs](CS/XafSolution.Module.Web/Controllers/RoleGeneratorControllerWeb.cs)/[RoleGeneratorControllerWeb.vb](VB/XafSolution.Module.Web/Controllers/RoleGeneratorControllerWeb.vb) controller to your Module.Web project.
 
 **5.** Add the following key to the [App.config](XafSolution.Module.Win/app.config) file in your Win project and to the [Web.config](XafSolution.Module.Web/Web.config) file in your Web project
``` xml
<appSettings>
    ...
  <add key="EnableRoleGeneratorAction" value="True" />
</appSettings>
```
 **6.** Run your Win or Web project, select roles in RoleListView, and click the Generate Role action. In Win projects, this action is in the 'Tools' category.
 
 WinForms:
    ![](images/win.jpg)
    
ASP.NET:
    ![](images/web.jpg)
 **7.** Save the file. 
 
 You will get a ready-to-use Updater file with the code that creates roles.
 
 
 To use this file in your XAF app, follow these steps:
 
 **1.** Include it in your Module project.
 
 **2.** Modify your [Module.cs](CS/XafSolution.Module/Module.cs)/[Module.vb](VB/XafSolution.Module/Module.vb) file to use this new Updater:
 
CS:
``` csharp
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
VB:
``` vb
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

### Customization
If you have a custom role, customize [RoleGenerator](RoleGenerator/RoleGenerator.cs) to get the necessary code lines.
To do it, modify the `GetCodeLinesFromRole` method and add necessary code lines in `codeLines` List.

CS:
``` csharp
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
        //codeLines.Add("your custom code line");
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

VB:
``` vb
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
           'codeLines.Add("your custom code line")
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
