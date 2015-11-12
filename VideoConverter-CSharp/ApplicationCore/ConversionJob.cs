using NLog;
using System;
using System.IO;
using System.Net;

namespace VideoConverter.ApplicationCore
{
    /// <summary>
    /// An actual job that is processing a request from the client.
    /// </summary>
    internal class ConversionJob : IDisposable
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        internal readonly Guid Id = Guid.NewGuid();

        private readonly JobRequest request;
        private readonly IJobObserver responseHandler;
        private readonly VideoCodec codec;
        private readonly WebClient transferrer;

        private readonly FileInfo localInputFile;
        private readonly FileInfo localOutputFile;

        internal ConversionJob(JobRequest request, IJobObserver responseHandler)
        {
            this.request = request;
            this.responseHandler = responseHandler;
            codec = new VideoCodec();
            transferrer = new WebClient();
            localInputFile = new FileInfo(Path.Combine(Path.GetTempPath(), $"{Id.ToString()}.input"));
            localOutputFile = new FileInfo(Path.Combine(Path.GetTempPath(), $"{Id.ToString()}.output"));
        }

        /// <summary>
        /// Note: This method is executed by the IScheduler, within the scheduler's thread.
        /// </summary>
        internal IDisposable Execute()
        {
            logger.Trace(nameof(Execute));
            try
            {
                responseHandler.OnJobStarted(Id);
                DoDownload();
                DoConvert();
                DoUpload();
                responseHandler.OnJobCompleted(Id);
                return this;
            }
            catch (Exception e)
            {
                logger.Error(e);
                responseHandler.OnJobFailed(Id);
                return this;
            }
        }

        internal void DoDownload()
        {
            logger.Trace(nameof(DoDownload));

            transferrer.DownloadFile(request.InputFile, localInputFile.FullName);
        }

        internal void DoConvert()
        {
            logger.Trace(nameof(DoConvert));

            codec.Encode(localInputFile, localOutputFile);
        }

        internal void DoUpload()
        {
            logger.Trace(nameof(DoUpload));

            transferrer.UploadFile(request.OutputFile, localOutputFile.FullName);
        }

        public void Dispose()
        {
            logger.Trace(nameof(Dispose));

            transferrer.Dispose();
        }
    }
}