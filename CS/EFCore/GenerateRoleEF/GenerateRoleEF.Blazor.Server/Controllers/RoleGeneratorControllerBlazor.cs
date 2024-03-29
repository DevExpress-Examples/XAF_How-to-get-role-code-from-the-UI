﻿using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Blazor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using XafSolution.Module.Controllers;

namespace XafSolution.Module.Blazor.Controllers {
    public class RoleGeneratorControllerBlazor : RoleGeneratorController {
        protected override void SaveFile(string updaterCode) {
            //Place your save logic here
            try {
                var folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var path = Path.Combine(folder, "RoleUpdater.cs");
                File.WriteAllText(path, updaterCode);
            }
            catch {
                throw new UserFriendlyException("An error occurred while saving the file. Please check the directory you are saving to.");
            }
        }
        protected override bool IsEnableRoleGeneratorAction() {
            IConfiguration configuration = ((BlazorApplication)Application).ServiceProvider.GetRequiredService<IConfiguration>();
            string enableRoleGeneratorActionString = configuration.GetSection("EnableRoleGeneratorAction").Value;
            return enableRoleGeneratorActionString == null ? false : bool.Parse(enableRoleGeneratorActionString);
        }
    }
}
