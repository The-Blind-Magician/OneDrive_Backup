using System;
using System.IO;
using System.Linq;
using System.Reflection;

[assembly: AssemblyVersion("1.0.0.5")]
[assembly: AssemblyFileVersion("1.0.0.5")]
namespace OneDrive_Backup
{
    class Program
    {        
        static string directoryPaths = AppDomain.CurrentDomain.BaseDirectory + "\\Directories to Copy.txt";

        static UInt64 fileCount = 0;
        static UInt64 modCount = 0;
        static UInt64 unMod = 0;
        static UInt64 created = 0;
        
        static void Main()
        {
            try
            {
                if (!Update.updateExecutable())
                    return;
            }
            catch
            {
                Console.WriteLine("No internet connection. Application will not be updated.");
            }
            string[] directories;
            if (!File.Exists(directoryPaths))
            {
                Console.WriteLine("Directories file does not exist. Creating new file.");
                File.Create(directoryPaths);
            }
            directories = buildDirectories(directoryPaths);

            foreach (string s in directories)
            {
                Console.WriteLine($"Processing {s}");
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
            string[] preProc = File.ReadAllLines(path);
            string[] postProc = preProc.Where(x => x.ToArray().First().Equals('*') == false).ToArray();
            return postProc;
        }  
    }
}
