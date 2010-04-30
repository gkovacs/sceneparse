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
	
	public interface IHeuristic {
		int Heuv {get; set;}
	}
	
	public interface ICost {
		int Cost {get; set;}
		int MaxCost {get; set;}
	}
	
	public interface INextable<T> {
		IEnumerable<T> Next();
	}
	
	public interface IVisNode : 
		IComparable<IVisNode>,
		IEquatable<IVisNode>,
		IHashable,
		IHeuristic,
		ICost,
		INextable<IVisNode>,
		IDeepCopyable<IVisNode> {
		int Width {get;}
		int Height {get;}
		string Name {get; set;}
		double HeuvCost {get;}
		int[,] Data {get; set;}
		VisTrans[] Transforms {get; set;}
		VisTransMulti[] TransformsMulti {get; set;}
		//VisTransCost[] TCosts {get; set;}
		int[] TCostCons {get; set;}
		bool IsGrid {get; set;}
		int[,] DataReal {get; set;}
		int GridScale {get; set;}
		//int[,] Render();
		//void Initialize();
		string Describe();
		int StartX {get; set;}
		int StartY {get; set;}
		int[] CachedXCoords {get; set;}
		int[] CachedYCoords {get; set;}
		void Init(IVisNode n);
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
		protected bool _IsGrid = false;
		public bool IsGrid {
			get {
				return _IsGrid;
			}
			set {
				_IsGrid = value;
			}
		}
		protected int[,] _DataReal;
		public virtual int[,] DataReal {
			get {
				if (_IsGrid) return _DataReal;
				else return Data;
			}
			set {
				if (_IsGrid) _DataReal = value;
				else Data = value;
			}
		}
		protected int _GridScale = 1;
		public virtual int GridScale {
			get {
				return _GridScale;
			}
			set {
				_GridScale = value;
			}
		}
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
			return (this.Name.Equals(o.Name) && this.Data.MatrixEquals(o.Data));
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
		public BaseVisNode() {
			Init(this);
		}
		public virtual void Init(IVisNode n) {
			
		}
	}
	
	public static class GridVisNode {
		public static IVisNode ScaleUp(IVisNode thso) {
			var ths = thso;
			var n = ths.DeepCopyNoData();
			++n.GridScale;
			n.Cost += ths.TCostCons[0];
			n.Data = n.DataReal.ScaleGrid(n.GridScale);
			return n;
		}
		public static IVisNode ScaleDown(IVisNode thso) {
			var ths = thso;
			var n = ths.DeepCopyNoData();
			--n.GridScale;
			if (n.GridScale <= 0) return null;
			n.Cost += ths.TCostCons[0];
			n.Data = n.DataReal.ScaleGrid(n.GridScale);
			return n;
		}
	}
	
	public class SquareN : BaseVisNode {
		public static IVisNode Expand(IVisNode ths) {
			var n = ths.DeepCopyNoData();
			n.DataReal = ths.DataReal.AddRightColumn().AddBottomRow();
			n.DataReal.SetRow(n.DataReal.LastY(), 255);
			n.DataReal.SetColumn(n.DataReal.LastX(), 255);
			n.Cost += ths.TCostCons[0];
			return n;
		}
		public static IVisNode Contract(IVisNode ths) {
			var n = ths.DeepCopyNoData();
			n.DataReal = ths.DataReal.SliceXY(0,ths.DataReal.LastX(),0,ths.DataReal.LastY());
			n.Cost += ths.TCostCons[0];
			if (n.DataReal.Width() < 1 || n.DataReal.Height() < 1) return null;
			return n;
		}
		public override void Init(IVisNode n) {
			n.Name = "SquareN";
			n.Data = new int[3,3] {{255,255,255},{255,255,255},{255,255,255}};
			n.MaxCost = 100000;
			n.TCostCons = new int[] {1,1};
			n.Transforms = new VisTrans[] {
				Expand,
				Contract,
			};
		}
		public SquareN() : base() {}
	}
	
	public class SquareGridN : BaseVisNode {
		public static IVisNode Expand(IVisNode ths) {
			var n = SquareN.Expand(ths);
			if (n == null) return null;
			n.Data = n.DataReal.ScaleGrid(n.GridScale);
			return n;
		}
		public static IVisNode Contract(IVisNode ths) {
			var n = SquareN.Contract(ths);
			if (n == null) return null;
			n.Data = n.DataReal.ScaleGrid(n.GridScale);
			return n;
		}
		public override void Init(IVisNode n) {
			n.Name = "SquareGridN";
			n.IsGrid = true;
			n.DataReal = new int[3,3] {{255,255,255},{255,255,255},{255,255,255}};
			n.GridScale = 1;
			n.Data = n.DataReal.ScaleGrid(n.GridScale);
			n.MaxCost = 100000;
			n.TCostCons = new int[] {1,1};
			n.Transforms = new VisTrans[] {
				Expand,
				Contract,
				GridVisNode.ScaleUp,
				GridVisNode.ScaleDown,
			};
		}
		public SquareGridN() : base() {}
	}
	
	public class RectangleN : BaseVisNode {
		public static IVisNode ExpandX(IVisNode ths) {
			var n = ths.DeepCopyNoData();
			n.DataReal = ths.DataReal.AddRightColumn();
			n.DataReal.SetColumn(n.DataReal.LastX(), 255);
			n.Cost += ths.TCostCons[0];
			return n;
		}
		public static IVisNode ExpandY(IVisNode ths) {
			var n = ths.DeepCopyNoData();
			n.DataReal = ths.DataReal.AddBottomRow();
			n.DataReal.SetRow(n.DataReal.LastY(), 255);
			n.Cost += ths.TCostCons[0];
			return n;
		}
		public static IVisNode ContractX(IVisNode ths) {
			var n = ths.DeepCopyNoData();
			n.DataReal = ths.DataReal.SliceX(0,ths.DataReal.LastX());
			n.Cost += ths.TCostCons[0];
			if (n.DataReal.Width() < 1 || n.DataReal.Height() < 1) return null;
			return n;
		}
		public static IVisNode ContractY(IVisNode ths) {
			var n = ths.DeepCopyNoData();
			n.DataReal = ths.DataReal.SliceY(0,ths.DataReal.LastY());
			n.Cost += ths.TCostCons[0];
			if (n.DataReal.Width() < 1 || n.DataReal.Height() < 1) return null;
			return n;
		}
		public override void Init(IVisNode n) {
			n.Name = "RectangleN";
			n.Data = new int[2,3] {{255,255,255},{255,255,255}};
			n.MaxCost = 100000;
			n.TCostCons = new int[] {1,1,1,1};
			n.Transforms = new VisTrans[] {
				ExpandX,
				ExpandY,
				ContractX,
				ContractY,
			};
		}
		public RectangleN() : base() {}
	}
	
	public class RectangleGridN : BaseVisNode {
		public static IVisNode ExpandX(IVisNode ths) {
			var n = RectangleN.ExpandX(ths);
			if (n == null) return null;
			n.Data = n.DataReal.ScaleGrid(n.GridScale);
			return n;
		}
		public static IVisNode ExpandY(IVisNode ths) {
			var n = RectangleN.ExpandY(ths);
			if (n == null) return null;
			n.Data = n.DataReal.ScaleGrid(n.GridScale);
			return n;
		}
		public static IVisNode ContractX(IVisNode ths) {
			var n = RectangleN.ContractX(ths);
			if (n == null) return null;
			n.Data = n.DataReal.ScaleGrid(n.GridScale);
			return n;
		}
		public static IVisNode ContractY(IVisNode ths) {
			var n = RectangleN.ContractY(ths);
			if (n == null) return null;
			n.Data = n.DataReal.ScaleGrid(n.GridScale);
			return n;
		}
		public override void Init(IVisNode n) {
			n.Name = "RectangleGridN";
			n.IsGrid = true;
			n.DataReal = new int[2,3] {{255,255,255},{255,255,255}};
			n.GridScale = 1;
			n.Data = n.DataReal.ScaleGrid(GridScale);
			n.MaxCost = 100000;
			n.TCostCons = new int[] {1};
			n.Transforms = new VisTrans[] {
				ExpandX,
				ExpandY,
				ContractX,
				ContractY,
				GridVisNode.ScaleUp,
				GridVisNode.ScaleDown,
			};
		}
		public RectangleGridN() : base() {}
	}
	
	public interface ITowerVisNode : IVisNode {
		int GrowDirection {get; set;}
	}
	
	public class TowerN : BaseVisNode, ITowerVisNode {
		public int GrowDirection {get; set;}
		// 0 = undecided
		// 1 = up/down
		// 2 = left/right
		public static IVisNode ExpandX(IVisNode thso) {
			var ths = (ITowerVisNode)thso;
			var n = (ITowerVisNode)RectangleN.ExpandX(ths);
			if (n == null) return null;
			if (ths.GrowDirection == 2) return null;
			else if (ths.GrowDirection == 0) n.GrowDirection = 1;
			return n;
		}
		public static IVisNode ExpandY(IVisNode thso) {
			var ths = (ITowerVisNode)thso;
			var n = (ITowerVisNode)RectangleN.ExpandY(ths);
			if (n == null) return null;
			if (ths.GrowDirection == 1) return null;
			else if (ths.GrowDirection == 0) n.GrowDirection = 2;
			return n;
		}
		public static IVisNode ContractX(IVisNode thso) {
			var ths = (ITowerVisNode)thso;
			var n = (ITowerVisNode)RectangleN.ContractX(ths);
			if (n == null) return null;
			if (ths.GrowDirection == 2) return null;
			else if (ths.GrowDirection == 0) n.GrowDirection = 1;
			return n;
		}
		public static IVisNode ContractY(IVisNode thso) {
			var ths = (ITowerVisNode)thso;
			var n = (ITowerVisNode)RectangleN.ContractY(ths);
			if (n == null) return null;
			if (ths.GrowDirection == 1) return null;
			else if (ths.GrowDirection == 0) n.GrowDirection = 2;
			return n;
		}
		public override void Init(IVisNode n) {
			n.Name = "TowerN";
			n.Data = new int[1,1] {{255}};
			n.MaxCost = 100000;
			n.TCostCons = new int[] {1};
			n.Transforms = new VisTrans[] {
				ExpandX,
				ExpandY,
				ContractX,
				ContractY,
			};
		}
		public TowerN() : base() {}
	}
	
	public class TowerGridN : BaseVisNode, ITowerVisNode {
		public int GrowDirection {get; set;}
		// 0 = undecided
		// 1 = up/down
		// 2 = left/right
		public static IVisNode ExpandX(IVisNode ths) {
			var n = TowerN.ExpandX(ths);
			if (n == null) return null;
			n.Data = n.DataReal.ScaleGrid(n.GridScale);
			return n;
		}
		public static IVisNode ExpandY(IVisNode ths) {
			var n = TowerN.ExpandY(ths);
			if (n == null) return null;
			n.Data = n.DataReal.ScaleGrid(n.GridScale);
			return n;
		}
		public static IVisNode ContractX(IVisNode ths) {
			var n = TowerN.ContractX(ths);
			if (n == null) return null;
			n.Data = n.DataReal.ScaleGrid(n.GridScale);
			return n;
		}
		public static IVisNode ContractY(IVisNode ths) {
			var n = TowerN.ContractY(ths);
			if (n == null) return null;
			n.Data = n.DataReal.ScaleGrid(n.GridScale);
			return n;
		}
		public override void Init(IVisNode n) {
			n.Name = "TowerGridN";
			n.IsGrid = true;
			n.DataReal = new int[1,1] {{255}};
			n.GridScale = 1;
			n.Data = n.DataReal.ScaleGrid(GridScale);
			n.MaxCost = 100000;
			n.TCostCons = new int[] {1};
			n.Transforms = new VisTrans[] {
				ExpandX,
				ExpandY,
				ContractX,
				ContractY,
				GridVisNode.ScaleUp,
				GridVisNode.ScaleDown,
			};
		}
		public TowerGridN() : base() {}
	}
	
	public interface IRingVisNode : IVisNode {
		int numitems {get; set;}
		int radius {get; set;}
		double rotation {get; set;}
	}
	
	public class RingN : BaseVisNode, IRingVisNode {
		public int numitems {get; set;}
		public int radius {get; set;}
		public double rotation {get; set;}
		public static int[,] Render(int rad, int numi, double rotn) {
			var o = new int[rad*2-1,rad*2-1];
			double radmh = rad-0.5;
			double angleincr = 2*Math.PI / numi;
			double totang = rotn;
			for (int i = 0; i < numi; ++i) {
				o[rad-1+(int)(Math.Cos(totang)*radmh), rad-1+(int)(Math.Sin(totang)*radmh)] = 255;
				totang += angleincr;
				totang %= 2*Math.PI;
			}
			return o;
		}
		public static IVisNode AddItem(IVisNode thso) {
			var ths = (IRingVisNode)thso;
			var n = (IRingVisNode)ths.DeepCopyNoData();
			++n.numitems;
			n.Cost += ths.TCostCons[0];
			n.Render();
			return n;
		}
		public static IVisNode RmItem(IVisNode thso) {
			var ths = (IRingVisNode)thso;
			var n = (IRingVisNode)ths.DeepCopyNoData();
			--n.numitems;
			if (n.numitems == 0) return null;
			n.Cost += ths.TCostCons[0];
			n.Render();
			return n;
		}
		public static IVisNode DoubleItems(IVisNode thso) {
			var ths = (IRingVisNode)thso;
			var n = (IRingVisNode)ths.DeepCopyNoData();
			n.numitems *= 2;
			n.Cost += ths.TCostCons[0];
			n.Render();
			return n;
		}
		public static IVisNode HalveItems(IVisNode thso) {
			var ths = (IRingVisNode)thso;
			if (ths.numitems % 2 != 0) return null;
			var n = (IRingVisNode)ths.DeepCopyNoData();
			n.numitems /= 2;
			n.Cost += ths.TCostCons[0];
			n.Render();
			return n;
		}
		public static IVisNode ExpandRadius(IVisNode thso) {
			var ths = (IRingVisNode)thso;
			var n = (IRingVisNode)ths.DeepCopyNoData();
			++n.radius;
			n.Cost += ths.TCostCons[0];
			n.Render();
			return n;
		}
		public static IVisNode ContractRadius(IVisNode thso) {
			var ths = (IRingVisNode)thso;
			var n = (IRingVisNode)ths.DeepCopyNoData();
			--n.radius;
			if (n.radius == 0) return null;
			n.Cost += ths.TCostCons[0];
			n.Render();
			return n;
		}
		public static IVisNode RotateUp(IVisNode thso) {
			var ths = (IRingVisNode)thso;
			var n = (IRingVisNode)ths.DeepCopyNoData();
			n.rotation += Math.Asin(1.0/n.radius);
			n.rotation %= 2*Math.PI / n.numitems;
			n.Cost += ths.TCostCons[0];
			n.Render();
			return n;
		}
		public static IVisNode RotateDown(IVisNode thso) {
			var ths = (IRingVisNode)thso;
			var n = (IRingVisNode)ths.DeepCopyNoData();
			n.rotation -= Math.Asin(1.0/n.radius);
			n.rotation %= 2*Math.PI / n.numitems;
			n.Cost += ths.TCostCons[0];
			n.Render();
			return n;
		}
		public override string Describe()
		{
			return Name+" of radius "+radius+" with "+numitems+" items at rotation "+rotation;
		}
		public override void Init(IVisNode nn) {
			var n = (IRingVisNode)nn;
			n.Name = "RingN";
			n.radius = 3;
			n.numitems = 3;
			n.Render();
			n.MaxCost = 100000;
			n.TCostCons = new int[] {1,1,1,1};
			n.Transforms = new VisTrans[] {
				AddItem,
				RmItem,
				ExpandRadius,
				ContractRadius,
				DoubleItems,
				HalveItems,
				//RotateUp,
				//RotateDown,
			};
		}
		public RingN() : base() {}
	}
	
	public interface IChainVisNode : IVisNode {
		int headx {get; set;}
		int heady {get; set;}
		int tailx {get; set;}
		int taily {get; set;}
	}
	
	public class ChainN : BaseVisNode, IChainVisNode {
		public int headx {get; set;}
		public int heady {get; set;}
		public int tailx {get; set;}
		public int taily {get; set;}
		public static IVisNode ExpandRightHead(IVisNode thso) {
			var ths = (IChainVisNode)thso;
			var n = (IChainVisNode)ths.DeepCopyNoData();
			if (n.headx == ths.DataReal.LastX()) {
				n.DataReal = ths.DataReal.AddRightColumn();
			} else {
				n.DataReal = ths.DataReal.DeepCopy();
			}
			++n.headx;
			n.DataReal[n.headx, n.heady] = 255;
			n.Cost += ths.TCostCons[0];
			return n;
		}
		public static IVisNode ExpandLeftHead(IVisNode thso) {
			var ths = (IChainVisNode)thso;
			var n = (IChainVisNode)ths.DeepCopyNoData();
			if (n.headx == 0) {
				n.DataReal = ths.DataReal.AddLeftColumn();
			} else {
				n.DataReal = ths.DataReal.DeepCopy();
				--n.headx;
			}
			n.DataReal[n.headx, n.heady] = 255;
			n.Cost += ths.TCostCons[0];
			return n;
		}
		public static IVisNode ExpandDownHead(IVisNode thso) {
			var ths = (IChainVisNode)thso;
			var n = (IChainVisNode)ths.DeepCopyNoData();
			if (n.heady == ths.DataReal.LastY()) {
				n.DataReal = ths.DataReal.AddBottomRow();
			} else {
				n.DataReal = ths.DataReal.DeepCopy();
			}
			++n.heady;
			n.DataReal[n.headx, n.heady] = 255;
			n.Cost += ths.TCostCons[0];
			return n;
		}
		public static IVisNode ExpandUpHead(IVisNode thso) {
			var ths = (IChainVisNode)thso;
			var n = (IChainVisNode)ths.DeepCopyNoData();
			if (n.heady == 0) {
				n.DataReal = ths.DataReal.AddTopRow();
			} else {
				n.DataReal = ths.DataReal.DeepCopy();
				--n.heady;
			}
			n.DataReal[n.headx, n.heady] = 255;
			n.Cost += ths.TCostCons[0];
			return n;
		}
		public static IEnumerable<IVisNode> ExpandMulti(IVisNode thso) {
			LinkedList<IVisNode> retv = new LinkedList<IVisNode>();
			var ths = (IChainVisNode)thso;
			for (int y = 0; y < ths.DataReal.Height(); ++y) {
				for (int x = 0; x < ths.DataReal.Width(); ++x) {
					if ((ths.DataReal.Width() == 1 && ths.DataReal.Height() == 1) || (ths.DataReal[x,y] > 0 && ths.DataReal.CountNeighbors(x,y) == 1)) {
						if (ths.DataReal.Height() <= 0 || ths.DataReal.Width() <= 0) continue;
						if (ths.headx < 0 || ths.headx >= ths.DataReal.Width() || ths.heady < 0 || ths.heady >= ths.DataReal.Height()) continue;
						ths.headx = x;
						ths.heady = y;
						var n = ExpandLeftHead(ths);
						if (n != null) retv.AddLast(n);
						n = ExpandRightHead(ths);
						if (n != null) retv.AddLast(n);
						n = ExpandUpHead(ths);
						if (n != null) retv.AddLast(n);
						n = ExpandDownHead(ths);
						if (n != null) retv.AddLast(n);
					}
				}
			}
			return retv;
		}
		public static IEnumerable<IVisNode> ContractMulti(IVisNode thso) {
			LinkedList<IVisNode> retv = new LinkedList<IVisNode>();
			var ths = (IChainVisNode)thso;
			for (int y = 0; y < ths.DataReal.Height(); ++y) {
				for (int x = 0; x < ths.DataReal.Width(); ++x) {
					if (ths.DataReal[x,y] > 0 && ths.DataReal.CountNeighbors(x,y) == 1) {
						if (ths.DataReal.Height() <= 0 || ths.DataReal.Width() <= 0) continue;
						if (ths.headx < 0 || ths.headx >= ths.DataReal.Width() || ths.heady < 0 || ths.heady >= ths.DataReal.Height()) continue;
						var n = (IChainVisNode)ths.DeepCopyNoData();
						n.DataReal = ths.DataReal.DeepCopy();
						n.DataReal[x,y] = 0;
						if (n.headx == x && n.heady == y) {
							int tx = 0;
							int ty = 0;
							n.DataReal.HasSingleNeighbor(x, y, ref tx, ref ty);
							n.headx = tx;
							n.heady = ty;
							if (n.headx < 0 || n.headx >= n.DataReal.Width() || n.heady < 0 || n.heady >= n.DataReal.Height()) continue;
						}
						if (x == 0 && n.DataReal.ColumnEquals(0, 0)) {
							n.DataReal = n.DataReal.SliceX(1, n.DataReal.Width());
							--n.headx;
							if (n.headx < 0 || n.headx >= n.DataReal.Width() || n.heady < 0 || n.heady >= n.DataReal.Height()) continue;
							if (n.DataReal.Height() <= 0 || n.DataReal.Width() <= 0) continue;
							
						}
						if (y == 0 && n.DataReal.RowEquals(0, 0)) {
							n.DataReal = n.DataReal.SliceY(1, n.DataReal.Height());
							--n.heady;
							if (n.headx < 0 || n.headx >= n.DataReal.Width() || n.heady < 0 || n.heady >= n.DataReal.Height()) continue;
							if (n.DataReal.Height() <= 0 || n.DataReal.Width() <= 0) continue;
						}
						if (x == n.DataReal.LastX() && n.DataReal.ColumnEquals(n.DataReal.LastX(), 0)) {
							n.DataReal = n.DataReal.SliceX(0, n.DataReal.LastX());
							if (n.headx < 0 || n.headx >= n.DataReal.Width() || n.heady < 0 || n.heady >= n.DataReal.Height()) continue;
							if (n.DataReal.Height() <= 0 || n.DataReal.Width() <= 0) continue;
						}
						if (y == n.DataReal.LastY() && n.DataReal.RowEquals(n.DataReal.LastY(), 0)) {
							n.DataReal = n.DataReal.SliceY(0, n.DataReal.LastY());
							if (n.headx < 0 || n.headx >= n.DataReal.Width() || n.heady < 0 || n.heady >= n.DataReal.Height()) continue;
							if (n.DataReal.Height() <= 0 || n.DataReal.Width() <= 0) continue;
						}
						n.Cost += ths.TCostCons[0];
						retv.AddLast(n);
					}
				}
			}
			return retv;
		}
		public override void Init(IVisNode n) {
			n.Name = "ChainN";
			n.Data = new int[2,1] {{255}, {255}};
			n.MaxCost = 100000;
			n.TCostCons = new int[] {1,1,1,1};
			n.Transforms = new VisTrans[] {
				//ExpandRightHead,
				//ExpandLeftHead,
				//ExpandDownHead,
				//ExpandUpHead,
				//ExpandRightTail,
				//ExpandLeftTail,
				//ExpandDownTail,
				//ExpandUpTail,
			};
			n.TransformsMulti = new VisTransMulti[] {
				ExpandMulti,
				ContractMulti,
			};
		}
		public ChainN() : base() {}
	}
	
	public class ChainGridN : BaseVisNode, IChainVisNode {
		public int headx {get; set;}
		public int heady {get; set;}
		public int tailx {get; set;}
		public int taily {get; set;}
		public static IEnumerable<IVisNode> ExpandMulti(IVisNode ths) {
			var retv = ChainN.ExpandMulti(ths);
			foreach (IVisNode x in retv) {
				x.Data = x.DataReal.ScaleGrid(x.GridScale);
			}
			return retv;
		}
		public static IEnumerable<IVisNode> ContractMulti(IVisNode ths) {
			var retv = ChainN.ContractMulti(ths);
			foreach (IVisNode x in retv) {
				x.Data = x.DataReal.ScaleGrid(x.GridScale);
			}
			return retv;
		}
		public override void Init(IVisNode n) {
			n.Name = "ChainGridN";
			n.IsGrid = true;
			n.DataReal = new int[2,1] {{255}, {255}};
			n.GridScale = 1;
			n.Data = n.DataReal.ScaleGrid(GridScale);
			n.MaxCost = 100000;
			n.TCostCons = new int[] {1,1,1,1};
			n.Transforms = new VisTrans[] {
				//ExpandRightHead,
				//ExpandLeftHead,
				//ExpandDownHead,
				//ExpandUpHead,
				//ExpandRightTail,
				//ExpandLeftTail,
				//ExpandDownTail,
				//ExpandUpTail,
				GridVisNode.ScaleUp,
				GridVisNode.ScaleDown,
			};
			n.TransformsMulti = new VisTransMulti[] {
				ExpandMulti,
				ContractMulti,
			};
		}
		public ChainGridN() : base() {}
	}
}
