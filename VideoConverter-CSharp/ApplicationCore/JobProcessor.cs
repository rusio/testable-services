using NLog;
using System;
using System.Reactive.Concurrency;

namespace VideoConverter.ApplicationCore
{
    /// <summary>
    /// An Active Object which can schedule and manage job processing.
    /// 
    /// NOTE: Jobs are processed asynchroneously, so clients are not blocked.
    /// NOTE: Jobs are processed single-threaded and in order of submission.
    /// </summary>
    public class JobProcessor : IJobProcessor
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly IProcessorObserver processorObserver;
        private readonly IJobObserver jobObserver;
        private readonly EventLoopScheduler jobScheduler;

        internal ProcessorState CurrentState { get; set; }
        public string State => CurrentState.ToString();

        public JobProcessor(IProcessorObserver processorObserver, IJobObserver jobObserver)
        {
            this.processorObserver = processorObserver;
            this.jobObserver = jobObserver;
            jobScheduler = new EventLoopScheduler();
            CurrentState = new Created();
        }

        #region "Invocations from the client of IRequestHanler"

        public void Start()
        {
            logger.Trace(nameof(Start));

            CurrentState.Start(this);
        }

        public void Stop()
        {
            logger.Trace(nameof(Stop));

            CurrentState.Stop(this);
        }

        public Guid Process(JobRequest request)
        {
            logger.Trace(nameof(Process));

            return CurrentState.Process(this, request);
        }

        public void Dispose()
        {
            logger.Trace(nameof(Dispose));

            CurrentState.Dispose(this);
        }

        #endregion
        #region "Invocations from from the current ProcessorState"

        internal void Initiate()
        {
            logger.Trace(nameof(Initiate));

            processorObserver.OnProcessorStarted();
        }

        internal void Terminate()
        {
            logger.Trace(nameof(Terminate));

            jobScheduler.Dispose();
            processorObserver.OnProcessorStopped();
        }

        internal Guid Schedule(JobRequest request)
        {
            logger.Trace(nameof(Schedule));

            var job = new ConversionJob(request, jobObserver);
            jobScheduler.Schedule(() => job.Execute());
            return job.Id;
        }

        #endregion
    }
}
