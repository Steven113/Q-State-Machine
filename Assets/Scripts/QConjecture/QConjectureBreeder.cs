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
using UnityEngine;
using System.Collections.Generic;


namespace AssemblyCSharp
{
	/*
	 * Given a list of learners, waits for them to learn enough examples, then takes the best performing conjectures, combines them, and assigns them back to all the learners
	 */
	public class QConjectureBreeder : MonoBehaviour
	{
		public int numExamplesToRunBeforeBreeding = 600;
		public List<QConjectureLearner> learnersToBreed = new List<QConjectureLearner>();

		void Update(){
			for (int i = 0; i<learnersToBreed.Count; ++i) {
				if (learnersToBreed[i].numExamplesRun>=numExamplesToRunBeforeBreeding){
					BreedLearners();
				}
			}
		}

		void BreedLearners(){
			float [] fitnessValuesByLearner = new float[learnersToBreed.Count];
			NodeList<QConjectureMap> []  conceptLearnerConjecturesByFitness = new NodeList<QConjectureMap>[learnersToBreed.Count];
			float totalFitness = 0;
			for (int i = 0; i<learnersToBreed.Count; ++i) {
				conceptLearnerConjecturesByFitness[i] = new NodeList<QConjectureMap>();
				conceptLearnerConjecturesByFitness[i].AddAll(learnersToBreed[i].conjectures);
				int exRunNum = learnersToBreed[i].numExamplesRun;
				fitnessValuesByLearner[i] = learnersToBreed[i].TotalFitness/(exRunNum>0?exRunNum:numExamplesToRunBeforeBreeding); //different learners could have executed different numbers of exampels, so we must take average fitness gained per example
				totalFitness+=fitnessValuesByLearner[i];
			}
			NodeList<QConjectureMap> conjecturesToBreed = new NodeList<QConjectureMap> ();

			for (int i = 0; i<learnersToBreed.Count; ++i) {
				int limit = (int)(learnersToBreed[i].conjectures.Count*(fitnessValuesByLearner[i]/totalFitness));
				for (int j = 0; j<limit; ++j){
					conjecturesToBreed.Add(conceptLearnerConjecturesByFitness[i][j]);
				}
			}

			for (int i = 0; i<learnersToBreed.Count; ++i) {
				learnersToBreed[i].GenerateBaseConjectures();
				learnersToBreed[i].conjectures.AddRange(conjecturesToBreed.toList());
				learnersToBreed[i].numExamplesRun = 0;
				learnersToBreed[i].ResetTimers();
				learnersToBreed[i].ResetFitness();
			}
		}
	}
}
