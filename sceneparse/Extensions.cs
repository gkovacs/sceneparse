// 
//  Extensions.cs
//  
//  Author:
//       Geza Kovacs <gkovacs@mit.edu>
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
}
