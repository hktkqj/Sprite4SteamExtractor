using System;
using System.IO;

namespace Decrypt
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!Options.TryParse(args, out var options))
            {
                Options.PrintUsage();
                Environment.ExitCode = 1;
                return;
            }

            if (options.ShowHelp || args.Length == 0)
            {
                Options.PrintUsage();
                return;
            }

            if (options.CombineCgPath != null)
            {
                if (!Directory.Exists(options.CombineCgPath))
                {
                    Console.WriteLine($"Game directory not found: {options.CombineCgPath}");
                    return;
                }
                if (options.Game == null || options.OutputDirectory == null)
                {
                    Console.WriteLine($"Must specify game with --game and output directory with --output when using --combine.");
                    return;
                }
                
                CgOrchestrator.Execute(options.CombineCgPath, options.OutputDirectory, options.Game.Value);
                return;
            } 
            else
            {
                if (!File.Exists(options.ArchivePath))
                {
                    Console.WriteLine($"Archive file not found: {options.ArchivePath}");
                    return;
                }
                
                if (options.ListEntries)
                {
                    using var fileStream = new FileStream(options.ArchivePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    var decryptHelper = new DecryptHelper(fileStream, options.Game);
                    decryptHelper.ShowFileEntries();
                }
                else if (options.ExtractEntries)
                {
                    if (string.IsNullOrWhiteSpace(options.OutputDirectory))
                    {
                        Console.WriteLine("--output is required when using --extract.");
                        Environment.ExitCode = 1;
                        return;
                    }
                    using var fileStream = new FileStream(options.ArchivePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    ArchiveExtractor.Extract(fileStream, options.OutputDirectory, options.Game);
                }
                else
                {
                    Console.WriteLine("At least one action is required: --list or --extract.");
                    Options.PrintUsage();
                    Environment.ExitCode = 1;
                }
            }
        }
    }
}
