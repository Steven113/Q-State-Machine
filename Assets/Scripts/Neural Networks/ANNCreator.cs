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
using UnityEditor;

//using AssemblyCSharp;

//namespace AssemblyCSharp
//{
using AssemblyCSharp;
using UnityEngine;


public class ANNCreator : EditorWindow
{
		
	int numHiddenLayers = 1;
	int numInputs = 5;
	int numOutputs = 3;

	[MenuItem("Window/Create ANN")]
		
	public static void ShowWindow ()
	{
		EditorWindow.GetWindow (typeof(ANNCreator));
	}
		
	LSEEditor<NeuralNet> netEditor = new LSEEditor<NeuralNet> ();

	public void OnGUI ()
	{
		netEditor.LoadWindow ();
		netEditor.SaveWindow ();

		numInputs = EditorGUILayout.IntField ("Number of Inputs ", numInputs);
		numHiddenLayers = EditorGUILayout.IntField ("Number of Hidden Layers ", numHiddenLayers);
		numOutputs = EditorGUILayout.IntField ("Number of Outputs ", numOutputs);

		if (GUILayout.Button ("Generate")) {
			netEditor.InstanceToEdit = new NeuralNet (numInputs, numHiddenLayers, numOutputs);
		}

		if (GUILayout.Button ("Clear")) {
			netEditor.InstanceToEdit = null;
		}

		netEditor.DisplayObjectForEditing ();
	}
}
//}

