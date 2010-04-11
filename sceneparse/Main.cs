// 
//  Main.cs
//  
//  Author:
//	   Geza Kovacs <gkovacs@mit.edu>
// 
//  Copyright (c) 2010 Geza Kovacs
// 
//  This library is free software; you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as
//  published by the Free Software Foundation; either version 2.1 of the
//  License, or (at your option) any later version.
// 
//  This library is distributed in the hope that it will be useful, but
//  WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//  Lesser General Public License for more details.
// 
//  You should have received a copy of the GNU Lesser General Public
//  License along with this library; if not, write to the Free Software
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;

namespace sceneparse {
	
	public class Constants {
		public static readonly string myname = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
		public static readonly int[,] m0 = {{1}};
		public const int v0 = 1;
		public static readonly int[,] m1 = {{1,1,1},{1,1,1},{1,1,1}};
		public const int v1 = 9;
		public static readonly int[,] m2 = {{1,1,1,1,1},{1,1,1,1,1},{1,1,1,1,1},{1,1,1,1,1},{1,1,1,1,1}};
		public const int v2 = 25;
		public static readonly int[,] m3 = {{1,1,1,1,1,1,1},{1,1,1,1,1,1,1},{1,1,1,1,1,1,1},{1,1,1,1,1,1,1},{1,1,1,1,1,1,1},{1,1,1,1,1,1,1},{1,1,1,1,1,1,1}};
		public const int v3 = 49;
		public static readonly int[,] m4 = {{1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1}};
		public const int v4 = 81;
		public static readonly int[][,] ml = {m0,m1,m2,m3,m4};
		public static readonly int[] vl = {v0,v1,v2,v3,v4};
	}
	
	public static class Extensions {
		
		public static int Width<T>(this T[,] a) {
			return a.GetLength(0);
		}
		
		public static int LastX<T>(this T[,] a) {
			return a.GetLength(0)-1;
		}
		
		public static int Height<T>(this T[,] a) {
			return a.GetLength(1);
		}
		
		public static int LastY<T>(this T[,] a) {
			return a.GetLength(1)-1;
		}
		
		public static int Diff(this int[,] b1, int[,] b2) {
			int total = 0;
			for (int x = 0; x < b1.Width(); ++x) {
				for (int y = 0; y < b1.Height(); ++y) {
					if (b1[x,y] != b2[x,y]) ++total;
					//total += Math.Abs(b1[x,y]-b2[x,y]);
				}
			}
			return total;
		}
		
		public static void Extend<T>(this Queue<T> v, IEnumerable<T> n) {
			foreach (T x in n) {
				v.Enqueue(x);
			}
		}
		
		public static void Extend<T>(this C5.IExtensible<T> v, IEnumerable<T> n) {
			foreach (T x in n) {
				v.Add(x);
			}
		}
		
		public static void SetRow<T>(this T[,] v, int num, T val) {
			for (int x = 0; x < v.Width(); ++x) {
				v[x,num] = val;
			}
		}
		
		public static void SetColumn<T>(this T[,] v, int num, T val) {
			for (int y = 0; y < v.Height(); ++y) {
				v[num,y] = val;
			}
		}
		
		public static void CopyRow<T>(this T[,] v, T[,] o, int yi) {
			v.CopyRow(o, yi, yi);
		}
		
		public static void CopyRow<T>(this T[,] v, T[,] o, int yi, int yo) {
			if (v.Width() != o.Width()) throw new Exception("not same width");
			for (int x = 0; x < v.Width(); ++x) {
				o[x,yo] = v[x,yi];
			}
		}
		
		public static void CopyColumn<T>(this T[,] v, T[,] o, int xi) {
			v.CopyColumn(o, xi, xi);
		}
		
		public static void CopyColumn<T>(this T[,] v, T[,] o, int xi, int xo) {
			if (v.Height() != o.Height()) throw new Exception("not same height");
			for (int y = 0; y < v.Height(); ++y) {
				o[xo,y] = v[xi,y];
			}
		}
		
		public static string MkString(this int[,] v) {
			string o = "\n";
			for (int y = 0; y < v.Height(); ++y) {
				for (int x = 0; x < v.Width(); ++x) {
					o += v[x,y].ToString()+", ";
				}
				o += "\n";
			}
			return o;
		}
		
		public static T[,] SliceX<T>(this T[,] v, int xs, int xe) {
			T[,] n = new T[xe-xs, v.Height()];
			for (int x = 0; x < xe-xs; ++x) {
				v.CopyColumn(n, x+xs, x);
			}
			return n;
		}
		
		public static T[,] SliceY<T>(this T[,] v, int ys, int ye) {
			T[,] n = new T[v.Width(), ye-ys];
			for (int y = 0; y < ye-ys; ++y) {
				v.CopyRow(n, y+ys, y);
			}
			return n;
		}
		
		public static T[,] SliceXY<T>(this T[,] v, int xs, int xe, int ys, int ye) {
			if (ys >= ye || xs >= xe) return new T[0,0];
			T[,] n = new T[xe-xs, ye-ys];
			for (int y = 0; y < ye-ys; ++y) {
				for (int x = 0; x < xe-xs; ++x) {
					n[x,y] = v[x+xs,y+ys];
				}
			}
			return n;
		}
		
		public static T[,] AddLeftColumn<T>(this T[,] v) {
			return v.AddLeftColumn(1);
		}
		
		public static T[,] AddLeftColumn<T>(this T[,] v, int num) {
			T[,] n = new T[v.Width()+num,v.Height()];
			for (int x = 0; x < v.Width(); ++x) {
				for (int y = 0; y < v.Height(); ++y) {
					n[x+num,y] = v[x,y];
				}
			}
			return n;
		}
		
		public static T[,] AddRightColumn<T>(this T[,] v) {
			return v.AddRightColumn(1);
		}
		
		public static T[,] AddRightColumn<T>(this T[,] v, int num) {
			T[,] n = new T[v.Width()+num,v.Height()];
			for (int x = 0; x < v.Width(); ++x) {
				for (int y = 0; y < v.Height(); ++y) {
					n[x,y] = v[x,y];
				}
			}
			return n;
		}
		
		public static T[,] AddTopRow<T>(this T[,] v) {
			return v.AddTopRow(1);
		}
		
		public static T[,] AddTopRow<T>(this T[,] v, int num) {
			T[,] n = new T[v.Width(),v.Height()+num];
			for (int x = 0; x < v.Width(); ++x) {
				for (int y = 0; y < v.Height(); ++y) {
					n[x,y+num] = v[x,y];
				}
			}
			return n;
		}
		
		public static T[,] AddBottomRow<T>(this T[,] v) {
			return v.AddBottomRow(1);
		}
		
		public static T[,] AddBottomRow<T>(this T[,] v, int num) {
			T[,] n = new T[v.Width(),v.Height()+num];
			for (int x = 0; x < v.Width(); ++x) {
				for (int y = 0; y < v.Height(); ++y) {
					n[x,y] = v[x,y];
				}
			}
			return n;
		}
		
		public static T[,] AddTopBottomRows<T>(this T[,] v) {
			return v.AddTopBottomRows(1);
		}
		
		public static T[,] AddTopBottomRows<T>(this T[,] v, int num) {
			T[,] n = new T[v.Width(),v.Height()+2*num];
			for (int x = 0; x < v.Width(); ++x) {
				for (int y = 0; y < v.Height(); ++y) {
					n[x,y+num] = v[x,y];
				}
			}
			return n;
		}
		
		public static T[,] AddLeftRightColumns<T>(this T[,] v) {
			return v.AddLeftRightColumns(1);
		}
		
		public static T[,] AddLeftRightColumns<T>(this T[,] v, int num) {
			T[,] n = new T[v.Width()+2*num,v.Height()];
			for (int x = 0; x < v.Width(); ++x) {
				for (int y = 0; y < v.Height(); ++y) {
					n[x+num,y] = v[x,y];
				}
			}
			return n;
		}
		
		public static T[,] AddAllSidesRowsColumns<T>(this T[,] v) {
			return v.AddAllSidesRowsColumns(1);
		}
		
		public static T[,] AddAllSidesRowsColumns<T>(this T[,] v, int num) {
			T[,] n = new T[v.Width()+2*num,v.Height()+2*num];
			for (int x = 0; x < v.Width(); ++x) {
				for (int y = 0; y < v.Height(); ++y) {
					n[x+num,y+num] = v[x,y];
				}
			}
			return n;
		}
		
		public static T[,] DeepCopy<T>(this T[,] b1) {
			T[,] b2 = new T[b1.Width(),b1.Height()];
			for (int x = 0; x < b1.Width(); ++x) {
				for (int y = 0; y < b1.Height(); ++y) {
					b2[x,y] = b1[x,y];
				}
			}
			return b2;
		}
		
		public static T[] DeepCopy<T>(this T[] b1) {
			T[] b2 = new T[b1.Length];
			for (int x = 0; x < b1.Length; ++x) {
				b2[x] = b1[x];
			}
			return b2;
		}
		
		public static string DeepCopy(this string n) {
			return string.Copy(n);
		}
		
		public static Bitmap ToBitmap(this int[,] b1) {
			var o = new Bitmap(b1.Width(), b1.Height());
			for (int x = 0; x < b1.Width(); ++x) {
				for (int y = 0; y < b1.Width(); ++y) {
					if (b1[x,y] > 0)
						o.SetPixel(x,y,Color.Red);
					else
						o.SetPixel(x,y,Color.Black);
				}
			}
			return o;
		}
		
		public static void ToPNG(this int[,] b1, string fn) {
			b1.ToBitmap().Save(fn+".png");
		}
		
		public static void PixelProp(this int[,] b1, int[,] b2) {
			// topleft corner
			if (b1[0,0] > 0 ||
				b1[1,0] > 0 ||
				b1[0,1] > 0) {
				b2[0,0] = 255;
			}
			// topright corner
			if (b1[b1.LastX(),0] > 0 ||
				b1[b1.LastX()-1,0] > 0 ||
				b1[b1.LastX(),1] > 0) {
				b2[b1.LastX(),0] = 255;
			}
			// bottomleft corner
			if (b1[0,b1.LastY()] > 0 ||
				b1[1,b1.LastY()] > 0 ||
				b1[0,b1.LastY()-1] > 0) {
				b2[0,b1.LastY()] = 255;
			}
			// bottomright corner
			if (b1[b1.LastX(),b1.LastY()] > 0 ||
				b1[b1.LastX()-1,b1.LastY()] > 0 ||
				b1[b1.LastX(),b1.LastY()-1] > 0) {
				b2[b1.LastX(),b1.LastY()] = 255;
			}
			// left row
			for (int y = 1; y < b1.LastY(); ++y) {
				if (b1[0,y] > 0 ||
					b1[1,y] > 0 ||
					b1[0,y-1] > 0 ||
					b1[0,y+1] > 0) {
					b2[0,y] = 255;
				}
			}
			// right row
			for (int y = 1; y < b1.LastY(); ++y) {
				if (b1[b1.LastX(),y] > 0 ||
					b1[b1.LastX()-1,y] > 0 ||
					b1[b1.LastX(),y-1] > 0 ||
					b1[b1.LastX(),y+1] > 0) {
					b2[b1.LastX(),y] = 255;
				}
			}
			// top row
			for (int x = 1; x < b1.LastX(); ++x) {
				if (b1[x,0] > 0 ||
					b1[x,1] > 0 ||
					b1[x-1,0] > 0 ||
					b1[x+1,0] > 0) {
					b2[x,0] = 255;
				}
			}
			// bottom row
			for (int x = 1; x < b1.LastX(); ++x) {
				if (b1[x,b1.LastY()] > 0 ||
					b1[x,b1.LastY()-1] > 0 ||
					b1[x-1,b1.LastY()] > 0 ||
					b1[x+1,b1.LastY()] > 0) {
					b2[x,b1.LastY()] = 255;
				}
			}
			// center
			for (int x = 2; x < b1.LastX()-1; ++x) {
				for (int y = 2; y < b1.LastY()-1; ++y) {
					if (b1[x,y] > 0 ||
						b1[x,y-1] > 0 ||
						b1[x,y+1] > 0 ||
						b1[x-1,y] > 0 ||
						b1[x+1,y] > 0) {
						b2[x,y] = 255;
					}
				}
			}
		}
	}
	
	public delegate IVisNode VisTrans(IVisNode n);
	public delegate int VisTransCost();

	public interface IDeepCopyable<T> {
		T DeepCopy();
		T DeepCopyNoData();
	}
	
	public interface IVisNode : IComparable<IVisNode>, IDeepCopyable<IVisNode> {
		int Width {get;}
		int Height {get;}
		string Name {get; set;}
		int Cost {get; set;}
		int Heuv {get; set;}
		int HeuvCost {get;}
		int MaxCost {get; set;}
		int[,] Data {get; set;}
		VisTrans[] Transforms {get; set;}
		//VisTransCost[] TCosts {get; set;}
		int[] TCostCons {get; set;}
		//int[,] Render();
		void Initialize();
		IVisNode[] Next {get;}
	}
	
	public class BaseVisNode : IVisNode {
		public int Width {get {return this.Data.Width();} }
		public int Height {get {return this.Data.Height();} }
		public string Name {get; set;}
		public int Cost {get; set;}
		public int Heuv {get; set;}
		public int HeuvCost {get {return Cost + Heuv;} }
		public int MaxCost {get; set;}
		public int[,] Data {get; set;}
		public VisTrans[] Transforms {get; set;}
		//public VisTransCost[] TCosts {get; set;}
		public int[] TCostCons {get; set;}
		//public virtual int[,] Render () {return null;}
		public virtual void Initialize() {}
		
		public IVisNode[] Next {get {
				var rv = new IVisNode[Transforms.Length];
				for (int x = 0; x < Transforms.Length; ++x) {
					rv[x] = this.Transforms[x](this);
					Console.WriteLine("rvx is"+rv[x].Cost);
				}
				return rv;
			} }
		
		public int CompareTo(IVisNode o) {
			return this.Cost.CompareTo(o.Cost);
		}
		
		public IVisNode DeepCopy() {
			IVisNode n = this.DeepCopyNoData();
			n.Data = this.Data.DeepCopy();
			return n;
		}
		
		public IVisNode DeepCopyNoData() {
			IVisNode n = (IVisNode)this.MemberwiseClone();//MakeNew<T>();
			//n.Initialize();
			n.Name = this.Name.DeepCopy();
			n.Data = null;
			n.Cost = this.Cost;
			n.Heuv = this.Heuv; // not necessary, heuv computed externally
			n.MaxCost = this.MaxCost;
			n.Transforms = this.Transforms.DeepCopy();
			//n.TCosts = o.TCosts.DeepCopy();
			n.TCostCons = this.TCostCons.DeepCopy();
			return n;
		}
	}
	
	public class SquareN : BaseVisNode {
		public static IVisNode ExpandOut(IVisNode ths) {
			var n = ths.DeepCopyNoData();
			n.Data = ths.Data.AddAllSidesRowsColumns();
			n.Data.SetRow(0, 255);
			n.Data.SetRow(n.Data.LastY(), 255);
			n.Data.SetColumn(0, 255);
			n.Data.SetColumn(n.Data.LastX(), 255);
			n.Cost += ths.TCostCons[0];
			Console.WriteLine("cost is "+n.Cost);
			Console.WriteLine("data width is "+n.Width);
			return n;
		}
		public static IVisNode ContractIn(IVisNode ths) {
			var n = ths.DeepCopyNoData();
			n.Data = ths.Data.SliceXY(1,ths.Data.LastX(),1,ths.Data.LastY());
			n.Data.SetRow(0, 255);
			n.Data.SetRow(n.Data.LastY(), 255);
			n.Data.SetColumn(0, 255);
			n.Data.SetColumn(n.Data.LastX(), 255);
			n.Cost += ths.TCostCons[0];
			if (n.Width < 1) n.Cost = n.MaxCost+1;
			Console.WriteLine("cost is "+n.Cost);
			Console.WriteLine("data width is "+n.Width);
			return n;
		}
		public override void Initialize() {
			Name = "SquareN";
			Data = new int[3,3] {{255,255,255},{255,255,255},{255,255,255}};
			MaxCost = 10;
			TCostCons = new int[] {1};
			Transforms = new VisTrans[] {
				ExpandOut,
				ContractIn,
				/*
				(ths) => {
					var n = ths.DeepCopyNoData();
					Console.WriteLine(ths.GetHashCode());
					Console.WriteLine(n.GetHashCode());
					n.transnum = ths.transnum+"a";
					Console.WriteLine("transform called "+n.transnum);
					n.Data = ths.Data.AddAllSidesRowsColumns();
					n.Cost += 1;//this.TCostCons[0];
					Console.WriteLine("cost is "+n.Cost);
					return n;
				},*/
			};
			//TCosts = new VisTransCost[] {
			//	() => {return TCostCons[0];}
			//};
		}
	}
	/*
	public class TowerN : BaseVisNode {
		public static IVisNode GrowUp(IVisNode ths) {
			var n = ths.DeepCopyNoData();
			n.Data = ths.Data.AddAllSidesRowsColumns();
			n.Data.SetRow(0, 255);
			n.Data.SetRow(n.Data.LastY(), 255);
			n.Data.SetColumn(0, 255);
			n.Data.SetColumn(n.Data.LastX(), 255);
			n.Cost += ths.TCostCons[0];
			Console.WriteLine("cost is "+n.Cost);
			Console.WriteLine("data width is "+n.Width);
			return n;
		}
		public override void Initialize() {
			Name = "TowerN";
			TCostCons = new int[] {1};
			Transforms = new VisTrans[] {
				GrowUp,
			};
			Data = new int[1,1] {{255}};
		}
	}*/

	public class MainClass {
		
		public static int CompImages(int[,] o1, int[,] o2) {
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
			int numiter = 0;
			var opset = new NDesk.Options.OptionSet() {
				{"r|ref=", "the {REF} image file", v => {
						refimg = LoadImage(v);
					}},
				{"i|img=", "the {IMAGE} file to load", v => {
						img1 = LoadImage(v);
					}},
				{"t|itr=", "number of {ITERATIONS} to go", v => {
						numiter = Int32.Parse(v);
					}},
				{"s|ser=", "object {TYPE} to serialize", v => {
						if (v.Contains("."))
							geno = (IVisNode)Activator.CreateInstance(Type.GetType(v));
						else
							geno = (IVisNode)Activator.CreateInstance(Type.GetType("sceneparse."+v));
					}},
				{"g|gen=", "object {TYPE} to generate", v => {
						if (v.Contains("."))
							geno = (IVisNode)Activator.CreateInstance(Type.GetType(v));
						else
							geno = (IVisNode)Activator.CreateInstance(Type.GetType("sceneparse."+v));
					}},
				{"c|compare", "compare images", v => {
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
				{"h|help", "show this message and exit", v => {
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
			if (geno != null) {
				geno.Initialize();
				var agenda = new C5.IntervalHeap<IVisNode>();
				//var agenda = new Queue<IVisNode>();
				//agenda.Enqueue(geno);
				agenda.Add(geno);
				for (int i = 0; i < numiter; ++i) {
					if (agenda.IsEmpty) break;
					//if (agenda.Count < 1) break;
					var cn = agenda.DeleteMin();
					if (cn.Cost > cn.MaxCost) continue;
					//var cn = agenda.Dequeue();
					Console.WriteLine(cn.Name);
					Console.WriteLine(cn.Cost);
					cn.Data.ToPNG("out"+i);
					agenda.Extend(cn.Next);
				}
			}
		}
	}
}
