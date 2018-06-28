﻿using pdfforge.PDFCreator.Conversion.Jobs.JobInfo;
using pdfforge.PDFCreator.UI.Presentation.DesignTime.Helper;
using pdfforge.PDFCreator.UI.Presentation.Windows;
using pdfforge.PDFCreator.UI.Presentation.Wrapper;
using DesignTimeDragAndDropHandler = pdfforge.PDFCreator.UI.DesignTimeViewModels.Helper.DesignTimeDragAndDropHandler;

namespace pdfforge.PDFCreator.UI.Presentation.DesignTime
{
    public class DesignTimeManagePrintJobsVm : ManagePrintJobsViewModel
    {
        public DesignTimeManagePrintJobsVm() : base(new DesignTimeJobInfoQueue(), new DesignTimeDragAndDropHandler(), new JobInfoManager(null), new DispatcherWrapper(), new DesignTimeTranslationUpdater(), new DesignTimeApplicationNameProvider())
        {
        }
    }
}
