using System;
using System.IO;
using System.Linq;

namespace HandyFile
{
    public class PathInfo
    {
        public DirectoryInfo Directory { get; }
        public FileInfo File { get; }
        public DriveInfo Drive { get; }
        public int Depth { get; }
        public int StartDepth { get; }
        public bool IsFile { get; }
        public bool IsDirectory { get; }
        public bool IsDrive { get; }

        public bool IsEmpty
        {
            get
            {
                try
                {
                    return !System.IO.Directory.EnumerateFileSystemEntries(FullName).Any();
                }
                catch (UnauthorizedAccessException)
                {
                    return false;
                }
            }
        }


        public string FullName => IsFile ? File.FullName : IsDirectory ? Directory.FullName : Drive.ToString();
        public string Name => IsFile ? File.Name : IsDirectory ? Directory.Name : Drive.Name;
        public string NameForPrint => Depth < 0 ? ".." : 
            new string(' ', Depth * 2) + 
            (IsFile ? "" + Name : IsDirectory ? (IsEmpty || Depth == StartDepth ? "► " : "▼ ") + Name : Name);
        
        public PathInfo(DirectoryInfo directory, int depth, int startDepth)
        {
            (Depth, StartDepth, Directory, IsDirectory) = (depth, startDepth, directory, true);
        }
        
        public PathInfo(FileInfo file, int depth, int startDepth)
        {
            (Depth, StartDepth, File, IsFile) = (depth, startDepth, file, true);
        }
        
        public PathInfo(DriveInfo drive, int depth, int startDepth)
        {
            (Depth, StartDepth, Drive, IsDrive) = (depth, startDepth, drive, true);
        }

        public bool CanBeAccessed()
        {
            try
            {
                if (IsDirectory)
                {
                    Directory.GetFiles();
                }
                else if (IsFile)
                {
                    System.IO.File.ReadAllText(FullName);
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public void Delete()
        {
            try
            {
                if (IsFile)
                {
                    File.Delete();
                }
                else
                {
                    Directory.Delete();
                }
            }
            catch (Exception e)
            {
                Console.SetCursorPosition(0, Console.WindowHeight + 1);
                Console.WriteLine(e);
            }
        }
    }
}