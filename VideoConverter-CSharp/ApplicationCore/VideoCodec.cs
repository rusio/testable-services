using NLog;
using System;
using System.IO;

namespace VideoConverter.ApplicationCore
{
    internal class VideoCodec
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        internal void Encode(FileInfo inputFile, FileInfo outputFile)
        {
            logger.Trace(nameof(Encode));

            // simulate video encoding
            inputFile.CopyTo(outputFile.FullName);
        }
    }
}
