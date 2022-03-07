# Universe.LzmaDecompressor

- Fully Managed and Crossplatform
- Coverage 94+ percents using ancient liblzma from Debian 7 to Ubuntu 22.04
- SMall single source file sized as 22Kb written in 2nd version of C#
- Net Standard 1.0+, Net Framwork 2.0+
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

Stopwatch startAt = Stopwatch.StartNew();
void ShowProgress(LzmaDecompressor.Progress info)
{
    Console.WriteLine($"{startAt.Elapsed} Progress: {info}");
    hasProgressNotification = true;
}

int step = (int) (lzmaCase.Size >= 100000 ? lzmaCase.Size / 10 : 4000);
var progressOptions = new LzmaDecompressor.ProgressOptions()
{
    MinimumStep = 2*1024*1024,
    NotifyProgress = ShowProgress
};

