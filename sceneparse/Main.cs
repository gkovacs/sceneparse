//  
//  Main.cs
//  
//  Author:
//       Geza Kovacs <gkovacs@mit.edu>
// 
//  Copyright (c) 2010 Geza Kovacs
// 
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
// 
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;

namespace sceneparse
{
	public class MainClass {
		public static IVisNode DeSerializeFromFile(string fn) {
			var x = new Polenter.Serialization.SharpSerializer();
			//XmlSerializer x = new XmlSerializer(typen);
			FileStream fs = new FileStream(fn, FileMode.Open);
			var n = (IVisNode)x.Deserialize(fs);
			fs.Close();
			return n;
		}
		
		public static int SlidingImgComp(int[,] refi, int[,] s, ref int xout, ref int yout) {
			if (s.Width() > refi.Width())
				throw new Exception("Supplied image too wide");
			if (s.Height() > refi.Height())
				throw new Exception("Supplied image too high");
			int mindiff = int.MaxValue;
			int[,] o = new int[refi.Width(),refi.Height()];
			int xbest = 0;
			int ybest = 0;
			for (int y = 10; y <= o.Height()-s.Height(); y += 20) {
				for (int x = 10; x <= o.Width()-s.Width(); x += 20) {
					o.SetAll(0);
					s.PadXY(o, x, o.Width()-s.Width()-x, y, o.Height()-s.Height()-y);
					//o.ToPNG("nout"+(y*o.Width()+x));
					int diffv = CompImages(refi, o, 10);
					//Console.WriteLine(diffv);
					if (diffv < mindiff) {
						mindiff = diffv;
						xbest = x;
						ybest = y;
					}
				}
			}
			for (int y = Math.Max(0, ybest-18); y <= Math.Min(o.Height()-s.Height(), ybest+18); y += 10) {
				for (int x = Math.Max(0, xbest-18); x <= Math.Min(o.Width()-s.Width(), xbest+18); x += 10) {
					o.SetAll(0);
					s.PadXY(o, x, o.Width()-s.Width()-x, y, o.Height()-s.Height()-y);
					//o.ToPNG("nout"+(y*o.Width()+x));
					int diffv = CompImages(refi, o, 5);
					//Console.WriteLine(diffv);
					if (diffv < mindiff) {
						mindiff = diffv;
						xbest = x;
						ybest = y;
					}
				}
			}
			for (int y = Math.Max(0, ybest-8); y <= Math.Min(o.Height()-s.Height(), ybest+8); y += 5) {
				for (int x = Math.Max(0, xbest-8); x <= Math.Min(o.Width()-s.Width(), xbest+8); x += 5) {
					o.SetAll(0);
					s.PadXY(o, x, o.Width()-s.Width()-x, y, o.Height()-s.Height()-y);
					//o.ToPNG("nout"+(y*o.Width()+x));
					int diffv = CompImages(refi, o, 3);
					//Console.WriteLine(diffv);
					if (diffv < mindiff) {
						mindiff = diffv;
						xbest = x;
						ybest = y;
					}
				}
			}
			for (int y = Math.Max(0, ybest-4); y <= Math.Min(o.Height()-s.Height(), ybest+4); y += 2) {
				for (int x = Math.Max(0, xbest-4); x <= Math.Min(o.Width()-s.Width(), xbest+4); x += 2) {
					o.SetAll(0);
					s.PadXY(o, x, o.Width()-s.Width()-x, y, o.Height()-s.Height()-y);
					//o.ToPNG("nout"+(y*o.Width()+x));
					int diffv = CompImages(refi, o, 1);
					//Console.WriteLine(diffv);
					if (diffv < mindiff) {
						mindiff = diffv;
						xbest = x;
						ybest = y;
					}
				}
			}
			for (int y = Math.Max(0, ybest-1); y <= Math.Min(o.Height()-s.Height(), ybest+1); y += 1) {
				for (int x = Math.Max(0, xbest-1); x <= Math.Min(o.Width()-s.Width(), xbest+1); x += 1) {
					o.SetAll(0);
					s.PadXY(o, x, o.Width()-s.Width()-x, y, o.Height()-s.Height()-y);
					//o.ToPNG("nout"+(y*o.Width()+x));
					int diffv = CompImages(refi, o, 1);
					//Console.WriteLine(diffv);
					if (diffv < mindiff) {
						mindiff = diffv;
						xbest = x;
						ybest = y;
					}
				}
			}
			xout = xbest;
			yout = ybest;
			return mindiff;
		}
		
		public static int SlidingImgComp2(int[,] refi, int[,] s, ref int xout, ref int yout) {
			if (s.Width() > refi.Width())
				throw new Exception("Supplied image too wide");
			if (s.Height() > refi.Height())
				throw new Exception("Supplied image too high");
			var rpixprop = new int[10][,];
			var spixprop = new int[10][,];
			rpixprop[0] = refi;
			spixprop[0] = s;
			int rwidth = refi.Width();
			int rheight = refi.Height();
			int swidth = s.Width();
			int sheight = s.Height();
			for (int i = 0; i < 9; ++i) {
				rpixprop[i+1] = new int[rwidth,rheight];
				spixprop[i+1] = new int[swidth+2*i+2,sheight+2*i+2];
				rpixprop[i].PixelProp8(rpixprop[i+1]);
				spixprop[i].PixelProp8Exp(spixprop[i+1]);
			}
			int rsheightdiff = rheight-sheight+1;
			int rswidthdiff = rwidth-swidth+1;
			int[,] total = new int[rwidth-swidth+1,rheight-sheight+1];
			int weight = 20;
			for (int i = 0; i < 10; --i) {
				int csheight = sheight+2*i;
				int cswidth = swidth+2*i;
				for (int y = 0; y < rheight-csheight+1; ++y) {
					for (int x = 0; x < rwidth-cswidth+1; ++x) {
						//total[x,y] += weight*rpixprop[i].Diff(spixprop[i], x, y);
						int loctot = 0;
						//for (int ly = 0; ly < y; ++ly) {
						//	for (int lx = 0; lx < x; ++lx) {
						//		if (rpixprop[i][lx,ly] != 0) ++loctot;
						//	}
						//}
						for (int ny = 0; ny < csheight; ++ny) {
							for (int nx = 0; nx < cswidth; ++nx) {
								if (rpixprop[i][nx+x,ny+y] != spixprop[i][nx,ny]) ++loctot;
							}
						}
						//for (int ly = sheight; ly < rheight; ++ly) {
						//	for (int lx = swidth; lx < rwidth; ++lx) {
						//		if (rpixprop[i][lx,ly] != 0) ++loctot;
						//	}
						//}
						total[x,y] += weight*loctot;
					}
				}
				//rheight += 2;
				//rwidth += 2;
				//sheight += 2;
				//swidth += 2;
				++weight;
			}
			return total.Min(ref xout, ref yout);
		}
		
		public static int CompImages(int[,] o1, int[,] o2, int propdepth) {
			if (o1.Width() != o2.Width())
				throw new Exception("wrong width");
			if (o1.Height() != o2.Height())
				throw new Exception("wrong height");
			int[,] a1 = o1.DeepCopy();
			int[,] a2 = o2.DeepCopy();
			int[,] b1 = new int[o1.Width(),o1.Height()];
			int[,] b2 = new int[o2.Width(),o2.Height()];
			int total = 0;
			int weight = 30;
			for (int i = 0; i < propdepth; ++i) {
				a1.PixelProp8(b1);
				a2.PixelProp8(b2);
				total += b1.Diff(b2)*weight;
				--weight;
				b1.PixelProp8(a1);
				b2.PixelProp8(a2);
				total += a1.Diff(a2)*weight;
				--weight;
			}
			return total;
		}
		
		public static int[,] LoadImage(string imgf) {
			Console.WriteLine("loading "+imgf);
			if (!File.Exists(imgf))
				throw new Exception("image file "+imgf+" not found");
			if (imgf.EndsWith(".pnm") || imgf.EndsWith(".pbm") || imgf.EndsWith(".pgm") || imgf.EndsWith(".ppm"))
				return LoadPNM(imgf);
			var bmp = new Bitmap(imgf);
			var dat1 = new int[bmp.Width,bmp.Height];
			for (int x = 0; x < bmp.Width; ++x) {
				for (int y = 0; y < bmp.Height; ++y) {
					dat1[x,y] = bmp.GetPixel(x,y).R;
				}
			}
			return dat1;
		}
		
		public static int[,] LoadPNM(string imgf) {
			var textr = new StreamReader(imgf);
			string pnmformat = textr.ReadNextChunkDiscardComments();
			if (pnmformat != "P1" && pnmformat != "P2")
				throw new Exception("PNM format "+pnmformat+" not supported");
			int width = textr.ReadNextChunkDiscardComments().ToInt();
			int height = textr.ReadNextChunkDiscardComments().ToInt();
			int maxval = 0;
			if (pnmformat != "P1")
				maxval = textr.ReadNextChunkDiscardComments().ToInt();
			//maxval = 255; // ignoring value written in file
			if (pnmformat == "P1")
				return LoadPBMData(textr, width, height);
			else if (pnmformat == "P2")
				return LoadPGMData(textr, width, height, maxval);
			throw new Exception("PNM format "+pnmformat+" didn't match supported");
		}
		
		public static int[,] LoadPGMData(StreamReader textr, int width, int height, int maxval) {
			int[,] data = new int[width, height];
			int y = 0;
			int x = 0;
			while (true) {
				string nexts = textr.ReadNextChunkDiscardComments();
				if (nexts == null) break;
				data[x,y] = nexts.ToInt();
				++x;
				if (x >= width) {
					++y;
					x = 0;
				}
			}
			if (y != height || x != 0) {
				throw new Exception("specified dimensions not correct, have ("+x.ToString()+","+y.ToString()+") instead of ("+width.ToString()+","+height.ToString()+")");
			}
			textr.Close();
			return data;
		}
		
		public static int[,] LoadPBMData(StreamReader textr, int width, int height) {
			int[,] data = new int[width, height];
			int y = 0;
			int x = 0;
			while (!textr.EndOfStream) {
				int nextv = textr.Read();
				if (nextv == (int)'0') {
					data[x,y] = 255;
					++x;
					if (x >= width) {
						++y;
						x = 0;
					}
				} else if (nextv == (int)'1') {
					data[x,y] = 0;
					++x;
					if (x >= width) {
						++y;
						x = 0;
					}
				}
			}
			if (y != height || x != 0) {
				throw new Exception("specified dimensions not correct, have ("+x.ToString()+","+y.ToString()+") instead of ("+width.ToString()+","+height.ToString()+")");
			}
			textr.Close();
			return data;
		}
		
		public static void ShowHelp(NDesk.Options.OptionSet p) {
			Console.WriteLine ("Usage: "+Constants.myname+" [OPTIONS]");
			Console.WriteLine ("Options:");
			p.WriteOptionDescriptions (Console.Out);
		}
		
		public static void Main (string[] args) {
			int[,] refimg = null;
			int[,] img1 = null;
			bool show_help = false;
			bool useheuristic = false;
			bool decompose = false;
			bool imgcmp = false;
			string imgcomparer = "sceneparse.FullPixelDiffImageComparer";
			IVisNode[] genos = null;
			int numiter = int.MaxValue;
			var opset = new NDesk.Options.OptionSet() {
				{"r|ref=", "the {REF} image file", (string v) => {
						refimg = LoadImage(v);
						useheuristic = true;
					}},
				{"i|img=", "the {IMAGE} file to load", (string v) => {
						img1 = LoadImage(v);
					}},
				{"g|gen=", "comma,separted {LIST} of objects", (string v) => {
						var objnames = v.Split(',');
						genos = new IVisNode[objnames.Length];
						for (int i = 0; i < objnames.Length; ++i) {
							var nv = objnames[i].DeepCopy();
							if (!nv.Contains(".")) nv = "sceneparse."+nv;
							genos[i] = (IVisNode)Activator.CreateInstance(Type.GetType(nv));
						}
					}},
				{"l|load=", "object {TYPE} to load", (string v) => {
						var nv = v.DeepCopy();
						if (!nv.Contains(".")) nv = "sceneparse."+nv;
						var diri = new DirectoryInfo(v);
						var fil = diri.GetFiles("*.xml");
						genos = new IVisNode[fil.Length];
						for (int i = 0; i < fil.Length; ++i) {
							genos[i] = DeSerializeFromFile(fil[i].FullName);
						}
					}},
				{"d|decompose=", "comma,separted {LIST} of objects", (string v) => {
						var objnames = v.Split(',');
						genos = new IVisNode[objnames.Length];
						for (int i = 0; i < objnames.Length; ++i) {
							var nv = objnames[i].DeepCopy();
							if (!nv.Contains(".")) nv = "sceneparse."+nv;
							genos[i] = (IVisNode)Activator.CreateInstance(Type.GetType(nv));
						}
						numiter = 100;
						decompose = true;
						useheuristic = true;
					}},
				{"t|itr=", "number of {ITERATIONS} to go", (string v) => {
						numiter = int.Parse(v);
					}},
				{"m|comparer=", "the {COMPARER} to use", (string v) => {
						imgcomparer = v.DeepCopy();
						if (!imgcomparer.Contains("ImageComparer")) imgcomparer += "ImageComparer";
						if (!imgcomparer.Contains(".")) imgcomparer = "sceneparse."+imgcomparer;
					}},
				{"topgm=", "png {FILE} to convert", (string v) => {
						LoadImage(v).ToPGM(v.ReplaceExtension("pgm"));
					}},
				{"topbm=", "png {FILE} to convert", (string v) => {
						LoadImage(v).ToPBM(v.ReplaceExtension("pbm"));
					}},
				{"topng=", "pnm {FILE} to convert", (string v) => {
						LoadImage(v).ToPNG(v.ReplaceExtension("png"));
					}},
				{"u|uheu", "use heuristic", (string v) => {
						if (v != null) {
							useheuristic = true;
						}
					}},
				{"c|compare", "compare images", (string v) => {
						if (v != null) {
							imgcmp = true;
						}
					}},
				{"h|help", "show this message and exit", (string v) => {
						show_help = (v != null);
					}},
			};
			List<string> extrargs;
			try {
				extrargs = opset.Parse (args);
			}
			catch (NDesk.Options.OptionException e) {
				Console.Write (Constants.myname+": ");
				Console.WriteLine (e.Message);
				Console.WriteLine ("Try `"+Constants.myname+" --help' for more information.");
				return;
			}
			if (show_help) {
				ShowHelp(opset);
				return;
			}
			if (imgcmp) {
				if (refimg == null) {
					Console.WriteLine("need ref img");
				} else if (img1 == null) {
					Console.WriteLine("need img");
				} else {
					int xout = 0;
					int yout = 0;
					IImageComparer imgc = (IImageComparer)Activator.CreateInstance(Type.GetType(imgcomparer), new object[] {refimg});
					//IImageComparer imgc = new PixelPropImageComparer(refimg);
					//int heuv = 0;
					int heuv = imgc.CompareImg(img1, ref xout, ref yout);
					//int heuv = SlidingImgComp2(refimg, img1, ref xout, ref yout);
					Console.WriteLine(heuv+" at "+xout+","+yout);
				}
			}
			if (decompose) {
				int imgn = 0;
				int[,] fullimg = new int[refimg.Width(),refimg.Height()];
				int[,] supstruct = new int[refimg.Width(),refimg.Height()];
				int[,] rendertarg = new int[refimg.Width(),refimg.Height()];
				int subimgn = 0;
				var search = new SearchAstar<IVisNode>((IVisNode cn) => {
					Console.WriteLine(cn.Describe());
					Console.WriteLine();
					if (rendertarg != null) {
						cn.Data.CopyMatrix(rendertarg, cn.StartX, cn.StartY);
						rendertarg.ToPBM("out"+subimgn+"-"+imgn);
						rendertarg.SetRegion(0, cn.StartX, cn.StartX+cn.Data.Width()-1, cn.StartY, cn.StartY+cn.Data.Height()-1);
					} else {
						cn.Data.ToPBM("out"+subimgn+"-"+imgn);
					}
					cn.SerializeToFile("out"+subimgn+"-"+imgn);
					++imgn;
				});
				int BestHeu = int.MaxValue;
				IVisNode BestNode = null;
				IImageComparer imgc = (IImageComparer)Activator.CreateInstance(Type.GetType(imgcomparer), new object[] {refimg});
				search.FlushNodeCache = imgc.FlushNodeCache;
				search.FullFlushNodeCache = imgc.FullFlushNodeCache;
				search.NodeHeuristic = (IVisNode cn) => {
					//return 0; // disable heuristic
					
					return imgc.CompareImg(cn);
				};
				search.NodeTermination = (IVisNode cn) => {
					if (cn.Heuv < BestHeu) {
						BestHeu = cn.Heuv;
						BestNode = cn;
						search.Lifetime = numiter;
					}
					if (cn.Heuv <= 0) {
						return true;
					}
					Console.WriteLine("current heuv is"+cn.Heuv);
					return false;
				};
				while (true) {
					search.Lifetime = numiter;
					search.AddNewRange(genos);
					search.Run();
					Console.WriteLine("object type is"+BestNode.Name);
					Console.WriteLine("object description is"+BestNode.Describe());
					Console.WriteLine("heuristic value is "+BestNode.Heuv);
					if (rendertarg != null) {
						BestNode.Data.CopyMatrix(rendertarg, BestNode.StartX, BestNode.StartY);
						rendertarg.ToPBM("outresult"+subimgn);
						//rendertarg.SetRegion(0, BestNode.StartX, BestNode.StartX+BestNode.Data.Width()-1, BestNode.StartY, BestNode.StartY+BestNode.Data.Height()-1);
					} else {
						BestNode.Data.ToPBM("outresult"+subimgn);
					}
					BestNode.SerializeToFile("outresult"+subimgn);
					supstruct[BestNode.StartX+BestNode.Width/2, BestNode.StartY+BestNode.Height/2] = 255;
					supstruct.ToPBM("outresult-sup");
					fullimg = fullimg.AddMatrix(rendertarg);
					fullimg.ToPBM("outresult-full");
					imgc = (IImageComparer)Activator.CreateInstance(Type.GetType(imgcomparer), new object[] {refimg, fullimg});
					if (!genos.Contains(BestNode))
						genos = genos.AddResize(BestNode);
					//rendertarg.SetAll(0);
					rendertarg.SetRegion(0, BestNode.StartX, BestNode.StartX+BestNode.Data.Width()-1, BestNode.StartY, BestNode.StartY+BestNode.Data.Height()-1);
					search.Reset();
					++subimgn;
					if (search.BestHeu <= 0)
						break;
				}
			}
			else if (genos != null) {
				int imgn = 0;
				int[,] rendertarg = null;
				if (refimg != null) {
					rendertarg = new int[refimg.Width(),refimg.Height()];
				}
				var search = new SearchAstar<IVisNode>((IVisNode cn) => {
					Console.WriteLine(cn.Describe());
					Console.WriteLine();
					if (rendertarg != null) {
						cn.Data.CopyMatrix(rendertarg, cn.StartX, cn.StartY);
						rendertarg.ToPBM("out"+imgn);
						rendertarg.SetRegion(0, cn.StartX, cn.StartX+cn.Data.Width()-1, cn.StartY, cn.StartY+cn.Data.Height()-1);
					} else {
						cn.Data.ToPBM("out"+imgn);
					}
					cn.SerializeToFile("out"+imgn);
					++imgn;
				});
				int BestHeu = int.MaxValue;
				IVisNode BestNode = null;
				if (useheuristic) {
					IImageComparer imgc = (IImageComparer)Activator.CreateInstance(Type.GetType(imgcomparer), new object[] {refimg});
					search.FlushNodeCache = imgc.FlushNodeCache;
					search.FullFlushNodeCache = imgc.FullFlushNodeCache;
					search.NodeHeuristic = (IVisNode cn) => {
						//return 0; // disable heuristic
						
						return imgc.CompareImg(cn);
					};
					search.NodeTermination = (IVisNode cn) => {
						if (cn.Heuv < BestHeu) {
							BestHeu = cn.Heuv;
							BestNode = cn;
						}
						if (cn.Heuv <= 0) {
							return true;
						}
						Console.WriteLine("current heuv is"+cn.Heuv);
						return false;
					};
				}
				search.Lifetime = numiter;
				search.AddNewRange(genos);
				search.Run();
				if (useheuristic) {
					Console.WriteLine("object type is"+BestNode.Name);
					Console.WriteLine("object description is"+BestNode.Describe());
					Console.WriteLine("heuristic value is "+BestNode.Heuv);
					if (rendertarg != null) {
						BestNode.Data.CopyMatrix(rendertarg, BestNode.StartX, BestNode.StartY);
						rendertarg.ToPBM("outresult");
						rendertarg.SetRegion(0, BestNode.StartX, BestNode.StartX+BestNode.Data.Width()-1, BestNode.StartY, BestNode.StartY+BestNode.Data.Height()-1);
					} else {
						BestNode.Data.ToPBM("outresult");
					}
					BestNode.SerializeToFile("outresult");
				}
			}
		}
	}
}
