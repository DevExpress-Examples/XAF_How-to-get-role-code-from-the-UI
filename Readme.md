This article demonstrates how to get role code from the UI.


### Problem
You can create roles in the UI to test them, but it's difficult to write code that creates these roles in the Updater. 
You can use the approach described below to create an Updater file from the UI.


### How does it work?
 - Add the [RoleGenerator](RoleGenerator/RoleGenerator.cs) project to your solution.
 - Add a reference to the RoleGenerator project to your Module project.
 - Add the [RoleGeneratorController](XafSolution.Module/Controllers/RoleGeneratorController.cs) class to your Module project.
 - Add the [RoleGeneratorControllerWin](XafSolution.Module.Win/Controllers/RoleGeneratorControllerWin.cs) controller to your Module.Win project and the [RoleGeneratorControllerWeb](XafSolution.Module.Web/Controllers/RoleGeneratorControllerWeb.cs) controller to your Module.Web project.
 - Add the following key to the `App.config` file in your Win project and to the `Web.config` file in your Web project
``` xml
<appSettings>
    ...
    <add key="EnableRoleGeneratorAction" value="True" />
</appSettings>
```
 - Run your Win or Web project, select roles in RoleListView, and click the Generate Role action. In Win projects, this action is in the 'Tools' category.
    ![](images/win.jpg)
    ![](images/web.jpg)
 - Save the file. You will get a ready-to-use Updater file with the code that creates roles.
 
 To use this file in your XAF app, follow these steps:
 - Include  it in your Module project.
 - Modify your `Module.cs` file to use this new Updater:

  	[](#tab/tabid-csharp)

``` csharp
public override IEnumerable<ModuleUpdater> GetModuleUpdaters(IObjectSpace objectSpace, Version versionFromDB) {
    ModuleUpdater updater = new DatabaseUpdate.Updater(objectSpace, versionFromDB);
    ModuleUpdater roleUpdater = new RoleUpdater(objectSpace, versionFromDB);
    return new ModuleUpdater[] { updater, roleUpdater };
}
```

### Customization
If you have a custom role, customize [RoleGenerator](RoleGenerator/RoleGenerator.cs) to get the necessary code lines.
To do it, modify the `GetCodeLinesFromRole` method and add necessary code lines in `codeLines` List.

``` csharp
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
```
