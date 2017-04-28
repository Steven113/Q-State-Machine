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

		/*the learning rate and discount factor are determined by the learning agent that manages this conjecture, so we can just pass it in
		 * rather than waste CPU cycles on parsing the string to get the learning rate and discount factor
		 */
		public QConjectureMap(string data, float learningRate, float discountFactor){

			this.discountFactor = discountFactor;
			this.learningRate = learningRate;

			List<string> splitData = new List<string>(data.Split ("|".ToCharArray ()));
			Debug.Assert (splitData.Count == 3);
			List<string> splitStringForStates = new List<string>(splitData [0].Split ("-".ToCharArray ()));
			stateBinaryString = new int[splitStringForStates.Count];
			for (int i = 0; i<stateBinaryString.Length; ++i) {
				int temp = 0;
				Debug.Assert(int.TryParse(splitStringForStates[i], out temp));
				stateBinaryString[i] = temp;
			}

			List<string> splitStringForActions = new List<string>(splitData [1].Split ("-".ToCharArray ()));
			stateBinaryString = new int[splitStringForActions.Count];
			for (int i = 0; i<actionBinaryString.Length; ++i) {
				int temp = 0;
				Debug.Assert(int.TryParse(splitStringForActions[i], out temp));
				actionBinaryString[i] = temp;
			}
			Debug.Assert (int.TryParse (splitData [2], out generation));
		}

		public QConjectureMap(QConjectureMap other){
			this.stateBinaryString = new int[other.stateBinaryString.Length];
			for (int i = 0; i<stateBinaryString.Length; ++i) {
				stateBinaryString[i]=other.stateBinaryString[i];
			}

			this.actionBinaryString = new int[other.actionBinaryString.Length];
			for (int i = 0; i<actionBinaryString.Length; ++i) {
				actionBinaryString[i]=other.actionBinaryString[i];
			}

			this.timeWhenConjectureWasLastSelected = other.timeWhenConjectureWasLastSelected;
			this.fitness = other.fitness;
			this.learningRate = other.learningRate;
			this.discountFactor = other.discountFactor;
			this.generation = other.generation;
		}

		public string ToFileFormat(){
			string result = "";
			for (int i = 0; i<stateBinaryString.Length; ++i) {
				result = result + stateBinaryString[i];
				if (i<stateBinaryString.Length-1){
					result = result + "-";
				}
			}

			result = result + "|";

			for (int i = 0; i<actionBinaryString.Length; ++i) {
				result = result + actionBinaryString[i];
				if (i<actionBinaryString.Length-1){
					result = result + "-";
				}
			}

			result = result + "|" + generation;

			return result;
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

		public void Reward(float reward,float humanResponseDelayCompensation = 0){
			if (timeWhenConjectureWasLastSelected != 0) {
				float adjustedTime = Mathf.Max (Time.time, 0.01f);
				//fitness = learningRate*(reward * (((adjustedTime) - timeWhenConjectureWasLastSelected) / Time.time))+(1-learningRate)*fitness;
				//fitness += (reward * (((adjustedTime) - timeWhenConjectureWasLastSelected) / Time.time));
				if ((adjustedTime-humanResponseDelayCompensation) >0 && (adjustedTime+humanResponseDelayCompensation)<Time.time){
				adjustedTime+=humanResponseDelayCompensation;
				float timeMultiplier = (1f/(((1f+(adjustedTime) - timeWhenConjectureWasLastSelected))));
				fitness += (reward * timeMultiplier*timeMultiplier);
				}
				//fitness = learningRate*(reward * (1f/(((1f+(adjustedTime) - timeWhenConjectureWasLastSelected)))))+(1-learningRate)*fitness;
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

