using System;
using System.ServiceModel;
using NLog;
using System.ServiceModel.Web;
using VideoConverter.ApplicationCore;

namespace VideoConverter.HttpConnector
{
    /// <summary>
    /// An embedded (self-hosted) HTTP server for hosting the REST service.
    /// </summary>
    public class HttpServer : IProcessorObserver
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly int listenPort;
        private ServiceHost serviceHost;
        private RestService restService;

        public HttpServer(int listenPort)
        {
            this.listenPort = listenPort;
        }
        
        public void Initialize(IJobProcessor processor)
        {
            this.restService = new RestService(processor);
            var baseUrl = string.Format("http://localhost:{0}/", listenPort);
            this.serviceHost = new WebServiceHost(restService, new Uri(baseUrl));
        }

        public void OnProcessorStarted()
        {
            logger.Trace(nameof(OnProcessorStarted));

            serviceHost.Open();
        }

        public void OnProcessorStopped()
        {
            logger.Trace(nameof(OnProcessorStopped));

            serviceHost.Close();
        }
    }
}
