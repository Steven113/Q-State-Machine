using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace AssemblyCSharp
{
	public class QGraph
	{
		public List<QGraphNode> nodes = new List<QGraphNode> ();

		QGraphNode currentNode;

		QGraphNode root;
		//this node is the "default" node - if the current node has no edge for the current state, this is the selected node

		bool busyWithAction = false;

		float mutationIncrement = 0.001f;

		float edgeMutationChance = 0.1f;
		float actionMutationChance = 0.1f;
		float addNodeChance = 0.8f;
		float changeInterruptWeightChance = 0.01f;

		public QGraph (IEnumerable<string> possibleStates, IEnumerable<string> possibleActions, IEnumerable<float> default_float_restrictions)
		{
			root = new QGraphNode ();
			nodes.Add (root);
			currentNode = root;

			string[] states = possibleStates.ToArray ();
			string[] actions = possibleActions.ToArray ();

			int s_c = states.Count ();
			int a_c = actions.Count ();

			Debug.Assert (s_c > 0);
			Debug.Assert (a_c > 0);

			for (int i = 0; i < a_c; ++i) {
				QGraphNode tempNode = new QGraphNode (actions [i]);
				nodes.Add (tempNode);
			}

			//randomly plug root node into other nodes
			for (int i = 1; i < a_c + 1; ++i) {
				QGraphEdge temp_edge = new QGraphEdge (i);
				temp_edge.RequiredStates = new List<string>{ states [i % s_c] };
				temp_edge.Float_restrictions = new List<float> (default_float_restrictions.ToArray ());
				root.AddEdge (temp_edge);
			}

			states = Utils.ShuffleArray (states);

			int stateIndex = -1;

			for (int i = 1; i < a_c + 1; ++i) {
				for (int j = 1; j < a_c + 1; ++j) {
					if (i == j) {
						continue;
					}

					++stateIndex;

					if (stateIndex > s_c) {
						stateIndex = 0;
						states = Utils.ShuffleArray (states);
					}

					QGraphEdge temp_edge = new QGraphEdge (j);
					temp_edge.RequiredStates = new List<string>{ states [stateIndex] };
					temp_edge.Float_restrictions = new List<float> (default_float_restrictions.ToArray ());
					nodes [i].AddEdge (temp_edge);

				}
			}

		}

		public QGraph (QGraph other){
			int n_c = other.nodes.Count; 

			for (int i = 0; i < n_c; ++i) {
				nodes.Add (new QGraphNode (other.nodes [i]));
			}

			root = nodes [0];

			currentNode = nodes [other.nodes.IndexOf (other.currentNode)];

			this.busyWithAction = other.busyWithAction;

			this.mutationIncrement = other.mutationIncrement;

			this.edgeMutationChance = other.edgeMutationChance;
			this.actionMutationChance = other.actionMutationChance;
			this.addNodeChance = this.addNodeChance;
			this.changeInterruptWeightChance = this.changeInterruptWeightChance;
		}

		public List<string> GetActionsToTake (IEnumerable<string> states, IEnumerable<float> values)
		{
			//List<string> actions = new List<string> ();

			busyWithAction = false;
			int edgeToUse = -1;
			int edgeMatchLevel = 0;

			int e_c = currentNode.outgoingEdges.Count;

			for (int i = 0; i < e_c; ++i) {
				int t_matchLevel = currentNode.outgoingEdges [i].GetStateMatchLevel (states, values);
				if (t_matchLevel > edgeMatchLevel) {
					edgeToUse = i;
					edgeMatchLevel = t_matchLevel;
				}
			}

			Debug.Assert (edgeToUse > 0);

			currentNode.outgoingEdges [edgeToUse].LastTriggeredTime = Time.time;

			currentNode = nodes [currentNode.outgoingEdges [edgeToUse].targetNode];

			if (!busyWithAction || UnityEngine.Random.value > currentNode.outgoingEdges [edgeToUse].InterruptThreshold) {
				return new List<string> (nodes [currentNode.outgoingEdges [edgeToUse].targetNode].Actions);
			} else {
				return new List<string> ();
			}
		}

		public bool BusyWithAction {
			get {
				return busyWithAction;
			}
			set {
				busyWithAction = value;
			}
		}

		public float MutationIncrement {
			get {
				return mutationIncrement;
			}
			set {
				mutationIncrement = value;
			}
		}

		public QGraph Mutate(){
			QGraph mutant = new QGraph (this);



			edgeMutationChance += mutationIncrement*(0.5f-UnityEngine.Random.value);
			actionMutationChance += mutationIncrement*(0.5f-UnityEngine.Random.value);
			addNodeChance += mutationIncrement*(0.5f-UnityEngine.Random.value);
			changeInterruptWeightChance += mutationIncrement*(0.5f-UnityEngine.Random.value);

			float mutation_sum = edgeMutationChance + actionMutationChance + addNodeChance + changeInterruptWeightChance;

			float t_sum = 0;

			float r_val = mutation_sum*UnityEngine.Random.value;

			t_sum += edgeMutationChance;

			if (t_sum > r_val) {

			} else {

				t_sum += actionMutationChance;

				if (t_sum > r_val) {
					

				} else {

					t_sum += addNodeChance;

					if (t_sum > r_val) {

					} else {

					}
				}
			}

			return mutant;
		}
	}
}

