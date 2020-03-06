Imports DevExpress.Persistent.Base
Imports DevExpress.Persistent.BaseImpl
Imports DevExpress.Xpo
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks

Namespace XafSolution.Module.BusinessObjects
    <DefaultClassOptions>
    Public Class Employee
        Inherits Person

        Public Sub New(ByVal session As Session)
            MyBase.New(session)
        End Sub

        Private department_Renamed As Department
        <Association("Department-Employees"), ImmediatePostData>
        Public Property Department() As Department
            Get
                Return department_Renamed
            End Get
            Set(ByVal value As Department)
                SetPropertyValue("Department", department_Renamed, value)
            End Set
        End Property
    End Class
End Namespace
