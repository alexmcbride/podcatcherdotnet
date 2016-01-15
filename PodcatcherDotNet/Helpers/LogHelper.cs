using System;
using System.IO;

namespace PodcatcherDotNet.Helpers {
    public static class LogHelper {
        private static string GetLogPath() {
            return Path.Combine(Environment.CurrentDirectory, "Log.txt");
        }

        public static void WriteLine(string message, params object[] args) {
            using (var file = File.AppendText(GetLogPath())) {
                file.WriteLine("BEGIN LOG ENTRY {0}", DateTime.Now);
                file.WriteLine(message, args);
                file.WriteLine("END");
            }
        }
    }
}
