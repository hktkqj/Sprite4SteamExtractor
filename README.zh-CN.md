# Sprite4SteamExtractor

用于雪碧社游戏在Steam平台版资源解析、解密、提取与CG合成的命令行工具。

## 项目简介

`Sprite4SteamExtractor` 目前支持：
- 解析并解密指定游戏的 `.dat` 资源包
- 列出资源包文件条目
- 批量导出资源文件
- 从已导出的图层素材合成最终 CG 图片

当前已支持游戏：
The tool currently supports:
- `AOKANA` - 苍之彼方的四重奏 / 蒼の彼方のフォーリズム
- `AOKANAEX1` - 苍之彼方的四重奏 Extra 1/ 蒼の彼方のフォーリズム(Extra1)
- `AOKANAEX2` - 苍之彼方的四重奏 Extra 2/ 蒼の彼方のフォーリズム(Extra2)
- `KOICHOCO` - 恋爱与选举与巧克力 / 恋と選挙とチョコレート

## 程序结构

- `Program.cs`  
  程序入口，负责命令路由与主流程控制。

- `Options.cs`  
  命令行参数解析与帮助信息输出。

- `GameConfig.cs`  
  各游戏所需资源包名称、CG 目录配置以及对应的解密参数密钥表。

- `DecryptHelper.cs`  
  资源表解析、数据解密、单文件读取逻辑。

- `ArchiveExtractor.cs`  
  并行提取流程，将解密后的内容写入磁盘。

- `CgCombine.cs`  
  CG 列表解析、素材路径收集、位图合成（SkiaSharp）。

- `CgOrchestrator.cs`  
  CG 流程编排：校验必需文件 -> 提取相关资源 -> 批量合成 CG。

- `Sprite4SteamExtractor.csproj`  
  .NET 项目配置及依赖声明。

## 功能说明

1. 资源条目查看（`--list`）
2. 资源包完整提取（`--extract`）
3. 多游戏解密支持（`--game`）
4. 自动 CG 合成（`--combine`）
5. 并行提取与并行合成

## 下载

预编译的二进制文件可以在 [Releases](https://github.com/hktkqj/Sprite4SteamExtractor/releases) 页面下载。请选择适合您平台的版本：

- **Windows**: `Sprite4SteamExtractor-win-x64.zip`
- **Linux**: `Sprite4SteamExtractor-linux-x64.tar.gz`
- **macOS (Intel)**: `Sprite4SteamExtractor-osx-x64.tar.gz`
- **macOS (Apple Silicon)**: `Sprite4SteamExtractor-osx-arm64.tar.gz`

解压后可直接运行，无需安装。

## 使用方法

### 查看帮助

```bash
Sprite4SteamExtractor.exe -h
```

### 查看资源包条目

```bash
Sprite4SteamExtractor.exe --game KOICHOCO --list "D:\\path\\to\\graphics.dat"
```

### 提取资源包全部文件

```bash
Sprite4SteamExtractor.exe --game AOKANA --extract --output "D:\\out\\extract" "D:\\path\\to\\evcg2.dat"
```

### 从游戏目录执行 CG 合成

```bash
Sprite4SteamExtractor.exe --game KOICHOCO --combine "C:\\SteamLibrary\\steamapps\\common\\KoiChoco\\KoiChoco_Data" --output "D:\\output_dir"
```

合成结果将输出到：
- `...\\<output_dir>\\CombinedCG\\`

## 二次开发与编译准备流程

### 1）环境准备

- Windows（当前工作区已在 Windows 下验证）
- .NET SDK `10.0` 或更高版本（项目目标框架：`net10.0`）
- Git

### 2）还原依赖

```bash
dotnet restore
```

当前主要 NuGet 依赖：
- `SkiaSharp`

### 3）开发构建

```bash
dotnet build
```

### 4）源码运行

```bash
dotnet run -- --help
```

示例：

```bash
dotnet run -- --game KOICHOCO --list "D:\\path\\to\\graphics.dat"
dotnet run -- --game AOKANA --extract --output "D:\\out" "D:\\path\\to\\evcg2.dat"
```

### 5）发布单文件可执行程序

为特定平台发布：

```bash
# Windows
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true

# Linux
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true

# macOS (Intel)
dotnet publish -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true

# macOS (Apple Silicon)
dotnet publish -c Release -r osx-arm64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true
```

默认输出目录通常为：
- `bin/Release/net8.0/<runtime-id>/publish/`

## 自动发布

仓库已配置 GitHub Actions，在推送新标签时会自动构建并发布二进制文件。创建新版本的方法：

```bash
git tag v1.0.0
git push origin v1.0.0
```

工作流会自动为所有支持的平台构建二进制文件，并创建包含这些文件的 GitHub Release。

## 新游戏支持（雪碧还出新游戏吗，，，）

- 若要新增游戏支持，建议按以下顺序：
  1. 在 `Games` 枚举增加游戏项
  2. 在 `GameConsts.GameUIntArrayMap` 增加密钥参数
  3. 在 `GameConfig.GetRequiredFiles` 配置必需资源包
  4. 在 `GameConfig.FolderList` 校对 CG 素材目录
- 对大资源包建议使用真实数据做吞吐与内存测试。

## 免责声明

本项目仅建议用于合法的个人学习、备份与研究场景。使用前请确认符合当地法律法规、游戏 EULA 与版权要求。
