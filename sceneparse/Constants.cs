//  
//  Constants.cs
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
	public static class Constants {
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
}
