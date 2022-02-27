function ReadAllLines
{
    param([System.String] $fileName)

    $ret = @();
    $utf = new-object System.Text.UTF8Encoding($false)
    $fs = new-object System.IO.FileStream($fileName, [System.IO.FileMode]::Open)
    $rdr = new-object System.IO.StreamReader($fs, $utf)
    while($true) {
        $line=$rdr.ReadLine()
        if (($line -eq $null))
        {
            $rdr.Dispose()
            return $ret
        }
        $ret += $line
    }
}

Set-Location ".."
# $lines = ReadAllLines "Universe.LzmaDecompressor/LzmaDecompressor.cs"
$lines=@();
# exit 0;
# $lines += "";
$csFiles = Get-Childitem –Path ".\Universe.LzmaDecompressor\LzmaDecompressorImplementation" -Filter *.cs
foreach ($csFile in $csFiles) 
{
  $list = ReadAllLines $csFile.FullName
  foreach ($l in $list) {
    $lines += $l
  }
  $lines += ""
}

Write-Output "" > LzmaDecoder.cs
foreach ($l in $list) { 
  Write-Output $l >> LzmaDecoder.cs
}
