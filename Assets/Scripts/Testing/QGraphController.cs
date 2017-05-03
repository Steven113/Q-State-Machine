using System;
using UnityEngine;
using System.Collections.Generic;

using AssemblyCSharp;


namespace AssemblyCSharp
{
	public class QGraphController : MonoBehaviour
	{
		public string [] possibleStates;
		public string [] possibleActions;

		//public StringFloatMap [] floatRestrictions;

		public StringFloatMap [] floatMult;

		public FloatRange [] float_restriction_range;

		public List<SAConstraint> stateConstraints;
		public List<SAConstraint> actionConstraints;

		QGraph [] graphs;

		public int numGraphs = 5;

		public float mutationIncrement = 0.01f;

		public TextAsset heuristicInitFile;

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
					graphs [i] = new QGraph (possibleStates, possibleActions, float_m, float_r, stateConstraints,actionConstraints);
					graphs [i].MutationIncrement = mutationIncrement;
				} else {
					graphs [i] = new QGraph (heuristicInitFile);
					graphs [i].MutationIncrement = mutationIncrement;
					if (i > 0) {
						graphs [i] = QGraph.Mutate (graphs [i]);
					}
				}

				graphs [i].ActionConstraints = new ConstraintMapping (actionConstraints);
				graphs [i].StateConstraints = new ConstraintMapping (stateConstraints);

			}


		}

		public void Evolve(){
			graphs = QGraph.Evolve (graphs);
		}

		public QGraph[] Graphs {
			get {
				return graphs;
			}
			set {
				graphs = value;
			}
		}
	}
}

