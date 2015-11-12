using System.ServiceProcess;

namespace VideoConverter.WindowsService
{
    static class Program
    {
        static void Main()
        {
            ServiceBase.Run(new Bootstrap());
        }
    }
}
