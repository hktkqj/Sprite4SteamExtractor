# Sprite4SteamExtractor

A command-line extractor and CG composer for selected Steam visual novel archives.

[Chinese | 中文版](README.zh-CN.md)

## Overview

`Sprite4SteamExtractor` can:
- Parse encrypted `.dat` archives for supported games
- List archive entries
- Extract all entries to an output folder
- Compose final CG images from extracted sprite layers

The tool currently supports:
- `AOKANA` - Aokana: Four Rhythms Across the Blue / 蒼の彼方のフォーリズム
- `AOKANAEX1` - Aokana: Four Rhythms Across the Blue Extra 1/ 蒼の彼方のフォーリズム (Extra1)
- `AOKANAEX2` - Aokana: Four Rhythms Across the Blue Extra 2/ 蒼の彼方のフォーリズム (Extra2)
- `KOICHOCO` - Love, Elections, and Chocolate / 恋と選挙とチョコレート

## Project Structure

- `Program.cs`  
  CLI entrypoint and top-level command routing.

- `Options.cs`  
  Command-line option parsing and help text.

- `GameConfig.cs`  
  Game-dependent archive and folder configuration for CG workflows. Including core decryption key tables per game.

- `DecryptHelper.cs`  
  Archive table parsing and file data decryption.

- `ArchiveExtractor.cs`  
  Parallel extraction pipeline that writes decrypted files to disk.

- `CgCombine.cs`  
  CG list parsing, sprite path collection, and bitmap composition using SkiaSharp.

- `CgOrchestrator.cs`  
  End-to-end CG workflow orchestration:
  validate required files -> extract related archives -> combine CGs.

- `Sprite4SteamExtractor.csproj`  
  .NET project file and package references.

## Features

1. Archive entry listing (`--list`)
2. Full archive extraction (`--extract`)
3. Multi-game key selection (`--game`)
4. Automated CG composition (`--combine`)
5. Parallelized extraction and CG processing

## Usage

### Show help

```bash
Sprite4SteamExtractor.exe -h
```

### List entries in an archive

```bash
Sprite4SteamExtractor.exe --game KOICHOCO --list "D:\\path\\to\\graphics.dat"
```

### Extract all files from an archive

```bash
Sprite4SteamExtractor.exe --game AOKANA --extract --output "D:\\out\\extract" "D:\\path\\to\\evcg2.dat"
```

### Compose CGs from a game data directory

```bash
Sprite4SteamExtractor.exe --game KOICHOCO --combine "C:\\SteamLibrary\\steamapps\\common\\KoiChoco\\KoiChoco_Data" --output "D:\\out\\cg"
```

After composition, final images are written to:
- `...\\<output>\\CombinedCG\\`

## Secondary Development & Build Preparation

## 1) Prerequisites

- Windows (recommended, tested in this workspace)
- .NET SDK `10.0` or newer (project target: `net10.0`)
- Git

## 2) Restore dependencies

```bash
dotnet restore
```

NuGet package used by this project:
- `SkiaSharp`

## 3) Build for development

```bash
dotnet build
```

## 4) Run from source

```bash
dotnet run -- --help
```

Examples:

```bash
dotnet run -- --game KOICHOCO --list "D:\\path\\to\\graphics.dat"
dotnet run -- --game AOKANA --extract --output "D:\\out" "D:\\path\\to\\evcg2.dat"
```

## 5) Publish single-file executable

Project already configures single-file/self-contained publish for `win-x64`.

```bash
dotnet publish -c Release
```

Output is typically under:
- `bin\\Release\\net10.0\\win-x64\\publish\\`

## Development Notes

- Keep module responsibilities isolated (parser / extractor / compositor / orchestrator).
- If adding a new game:
  1. Add enum in `Games`
  2. Add key table in `GameConfig.GameUIntArrayMap`
  3. Add required archive mapping in `GameConfig.GetRequiredFiles`
  4. Verify CG folder list in `GameConfig.FolderList`
- For large archives, test with representative data to validate extraction throughput and memory usage.

## Disclaimer

This tool is intended for legitimate personal backup/modding and reverse-engineering learning scenarios. Ensure you comply with local laws, game EULA, and copyright terms before use.
