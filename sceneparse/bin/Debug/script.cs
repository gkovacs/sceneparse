LoadAssembly("sceneparse.exe");
using sceneparse;

int[,] x = new int[10,10];
x.SetColumn(0,255);
x.SetRow(1, 255);
x.MkString();
