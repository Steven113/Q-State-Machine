using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace AssemblyCSharp
{
	public class QGraph : IComparable<QGraph>
	{

		static int numGraphs;

		public int ID;

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
		List<FloatRange> float_restriction_range;
		List<float> float_mult;

		public QGraph (IEnumerable<string> possibleStates, IEnumerable<string> possibleActions, IEnumerable<float> default_float_mult, IEnumerable<FloatRange> default_restriction_range)
		{
			ID = numGraphs;

			++numGraphs;

			root = new QGraphNode ();
			nodes.Add (root);
			currentNode = root;

			string[] states = possibleStates.ToArray ();
			string[] actions = possibleActions.ToArray ();


			this.possibleStates = new List<string> (states);
			this.possibleActions = new List<string> (actions);
			this.float_mult = new List<float> (default_float_mult.ToArray ());
			this.float_restriction_range = new List<FloatRange>(default_restriction_range.ToArray ());

			Debug.Assert (float_restriction_range.Count == float_restriction_range.Count);

			this.possibleStates = Utils.ShuffleList (this.possibleStates);
			this.possibleActions = Utils.ShuffleList (this.possibleActions);

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
				temp_edge.Float_restrictions = new List<float> (float_restriction_range.Count);
				for (int j = 0; j < float_restriction_range.Count; ++j) {
					temp_edge.Float_restrictions.Add (UnityEngine.Random.Range (float_restriction_range [j].min, float_restriction_range [j].max));
					//temp_edge.Float_restrictions [j] = (UnityEngine.Random.Range (float_restriction_range, 1f));
					//temp_edge.Float_restrictions [j] = Mathf.ac
				}
				temp_edge.Float_mult = new List<float> (default_float_mult.ToArray ());
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
					temp_edge.Float_restrictions = new List<float> (float_restriction_range.Count);
					for (int k = 0; k<float_restriction_range.Count; ++k){
						temp_edge.Float_restrictions.Add (UnityEngine.Random.Range (float_restriction_range [k].min, float_restriction_range [k].max));
					}
					temp_edge.Float_mult = new List<float> (default_float_mult.ToArray ());
					nodes [i].AddEdge (temp_edge);

				}
			}

		}

		//load QGraph from file, for heuristic init
		public QGraph (TextAsset asset){
			ID = numGraphs;

			++numGraphs;

			List<string> lines = new List<string>(asset.text.Split (Environment.NewLine.ToCharArray ()));
			for (int i = 0; i < lines.Count; ++i) {
				if (lines [i].Contains ("//") || string.IsNullOrEmpty(lines[i])) {
					Debug.Log ("Removing: " + lines [i]);
					lines.RemoveAt (i);
					--i;
				}
			}

			Dictionary<string,QGraphNode> neuronDict = new Dictionary<string, QGraphNode> ();

			Debug.Assert (lines.Count > 5);

			Utils.ConverterTU<string, float> f_conv = Utils.TryParseR;

			Debug.Log (lines [0]);
			possibleStates = new List<string> (lines [0].Split (" ".ToCharArray()));
			Debug.Log (lines [1]);
			possibleActions = new List<string> (lines [1].Split (" ".ToCharArray()));
			Debug.Log (lines [2]);
			Debug.Log (lines [3]);
			float_restriction_range = new List<FloatRange>(FloatRange.ToFloatRange(new List<float> (Utils.ConvertArrayType<string, float> (lines [2].Split(" ".ToCharArray()), f_conv)),new List<float> (Utils.ConvertArrayType<string, float> (lines [3].Split(" ".ToCharArray()), f_conv))));
			float_mult = new List<float> (Utils.ConvertArrayType<string, float> (lines [4].Split (" ".ToCharArray ()), f_conv));

			for (int i = 5; i < lines.Count; ++i) {
				Debug.Log ("Parsing: " + lines [i]);
				lines [i] = lines [i].Trim ();
				if (lines [i].StartsWith ("Node")) {
					//format: Neuron [refName] [Action1] ... [ActionN]
					string[] neuronLine = lines [i].Split (" ".ToCharArray ());
					QGraphNode node = new QGraphNode ();

					Debug.Assert (neuronLine.Length > 1, "Line is missing a neuron name!");
					Debug.Assert (!neuronDict.ContainsKey (neuronLine [1]), "Node with this name is already defined!");

					for (int j = 2; j < neuronLine.Length; ++j) {
						node.AddAction (neuronLine [j]);
					}
					nodes.Add (node);
					neuronDict.Add (neuronLine [1], node);
				} else if (lines [i].StartsWith ("Edge")) {
					//format: [startNode] [Node connected to] | [state1] ... [stateN] | [float_restriction1] ... [float_restrictionN] | [mult1] ... [multN] | [interruptChance]

					string[] lineSegs = lines [i].Split ("|".ToCharArray ());
					Debug.Assert (lineSegs.Length == 5);
					string[] edgeNodes = lineSegs [0].Split (" ".ToCharArray ());
					Debug.Assert (edgeNodes.Length == 3, "Start and end nodes not defined");
					Debug.Assert (neuronDict.ContainsKey (edgeNodes [1]), "Start neuron not defined");
					Debug.Assert (neuronDict.ContainsKey (edgeNodes [2]), "End neuron not defined");

					//string [] reqFloats = lineSegs[2].Split (" ".ToCharArray ());

					

//					List<float> restrictions = new List<float> (reqFloats.Length);
//
//					for (int j = 0; j < reqFloats.Length; ++j) {
//						float f = 0;
//						Debug.Assert (float.TryParse (reqFloats [j], out f));
//						restrictions.Add (f);
//					}

					float interruptChance = 0;

					Debug.Assert (Utils.TryParseR (lineSegs [4], out interruptChance));


					QGraphEdge edge = new QGraphEdge (new List<string> (lineSegs [1].Split (" ".ToCharArray ())), Utils.ConvertArrayType<string,float> (lineSegs [2].Split (" ".ToCharArray ()), f_conv), Utils.ConvertArrayType<string,float> (lineSegs [3].Split (" ".ToCharArray ()), f_conv), nodes.IndexOf (neuronDict [edgeNodes [2]]));

					edge.InterruptThreshold = interruptChance;

					nodes [nodes.IndexOf (neuronDict [edgeNodes [1]])].AddEdge (edge);



				} else if (lines [i].StartsWith ("CurrentNode")) {
					string[] vals = lines [i].Split (" ".ToCharArray ());
					Debug.Assert (neuronDict.ContainsKey (vals [1]));
					currentNode = neuronDict [vals [1]];
				} else if (lines [i].StartsWith ("Root")) {
					string[] vals = lines [i].Split (" ".ToCharArray ());
					Debug.Assert (neuronDict.ContainsKey (vals [1]));
					root = neuronDict [vals [1]];
				} else {
					throw new Exception ("Line definition format could not be determined: "+i+ " " + lines [i]);
				}
			}
		}

		public void ResetCurrentNodeToRoot(){
			currentNode = root;
		}

		public QGraph (QGraph other){

			ID = numGraphs;

			++numGraphs;


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

			this.possibleStates = new List<string> (other.possibleStates);
			this.possibleActions = new List<string> (other.possibleActions);
			this.float_restriction_range = new List<FloatRange> (other.float_restriction_range);
			this.float_mult = new List<float> (other.float_mult);
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

			//Debug.Assert (edgeToUse > -1);

			if (edgeToUse < 0 || nodes [currentNode.outgoingEdges [edgeToUse].targetNode] == root) {
				edgeToUse = UnityEngine.Random.Range (0, currentNode.outgoingEdges.Count);
			}

			//Debug.Log (currentNode.outgoingEdges.Count);

			currentNode.outgoingEdges [edgeToUse].LastTriggeredTime = Time.time;



			if (numActions ==0 || UnityEngine.Random.value > currentNode.outgoingEdges [edgeToUse].InterruptThreshold) {
				currentNode = nodes [currentNode.outgoingEdges [edgeToUse].targetNode];
				return new List<string> (currentNode.Actions);
			} else {
				currentNode = nodes [currentNode.outgoingEdges [edgeToUse].targetNode];
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
					temp_edge.Float_restrictions = new List<float> (mutant.float_restriction_range.Count);

					for (int k = 0; k<temp_edge.Float_restrictions.Count; ++k){
						temp_edge.Float_restrictions.Add (UnityEngine.Random.Range (mutant.float_restriction_range [k].min, mutant.float_restriction_range [k].max));
					}

					temp_edge.Float_mult = new List<float> (mutant.float_mult);
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

