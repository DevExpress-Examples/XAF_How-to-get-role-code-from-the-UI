namespace XafSolution.Module.Web {
    partial class XafSolutionAspNetModule {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if(disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            // 
            // XafSolutionAspNetModule
            // 
            this.RequiredModuleTypes.Add(typeof(XafSolution.Module.XafSolutionModule));
            this.RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.Web.SystemModule.SystemAspNetModule));
			this.RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.Validation.Web.ValidationAspNetModule));
        }

        #endregion
    }
}