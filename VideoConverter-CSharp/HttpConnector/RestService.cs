using VideoConverter.ApplicationCore;
using NLog;
using System;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace VideoConverter.HttpConnector
{
    /// <summary>
    /// This class defines REST URIs, which are automatically mapped to 
    /// methods and called by the runtime. In other words, these methods 
    /// are entry-points into the application, when accessed over HTTP. 
    /// </summary>
    [ServiceContract]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class RestService
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly IJobProcessor processor;

        public RestService(IJobProcessor processor)
        {
            this.processor = processor;
        }

        [OperationContract]
        [WebGet(UriTemplate = "/processor", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public string GET_RetrieveProcessorState()
        {
            logger.Trace(nameof(GET_RetrieveProcessorState));
            try
            {
                return processor.State;
            }
            catch (Exception e)
            {
                logger.Error(e);
                throw new FaultException(e.Message);
            }
        }


        [OperationContract]
        [WebInvoke(UriTemplate = "/jobs", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        public Guid POST_SubmitConversionJob(JobRequestDTO request)
        {
            logger.Trace(nameof(POST_SubmitConversionJob));
            try
            {
                return processor.Process(request.ToModel());
            }
            catch (Exception e)
            {
                logger.Error(e);
                throw new FaultException(e.Message);
            }
        }
    }
}
