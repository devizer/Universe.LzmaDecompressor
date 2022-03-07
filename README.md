# LZMA Decompressor

- Fully Managed and Crossplatform
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

# Documentation
```CSharp
public static void LzmaDecompressTo(Stream inStream, Stream plainStream) {...}
public static void LzmaDecompressTo(Stream inStream, Stream plainStream, ProgressOptions progressOptions) {...}
```

Optional ProgressOptions is a holder of notification step in bytes and a callback:
```CSharp
Stopwatch startAt = Stopwatch.StartNew();
var progressOptions = new LzmaDecompressor.ProgressOptions()
{
    MinimumStep = 2 * 1024 * 1024,
    NotifyProgress = progress =>
    {
        Console.WriteLine($"{startAt.Elapsed} Progress: {progress.CurrentBytes}");
    }
};
```
