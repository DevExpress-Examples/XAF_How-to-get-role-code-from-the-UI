Imports DevExpress.ExpressApp.Security
Imports DevExpress.Persistent.Base
Imports System
Imports System.Collections.Generic

Namespace RoleGeneratorSpace
    Public Class RoleGenerator
        Private roleType As Type
        Private codeLines As New Dictionary(Of String, List(Of String))()
        Private nameSpacesCodeLines As New HashSet(Of String)()
        Public Event CustomizeCodeLines As EventHandler(Of CustomizeCodeLinesEventArg)
        Public Sub New(ByVal roleType As Type)
            Me.roleType = roleType
            nameSpacesCodeLines.Add(GetType(PermissionPolicy).Namespace)
            nameSpacesCodeLines.Add(roleType.Namespace)
        End Sub
        Public Function GetUpdaterCode(ByVal roleList As IEnumerable(Of IPermissionPolicyRole)) As String
            If roleList Is Nothing Then
                Return String.Empty
            End If
            Dim template As New UpdaterRuntimeTemplate()
            For Each role As IPermissionPolicyRole In roleList
                codeLines.Add(role.Name, GetCodeLinesFromRole(role))
            Next role
            template.Session = New Dictionary(Of String, Object)()
            template.Session("CodeLines") = codeLines
            template.Session("NameSpacesCodeLines") = nameSpacesCodeLines
            template.Session("RoleTypeName") = roleType.Name
            template.Initialize()
            Return template.TransformText()
        End Function
        Private Function GetCodeLinesFromRole(ByVal role As IPermissionPolicyRole) As List(Of String)
            Dim codeLines As New List(Of String)()
            If role IsNot Nothing Then
                codeLines.Add($"role.Name = ""{role.Name}""")
                codeLines.Add($"role.PermissionPolicy = SecurityPermissionPolicy.{role.PermissionPolicy.ToString()}")
                If role.IsAdministrative Then
                    codeLines.Add($"role.IsAdministrative = true")
                End If
                If role.CanEditModel Then
                    codeLines.Add($"role.CanEditModel = true")
                End If
                If CustomizeCodeLinesEvent IsNot Nothing Then
                    Dim customCodeLines As New List(Of String)()
                    RaiseEvent CustomizeCodeLines(Me, New CustomizeCodeLinesEventArg(role, customCodeLines))
                    codeLines.AddRange(customCodeLines)
                End If
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
        Private Function GetCodeLinesFromTypePermissionObject(ByVal typePermissionObject As IPermissionPolicyTypePermissionObject) As List(Of String)
            Dim codeLines As New List(Of String)()
            Dim targetType As Type = typePermissionObject.TargetType
            If targetType IsNot Nothing Then
                nameSpacesCodeLines.Add(targetType.Namespace)
                Dim allowOperationBuilder As New OperationBuilder()
                Dim dennyOperationBuilder As New OperationBuilder()
                AddOperation(allowOperationBuilder, dennyOperationBuilder, typePermissionObject.ReadState, Operations.Read)
                AddOperation(allowOperationBuilder, dennyOperationBuilder, typePermissionObject.WriteState, Operations.Write)
                AddOperation(allowOperationBuilder, dennyOperationBuilder, typePermissionObject.CreateState, Operations.Create)
                AddOperation(allowOperationBuilder, dennyOperationBuilder, typePermissionObject.DeleteState, Operations.Delete)
                AddOperation(allowOperationBuilder, dennyOperationBuilder, typePermissionObject.NavigateState, Operations.Navigate)
                Dim allowOperations As String = allowOperationBuilder.GetOperations()
                If allowOperations <> String.Empty Then
                    codeLines.Add(GetCodeLine(typePermissionObject, allowOperations, True))
                End If
                Dim dennyOperations As String = dennyOperationBuilder.GetOperations()
                If dennyOperations <> String.Empty Then
                    codeLines.Add(GetCodeLine(typePermissionObject, dennyOperations, False))
                End If
                For Each objectPermissionObject As IPermissionPolicyObjectPermissionsObject In typePermissionObject.ObjectPermissions
                    codeLines.AddRange(GetCodeLinesFromObjectPermissionObject(objectPermissionObject))
                Next objectPermissionObject
                For Each memberPermissionObject As IPermissionPolicyMemberPermissionsObject In typePermissionObject.MemberPermissions
                    codeLines.AddRange(GetCodeLinesFromMemberPermissionObject(memberPermissionObject))
                Next memberPermissionObject
            End If
            Return codeLines
        End Function
        Private Function GetCodeLinesFromObjectPermissionObject(ByVal objectPermissionObject As IPermissionPolicyObjectPermissionsObject) As List(Of String)
            Dim codeLines As New List(Of String)()
            Dim allowOperationBuilder As New OperationBuilder()
            Dim dennyOperationBuilder As New OperationBuilder()
            nameSpacesCodeLines.Add(objectPermissionObject.TypePermissionObject.TargetType.Namespace)
            AddOperation(allowOperationBuilder, dennyOperationBuilder, objectPermissionObject.ReadState, Operations.Read)
            AddOperation(allowOperationBuilder, dennyOperationBuilder, objectPermissionObject.WriteState, Operations.Write)
            AddOperation(allowOperationBuilder, dennyOperationBuilder, objectPermissionObject.DeleteState, Operations.Delete)
            AddOperation(allowOperationBuilder, dennyOperationBuilder, objectPermissionObject.NavigateState, Operations.Navigate)
            Dim allowOperations As String = allowOperationBuilder.GetOperations()
            If allowOperations <> String.Empty Then
                codeLines.Add(GetCodeLine(objectPermissionObject, allowOperations, True))
            End If
            Dim dennyOperations As String = dennyOperationBuilder.GetOperations()
            If dennyOperations <> String.Empty Then
                codeLines.Add(GetCodeLine(objectPermissionObject, dennyOperations, False))
            End If
            Return codeLines
        End Function
        Private Function GetCodeLinesFromMemberPermissionObject(ByVal memberPermissionObject As IPermissionPolicyMemberPermissionsObject) As List(Of String)
            Dim codeLines As New List(Of String)()
            Dim allowOperationBuilder As New OperationBuilder()
            Dim dennyOperationBuilder As New OperationBuilder()
            nameSpacesCodeLines.Add(memberPermissionObject.TypePermissionObject.TargetType.Namespace)
            AddOperation(allowOperationBuilder, dennyOperationBuilder, memberPermissionObject.ReadState, Operations.Read)
            AddOperation(allowOperationBuilder, dennyOperationBuilder, memberPermissionObject.WriteState, Operations.Write)
            Dim allowOperations As String = allowOperationBuilder.GetOperations()
            If allowOperations <> String.Empty Then
                codeLines.Add(GetCodeLine(memberPermissionObject, allowOperations, True))
            End If
            Dim dennyOperations As String = dennyOperationBuilder.GetOperations()
            If dennyOperations <> String.Empty Then
                codeLines.Add(GetCodeLine(memberPermissionObject, dennyOperations, False))
            End If
            Return codeLines
        End Function
        Private Sub AddOperation(ByVal allowOperationBuilder As OperationBuilder, ByVal dennyOperationBuilder As OperationBuilder, ByVal state? As SecurityPermissionState, ByVal operation As Operations)
            If state = SecurityPermissionState.Allow Then
                allowOperationBuilder.AddOperation(operation)
            ElseIf state = SecurityPermissionState.Deny Then
                dennyOperationBuilder.AddOperation(operation)
            End If
        End Sub
        Private Function GetCodeLine(ByVal navigationPermissionObject As IPermissionPolicyNavigationPermissionObject) As String
            Dim result As String = String.Empty
            If navigationPermissionObject.ItemPath IsNot Nothing AndAlso navigationPermissionObject.NavigateState IsNot Nothing Then
                result = $"role.AddNavigationPermission(""{navigationPermissionObject.ItemPath}"", SecurityPermissionState.{navigationPermissionObject.NavigateState.ToString()})"
            End If
            Return result
        End Function
        Private Function GetCodeLine(ByVal typePermissionObject As IPermissionPolicyTypePermissionObject, ByVal operation As String, ByVal isGranted As Boolean) As String
            Dim securityPermissionState As String = GetSecurityPermissionState(isGranted)
            Dim typeName As String = typePermissionObject.TargetType.Name
            Return $"role.AddTypePermission(Of {typeName})({operation}, SecurityPermissionState.{securityPermissionState})"
        End Function
        Private Function GetCodeLine(ByVal objectPermissionObject As IPermissionPolicyObjectPermissionsObject, ByVal operation As String, ByVal isGranted As Boolean) As String
            Dim securityPermissionState As String = GetSecurityPermissionState(isGranted)
            Dim typeName As String = objectPermissionObject.TypePermissionObject.TargetType.Name
            Dim criteria As String = objectPermissionObject.Criteria
            Return $"role.AddObjectPermission(Of {typeName})({operation}, " & $"""{criteria}"", SecurityPermissionState.{securityPermissionState})"
        End Function
        Private Function GetCodeLine(ByVal memberPermissionObject As IPermissionPolicyMemberPermissionsObject, ByVal operation As String, ByVal isGranted As Boolean) As String
            Dim securityPermissionState As String = GetSecurityPermissionState(isGranted)
            Dim typeName As String = memberPermissionObject.TypePermissionObject.TargetType.Name
            Dim criteria As String = If(String.IsNullOrEmpty(memberPermissionObject.Criteria), "Nothing", """"c & memberPermissionObject.Criteria & """"c)
            Dim memberName As String = memberPermissionObject.Members
            Return $"role.AddMemberPermission(Of {typeName})({operation}, " & $"""{memberName}"", {criteria}, SecurityPermissionState.{securityPermissionState})"
        End Function
        Private Function GetSecurityPermissionState(ByVal isGranted As Boolean) As String
            Return If(isGranted, "Allow", "Deny")
        End Function
    End Class
End Namespace
