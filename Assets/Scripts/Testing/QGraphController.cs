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

		public StringFloatMap [] floatRestrictions;

		public StringFloatMap [] floatMult;

		QGraph [] graphs;

		public int numGraphs = 5;

		public float mutationIncrement = 0.01f;

		public void Awake(){
			int f_l = floatRestrictions.Length;
			List<float> float_r = new List<float> (f_l);
			List<float> float_m = new List<float> (f_l);
			for (int i = 0; i < f_l; ++i) {
				float_r.Add (floatRestrictions [i].second);
				float_m.Add (floatMult [i].second);
			}

			graphs = new QGraph[numGraphs];

			for (int i = 0; i < numGraphs; ++i) {
				graphs [i] = new QGraph (possibleStates, possibleActions, float_r, float_m);
				graphs [i].MutationIncrement = mutationIncrement;
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

