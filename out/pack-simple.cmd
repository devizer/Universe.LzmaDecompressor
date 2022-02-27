copy /b ..\Universe.LzmaDecompressor\LzmaDecompressor.cs + ..\Universe.LzmaDecompressor\LzmaDecompressorImplementation\*.cs LzmaDecoder.cs

C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /target:library /debug- /optimize+ /out:LzmaDecoder-net-4.0.dll LzmaDecoder.cs