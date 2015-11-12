using System;
using System.Runtime.CompilerServices;

namespace VideoConverter.ApplicationCore
{
    internal abstract class ProcessorState
    {
        internal virtual void Start(JobProcessor processor)
        {
            throw new InvalidOperationException(IllegalCommandInStateMessage());
        }

        internal virtual void Stop(JobProcessor processor)
        {
            throw new InvalidOperationException(IllegalCommandInStateMessage());
        }

        internal virtual Guid Process(JobProcessor processor, JobRequest request)
        {
            throw new InvalidOperationException(IllegalCommandInStateMessage());
        }

        internal virtual void Dispose(JobProcessor processor)
        {
            throw new InvalidOperationException(IllegalCommandInStateMessage());
        }

        protected string IllegalCommandInStateMessage([CallerMemberName] string callerMethod = "")
        {
            return $"'{callerMethod}' is not a valid command in state '{GetType().Name}'";
        }
    }

    /// <summary>
    /// The job processor has been created, but not started yet.
    /// </summary>
    internal class CreatedProcessor : ProcessorState
    {
        internal override void Start(JobProcessor processor)
        {
            processor.CurrentState = new IdleProcessor();
            processor.Initiate();
        }

        internal override void Dispose(JobProcessor processor)
        {
            processor.CurrentState = new DisposedProcessor();
            processor.Terminate();
        }

        public override string ToString() { return "CREATED"; }
    }

    /// <summary>
    /// The job processor is waiting for any jobs to process.
    /// </summary>
    internal class IdleProcessor : ProcessorState
    {
        internal override void Stop(JobProcessor processor)
        {
            processor.CurrentState = new DisposedProcessor();
            processor.Terminate();
        }

        internal override void Dispose(JobProcessor processor)
        {
            processor.CurrentState = new DisposedProcessor();
            processor.Terminate();
        }

        internal override Guid Process(JobProcessor processor, JobRequest request)
        {
            return processor.Schedule(request);
        }

        public override string ToString() { return "IDLE"; }
    }

    /// <summary>
    /// The job processor is currently processing a job.
    /// </summary>
    internal class BusyProcessor : ProcessorState
    {
        private readonly ConversionJob job;

        internal BusyProcessor(ConversionJob job)
        {
            this.job = job;
        }

        internal override void Stop(JobProcessor processor)
        {
            processor.CurrentState = new DisposedProcessor();
            processor.Terminate();
        }

        internal override void Dispose(JobProcessor processor)
        {
            processor.CurrentState = new DisposedProcessor();
            processor.Terminate();
        }

        internal override Guid Process(JobProcessor processor, JobRequest request)
        {
            return processor.Schedule(request);
        }

        public override string ToString() { return "BUSY"; }
    }

    /// <summary>
    /// The job processor has been disposed and any resources have been released.
    /// </summary>
    internal class DisposedProcessor : ProcessorState
    {
        internal override void Start(JobProcessor processor)
        {
            throw new ObjectDisposedException(IllegalCommandInStateMessage());
        }

        internal override void Stop(JobProcessor processor)
        {
            throw new ObjectDisposedException(IllegalCommandInStateMessage());
        }

        internal override void Dispose(JobProcessor processor)
        {
            // We are already disposed and must handle subsequent calls to Dispose()
            // in an idempotent manner, thus - do nothing instead of throwing.
        }

        internal override Guid Process(JobProcessor processor, JobRequest request)
        {
            throw new ObjectDisposedException(IllegalCommandInStateMessage());
        }

        public override string ToString() { return "DISPOSED"; }
    }
}