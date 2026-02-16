using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Decrypt
{
    public class ArchiveExtractor
    {
        public static void Extract(FileStream fileStream, string outputDirectory, Games? game)
        {
            DecryptHelper decryptHelper;
            try
            {
                if (game == null)
                {
                    Console.WriteLine("Trying to detect game...");
                    decryptHelper = new DecryptHelper(fileStream, null);
                } 
                else 
                {
                    Console.WriteLine($"Selected game: {game}");
                    decryptHelper = new DecryptHelper(fileStream, game);
                }
            } 
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing DecryptHelper: {ex.Message}");
                // In a library method we might want to throw, but to request specific behavior we keep it close to original
                // but since it returns void and prints to console, let's rethrow or handle gracefully.
                // The original code set Environment.ExitCode = 1 and returned. 
                // We should probably throw so the caller can handle exit code.
                throw;
            }

            var total = decryptHelper.TableIndex.Count;
            var completed = 0;
            object progressLock = new object();
            Parallel.ForEach(
                decryptHelper.TableIndex,
                new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                () => new FileStream(fileStream.Name, FileMode.Open, FileAccess.Read, FileShare.Read),
                (entry, _, localStream) =>
                {
                    var data = decryptHelper.ReadFileData(entry.Value, localStream);
                    var outputPath = Path.Combine(outputDirectory, entry.Key);
                    var dirPath = Path.GetDirectoryName(outputPath);
                    if (!string.IsNullOrEmpty(dirPath))
                    {
                        Directory.CreateDirectory(dirPath);
                    }
                    File.WriteAllBytes(outputPath, data);

                    lock (progressLock)
                    {
                        completed++;
                        if (completed % 10 == 0 || completed == total)
                        {
                            Console.Write($"\rExtracting files: {completed}/{total} ({completed * 100 / total}%)   ");
                        }
                    }

                    return localStream;
                },
                localStream => localStream.Dispose());
            Console.WriteLine($"\nExtraction complete. {completed} files extracted.");
        }
    }
}
