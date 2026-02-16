using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharp;

namespace Decrypt
{
    public class CgOrchestrator
    {
        public static void Execute(string includePath, string outputPath, Games game)
        {
            // First verify required files exist and try extract CG list if possible
            var requiredFiles = GameConfig.GetRequiredFiles(game);
            
            foreach (var fileType in requiredFiles.Keys)
            {
                var fileName = requiredFiles[fileType];
                if (string.IsNullOrEmpty(fileName)) continue;

                var filePath = Path.Combine(includePath, fileName);
                if (!File.Exists(filePath))
                {
                    if (fileType == "ADULTFILE")
                    {
                        Console.WriteLine($"Optional file {fileName} not found in {includePath}. Skipping related CGs.");
                        continue;
                    }
                    Console.WriteLine($"Required file {fileName} not found in {includePath}.");
                    return;
                } 
                else
                {
                    using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                    ArchiveExtractor.Extract(fs, outputPath, game);
                }
                Console.WriteLine($"Extracted required file {fileName} to {outputPath}.");
            }

            var cgCombine = new CgCombine(outputPath);
            var counter = 0;
            var processed = 0;
            var failedEntry = new Dictionary<string, List<string>>();
            outputPath = Path.Combine(outputPath, "CombinedCG");
            var totalCg = CgCombine.CgList.Count;
            
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            Parallel.ForEach(CgCombine.CgList, cg =>
            {
                var exportFilename = cg.Key;
                var bitmapData = CgCombine.Combiner(cg.Value);
                if (bitmapData != null)
                {
                    using (bitmapData)
                    {
                        Interlocked.Increment(ref counter);
                        var outputFilePath = Path.Combine(outputPath, exportFilename);
                        using var outputStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write);
                        bitmapData.Encode(outputStream, SKEncodedImageFormat.Png, 100);
                    }
                }
                else
                {
                    Console.WriteLine($"Failed to combine {exportFilename}.");
                    lock (failedEntry)
                    {
                        failedEntry.Add(exportFilename, cg.Value);
                    }
                }

                var done = Interlocked.Increment(ref processed);
                if (done % 20 == 0 || done == totalCg)
                {
                    var success = Volatile.Read(ref counter);
                    Console.Write($"\rCombining CG: {done}/{totalCg} (success: {success}, failed: {done - success})   ");
                }
            });

            Console.WriteLine();

            Console.WriteLine($"Total {counter} of {CgCombine.CgList.Count} CGs combined, saving to {outputPath}/.");
            Console.WriteLine($"Failed to combine {failedEntry.Count} CGs:");
            foreach (var entry in failedEntry)
            {
                Console.WriteLine($"  Failed to combine {entry.Key} from {string.Join(", ", entry.Value)}.");
            }
        }
    }
}
