using System;
using System.Collections.Generic;
using UnityEngine;

namespace AssemblyCSharp
{
	public class QGraphAgent : QAgent
	{
		QGraph graph;

		[SerializeField]TextAsset graphDefinition;

		public void Awake(){
			if (graphDefinition != null && !graphDefinition.text.Equals("")) {
				graph = new QGraph (graphDefinition);
			}
		}

		public override List<string> GetAction (List<string> state, List<float> variables){
			return graph.GetActionsToTake (state, variables);
		}

		//give the agent a reward instantly. The agent will add a reduced amount of the given reward value for the reward for it's given state/action pair
		public override bool RewardAgent (float reward){
			graph.Reward (reward);
			return false;
		}

		public QGraph Graph {
			get {
				return graph;
			}
			set {
				graph = value;
			}
		}
	}
}

