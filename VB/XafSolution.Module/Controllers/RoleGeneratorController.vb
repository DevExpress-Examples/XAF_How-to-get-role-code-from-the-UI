Imports DevExpress.ExpressApp
Imports DevExpress.ExpressApp.Actions
Imports DevExpress.ExpressApp.Security
Imports DevExpress.Persistent.Base
Imports RoleGeneratorSpace
Imports System
Imports System.ComponentModel
Imports System.Configuration
Imports System.Linq

Namespace XafSolution.Module.Controllers
    Public MustInherit Class RoleGeneratorController
        Inherits ViewController(Of ListView)

        Private roleType As Type
        Private components As IContainer = Nothing
        Protected WithEvents roleGeneratorAction As SimpleAction
        Public Sub New()
            InitializeComponent()
            TargetObjectType = GetType(IPermissionPolicyRole)
        End Sub
        Private Sub InitializeComponent()
            components = New Container()
            roleGeneratorAction = New SimpleAction(components)
            roleGeneratorAction.Caption = "Generate Role"
            roleGeneratorAction.Category = "Tools"
            roleGeneratorAction.ConfirmationMessage = Nothing
            roleGeneratorAction.Id = "RoleGeneratorAction"
            roleGeneratorAction.ImageName = "Action_Export"
            roleGeneratorAction.ToolTip = Nothing
            Actions.Add(roleGeneratorAction)
        End Sub
        Protected Overrides Sub OnActivated()
            MyBase.OnActivated()
            Dim security As SecurityStrategy = Application.GetSecurityStrategy()
            roleType = CType(Application.Security, SecurityStrategyComplex).RoleType
            roleGeneratorAction.Active("ActionExecuted") = IsEnableRoleGeneratorAction() AndAlso security.CanRead(roleType)
        End Sub
        Protected Sub RoleGeneratorAction_Execute(ByVal sender As Object, ByVal e As SimpleActionExecuteEventArgs) Handles roleGeneratorAction.Execute
            Dim roleGenerator As New RoleGenerator(roleType)
            Dim roleList As IEnumerable(Of IPermissionPolicyRole) = e.SelectedObjects.OfType(Of IPermissionPolicyRole)()
            Dim updaterCode As String = roleGenerator.GetUpdaterCode(roleList)
            SaveFile(updaterCode)
        End Sub
        Protected MustOverride Sub SaveFile(ByVal updaterCode As String)
        Private Function IsEnableRoleGeneratorAction() As Boolean
            Dim enableRoleGeneratorActionString As String = ConfigurationManager.AppSettings("EnableRoleGeneratorAction")
            Return If(enableRoleGeneratorActionString Is Nothing, False, Boolean.Parse(enableRoleGeneratorActionString))
        End Function
    End Class
End Namespace
