﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.ExtensionManager;
using Microsoft.VisualStudio.Shell;

namespace SolutionExtensions
{
    internal sealed class ShowSuggestionsCommand
    {
        private readonly Package _package;
        private IVsExtensionRepository _repository;
        private IVsExtensionManager _manager;

        private ShowSuggestionsCommand(Package package, IVsExtensionRepository repository, IVsExtensionManager manager)
        {
            _package = package;
            _repository = repository;
            _manager = manager;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(PackageGuids.guidExtensionCmdSet, PackageIds.cmdShowSuggestions);
                var button = new OleMenuCommand(async (s, e) => await ShowSuggestions(s, e), menuCommandID);
                commandService.AddCommand(button);
            }
        }
        
        public static ShowSuggestionsCommand Instance { get; private set; }

        private IServiceProvider ServiceProvider
        {
            get { return _package; }
        }

        public static void Initialize(Package package, IVsExtensionRepository repository, IVsExtensionManager manager)
        {
            Instance = new ShowSuggestionsCommand(package, repository, manager);
        }
        
        private async System.Threading.Tasks.Task ShowSuggestions(object sender, EventArgs e)
        {
            var dte = ServiceProvider.GetService(typeof(DTE)) as DTE2;

            SuggestionResult result;

            if (dte.ActiveDocument != null && !string.IsNullOrEmpty(dte.ActiveDocument.FullName))
            {
                string fileName = Path.GetFileName(dte.ActiveDocument.FullName);
                IEnumerable<string> fileTypes;
                result = SuggestionHandler.Instance.GetSuggestions(fileName, out fileTypes);
            }
            else
            {
                result = new SuggestionResult
                {
                    Extensions = SuggestionHandler.Instance.GetCurrentFileModel().Extensions[SuggestionFileModel.GENERAL],
                    Matches = new string[0]
                };
            }

            if (result != null)
            {
                InstallerDialog dialog = new InstallerDialog(result.Extensions);
                dialog.NeverShowAgainForSolution = Settings.IsFileTypeIgnored(result.Matches);
                var test = dialog.ShowDialog();

                Settings.IgnoreFileType(result.Matches, dialog.NeverShowAgainForSolution);

                if (!test.HasValue || !test.Value)
                    return;

                ExtensionInstaller installer = new ExtensionInstaller(_repository, _manager);
                await installer.InstallExtensions(dialog.SelectedExtensions);
            }
        }
    }
}

