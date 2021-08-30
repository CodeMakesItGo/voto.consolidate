using file.consolidate;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace File.Consolidate
{
    public class FileBase
    {
        protected static bool Cancel { get; set; }

        public FileBase()
        {
           
        }

        public string SourceDirectory { get; set; }
        public string DestinationDirectory { get; set; }
        public bool SubDirectoriesFlag { get; set; }
        public bool StoreByPictureDate { get; set; }
        public int DaysOld { get; set; }
        public List<string> Extensions { get; set; }

        protected List<string> GetFileList(string source, bool subdirectorySearch)
        {
            if (Directory.Exists(source) == false)
            {
                ProgressReport.Instance.Report = "Source does not exist.";
                return null;
            }

            if (Extensions == null || Extensions.Count == 0)
                return null;

            try
            {
                return Directory.EnumerateFiles(source, "*.*",
                        subdirectorySearch ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                    .Where(s => Extensions.Any(ext => string.Equals(ext, Path.GetExtension(s),
                        StringComparison.CurrentCultureIgnoreCase)))
                    .ToList();
            }
            catch (Exception e)
            {
                ProgressReport.Instance.Report = e.Message;
            }

            return null;
        }

        protected string CreateDestinationPath(string destination, string file)
        {
            var fileName = Path.GetFileName(file);

            return CreateDestinationPath(destination, fileName, new FileInfo(file).LastWriteTime);
        }

        protected string CreateDestinationPath(string destination, string fileName, DateTime fileLastWriteTime)
        {
            var filePath = "";

            if (fileName == null) return filePath;

            if (StoreByPictureDate)
            {
                var dt = fileLastWriteTime;

                filePath = Path.Combine(destination, dt.ToString("yyyy-MM-dd"));
            }
            else
            {
                filePath = Path.Combine(destination, DateTime.Now.ToString("yyyy-MM-dd"));
            }

            if (Directory.Exists(filePath) == false)
            {
                Directory.CreateDirectory(filePath);
            }

            return Path.Combine(filePath, fileName);
        }

        public static void CancelAll()
        {
            Cancel = true;
        }

    }
}