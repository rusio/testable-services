using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using VideoConverter.ApplicationCore;
using VideoConverter.HttpConnector;

namespace Lindenbaum.Presentation.Converter.IntegrationTests.HttpConnector
{
    class HttpConnectorIT
    {
        // central class in the satellite component (real component under test)
        private HttpServer httpServer;

        // central class in the core component (mocked collaborator component)
        private Mock<IJobProcessor> jobProcessor;

        // test infrastructure to excercise the sattelite component via HTTP
        private HttpClient httpClient;

        [SetUp]
        public void SetUpComponentUnderTest()
        {
            httpServer = new HttpServer(5050);
            jobProcessor = new Mock<IJobProcessor>();
            httpServer.Initialize(jobProcessor.Object);
            httpServer.OnProcessorStarted();
        }

        [SetUp]
        public void SetUpTestInfrastructure()
        {
            httpClient = new HttpClient()
            {
                BaseAddress = new Uri(string.Format("http://localhost:{0}/", 5050)),
                Timeout = TimeSpan.FromSeconds(2),
            };
        }

        [TearDown]
        public void TearDownComponentUnderTest()
        {
            httpServer?.OnProcessorStopped();
        }

        [TearDown]
        public void TearDownTestInfrastructure()
        {
            httpClient?.Dispose();
        }

        [Test]
        public void GivenStartedServer_WhenRequestArrives_ThenServerResponds()
        {
            // act
            var response = httpClient.GetAsync("/Invalid-URL").Result;
            
            // assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public void GivenIdleProcessor_WhenStatusIsQueried_ThenStatusIsIdle()
        {
            // arrange
            jobProcessor.SetupGet(obj => obj.State).Returns("IDLE");

            // act
            var response = httpClient.GetAsync("/processor").Result;

            // assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var json = response.Content.ReadAsStringAsync().Result;
            var processorState = JsonConvert.DeserializeObject<string>(json);
            Assert.That(processorState, Is.EqualTo("IDLE"));
        }

        [Test]
        public void GivenIdleProcessor_WhenJobRequestIsSubmitted_ThenJobIdIsReturned()
        {
            // arrange
            var expectedJobId = Guid.NewGuid();
            jobProcessor.Setup(obj => obj.Process(It.IsAny<JobRequest>())).Returns(expectedJobId);

            // act
            var requestJson = JsonConvert.SerializeObject(new JobRequestDTO
            {
                InputFile = new Uri(Path.GetTempFileName()),
                OutputFile = new Uri(Path.GetTempFileName()),
            });
            var stringContent = new StringContent(requestJson, Encoding.UTF8, "application/json");
            var response = httpClient.PostAsync("/jobs", stringContent).Result;

            // assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var responseJson = response.Content.ReadAsStringAsync().Result;
            var actualJobId = JsonConvert.DeserializeObject<Guid>(responseJson);
            Assert.That(actualJobId, Is.EqualTo(expectedJobId));
        }
    }
}
