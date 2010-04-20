LoadAssembly("sceneparse.exe");
using sceneparse;

var x = new int[] {2,4,6,8,10};
x.MinInsertIncreasing(13);
Console.WriteLine(x.MkString());
