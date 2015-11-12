using VideoConverter.ApplicationCore;
using VideoConverter.HttpConnector;
using VideoConverter.SqlConnector;
using NLog;
using Npgsql;
using System;
using System.Configuration;
using System.ServiceProcess;
using System.Data;

namespace VideoConverter.WindowsService
{
    /// <summary>
    /// This is the main entry-point in the application, when invoked by 
    /// the operating system as a Windows Service.
    /// </summary>
    public partial class Bootstrap : ServiceBase
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private const string DefaultConnectionName = "PostgreSQL";

        private JobProcessor processor;

        public Bootstrap()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            logger.Trace(nameof(OnStart));
            try
            {   
                processor = CreateProcessor();
                processor.Start();
            }
            catch (Exception e)
            {
                logger.Error(e);
                ExitCode = 1;
            }
        }

        private JobProcessor CreateProcessor()
        {
            var httpServer = new HttpServer(Config.Default.HttpPort);
            var dataReporter = new EventLog(CreateConnection());
            processor = new JobProcessor(httpServer, dataReporter);
            httpServer.Initialize(processor);
            return processor;
        }

        public static IDbConnection CreateConnection()
        {
            var connectionString = ConfigurationManager.ConnectionStrings[DefaultConnectionName].ConnectionString;
            return new NpgsqlConnection(connectionString);
        }

        protected override void OnStop()
        {
            logger.Trace(nameof(OnStop));
            try
            { 
                processor?.Stop();
            }
            catch (Exception e)
            {
                logger.Error(e);
                ExitCode = 2;
            }
        }
    }
}
