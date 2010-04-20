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
using System.Xml.Serialization;

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
	public delegate int VisTransCost();

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
		int HeuvCost {get;}
		int MaxCost {get; set;}
		int[,] Data {get; set;}
		int[] SerData {get; set;}
		int SerWidth {get; set;}
		int SerHeight {get; set;}
		VisTrans[] Transforms {get; set;}
		//VisTransCost[] TCosts {get; set;}
		int[] TCostCons {get; set;}
		//int[,] Render();
		//void Initialize();
		string Describe();
		IVisNode[] Next();
	}
	
	public abstract class BaseVisNode : IVisNode {
		public int Width {get {return this.Data.Width();} }
		public int Height {get {return this.Data.Height();} }
		public string Name {get; set;}
		public int Cost {get; set;}
		public int Heuv {get; set;}
		public int HeuvCost {get {return Cost + Heuv;} }
		public int MaxCost {get; set;}
		[XmlIgnore] public int[,] Data {get; set;}
		[XmlIgnore] public VisTrans[] Transforms {get; set;}
		public int[] SerData {get; set;}
		public int SerWidth {get; set;}
		public int SerHeight {get; set;}
		//public VisTransCost[] TCosts {get; set;}
		public int[] TCostCons {get; set;}
		//public virtual int[,] Render () {return null;}
		//public virtual void Initialize() {}
		
		public IVisNode[] Next() {
			var rv = new IVisNode[Transforms.Length];
			for (int x = 0; x < Transforms.Length; ++x) {
				rv[x] = this.Transforms[x](this);
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
			if (n.Width < 1 || n.Height < 1) n.Cost = n.MaxCost+1;
			return n;
		}
		public SquareN() {
			Name = "SquareN";
			Data = new int[3,3] {{255,255,255},{255,255,255},{255,255,255}};
			MaxCost = 100;
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
			if (n.Width < 1 || n.Height < 1) n.Cost = n.MaxCost+1;
			return n;
		}
		public static IVisNode ContractY(IVisNode ths) {
			var n = ths.DeepCopyNoData();
			n.Data = ths.Data.SliceY(0,ths.Data.LastY());
			n.Cost += ths.TCostCons[0];
			if (n.Width < 1 || n.Height < 1) n.Cost = n.MaxCost+1;
			return n;
		}

		public RectangleN() {
			Name = "RectangleN";
			Data = new int[3,3] {{255,255,255},{255,255,255},{255,255,255}};
			MaxCost = 100;
			TCostCons = new int[] {0,0,0,0};
			Transforms = new VisTrans[] {
				ExpandX,
				ExpandY,
				ContractX,
				ContractY,
			};
		}
	}
	
	public class TowerN : BaseVisNode {
		public int GrowDirection = 0;
		// 0 = undecided
		// 1 = up/down
		// 2 = left/right
		public static IVisNode ExpandX(IVisNode thso) {
			var ths = (TowerN)thso;
			var n = (TowerN)RectangleN.ExpandX(ths);
			if (ths.GrowDirection == 2) n.Cost = n.MaxCost+1;
			else if (ths.GrowDirection == 0) n.GrowDirection = 1;
			return n;
		}
		public static IVisNode ExpandY(IVisNode thso) {
			var ths = (TowerN)thso;
			var n = (TowerN)RectangleN.ExpandY(ths);
			if (ths.GrowDirection == 1) n.Cost = n.MaxCost+1;
			else if (ths.GrowDirection == 0) n.GrowDirection = 2;
			return n;
		}
		public static IVisNode ContractX(IVisNode thso) {
			var ths = (TowerN)thso;
			var n = (TowerN)RectangleN.ContractX(ths);
			if (ths.GrowDirection == 2) n.Cost = n.MaxCost+1;
			else if (ths.GrowDirection == 0) n.GrowDirection = 1;
			return n;
		}
		public static IVisNode ContractY(IVisNode thso) {
			var ths = (TowerN)thso;
			var n = (TowerN)RectangleN.ContractY(ths);
			if (ths.GrowDirection == 1) n.Cost = n.MaxCost+1;
			else if (ths.GrowDirection == 0) n.GrowDirection = 2;
			return n;
		}
		public TowerN() {
			Name = "TowerN";
			Data = new int[1,1] {{255}};
			MaxCost = 100;
			TCostCons = new int[] {1};
			Transforms = new VisTrans[] {
				ExpandX,
				ExpandY,
				ContractX,
				ContractY,
			};
		}
	}
}
