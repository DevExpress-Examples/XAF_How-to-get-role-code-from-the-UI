Imports DevExpress.Persistent.Base
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text

Namespace RoleGeneratorSpace
    Public Class CustomizeCodeLinesEventArg
        Public ReadOnly Property Role As IPermissionPolicyRole
        Public Property CustomCodeLines As List(Of String)

        Public Sub New(ByVal role As IPermissionPolicyRole, ByVal customCodeLines As List(Of String))
            Me.Role = role
            Me.CustomCodeLines = customCodeLines
        End Sub
    End Class
End Namespace
