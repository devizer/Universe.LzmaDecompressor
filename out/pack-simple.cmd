copy /b ..\Universe.LzmaDecompressor\LzmaDecompressor.cs + ..\Universe.LzmaDecompressor\LzmaDecompressorImplementation\*.cs Standalone.LzmaDecompressor.cs
del /f /q Standalone.LzmaDecompressor.cs-net-4.0.dll
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /target:library /debug- /optimize+ /out:Standalone.LzmaDecompressor.cs-net-4.0.dll Standalone.LzmaDecompressor.cs