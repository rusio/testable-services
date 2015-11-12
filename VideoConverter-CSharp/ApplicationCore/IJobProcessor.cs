using System;

namespace VideoConverter.ApplicationCore
{
    /// <summary>
    /// A handler for all commands that are accepted in the ApplicationCore.
    /// </summary>
    public interface IJobProcessor
    {
        string State { get; }
        
        Guid Process(JobRequest request);
    }
}
