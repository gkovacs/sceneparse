//  
//  ImageComparer.cs
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

namespace sceneparse
{
	public interface IImageComparer {
		int CompareImg(int[,] simg, ref int xout, ref int yout);
		int CompareImg(IVisNode cn);
		int[,] RefImg {get; set;}
		int[,] BaseImg {get; set;}
		NodeActionDelegate FlushNodeCache {get;}
		NodeActionDelegate FullFlushNodeCache {get;}
	}
	
	public abstract class BaseImageComparer : IImageComparer {
		public int[,] RefImg {get; set;}
		public int[,] BaseImg {get; set;}
		public virtual int CompareImg(int[,] simg, ref int xout, ref int yout) {return 0;}
		public virtual int CompareImg(IVisNode cn) {
			int tx = 0;
			int ty = 0;
			int outv = CompareImg(cn.Data, ref tx, ref ty);
			cn.StartX = tx;
			cn.StartY = ty;
			return outv;
		}
		public virtual NodeActionDelegate FlushNodeCache {
			get {return (IVisNode cn) => {};}
		}
		public virtual NodeActionDelegate FullFlushNodeCache {
			get {return FlushNodeCache;}
		}
	}
	
	public class SlidingPixelDiffImageComparer : BaseImageComparer {
		
		public SlidingPixelDiffImageComparer(int[,] refi, int[,] basei) {
			if (refi.Width() != basei.Width())
				throw new Exception("Reference and Base image width mismatch");
			if (refi.Height() != basei.Height())
				throw new Exception("Reference and Base image height mismatch");
			RefImg = refi;
			BaseImg = basei;
		}
		
		public SlidingPixelDiffImageComparer(int[,] refi)
			: this(refi, new int[refi.Width(), refi.Height()]) {}
		
		public override int CompareImg(int[,] simg, ref int xout, ref int yout) {
			int rsdiffheight = RefImg.Height()-simg.Height()+1;
			int rsdiffwidth = RefImg.Width()-simg.Width()+1;
			int[,] total = new int[rsdiffwidth,rsdiffheight];
			for (int y = 0; y < rsdiffheight; ++y) {
				for (int x = 0; x < rsdiffwidth; ++x) {
					total[x,y] = RefImg.Diff(simg, x, y);
				}
			}
			return total.Min(ref xout, ref yout);
		}
	}

	public class PixelPropImageComparer : BaseImageComparer
	{
		public int[] BaseRefDiff;
		public int[][,] RefImgProp;
		public int[][,] BaseImgProp;
		public int PropDepth = 5;

		public PixelPropImageComparer(int[,] refi, int[,] basei) {
			if (refi.Width() != basei.Width())
				throw new Exception("Reference and Base image width mismatch");
			if (refi.Height() != basei.Height())
				throw new Exception("Reference and Base image height mismatch");
			RefImg = refi;
			BaseImg = basei;
			BaseRefDiff = new int[PropDepth];
			BaseImgProp = new int[PropDepth][,];
			RefImgProp = new int[PropDepth][,];
			RefImgProp[0] = RefImg.PadXY(PropDepth, PropDepth, PropDepth, PropDepth);
			BaseImgProp[0] = BaseImg.PadXY(PropDepth, PropDepth, PropDepth, PropDepth);
			BaseRefDiff[0] = RefImgProp[0].Diff(BaseImgProp[0]);
			int imgwidth = RefImgProp[0].Width();
			int imgheight = RefImgProp[0].Height();
			for (int i = 0; i < PropDepth-2; ++i) {
				RefImgProp[i+1] = new int[imgwidth, imgheight];
				BaseImgProp[i+1] = new int[imgwidth, imgheight];
				RefImgProp[i].PixelProp8(RefImgProp[i+1]);
				BaseImgProp[i].PixelProp8(BaseImgProp[i+1]);
				BaseRefDiff[i+1] = RefImgProp[i+1].Diff(BaseImgProp[i+1]);
			}
		}
		
		public PixelPropImageComparer(int[,] refi)
			: this(refi, new int[refi.Width(), refi.Height()]) {}
		
		public int BaseRefDiffRange(int scalelev, int[,] simg, int startx, int starty) {
			return BaseRefDiffRange(scalelev, startx, startx+simg.Width(), starty, starty+simg.Height());
		}
		
		public int BaseRefDiffRange(int scalelev, int startx, int endx, int starty, int endy) {
			int total = 0;
			for (int y = starty; y < endy; ++y) {
				for (int x = startx; x < endx; ++x) {
					if (RefImgProp[scalelev][x,y] != BaseImgProp[scalelev][x,y])
						++total;
				}
			}
			return total;
		}
		
		public int[,] CompareImgAllCoords(int[,] simg) {
			int rsheightdiff = RefImg.Height()-simg.Height()+1;
			int rswidthdiff = RefImg.Width()-simg.Width()+1;
			int[,] total = new int[rswidthdiff,rsheightdiff];
			if (rsheightdiff <= 0 || rswidthdiff <= 0) {
				total.SetAll(int.MaxValue);
				return total;
			}
			var SImgProp = new int[PropDepth][,];
			SImgProp[0] = simg.PadXY(PropDepth, PropDepth, PropDepth, PropDepth);
			int imgwidth = SImgProp[0].Width();
			int imgheight = SImgProp[0].Height();
			for (int i = 0; i < PropDepth-2; ++i) {
				SImgProp[i+1] = new int[imgwidth, imgheight];
				SImgProp[i].PixelProp8(SImgProp[i+1]);
			}
			int weight = 3*(PropDepth-1)/2;
			for (int i = 0; i < PropDepth-1; ++i) {
				for (int y = 0; y < rsheightdiff; ++y) {
					for (int x = 0; x < rswidthdiff; ++x) {
						int loctot = 0;
						loctot += BaseRefDiff[i]-BaseRefDiffRange(i, x+PropDepth-i, x+PropDepth+i+simg.Width(), y+PropDepth-i, y+PropDepth+i+simg.Height());
						for (int ny = PropDepth-i; ny < PropDepth+i+simg.Height(); ++ny) {
							for (int nx = PropDepth-i; nx < PropDepth+i+simg.Width(); ++nx) {
								if (SImgProp[i][nx,ny] != RefImgProp[i][x+nx,y+ny]) ++loctot;
							}
						}
						total[x,y] = loctot*weight;
					}
				}
				--weight;
			}
			return total;
		}
		
		public override int CompareImg(int[,] simg, ref int xout, ref int yout) {
			return CompareImgAllCoords(simg).Min(ref xout, ref yout);
		}
	}
	
	public class CachedPixelPropImageComparer : PixelPropImageComparer {
		//public int[] xcoordsloc;
		//public int[] ycoordsloc;
		//public int[] minvals = new int[numcoords];
		public const int numcoords = 50;
		//public bool minvalsvalid = false;
		
		public CachedPixelPropImageComparer(int[,] refi, int[,] basei)
			: base(refi, basei) {}
		
		public CachedPixelPropImageComparer(int[,] refi)
			: base(refi) {}
		
		public override NodeActionDelegate FlushNodeCache {
			get {return (IVisNode cn) => {
					Console.WriteLine("flushing new node with heuv "+cn.Heuv);
					var total = base.CompareImgAllCoords(cn.Data);
					cn.CachedXCoords = new int[numcoords];
					cn.CachedYCoords = new int[numcoords];
					int[] minvals = new int[numcoords];
					minvals.SetAll(int.MaxValue);
					UpdateCachedCoords(total, minvals, cn.CachedXCoords, cn.CachedYCoords);
					//xcoords.CopyTo(cn.CachedXCoords, 0);
					//ycoords.CopyTo(cn.CachedYCoords, 0);
					cn.Heuv = minvals[0];
					//Console.WriteLine("updated heuv to "+cn.Heuv);
					//Console.WriteLine("xcoords are "+xcoords.MkString());
					//Console.WriteLine("ycoords are "+ycoords.MkString());
					//Console.WriteLine("minvals are "+minvals.MkString());
					//Console.WriteLine("total are "+total.MkString());
				};}
		}
		
		public void UpdateCachedCoords(int[,] total, int[] minvals, int[] xcoords, int[] ycoords) {
			for (int y = 0; y < total.Height(); ++y) {
				for (int x = 0; x < total.Width(); ++x) {
					int sortedidx = minvals.MinInsertIncreasing(total[x,y]);
					if (sortedidx != -1) { // new minimum
						xcoords.ShiftRight(x, sortedidx);
						ycoords.ShiftRight(y, sortedidx);
					}
				}
			}
			//minvalsvalid = true;
		}
		
		public override int CompareImg(IVisNode cn) {
			int tx = 0;
			int ty = 0;
			int outv = CompareImg(cn.Data, ref tx, ref ty, cn.CachedXCoords, cn.CachedYCoords);
			cn.StartX = tx;
			cn.StartY = ty;
			return outv;
		}
		
		public override int CompareImg(int[,] simg, ref int xout, ref int yout) {
			/*if (minvals == null) {
				minvals = new int[numcoords];
				minvals.SetAll(int.MaxValue);
				xcoordsloc = new int[numcoords];
				xcoordsloc.SetAll(int.MaxValue);
				ycoordsloc = new int [numcoords];
				ycoordsloc.SetAll(int.MaxValue);
			}*/
			throw new Exception("don't use CompareImg");
			//return CompareImg(simg, ref xout, ref yout, xcoordsloc, ycoordsloc);
		}
		
		public int CompareImg(int[,] simg, ref int xout, ref int yout, int[] xcoords, int[] ycoords) {
			int rsheightdiff = RefImg.Height()-simg.Height()+1;
			int rswidthdiff = RefImg.Width()-simg.Width()+1;
			if (rsheightdiff <= 0)
				return int.MaxValue;
				//throw new Exception("Supplied image height too large");
			if (rswidthdiff <= 0)
				return int.MaxValue;
				//throw new Exception("Supplied image width too large");
			//if (xcoords[0] == int.MinValue) {
			//	UpdateCachedCoords(base.CompareImgAllCoords(simg), xcoords, ycoords);
			//	return minvals[0];
			//}
			//if (minvalsvalid) {
			//	minvalsvalid = false;
			//	return minvals[0];
			//}
			int[] total = new int[numcoords];
			total.SetAll(int.MaxValue);
			var SImgProp = new int[PropDepth][,];
			SImgProp[0] = simg.PadXY(PropDepth, PropDepth, PropDepth, PropDepth);
			int imgwidth = SImgProp[0].Width();
			int imgheight = SImgProp[0].Height();
			for (int i = 0; i < PropDepth-2; ++i) {
				SImgProp[i+1] = new int[imgwidth, imgheight];
				SImgProp[i].PixelProp8(SImgProp[i+1]);
			}
			int weight = 3*(PropDepth-1)/2;
			for (int i = 0; i < PropDepth-1; ++i) {
				for (int j = 0; j < numcoords; ++j) {
					int x = xcoords[j];
					//if (x >= rswidthdiff) continue;
					int y = ycoords[j];
					//if (x >= rsheightdiff) continue;
						int loctot = 0;
						loctot += BaseRefDiff[i]-BaseRefDiffRange(i, x+PropDepth-i, x+PropDepth+i+simg.Width(), y+PropDepth-i, y+PropDepth+i+simg.Height());
						for (int ny = PropDepth-i; ny < PropDepth+i+simg.Height(); ++ny) {
							for (int nx = PropDepth-i; nx < PropDepth+i+simg.Width(); ++nx) {
								if (SImgProp[i][nx,ny] != RefImgProp[i][x+nx,y+ny]) ++loctot;
							}
						}
						total[j] = loctot*weight;
				}
				--weight;
			}
			int minidx = 0;
			int minval = total.Min(ref minidx);
			xout = xcoords[minidx];
			yout = ycoords[minidx];
			return minval;
		}
	}
}
