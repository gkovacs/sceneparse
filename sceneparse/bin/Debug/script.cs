LoadAssembly("sceneparse.exe");
using sceneparse;

int[,] x = new int[10,10];
x.SetAll(255);
Console.WriteLine(x.PadXY(1,2,10,5).MkString());
