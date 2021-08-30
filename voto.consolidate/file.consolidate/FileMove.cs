using file.consolidate;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace File.Consolidate
{
    public class FileMove : FileBase
    {
        public bool DeleteEmptyDirectories { get; set; }
        public bool DeleteDuplicateFiles { get; set; } //deletes file from source side

        public void SyncronousMove()
        {
            MoveDirectory(SourceDirectory, DestinationDirectory, SubDirectoriesFlag);
        }

        public async void AsyncronousMove()
        {
            await Task.Run(() => MoveDirectory(SourceDirectory, DestinationDirectory, SubDirectoriesFlag));
        }

        private void MoveDirectory(string source, string destination, bool subdirectories = true)
        {
            if (Directory.Exists(source) == false)
            {
                ProgressReport.Instance.Report = "Source does not exist.";
                return;
            }

            var files = GetFileList(source, subdirectories);

            if (files == null)
            {
                ProgressReport.Instance.Report = "No files exist with selected extensions.";
                return;
            }
            MoveFiles(files, destination);

            var directories = Directory.EnumerateDirectories(source);

            foreach (var directory in directories)
                if (Directory.EnumerateFileSystemEntries(directory).Any() == false &&
                    DeleteEmptyDirectories)
                    try
                    {
                        Directory.Delete(directory);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
        }


        private void MoveFiles(List<string> fileList, string destination)
        {
            Cancel = false;

            if (fileList == null) return;

            try
            {
                double TotalBps = 0;

                ProgressReport.Instance.Total += fileList.Count;

                foreach (var file in fileList)
                {
                    if (Cancel) break;

                    if (System.IO.File.Exists(file) == false) continue;

                    ProgressReport.Instance.FileName = Path.GetFileName(file);
                    var size = new FileInfo(file).Length;

                    //Based on creation time, not LastWrite time
                    if (new TimeSpan(DateTime.Now.Ticks - new FileInfo(file).CreationTime.Ticks).TotalDays < DaysOld)
                    {
                        ProgressReport.Instance.Skipped++;
                        continue;
                    }

                     var FullPath = CreateDestinationPath(destination, file);

                    try
                    {
                        if (System.IO.File.Exists(FullPath) == false)
                        {
                            var start = DateTime.Now;

                            System.IO.File.Move(file, FullPath);

                            TotalBps += size / new TimeSpan(DateTime.Now.Ticks - start.Ticks).TotalSeconds;

                            ProgressReport.Instance.Successful++;
                            ProgressReport.Instance.TotalBytes += size;
                            ProgressReport.Instance.Bps = TotalBps / ProgressReport.Instance.Successful;
                            ProgressReport.Instance.Report = "Successful";
                        }
                        else
                        {
                            if (DeleteDuplicateFiles)
                            {
                                System.IO.File.Delete(file);
                                ProgressReport.Instance.Report = "Deleted";
                            }
                            else
                            {
                                ProgressReport.Instance.Report = "Skipped";
                            }
                            ProgressReport.Instance.Skipped++;
                        }
                    }

                    catch (Exception e)
                    {
                        ProgressReport.Instance.Failed++;
                        ProgressReport.Instance.Report = $"Failed: {e.Message}";
                    }

                }
            }
            catch (Exception e)
            {
                ProgressReport.Instance.Report = e.Message;
            }
        }
    }
}