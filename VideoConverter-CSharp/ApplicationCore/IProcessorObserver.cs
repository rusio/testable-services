namespace VideoConverter.ApplicationCore
{
    public interface IProcessorObserver
    {
        /// <summary>
        /// Signals that the processor has been started and ready to accept work.
        /// </summary>
        void OnProcessorStarted();

        /// <summary>
        /// Signals that the processor has been stopped and no longer usable.
        /// </summary>
        void OnProcessorStopped();
    }
}
