LoadAssembly("sceneparse.exe");
using sceneparse;

var x = new int[,] {{1}};
//x.MinInsertIncreasing(13);
Console.WriteLine(x.ScaleGrid(1).MkString());
