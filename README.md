# LZMA Decompressor

- Fully managed and cross-platform
- 95+ percent test coverage
- Small single source file sized as 22Kb written in 2nd version of C#: [Universe.LzmaDecompressor.cs](out/Universe.LzmaDecompressor.cs)
- Net Standard 1.0+, Net Framwork 2.0+, Net Core 1.0+
- Based on 7z SDK. Fast decompression and very high compression rate
- Progress notification callback

# Test Coverage
- liblzma versions from 4.x to the latest version 5.2.5
- Sizes from 1 byte upto over 4+ GB
- Compression levels from `-1` to `-9 --extreme`
- Linux, Windows and MacOS

# API
```CSharp
public static void LzmaDecompressTo(Stream compressed, Stream plain);
public static void LzmaDecompressTo(Stream compressed, Stream plain, ProgressOptions progressOptions);
```
Optional ProgressOptions is a holder of notification step in bytes and a callback:
```CSharp
Stopwatch startAt = Stopwatch.StartNew();
var progressOptions = new LzmaDecompressor.ProgressOptions()
{
    MinimumStep = 2 * 1024 * 1024, /* bytes */
    NotifyProgress = progress =>
    {
        Console.WriteLine($"{startAt.Elapsed} Progress: {progress.CurrentBytes}");
    }
};
```

# Adding or upgrading
On windows, linux and macos
```sh
curl -kSL -o Universe.LzmaDecompressor.cs https://raw.githubusercontent.com/devizer/Universe.LzmaDecompressor/main/out/Universe.LzmaDecompressor.cs
```

# Integrity check
Regarding integration tests an LZMA archive needs external integrity check (like SHA). Aborted or incomplete lzma-archive causes false positive decompression or even out-of-memory exceptions.