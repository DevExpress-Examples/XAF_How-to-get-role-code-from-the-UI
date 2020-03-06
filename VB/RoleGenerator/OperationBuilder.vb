Imports DevExpress.ExpressApp.Security
Imports System
Imports System.Collections.Generic

Namespace RoleGeneratorSpace
    <Flags>
    Public Enum Operations
        None = 0
        Read = 1
        Write = 2
        Create = 4
        Delete = 8
        Navigate = 16
        FullObjectAccess = Read Or Write Or Delete Or Navigate
        Full = FullObjectAccess Or Create
        [ReadOnly] = Read Or Navigate
        CRUD = Create Or Read Or Write Or Delete
        ReadWrite = Read Or Write
    End Enum

    Public Class OperationBuilder
        Private operationMask As Operations = Operations.None
        Private operationString As String = String.Empty
        Private operationsDictionary As Dictionary(Of Operations, String)

        Public Sub New()
            operationsDictionary = New Dictionary(Of Operations, String) From {
                { Operations.Read, nameof(SecurityOperations.Read) },
                { Operations.Write, nameof(SecurityOperations.Write) },
                { Operations.Create, nameof(SecurityOperations.Create) },
                { Operations.Delete, nameof(SecurityOperations.Delete) },
                { Operations.Navigate, nameof(SecurityOperations.Navigate) },
                { Operations.FullObjectAccess, nameof(SecurityOperations.FullObjectAccess) },
                { Operations.Full, nameof(SecurityOperations.FullAccess) },
                { Operations.ReadOnly, nameof(SecurityOperations.ReadOnlyAccess) },
                { Operations.CRUD, nameof(SecurityOperations.CRUDAccess) },
                { Operations.ReadWrite, nameof(SecurityOperations.ReadWriteAccess) }
            }
        End Sub
        Public Sub AddOperation(ByVal operation As Operations)
            operationMask = operationMask Or operation
            operationString &= SecurityOperations.Delimiter & System.Enum.GetName(GetType(Operations), operation)
        End Sub
        Public Function GetOperations() As String
            Dim result As String = String.Empty
            If operationString <> String.Empty Then
                If operationsDictionary.TryGetValue(operationMask, result) Then
                    result = GetType(SecurityOperations).Name & "."c & result
                Else
                    result = """"c & operationString.Substring(1) & """"c
                End If
            End If
            Return result
        End Function
    End Class
End Namespace
