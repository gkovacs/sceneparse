//  
//  Search.cs
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
	public delegate void NodeActionDelegate(IVisNode n);
	public delegate int HeuristicDelegate(IVisNode n);
	public delegate bool TerminationDelegate(IVisNode n);
	
	public interface ISearchAlgorithm {
		C5.IntervalHeap<IVisNode> Agenda {get; set;}
		Dictionary<int[,], IVisNode> Visited {get; set;}
		NodeActionDelegate NodeAction {get; set;}
		HeuristicDelegate NodeHeuristic {get; set;}
		TerminationDelegate NodeTermination {get; set;}
		int Lifetime {get; set;}
		void Add(IVisNode n);
		void Extend(IEnumerable<IVisNode> nl);
		bool Next();
		void Run();
	}
	
	public abstract class BaseSearchAlgorithm : ISearchAlgorithm {
		public C5.IntervalHeap<IVisNode> Agenda {get; set;}
		public Dictionary<int[,], IVisNode> Visited {get; set;}
		public NodeActionDelegate NodeAction {get; set;}
		public int Lifetime {get; set;}
		public HeuristicDelegate NodeHeuristic {get; set;}
		public TerminationDelegate NodeTermination {get; set;}
		public NodeActionDelegate NewBestNode {get; set;}
		public int BestHeu {get; set;}
		public virtual void Add(IVisNode n) {
			this.Agenda.Add(n);
			this.Visited.Add(n.Data, n);
		}
		public void Extend(IEnumerable<IVisNode> nl) {
			foreach (var n in nl) {
				Add(n);
			}
		}
		public virtual bool Next() {return false;}
		public void Run() {
			while (this.Next()) {};
		}
	}
	
	public class SearchAstar : BaseSearchAlgorithm {
		public SearchAstar(NodeActionDelegate nadel, HeuristicDelegate heudel, TerminationDelegate termdel, NodeActionDelegate nbest) {
			Agenda = new C5.IntervalHeap<IVisNode>(new VisNodeComparer());
			Visited = new Dictionary<int[,], IVisNode>(new MatrixEqualityComparerInt());
			NodeAction = nadel;
			NodeHeuristic = heudel;
			NodeTermination = termdel;
			NewBestNode = nbest;
			Lifetime = int.MaxValue;
			BestHeu = int.MaxValue;
		}
		
		public SearchAstar(NodeActionDelegate nadel, HeuristicDelegate heudel, TerminationDelegate termdel)
			: this(nadel, heudel, termdel, (IVisNode v) => {}) {}
		
		public SearchAstar(NodeActionDelegate nadel, HeuristicDelegate heudel)
			: this(nadel, heudel, (IVisNode v) => {return false;}) {}
		
		public SearchAstar(NodeActionDelegate nadel)
			: this(nadel, (IVisNode v) => {return 0;}) {}
		
		public override void Add(IVisNode n) {
			n.Heuv = NodeHeuristic(n);
			this.Agenda.Add(n);
			this.Visited.Add(n.Data, n);
		}
		public override bool Next() {
			if (Lifetime != int.MaxValue) {
				if (--Lifetime <= 0) return false;
			}
			IVisNode cn = null;
			while (cn == null || cn.Cost > cn.MaxCost) {
				if (Agenda.IsEmpty) return false;
				cn = Agenda.DeleteMin();
			}
			NodeAction(cn);
			if (cn.Heuv < BestHeu) {
				NewBestNode(cn);
				BestHeu = cn.Heuv;
			}
			if (NodeTermination(cn)) return false;
			var nvals = cn.Next();
			foreach (var x in nvals) {
				if (Visited.ContainsKey(x.Data)) {
					continue;
				} else {
					Add(x);
				}
			}
			return true;
		}
	}
}
