using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.Threading;

namespace WifiReconnect
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                StreamReader stream = new StreamReader("settings.json");
                string settingsString = stream.ReadToEnd();
                Settings settings = JsonConvert.DeserializeObject<Settings>(settingsString);
                stream.Dispose();

                StreamWriter logger = File.AppendText($"logs-{DateTime.Now:yyyy-MM-dd}.txt");
                string host = settings.host;

                if (!Ping(host, logger))
                {
                    logger.Log($"Trying to connect to: {settings.ssid}");
                    string cmd = $"wlan connect name=\"{settings.ssid}\" ssid=\"{settings.ssid}\"";
                    Process proc = new Process();
                    proc.StartInfo.FileName = "netsh.exe";
                    proc.StartInfo.Arguments = cmd;
                    proc.StartInfo.UseShellExecute = false;
                    proc.StartInfo.RedirectStandardOutput = true;
                    proc.StartInfo.CreateNoWindow = true;
                    proc.Start();
                    logger.Log(proc.StandardOutput.ReadToEnd());
                    proc.Dispose();
                }

                logger.Log("Connected, sleeping...");
                logger.Dispose();

                Thread.Sleep(TimeSpan.FromSeconds(30));
            }
        }

        private static bool Ping(string host, StreamWriter logger)
        {
            bool pinged = false;
            Ping ping = new Ping();
            try
            {
                PingReply reply = ping.Send(host);
                pinged = reply.Status == IPStatus.Success;
            }
            catch (Exception e)
            {
                logger.Log($"An exception has occured while pinging: {e.Message}");                               
            }
            return pinged;
        }
    }

    public static class Extensions
    {
        public static void Log(this StreamWriter stream, string line)
        {
            stream.WriteLine($"Date: {DateTime.Now.ToString()} | {line}");
        }
    }

    public class Settings
    {
        public string host { get; set; }
        public string ssid { get; set; }
    }
}
