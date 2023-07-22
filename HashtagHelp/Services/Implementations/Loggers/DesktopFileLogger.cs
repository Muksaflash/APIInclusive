using HashtagHelp.Services.Interfaces;

namespace HashtagHelp.Services.Implementations.Loggers
{
    public class DesktopFileLogger : IProcessLogger
    {
        private readonly string logFilePath;
        private readonly string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        
        public void Log(string message)
        {
            var logFilePath = Path.Combine(desktopPath,"log.txt");
            string logMessage = $"{DateTime.Now} - {message}{Environment.NewLine}";
            File.AppendAllText(logFilePath, logMessage);
        }
    }
}