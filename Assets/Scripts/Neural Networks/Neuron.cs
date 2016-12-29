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
using UnityEditor;
using UnityEngine;
using System.Runtime.Serialization;

namespace AssemblyCSharp
{
	[Serializable]
	public class Neuron : EditorDisplay,IDeserializationCallback
	{


		//public bool allowInputs = false;
		//public bool allowOutputs = false;
		public int ID = 0; //when creating neuron, it must get a unique ID
		/*need ints referencing neurons by their IDs, so that we can serialize network. Referencing neurons directly leads to circular references that break serialization
		* We store the neurons in a list of lists for the entire network, and search for the neurons with the corresponding IDs
		*/
		public List<int> outputsIDs = new List<int> ();
		public List<int> inputsIDs = new List<int> ();
		public List<float> weights = new List<float> ();
		public float bias = 0f;
		public float summedInput;
		public float transformedInput;
		public ActivationFunctionType activationFunctionType = ActivationFunctionType.SIGMOID;
		public string label = "default_label";
		[NonSerialized]public float error = 0;
		[NonSerialized]Vector2 scrollPos = Vector2.zero;
		/*
		 * We must not serialize the actual neurons links! We must relink them whenever we deserialize
		 */
		[NonSerialized]
		public List<Neuron> outputs = new List<Neuron> ();
		[NonSerialized]
		public List<Neuron> inputs = new List<Neuron> ();



		public Neuron (int ID)
		{
			this.ID = ID;
			//this.allowInputs = allowInputs;
			//this.allowOutputs = allowOutputs;
		}

		public Neuron(ref Neuron other, int newID){
			this.ID = newID;
			this.activationFunctionType = other.activationFunctionType;
			this.bias = other.bias;
			this.label = new string (other.label.ToCharArray());
			for (int i = 0; i<other.inputsIDs.Count; ++i) {
				inputsIDs.Add(other.inputsIDs[i]);
			}
			for (int i = 0; i<other.outputsIDs.Count; ++i) {
				outputsIDs.Add(other.outputsIDs[i]);
			}
			for (int i = 0; i<other.weights.Count; ++i) {
				weights.Add(other.weights[i]);
			}
		}

		public void ToEditorView ()
		{
			scrollPos = EditorGUILayout.BeginScrollView (scrollPos, GUILayout.Width(400));
			EditorGUILayout.LabelField ("Neuron ID " + ID);
			bias = EditorGUILayout.FloatField ("Bias", bias);
			label = EditorGUILayout.TextField("Label",label);
			activationFunctionType = (ActivationFunctionType)EditorGUILayout.EnumPopup ("Activation Function Type ", activationFunctionType);
			//if (allowInputs) {
			EditorGUILayout.LabelField ("Input neuron IDs");
			//EditorGUILayout.BeginHorizontal ();
			EditorGUI.indentLevel += 3;
			for (int i = 0; i<inputsIDs.Count; ++i) {
				inputsIDs [i] = EditorGUILayout.IntField ("ID",inputsIDs [i],GUILayout.MinWidth(120));
				weights[i] = EditorGUILayout.FloatField("Weight",weights[i],GUILayout.MinWidth(120));
				if (GUILayout.Button ("Remove input weight", GUILayout.MaxWidth (150))) {
					inputsIDs.RemoveAt (i);
					weights.RemoveAt(i);
					--i;
				}

//				if (i>0 && i%3 == 0){
//					EditorGUILayout.EndHorizontal ();
//					EditorGUILayout.BeginVertical ();
//					EditorGUILayout.EndVertical ();
//					EditorGUILayout.BeginHorizontal ();
//				}
			}

			if (GUILayout.Button ("Add input weight", GUILayout.MaxWidth (150))) {
				inputsIDs.Add (0);
				weights.Add(UnityEngine.Random.value);
			}
			EditorGUI.indentLevel -= 3;
			//EditorGUILayout.EndHorizontal ();
			//EditorGUILayout.BeginVertical ();
			//EditorGUILayout.EndVertical ();
			//}

			//if (allowOutputs) {
			EditorGUILayout.LabelField ("Output neuron IDs");

			//EditorGUILayout.BeginHorizontal ();
			EditorGUI.indentLevel += 3;
			for (int i = 0; i<outputsIDs.Count; ++i) {
				outputsIDs [i] = EditorGUILayout.IntField (outputsIDs [i],GUILayout.MinWidth(120));
				if (GUILayout.Button ("Remove output weight", GUILayout.MaxWidth (150))) {
					outputsIDs.RemoveAt (i);
					--i;
				}

//				if (i>0 && i%3 == 0){
//					EditorGUILayout.EndHorizontal ();
//					EditorGUILayout.BeginVertical ();
//					EditorGUILayout.EndVertical ();
//					EditorGUILayout.BeginHorizontal ();
//				}
			}
				
			if (GUILayout.Button ("Add output weight", GUILayout.MaxWidth (150))) {
				outputsIDs.Add (0);
			}
			EditorGUI.indentLevel -= 3;
			EditorGUILayout.EndScrollView ();
			//EditorGUILayout.EndHorizontal ();
			//EditorGUILayout.BeginVertical ();
			//EditorGUILayout.EndVertical ();
			//}
		}

//		public void OnBeforeSerialize(){
//			
//		}
		
		void IDeserializationCallback.OnDeserialization(System.Object sender){

			scrollPos = Vector2.zero;
			outputs = new List<Neuron> ();
			inputs = new List<Neuron>();

		}
	}
}
