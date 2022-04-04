;#DropDB XafSolutionEasyTest

#Application XafSolutionWin
#Application XafSolutionBlazor

*FillForm
 User Name = Admin
*Action Log In

#IfDef XafSolutionWin

*Action About...
 
*Action OK

#EndIf

#IfDef XafSolutionBlazor
*Action Navigation(My Details)
#EndIf
