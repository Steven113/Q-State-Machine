using System;
using UnityEngine;
using System.Collections.Generic;

using AssemblyCSharp;
using QGraphLearning;


namespace AssemblyCSharp
{
	public class QGraphController : MonoBehaviour
	{
		[SerializeField]string [] possibleStates;
		[SerializeField]string [] possibleActions;

		//public StringFloatMap [] floatRestrictions;

		[SerializeField]float edgeMutationChance = 0.1f;
		[SerializeField]float actionMutationChance = 0.1f;
		[SerializeField]float addNodeChance = 0.8f;
		[SerializeField]float changeInterruptWeightChance = 0.01f;
		[SerializeField]float numNodesToConnectToNewNode = 0.5f;
		[SerializeField]float numNodesToConnectNewNodeTo = 0.5f;

		[SerializeField]StringFloatMap [] floatMult;

		[SerializeField]FloatRange [] float_restriction_range;

		[SerializeField]List<SAConstraint> stateConstraints;
		[SerializeField]List<SAConstraint> actionConstraints;
		[SerializeField]List<ComparisonOperator> comparisonOperators;

		QGraph [] graphs;

		[SerializeField]int numGraphs = 5;

		[SerializeField]float mutationIncrement = 0.01f;

		[SerializeField]TextAsset heuristicInitFile;

		[SerializeField]int windowSize;

		[SerializeField]float timeCostDiscount;

		[SerializeField]bool useSmartMutation = false;

		public void Awake(){
			int f_l = float_restriction_range.Length;
			List<FloatRange> float_r = new List<FloatRange> (float_restriction_range);
			List<float> float_m = new List<float> (f_l);
			//List<float> float_noise = new List<float> (f_l);
			for (int i = 0; i < f_l; ++i) {
				//float_r.Add (floatRestrictions [i].second);
				float_m.Add (floatMult [i].second);
				//float_noise.Add (multiplier_randomness [i].second);
			}

			graphs = new QGraph[numGraphs];

			for (int i = 0; i < numGraphs; ++i) {
				if (heuristicInitFile == null || string.IsNullOrEmpty(heuristicInitFile.text)) {
					/*
					 * Generate random graph
					 */
					graphs [i] = new QGraph (possibleStates, possibleActions, float_m, float_r, stateConstraints,actionConstraints,edgeMutationChance, actionMutationChance,addNodeChance, changeInterruptWeightChance,numNodesToConnectToNewNode,numNodesToConnectNewNodeTo,comparisonOperators,this.windowSize,this.timeCostDiscount);
					graphs [i].MutationIncrement = mutationIncrement;
				} else {
					/*
					 * Parse QGraph from file
					 */
					graphs [i] = new QGraph (heuristicInitFile);
					graphs [i].MutationIncrement = mutationIncrement;
					if (i > 0) {
						graphs [i] = QGraph.Mutate (graphs [i],false);
					}
				}

				graphs [i].ActionConstraints = new ConstraintMapping (actionConstraints);
				graphs [i].StateConstraints = new ConstraintMapping (stateConstraints);

			}


		}

		public void Evolve(bool reinjectOriginal){
			graphs = QGraph.Evolve (graphs,useSmartMutation);
			if (reinjectOriginal) {
				Array.Sort(graphs);
				Array.Reverse(graphs);
				graphs[graphs.Length-1] = new QGraph (heuristicInitFile);
				graphs = Utils.ShuffleArray(graphs);
			}
		}

		public QGraph[] Graphs {
			get {
				return graphs;
			}
			set {
				graphs = value;
			}
		}

		public int NumGraphs {
			get {
				return numGraphs;
			}
			set {
				numGraphs = value;
			}
		}
	}
}

