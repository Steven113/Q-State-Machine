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


namespace AssemblyCSharp
{
	//this stores the 
	[Serializable]
	public class QConjectureMap : IComparable<QConjectureMap>
	{
		public int [] stateBinaryString;
		public int [] actionBinaryString;
		public float timeWhenConjectureWasLastSelected = 0;
		public float fitness = 0;

		public float learningRate = 0.1f;
		public float discountFactor = 0.3f;

		public int generation = 0;

		public QConjectureMap(int [] stateBinaryString, int [] actionBinaryString, float learningRate, float discountFactor, int generation){
			this.stateBinaryString = stateBinaryString;
			this.actionBinaryString = actionBinaryString;
			this.discountFactor = discountFactor;
			this.learningRate = learningRate;
			this.generation = generation;
		}

		//the storage of states in binary form means we can test whether a QConjecture's concept contraints contains the current state using a simple AND operation.
		public bool StatesCorrespond(int [] inputState){
			if (inputState.Length != stateBinaryString.Length) {
				return false;
			} else {
				for (int i = 0; i<inputState.Length; ++i){
					if ((stateBinaryString[i] & inputState[i])<(inputState[i])){
						return false;
					}
				}
			}
			return true;
		}

		public void Reward(float reward){
			if (timeWhenConjectureWasLastSelected != 0) {
				float adjustedTime = Mathf.Max (Time.time, 0.01f);
				//fitness = learningRate*(reward * (((adjustedTime) - timeWhenConjectureWasLastSelected) / Time.time))+(1-learningRate)*fitness;
				fitness += (reward * (((adjustedTime) - timeWhenConjectureWasLastSelected) / Time.time));
			}
		}

		public void ResetTime(){
			timeWhenConjectureWasLastSelected = 0;
		}

		public int CompareTo(QConjectureMap other){
			if ((this.fitness == other.fitness)) {
				return 0;
			} else if (this.fitness < other.fitness) {
				return -1;
			} else {
				return 1;
			}
		}

		public string ToString(){
			string result = "State String ";
			for (int i = 0; i<stateBinaryString.Length; ++i) {
				result += Convert.ToString(stateBinaryString[i], 2).PadLeft(32, '0');
				result += " ";
			}
			//result += Environment.NewLine;
			result += " Action String: ";

			for (int i = 0; i<actionBinaryString.Length; ++i) {
				result += Convert.ToString(actionBinaryString[i], 2).PadLeft(32, '0');
				result += " ";
			}
			return result;
		}
	}
}

