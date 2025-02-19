using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ServiceFile
{
    public static class LogService
    {
        private static readonly List<string> _logs = new();

        public static void AddLog(string message)
        {
            var logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
            _logs.Add(logMessage);

            // برای جلوگیری از مصرف بیش از حد حافظه، فقط 100 لاگ آخر را نگه می‌داریم
            if (_logs.Count > 100)
            {
                _logs.RemoveAt(0);
            }
        }

        public static List<string> GetLogs()
        {
            return _logs;
        }
    }

}
