﻿using NSubstitute;
using NUnit.Framework;
using pdfforge.PDFCreator.Conversion.Jobs;
using pdfforge.PDFCreator.Conversion.Jobs.JobInfo;
using pdfforge.PDFCreator.Conversion.Jobs.Jobs;
using pdfforge.PDFCreator.Conversion.Settings;
using pdfforge.PDFCreator.Core.Workflow;
using pdfforge.PDFCreator.Core.Workflow.Exceptions;
using pdfforge.PDFCreator.Core.Workflow.Output;
using pdfforge.PDFCreator.Core.Workflow.Queries;

namespace pdfforge.PDFCreator.UnitTest.Core.Workflow
{
    [TestFixture]
    public class ConversionWorkflowTest
    {
        [SetUp]
        public void SetUp()
        {
            _jobInfo = new JobInfo
            {
                Metadata = new Metadata
                {
                    Title = "Test"
                }
            };
            _job = new Job(_jobInfo, _profile, new JobTranslations(), new Accounts());
            _job.OutputFiles.Add("X:\\test.pdf");
            _profileChecker = Substitute.For<IProfileChecker>();
            _profileChecker.ProfileCheck(Arg.Any<ConversionProfile>(), Arg.Any<Accounts>()).Returns(_validActionResult);

            _query = Substitute.For<ITargetFileNameComposer>();
            _jobRunner = Substitute.For<IJobRunner>();
            _jobDataUpdater = Substitute.For<IJobDataUpdater>();
            _notificationService = Substitute.For<INotificationService>();

            _workflow = new AutoSaveWorkflow(_jobDataUpdater, _jobRunner, _profileChecker, _query, null, _notificationService);
        }

        private Job _job;
        private JobInfo _jobInfo;
        private readonly ConversionProfile _profile = new ConversionProfile();
        private IProfileChecker _profileChecker;
        private readonly ActionResult _validActionResult = new ActionResult();

        private ConversionWorkflow _workflow;
        private ITargetFileNameComposer _query;
        private IJobRunner _jobRunner;
        private IJobDataUpdater _jobDataUpdater;
        private INotificationService _notificationService;

        private void SetUpConditionsForCompleteWorkflow()
        {
            _profileChecker.ProfileCheck(Arg.Any<ConversionProfile>(), Arg.Any<Accounts>()).Returns(_validActionResult);
        }

        [Test]
        public void DoWorkflow_OnFailedJob_CallsErrorNotifier()
        {
            SetUpConditionsForCompleteWorkflow();
            _jobRunner.When(x => x.RunJob(_job, Arg.Any<IOutputFileMover>())).Do(x => { throw new ProcessingException("", ErrorCode.Conversion_UnknownError); });

            _workflow.RunWorkflow(_job);

            Assert.AreEqual(ErrorCode.Conversion_UnknownError, _workflow.LastError);
        }

        [Test]
        public void DoWorkflow_OnSuccessfulJob_DoesNotCallErrorNotifier()
        {
            SetUpConditionsForCompleteWorkflow();
            _jobRunner.When(x => x.RunJob(_job, Arg.Any<IOutputFileMover>())).Do(x => { _job.Completed = true; });

            _workflow.RunWorkflow(_job);

            Assert.IsTrue(_job.Completed);
            Assert.IsNull(_workflow.LastError);
        }

        [Test]
        public void RunWorkFlow_AbortWorkflowExceptionGetsCatched_WorkflowStepIsAbortedByUser()
        {
            SetUpConditionsForCompleteWorkflow();
            _query.When(x => x.ComposeTargetFileName(Arg.Any<Job>())).Do(x => { throw new AbortWorkflowException("message"); });

            var workflowResult = _workflow.RunWorkflow(_job);
            Assert.AreEqual(WorkflowResult.AbortedByUser, workflowResult);
        }

        [Test]
        public void RunWorkFlow_CheckOrderOfCallsAndActionAssignmentsForCompleteProcess()
        {
            SetUpConditionsForCompleteWorkflow();
            _workflow.RunWorkflow(_job);

            Received.InOrder(() =>
            {
                _jobDataUpdater.UpdateTokensAndMetadata(_job);
                _query.ComposeTargetFileName(_job);
                _profileChecker.ProfileCheck(_job.Profile, _job.Accounts);
                _jobRunner.RunJob(_job, Arg.Any<IOutputFileMover>());
            });
        }

        [Test]
        public void
            RunWorkFlow_QueryTargetFileThrowsManagePrintJobsException_ThrowsManagePrintJobsExceptionAndMetadataGetsReverted
            ()
        {
            var titleToRevert = "My Title to revert!";

            Job job = null;
            _jobDataUpdater
                .When(x => x.UpdateTokensAndMetadata(Arg.Any<Job>()))
                .Do(x =>
                {
                    job = x.Arg<Job>();
                    job.JobInfo.Metadata.Title = titleToRevert;
                });

            _query.When(x => x.ComposeTargetFileName(Arg.Any<Job>())).Do(x => { throw new ManagePrintJobsException(); });
            Assert.Throws<ManagePrintJobsException>(() => _workflow.RunWorkflow(_job), "Did not throw exception");
            Assert.AreNotEqual(titleToRevert, job.JobInfo.Metadata.Title, "Metadata not reverted");
        }

        [Test]
        public void RunWorkFlow_WorkflowExceptionGetsCatched_WorkflowStepIsError()
        {
            SetUpConditionsForCompleteWorkflow();
            _query.When(x => x.ComposeTargetFileName(Arg.Any<Job>())).Do(x => { throw new WorkflowException("message"); });

            var workflowResult = _workflow.RunWorkflow(_job);
            Assert.AreEqual(WorkflowResult.Error, workflowResult);
        }

        [Test]
        public void RunWorkFlow_WithErrorsInProfileCheck_ThrowsProcessingException()
        {
            var errorResult = new ActionResult(ErrorCode.Conversion_UnknownError);
            SetUpConditionsForCompleteWorkflow();
            _profileChecker.ProfileCheck(_profile, new Accounts()).Returns(errorResult);

            var workflowResult = _workflow.RunWorkflow(_job);
            Assert.AreEqual(WorkflowResult.Error, workflowResult);
        }

        [Test]
        public void AutoWorkflow_NotificationWasCalled()
        {
            var workflowResult = _workflow.RunWorkflow(_job);

            _notificationService.Received().ShowInfoNotification(Arg.Any<string>());
            _notificationService.DidNotReceive().ShowErrorNotification(Arg.Any<string>());
        }

        [Test]
        public void AutoWorkflow_DoWorkflowThrowsError_NotificationErrorWasCalled()
        {
            var errorResult = new ActionResult(ErrorCode.Conversion_UnknownError);
            SetUpConditionsForCompleteWorkflow();
            _profileChecker.ProfileCheck(_profile, new Accounts()).Returns(errorResult);

            var workflowResult = _workflow.RunWorkflow(_job);

            _notificationService.DidNotReceive().ShowInfoNotification(Arg.Any<string>());
            _notificationService.Received().ShowErrorNotification(Arg.Any<string>());
        }
    }
}
