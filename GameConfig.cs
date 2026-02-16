using System.Collections.Generic;

namespace Decrypt
{
    public static class GameConfig
    {
        public static Dictionary<string, string> GetRequiredFiles(Games game)
        {
            var requiredFiles = new Dictionary<string, string>
            {
                { "SYSFILE", "system.dat" },
                { "CGFILE", "" },
                { "ADULTFILE", "adult.dat" }
            };

            switch (game)
            {
                case Games.AOKANA:
                    requiredFiles["CGFILE"] = "evcg2.dat";
                    break;
                case Games.AOKANAEX2:
                    requiredFiles["CGFILE"] = "evcg.dat";
                    requiredFiles["ADULTFILE"] = "adult.dat"; 
                    break;
                case Games.KOICHOCO:
                    requiredFiles["CGFILE"] = "graphics.dat";
                    requiredFiles["ADULTFILE"] = "f_graphics.dat"; 
                    break;
            }
            return requiredFiles;
        }

        // Adding more folders to cover all CGs, or use different localization(cn/jp/en) if needed.
        public static readonly List<string> FolderList = new List<string>{
            "def",
            "evcg", "evcg/jp",
            "evcg2", "evcg2/jp",
            "evcg3", "evcg3/jp",
            "hevcg", "hevcg/jp",
            "evcg_f", "evcg_f/jp",
            "evcg_censor_hi",
            "evcg_hi", "evcg_hi/jp",
            "sd_hi", "sd_hi/jp",
        };

        // Adding more games and their corresponding key tables as needed.
        public static readonly Dictionary<Games, uint[]> GameUIntArrayMap = new Dictionary<Games, uint[]>
        {
            // Reference: `DecryptHelper.GenerateKeyTable`, `DecryptHelper.DecryptData`
            // StartBytes, seedMultiplier, seedAdder, seedMoveBits, 
            // roundAdder, roundAnd, roundMoveBits
            // locater1, nextPlus1, locater2, nextXor2
            { Games.AOKANA,    new uint[] { 4U, 7391U, 42828U, 17U, 56U, 239U, 1U, 253U, 3U, 89U, 153U}  },
            { Games.AOKANAEX2, new uint[] { 3U, 4892U, 42816U, 7U, 156U, 206U, 3U, 179U, 3U, 89U, 119U}  },
            { Games.KOICHOCO,  new uint[] { 3U, 5892U, 41280U, 7U, 341U, 220U, 2U, 235U, 31U, 87U, 165U} }
        };
    }

    public enum Games
    {
        AOKANA,
        AOKANAEX2,
        KOICHOCO
    }

    public class Keys
    {
        public uint StartBytes { get; set; }
        public uint SeedMultiplier { get; set; }
        public uint SeedAdder { get; set; }
        public uint SeedMoveBits { get; set; }
        public uint RoundAdder { get; set; }
        public uint RoundAnd { get; set; }
        public uint Locater1 { get; set; }
        public uint NextPlus1 { get; set; }
        public uint Locater2 { get; set; }
        public uint NextXor2 { get; set; }
        public uint RoundMoveBits { get; set; }
        
        public Keys(Games game)
        {
            if (GameConfig.GameUIntArrayMap.TryGetValue(game, out uint[]? values) && values.Length == 11)
            {
                StartBytes = values[0];
                SeedMultiplier = values[1];
                SeedAdder = values[2];
                SeedMoveBits = values[3];
                RoundAdder = values[4];
                RoundAnd = values[5];
                RoundMoveBits = values[6];
                Locater1 = values[7];
                NextPlus1 = values[8];
                Locater2 = values[9];
                NextXor2 = values[10];
            }
            else
            {
                throw new ArgumentException($"Invalid game or missing keys for game: {game}");
            }
        }
    }
}
