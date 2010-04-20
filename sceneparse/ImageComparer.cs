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
		int[,] RefImg {get; set;}
		int[,] BaseImg {get; set;}
	}
	
	public abstract class BaseImageComparer : IImageComparer {
		public int[,] RefImg {get; set;}
		public int[,] BaseImg {get; set;}
		public virtual int CompareImg(int[,] simg, ref int xout, ref int yout) {return 0;}
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
		
		public override int CompareImg(int[,] simg, ref int xout, ref int yout) {
			int rsheightdiff = RefImg.Height()-simg.Height()+1;
			int rswidthdiff = RefImg.Width()-simg.Width()+1;
			if (rsheightdiff <= 0)
				return int.MaxValue;
				//throw new Exception("Supplied image height too large");
			if (rswidthdiff <= 0)
				return int.MaxValue;
				//throw new Exception("Supplied image width too large");
			int[,] total = new int[rswidthdiff,rsheightdiff];
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
			return total.Min(ref xout, ref yout);
		}
	}
}
