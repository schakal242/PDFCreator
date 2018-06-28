﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using pdfforge.PDFCreator.Conversion.ActionsInterface;
using pdfforge.PDFCreator.Conversion.Settings;
using pdfforge.PDFCreator.Core.Services;
using pdfforge.PDFCreator.Core.Services.Logging;
using pdfforge.PDFCreator.Core.SettingsManagement;
using pdfforge.PDFCreator.UI.Presentation.Commands.FirstTimeCommands;
using pdfforge.PDFCreator.UI.ViewModels;
using pdfforge.PDFCreator.Utilities;

namespace pdfforge.PDFCreator.UI.Presentation.Settings
{
    public class SettingsManager : ISettingsManager
    {
        private readonly ISettingsLoader _loader;
        private readonly IInstallationPathProvider _installationPathProvider;
        private readonly IVersionHelper _versionHelper;
        private readonly IEnumerable<IFirstTimeCommand> _firstTimeCommands;
        private readonly ICommandLocator _commandLocator;
        private readonly SettingsProvider _settingsProvider;

        public SettingsManager(SettingsProvider settingsProvider, ISettingsLoader loader, IInstallationPathProvider installationPathProvider, IVersionHelper versionHelper, IEnumerable<IFirstTimeCommand> firstTimeCommands, ICommandLocator commandLocator)
        {
            _settingsProvider = settingsProvider;
            _loader = loader;
            _installationPathProvider = installationPathProvider;
            _versionHelper = versionHelper;
            _firstTimeCommands = firstTimeCommands;
            _commandLocator = commandLocator;
        }

        private void LoadAllSettingsSynchronized()
        {
            // Synchronize across processes with a Mutex to avaoid partial reads/writes
            RunSynchronized(DoLoadPdfCreatorSettings);
        }

        private void DoLoadPdfCreatorSettings()
        {
            var settings = _loader.LoadPdfCreatorSettings();
            _settingsProvider.UpdateSettings(settings);

            LoggingHelper.ChangeLogLevel(settings.ApplicationSettings.LoggingLevel);
        }

        public ISettingsProvider GetSettingsProvider()
        {
            return _settingsProvider;
        }

        public void SaveCurrentSettings()
        {
            // Synchronize across processes with a Mutex to avoid partial reads/writes
            RunSynchronized(SaveSettings);
        }

        private void SaveSettings()
        {
            var settings = _settingsProvider.Settings;
            _loader.SaveSettingsInRegistry(settings);
        }

        public void ApplyAndSaveSettings(PdfCreatorSettings settings)
        {
            _settingsProvider.UpdateSettings(settings);
            SaveCurrentSettings();
        }

        public void LoadAllSettings()
        {
            LoadAllSettingsSynchronized();

            var version = _versionHelper.ApplicationVersion.ToString();
            if (version != _settingsProvider.Settings.ApplicationSettings.LastLoginVersion)
            {
                _firstTimeCommands.ToList().ForEach(x => x.Execute(null));
                _settingsProvider.Settings.ApplicationSettings.LastLoginVersion = version;
            }
        }
        
        private void RunSynchronized(Action action)
        {
            var mutexName = "PDFCreator-Settings" + _installationPathProvider.ApplicationGuid;
            var mutex = new Mutex(false, mutexName);

            // we have two try blocks, because we can ignore the AbandonedMutexException
            // The action must be called and in case of an exception, ReleaseMutex must be called
            try
            {
                try
                {
                    mutex.WaitOne();
                }
                catch (AbandonedMutexException)
                {
                }

                action();
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }
    }
}
