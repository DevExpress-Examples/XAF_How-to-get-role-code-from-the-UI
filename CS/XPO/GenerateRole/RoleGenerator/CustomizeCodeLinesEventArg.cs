using DevExpress.Persistent.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoleGeneratorSpace
{
	public class CustomizeCodeLinesEventArg
	{
		public IPermissionPolicyRole Role { get; }
		public List<string> CustomCodeLines { get; set; }
		public CustomizeCodeLinesEventArg(IPermissionPolicyRole role, List<string> customCodeLines)
		{
			Role = role;
			CustomCodeLines = customCodeLines;
		}
	}
}
