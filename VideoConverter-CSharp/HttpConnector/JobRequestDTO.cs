using System;
using System.Runtime.Serialization;
using VideoConverter.ApplicationCore;

/// <summary>
/// Data transfer object which are mapped and serialized to JSON.
/// </summary>
namespace VideoConverter.HttpConnector
{
    [DataContract]
    public class JobRequestDTO
    {
        [DataMember]
        public Uri InputFile { get; set; }

        [DataMember]
        public Uri OutputFile { get; set; }

        internal JobRequest ToModel()
        {
            return new JobRequest(InputFile, OutputFile);
        }
    }
}
