using System;
using System.Collections.Generic;

namespace Decrypt
{
    public class Options
    {
        public Games? Game { get; set; }
        public string? ArchivePath { get; set; }
        public string? OutputDirectory { get; set; }
        public string? CombineCgPath { get; set; }
        public bool ListEntries = false;
        public bool ExtractEntries = false;
        public bool ShowHelp { get; set; }

        public static bool TryParse(string[] args, out Options options)
        {
            options = new Options();

            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                switch (arg)
                {
                    case "-h":
                    case "--help":
                        options.ShowHelp = true;
                        break;

                    case "-g":
                    case "--game":
                        if (i + 1 >= args.Length)
                        {
                            Console.WriteLine("Missing value for --game.");
                            return false;
                        }
                        i++;
                        if (!Enum.TryParse<Games>(args[i], true, out var game))
                        {
                            Console.WriteLine($"Invalid game name: {args[i]}");
                            Console.WriteLine("Available values: " + string.Join(", ", Enum.GetNames(typeof(Games))));
                            return false;
                        }
                        options.Game = game;
                        break;
                    
                    case "-l":
                    case "--list":
                        options.ListEntries = true;
                        break;

                    case "-e":
                    case "--extract":
                        options.ExtractEntries = true;
                        break;

                    case "-o":
                    case "--output":
                        if (i + 1 >= args.Length)
                        {
                            Console.WriteLine("Missing value for --output.");
                            return false;
                        }
                        options.OutputDirectory = args[++i];
                        break;
                    
                    case "-c":
                    case "--combine":
                        if (i + 1 >= args.Length)
                        {
                            Console.WriteLine("Missing value for --combine.");
                            return false;
                        }
                        options.CombineCgPath = args[++i];
                        break;
                }
            }
            options.ArchivePath = args.Length > 0 && !args[^1].StartsWith("-") ? args[^1] : null;
            return true;
        }

        public static void PrintUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  Sprite4SteamExtractor.exe [-g|--game <game_name>] [-l|--list] [-e|--extract] [-o|--output <dir_path>] <encrypted_file.dat>");
            Console.WriteLine("  Sprite4SteamExtractor.exe [-g|--game <game_name>] -o|--output <dir_path> [-c|--combine] <game_dir> "); 
            Console.WriteLine("                              (e.g. \"C:\\SteamLibrary\\steamapps\\common\\KoiChoco\\KoiChoco_Data\")");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  -g, --game <game_name>    Game name defined in GameConsts.cs.");
            Console.WriteLine("                                Available values: " + string.Join(", ", Enum.GetNames(typeof(Games))));
            Console.WriteLine("                                Notice: Aokana EX1 and Aokana share the same keys.");
            Console.WriteLine("  -l, --list                List file entries in the specified archive file.");
            Console.WriteLine("  -e, --extract             Extract all files from the specified archive file.");
            Console.WriteLine("  -o, --output <dir_path>   Output directory for extraction (required with --extract).");
            Console.WriteLine("  -c, --combine <game_dir>  Combine CGs from the specified game directory.");
            Console.WriteLine("  -h, --help                Show help.");
        }
    }
}
