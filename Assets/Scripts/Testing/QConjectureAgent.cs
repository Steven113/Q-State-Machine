 //------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34209
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;


namespace AssemblyCSharp
{
	public class QConjectureAgent : QAgent
	{

		//public float breedingInterval = 0f;
		public bool loadLearnerFromFile = true;

		public QConjectureLearner learner = null;

		public bool canPrintConjectures = false;

		//get the action(s) a agent should perform given it's current state
		// we use the list of strings rather than one string as a return parameter as we want to facilitate concept-action-mapping

		public override List<string> GetAction (List<string> state,List<float> values){
			return learner.GetAction (state,values);
		}
		//give the agent a reward instantly. The agent will add a reduced amount of the given reward value for the reward for it's given state/action pair
		public bool RewardAgent (float reward, float reflexCompensationTime = 0){
			return learner.RewardAgent (reward, reflexCompensationTime);
		}

		public override bool RewardAgent (float reward){
			return learner.RewardAgent (reward, 0);
		}

		public void Awake(){
			if (loadLearnerFromFile) {
				UnityEngine.Debug.Assert (Utils.DeserializeFile<QConjectureLearner> (qFileName, ref learner));
			}
		}

		public void Update(){
//			if (canPrintConjectures && Input.GetKeyDown (KeyCode.Q)) {
//				learner.PrintConjectures();
//			}
		}

		public override void Reset(){

		}


	}
}

