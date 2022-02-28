work=$HOME/build
mkdir -p $work && cd $work
git clone https://github.com/devizer/Universe.LzmaDecompressor
cd Universe.LzmaDecompressor
git pull;
cd Universe.LzmaDecompressor.Tests

time dotnet test -c Release
