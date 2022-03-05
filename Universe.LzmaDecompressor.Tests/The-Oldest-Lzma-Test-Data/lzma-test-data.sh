tar cf /tmp/code.tar /usr/lib/x86_64-linux-gnu /usr/bin 
ls -la /tmp/code.tar
work=$HOME/lzma-test-data
mkdir -p $work; cd $work && rm -f *
skip=0;
for size in 1 11 42 101 142 10001 10421 1000001; do
  echo "offset: $skip"
  dd if=/tmp/code.tar of=size-$size.code count=$size bs=1 skip=$skip
  skip=$((skip+size))
  xz --format=lzma -f -z -9 -e -k size-$size.code
done

