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
	
	public interface ISearchAlgorithm<T> {
		C5.IntervalHeap<T> Agenda {get; set;}
		HashSet<T> Visited {get; set;}
		NodeActionDelegate<T> NodeAction {get; set;}
		HeuristicDelegate<T> NodeHeuristic {get; set;}
		TerminationDelegate<T> NodeTermination {get; set;}
		NodeActionDelegate<T> FlushNodeCache {get; set;}
		NodeActionDelegate<T> FullFlushNodeCache {get; set;}
		int Lifetime {get; set;}
		void Add(T n);
		void AddNew(T n);
		void AddRange(IEnumerable<T> nl);
		void AddNewRange(IEnumerable<T> nl);
		bool Next();
		void Run();
		void Reset();
	}
	
	public abstract class BaseSearchAlgorithm<T> : ISearchAlgorithm<T> {
		public C5.IntervalHeap<T> Agenda {get; set;}
		public HashSet<T> Visited {get; set;}
		public NodeActionDelegate<T> NodeAction {get; set;}
		public int Lifetime {get; set;}
		public HeuristicDelegate<T> NodeHeuristic {get; set;}
		public TerminationDelegate<T> NodeTermination {get; set;}
		public NodeActionDelegate<T> FlushNodeCache {get; set;}
		public NodeActionDelegate<T> FullFlushNodeCache {get; set;}
		public int BestHeu {get; set;}
		public virtual void Add(T n) {
			this.Agenda.Add(n);
			this.Visited.Add(n);
		}
		public virtual void AddNew(T n) {
			this.Agenda.Add(n);
			this.Visited.Add(n);
		}
		public void AddRange(IEnumerable<T> nl) {
			foreach (var n in nl) {
				Add(n);
			}
		}
		public void AddNewRange(IEnumerable<T> nl) {
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
	
	public class SearchAstar : BaseSearchAlgorithm<IVisNode> {
		public SearchAstar(NodeActionDelegate<IVisNode> nadel, HeuristicDelegate<IVisNode> heudel, TerminationDelegate<IVisNode> termdel, NodeActionDelegate<IVisNode> flushncache, NodeActionDelegate<IVisNode> fullflushncache) {
			Agenda = new C5.IntervalHeap<IVisNode>(new VisNodeComparer());
			Visited = new HashSet<IVisNode>();
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
			this.Visited.Add(n);
		}
		
		public override void Add(IVisNode n) {
			n.Heuv = NodeHeuristic(n);
			//if (n.Heuv < BestHeu) {
			//	FlushNodeCache(n);
			//	BestHeu = n.Heuv;
			//}
			this.Agenda.Add(n);
			this.Visited.Add(n);
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
				if (Visited.Contains(x)) {
					continue;
				} else {
					Add(x);
				}
			}
			return true;
		}
	}
}
