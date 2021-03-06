﻿using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using QGraphLearning;

namespace AssemblyCSharp
{
	public class QGraphAgent : QAgent
	{
		QGraph graph;

		[SerializeField]TextAsset graphDefinition;

		[SerializeField]string fileSaveName = "";

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

		public override void Reset(){
			graph.ResetCurrentNodeToRoot ();
		}

		public QGraph Graph {
			get {
				return graph;
			}
			set {
				graph = value;
			}
		}

		public void OnDestroy(){
			//if (File.Exists (fileSaveName)) {
			Utils.SerializeToFileJSON<QGraph> (ref graph, fileSaveName + "_ID_"+ graph.ID +"_"+gameObject.transform.name+"_"+DateTime.Now.Ticks + ".json");
			//}
		}
	}
}

