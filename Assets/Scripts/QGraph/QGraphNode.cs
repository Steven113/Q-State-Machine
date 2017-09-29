using System;
using System.Collections.Generic;
using AssemblyCSharp;

namespace QGraphLearning
{
	[Serializable]
	public class QGraphNode
	{

		List<string> actions = new List<string>();
		public List<QGraphEdge> outgoingEdges = new List<QGraphEdge>();

		float reward = 0;

		public QGraphNode ()
		{
			
		}

		public QGraphNode (string action)
		{
			AddAction (action);
		}

		public QGraphNode(QGraphNode other){
			actions = new List<string> (other.actions);
			int e_c = other.outgoingEdges.Count; 

			for (int i = 0; i < e_c; ++i) {
				outgoingEdges.Add (new QGraphEdge (other.outgoingEdges [i]));
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

		public static QGraphNode MutateNode(QGraphNode node, List<string> possibleActions,ConstraintMapping constraints){
			QGraphNode mutant = new QGraphNode (node);
			mutant.actions = Utils.RandomlyModifyList_FilterInvalidLists (possibleActions, mutant.actions,constraints);
			return mutant;
		}

		public List<string> Actions {
			get {
				return actions;
			}
			set {
				actions = value;
			}
		}

		public float Reward {
			get {
				return reward;
			}
			set {
				reward = value;
			}
		}
	}
}

