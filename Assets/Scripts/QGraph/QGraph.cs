using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using AssemblyCSharp;

namespace QGraphLearning
{
	[Serializable]
	/// <summary>
	/// Represents a QGraph, a state matchine allowing varying numbers of constraints
	/// for edge transitions, and variable numbers of actions to take upon transitioning.
	/// A QGraph can also be mutated
	/// </summary>
	public class QGraph : IComparable<QGraph>
	{

		static int numGraphs;
		public int ID;
		float totalReward = 0;
		public List<QGraphNode> nodes = new List<QGraphNode> ();
		QGraphNode currentNode;
		QGraphNode root;
		//this node is the "default" node - if the current node has no edge for the current state, this is the selected node

		ConstraintMapping actionConstraints = new ConstraintMapping ();
		ConstraintMapping stateConstraints = new ConstraintMapping ();
		int numActions = 0;
		float mutationIncrement = 0.001f;
		float edgeMutationChance = 0.1f;
		float actionMutationChance = 0.1f;
		float addNodeChance = 0.8f;
		float changeInterruptWeightChance = 0.01f;
		float numNodesToConnectToNewNode = 0.5f;
		float numNodesToConnectNewNodeTo = 0.5f;
		List<string> possibleStates;
		List<string> possibleActions;
		List<FloatRange> float_restriction_range;
		List<float> float_mult;
		List<ComparisonOperator> comparison_operators = new List<ComparisonOperator> ();
		List<QGraphNode> memoryWindow; //a sequence of the previously taken action
		int windowSize = 20; //max number of items in memoryWindow
		int windowIndex = 0; //index of latest action previously taken
		float timeCostDiscount = 0.5f;

		/// <summary>
		/// Generate a random QGraph using the given info
		/// </summary>
		/// <param name="possibleStates">Possible states.</param>
		/// <param name="possibleActions">Possible actions.</param>
		/// <param name="default_float_mult">Default float mult.</param>
		/// <param name="default_restriction_range">Default restriction range.</param>
		/// <param name="stateConstraints">State constraints.</param>
		/// <param name="actionConstraints">Action constraints.</param>
		/// <param name="edgeMutationChance">Edge mutation chance.</param>
		/// <param name="actionMutationChance">Action mutation chance.</param>
		/// <param name="addNodeChance">Add node chance.</param>
		/// <param name="changeInterruptWeightChance">Change interrupt weight chance.</param>
		/// <param name="numNodesToConnectToNewNode">Number nodes to connect to new node.</param>
		/// <param name="numNodesToConnectNewNodeTo">Number nodes to connect new node to.</param>
		/// <param name="comparison_ops">Comparison ops.</param>
		/// <param name="windowSize">Window size.</param>
		/// <param name="timeCostDiscount">Time cost discount.</param>
		public QGraph (IEnumerable<string> possibleStates, IEnumerable<string> possibleActions, IEnumerable<float> default_float_mult, IEnumerable<FloatRange> default_restriction_range, IEnumerable<SAConstraint> stateConstraints, IEnumerable<SAConstraint> actionConstraints, float edgeMutationChance, float actionMutationChance, float addNodeChance, float changeInterruptWeightChance, float numNodesToConnectToNewNode, float numNodesToConnectNewNodeTo, IEnumerable<ComparisonOperator> comparison_ops, int windowSize, float timeCostDiscount)
		{

			this.stateConstraints = new ConstraintMapping (stateConstraints.ToList ());

			this.actionConstraints = new ConstraintMapping (actionConstraints.ToList ());

			this.comparison_operators = comparison_ops.ToList ();

			this.timeCostDiscount = timeCostDiscount;

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
			this.float_restriction_range = new List<FloatRange> (default_restriction_range.ToArray ());

			this.edgeMutationChance = edgeMutationChance;
			this.actionMutationChance = actionMutationChance;
			this.addNodeChance = addNodeChance;
			this.changeInterruptWeightChance = changeInterruptWeightChance;
			this.numNodesToConnectToNewNode = numNodesToConnectToNewNode;
			this.numNodesToConnectNewNodeTo = numNodesToConnectNewNodeTo;


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
				temp_edge.Comparison_operators = new List<ComparisonOperator> (this.comparison_operators);
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
					for (int k = 0; k<float_restriction_range.Count; ++k) {
						temp_edge.Float_restrictions.Add (UnityEngine.Random.Range (float_restriction_range [k].min, float_restriction_range [k].max));
					}
					temp_edge.Float_mult = new List<float> (default_float_mult.ToArray ());
					temp_edge.Comparison_operators = new List<ComparisonOperator> (this.comparison_operators);
					nodes [i].AddEdge (temp_edge);

				}
			}

			//init window
			this.windowSize = windowSize;
			memoryWindow = new List<QGraphNode> (this.windowSize);
			memoryWindow.Add (currentNode);

		}

		/// <summary>
		/// Load QGraph from given text file, for heuristic initialization
		/// </summary>
		/// <param name="asset">Asset.</param>
		public QGraph (TextAsset asset)
		{
			ID = numGraphs;

			++numGraphs;

			List<string> lines = new List<string> (asset.text.Split (Environment.NewLine.ToCharArray ()));
			for (int i = 0; i < lines.Count; ++i) {
				if (lines [i].Contains ("//") || string.IsNullOrEmpty (lines [i])) {
					Debug.Log ("Removing: " + lines [i]);
					lines.RemoveAt (i);
					--i;
				}
			}

			Dictionary<string,QGraphNode> neuronDict = new Dictionary<string, QGraphNode> ();

			Debug.Assert (lines.Count > 7);

			Utils.ConverterTU<string, float> f_conv = Utils.TryParseRandomValue;

			Debug.Log (lines [0]);
			possibleStates = new List<string> (lines [0].Split (" ".ToCharArray ()));
			Debug.Log (lines [1]);
			possibleActions = new List<string> (lines [1].Split (" ".ToCharArray ()));
			Debug.Log (lines [2]);
			Debug.Log (lines [3]);
			float_restriction_range = new List<FloatRange> (FloatRange.ToFloatRange (new List<float> (Utils.ConvertArrayType<string, float> (lines [2].Split (" ".ToCharArray ()), f_conv)), new List<float> (Utils.ConvertArrayType<string, float> (lines [3].Split (" ".ToCharArray ()), f_conv))));
			float_mult = new List<float> (Utils.ConvertArrayType<string, float> (lines [4].Split (" ".ToCharArray ()), f_conv));
			List<float> evol_values = new List<float> (Utils.ConvertArrayType<string, float> (lines [5].Split (" ".ToCharArray ()), f_conv));

			string [] comparisonLine = lines [6].Split (" ".ToCharArray ());

			for (int i = 0; i<comparisonLine.Length; ++i) {
				this.comparison_operators.Add ((ComparisonOperator)Enum.Parse (typeof(ComparisonOperator), comparisonLine [i]));
			}

			Debug.Assert (evol_values.Count == 6);

			this.edgeMutationChance = evol_values [0];
			this.actionMutationChance = evol_values [1];
			this.addNodeChance = evol_values [2];
			this.changeInterruptWeightChance = evol_values [3];
			this.numNodesToConnectToNewNode = evol_values [4];
			this.numNodesToConnectNewNodeTo = evol_values [5];

			Debug.Assert (int.TryParse (lines [7], out this.windowSize));

			memoryWindow = new List<QGraphNode> (this.windowSize);
			//memoryWindow.Add (currentNode);

			for (int i = 8; i < lines.Count; ++i) {
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
					//format: [startNode] [Node connected to] | [state1] ... [stateN] | [float_restriction1] ... [float_restrictionN] | [mult1] ... [multN] | [op1] ... [opN] | [interruptChance]

					string[] lineSegs = lines [i].Split ("|".ToCharArray ());
					Debug.Assert (lineSegs.Length == 6);
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

					comparisonLine = lineSegs [4].Split (" ".ToCharArray ());

					List<ComparisonOperator> comp_ops = new List<ComparisonOperator> (comparisonLine.Length);

					for (int j = 0; j<comparisonLine.Length; ++j) {
						comp_ops.Add ((ComparisonOperator)Enum.Parse (typeof(ComparisonOperator), comparisonLine [j]));
					}

					float interruptChance = 0;

					Debug.Assert (Utils.TryParseRandomValue (lineSegs [5], out interruptChance));


					QGraphEdge edge = new QGraphEdge (new List<string> (lineSegs [1].Split (" ".ToCharArray ())), Utils.ConvertArrayType<string,float> (lineSegs [2].Split (" ".ToCharArray ()), f_conv), Utils.ConvertArrayType<string,float> (lineSegs [3].Split (" ".ToCharArray ()), f_conv), nodes.IndexOf (neuronDict [edgeNodes [2]]), comp_ops);

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
					throw new Exception ("Line definition format could not be determined: " + i + " " + lines [i]);
				}
			}

			memoryWindow.Add (currentNode);
		}

		public void ResetCurrentNodeToRoot ()
		{
			currentNode = root;
		}

		/// <summary>
		/// Copy constructor of QGraph
		/// </summary>
		/// <param name="other">Other.</param>
		public QGraph (ref QGraph other)
		{

			this.actionConstraints = new ConstraintMapping (actionConstraints);
			this.stateConstraints = new ConstraintMapping (stateConstraints);
			this.memoryWindow = new List<QGraphNode> (other.memoryWindow);

			this.windowSize = other.windowSize;
			this.windowIndex = other.windowIndex;

			ID = numGraphs;

			++numGraphs;

			this.comparison_operators = new List<ComparisonOperator> (other.comparison_operators);

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

		/// <summary>
		/// Determine actions to take, based on the given enumerable and continuous states.
		/// </summary>
		/// <returns>The actions to take.</returns>
		/// <param name="states">States.</param>
		/// <param name="values">Values.</param>
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



			if (numActions == 0 || UnityEngine.Random.value > currentNode.outgoingEdges [edgeToUse].InterruptThreshold) {
				currentNode = nodes [currentNode.outgoingEdges [edgeToUse].targetNode];

				++windowIndex;
				windowIndex %= windowSize;
				if (windowIndex >= memoryWindow.Count) {
					memoryWindow.Add (currentNode);
				} else {
					memoryWindow [windowIndex] = currentNode;
				}

				return new List<string> (currentNode.Actions);
			} else {
				currentNode = nodes [currentNode.outgoingEdges [edgeToUse].targetNode];

				++windowIndex;
				windowIndex %= windowSize;
				memoryWindow [windowIndex] = currentNode;

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

		public static QGraph Mutate (QGraph graph, bool useSmartMutation)
		{
			if (useSmartMutation) {
				return SmartMutateQGraph (graph);
			} else {
				return BasicMutateQGraph (graph);
			}
		}

		static QGraph BasicMutateQGraph (QGraph graph)
		{
			QGraph mutant = new QGraph (ref graph);
			mutant.edgeMutationChance += mutant.mutationIncrement * (0.5f - UnityEngine.Random.value);
			mutant.actionMutationChance += mutant.mutationIncrement * (0.5f - UnityEngine.Random.value);
			mutant.addNodeChance += mutant.mutationIncrement * (0.5f - UnityEngine.Random.value);
			mutant.changeInterruptWeightChance += mutant.mutationIncrement * (0.5f - UnityEngine.Random.value);
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
				int nodeToChange = UnityEngine.Random.Range (0, t_nodes.Count);
				int edgeToChange = UnityEngine.Random.Range (0, t_nodes [nodeToChange].outgoingEdges.Count);
				//				if (nodeToChange >= t_nodes.Count || edgeToChange>=t_nodes[nodeToChange].outgoingEdges.Count) {
				//					break;
				//				}
				t_nodes [nodeToChange].outgoingEdges [edgeToChange] = QGraphEdge.MutateEdge (t_nodes [nodeToChange].outgoingEdges [edgeToChange], mutant.possibleStates, mutant.mutationIncrement, mutant.stateConstraints);
			}
			//mutate nodes
			numChanges = mutant.nodes.Count * mutant.actionMutationChance;
			t_nodes = Utils.ShuffleList<QGraphNode> (t_nodes);
			for (int i = 0; i < numChanges; ++i) {
				int nodeToChange = UnityEngine.Random.Range (0, t_nodes.Count);
				t_nodes [nodeToChange] = QGraphNode.MutateNode (t_nodes [nodeToChange], mutant.possibleActions, mutant.actionConstraints);
			}
			//add nodes
			numChanges = mutant.nodes.Count * mutant.addNodeChance;
			t_nodes = Utils.ShuffleList<QGraphNode> (t_nodes);
			if (numChanges < 1) {
				numChanges = 1f;
			}
			for (int i = 0; i < numChanges; ++i) {
				QGraphNode newNode = new QGraphNode (t_nodes [UnityEngine.Random.Range (0, t_nodes.Count - 1)]);
				newNode = QGraphNode.MutateNode (newNode, mutant.possibleActions, mutant.actionConstraints);
				int numNodesToConnectTo = (int)(n_c * mutant.numNodesToConnectToNewNode);
				if (numNodesToConnectTo < 1) {
					numNodesToConnectTo = 1;
				}
				List<QGraphNode> nodesToRandomlyConnectToTemp = new List<QGraphNode> (mutant.nodes);
				nodesToRandomlyConnectToTemp = Utils.ShuffleList (nodesToRandomlyConnectToTemp);
				//connect nodes to the new node
				for (int j = 0; j < numNodesToConnectTo; ++j) {
					QGraphEdge temp_edge = new QGraphEdge (mutant.nodes.Count);
					if (mutant.possibleStates.Count > 0) {
						temp_edge.RequiredStates = new List<string> {
							mutant.possibleStates [j % mutant.possibleStates.Count]
						};
					}
					temp_edge.Float_restrictions = new List<float> (mutant.float_restriction_range.Count);
					for (int k = 0; k < temp_edge.Float_restrictions.Count; ++k) {
						temp_edge.Float_restrictions.Add (UnityEngine.Random.Range (mutant.float_restriction_range [k].min, mutant.float_restriction_range [k].max));
					}
					temp_edge.Float_mult = new List<float> (mutant.float_mult);
					temp_edge.Comparison_operators = new List<ComparisonOperator> (mutant.comparison_operators);
					nodesToRandomlyConnectToTemp [j].AddEdge (temp_edge);
				}
				numNodesToConnectTo = (int)(n_c * mutant.numNodesToConnectNewNodeTo);
				if (numNodesToConnectTo < 1) {
					numNodesToConnectTo = 1;
				}
				//connect the new node to other nodes;
				nodesToRandomlyConnectToTemp = Utils.ShuffleList (nodesToRandomlyConnectToTemp);
				for (int j = 0; j < numNodesToConnectTo; ++j) {
					QGraphEdge temp_edge = new QGraphEdge (mutant.nodes.IndexOf (nodesToRandomlyConnectToTemp [j]));
					if (mutant.possibleStates.Count > 0) {
						temp_edge.RequiredStates = new List<string> {
							mutant.possibleStates [j % mutant.possibleStates.Count]
						};
					}
					temp_edge.Float_restrictions = new List<float> (mutant.float_restriction_range.Count);
					for (int k = 0; k < temp_edge.Float_restrictions.Count; ++k) {
						temp_edge.Float_restrictions.Add (UnityEngine.Random.Range (mutant.float_restriction_range [k].min, mutant.float_restriction_range [k].max));
					}
					temp_edge.Float_mult = new List<float> (mutant.float_mult);
					temp_edge.Comparison_operators = new List<ComparisonOperator> (mutant.comparison_operators);
					newNode.AddEdge (temp_edge);
				}
				mutant.nodes.Add (newNode);
			}
			mutant.ResetCurrentNodeToRoot ();
			mutant.memoryWindow = new List<QGraphNode> (graph.memoryWindow.Count);
			mutant.windowSize = graph.windowSize;
			mutant.windowIndex = 0;
			mutant.memoryWindow.Add (mutant.currentNode);
			return mutant;
		}

		static QGraph SmartMutateQGraph (QGraph graph)
		{
			QGraph mutant = new QGraph (ref graph);
			mutant.edgeMutationChance += mutant.mutationIncrement * (0.5f - UnityEngine.Random.value);
			mutant.actionMutationChance += mutant.mutationIncrement * (0.5f - UnityEngine.Random.value);
			mutant.addNodeChance += mutant.mutationIncrement * (0.5f - UnityEngine.Random.value);
			mutant.changeInterruptWeightChance += mutant.mutationIncrement * (0.5f - UnityEngine.Random.value);
			//			float mutationIncrement = 0.001f;
			//
			//			float edgeMutationChance = 0.1f;
			//			float actionMutationChance = 0.1f;
			//			float addNodeChance = 0.8f;
			//			float changeInterruptWeightChance = 0.01f;
			//mutate edges
			List<QGraphNode> t_nodes = new List<QGraphNode> (mutant.nodes);
			float n_c = t_nodes.Count;
			float numChanges = n_c;
			t_nodes = Utils.ShuffleList<QGraphNode> (t_nodes);
			float max_delta_reward = 0;
			float min_delta_reward = 0;
			//calculate max and min change in reward between two nodes
			for (int i = 0; i < n_c; ++i) {
				for (int j = 0; j < mutant.nodes [i].outgoingEdges.Count; ++j) {
					float delta = (mutant.nodes [mutant.nodes [i].outgoingEdges [j].targetNode].Reward - mutant.nodes [i].Reward);
					if (max_delta_reward < delta) {
						max_delta_reward = delta;
					}
					if (min_delta_reward > delta) {
						min_delta_reward = delta;
					}
				}
			}
			for (int i = 0; i < numChanges; ++i) {
				for (int j = 0; j < mutant.nodes [i].outgoingEdges.Count; ++j) {
					float mutationChance = (mutant.nodes [mutant.nodes [i].outgoingEdges [j].targetNode].Reward - mutant.nodes [i].Reward) / (max_delta_reward - min_delta_reward);
					if (UnityEngine.Random.value > mutationChance * 0.8f) {
						mutant.nodes [i].outgoingEdges [j] = QGraphEdge.MutateEdge (mutant.nodes [i].outgoingEdges [j], mutant.possibleStates, mutant.mutationIncrement, mutant.stateConstraints);
					}
				}
			}
			//mutate nodes
			//numChanges = mutant.nodes.Count * mutant.actionMutationChance;
			t_nodes = Utils.ShuffleList<QGraphNode> (t_nodes);
			for (int i = 0; i < numChanges; ++i) {
				int nodeToChange = UnityEngine.Random.Range (0, t_nodes.Count);
				t_nodes [nodeToChange] = QGraphNode.MutateNode (t_nodes [nodeToChange], mutant.possibleActions, mutant.actionConstraints);
			}
			//add nodes
			numChanges = mutant.nodes.Count * mutant.addNodeChance;
			t_nodes = Utils.ShuffleList<QGraphNode> (t_nodes);
			if (numChanges < 1) {
				numChanges = 1f;
			}
			for (int i = 0; i < numChanges; ++i) {
				QGraphNode newNode = new QGraphNode (t_nodes [UnityEngine.Random.Range (0, t_nodes.Count - 1)]);
				newNode = QGraphNode.MutateNode (newNode, mutant.possibleActions, mutant.actionConstraints);
				int numNodesToConnectTo = (int)(n_c * mutant.numNodesToConnectToNewNode);
				if (numNodesToConnectTo < 1) {
					numNodesToConnectTo = 1;
				}
				List<QGraphNode> nodesToRandomlyConnectToTemp = new List<QGraphNode> (mutant.nodes);
				nodesToRandomlyConnectToTemp = Utils.ShuffleList (nodesToRandomlyConnectToTemp);
				//connect nodes to the new node
				for (int j = 0; j < numNodesToConnectTo; ++j) {
					QGraphEdge temp_edge = new QGraphEdge (mutant.nodes.Count);
					if (mutant.possibleStates.Count > 0) {
						temp_edge.RequiredStates = new List<string> {
							mutant.possibleStates [j % mutant.possibleStates.Count]
						};
					}
					temp_edge.Float_restrictions = new List<float> (mutant.float_restriction_range.Count);
					for (int k = 0; k < temp_edge.Float_restrictions.Count; ++k) {
						temp_edge.Float_restrictions.Add (UnityEngine.Random.Range (mutant.float_restriction_range [k].min, mutant.float_restriction_range [k].max));
					}
					temp_edge.Float_mult = new List<float> (mutant.float_mult);
					temp_edge.Comparison_operators = new List<ComparisonOperator> (mutant.comparison_operators);
					nodesToRandomlyConnectToTemp [j].AddEdge (temp_edge);
				}
				numNodesToConnectTo = (int)(n_c * mutant.numNodesToConnectNewNodeTo);
				if (numNodesToConnectTo < 1) {
					numNodesToConnectTo = 1;
				}
				//connect the new node to other nodes;
				nodesToRandomlyConnectToTemp = Utils.ShuffleList (nodesToRandomlyConnectToTemp);
				for (int j = 0; j < numNodesToConnectTo; ++j) {
					QGraphEdge temp_edge = new QGraphEdge (mutant.nodes.IndexOf (nodesToRandomlyConnectToTemp [j]));
					if (mutant.possibleStates.Count > 0) {
						temp_edge.RequiredStates = new List<string> {
							mutant.possibleStates [j % mutant.possibleStates.Count]
						};
					}
					temp_edge.Float_restrictions = new List<float> (mutant.float_restriction_range.Count);
					for (int k = 0; k < temp_edge.Float_restrictions.Count; ++k) {
						temp_edge.Float_restrictions.Add (UnityEngine.Random.Range (mutant.float_restriction_range [k].min, mutant.float_restriction_range [k].max));
					}
					temp_edge.Float_mult = new List<float> (mutant.float_mult);
					temp_edge.Comparison_operators = new List<ComparisonOperator> (mutant.comparison_operators);
					newNode.AddEdge (temp_edge);
				}
				mutant.nodes.Add (newNode);
			}
			mutant.ResetCurrentNodeToRoot ();
			mutant.memoryWindow = new List<QGraphNode> (graph.memoryWindow.Count);
			mutant.windowSize = graph.windowSize;
			mutant.windowIndex = 0;
			mutant.memoryWindow.Add (mutant.currentNode);
			return mutant;
		}

		/// <summary>
		/// Add reward to QGraph instance
		/// </summary>
		/// <param name="reward">Reward.</param>
		public void Reward (float reward)
		{
			totalReward += reward;

			float t_reward = reward;

			for (int i = 0; i<windowSize; ++i) {
				if ((i + windowIndex) % windowSize >= memoryWindow.Count || memoryWindow [(i + windowIndex) % windowSize] == null) {
					break;
				} else {
					memoryWindow [(i + windowIndex) % windowSize].Reward += (t_reward * timeCostDiscount);
					t_reward -= (t_reward * timeCostDiscount);
				}
			}

		}

		/// <summary>
		/// Clear record of reward accumulated for each node
		/// </summary>
		public void ClearNodeRewards ()
		{
			for (int i = 0; i<nodes.Count; ++i) {
				nodes [i].Reward = 0;
			}
		}

		public int CompareTo (QGraph other)
		{
			return totalReward.CompareTo (other.totalReward);
		}

		/// <summary>
		/// Generates a population evolved from a given population of QGraph instances.
		/// Replace worst in population with mutated version of best in population.
		/// </summary>
		/// <param name="population">Population.</param>
		/// <param name="useSmartMutation">If set to <c>true</c> use smart mutation.</param>
		public static QGraph [] Evolve (QGraph[] population, bool useSmartMutation)
		{
			Array.Sort (population);
			Array.Reverse (population);

			int p_c = population.Length;

			//for (int i = 1; i < p_c; ++i) {
			population [population.Length - 1] = QGraph.Mutate (population [0],useSmartMutation);
			//}

			for (int i = 0; i < p_c; ++i) {
				population [i].totalReward = 0;
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

		public ConstraintMapping ActionConstraints {
			get {
				return actionConstraints;
			}
			set {
				actionConstraints = value;
			}
		}

		public ConstraintMapping StateConstraints {
			get {
				return stateConstraints;
			}
			set {
				stateConstraints = value;
			}
		}
	}
}

