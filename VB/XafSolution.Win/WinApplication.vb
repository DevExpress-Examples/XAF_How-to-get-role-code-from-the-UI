Imports Microsoft.VisualBasic
Imports System
Imports System.ComponentModel
Imports DevExpress.ExpressApp
Imports DevExpress.ExpressApp.Win
Imports System.Collections.Generic
Imports DevExpress.ExpressApp.Updating
Imports DevExpress.ExpressApp.Win.Utils
Imports DevExpress.ExpressApp.Xpo
Imports DevExpress.ExpressApp.Security
Imports DevExpress.ExpressApp.Security.ClientServer
Imports DevExpress.Persistent.BaseImpl

' For more typical usage scenarios, be sure to check out https://docs.devexpress.com/eXpressAppFramework/DevExpress.ExpressApp.Win.WinApplication._members
Partial Public Class XafSolutionWindowsFormsApplication
    Inherits WinApplication
#Region "Default XAF configuration options (https://www.devexpress.com/kb=T501418)"
    Shared Sub New()
        DevExpress.Persistent.Base.PasswordCryptographer.EnableRfc2898 = True
        DevExpress.Persistent.Base.PasswordCryptographer.SupportLegacySha512 = False
        DevExpress.ExpressApp.Utils.ImageLoader.Instance.UseSvgImages = True
        SecurityStrategy.EnableSecurityForActions = True
    End Sub
    Private Sub InitializeDefaults()
        LinkNewObjectToParentImmediately = False
        OptimizedControllersCreation = True
        UseLightStyle = True
        SplashScreen = New DXSplashScreen(GetType(XafSplashScreen), New DefaultOverlayFormOptions())
        ExecuteStartupLogicBeforeClosingLogonWindow = True
    End Sub
#End Region
    Protected Overrides Sub CustomizeTypesInfo()
        MyBase.CustomizeTypesInfo()
        TypesInfo.RegisterEntity(GetType(ModelDifference))
    End Sub
    Public Sub New()
        InitializeComponent()
        InitializeDefaults()
    End Sub

    Protected Overrides Sub CreateDefaultObjectSpaceProvider(ByVal args As CreateCustomObjectSpaceProviderEventArgs)
        args.ObjectSpaceProviders.Add(New SecuredObjectSpaceProvider(DirectCast(Security, SecurityStrategyComplex), XPObjectSpaceProvider.GetDataStoreProvider(args.ConnectionString, args.Connection, True), False))
        args.ObjectSpaceProviders.Add(New NonPersistentObjectSpaceProvider(TypesInfo, Nothing))
    End Sub
    Private Sub XafSolutionWindowsFormsApplication_DatabaseVersionMismatch(ByVal sender As Object, ByVal e As DevExpress.ExpressApp.DatabaseVersionMismatchEventArgs) Handles MyBase.DatabaseVersionMismatch
#If EASYTEST Then
        e.Updater.Update()
        e.Handled = True
#Else
        If System.Diagnostics.Debugger.IsAttached Then
            e.Updater.Update()
            e.Handled = True
        Else
            Dim message As String = "The application cannot connect to the specified database, " &
                "because the database doesn't exist, its version is older " &
                "than that of the application or its schema does not match " &
                "the ORM data model structure. To avoid this error, use one " &
                "of the solutions from the https://www.devexpress.com/kb=T367835 KB Article."

            If e.CompatibilityError IsNot Nothing AndAlso e.CompatibilityError.Exception IsNot Nothing Then
                message &= Constants.vbCrLf & Constants.vbCrLf & "Inner exception: " & e.CompatibilityError.Exception.Message
            End If
            Throw New InvalidOperationException(message)
        End If
#End If
    End Sub
    Private Shared Sub XafSolutionWindowsFormsApplication_CustomizeLanguagesList(ByVal sender As Object, ByVal e As CustomizeLanguagesListEventArgs) Handles MyBase.CustomizeLanguagesList
        Dim userLanguageName As String = System.Threading.Thread.CurrentThread.CurrentUICulture.Name
        If userLanguageName <> "en-US" And e.Languages.IndexOf(userLanguageName) = -1 Then
            e.Languages.Add(userLanguageName)
        End If
    End Sub
End Class
