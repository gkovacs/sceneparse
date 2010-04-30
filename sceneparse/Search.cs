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

namespace sceneparse
{
	public delegate void NodeActionDelegate<T>(T n);
	public delegate int HeuristicDelegate<T>(T n);
	public delegate bool TerminationDelegate<T>(T n);
	
	public interface ISearchAlgorithm {
		C5.IntervalHeap<IVisNode> Agenda {get; set;}
		Dictionary<int[,], IVisNode> Visited {get; set;}
		NodeActionDelegate<IVisNode> NodeAction {get; set;}
		HeuristicDelegate<IVisNode> NodeHeuristic {get; set;}
		TerminationDelegate<IVisNode> NodeTermination {get; set;}
		NodeActionDelegate<IVisNode> FlushNodeCache {get; set;}
		NodeActionDelegate<IVisNode> FullFlushNodeCache {get; set;}
		int Lifetime {get; set;}
		void Add(IVisNode n);
		void AddNew(IVisNode n);
		void AddRange(IEnumerable<IVisNode> nl);
		void AddNewRange(IEnumerable<IVisNode> nl);
		bool Next();
		void Run();
		void Reset();
	}
	
	public abstract class BaseSearchAlgorithm : ISearchAlgorithm {
		public C5.IntervalHeap<IVisNode> Agenda {get; set;}
		public Dictionary<int[,], IVisNode> Visited {get; set;}
		public NodeActionDelegate<IVisNode> NodeAction {get; set;}
		public int Lifetime {get; set;}
		public HeuristicDelegate<IVisNode> NodeHeuristic {get; set;}
		public TerminationDelegate<IVisNode> NodeTermination {get; set;}
		public NodeActionDelegate<IVisNode> FlushNodeCache {get; set;}
		public NodeActionDelegate<IVisNode> FullFlushNodeCache {get; set;}
		public int BestHeu {get; set;}
		public virtual void Add(IVisNode n) {
			this.Agenda.Add(n);
			this.Visited.Add(n.Data, n);
		}
		public virtual void AddNew(IVisNode n) {
			this.Agenda.Add(n);
			this.Visited.Add(n.Data, n);
		}
		public void AddRange(IEnumerable<IVisNode> nl) {
			foreach (var n in nl) {
				Add(n);
			}
		}
		public void AddNewRange(IEnumerable<IVisNode> nl) {
			foreach (var n in nl) {
				AddNew(n);
			}
		}
		public virtual bool Next() {return false;}
		public void Run() {
			while (this.Next()) {};
		}
		public void Reset() {
			while (!Agenda.IsEmpty) {
				Agenda.DeleteMin();
			}
			Visited.Clear();
		}
	}
	
	public class SearchAstar : BaseSearchAlgorithm {
		public SearchAstar(NodeActionDelegate<IVisNode> nadel, HeuristicDelegate<IVisNode> heudel, TerminationDelegate<IVisNode> termdel, NodeActionDelegate<IVisNode> flushncache, NodeActionDelegate<IVisNode> fullflushncache) {
			Agenda = new C5.IntervalHeap<IVisNode>(new VisNodeComparer());
			Visited = new Dictionary<int[,], IVisNode>(new MatrixEqualityComparerInt());
			NodeAction = nadel;
			NodeHeuristic = heudel;
			NodeTermination = termdel;
			FlushNodeCache = flushncache;
			FullFlushNodeCache = fullflushncache;
			Lifetime = int.MaxValue;
			BestHeu = int.MaxValue;
		}
		
		public SearchAstar(NodeActionDelegate<IVisNode> nadel, HeuristicDelegate<IVisNode> heudel, TerminationDelegate<IVisNode> termdel, NodeActionDelegate<IVisNode> flushncache)
			: this(nadel, heudel, termdel, flushncache, flushncache) {}
		
		public SearchAstar(NodeActionDelegate<IVisNode> nadel, HeuristicDelegate<IVisNode> heudel, TerminationDelegate<IVisNode> termdel)
			: this(nadel, heudel, termdel, (IVisNode v) => {}) {}
		
		public SearchAstar(NodeActionDelegate<IVisNode> nadel, HeuristicDelegate<IVisNode> heudel)
			: this(nadel, heudel, (IVisNode v) => {return false;}) {}
		
		public SearchAstar(NodeActionDelegate<IVisNode> nadel)
			: this(nadel, (IVisNode v) => {return 0;}) {}
		
		public override void AddNew(IVisNode n) {
			FullFlushNodeCache(n);
			n.Heuv = NodeHeuristic(n);
			//if (n.Heuv < BestHeu) {
			//	BestHeu = n.Heuv;
			//}
			this.Agenda.Add(n);
			this.Visited.Add(n.Data, n);
		}
		
		public override void Add(IVisNode n) {
			n.Heuv = NodeHeuristic(n);
			//if (n.Heuv < BestHeu) {
			//	FlushNodeCache(n);
			//	BestHeu = n.Heuv;
			//}
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
			if (cn.Heuv == int.MaxValue) return false;
			NodeAction(cn);
			if (cn.Heuv < BestHeu) {
				FlushNodeCache(cn);
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
