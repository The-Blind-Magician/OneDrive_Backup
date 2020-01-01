using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Net;
using System.Diagnostics;

namespace OneDrive_Backup
{
    public class Update
    {
        static string version = AppDomain.CurrentDomain.BaseDirectory + "\\Version.txt";
        static string exe0Path = AppDomain.CurrentDomain.BaseDirectory + "\\OneDrive_Backup.exe";
        static string exe1Path = AppDomain.CurrentDomain.BaseDirectory + "\\OneDrive_Backup1.exe";
        static string exe2Path = AppDomain.CurrentDomain.BaseDirectory + "\\OneDrive_Backup2.exe";
        static string exeName = Path.GetFileName(Assembly.GetEntryAssembly().Location);

        public static bool updateExecutable()
        {
            string txtVer;
            using (var ctxt = new WebClient())
            {
                Uri uri = new Uri("https://github.com/The-Blind-Magician/OneDrive_Backup/raw/no-Jekyll/Version.txt");
                txtVer = ctxt.DownloadString(uri);
                System.Threading.Thread.Sleep(500);
            }
            if ((exeName != "OneDrive_Backup2.exe") &&
                Assembly.GetExecutingAssembly().GetName().Version.ToString().Trim() == txtVer.Trim())
            {
                if (File.Exists(exe2Path))
                {
                    File.SetAttributes(exe2Path, FileAttributes.Normal);
                    File.Delete(exe2Path);
                }
                if (File.Exists(exe1Path))
                {
                    File.SetAttributes(exe1Path, FileAttributes.Normal);
                    File.Delete(exe1Path);
                }
                File.Delete(version);
                return true;
            }
            try
            {
                if (exeName == "OneDrive_Backup.exe")
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    using (var client = new WebClient())
                    {
                        if (!File.Exists(exe1Path))
                        {
                            Uri uri = new Uri("https://github.com/The-Blind-Magician/OneDrive_Backup/raw/no-Jekyll/bin/Debug/OneDrive_Backup.exe");
                            client.DownloadFile(uri, "OneDrive_Backup1.exe");
                        }
                        File.SetAttributes(exe1Path, FileAttributes.Normal);
                        File.Copy(exe1Path, exe2Path, true);
                        File.SetAttributes(exe2Path, FileAttributes.Normal);

                        File.SetAttributes(exe2Path, FileAttributes.Normal);
                        ProcessStartInfo info = new ProcessStartInfo(exe2Path);
                        info.UseShellExecute = true;
                        info.Verb = "runas";
                        Process.Start(info);

                        return false;
                    }
                }
                else if (exeName == "OneDrive_Backup2.exe")
                {
                    System.Threading.Thread.Sleep(1000);
                    File.SetAttributes(exe0Path, FileAttributes.Normal);
                    File.Delete(exe0Path);

                    File.Move(exe1Path, exe0Path);
                    File.SetAttributes(exe0Path, FileAttributes.Normal);

                    Process.Start(exe0Path);
                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.Read();
                return false;
            }
            return false;
        }
    }
}
