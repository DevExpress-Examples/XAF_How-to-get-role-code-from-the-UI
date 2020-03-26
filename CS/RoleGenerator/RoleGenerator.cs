using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.Base;
using System;
using System.Collections.Generic;

namespace RoleGeneratorSpace {
	public class RoleGenerator {
		private Type roleType;
		private Dictionary<string, List<string>> codeLines = new Dictionary<string, List<string>>();
		private HashSet<string> nameSpacesCodeLines = new HashSet<string>();
		public event EventHandler<CustomizeCodeLinesEventArg> CustomizeCodeLines;
		public RoleGenerator(Type roleType) {
			this.roleType = roleType;
			nameSpacesCodeLines.Add(typeof(PermissionPolicy).Namespace);
			nameSpacesCodeLines.Add(roleType.Namespace);
		}
		public string GetUpdaterCode(IEnumerable<IPermissionPolicyRole> roleList) {
			if(roleList == null) {
				return string.Empty;
			}
			UpdaterRuntimeTemplate template = new UpdaterRuntimeTemplate();
			foreach(IPermissionPolicyRole role in roleList) {
				codeLines.Add(role.Name, GetCodeLinesFromRole(role));
			}
			template.Session = new Dictionary<string, object>();
			template.Session["CodeLines"] = codeLines;
			template.Session["NameSpacesCodeLines"] = nameSpacesCodeLines;
			template.Session["RoleTypeName"] = roleType.Name;
			template.Initialize();
			return template.TransformText();
		}
		private List<string> GetCodeLinesFromRole(IPermissionPolicyRole role) {
			List<string> codeLines = new List<string>();
			if(role != null) {
				codeLines.Add($"role.Name = \"{role.Name}\";");
				codeLines.Add($"role.PermissionPolicy = SecurityPermissionPolicy.{role.PermissionPolicy.ToString()};");
				if(role.IsAdministrative) {
					codeLines.Add($"role.IsAdministrative = true;");
				}
				if(role.CanEditModel) {
					codeLines.Add($"role.CanEditModel = true;");
				}
				if(CustomizeCodeLines != null) {
					List<string> customCodeLines = new List<string>();
					CustomizeCodeLines(this, new CustomizeCodeLinesEventArg(role, customCodeLines));
					codeLines.AddRange(customCodeLines);
				}
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
		private List<string> GetCodeLinesFromTypePermissionObject(IPermissionPolicyTypePermissionObject typePermissionObject) {
			List<string> codeLines = new List<string>();
			Type targetType = typePermissionObject.TargetType;
			if(targetType != null) {
				nameSpacesCodeLines.Add(targetType.Namespace);
				OperationBuilder allowOperationBuilder = new OperationBuilder();
				OperationBuilder dennyOperationBuilder = new OperationBuilder();
				AddOperation(allowOperationBuilder, dennyOperationBuilder, typePermissionObject.ReadState, Operations.Read);
				AddOperation(allowOperationBuilder, dennyOperationBuilder, typePermissionObject.WriteState, Operations.Write);
				AddOperation(allowOperationBuilder, dennyOperationBuilder, typePermissionObject.CreateState, Operations.Create);
				AddOperation(allowOperationBuilder, dennyOperationBuilder, typePermissionObject.DeleteState, Operations.Delete);
				AddOperation(allowOperationBuilder, dennyOperationBuilder, typePermissionObject.NavigateState, Operations.Navigate);
				string allowOperations = allowOperationBuilder.GetOperations();
				if(allowOperations != string.Empty) {
					codeLines.Add(GetCodeLine(typePermissionObject, allowOperations, true));
				}
				string dennyOperations = dennyOperationBuilder.GetOperations();
				if(dennyOperations != string.Empty) {
					codeLines.Add(GetCodeLine(typePermissionObject, dennyOperations, false));
				}
				foreach(IPermissionPolicyObjectPermissionsObject objectPermissionObject in typePermissionObject.ObjectPermissions) {
					codeLines.AddRange(GetCodeLinesFromObjectPermissionObject(objectPermissionObject));
				}
				foreach(IPermissionPolicyMemberPermissionsObject memberPermissionObject in typePermissionObject.MemberPermissions) {
					codeLines.AddRange(GetCodeLinesFromMemberPermissionObject(memberPermissionObject));
				}
			}
			return codeLines;
		}
		private List<string> GetCodeLinesFromObjectPermissionObject(IPermissionPolicyObjectPermissionsObject objectPermissionObject) {
			List<string> codeLines = new List<string>();
			OperationBuilder allowOperationBuilder = new OperationBuilder();
			OperationBuilder dennyOperationBuilder = new OperationBuilder();
			nameSpacesCodeLines.Add(objectPermissionObject.TypePermissionObject.TargetType.Namespace);
			AddOperation(allowOperationBuilder, dennyOperationBuilder, objectPermissionObject.ReadState, Operations.Read);
			AddOperation(allowOperationBuilder, dennyOperationBuilder, objectPermissionObject.WriteState, Operations.Write);
			AddOperation(allowOperationBuilder, dennyOperationBuilder, objectPermissionObject.DeleteState, Operations.Delete);
			AddOperation(allowOperationBuilder, dennyOperationBuilder, objectPermissionObject.NavigateState, Operations.Navigate);
			string allowOperations = allowOperationBuilder.GetOperations();
			if(allowOperations != string.Empty) {
				codeLines.Add(GetCodeLine(objectPermissionObject, allowOperations, true));
			}
			string dennyOperations = dennyOperationBuilder.GetOperations();
			if(dennyOperations != string.Empty) {
				codeLines.Add(GetCodeLine(objectPermissionObject, dennyOperations, false));
			}
			return codeLines;
		}
		private List<string> GetCodeLinesFromMemberPermissionObject(IPermissionPolicyMemberPermissionsObject memberPermissionObject) {
			List<string> codeLines = new List<string>();
			OperationBuilder allowOperationBuilder = new OperationBuilder();
			OperationBuilder dennyOperationBuilder = new OperationBuilder();
			nameSpacesCodeLines.Add(memberPermissionObject.TypePermissionObject.TargetType.Namespace);
			AddOperation(allowOperationBuilder, dennyOperationBuilder, memberPermissionObject.ReadState, Operations.Read);
			AddOperation(allowOperationBuilder, dennyOperationBuilder, memberPermissionObject.WriteState, Operations.Write);
			string allowOperations = allowOperationBuilder.GetOperations();
			if(allowOperations != string.Empty) {
				codeLines.Add(GetCodeLine(memberPermissionObject, allowOperations, true));
			}
			string dennyOperations = dennyOperationBuilder.GetOperations();
			if(dennyOperations != string.Empty) {
				codeLines.Add(GetCodeLine(memberPermissionObject, dennyOperations, false));
			}
			return codeLines;
		}
		private void AddOperation(OperationBuilder allowOperationBuilder, OperationBuilder dennyOperationBuilder, SecurityPermissionState? state, Operations operation) {
			if(state == SecurityPermissionState.Allow) {
				allowOperationBuilder.AddOperation(operation);
			}
			else if(state == SecurityPermissionState.Deny) {
				dennyOperationBuilder.AddOperation(operation);
			}
		}
		private string GetCodeLine(IPermissionPolicyNavigationPermissionObject navigationPermissionObject) {
			string result = string.Empty;
			if(navigationPermissionObject.ItemPath != null && navigationPermissionObject.NavigateState != null) {
				result = $"role.AddNavigationPermission(@\"{navigationPermissionObject.ItemPath}\", SecurityPermissionState.{navigationPermissionObject.NavigateState.ToString()});";
			}
			return result;
		}
		private string GetCodeLine(IPermissionPolicyTypePermissionObject typePermissionObject, string operation, bool isGranted) {
			string securityPermissionState = GetSecurityPermissionState(isGranted);
			string typeName = typePermissionObject.TargetType.Name;
			return $"role.AddTypePermission<{typeName}>({operation}, SecurityPermissionState.{securityPermissionState});";
		}
		private string GetCodeLine(IPermissionPolicyObjectPermissionsObject objectPermissionObject, string operation, bool isGranted) {
			string securityPermissionState = GetSecurityPermissionState(isGranted);
			string typeName = objectPermissionObject.TypePermissionObject.TargetType.Name;
			string criteria = objectPermissionObject.Criteria;
			return $"role.AddObjectPermission<{typeName}>({operation}, " +
				$"\"{criteria}\", SecurityPermissionState.{securityPermissionState});";
		}
		private string GetCodeLine(IPermissionPolicyMemberPermissionsObject memberPermissionObject, string operation, bool isGranted) {
			string securityPermissionState = GetSecurityPermissionState(isGranted);
			string typeName = memberPermissionObject.TypePermissionObject.TargetType.Name;
			string criteria = string.IsNullOrEmpty(memberPermissionObject.Criteria) ? "null" : '"' + memberPermissionObject.Criteria + '"';
			string memberName = memberPermissionObject.Members;
			return $"role.AddMemberPermission<{typeName}>({operation}, " +
				$"\"{memberName}\", {criteria}, SecurityPermissionState.{securityPermissionState});";
		}
		string GetSecurityPermissionState(bool isGranted) {
			return isGranted ? "Allow" : "Deny";
		}
	}
}
