using System;

namespace VideoConverter.ApplicationCore
{
    public class JobRequest
    {
        public readonly Uri InputFile;
        public readonly Uri OutputFile;

        public JobRequest(Uri inputFile, Uri outputFile)
        {
            InputFile = inputFile;
            OutputFile = outputFile;
        }
    }
}
