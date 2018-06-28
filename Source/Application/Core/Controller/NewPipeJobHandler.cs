﻿using NLog;
using pdfforge.PDFCreator.Conversion.Jobs.JobInfo;
using pdfforge.PDFCreator.Core.Communication;
using pdfforge.PDFCreator.Core.SettingsManagement;
using pdfforge.PDFCreator.Core.Workflow;
using pdfforge.PDFCreator.Utilities.Threading;
using System;
using System.IO;
using System.Threading;

namespace pdfforge.PDFCreator.Core.Controller
{
    public class NewPipeJobHandler : IPipeMessageHandler
    {
        private readonly IFileConversionHandler _fileConversionHandler;
        private readonly IJobInfoManager _jobInfoManager;
        private readonly IThreadManager _threadManager;
        private readonly IJobInfoQueue _jobInfoQueue;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly IMainWindowThreadLauncher _mainWindowThreadLauncher;
        private readonly ISettingsManager _settingsManager;

        public NewPipeJobHandler(IJobInfoQueue jobInfoQueue, ISettingsManager settingsManager, IFileConversionHandler fileConversionHandler, IMainWindowThreadLauncher mainWindowThreadLauncher, IJobInfoManager jobInfoManager, IThreadManager threadManager)
        {
            _jobInfoQueue = jobInfoQueue;
            _settingsManager = settingsManager;
            _fileConversionHandler = fileConversionHandler;
            _mainWindowThreadLauncher = mainWindowThreadLauncher;
            _jobInfoManager = jobInfoManager;
            _threadManager = threadManager;
        }

        public void HandlePipeMessage(string message)
        {
            _logger.Debug("New Message received: " + message);
            if (message.StartsWith("NewJob|", StringComparison.OrdinalIgnoreCase))
            {
                HandleNewJobMessage(message);
            }
            else if (message.StartsWith("DragAndDrop|"))
            {
                HandleDroppedFileMessage(message);
            }
            else if (message.StartsWith("ShowMain|", StringComparison.OrdinalIgnoreCase))
            {
                _mainWindowThreadLauncher.LaunchMainWindow(null);
            }
            else if (message.StartsWith("ReloadSettings|", StringComparison.OrdinalIgnoreCase))
            {
                _logger.Info("Pipe Command: Reloading settings");
                _settingsManager.LoadAllSettings();
            }
        }

        private void HandleDroppedFileMessage(string message)
        {
            var droppedFiles = message.Split('|');

            var threadStart = new ThreadStart(() => _fileConversionHandler.HandleFileList(droppedFiles));

            _threadManager.StartSynchronizedUiThread(threadStart, "PipeDragAndDrop");
        }

        private void HandleNewJobMessage(string message)
        {
            var file = message.Substring(7);
            try
            {
                _logger.Debug("NewJob found: " + file);
                if (!File.Exists(file))
                    return;

                _logger.Trace("The given file exists.");
                var jobInfo = _jobInfoManager.ReadFromInfFile(file);
                _jobInfoQueue.Add(jobInfo);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "There was an Exception while adding the print job: ");
            }
        }
    }
}
