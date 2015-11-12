using VideoConverter.ApplicationCore;
using NLog;
using System;
using System.Data;
using Dapper.Contrib.Extensions;

namespace VideoConverter.SqlConnector
{
    public class EventLog : IJobObserver, IDisposable
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly IDbConnection connection;

        public EventLog(IDbConnection connection)
        {
            this.connection = connection;
        }

        public void OnJobStarted(Guid jobId)
        {
            logger.Trace(nameof(OnJobStarted));

            EnsureConnected();
            connection.Insert(new JobEvent { uuid = jobId.ToString(), state = "STARTED" });
        }

        public void OnJobCompleted(Guid jobId)
        {
            logger.Trace(nameof(OnJobCompleted));

            EnsureConnected();
            connection.Insert(new JobEvent { uuid = jobId.ToString(), state = "COMPLETED" });
        }

        public void OnJobFailed(Guid jobId)
        {
            logger.Trace(nameof(OnJobFailed));

            EnsureConnected();
            connection.Insert(new JobEvent { uuid = jobId.ToString(), state = "FAILED" });
        }

        private void EnsureConnected()
        {
            logger.Trace(nameof(EnsureConnected));

            connection.Open();
        }

        public void Dispose()
        {
            connection.Dispose();
        }
    }
}
