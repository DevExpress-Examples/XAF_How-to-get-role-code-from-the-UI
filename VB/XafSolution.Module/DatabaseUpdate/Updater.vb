Imports System
Imports System.Linq
Imports System.Data
Imports DevExpress.ExpressApp
Imports DevExpress.Data.Filtering
Imports DevExpress.Persistent.Base
Imports DevExpress.ExpressApp.Updating
Imports DevExpress.ExpressApp.Security
Imports DevExpress.ExpressApp.SystemModule
Imports DevExpress.ExpressApp.Security.Strategy
Imports DevExpress.Xpo
Imports DevExpress.ExpressApp.Xpo
Imports DevExpress.Persistent.BaseImpl
Imports DevExpress.Persistent.BaseImpl.PermissionPolicy
Imports XafSolution.Module.BusinessObjects
Imports System.IO

Namespace XafSolution.Module.DatabaseUpdate
    Public Class Updater
        Inherits ModuleUpdater

        Public Sub New(ByVal objectSpace As IObjectSpace, ByVal currentDBVersion As Version)
            MyBase.New(objectSpace, currentDBVersion)
        End Sub
        Public Overrides Sub UpdateDatabaseAfterUpdateSchema()
            MyBase.UpdateDatabaseAfterUpdateSchema()
            CreateDepartments()
            CreateEmployees()
            CreateUser()
            CreateAdmin()
            ObjectSpace.CommitChanges()
        End Sub
        Private Function GetEmployeesDataTable() As DataTable
            Dim shortName As String = "Employees.xml"
            Dim embeddedResourceName As String = Array.Find(Of String)(Me.GetType().Assembly.GetManifestResourceNames(), Function(s) s.Contains(shortName))
            Dim stream As Stream = Me.GetType().Assembly.GetManifestResourceStream(embeddedResourceName)
            If stream Is Nothing Then
                Throw New Exception(String.Format("Cannot read employees data from the {0} file!", shortName))
            End If
            Dim ds As New DataSet()
            ds.ReadXml(stream)
            Return ds.Tables("Employee")
        End Function
        Private Sub CreateEmployees()
            Dim employeesTable As DataTable = GetEmployeesDataTable()
            For Each employeeRow As DataRow In employeesTable.Rows
                Dim email As String = Convert.ToString(employeeRow("EmailAddress"))
                Dim employee As Employee = ObjectSpace.FindObject(Of Employee)(CriteriaOperator.Parse("Email=?", email))
                If employee Is Nothing Then
                    employee = ObjectSpace.CreateObject(Of Employee)()
                    employee.Email = email
                    employee.FirstName = Convert.ToString(employeeRow("FirstName"))
                    employee.LastName = Convert.ToString(employeeRow("LastName"))
                    employee.Birthday = Convert.ToDateTime(employeeRow("BirthDate"))

                    Dim departmentTitle As String = Convert.ToString(employeeRow("GroupName"))
                    Dim department As Department = ObjectSpace.FindObject(Of Department)(CriteriaOperator.Parse("Title=?", departmentTitle), True)
                    If department Is Nothing Then
                        department = ObjectSpace.CreateObject(Of Department)()
                        department.Title = departmentTitle
                        Dim rnd As New Random()
                        department.Office = String.Format("{0}0{0}", rnd.Next(1, 7), rnd.Next(9))
                    End If
                    employee.Department = department
                End If
            Next employeeRow
        End Sub
        Private Sub CreateDepartments()
            Dim devDepartment As Department = ObjectSpace.FindObject(Of Department)(CriteriaOperator.Parse("Title == 'Development Department'"))
            If devDepartment Is Nothing Then
                devDepartment = ObjectSpace.CreateObject(Of Department)()
                devDepartment.Title = "Development Department"
                devDepartment.Office = "205"
            End If
            Dim seoDepartment As Department = ObjectSpace.FindObject(Of Department)(CriteriaOperator.Parse("Title == 'SEO'"))
            If seoDepartment Is Nothing Then
                seoDepartment = ObjectSpace.CreateObject(Of Department)()
                seoDepartment.Title = "SEO"
                seoDepartment.Office = "703"
            End If
        End Sub
        Private Sub CreateUser()
            Dim sampleUser As PermissionPolicyUser = ObjectSpace.FindObject(Of PermissionPolicyUser)(New BinaryOperator("UserName", "User"))
            If sampleUser Is Nothing Then
                sampleUser = ObjectSpace.CreateObject(Of PermissionPolicyUser)()
                sampleUser.UserName = "User"
                sampleUser.SetPassword("")
            End If
            Dim defaultRole As PermissionPolicyRole = CreateDefaultRole()
            sampleUser.Roles.Add(defaultRole)
        End Sub
        Private Sub CreateAdmin()
            Dim userAdmin As PermissionPolicyUser = ObjectSpace.FindObject(Of PermissionPolicyUser)(New BinaryOperator("UserName", "Admin"))
            If userAdmin Is Nothing Then
                userAdmin = ObjectSpace.CreateObject(Of PermissionPolicyUser)()
                userAdmin.UserName = "Admin"
                userAdmin.SetPassword("")
            End If
            Dim adminRole As PermissionPolicyRole = CreateAdminRole()
            userAdmin.Roles.Add(adminRole)
        End Sub
        Public Overrides Sub UpdateDatabaseBeforeUpdateSchema()
            MyBase.UpdateDatabaseBeforeUpdateSchema()
        End Sub
        Private Function CreateAdminRole() As PermissionPolicyRole
            Dim adminRole As PermissionPolicyRole = ObjectSpace.FindObject(Of PermissionPolicyRole)(New BinaryOperator("Name", "Administrators"))
            If adminRole Is Nothing Then
                adminRole = ObjectSpace.CreateObject(Of PermissionPolicyRole)()
                adminRole.Name = "Administrators"
            End If
            adminRole.IsAdministrative = True
            Return adminRole
        End Function
        Private Function CreateDefaultRole() As PermissionPolicyRole
            Dim defaultRole As PermissionPolicyRole = ObjectSpace.FindObject(Of PermissionPolicyRole)(New BinaryOperator("Name", "Default"))
            If defaultRole Is Nothing Then
                defaultRole = ObjectSpace.CreateObject(Of PermissionPolicyRole)()
                defaultRole.Name = "Default"

                defaultRole.AddObjectPermission(Of PermissionPolicyUser)(SecurityOperations.Read, "[Oid] = CurrentUserId()", SecurityPermissionState.Allow)
                defaultRole.AddNavigationPermission("Application/NavigationItems/Items/Default/Items/MyDetails", SecurityPermissionState.Allow)
                defaultRole.AddNavigationPermission("Application/NavigationItems/Items/Default/Items/Department_ListView", SecurityPermissionState.Allow)
                defaultRole.AddNavigationPermission("Application/NavigationItems/Items/Default/Items/Employee_ListView", SecurityPermissionState.Allow)
                defaultRole.AddMemberPermission(Of PermissionPolicyUser)(SecurityOperations.Write, "ChangePasswordOnFirstLogon", "[Oid] = CurrentUserId()", SecurityPermissionState.Allow)
                defaultRole.AddMemberPermission(Of PermissionPolicyUser)(SecurityOperations.Write, "StoredPassword", "[Oid] = CurrentUserId()", SecurityPermissionState.Allow)
                defaultRole.AddTypePermissionsRecursively(Of PermissionPolicyRole)(SecurityOperations.Read, SecurityPermissionState.Deny)
                defaultRole.AddTypePermissionsRecursively(Of ModelDifference)(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow)
                defaultRole.AddTypePermissionsRecursively(Of ModelDifferenceAspect)(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow)
                defaultRole.AddTypePermissionsRecursively(Of ModelDifference)(SecurityOperations.Create, SecurityPermissionState.Allow)
                defaultRole.AddTypePermissionsRecursively(Of ModelDifferenceAspect)(SecurityOperations.Create, SecurityPermissionState.Allow)
                defaultRole.AddTypePermissionsRecursively(Of Department)(SecurityOperations.Read, SecurityPermissionState.Deny)
                defaultRole.AddObjectPermission(Of Department)(SecurityOperations.Read, "Contains([Title], 'Development')", SecurityPermissionState.Allow)
                defaultRole.AddTypePermissionsRecursively(Of Employee)(SecurityOperations.Read, SecurityPermissionState.Allow)
                defaultRole.AddTypePermissionsRecursively(Of Employee)(SecurityOperations.Write, SecurityPermissionState.Allow)
                defaultRole.AddObjectPermission(Of Employee)(SecurityOperations.Delete, "Contains([Department.Title], 'Development')", SecurityPermissionState.Allow)
                defaultRole.AddMemberPermission(Of Employee)(SecurityOperations.Write, "LastName", "Not Contains([Department.Title], 'Development')", SecurityPermissionState.Deny)
                defaultRole.AddActionPermission("RoleGeneratorAction")
            End If
            Return defaultRole
        End Function
    End Class
End Namespace
