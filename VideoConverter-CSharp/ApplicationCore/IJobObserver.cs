using System;

namespace VideoConverter.ApplicationCore
{
    public interface IJobObserver
    {
        void OnJobStarted(Guid jobId);

        void OnJobCompleted(Guid jobId);

        void OnJobFailed(Guid jobId);
    }
}
