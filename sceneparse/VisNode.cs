//  
//  VisNode.cs
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
	/*
	public class MatrixEqualityComparer<T> : IEqualityComparer<T[,]> where T : IEquatable<T> {
		public bool Equals(T[,] v, T[,] o) {
			return v.MatrixEquals(o);
		}
		public int GetHashCode(T[,] v) {
			return v.MatrixHash();
		}
	}
	*/
	public class MatrixEqualityComparerInt : IEqualityComparer<int[,]> {
		public bool Equals(int[,] v, int[,] o) {
			return v.MatrixEquals(o);
		}
		public int GetHashCode(int[,] v) {
			return v.MatrixHash();
		}
	}
	
	public class VisNodeEqualityComparer : IEqualityComparer<IVisNode> {
		public bool Equals(IVisNode v, IVisNode o) {
			return v.Equals(o);
		}
		public int GetHashCode(IVisNode v) {
			return v.GetHashCode();
		}
	}
	
	public class VisNodeComparer : IComparer<IVisNode> {
		public int Compare(IVisNode v, IVisNode o) {
			return v.CompareTo(o);
		}
	}
	
	public delegate IVisNode VisTrans(IVisNode n);
	public delegate IEnumerable<IVisNode> VisTransMulti(IVisNode n);
	//public delegate int VisTransCost();

	public interface IDeepCopyable<T> {
		T DeepCopy();
		T DeepCopyNoData();
	}
	
	public interface IHashable {
		int GetHashCode();
	}
	
	public interface IVisNode : 
		IComparable<IVisNode>,
		IEquatable<IVisNode>,
		IHashable,
		IDeepCopyable<IVisNode> {
		int Width {get;}
		int Height {get;}
		string Name {get; set;}
		int Cost {get; set;}
		int Heuv {get; set;}
		double HeuvCost {get;}
		int MaxCost {get; set;}
		int[,] Data {get; set;}
		VisTrans[] Transforms {get; set;}
		VisTransMulti[] TransformsMulti {get; set;}
		//VisTransCost[] TCosts {get; set;}
		int[] TCostCons {get; set;}
		//int[,] Render();
		//void Initialize();
		string Describe();
		IEnumerable<IVisNode> Next();
		int StartX {get; set;}
		int StartY {get; set;}
		int[] CachedXCoords {get; set;}
		int[] CachedYCoords {get; set;}
	}
	
	public abstract class BaseVisNode : IVisNode {
		public int Width {get {return this.Data.Width();} }
		public int Height {get {return this.Data.Height();} }
		public string Name {get; set;}
		public int Cost {get; set;}
		protected int _Heuv = 0;
		public int Heuv {
			get {
				return _Heuv;
			} set {
				_Heuv = value;
			}}
		public double HeuvCost {get {return Cost/10000.0 + Heuv;} }
		public int MaxCost {get; set;}
		public int[,] Data {get; set;}
		[Polenter.Serialization.ExcludeFromSerializationAttribute]
		public VisTrans[] Transforms {get; set;}
		protected VisTransMulti[] _TransformsMulti = new VisTransMulti[0];
		[Polenter.Serialization.ExcludeFromSerializationAttribute]
		public VisTransMulti[] TransformsMulti {
			get {
				return _TransformsMulti;
			} set {
				_TransformsMulti = value;
			}}
		//public VisTransCost[] TCosts {get; set;}
		public int[] TCostCons {get; set;}
		public int StartX {get; set;}
		public int StartY {get; set;}
		public int[] CachedXCoords {get; set;}
		public int[] CachedYCoords {get; set;}
		//public virtual int[,] Render () {return null;}
		//public virtual void Initialize() {}
		
		public IEnumerable<IVisNode> Next() {
			var rv = new LinkedList<IVisNode>();
			foreach (var f in Transforms) {
				var n = f(this);
				if (n != null)
					rv.AddLast(n);
			}
			foreach (var f in TransformsMulti) {
				var l = f(this);
				foreach (var n in l) {
					rv.AddLast(n);
				}
			}
			return rv;
		}
		
		public bool Equals(IVisNode o) {
			return this.Data.MatrixEquals(o.Data);
		}
		
		public int CompareTo(IVisNode o) {
			return this.HeuvCost.CompareTo(o.HeuvCost);
		}
		
		public override int GetHashCode() {
			return this.Data.MatrixHash();
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
			n.TransformsMulti = this.TransformsMulti.DeepCopy();
			//n.TCosts = o.TCosts.DeepCopy();
			n.TCostCons = this.TCostCons.DeepCopy();
			return n;
		}
		
		public virtual string Describe() {
			return	"Name: "+this.Name+"\n"+
					"Cost: "+this.Cost+"\n"+
					"Width: "+this.Width+"\n"+
					"Height: "+this.Height;
		}
	}
	
	public class SquareN : BaseVisNode {
		public static IVisNode Expand(IVisNode ths) {
			var n = ths.DeepCopyNoData();
			n.Data = ths.Data.AddRightColumn().AddBottomRow();
			n.Data.SetRow(n.Data.LastY(), 255);
			n.Data.SetColumn(n.Data.LastX(), 255);
			n.Cost += ths.TCostCons[0];
			return n;
		}
		public static IVisNode Contract(IVisNode ths) {
			var n = ths.DeepCopyNoData();
			n.Data = ths.Data.SliceXY(0,ths.Data.LastX(),0,ths.Data.LastY());
			n.Cost += ths.TCostCons[0];
			if (n.Width < 1 || n.Height < 1) return null;
			return n;
		}
		public SquareN() {
			Name = "SquareN";
			Data = new int[3,3] {{255,255,255},{255,255,255},{255,255,255}};
			MaxCost = 100000;
			TCostCons = new int[] {1,1};
			Transforms = new VisTrans[] {
				Expand,
				Contract,
			};
		}
	}
	
	public class RectangleN : BaseVisNode {
		public static IVisNode ExpandX(IVisNode ths) {
			var n = ths.DeepCopyNoData();
			n.Data = ths.Data.AddRightColumn();
			n.Data.SetColumn(n.Data.LastX(), 255);
			n.Cost += ths.TCostCons[0];
			return n;
		}
		public static IVisNode ExpandY(IVisNode ths) {
			var n = ths.DeepCopyNoData();
			n.Data = ths.Data.AddBottomRow();
			n.Data.SetRow(n.Data.LastY(), 255);
			n.Cost += ths.TCostCons[0];
			return n;
		}
		public static IVisNode ContractX(IVisNode ths) {
			var n = ths.DeepCopyNoData();
			n.Data = ths.Data.SliceX(0,ths.Data.LastX());
			n.Cost += ths.TCostCons[0];
			if (n.Width < 1 || n.Height < 1) return null;
			return n;
		}
		public static IVisNode ContractY(IVisNode ths) {
			var n = ths.DeepCopyNoData();
			n.Data = ths.Data.SliceY(0,ths.Data.LastY());
			n.Cost += ths.TCostCons[0];
			if (n.Width < 1 || n.Height < 1) return null;
			return n;
		}

		public RectangleN() {
			Name = "RectangleN";
			Data = new int[2,3] {{255,255,255},{255,255,255}};
			MaxCost = 100000;
			TCostCons = new int[] {1,1,1,1};
			Transforms = new VisTrans[] {
				ExpandX,
				ExpandY,
				ContractX,
				ContractY,
			};
		}
	}
	
	public class RectangleGridN : BaseVisNode {
		public int[,] DataReal {get; set;}
		public int GridScale {get; set;}
		public static IVisNode ExpandX(IVisNode thso) {
			var ths = (RectangleGridN)thso;
			var n = (RectangleGridN)ths.DeepCopyNoData();
			n.DataReal = ths.DataReal.AddRightColumn();
			n.DataReal.SetColumn(n.DataReal.LastX(), 255);
			n.Cost += ths.TCostCons[0];
			n.Data = n.DataReal.ScaleGrid(n.GridScale);
			return n;
		}
		public static IVisNode ExpandY(IVisNode thso) {
			var ths = (RectangleGridN)thso;
			var n = (RectangleGridN)ths.DeepCopyNoData();
			n.DataReal = ths.DataReal.AddBottomRow();
			n.DataReal.SetRow(n.DataReal.LastY(), 255);
			n.Cost += ths.TCostCons[0];
			n.Data = n.DataReal.ScaleGrid(n.GridScale);
			return n;
		}
		public static IVisNode ContractX(IVisNode thso) {
			var ths = (RectangleGridN)thso;
			var n = (RectangleGridN)ths.DeepCopyNoData();
			n.DataReal = ths.DataReal.SliceX(0,ths.DataReal.LastX());
			n.Cost += ths.TCostCons[0];
			if (n.DataReal.Width() < 1 || n.DataReal.Height() < 1) return null;
			n.Data = n.DataReal.ScaleGrid(n.GridScale);
			return n;
		}
		public static IVisNode ContractY(IVisNode thso) {
			var ths = (RectangleGridN)thso;
			var n = (RectangleGridN)ths.DeepCopyNoData();
			n.DataReal = ths.DataReal.SliceY(0,ths.DataReal.LastY());
			n.Cost += ths.TCostCons[0];
			if (n.DataReal.Width() < 1 || n.DataReal.Height() < 1) return null;
			n.Data = n.DataReal.ScaleGrid(n.GridScale);
			return n;
		}
		public static IVisNode ScaleUp(IVisNode thso) {
			var ths = (RectangleGridN)thso;
			var n = (RectangleGridN)ths.DeepCopyNoData();
			++n.GridScale;
			n.Cost += ths.TCostCons[0];
			n.Data = n.DataReal.ScaleGrid(n.GridScale);
			return n;
		}
		public static IVisNode ScaleDown(IVisNode thso) {
			var ths = (RectangleGridN)thso;
			var n = (RectangleGridN)ths.DeepCopyNoData();
			--n.GridScale;
			if (n.GridScale <= 0) return null;
			n.Cost += ths.TCostCons[0];
			n.Data = n.DataReal.ScaleGrid(n.GridScale);
			return n;
		}
		public RectangleGridN() {
			Name = "RectangleGridN";
			DataReal = new int[1,1] {{255}};
			GridScale = 1;
			Data = DataReal.ScaleGrid(GridScale);
			MaxCost = 100000;
			TCostCons = new int[] {1};
			Transforms = new VisTrans[] {
				ExpandX,
				ExpandY,
				ContractX,
				ContractY,
				ScaleUp,
				ScaleDown,
			};
		}
	}
	
	public class TowerN : BaseVisNode {
		public int GrowDirection {get; set;}
		// 0 = undecided
		// 1 = up/down
		// 2 = left/right
		public static IVisNode ExpandX(IVisNode thso) {
			var ths = (TowerN)thso;
			var n = (TowerN)RectangleN.ExpandX(ths);
			if (n == null) return null;
			if (ths.GrowDirection == 2) return null;
			else if (ths.GrowDirection == 0) n.GrowDirection = 1;
			return n;
		}
		public static IVisNode ExpandY(IVisNode thso) {
			var ths = (TowerN)thso;
			var n = (TowerN)RectangleN.ExpandY(ths);
			if (n == null) return null;
			if (ths.GrowDirection == 1) return null;
			else if (ths.GrowDirection == 0) n.GrowDirection = 2;
			return n;
		}
		public static IVisNode ContractX(IVisNode thso) {
			var ths = (TowerN)thso;
			var n = (TowerN)RectangleN.ContractX(ths);
			if (n == null) return null;
			if (ths.GrowDirection == 2) return null;
			else if (ths.GrowDirection == 0) n.GrowDirection = 1;
			return n;
		}
		public static IVisNode ContractY(IVisNode thso) {
			var ths = (TowerN)thso;
			var n = (TowerN)RectangleN.ContractY(ths);
			if (n == null) return null;
			if (ths.GrowDirection == 1) return null;
			else if (ths.GrowDirection == 0) n.GrowDirection = 2;
			return n;
		}
		public TowerN() {
			Name = "TowerN";
			Data = new int[1,1] {{255}};
			MaxCost = 100000;
			GrowDirection = 0;
			TCostCons = new int[] {1};
			Transforms = new VisTrans[] {
				ExpandX,
				ExpandY,
				ContractX,
				ContractY,
			};
		}
	}
	
	public class TowerGridN : BaseVisNode {
		public int[,] DataReal {get; set;}
		public int GridScale {get; set;}
		public int GrowDirection {get; set;}
		// 0 = undecided
		// 1 = up/down
		// 2 = left/right
		public static IVisNode ExpandX(IVisNode thso) {
			var ths = (TowerGridN)thso;
			var n = (TowerGridN)ths.DeepCopyNoData();
			n.DataReal = ths.DataReal.AddRightColumn();
			n.DataReal.SetColumn(n.DataReal.LastX(), 255);
			n.Cost += ths.TCostCons[0];
			if (ths.GrowDirection == 2) return null;
			else if (ths.GrowDirection == 0) n.GrowDirection = 1;
			n.Data = n.DataReal.ScaleGrid(n.GridScale);
			return n;
		}
		public static IVisNode ExpandY(IVisNode thso) {
			var ths = (TowerGridN)thso;
			var n = (TowerGridN)ths.DeepCopyNoData();
			n.DataReal = ths.DataReal.AddBottomRow();
			n.DataReal.SetRow(n.DataReal.LastY(), 255);
			n.Cost += ths.TCostCons[0];
			if (ths.GrowDirection == 1) return null;
			else if (ths.GrowDirection == 0) n.GrowDirection = 2;
			n.Data = n.DataReal.ScaleGrid(n.GridScale);
			return n;
		}
		public static IVisNode ContractX(IVisNode thso) {
			var ths = (TowerGridN)thso;
			var n = (TowerGridN)ths.DeepCopyNoData();
			n.DataReal = ths.DataReal.SliceX(0,ths.DataReal.LastX());
			n.Cost += ths.TCostCons[0];
			if (n.DataReal.Width() < 1 || n.DataReal.Height() < 1) return null;
			if (ths.GrowDirection == 2) return null;
			else if (ths.GrowDirection == 0) n.GrowDirection = 1;
			n.Data = n.DataReal.ScaleGrid(n.GridScale);
			return n;
		}
		public static IVisNode ContractY(IVisNode thso) {
			var ths = (TowerGridN)thso;
			var n = (TowerGridN)ths.DeepCopyNoData();
			n.DataReal = ths.DataReal.SliceY(0,ths.DataReal.LastY());
			n.Cost += ths.TCostCons[0];
			if (n.DataReal.Width() < 1 || n.DataReal.Height() < 1) return null;
			if (ths.GrowDirection == 1) return null;
			else if (ths.GrowDirection == 0) n.GrowDirection = 2;
			n.Data = n.DataReal.ScaleGrid(n.GridScale);
			return n;
		}
		public static IVisNode ScaleUp(IVisNode thso) {
			var ths = (TowerGridN)thso;
			var n = (TowerGridN)ths.DeepCopyNoData();
			++n.GridScale;
			n.Cost += ths.TCostCons[0];
			n.Data = n.DataReal.ScaleGrid(n.GridScale);
			return n;
		}
		public static IVisNode ScaleDown(IVisNode thso) {
			var ths = (TowerGridN)thso;
			var n = (TowerGridN)ths.DeepCopyNoData();
			--n.GridScale;
			if (n.GridScale <= 0) return null;
			n.Cost += ths.TCostCons[0];
			n.Data = n.DataReal.ScaleGrid(n.GridScale);
			return n;
		}
		public TowerGridN() {
			Name = "TowerGridN";
			DataReal = new int[1,1] {{255}};
			GridScale = 1;
			GrowDirection = 0;
			Data = DataReal.ScaleGrid(GridScale);
			MaxCost = 100000;
			TCostCons = new int[] {1};
			Transforms = new VisTrans[] {
				ExpandX,
				ExpandY,
				ContractX,
				ContractY,
				ScaleUp,
				ScaleDown,
			};
		}
	}
	
	public class ChainN : BaseVisNode {
		public int headx;
		public int heady;
		public static IVisNode ExpandRight(IVisNode thso) {
			var ths = (ChainN)thso;
			var n = (ChainN)ths.DeepCopyNoData();
			if (n.headx == ths.Data.LastX()) {
				n.Data = ths.Data.AddRightColumn();
			} else {
				n.Data = ths.Data.DeepCopy();
			}
			++n.headx;
			n.Data[n.headx, n.heady] = 255;
			n.Cost += ths.TCostCons[0];
			return n;
		}
		public static IVisNode ExpandLeft(IVisNode thso) {
			var ths = (ChainN)thso;
			var n = (ChainN)ths.DeepCopyNoData();
			if (n.headx == 0) {
				n.Data = ths.Data.AddLeftColumn();
			} else {
				n.Data = ths.Data.DeepCopy();
				--n.headx;
			}
			n.Data[n.headx, n.heady] = 255;
			n.Cost += ths.TCostCons[0];
			return n;
		}
		public static IVisNode ExpandDown(IVisNode thso) {
			var ths = (ChainN)thso;
			var n = (ChainN)ths.DeepCopyNoData();
			if (n.heady == ths.Data.LastY()) {
				n.Data = ths.Data.AddBottomRow();
			} else {
				n.Data = ths.Data.DeepCopy();
			}
			++n.heady;
			n.Data[n.headx, n.heady] = 255;
			n.Cost += ths.TCostCons[0];
			return n;
		}
		public static IVisNode ExpandUp(IVisNode thso) {
			var ths = (ChainN)thso;
			var n = (ChainN)ths.DeepCopyNoData();
			if (n.heady == 0) {
				n.Data = ths.Data.AddTopRow();
			} else {
				n.Data = ths.Data.DeepCopy();
				--n.heady;
			}
			n.Data[n.headx, n.heady] = 255;
			n.Cost += ths.TCostCons[0];
			return n;
		}
		public static IEnumerable<IVisNode> ExpandMulti(IVisNode thso) {
			LinkedList<IVisNode> retv = new LinkedList<IVisNode>();
			var ths = (ChainN)thso;
			for (int y = 0; y < ths.Data.Height(); ++y) {
				for (int x = 0; x < ths.Data.Width(); ++x) {
					if ((ths.Data.Width() == 1 && ths.Data.Height() == 1) || (ths.Data[x,y] > 0 && ths.Data.CountNeighbors(x,y) == 1)) {
						if (ths.Data.Height() <= 0 || ths.Data.Width() <= 0) continue;
						if (ths.headx < 0 || ths.headx >= ths.Data.Width() || ths.heady < 0 || ths.heady >= ths.Data.Height()) continue;
						ths.headx = x;
						ths.heady = y;
						var n = ExpandLeft(ths);
						if (n != null) retv.AddLast(n);
						n = ExpandRight(ths);
						if (n != null) retv.AddLast(n);
						n = ExpandUp(ths);
						if (n != null) retv.AddLast(n);
						n = ExpandDown(ths);
						if (n != null) retv.AddLast(n);
					}
				}
			}
			return retv;
		}
		public static IEnumerable<IVisNode> ContractMulti(IVisNode thso) {
			LinkedList<IVisNode> retv = new LinkedList<IVisNode>();
			var ths = (ChainN)thso;
			for (int y = 0; y < ths.Data.Height(); ++y) {
				for (int x = 0; x < ths.Data.Width(); ++x) {
					if (ths.Data[x,y] > 0 && ths.Data.CountNeighbors(x,y) == 1) {
						if (ths.Data.Height() <= 0 || ths.Data.Width() <= 0) continue;
						if (ths.headx < 0 || ths.headx >= ths.Data.Width() || ths.heady < 0 || ths.heady >= ths.Data.Height()) continue;
						var n = (ChainN)ths.DeepCopyNoData();
						n.Data = ths.Data.DeepCopy();
						n.Data[x,y] = 0;
						if (n.headx == x && n.heady == y) {
							n.Data.HasSingleNeighbor(x, y, ref n.headx, ref n.heady);
							if (n.headx < 0 || n.headx >= n.Data.Width() || n.heady < 0 || n.heady >= n.Data.Height()) continue;
						}
						if (x == 0 && n.Data.ColumnEquals(0, 0)) {
							n.Data = n.Data.SliceX(1, n.Data.Width());
							--n.headx;
							if (n.headx < 0 || n.headx >= n.Data.Width() || n.heady < 0 || n.heady >= n.Data.Height()) continue;
							if (n.Data.Height() <= 0 || n.Data.Width() <= 0) continue;
							
						}
						if (y == 0 && n.Data.RowEquals(0, 0)) {
							n.Data = n.Data.SliceY(1, n.Data.Height());
							--n.heady;
							if (n.headx < 0 || n.headx >= n.Data.Width() || n.heady < 0 || n.heady >= n.Data.Height()) continue;
							if (n.Data.Height() <= 0 || n.Data.Width() <= 0) continue;
						}
						if (x == n.Data.LastX() && n.Data.ColumnEquals(n.Data.LastX(), 0)) {
							n.Data = n.Data.SliceX(0, n.Data.LastX());
							if (n.headx < 0 || n.headx >= n.Data.Width() || n.heady < 0 || n.heady >= n.Data.Height()) continue;
							if (n.Data.Height() <= 0 || n.Data.Width() <= 0) continue;
						}
						if (y == n.Data.LastY() && n.Data.RowEquals(n.Data.LastY(), 0)) {
							n.Data = n.Data.SliceY(0, n.Data.LastY());
							if (n.headx < 0 || n.headx >= n.Data.Width() || n.heady < 0 || n.heady >= n.Data.Height()) continue;
							if (n.Data.Height() <= 0 || n.Data.Width() <= 0) continue;
						}
						n.Cost += ths.TCostCons[0];
						retv.AddLast(n);
					}
				}
			}
			return retv;
		}
		public ChainN() {
			Name = "ChainN";
			Data = new int[2,1] {{255}, {255}};
			MaxCost = 100000;
			TCostCons = new int[] {1,1,1,1};
			Transforms = new VisTrans[] {
				//ExpandRight,
				//ExpandLeft,
				//ExpandDown,
				//ExpandUp,
			};
			TransformsMulti = new VisTransMulti[] {
				ExpandMulti,
				ContractMulti,
			};
		}
	}
}
