using System;
using System.Collections.Generic;
using System.Deployment.Application;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Diagnostics;

[assembly: AssemblyVersion("1.0.0.1")]
[assembly: AssemblyFileVersion("1.0.0.1")]
namespace OneDrive_Backup
{
    class Program
    {
        
        static string directoryPaths = AppDomain.CurrentDomain.BaseDirectory + "\\Directories to Copy.txt";
        static string version = AppDomain.CurrentDomain.BaseDirectory + "\\Version.txt";
        static string exe0Path = AppDomain.CurrentDomain.BaseDirectory + "\\OneDrive_Backup.exe";
        static string exe1Path = AppDomain.CurrentDomain.BaseDirectory + "\\OneDrive_Backup1.exe";
        static string exe2Path = AppDomain.CurrentDomain.BaseDirectory + "\\OneDrive_Backup2.exe";
        static string exeName = Path.GetFileName(Assembly.GetEntryAssembly().Location);

        static UInt64 fileCount = 0;
        static UInt64 modCount = 0;
        static UInt64 unMod = 0;
        static UInt64 created = 0;


        static void Main()
        {
            
            if (!updateExecutable())
                return;
            string[] directories;
            if (!File.Exists(directoryPaths))
            {
                Console.WriteLine("Directories file does not exist. Creating new file.");
                File.Create(directoryPaths);
            }
            directories = buildDirectories(directoryPaths);

            foreach (string s in directories)
            {
                DirectoryCopy(s, AppDomain.CurrentDomain.BaseDirectory + "\\" + s.Substring(s.LastIndexOf('\\')), true);
            }
            Console.WriteLine("\n\nTransfer Complete\n");
            Console.WriteLine($"Files Processed: {fileCount}");
            Console.WriteLine($"       Modified: {modCount}");
            Console.WriteLine($"    Un-Modified: {unMod}");
            Console.WriteLine($"        Created: {created}");
            Console.Read();
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            Console.WriteLine($"\nCopying Directory: {sourceDirName}");
            foreach (FileInfo file in files)
            {
                Console.Write($"\tCopying {file.Name} ..... ");
                string temppath = Path.Combine(destDirName, file.Name);
                FileInfo destFile = new FileInfo(temppath);
                if (destFile.Exists)
                {
                    if (file.LastWriteTime > destFile.LastWriteTime)
                    {
                        file.CopyTo(temppath, true);
                        Console.Write("Overwritten\n");
                        modCount++;
                    }
                    else
                    {
                        Console.Write("Not Modified\n");
                        unMod++;
                    }
                }
                else
                {
                    file.CopyTo(temppath, true);
                    Console.Write("Created\n");
                    created++;
                }
                fileCount++;
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        private static string[] buildDirectories(string path)
        {
            return (File.ReadAllLines(path));
        }

        private static bool updateExecutable()
        {
            string txtVer;
            using (var ctxt = new WebClient())
            {
                Uri uri = new Uri("https://github.com/The-Blind-Magician/OneDrive_Backup/raw/master/Version.txt");
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
                            Uri uri = new Uri("https://github.com/The-Blind-Magician/OneDrive_Backup/raw/master/bin/Debug/OneDrive_Backup.exe");
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
            catch(Exception e)
            {
                Console.WriteLine(e);
                Console.Read();
                return false;
            }
            return false;
        }
    }
}
