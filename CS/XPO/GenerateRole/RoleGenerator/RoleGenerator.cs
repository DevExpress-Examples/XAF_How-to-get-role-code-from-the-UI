using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.Base;
using RoleGenerator;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RoleGeneratorSpace
{
	public class RoleGenerator
	{
		private Type roleType;
		private Dictionary<string, List<string>> codeLines = new Dictionary<string, List<string>>();
		private HashSet<string> nameSpacesCodeLines = new HashSet<string>();
		public event EventHandler<CustomizeCodeLinesEventArg> CustomizeCodeLines;
		public RoleGenerator(Type roleType)
		{
			this.roleType = roleType;
			nameSpacesCodeLines.Add(typeof(PermissionPolicy).Namespace);
			nameSpacesCodeLines.Add(roleType.Namespace);
		}

		string SanitizeRole(string name) {
			if (string.IsNullOrEmpty(name))
			{
				return "Role";
			}

			// Replace spaces and common separators with underscores
			string sanitized = name.Replace(' ', '_')
				.Replace('-', '_')
				.Replace('.', '_')
				.Replace('/', '_')
				.Replace('\\', '_')
				.Replace('(', '_')
				.Replace(')', '_')
				.Replace('[', '_')
				.Replace(']', '_')
				.Replace('{', '_')
				.Replace('}', '_')
				.Replace('<', '_')
				.Replace('>', '_')
				.Replace('!', '_')
				.Replace('@', '_')
				.Replace('#', '_')
				.Replace('$', '_')
				.Replace('%', '_')
				.Replace('^', '_')
				.Replace('&', '_')
				.Replace('*', '_')
				.Replace('+', '_')
				.Replace('=', '_')
				.Replace('|', '_')
				.Replace(':', '_')
				.Replace(';', '_')
				.Replace('"', '_')
				.Replace('\'', '_')
				.Replace('?', '_')
				.Replace(',', '_');

			// Remove any characters that are not letters, digits, or underscores
			sanitized = System.Text.RegularExpressions.Regex.Replace(sanitized, @"[^\w]", "_");

			// Remove consecutive underscores
			sanitized = System.Text.RegularExpressions.Regex.Replace(sanitized, @"_+", "_");

			// Remove leading and trailing underscores
			sanitized = sanitized.Trim('_');

			// Ensure it starts with a letter or underscore (not a digit)
			if (sanitized.Length > 0 && char.IsDigit(sanitized[0]))
			{
				sanitized = "_" + sanitized;
			}

			// If the result is empty or only underscores, return a default name
			if (string.IsNullOrEmpty(sanitized) || sanitized.All(c => c == '_'))
			{
				return "Role"+DateTime.Now.Millisecond;
			}

			// Ensure the first character is uppercase for proper method naming convention
			return char.ToUpper(sanitized[0]) + sanitized.Substring(1);
		}
		public string GetUpdaterCode(IEnumerable<IPermissionPolicyRole> roleList)
		{
			if (roleList == null)
			{
				return string.Empty;
			}
			UpdaterRuntimeTemplate template = new UpdaterRuntimeTemplate();
			foreach (IPermissionPolicyRole role in roleList)
			{
				var sanitizedRoleName = SanitizeRole(role.Name);

                codeLines.Add(sanitizedRoleName, GetCodeLinesFromRole(role, sanitizedRoleName));
			}
			//template.Session = new Dictionary<string, object>();
			//template.Session["CodeLines"] = codeLines;
			//template.Session["NameSpacesCodeLines"] = nameSpacesCodeLines;
			//template.Session["RoleTypeName"] = roleType.Name;
			//template.Initialize();
			template.CodeLines = codeLines;
			template.NameSpacesCodeLines = nameSpacesCodeLines;
			template.RoleTypeName = roleType.Name;
			return template.TransformText();
		}
		private List<string> GetCodeLinesFromRole(IPermissionPolicyRole role,string sanitizedRoleName)
		{
			List<string> codeLines = new List<string>();
			if (role != null)
			{
				codeLines.Add($"role.Name = \"{sanitizedRoleName}\";");
				codeLines.Add($"role.PermissionPolicy = SecurityPermissionPolicy.{role.PermissionPolicy.ToString()};");
				if (role.IsAdministrative)
				{
					codeLines.Add($"role.IsAdministrative = true;");
				}
				if (role.CanEditModel)
				{
					codeLines.Add($"role.CanEditModel = true;");
				}
				if (CustomizeCodeLines != null)
				{
					List<string> customCodeLines = new List<string>();
					CustomizeCodeLines(this, new CustomizeCodeLinesEventArg(role, customCodeLines));
					codeLines.AddRange(customCodeLines);
				}
				foreach (IPermissionPolicyTypePermissionObject typePermissionObject in role.TypePermissions)
				{
					codeLines.AddRange(GetCodeLinesFromTypePermissionObject(typePermissionObject));
				}
				if (role is INavigationPermissions navigationPermissionsRole)
				{
					foreach (IPermissionPolicyNavigationPermissionObject navigationPermissionObject in navigationPermissionsRole.NavigationPermissions)
					{
						string codeLine = GetCodeLine(navigationPermissionObject);
						if (codeLine != string.Empty)
						{
							codeLines.Add(codeLine);
						}
					}
				}
				if (role is IActionPermissions actionPermissionsRole)
				{
					foreach (IPermissionPolicyActionPermissionObject actionPermissionsObject in actionPermissionsRole.ActionPermissions)
					{
						string codeLine = GetCodeLine(actionPermissionsObject);
						if (codeLine != string.Empty)
						{
							codeLines.Add(codeLine);
						}
					}
				}
			}
			return codeLines;
		}
		private List<string> GetCodeLinesFromTypePermissionObject(IPermissionPolicyTypePermissionObject typePermissionObject)
		{
			List<string> codeLines = new List<string>();
			Type targetType = typePermissionObject.TargetType;
			if (targetType != null)
			{
				nameSpacesCodeLines.Add(targetType.Namespace);
				OperationBuilder allowOperationBuilder = new OperationBuilder();
				OperationBuilder dennyOperationBuilder = new OperationBuilder();
				AddOperation(allowOperationBuilder, dennyOperationBuilder, typePermissionObject.ReadState, Operations.Read);
				AddOperation(allowOperationBuilder, dennyOperationBuilder, typePermissionObject.WriteState, Operations.Write);
				AddOperation(allowOperationBuilder, dennyOperationBuilder, typePermissionObject.CreateState, Operations.Create);
				AddOperation(allowOperationBuilder, dennyOperationBuilder, typePermissionObject.DeleteState, Operations.Delete);
				AddOperation(allowOperationBuilder, dennyOperationBuilder, typePermissionObject.NavigateState, Operations.Navigate);
				string allowOperations = allowOperationBuilder.GetOperations();
				if (allowOperations != string.Empty)
				{
					codeLines.Add(GetCodeLine(typePermissionObject, allowOperations, true));
				}
				string dennyOperations = dennyOperationBuilder.GetOperations();
				if (dennyOperations != string.Empty)
				{
					codeLines.Add(GetCodeLine(typePermissionObject, dennyOperations, false));
				}
				foreach (IPermissionPolicyObjectPermissionsObject objectPermissionObject in typePermissionObject.ObjectPermissions)
				{
					codeLines.AddRange(GetCodeLinesFromObjectPermissionObject(objectPermissionObject));
				}
				foreach (IPermissionPolicyMemberPermissionsObject memberPermissionObject in typePermissionObject.MemberPermissions)
				{
					codeLines.AddRange(GetCodeLinesFromMemberPermissionObject(memberPermissionObject));
				}
			}
			return codeLines;
		}
		private List<string> GetCodeLinesFromObjectPermissionObject(IPermissionPolicyObjectPermissionsObject objectPermissionObject)
		{
			List<string> codeLines = new List<string>();
			OperationBuilder allowOperationBuilder = new OperationBuilder();
			OperationBuilder dennyOperationBuilder = new OperationBuilder();
			nameSpacesCodeLines.Add(objectPermissionObject.TypePermissionObject.TargetType.Namespace);
			AddOperation(allowOperationBuilder, dennyOperationBuilder, objectPermissionObject.ReadState, Operations.Read);
			AddOperation(allowOperationBuilder, dennyOperationBuilder, objectPermissionObject.WriteState, Operations.Write);
			AddOperation(allowOperationBuilder, dennyOperationBuilder, objectPermissionObject.DeleteState, Operations.Delete);
			AddOperation(allowOperationBuilder, dennyOperationBuilder, objectPermissionObject.NavigateState, Operations.Navigate);
			string allowOperations = allowOperationBuilder.GetOperations();
			if (allowOperations != string.Empty)
			{
				codeLines.Add(GetCodeLine(objectPermissionObject, allowOperations, true));
			}
			string dennyOperations = dennyOperationBuilder.GetOperations();
			if (dennyOperations != string.Empty)
			{
				codeLines.Add(GetCodeLine(objectPermissionObject, dennyOperations, false));
			}
			return codeLines;
		}
		private List<string> GetCodeLinesFromMemberPermissionObject(IPermissionPolicyMemberPermissionsObject memberPermissionObject)
		{
			List<string> codeLines = new List<string>();
			OperationBuilder allowOperationBuilder = new OperationBuilder();
			OperationBuilder dennyOperationBuilder = new OperationBuilder();
			nameSpacesCodeLines.Add(memberPermissionObject.TypePermissionObject.TargetType.Namespace);
			AddOperation(allowOperationBuilder, dennyOperationBuilder, memberPermissionObject.ReadState, Operations.Read);
			AddOperation(allowOperationBuilder, dennyOperationBuilder, memberPermissionObject.WriteState, Operations.Write);
			string allowOperations = allowOperationBuilder.GetOperations();
			if (allowOperations != string.Empty)
			{
				codeLines.Add(GetCodeLine(memberPermissionObject, allowOperations, true));
			}
			string dennyOperations = dennyOperationBuilder.GetOperations();
			if (dennyOperations != string.Empty)
			{
				codeLines.Add(GetCodeLine(memberPermissionObject, dennyOperations, false));
			}
			return codeLines;
		}
		private void AddOperation(OperationBuilder allowOperationBuilder, OperationBuilder dennyOperationBuilder, SecurityPermissionState? state, Operations operation)
		{
			if (state == SecurityPermissionState.Allow)
			{
				allowOperationBuilder.AddOperation(operation);
			}
			else if (state == SecurityPermissionState.Deny)
			{
				dennyOperationBuilder.AddOperation(operation);
			}
		}
		private string GetCodeLine(IPermissionPolicyNavigationPermissionObject navigationPermissionObject)
		{
			string result = string.Empty;
			if (navigationPermissionObject.ItemPath != null && navigationPermissionObject.NavigateState != null)
			{
				result = $"role.AddNavigationPermission(@\"{navigationPermissionObject.ItemPath}\", SecurityPermissionState.{navigationPermissionObject.NavigateState.ToString()});";
			}
			return result;
		}
		private string GetCodeLine(IPermissionPolicyActionPermissionObject actionPermissionsObject)
		{
			string result = string.Empty;
			if (!string.IsNullOrEmpty(actionPermissionsObject.ActionId))
			{
				result = $"role.AddActionPermission(\"{actionPermissionsObject.ActionId}\");";
			}
			return result;
		}
		private string GetCodeLine(IPermissionPolicyTypePermissionObject typePermissionObject, string operation, bool isGranted)
		{
			string securityPermissionState = GetSecurityPermissionState(isGranted);
			string typeName = typePermissionObject.TargetType.Name;
			return $"role.AddTypePermission<{typeName}>({operation}, SecurityPermissionState.{securityPermissionState});";
		}
		private string GetCodeLine(IPermissionPolicyObjectPermissionsObject objectPermissionObject, string operation, bool isGranted)
		{
			string securityPermissionState = GetSecurityPermissionState(isGranted);
			string typeName = objectPermissionObject.TypePermissionObject.TargetType.Name;
			string criteria = objectPermissionObject.Criteria;
			return $"role.AddObjectPermission<{typeName}>({operation}, " +
				$"\"{criteria}\", SecurityPermissionState.{securityPermissionState});";
		}
		private string GetCodeLine(IPermissionPolicyMemberPermissionsObject memberPermissionObject, string operation, bool isGranted)
		{
			string securityPermissionState = GetSecurityPermissionState(isGranted);
			string typeName = memberPermissionObject.TypePermissionObject.TargetType.Name;
			string criteria = string.IsNullOrEmpty(memberPermissionObject.Criteria) ? "null" : '"' + memberPermissionObject.Criteria + '"';
			string memberName = memberPermissionObject.Members;
			return $"role.AddMemberPermission<{typeName}>({operation}, " +
				$"\"{memberName}\", {criteria}, SecurityPermissionState.{securityPermissionState});";
		}
		string GetSecurityPermissionState(bool isGranted)
		{
			return isGranted ? "Allow" : "Deny";
		}
	}
}
