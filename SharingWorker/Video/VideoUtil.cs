using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using SharpCompress.Archive;

namespace SharingWorker.Video
{
    static class VideoUtil
    {
        private static bool isRarSuccess;
        private static readonly string rarExecutable = ConfigurationManager.AppSettings["RarExecutable"];
        
        public static string GetFileFormat(string filePath)
        {
            string ret = null;
            if (filePath == null) return ret;

            using (var archive = ArchiveFactory.Open(filePath))
            {
                foreach (var entry in archive.Entries)
                {
                    if (!entry.IsDirectory)
                    {
                        var ext = Path.GetExtension(entry.FilePath);
                        if (ext != ".URL" && ext != ".url" && ext != ".lnk" && ext != ".LNK")
                            ret = ext;
                    }
                }
            }
            return ret.TrimStart('.').ToUpper();
        }

        public static string GetFileSizeGB(string uploadPath, string fileName)
        {
            var files = Directory.GetFiles(uploadPath, string.Format("{0}*.part*.rar", fileName), SearchOption.TopDirectoryOnly);
            long ret = 0;
            if (files.Length == 0) return ret.ToString();
            
            foreach (var file in files)
            {
                var info = new FileInfo(file);
                ret += info.Length;
            }
            double GB = ret / 1073741824.0;
            return GB.ToString("F");
        }

        public static async Task<bool> ToRAR(string sourcePath, string outputName)
        {
            var outputDir = @"D:\Upload";
            var outputPath = Path.Combine(outputDir, outputName);
            var attachment = Path.Combine(outputDir, "nanamiyusa's Collection.url");

            var fileInfo = new FileInfo(sourcePath);
            var splitSize = GetSplitSize(fileInfo.Length.ToFileSize());
            
            var processInfo = new ProcessStartInfo
            {
                FileName = rarExecutable,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                
            };

            if (File.Exists(attachment))
                processInfo.Arguments = string.Format("a -t -m0 -rr3% -v{3}m -ep {0}.rar \"{1}\" \"{2}\"", outputPath, sourcePath, attachment, splitSize);
            else
                processInfo.Arguments = string.Format("a -t -m0 -rr3% -v{2}m -ep {0}.rar \"{1}\"", outputPath, sourcePath, splitSize);
            
            return await Task.Run(() =>
            {
                using (var process = Process.Start(processInfo))
                {
                    isRarSuccess = false;
                    if (process != null)
                    {
                        process.OutputDataReceived += (sender, args) =>
                        {
                            if (!isRarSuccess)
                                isRarSuccess = args.Data.Contains("全部確認");
                        };
                        process.BeginOutputReadLine();
                        process.WaitForExit();
                    }
                    return isRarSuccess;
                }
            });
        }

        private static int GetSplitSize(string fileSize)
        {
            var ret = 300;
            if (fileSize.Contains("GB"))
            {
                var size = Convert.ToDouble(fileSize.Replace("GB", string.Empty));
                if (size >= 1.5 && size < 2.0)
                    ret = 350;
                else if (size >= 2.0 && size < 3.0)
                    ret = 400;
                else if (size >= 3.0)
                    ret = 450;
            }
            return ret;
        }
    }
}
