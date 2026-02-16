using System.Text;

namespace Decrypt
{
    public class DecryptHelper
    {
        private readonly FileStream FileStream;
        public Dictionary<string, FileEntry> TableIndex;
        public Keys Key;
        public struct FileEntry
        {
            public uint Position;
            public uint Length;
            public uint Key;
        }

        public bool TryParseFileEntry()
        {
            TableIndex.Clear();
            FileStream.Position = 0L;
            try
            {
                byte[] array = new byte[1024];
                FileStream.ReadExactly(array, 0, 1024);
                
                int numFiles = 0;
                for (int i = (int)Key.StartBytes; i < 255; i++)
                {
                    numFiles += BitConverter.ToInt32(array, i * 4);
                }
                int fileEntriesSize = 16 * numFiles;
                if (fileEntriesSize <= 0 || fileEntriesSize > FileStream.Length - FileStream.Position)
                {
                    return false;
                }

                byte[] array2 = new byte[fileEntriesSize];
                FileStream.ReadExactly(array2, 0, fileEntriesSize);
                DecryptData(array2, fileEntriesSize, BitConverter.ToUInt32(array, 212));
                
                int pathsSize = BitConverter.ToInt32(array2, 12) - (1024 + fileEntriesSize);
                if (pathsSize <= 0 || pathsSize > FileStream.Length - FileStream.Position)
                {
                    return false;
                }
                byte[] array3 = new byte[pathsSize];
                FileStream.ReadExactly(array3, 0, array3.Length);
                
                DecryptData(array3, pathsSize, BitConverter.ToUInt32(array, 92));
                
                ParseFileEntries(array2, array3, numFiles);
                if (FileStream.Name.ToLower().EndsWith("adult.dat"))
                {
                    TableIndex.Remove("def/version.txt");
                }
                return true;
            } 
            catch  
            {
                return false;
            }
        }
        
        public DecryptHelper(FileStream fileStream, Games? game)
        {
            TableIndex = new Dictionary<string, FileEntry>();
            FileStream = fileStream;
            FileStream.Position = 0L;
            if (game == null)
            {
                foreach (var g in Enum.GetValues(typeof(Games)))                
                {
                    Key = new Keys((Games)g);
                    if (TryParseFileEntry())
                    {
                        Console.WriteLine($"Game detected: {g}");
                        return;
                    }
                }
                throw new Exception("Failed to detect game. Unsupported game or invalid archive file.");
            }
            else
            {
                Key = new Keys(game.Value);
                if (!TryParseFileEntry())
                {
                    throw new Exception($"Failed to parse file entries with the specified game: {game}.");
                }
            }
        }

        protected void ParseFileEntries(byte[] rtoc, byte[] rpaths, int numfiles)
        {
            int currentPathOffset = 0;
            for (int i = 0; i < numfiles; i++)
            {
                int entryOffset = 16 * i;
                uint length = BitConverter.ToUInt32(rtoc, entryOffset);
                int pathOffsetHint = BitConverter.ToInt32(rtoc, entryOffset + 4);
                uint key = BitConverter.ToUInt32(rtoc, entryOffset + 8);
                uint position = BitConverter.ToUInt32(rtoc, entryOffset + 12);
                
                int pathEnd = pathOffsetHint;
                while (pathEnd < rpaths.Length && rpaths[pathEnd] != 0)
                {
                    pathEnd++;
                }
                string filename = Encoding.ASCII.GetString(rpaths, currentPathOffset, pathEnd - currentPathOffset).ToLower();
                
                TableIndex.Add(filename, new FileEntry()
                {
                    Position = position,
                    Length = length,
                    Key = key
                });
                
                currentPathOffset = pathEnd + 1;
            }
        }

        private void GenerateKeyTable(byte[] outTable, uint seedKey)
        {
            // seedMultiplier, seedAdder, seedMoveBits
            uint num = seedKey * Key.SeedMultiplier + Key.SeedAdder;
            uint num2 = num << (int)Key.SeedMoveBits ^ num;
            for (int i = 0; i < 256; i++)
            {
                num -= seedKey;
                num += num2;
                // roundAdder, roundAnd
                num2 = num + Key.RoundAdder;
                num *= num2 & Key.RoundAnd;
                outTable[i] = (byte)num;
                num >>= (int)Key.RoundMoveBits;
            }
        }

        protected void DecryptData(byte[] encryptedData, int length, uint key)
        {
            byte[] array = new byte[256];
            GenerateKeyTable(array, key);
            for (int i = 0; i < length; i++)
            {
                byte b2 = encryptedData[i];
                // locater1, nextPlus1, locater2, nextXor2
                b2 ^= array[i % Key.Locater1];
                b2 += (byte)Key.NextPlus1;
                b2 += array[i % Key.Locater2];
                b2 ^= (byte)Key.NextXor2;
                encryptedData[i] = b2;
            }
        }

        public byte[] ReadFileData(FileEntry fileEntry, FileStream stream)
        {
            byte[] array = new byte[fileEntry.Length];
            stream.Position = fileEntry.Position;
            int read = 0;
            while (read < array.Length)
            {
                int r = stream.Read(array, read, array.Length - read);
                if (r == 0)
                {
                    throw new EndOfStreamException($"Unexpected EOF while reading entry at position {fileEntry.Position}.");
                }
                read += r;
            }
            DecryptData(array, array.Length, fileEntry.Key);
            return array;
        }

        public void ShowFileEntries()
        {
            foreach (var entry in TableIndex)
            {
                Console.WriteLine(
                    $"{entry.Key,-40} Position={entry.Value.Position, -10}   Length={entry.Value.Length, -6}   Key=0x{entry.Value.Key.ToString("X8")}");
            }
            Console.WriteLine($"Total entries: {TableIndex.Count}");
        }
    }
}
