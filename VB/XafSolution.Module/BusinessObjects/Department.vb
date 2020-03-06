Imports DevExpress.Persistent.Base
Imports DevExpress.Persistent.BaseImpl
Imports DevExpress.Xpo
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks

Namespace XafSolution.Module.BusinessObjects
    <DefaultClassOptions, System.ComponentModel.DefaultProperty("Title")>
    Public Class Department
        Inherits BaseObject


        Private title_Renamed As String

        Private office_Renamed As String
        Public Sub New(ByVal session As Session)
            MyBase.New(session)
        End Sub
        Public Property Title() As String
            Get
                Return title_Renamed
            End Get
            Set(ByVal value As String)
                SetPropertyValue(nameof(Title), title_Renamed, value)
            End Set
        End Property
        Public Property Office() As String
            Get
                Return office_Renamed
            End Get
            Set(ByVal value As String)
                SetPropertyValue(nameof(Office), office_Renamed, value)
            End Set
        End Property
        <Association("Department-Employees")>
        Public ReadOnly Property Employees() As XPCollection(Of Employee)
            Get
                Return GetCollection(Of Employee)(nameof(Employees))
            End Get
        End Property
    End Class
End Namespace
