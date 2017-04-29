using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace AssemblyCSharp
{
	public class QGraph
	{
		float totalReward = 0;

		public List<QGraphNode> nodes = new List<QGraphNode> ();

		QGraphNode currentNode;

		QGraphNode root;
		//this node is the "default" node - if the current node has no edge for the current state, this is the selected node

		int numActions = 0;

		float mutationIncrement = 0.001f;

		float edgeMutationChance = 0.1f;
		float actionMutationChance = 0.1f;
		float addNodeChance = 0.8f;
		float changeInterruptWeightChance = 0.01f;

		List<string> possibleStates;
		List<string> possibleActions;
		List<float> float_restriction;

		public QGraph (IEnumerable<string> possibleStates, IEnumerable<string> possibleActions, IEnumerable<float> default_float_restrictions)
		{
			root = new QGraphNode ();
			nodes.Add (root);
			currentNode = root;

			string[] states = possibleStates.ToArray ();
			string[] actions = possibleActions.ToArray ();


			this.possibleStates = new List<string> (states);
			this.possibleActions = new List<string> (actions);
			this.float_restriction = new List<float> (default_float_restrictions.ToArray ());

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

		public void ResetCurrentNodeToRoot(){
			currentNode = root;
		}

		public QGraph (QGraph other){
			int n_c = other.nodes.Count; 

			for (int i = 0; i < n_c; ++i) {
				nodes.Add (new QGraphNode (other.nodes [i]));
			}

			root = nodes [0];

			currentNode = nodes [other.nodes.IndexOf (other.currentNode)];

			this.numActions = other.numActions;

			this.mutationIncrement = other.mutationIncrement;

			this.edgeMutationChance = other.edgeMutationChance;
			this.actionMutationChance = other.actionMutationChance;
			this.addNodeChance = this.addNodeChance;
			this.changeInterruptWeightChance = this.changeInterruptWeightChance;
		}

		public List<string> GetActionsToTake (IEnumerable<string> states, IEnumerable<float> values)
		{
			//List<string> actions = new List<string> ();

			//numActions = false;
			int edgeToUse = -1;
			int edgeMatchLevel = 0;

			int e_c = currentNode.outgoingEdges.Count;

			currentNode.outgoingEdges = Utils.ShuffleList (currentNode.outgoingEdges);

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

			if (numActions ==0 || UnityEngine.Random.value > currentNode.outgoingEdges [edgeToUse].InterruptThreshold) {
				return new List<string> (nodes [currentNode.outgoingEdges [edgeToUse].targetNode].Actions);
			} else {
				return new List<string> ();
			}
		}

		public int BusyWithAction {
			get {
				return numActions;
			}
			set {
				numActions = value;
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

		public static QGraph Mutate(QGraph graph){
			QGraph mutant = new QGraph (graph);



			mutant.edgeMutationChance += mutant.mutationIncrement*(0.5f-UnityEngine.Random.value);
			mutant.actionMutationChance += mutant.mutationIncrement*(0.5f-UnityEngine.Random.value);
			mutant.addNodeChance += mutant.mutationIncrement*(0.5f-UnityEngine.Random.value);
			mutant.changeInterruptWeightChance += mutant.mutationIncrement*(0.5f-UnityEngine.Random.value);

//			float mutationIncrement = 0.001f;
//
//			float edgeMutationChance = 0.1f;
//			float actionMutationChance = 0.1f;
//			float addNodeChance = 0.8f;
//			float changeInterruptWeightChance = 0.01f;

			//mutate edges

			List<QGraphNode> t_nodes = new List<QGraphNode> (mutant.nodes);

			float numChanges = mutant.nodes.Count * mutant.edgeMutationChance;

			float n_c = t_nodes.Count;

			t_nodes = Utils.ShuffleList<QGraphNode> (t_nodes);

			for (int i = 0; i < numChanges; ++i) {
				int nodeToChange = (int)(mutant.nodes.Count * UnityEngine.Random.value);
				int edgeToChange = (int)(mutant.nodes [nodeToChange].outgoingEdges.Count * UnityEngine.Random.value);

				t_nodes [nodeToChange].outgoingEdges [edgeToChange] =QGraphEdge.MutateEdge (t_nodes [nodeToChange].outgoingEdges [edgeToChange], mutant.possibleStates, mutant.mutationIncrement);

			}

			//mutate nodes
			numChanges = mutant.nodes.Count * mutant.actionMutationChance;

			t_nodes = Utils.ShuffleList<QGraphNode> (t_nodes);

			for (int i = 0; i < numChanges; ++i) {
				int nodeToChange = (int)(mutant.nodes.Count * UnityEngine.Random.value);

				t_nodes [nodeToChange] = QGraphNode.MutateNode (t_nodes [nodeToChange],mutant.possibleStates);

			}

			//add nodes
			numChanges = mutant.nodes.Count * mutant.addNodeChance;

			t_nodes = Utils.ShuffleList<QGraphNode> (t_nodes);

			for (int i = 0; i < numChanges; ++i) {
				QGraphNode newNode = new QGraphNode (mutant.possibleActions [i % mutant.possibleActions.Count]);

				for (int j = 0; j < n_c; ++j) {
					QGraphEdge temp_edge = new QGraphEdge (j);
					temp_edge.RequiredStates = new List<string>{ mutant.possibleStates [j % mutant.possibleStates.Count] };
					temp_edge.Float_restrictions = new List<float> (mutant.float_restriction);
					newNode.AddEdge (temp_edge);
				}

			}

			return mutant;
		}

		public void Reward(float reward){
			totalReward += reward;
		}

		public float TotalReward {
			get {
				return totalReward;
			}
		}
	}
}

