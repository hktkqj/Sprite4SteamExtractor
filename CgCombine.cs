using System;
using SkiaSharp;

namespace Decrypt
{
    public class CgCombine
    {
        private static readonly Dictionary<string, string> cgPathDict = new();
        public static readonly Dictionary<string, List<string>> CgList = new();

        public CgCombine(string baseFolder)
        {
            var defFolder = Path.Combine(baseFolder, "def");
            Console.WriteLine($"Collcting CG list(def/vcglist.csv) from {defFolder}...");
            CgListHandler(Path.Combine(defFolder, "vcglist.csv"));
            foreach (var folder in GameConfig.FolderList)
            {
                var folderPath = Path.Combine(baseFolder, folder);
                if (!Directory.Exists(folderPath))
                {
                    Console.WriteLine($"Folder {folder} not found, skipping related CGs...");
                    continue;
                }
                Console.WriteLine($"Collecting CG paths from {folderPath}...");
                CgPathCollector(folderPath);
            }

            Console.WriteLine($"\nDone Initialize.");
        }
        public static SKBitmap? Combiner(List<string> FileList)
        {
            SKBitmap? combinedBitmap = null;
            try 
            {
                foreach (var file in FileList)
                {
                    if (!cgPathDict.TryGetValue(file, out var filePath))
                    {
                        Console.WriteLine($"File {file} not found in CgPathDict.");
                        combinedBitmap?.Dispose();
                        return null;
                    }
                    using var inputStream = new SKFileStream(filePath);
                    using var bitmap = SKBitmap.Decode(inputStream);
                    if (bitmap == null)
                    {
                        Console.WriteLine($"Failed to decode {file}@{filePath}.");
                        combinedBitmap?.Dispose();
                        return null;
                    }
                    if (combinedBitmap == null)
                    {
                        combinedBitmap = new SKBitmap(bitmap.Width, bitmap.Height, bitmap.ColorType, bitmap.AlphaType);
                        using (var canvas = new SKCanvas(combinedBitmap))
                        {
                            canvas.Clear(SKColors.Transparent);
                        }
                    } 
                    using (var canvas = new SKCanvas(combinedBitmap))
                    {
                        canvas.DrawBitmap(bitmap, 0, 0);
                    }
                }
            }
            catch
            {
                combinedBitmap?.Dispose();
                throw;
            }
            return combinedBitmap;
        }

        public static void CgListHandler(string cgListPath)
        {
            using var reader = new StreamReader(cgListPath);
            string? line;
            int Counter = 0;
            while ((line = reader.ReadLine()) != null)
            {
                // Split the line by whitespace and take the first part as the file name
                string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 1)
                {
                    string fileName = parts[0].ToLowerInvariant() + ".png";
                    var filePaths = new List<string>(parts.Length - 1);
                    for (int i = 1; i < parts.Length; i++)
                    {
                        filePaths.Add(parts[i].ToLowerInvariant() + ".webp");
                    }
                    CgList[fileName] = filePaths;
                    Counter++;
                }
            }
            Console.WriteLine($"Total {Counter} CGs added from {cgListPath}.");
        }

        public static void CgPathCollector(string CgFolder)
        {
            // List all file in CgFolder
            string[] files = Directory.GetFiles(CgFolder, "*.webp", SearchOption.TopDirectoryOnly);
            var Counter = 0;
            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file).ToLower();
                if (!cgPathDict.ContainsKey(fileName))
                {
                    cgPathDict[fileName] = Path.Combine(CgFolder, fileName);
                    Counter++;
                }
            }
            Console.WriteLine($"Collected {Counter} sprite(s) in {CgFolder}.");
        }
    }
}