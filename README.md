# LZMA Decompressor

- Fully managed and cross-platform
- Coverage 94+ percents
- Small single source file sized as 22Kb written in 2nd version of C#
- Net Standard 1.0+, Net Framwork 2.0+, Net Core 1.0+
- Based on 7z SDK
- Fast decompression and very high compression rate
- Progress notification call back

# Test Coverage
- liblzma from ancient Debian 7 to the latest version 5.2
- Sizes from 1 byte to 4+ GB
- Linux, Windows and MacOS

# API
```CSharp
public static void LzmaDecompressTo(Stream inStream, Stream plainStream);
public static void LzmaDecompressTo(Stream inStream, Stream plainStream, ProgressOptions progressOptions);
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

# Adding or Upgrading 
```sh
curl -kSL -o Universe.LzmaDecompressor.cs https://raw.githubusercontent.com/devizer/Universe.LzmaDecompressor/main/out/Universe.LzmaDecompressor.cs
```

# Integrity check
Regarding integration tests an LZMA archive needs external integrity check (like SHA). Aborted or incomplete lzma-archive causes false positive decompression or even out-of-memory exceptions.