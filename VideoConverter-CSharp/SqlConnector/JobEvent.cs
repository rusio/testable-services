using Dapper.Contrib.Extensions;
using VideoConverter.ApplicationCore;
using System;
using System.Diagnostics.Contracts;

namespace VideoConverter.SqlConnector
{
    [Table(TableName)]
    public class JobEvent
    {
        public const string TableName = "job_events";

        [ExplicitKey]
        public string uuid { get; set; }

        public string state { get; set; }
    }
}
