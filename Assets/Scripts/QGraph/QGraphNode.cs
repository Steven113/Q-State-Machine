using System;
using System.Collections.Generic;

namespace AssemblyCSharp
{
	public class QGraphNode
	{

		List<string> actions = new List<string>();
		public List<QGraphEdge> outgoingEdges = new List<QGraphEdge>();

		public QGraphNode ()
		{
			
		}

		public QGraphNode (string action)
		{
			AddAction (action);
		}

		public QGraphNode(QGraphNode other){
			actions = new List<string> (actions.Count);
			int e_c = outgoingEdges.Count; 

			for (int i = 0; i < e_c; ++i) {
				outgoingEdges.Add (new QGraphEdge (outgoingEdges [i]));
			}
		}

		/*we should only be adding one action at a time! Returns whether the action is already there
		 * Removes false if the action was already contained in the actions defined for the node */
		public bool AddAction(string action){
			if (!actions.Contains (action)) {
				actions.Add (action);
				return true;
			}
			return false;
		}

		/* Removes false if the action was not contained in the actions defined for the node. */
		public bool RemoveAction(string action){
			if (actions.Contains (action)) {
				actions.Remove (action);
				return true;
			}
			return false;
		}

		public void AddEdge(QGraphEdge edge){
			outgoingEdges.Add (edge);
		}

		public List<string> Actions {
			get {
				return actions;
			}
			set {
				actions = value;
			}
		}
	}
}

