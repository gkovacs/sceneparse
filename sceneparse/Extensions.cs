//  
//  Extensions.cs
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
		
		public static int Min(this int[,] a, ref int xout, ref int yout) {
			int xmax = 0;
			int ymax = 0;
			int maxval = a[xmax,ymax];
			for (int x = 0; x < a.Width(); ++x) {
				for (int y = 0; y < a.Height(); ++y) {
					if (a[x,y] < maxval) {
						xmax = x;
						ymax = y;
						maxval = a[x,y];
					}
				}
			}
			xout = xmax;
			yout = ymax;
			return maxval;
		}
		
		public static int Max(this int[,] a, ref int xout, ref int yout) {
			int xmax = 0;
			int ymax = 0;
			int maxval = a[xmax,ymax];
			for (int x = 0; x < a.Width(); ++x) {
				for (int y = 0; y < a.Height(); ++y) {
					if (a[x,y] > maxval) {
						xmax = x;
						ymax = y;
						maxval = a[x,y];
					}
				}
			}
			xout = xmax;
			yout = ymax;
			return maxval;
		}
		
		public static int Diff(this int[,] b1, int[,] b2) {
			if (b1.Width() != b2.Width() || b1.Height() != b2.Height())
				throw new Exception("dimensions don't match");
			int total = 0;
			for (int x = 0; x < b1.Width(); ++x) {
				for (int y = 0; y < b1.Height(); ++y) {
					if (b1[x,y] != b2[x,y]) ++total;
					//total += Math.Abs(b1[x,y]-b2[x,y]);
				}
			}
			return total;
		}
		
		public static int Diff(this int[,] refimg, int[,] simg, int startx, int starty) {
			if (startx+simg.Width() > refimg.Width())
				throw new Exception("Supplied image's x offset too large");
			if (starty+simg.Height() > refimg.Height())
				throw new Exception("Supplied image's x offset too large");
			int total = 0;
			for (int y = 0; y < simg.Height(); ++y) {
				for (int x = 0; x < simg.Width(); ++x) {
					if (refimg[x+startx,y+starty] != simg[x,y]) ++total;
				}
			}
			return total;
		}
		
		public static int Diff(this int[,] refimg, int[,] simg, int startx, int width, int starty, int height) {
			if (width > simg.Width())
				throw new Exception("Comparison width larger than supplied image");
			if (height > simg.Height())
				throw new Exception("Comparison height larger than supplied image");
			if (startx+width > refimg.Width())
				throw new Exception("Supplied image's x offset too large");
			if (starty+height > refimg.Height())
				throw new Exception("Supplied image's x offset too large");
			int total = 0;
			for (int y = 0; y < height; ++y) {
				for (int x = 0; x < width; ++x) {
					if (refimg[x+startx,y+starty] != simg[x,y]) ++total;
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
		
		public static void SetAll<T>(this T[,] v, T val) {
			for (int y = 0; y < v.Height(); ++y) {
				for (int x = 0; x < v.Width(); ++x) {
					v[x,y] = val;
				}
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
		
		public static string ReplaceExtension(this string s, string newext) {
			string outs = "";
			foreach (char x in s) {
				if (x == '.') break;
				outs += x;
			}
			outs += "."+newext;
			return outs;
		}
		
		public static string TrimHash(this string s) {
			string outs = "";
			foreach (char x in s) {
				if (x == '#') break;
				outs += x;
			}
			return outs;
		}
		
		public static bool ContainsNonWhitespace(this string s) {
			foreach (char x in s) {
				if (!char.IsWhiteSpace(x))
					return true;
			}
			return false;
		}
		
		public static bool ReadUntilAfter(this StreamReader textr, char t) {
			while (!textr.EndOfStream) {
				if (textr.Read() == t) {
					return true;
				}
			}
			return false;
		}
		
		public static string ReadNextChunkDiscardComments(this StreamReader textr) {
			char c = ' ';
			while (char.IsWhiteSpace(c)) {
				if (textr.EndOfStream) return null;
				c = (char)textr.Read();
				if (c == '#') {
					if (!textr.ReadUntilAfter('\n')) return null;
					if (textr.EndOfStream) return null;
					c = (char)textr.Read();
				}
			}
			string output = "";
			while (!char.IsWhiteSpace(c)) {
				output += c;
				if (textr.EndOfStream) return output;
				c = (char)textr.Read();
				if (c == '#') {
					textr.ReadUntilAfter('\n');
					return output;
				}
			}
			return output;
		}
		
		public static string ReadLineDiscardComments(this TextReader textr) {
			string outtxt = "";
			while (!outtxt.TrimHash().ContainsNonWhitespace()) {
				outtxt = textr.ReadLine();
				if (outtxt == null) return null;
			}
			return outtxt;
		}
		
		public static int ToInt(this string s) {
			return int.Parse(s);
		}
		
		public static void ToPNG(this int[,] b1, string fn) {
			b1.ToBitmap().Save(fn.ReplaceExtension("png"));
		}
		
		public static void ToPGM(this int[,] v, string fn) {
			var ots = new StreamWriter(fn.ReplaceExtension("pgm"));
			ots.Write(v.ToPGM());
			ots.Close();
		}
		
		public static void ToPBM(this int[,] v, string fn) {
			var ots = new StreamWriter(fn.ReplaceExtension("pbm"));
			ots.Write(v.ToPBM());
			ots.Close();
		}
		
		public static string ToPGM(this int[,] v) {
			string o = "P2\n"+v.Width().ToString()+" "+v.Height().ToString()+"\n255\n";
			for (int y = 0; y < v.Height(); ++y) {
				for (int x = 0; x < v.Width(); ++x) {
					o += v[x,y].ToString().PadLeft(4, ' ');
				}
				o += "\n\n";
			}
			return o;
		}
		
		public static string ToPBM(this int[,] v) {
			string o = "P1\n"+v.Width().ToString()+" "+v.Height().ToString()+"\n";
			for (int y = 0; y < v.Height(); ++y) {
				for (int x = 0; x < v.Width(); ++x) {
					if (v[x,y] > 0) o += "0";
					else o += "1";
				}
				o += "\n";
			}
			return o;
		}
		
		public static T First<T>(this T[] v) {
			return v[0];
		}
		
		public static T Last<T>(this T[] v) {
			return v[v.Length-1];
		}
		
		public static bool MatrixEquals<T>(this T[,] v, T[,] o) where T : IEquatable<T> {
			if (v.Width() != o.Width() || v.Height() != o.Height())
				return false;
			for (int x = 0; x < v.Width(); ++x) {
				for (int y = 0; y < v.Height(); ++y) {
					if (!v[x,y].Equals(o[x,y])) return false;
				}
			}
			return true;
		}
		/*
		public static int MatrixHash<T>(this T[,] v) {
			// FNV-1a hash
			throw new Exception("arbitrary value hashing not implemented");
		}
		*/
		public static int MatrixHash(this int[,] v) {
			// FNV-1a hash
			//unchecked
			//{
				const int p = 16777619;
				uint hash = 2166136261;
			
				for (int y = 0; y < v.Height(); ++y) {
					for (int x = 0; x < v.Width(); ++x) {
						int t = (int)v[x,y];
						byte t0 = (byte)t;
						hash = (hash ^ t0) * p;
						byte t1 = (byte)(t >> 8);
						hash = (hash ^ t1) * p;
						byte t2 = (byte)(t >> 16);
						hash = (hash ^ t2) * p;
						byte t3 = (byte)(t >> 24);
						hash = (hash ^ t3) * p;
					}
				}
			
				hash += hash << 13;
				hash ^= hash >> 7;
				hash += hash << 3;
				hash ^= hash >> 17;
				hash += hash << 5;
				return (int)hash;
			//}
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
		
		public static T[,] PadX<T>(this T[,] v, T[,] dest, int lx, int rx) {
			return v.PadXY(dest,lx,rx,0,0);
		}
		
		public static T[,] PadX<T>(this T[,] v, int lx, int rx) {
			return v.PadXY(lx,rx,0,0);
		}
		
		public static T[,] PadY<T>(this T[,] v, T[,] dest, int uy, int bty) {
			return v.PadXY(dest,0,0,uy,bty);
		}
		
		public static T[,] PadY<T>(this T[,] v, int uy, int bty) {
			return v.PadXY(0,0,uy,bty);
		}
		
		public static T[,] PadXY<T>(this T[,] v, T[,] n, int lx, int rx, int uy, int bty) {
			if (n.Width() != v.Width()+lx+rx)
				throw new Exception("wrong width");
			if (n.Height() != v.Height()+uy+bty)
				throw new Exception("wrong height");
			for (int y = 0; y < v.Height(); ++y) {
				for (int x = 0; x < v.Width(); ++x) {
					n[x+lx,y+uy] = v[x,y];
				}
			}
			return n;
		}
		
		public static T[,] PadXY<T>(this T[,] v, int lx, int rx, int uy, int bty) {
			T[,] n = new T[v.Width()+lx+rx,v.Height()+uy+bty];
			for (int y = 0; y < v.Height(); ++y) {
				for (int x = 0; x < v.Width(); ++x) {
					n[x+lx,y+uy] = v[x,y];
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
		
		public static void SerializeArrays(this IVisNode n) {
			n.SerWidth = n.Data.Width();
			n.SerHeight = n.Data.Height();
			n.SerData = new int[n.SerWidth*n.SerHeight];
			for (int y = 0; y < n.SerHeight; ++y) {
				for (int x = 0; x < n.SerWidth; ++x) {
					n.SerData[x+y*n.SerWidth] = n.Data[x,y];
				}
			}
		}
		
		public static void DeSerializeArrays(this IVisNode n) {
			n.Data = new int[n.SerWidth,n.SerHeight];
			for (int y = 0; y < n.SerHeight; ++y) {
				for (int x = 0; x < n.SerWidth; ++x) {
					n.Data[x,y] = n.SerData[x+y*n.SerWidth];
				}
			}
		}
		
		public static void SerializeToFile(this IVisNode n, string fn) {
			var nfn = fn.DeepCopy();
			if (!nfn.Contains(".xml")) nfn += ".xml";
			XmlSerializer x = new XmlSerializer(n.GetType());
			FileStream fs = new FileStream(nfn, FileMode.Create);
			n.SerializeArrays();
			x.Serialize(fs, n);
			fs.Close();
			n.SerData = null;
		}
				
		public static Bitmap ToBitmap(this int[,] b1) {
			var o = new Bitmap(b1.Width(), b1.Height());
			for (int x = 0; x < b1.Width(); ++x) {
				for (int y = 0; y < b1.Height(); ++y) {
					if (b1[x,y] > 0) {
						o.SetPixel(x,y,Color.White);
					}
					else
						o.SetPixel(x,y,Color.Black);
				}
			}
			return o;
		}
		
		public static void PixelProp8(this int[,] b1, int[,] b2) {
			// topleft corner
			if (b1[0,0] > 0 ||
				b1[1,0] > 0 ||
				b1[0,1] > 0 ||
				b1[1,1] > 0) {
				b2[0,0] = 255;
			}
			// topright corner
			if (b1[b1.LastX(),0] > 0 ||
				b1[b1.LastX()-1,0] > 0 ||
				b1[b1.LastX(),1] > 0 ||
				b1[b1.LastX()-1,1] > 0) {
				b2[b1.LastX(),0] = 255;
			}
			// bottomleft corner
			if (b1[0,b1.LastY()] > 0 ||
				b1[1,b1.LastY()] > 0 ||
				b1[0,b1.LastY()-1] > 0 ||
			    b1[1,b1.LastY()-1] > 0) {
				b2[0,b1.LastY()] = 255;
			}
			// bottomright corner
			if (b1[b1.LastX(),b1.LastY()] > 0 ||
				b1[b1.LastX()-1,b1.LastY()] > 0 ||
				b1[b1.LastX(),b1.LastY()-1] > 0 ||
			    b1[b1.LastX()-1,b1.LastY()-1] > 0) {
				b2[b1.LastX(),b1.LastY()] = 255;
			}
			// left row
			for (int y = 1; y < b1.LastY(); ++y) {
				if (b1[0,y] > 0 ||
					b1[1,y] > 0 ||
					b1[0,y-1] > 0 ||
					b1[0,y+1] > 0 ||
				    b1[1,y+1] > 0 ||
				    b1[1,y-1] > 0) {
					b2[0,y] = 255;
				}
			}
			// right row
			for (int y = 1; y < b1.LastY(); ++y) {
				if (b1[b1.LastX(),y] > 0 ||
					b1[b1.LastX()-1,y] > 0 ||
					b1[b1.LastX(),y-1] > 0 ||
					b1[b1.LastX(),y+1] > 0 ||
				    b1[b1.LastX()-1,y-1] > 0 ||
				    b1[b1.LastX()-1,y+1] > 0) {
					b2[b1.LastX(),y] = 255;
				}
			}
			// top row
			for (int x = 1; x < b1.LastX(); ++x) {
				if (b1[x,0] > 0 ||
					b1[x,1] > 0 ||
					b1[x-1,0] > 0 ||
					b1[x+1,0] > 0 ||
				    b1[x-1,1] > 0 ||
				    b1[x+1,1] > 0) {
					b2[x,0] = 255;
				}
			}
			// bottom row
			for (int x = 1; x < b1.LastX(); ++x) {
				if (b1[x,b1.LastY()] > 0 ||
					b1[x,b1.LastY()-1] > 0 ||
					b1[x-1,b1.LastY()] > 0 ||
					b1[x+1,b1.LastY()] > 0 ||
				    b1[x-1,b1.LastY()-1] > 0 ||
				    b1[x+1,b1.LastY()-1] > 0) {
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
						b1[x+1,y] > 0 ||
					    b1[x-1,y-1] > 0 ||
					    b1[x-1,y+1] > 0 ||
					    b1[x+1,y-1] > 0 ||
					    b1[x+1,y+1] > 0) {
						b2[x,y] = 255;
					}
				}
			}
		}
		
		public static void PixelProp8Exp(this int[,] b1, int[,] b2) {
			// topleft corner
			if (b1[0,0] > 0 ||
				b1[1,0] > 0 ||
				b1[0,1] > 0 ||
				b1[1,1] > 0) {
				b2[1,1] = 255;
			}
			// topright corner
			if (b1[b1.LastX(),0] > 0 ||
				b1[b1.LastX()-1,0] > 0 ||
				b1[b1.LastX(),1] > 0 ||
				b1[b1.LastX()-1,1] > 0) {
				b2[b1.LastX()+1,1] = 255;
			}
			// bottomleft corner
			if (b1[0,b1.LastY()] > 0 ||
				b1[1,b1.LastY()] > 0 ||
				b1[0,b1.LastY()-1] > 0 ||
			    b1[1,b1.LastY()-1] > 0) {
				b2[1,b1.LastY()+1] = 255;
			}
			// bottomright corner
			if (b1[b1.LastX(),b1.LastY()] > 0 ||
				b1[b1.LastX()-1,b1.LastY()] > 0 ||
				b1[b1.LastX(),b1.LastY()-1] > 0 ||
			    b1[b1.LastX()-1,b1.LastY()-1] > 0) {
				b2[b1.LastX()+1,b1.LastY()+1] = 255;
			}
			// left row
			for (int y = 1; y < b1.LastY(); ++y) {
				if (b1[0,y] > 0 ||
					b1[1,y] > 0 ||
					b1[0,y-1] > 0 ||
					b1[0,y+1] > 0 ||
				    b1[1,y+1] > 0 ||
				    b1[1,y-1] > 0) {
					b2[1,y+1] = 255;
				}
			}
			// right row
			for (int y = 1; y < b1.LastY(); ++y) {
				if (b1[b1.LastX(),y] > 0 ||
					b1[b1.LastX()-1,y] > 0 ||
					b1[b1.LastX(),y-1] > 0 ||
					b1[b1.LastX(),y+1] > 0 ||
				    b1[b1.LastX()-1,y-1] > 0 ||
				    b1[b1.LastX()-1,y+1] > 0) {
					b2[b1.LastX()+1,y+1] = 255;
				}
			}
			// top row
			for (int x = 1; x < b1.LastX(); ++x) {
				if (b1[x,0] > 0 ||
					b1[x,1] > 0 ||
					b1[x-1,0] > 0 ||
					b1[x+1,0] > 0 ||
				    b1[x-1,1] > 0 ||
				    b1[x+1,1] > 0) {
					b2[x+1,1] = 255;
				}
			}
			// bottom row
			for (int x = 1; x < b1.LastX(); ++x) {
				if (b1[x,b1.LastY()] > 0 ||
					b1[x,b1.LastY()-1] > 0 ||
					b1[x-1,b1.LastY()] > 0 ||
					b1[x+1,b1.LastY()] > 0 ||
				    b1[x-1,b1.LastY()-1] > 0 ||
				    b1[x+1,b1.LastY()-1] > 0) {
					b2[x+1,b1.LastY()+1] = 255;
				}
			}
			// center
			for (int x = 2; x < b1.LastX()-1; ++x) {
				for (int y = 2; y < b1.LastY()-1; ++y) {
					if (b1[x,y] > 0 ||
						b1[x,y-1] > 0 ||
						b1[x,y+1] > 0 ||
						b1[x-1,y] > 0 ||
						b1[x+1,y] > 0 ||
					    b1[x-1,y-1] > 0 ||
					    b1[x-1,y+1] > 0 ||
					    b1[x+1,y-1] > 0 ||
					    b1[x+1,y+1] > 0) {
						b2[x+1,y+1] = 255;
					}
				}
			}
			// left edge
			for (int y = 0; y < b1.Height(); ++y) {
				if (b1[0,y] > 0)
					b2[0,y+1] = 255;
			}
			// right edge
			for (int y = 0; y < b1.Height(); ++y) {
				if (b1[b1.LastX(),y] > 0)
					b2[b1.LastX()+2,y+1] = 255;
			}
			// top edge
			for (int x = 0; x < b1.Width(); ++x) {
				if (b1[x,0] > 0)
					b2[x+1,0] = 255;
			}
			// bottom edge
			for (int x = 0; x < b1.Width(); ++x) {
				if (b1[x,b1.LastY()] > 0)
					b2[x+1,b1.LastY()+2] = 255;
			}
			// topleft corner
			if (b1[0,0] > 0) b2[0,0] = 255;
			// topright corner
			if (b1[b1.LastX(),0] > 0) b2[b1.LastX()+2,0] = 255;
			// bottomleft corner
			if (b1[0,b1.LastY()] > 0) b2[0,b1.LastY()+2] = 255;
			// bottomright corner
			if (b1[b1.LastX(),b1.LastY()] > 0) b2[b1.LastX()+2,b1.LastY()+2] = 255;
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
}
