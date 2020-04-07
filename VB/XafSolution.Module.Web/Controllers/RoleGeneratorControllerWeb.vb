Imports DevExpress.ExpressApp.Web.SystemModule
Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks
Imports System.Web
Imports System.Web.Configuration
Imports XafSolution.Module.Controllers

Namespace XafSolution.Module.Web.Controllers
    Public Class RoleGeneratorControllerWeb
        Inherits RoleGeneratorController

        Protected Overrides Sub OnActivated()
            MyBase.OnActivated()
            roleGeneratorAction.Model.SetValue("IsPostBackRequired", True)
        End Sub
        Private Function GetStreamFromString(ByVal targetString As String) As MemoryStream
            Dim stream As New MemoryStream()
            Dim writer As New StreamWriter(stream)
            writer.Write(targetString)
            writer.Flush()
            stream.Position = 0
            Return stream
        End Function
        Protected Overrides Sub SaveFile(ByVal updaterCode As String)
            Using stream As MemoryStream = GetStreamFromString(updaterCode)
                HttpContext.Current.Response.ClearHeaders()
                ResponseWriter.WriteFileToResponse(stream, "RoleUpdater.vb")
            End Using
        End Sub
        Protected Overrides Function IsEnableRoleGeneratorAction() As Boolean
            Dim enableRoleGeneratorActionString As String = WebConfigurationManager.AppSettings("EnableRoleGeneratorAction")
            Return If(enableRoleGeneratorActionString Is Nothing, False, Boolean.Parse(enableRoleGeneratorActionString))
        End Function
    End Class
End Namespace
