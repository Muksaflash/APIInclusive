using HashtagHelp.Services.Interfaces;

namespace HashtagHelp.Services.Implementations.Loggers
{
    public class DesktopFileLogger : IProcessLogger
    {
        private readonly string logFilePath;

        public DesktopFileLogger()
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            logFilePath = Path.Combine(desktopPath,"log.txt");
        }

        public void Log(string message)
        {
            string logMessage = $"{DateTime.Now} - {message}{Environment.NewLine}";
            File.AppendAllText(logFilePath, logMessage);
        }
    }
}