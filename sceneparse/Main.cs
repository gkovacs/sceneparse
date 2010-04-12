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
using System.Xml.Serialization;

namespace sceneparse
{
	public class MainClass {
		
		public static IVisNode DeSerializeFromFile(string fn, string typen) {
			return DeSerializeFromFile(fn, Type.GetType(typen));
		}
		
		public static IVisNode DeSerializeFromFile(string fn, System.Type typen) {
			XmlSerializer x = new XmlSerializer(typen);
			FileStream fs = new FileStream(fn, FileMode.Open);
			var n = (IVisNode)x.Deserialize(fs);
			fs.Close();
			n.DeSerializeArrays();
			n.SerData = null;
			return n;
		}
		
		public static int CompImages(int[,] o1, int[,] o2) {
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
			for (int i = 0; i < 10; ++i) {
				a1.PixelProp(b1);
				a2.PixelProp(b2);
				total += b1.Diff(b2)*weight;
				--weight;
				b1.PixelProp(a1);
				b2.PixelProp(a2);
				total += a1.Diff(a2)*weight;
				--weight;
			}
			return total;
		}
		
		public static int[,] LoadImage(string imgf) {
			Console.WriteLine("loading "+imgf);
			var bmp = new Bitmap(imgf);
			var dat1 = new int[bmp.Width,bmp.Height];
			for (int x = 0; x < bmp.Width; ++x) {
				for (int y = 0; y < bmp.Height; ++y) {
					dat1[x,y] = bmp.GetPixel(x,y).R;
				}
			}
			return dat1;
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
			IVisNode geno = null;
			IVisNode[] genos = null;
			int numiter = int.MaxValue;
			var opset = new NDesk.Options.OptionSet() {
				{"r|ref=", "the {REF} image file", (string v) => {
						refimg = LoadImage(v);
					}},
				{"i|img=", "the {IMAGE} file to load", (string v) => {
						img1 = LoadImage(v);
					}},
				{"t|itr=", "number of {ITERATIONS} to go", (string v) => {
						numiter = int.Parse(v);
					}},
				{"l|load=", "object {TYPE} to load", (string v) => {
						var nv = v.DeepCopy();
						if (!nv.Contains(".")) nv = "sceneparse."+nv;
						var diri = new DirectoryInfo(v);
						var fil = diri.GetFiles("*.xml");
						genos = new IVisNode[fil.Length];
						for (int i = 0; i < fil.Length; ++i) {
							genos[i] = DeSerializeFromFile(fil[i].FullName, nv);
						}
					}},
				{"g|gen=", "object {TYPE} to generate", (string v) => {
						var nv = v.DeepCopy();
						if (!nv.Contains(".")) nv = "sceneparse."+nv;
						geno = (IVisNode)Activator.CreateInstance(Type.GetType(nv));
					}},
				{"c|compare", "compare images", (string v) => {
						if (v != null) {
							if (refimg == null) {
								Console.WriteLine("need ref img");
							} else if (img1 == null) {
								Console.WriteLine("need img");
							} else {
								Console.WriteLine(CompImages(refimg, img1));
							}
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
			if (genos != null) {
				int imgn = 0;
				var search = new SearchAstar((IVisNode cn) => {
					Console.WriteLine(cn.Describe());
					Console.WriteLine();
					cn.Data.ToPNG("out"+imgn);
					cn.SerializeToFile("out"+imgn);
					++imgn;
				});
				search.Lifetime = numiter;
				search.Extend(genos);
				search.Run();
			}
			if (geno != null) {
				//geno.Initialize();
				int imgn = 0;
				var search = new SearchAstar((IVisNode cn) => {
					Console.WriteLine(cn.Describe());
					Console.WriteLine();
					cn.Data.ToPNG("out"+imgn);
					cn.SerializeToFile("out"+imgn);
					++imgn;
				});
				search.Lifetime = numiter;
				search.Add(geno);
				search.Run();
			}
		}
	}
}
