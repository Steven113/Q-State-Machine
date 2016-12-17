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
using UnityEngine;
using UnityEditor;


namespace AssemblyCSharp
{
	[Serializable]
	public class QConjectureLearner : EditorDisplay
	{
		public List<string> possibleStates = new List<string>();
		public bool[,] stateCombinationConstraints = new bool[0, 0];

		public List<string> possibleActions = new List<string>();
		public bool[,] actionCombinationConstraints = new bool[0, 0];
		public float learningRate = 0.1f;
		public float discountFactor = 0.3f;
		string nextState = "";
		string nextAction = "";
		int offset = 0;
		int currentGeneration = 0;
		public int numExamplesRun = 0;
		int numExamplesBeforeEvolving = 600;

		bool useDeltaStateLearning = false;

		int [] previousState;

		[NonSerialized]Vector3 scrollPos = Vector2.zero;

		public List<QConjectureMap> conjectures = new List<QConjectureMap> (possibleStates.Count);

		//Func<QConjectureLearner,bool> generationValidator = map => map

		public QConjectureLearner(List<string> possibleStates, List<string> possibleActions, float learningRate, float discountFactor){
			this.possibleStates = possibleStates;
			this.possibleActions = possibleActions;
			this.discountFactor = discountFactor;
			this.learningRate = learningRate;
			this.stateCombinationConstraints = new bool[possibleStates.Count,possibleStates.Count];
			this.actionCombinationConstraints = new bool[possibleActions.Count,possibleActions.Count];

			previousState = new int[possibleStates.Count / 32 + 1];

			for (int i = 0; i<possibleStates.Count; ++i) {
				for (int j = 0; j<possibleStates.Count; ++j) {
					stateCombinationConstraints[i,j] = true;
				}
			}

			for (int i = 0; i<possibleActions.Count; ++i) {
				for (int j = 0; j<possibleActions.Count; ++j) {
					actionCombinationConstraints[i,j] = true;
				}
			}
		}

		public QConjectureLearner(List<string> possibleStates, List<string> possibleActions, bool [,] stateCombinationConstraints, bool [,] actionCombinationConstraints, float learningRate, float discountFactor){
			this.possibleStates = possibleStates;
			this.possibleActions = possibleActions;
			this.stateCombinationConstraints = stateCombinationConstraints;
			this.actionCombinationConstraints = actionCombinationConstraints;
			this.discountFactor = discountFactor;
			this.learningRate = learningRate;
			Debug.Assert (stateCombinationConstraints.GetLength (0) == possibleStates.Count);
			Debug.Assert (stateCombinationConstraints.GetLength (1) == possibleStates.Count);
			Debug.Assert (actionCombinationConstraints.GetLength (0) == possibleActions.Count);
			Debug.Assert (actionCombinationConstraints.GetLength (1) == possibleActions.Count);

			previousState = new int[possibleStates.Count / 32 + 1];
		}

		/*for all strings in stringsToConvert, looks up the index i of the matching string in conversionReference
		 * and sets the ith bit in the binary string to 1.
		 * so all bits are set to 1
		 */
		int [] toBinaryString(List<string> stringsToConvert, List<string> conversionReference){
			int[] result = new int[conversionReference.Count/32+1];
			for (int i = 0; i<stringsToConvert.Count; ++i) {
				int index = conversionReference.IndexOf(stringsToConvert[i]);
				int byteIndex = index/32;
				int bitIndex = index%32;
				if (i<0){
					throw new IndexOutOfRangeException();
					return null;
				} else {
					result[byteIndex] = result[byteIndex] | ((1<<bitIndex));
				}

			}
			return result;
		}

		List<String> toStringList(int [] byteString, List<string> conversionReference){
			List<string> result = new List<string> (possibleStates.Count);
			for (int i = 0; i<byteString.Length; ++i) {
				for (int j = 0; j<32; ++j) {
					if (0<((1<<j) & byteString[i])){
						result.Add(conversionReference[i*32 + j]);
					}
				}
			}
			result.TrimExcess ();
			return result;
		}

		int [] getNoConstraintBinaryString(List<string> conversionReference){
			int[] result = new int[conversionReference.Count/32+1];
			for (int i = 0; i<result.Length; ++i) {
				result[i] = Int32.MaxValue;
			}
			return result;
		}

		public void GenerateBaseConjectures(bool resetGeneration = true){
			previousState = new int[possibleStates.Count / 32 + 1];
			conjectures.Clear ();
			if (resetGeneration) currentGeneration = 0;
			for (int i = 0; i<possibleActions.Count; ++i) {
				List<string> actionsToSelect = new List<string>(possibleActions.Count);
				actionsToSelect.Add(possibleActions[i]);
				for (int j = i+1; j<possibleActions.Count; ++j) {
					bool noConstraints = true;
					for (int k = 0; k<actionsToSelect.Count; ++k){
						if (!actionCombinationConstraints[possibleActions.IndexOf(actionsToSelect[k]),(j)]){
							noConstraints = false;
							break;
						}
					}
					//if (actionCombinationConstraints[i,j]){
						if (noConstraints){
							actionsToSelect.Add(possibleActions[j]);
						}
					//}
				}
				actionsToSelect.TrimExcess();
				int [] tempBitMask_A = toBinaryString(actionsToSelect,possibleActions);
				int [] tempBitMask_B = getNoConstraintBinaryString(possibleStates);
				conjectures.Add(new QConjectureMap(tempBitMask_B,tempBitMask_A,learningRate,discountFactor,currentGeneration));
			}
		}

		public List<string> GetAction(List<string> state){
			++numExamplesRun;
			int [] binaryStateString = toBinaryString (state, possibleStates);
			if (useDeltaStateLearning) {
				binaryStateString = Utils.XOR_Integer (binaryStateString, previousState);
			}
			int conjectureCount = conjectures.Count;
			for (int i = 0; i<conjectureCount; ++i) {
				if (conjectures[(i+offset)%conjectureCount].StatesCorrespond(binaryStateString)){ //we need a offset to stop the same conjecture from being chosen every time, so that all conjectures get a chance to be rewarded
					conjectures[(i+offset)%conjectureCount].timeWhenConjectureWasLastSelected = Time.time; //update time when string was last used


					if (numExamplesRun>numExamplesBeforeEvolving){
						numExamplesRun%=numExamplesBeforeEvolving;
						//Evolve();
					}
					previousState = binaryStateString;
					List<string> actions = toStringList(conjectures[(i+offset)%conjectureCount].actionBinaryString,possibleActions);
					//if (conjectures[(i+offset)%conjectureCount].generation == 0){
					offset = (i+offset+1)%conjectureCount;
					//} else {
					//	offset = 0;
					//}
					//actions = Utils.ShuffleList(actions);
					return actions;

				}
			}
			return null;
		}

		public bool RewardAgent(float reward){
			for (int i =0; i<conjectures.Count; ++i) {
				conjectures[i].Reward(reward);
			}
			return true;
		}
		//the evolve method simply breeds the top two conjectures, but these connjectures may not create specialized state-action mappings, and even when the mappings are specialized we are not necessarily doing the right specializations
		// the advantage of this method is that it does not exponentially expand the conjecture space
		public void Evolve(){
			++currentGeneration;
			NodeList<QConjectureMap> orderedConjectures = new NodeList<QConjectureMap> ();
			for (int i = 0; i<conjectures.Count; ++i) {
				orderedConjectures.Add(conjectures[i]);
			}
			//orderedConjectures.Remove (orderedConjectures [orderedConjectures.Count - 1]);
			//orderedConjectures.Remove (orderedConjectures [orderedConjectures.Count - 1]);

			//NodeList<QConjectureMap> newOrderedConjectures = new NodeList<QConjectureMap> ();

			//newOrderedConjectures.AddAll((orderedConjectures.toArray()));
			QConjectureMap [] conjectureArr = orderedConjectures.toArray ();

			for (int i = 1; i<conjectureArr.Length; ++i) {
				if (orderedConjectures.Count>2000){
					Debug.Log("Too many conjectures!");
					break;
				}
				if (conjectureArr [i].generation == currentGeneration - 1){
					orderedConjectures.AddAll(BreedConjectures (conjectureArr [0], conjectureArr [i]));
				}
			}


			//orderedConjectures.AddAll (BreedConjectures (orderedConjectures [0], orderedConjectures [1]));
			//orderedConjectures.AddAll (SpecializeConjecture (orderedConjectures [0]));



			conjectures = new List<QConjectureMap>(orderedConjectures.toList (ValidateGeneration));

			for (int i = 0; i<conjectures.Count; ++i) {
				conjectures[i].timeWhenConjectureWasLastSelected = 0; //we want to reset the timers to give the new conjectures a fair chance to gain rewards
				conjectures[i].fitness = 0; //experimental
			}

			offset = 0;
		}


		/*
		 * Takes a given conjecture and produces conjectures with one state constraint turned off and/or one action removed from the original
		 */
		public List<QConjectureMap> SpecializeConjecture(QConjectureMap original){
			int numSpecialisations = Utils.GetNumberOfOnesInBinaryString (original.stateBinaryString)*Utils.GetNumberOfOnesInBinaryString (original.actionBinaryString);
			List<QConjectureMap> children = new List<QConjectureMap> (numSpecialisations);
			//Debug.Log("Specializing:");
			//Debug.Log (original.ToString ());
			for (int i = 0; i<possibleStates.Count; ++i) {
				if (0<(original.stateBinaryString[i/32] & (1<<(i%32)))){
				//Debug.Log("Specializing bit "+i);
					int [] tempStateString = new int[original.stateBinaryString.Length];
					for (int j = 0; j<tempStateString.Length; j++){
						tempStateString[j] = original.stateBinaryString[j];
					}
					tempStateString[i/32] = tempStateString[i/32] ^ (1<<(i%32));

					children.Add(new QConjectureMap(tempStateString,original.actionBinaryString,learningRate,discountFactor,currentGeneration));
					children[children.Count-1].fitness = original.fitness + 10;
					//Debug.Log(children[children.Count-1].ToString());

					for (int j = 0; j<possibleActions.Count; ++j){
						if (0<(original.actionBinaryString[j/32] & (1<<(j%32)))){
							int [] tempActionString = new int[original.actionBinaryString.Length];
							for (int k = 0; k<tempActionString.Length; k++){
								tempActionString[k] = original.actionBinaryString[k];
							}
							tempActionString[j/32] = tempActionString[j/32] ^ (1<<(j%32));
							children.Add(new QConjectureMap(tempStateString,tempActionString,learningRate,discountFactor,currentGeneration));
							children[children.Count-1].fitness = original.fitness + 10;
							//Debug.Log(children[children.Count-1].ToString());
						}
					}
				}
			}
			return children;
		}

		public List<QConjectureMap> BreedConjectures(QConjectureMap first, QConjectureMap second){
			List<QConjectureMap> children = new List<QConjectureMap> (2);
			int bitToFlip = UnityEngine.Random.Range (0, possibleStates.Count);
			//flip a random bit of the lowest scoring conjecture. We mutate it to try and make it more competitive
			if (first.fitness > second.fitness) {

				second.stateBinaryString [bitToFlip / 32] ^= (1 << bitToFlip % 32); //use XOR to flip the bit
			} else {
				first.stateBinaryString [bitToFlip / 32] ^= (1 << bitToFlip % 32); //use XOR to flip the bit
			}

			int [] stateString = new int[(possibleStates.Count / 32) +1];

			for (int i = 0; i<stateString.Length; ++i) {
				stateString[i] = first.stateBinaryString[i] & second.stateBinaryString[i];
			}

			for (int i = 0; i<possibleStates.Count; i++) {
				int firstBit = first.stateBinaryString [i / 32] & (1<<i%32);
				int secondBit = second.stateBinaryString [i / 32] & (1<<i%32);
				if (first!=second){
					if ((firstBit > secondBit && first.fitness>second.fitness) || (firstBit < secondBit && first.fitness<second.fitness)){
						stateString[i/32] = stateString[i/32] | (1<<i%32);
						break;
					} 
				}

			}

			children.Add (new QConjectureMap (stateString,Utils.OR_Integer(first.actionBinaryString,second.actionBinaryString),learningRate,discountFactor,currentGeneration));
			children [0].fitness = (first.fitness > second.fitness ? first.fitness : second.fitness) + (first.fitness + second.fitness) * 0.5f;
			children.Add (new QConjectureMap (stateString,Utils.XOR_Integer(first.actionBinaryString, second.actionBinaryString),learningRate,discountFactor,currentGeneration));
			children [1].fitness = children [0].fitness;
			return children;
		}

		public bool ValidateGeneration(QConjectureMap map){
			return map.generation == 0 || map.generation >= currentGeneration - 1;
		}

		public float TotalFitness {
			get{
				float val = 0;
				int l = conjectures.Count;
				for (int i= 0;i<l; ++i){
					val+=conjectures[i].fitness;
				}
				return val;
			}

		}

		public void ResetTimers(){
			for (int i = 0; i<conjectures.Count; ++i) {
				conjectures[i].timeWhenConjectureWasLastSelected = 0;
			}
		}

		public void ResetFitness(){
			for (int i = 0; i<conjectures.Count; ++i) {
				conjectures[i].fitness = 0;
			}
		}

		public void ToEditorView (){
			scrollPos = EditorGUILayout.BeginScrollView (scrollPos);
			EditorGUILayout.LabelField ("Set whether the QConjectureLearner learns based on state, or change in state.");
			EditorGUILayout.LabelField ("You must regenerate the conjectures from scratch after changing the settings.");
			useDeltaStateLearning = EditorGUILayout.Toggle ("Use Delta State Learning", useDeltaStateLearning);

			numExamplesBeforeEvolving = EditorGUILayout.IntField ("Num examples to run before evolving",numExamplesBeforeEvolving);
			nextState = EditorGUILayout.TextField ("State name", nextState);
			EditorGUILayout.LabelField ("Warning: when you add a state, all conjectures will accept that state as valid.");

			if (GUILayout.Button ("Add State")) {
				possibleStates.Add(nextState);
				bool [,] tempConjectureArr = new bool[possibleStates.Count,possibleStates.Count];
				for (int i = 0; i<possibleStates.Count; ++i){
					for (int j = 0; j<possibleStates.Count; ++j){
						if (i<possibleStates.Count-1 && j<possibleStates.Count-1){
							tempConjectureArr[i,j] = stateCombinationConstraints[i,j];
						} else {
							tempConjectureArr[i,j] = true;
						}
					}
				}
				stateCombinationConstraints = tempConjectureArr;
			}

			nextAction = EditorGUILayout.TextField ("Action name", nextAction);
			EditorGUILayout.LabelField ("Warning: when you add a action, all conjectures will accept that action as valid.");
			if (GUILayout.Button ("Add State")) {
				possibleActions.Add(nextState);
				bool [,] tempConjectureArr = new bool[possibleActions.Count,possibleActions.Count];
				for (int i = 0; i<possibleActions.Count; ++i){
					for (int j = 0; j<possibleActions.Count; ++j){
						if (i<possibleActions.Count-1 && j<possibleActions.Count-1){
							tempConjectureArr[i,j] = actionCombinationConstraints[i,j];
						} else {
							tempConjectureArr[i,j] = true;
						}
					}
				}
				actionCombinationConstraints = tempConjectureArr;
			}

			if (GUILayout.Button ("Generate Most Generic Conjectures")) {
				GenerateBaseConjectures();
			}

			EditorGUILayout.LabelField ("For a given pair of states below, 'true' indicates that the two states can occur at the same time");
			for (int i = 0; i<possibleStates.Count; ++i) {
				EditorGUILayout.LabelField(possibleStates[i]);
				EditorGUI.indentLevel+=3;
				for (int j = i+1; j<possibleStates.Count; ++j){
					stateCombinationConstraints[i,j] = EditorGUILayout.Toggle(possibleStates[j],stateCombinationConstraints[i,j]);
					stateCombinationConstraints[j,i] = stateCombinationConstraints[i,j];
				}
				EditorGUI.indentLevel-=3;
			}



			EditorGUILayout.LabelField ("For a given pair of actions below, 'true' indicates that the two actions can occur at the same time");
			for (int i = 0; i<possibleActions.Count; ++i) {
				EditorGUILayout.LabelField(possibleActions[i]);
				EditorGUI.indentLevel+=3;
				for (int j = i+1; j<possibleActions.Count; ++j){
					actionCombinationConstraints[i,j] = EditorGUILayout.Toggle(possibleActions[j],actionCombinationConstraints[i,j]);
					actionCombinationConstraints[j,i] = actionCombinationConstraints[i,j];
				}
				EditorGUI.indentLevel-=3;
			}

			EditorGUILayout.EndScrollView ();
		}

	}
}

