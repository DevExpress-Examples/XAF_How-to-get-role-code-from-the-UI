Imports DevExpress.ExpressApp.Web.SystemModule
Imports System
Imports System.IO
Imports System.Linq
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
    End Class
End Namespace
