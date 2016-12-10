using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using AssemblyCSharp;
using UnityEditor;

[Serializable]
public class QNode : EditorDisplay
{
	public string stateName = "default_state";
	public float learningRate = 0.1f;
	public float discountFactor = 0.3f;
	public List<int> childrenQStates = new List<int>(); //one for each state
	public float[,] rewardMatrix = new float[0, 0];
	public float[,] QMatrix = new float[0, 0];
	public float[,] TimeMatrix = new float[0, 0];
	public int[,] ExplorationMatrix = new int[0, 0];
	public List<string> stateStringToIDArr = new List<string> ();
	//public bool [,] ShowStateMatrix = new bool[0,0];
	public bool hasChildren = false;
	[NonSerialized]
	public QTree parent;
	public bool canTransition = false;
	bool allNodesConnectedByDefault = false;

	public bool showInEditorWindow = false;
	public bool showRewardMatrix = false;
	public bool showQMatrix = false;
	//public bool showTimeMatrix = false;
	public bool showExplorationMatrix = false;
	public static int currentID = 0;
	public int ID = 0;

	public QNode (string stateName, float learningRate, float discountFactor)
	{ //for a node that will have children
		this.stateName = stateName;
		this.learningRate = learningRate; 
		this.discountFactor = discountFactor;
	}

	public QNode (List<string> states, float learningRate, float discountFactor, bool hasChildren, bool canTransition, bool allNodesConnectedByDefault)
	{ //meant to be used for all nodes
		ID = currentID;
		++currentID;
		stateStringToIDArr.AddRange (states);
		this.hasChildren = hasChildren;
		this.learningRate = learningRate; 
		this.discountFactor = discountFactor;
		this.allNodesConnectedByDefault = allNodesConnectedByDefault;
		rewardMatrix = new float[stateStringToIDArr.Count, stateStringToIDArr.Count];
		QMatrix = new float[stateStringToIDArr.Count, stateStringToIDArr.Count];
		TimeMatrix = new float[stateStringToIDArr.Count, stateStringToIDArr.Count];
		ExplorationMatrix = new int[stateStringToIDArr.Count, stateStringToIDArr.Count];

		this.canTransition = canTransition;

		if (hasChildren) {
			childrenQStates = new List<int>(stateStringToIDArr.Count);
			//for (int i = 0; i<childrenQStates.GetLength(0); ++i) {
				for (int j = 0; j<childrenQStates.Count; ++j) {
					childrenQStates [j] = -1;
				}
			//}
		}

		if (!allNodesConnectedByDefault) {
			for (int i = 0; i<rewardMatrix.GetLength(0); ++i) {
				for (int j = 0; j<rewardMatrix.GetLength(1); ++j) {
					rewardMatrix [i, j] = float.NegativeInfinity;
				}
			}
		}
	}

	public bool VerifyChildrenAssignment(){
		//for (int i = 0; i<childrenQStates.GetLength(0); ++i) {
			
			for (int j = 0; j<childrenQStates.Count; ++j) {
				if (childrenQStates [j] == -1) {
					return false;
				}
			}
		//}
		return true;
	}

	public bool loadChildIndex (int child)
	{
//		bool assigned = false;
//		//for (int i = 0; i<childrenQStates.GetLength(0); ++i) {
//
//			for (int j = 0; j<childrenQStates.Count; ++j) {
//				if (childrenQStates[j]==-1){
//					childrenQStates[j] = child;
//					assigned = true;
//					break;
//				}
//			}
//			///if (assigned){
//			//	break;
//			////}
//		//}
		childrenQStates.Add (child);
//
//		rewardMatrix = new float[childrenQStates.Count, childrenQStates.Count];
//		QMatrix = new float[childrenQStates.Count, childrenQStates.Count];
//		TimeMatrix = new float[childrenQStates.Count, childrenQStates.Count];
//		ExplorationMatrix = new int[childrenQStates.Count, childrenQStates.Count];
//		if (!allNodesConnectedByDefault) {
//			for (int i = 0; i<rewardMatrix.GetLength(0); ++i) {
//				for (int j = 0; j<rewardMatrix.GetLength(1); ++j) {
//					rewardMatrix [i, j] = float.NegativeInfinity;
//				}
//			}
//		}
//
		return (childrenQStates.Count <= stateStringToIDArr.Count);
	}

	public void ToEditorView ()
	{
		float labelWidth = EditorGUIUtility.labelWidth;
		EditorGUIUtility.labelWidth = 400;
		showInEditorWindow = EditorGUILayout.Toggle ("Show Node", showInEditorWindow);
		EditorGUI.indentLevel += 3;
		if (showInEditorWindow) {
			showRewardMatrix = EditorGUILayout.Toggle ("Show Reward Matrix", showRewardMatrix);
			if (showRewardMatrix) {
				EditorGUI.indentLevel += 3;
				for (int i = 0; i<stateStringToIDArr.Count; ++i) {
					EditorGUILayout.LabelField (stateStringToIDArr [i]);
					EditorGUI.indentLevel += 3;
					for (int j = 0; j<stateStringToIDArr.Count; ++j) {
						rewardMatrix [i, j] = EditorGUILayout.FloatField (stateStringToIDArr [j], rewardMatrix [i, j]);
					}
					EditorGUI.indentLevel -= 3;
				}
				EditorGUI.indentLevel -= 3;
			}

			showQMatrix = EditorGUILayout.Toggle ("Show Q-Matrix", showQMatrix);
			if (showQMatrix) {
				EditorGUI.indentLevel += 3;
				for (int i = 0; i<stateStringToIDArr.Count; ++i) {
					EditorGUILayout.LabelField (stateStringToIDArr [i]);
					EditorGUI.indentLevel += 3;
					for (int j = 0; j<stateStringToIDArr.Count; ++j) {
						QMatrix [i, j] = EditorGUILayout.FloatField (stateStringToIDArr [j], QMatrix [i, j]);
					}
					EditorGUI.indentLevel -= 3;
				}
				EditorGUI.indentLevel -= 3;
			}

			showExplorationMatrix = EditorGUILayout.Toggle ("Show Exploration Matrix", showExplorationMatrix);
			if (showExplorationMatrix) {
				EditorGUI.indentLevel += 3;
				for (int i = 0; i<stateStringToIDArr.Count; ++i) {
					EditorGUILayout.LabelField (stateStringToIDArr [i]);
					EditorGUI.indentLevel += 3;
					for (int j = 0; j<stateStringToIDArr.Count; ++j) {
						ExplorationMatrix [i, j] = EditorGUILayout.IntField (stateStringToIDArr [j], ExplorationMatrix [i, j]);
					}
					EditorGUI.indentLevel -= 3;
				}
				EditorGUI.indentLevel -= 3;
			}
			EditorGUIUtility.labelWidth = labelWidth;
			EditorGUILayout.LabelField ("Children");
			EditorGUI.indentLevel += 3;
			//for (int j = 0; j<childrenQStates.GetLength(0); ++j){
				for (int k = 0; k<childrenQStates.Count; ++k){
					EditorGUILayout.LabelField(stateStringToIDArr[k]);
					parent.treeNodes[childrenQStates[k]].ToEditorView();
				}
			//}
			EditorGUI.indentLevel -= 3;

		}
		EditorGUI.indentLevel -= 3;
		EditorGUIUtility.labelWidth = labelWidth;
	}
}
