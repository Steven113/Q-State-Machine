using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace AssemblyCSharp
{
	public class QGraph : IComparable<QGraph>
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

			//Debug.Assert (s_c > 0);
			Debug.Assert (a_c > 0);

			for (int i = 0; i < a_c; ++i) {
				QGraphNode tempNode = new QGraphNode (actions [i]);
				nodes.Add (tempNode);
			}

			//randomly plug root node into other nodes
			for (int i = 1; i < a_c + 1; ++i) {
				QGraphEdge temp_edge = new QGraphEdge (i);
				if (s_c > 0) {
					temp_edge.RequiredStates = new List<string>{ states [i % s_c] };
				}
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
					if (s_c > 0) {
						temp_edge.RequiredStates = new List<string>{ states [stateIndex] };
					}
					temp_edge.Float_restrictions = new List<float> (default_float_restrictions.ToArray ());
					nodes [i].AddEdge (temp_edge);

				}
			}

		}

		//load QGraph from file, for heuristic init
		public QGraph (TextAsset asset){
			List<string> lines = new List<string>(asset.text.Split (Environment.NewLine.ToCharArray ()));
			for (int i = 0; i < lines.Count; ++i) {
				if (lines [i].Contains ("//")) {
					lines.RemoveAt (i);
					--i;
				}
			}

			Dictionary<string,QGraphNode> neuronDict = new Dictionary<string, QGraphNode> ();

			Debug.Assert (lines.Count > 3);

			Utils.ConverterTU<string, float> f_conv = Utils.TryParseR;

			possibleStates = new List<string> (lines [0].Split (" ".ToCharArray()));
			possibleActions = new List<string> (lines [1].Split (" ".ToCharArray()));
			float_restriction = new List<float> (Utils.ConvertArrayType<string, float> (lines [2].Split(" ".ToCharArray()), f_conv));

			for (int i = 3; i < lines.Count; ++i) {
				if (lines [i].StartsWith ("Neuron")) {
					//format: Neuron [refName] [Action1] ... [ActionN]
					string [] neuronLine = lines [i].Split(" ".ToCharArray());
					QGraphNode node = new QGraphNode();

					Debug.Assert (neuronLine.Length > 0, "Line is missing a neuron name!");
					Debug.Assert (!neuronDict.ContainsKey (neuronLine [0]), "Node with this name is already defined!");

					for (int j = 1; j<neuronLine.Length; ++j){
						node.AddAction (neuronLine [j]);
					}
					nodes.Add (node);
					neuronDict.Add (neuronLine [0], node);
				} else if (lines [i].StartsWith ("Edge")) {
					//format: [startNode] [Node connected to] | [state1] ... [stateN] | [float_restriction1] ... [float_restrictionN] | [interruptChance]

					string [] lineSegs = lines[i].Split("|".ToCharArray());
					Debug.Assert (lineSegs.Length == 4);
					string[] edgeNodes = lineSegs [0].Split (" ".ToCharArray ());
					Debug.Assert (edgeNodes.Length == 2, "Start and end nodes not defined");
					Debug.Assert (neuronDict.ContainsKey (edgeNodes [0]), "Start neuron not defined");
					Debug.Assert (neuronDict.ContainsKey (edgeNodes [1]), "End neuron not defined");

					//string [] reqFloats = lineSegs[2].Split (" ".ToCharArray ());

					

//					List<float> restrictions = new List<float> (reqFloats.Length);
//
//					for (int j = 0; j < reqFloats.Length; ++j) {
//						float f = 0;
//						Debug.Assert (float.TryParse (reqFloats [j], out f));
//						restrictions.Add (f);
//					}

					float interruptChance = 0;

					Debug.Assert (float.TryParse (lineSegs [3], out interruptChance));


					QGraphEdge edge = new QGraphEdge (new List<string> (lineSegs [1].Split(" ".ToCharArray())), Utils.ConvertArrayType<string,float>(lineSegs[2].Split(" ".ToCharArray()), f_conv), nodes.IndexOf (neuronDict [edgeNodes [1]]));

					edge.InterruptThreshold = interruptChance;

					nodes [nodes.IndexOf (neuronDict [edgeNodes [0]])].AddEdge (edge);



				} else {
					throw new Exception ("Line definition format could not be determined");
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
			this.addNodeChance = other.addNodeChance;
			this.changeInterruptWeightChance = other.changeInterruptWeightChance;

			this.possibleStates = other.possibleStates;
			this.possibleActions = other.possibleActions;
			this.float_restriction = other.float_restriction;
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

			Debug.Assert (edgeToUse > -1);

			if (nodes [currentNode.outgoingEdges [edgeToUse].targetNode] == root) {
				edgeToUse = UnityEngine.Random.Range (1, nodes.Count);
			}

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
				int nodeToChange = UnityEngine.Random.Range(0,t_nodes.Count);
				int edgeToChange = UnityEngine.Random.Range(0,t_nodes[nodeToChange].outgoingEdges.Count);

//				if (nodeToChange >= t_nodes.Count || edgeToChange>=t_nodes[nodeToChange].outgoingEdges.Count) {
//					break;
//				}

				t_nodes [nodeToChange].outgoingEdges [edgeToChange] = QGraphEdge.MutateEdge (t_nodes [nodeToChange].outgoingEdges [edgeToChange], mutant.possibleStates, mutant.mutationIncrement);

			}

			//mutate nodes
			numChanges = mutant.nodes.Count * mutant.actionMutationChance;

			t_nodes = Utils.ShuffleList<QGraphNode> (t_nodes);

			for (int i = 0; i < numChanges; ++i) {
				int nodeToChange = UnityEngine.Random.Range(0,t_nodes.Count);

				t_nodes [nodeToChange] = QGraphNode.MutateNode (t_nodes [nodeToChange],mutant.possibleStates);

			}

			//add nodes
			numChanges = mutant.nodes.Count * mutant.addNodeChance;

			t_nodes = Utils.ShuffleList<QGraphNode> (t_nodes);

			if (numChanges < 1) {
				numChanges = 1f;
			}

			for (int i = 0; i < numChanges; ++i) {
				QGraphNode newNode = new QGraphNode (mutant.possibleActions [i % mutant.possibleActions.Count]);

				for (int j = 0; j < n_c; ++j) {
					QGraphEdge temp_edge = new QGraphEdge (j);
					if (mutant.possibleStates.Count > 0) {
						temp_edge.RequiredStates = new List<string>{ mutant.possibleStates [j % mutant.possibleStates.Count] };
					}
					temp_edge.Float_restrictions = new List<float> (mutant.float_restriction);
					newNode.AddEdge (temp_edge);
				}

			}

			return mutant;
		}

		public void Reward(float reward){
			totalReward += reward;
		}

		public int CompareTo(QGraph other){
			return totalReward.CompareTo (other.totalReward);
		}

		public static QGraph [] Evolve(QGraph [] population){
			Array.Sort (population);
			Array.Reverse (population);

			int p_c = population.Length;

			//for (int i = 1; i < p_c; ++i) {
			population [population.Length-1] = QGraph.Mutate (population [0]);
			//}

			for (int i = 0; i < p_c; ++i) {
				population[i].totalReward = 0;
			}

			return population;

		}

		public float TotalReward {
			get {
				return totalReward;
			}

			set {
				totalReward = value;
			}
		}


	}
}

