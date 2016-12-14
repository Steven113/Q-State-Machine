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
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


namespace AssemblyCSharp
{
	public class QHeirarchyGenerator : EditorWindow
	{
		List<List<string>> heirarchy = new List<List<string>>();
		int heirarchyDepth = 1;
		List<int> nestedHeirarchyDepths = new List<int>();
		List<QLink> links = new List<QLink>();
		List<bool> autoLinksForThisHeirarchy = new List<bool>();
		bool preserveHeirarchyData = true;
		bool preserveLinkData = false;
		QTree qTree = null;
		float learningRate;
		float discountFactor;
		Vector2 stateScrollViewPosition = Vector2.zero;
		Vector2 treeScrollViewPosition = Vector2.zero;

		string fileToLoadTreeFrom = "default_q_File";
		string fileToSaveTreeTo = "default_q_File_sv";

		[MenuItem("Window/QHeirarchyGenerator")]
		
		public static void ShowWindow()
		{
			EditorWindow.GetWindow(typeof(QHeirarchyGenerator));
		}

		public void OnGUI(){

			fileToSaveTreeTo = EditorGUILayout.TextField ("Save: Q Tree File Location", fileToSaveTreeTo);
			
			if (GUILayout.Button("Save QTree")){
				Utils.SerializeFile(fileToSaveTreeTo,ref qTree);
			}
			
			fileToLoadTreeFrom = EditorGUILayout.TextField ("Load: Q Tree File Location", fileToLoadTreeFrom);
			
			if (GUILayout.Button("Load QTree")){
				QTree tempQ = null;
				if (Utils.DeserializeFile<QTree>(fileToLoadTreeFrom,ref tempQ) && tempQ != null){
					qTree = tempQ;
				} else {
					Debug.Log("File could not load!");
				}
			}

			learningRate = EditorGUILayout.FloatField ("Learning Rate", learningRate);
			EditorGUILayout.LabelField ("The learning rate determines how much weight a new reward has over the previous reward");
			discountFactor = EditorGUILayout.FloatField ("Discount Factor", discountFactor);
			EditorGUILayout.LabelField ("The discount factor determines how much we weight the utility of the successor state s' when calculating the utility of state s");

			EditorGUILayout.LabelField ("Number of Layers In Heirarchy");
			heirarchyDepth = EditorGUILayout.IntField ("",heirarchyDepth);
			EditorGUILayout.LabelField ("Preserve data in heirarchy when you change it's size?");
			preserveHeirarchyData = EditorGUILayout.Toggle (preserveHeirarchyData);
			if (heirarchyDepth < heirarchy.Count && !preserveHeirarchyData) {
				while (heirarchy.Count>(heirarchyDepth)) {
					heirarchy.RemoveAt (heirarchy.Count - 1);
					nestedHeirarchyDepths.RemoveAt (heirarchy.Count - 1);
					autoLinksForThisHeirarchy.RemoveAt (heirarchy.Count - 1);
				}
			} else if (heirarchyDepth > heirarchy.Count) {
				while (heirarchyDepth > heirarchy.Count){
					heirarchy.Add(new List<string>());
					nestedHeirarchyDepths.Add(0);
					autoLinksForThisHeirarchy.Add(false);
				}
			}
			stateScrollViewPosition = EditorGUILayout.BeginScrollView (stateScrollViewPosition, true, true);
			if (heirarchyDepth > 0) {
				EditorGUILayout.LabelField ("State Heirarchy");
				//if (GUILayout.Button("Insert")){
				//	++heirarchyDepth;
				//	heirarchy.Insert(0, new List<string>());
			//	}

			}
			EditorGUI.indentLevel += 2;
			for (int i = 0; i<heirarchyDepth; ++i) {
				EditorGUILayout.LabelField ("Level "+(i+1));
				//EditorGUILayout.LabelField ("Preserve data in heirarchy when you change it's size?");
				nestedHeirarchyDepths[i] = EditorGUILayout.IntField(nestedHeirarchyDepths[i]);
				if (nestedHeirarchyDepths[i] < heirarchy[i].Count && !preserveHeirarchyData) {
					while (heirarchy[i].Count>(nestedHeirarchyDepths[i])) {
						heirarchy[i].RemoveAt (heirarchy[i].Count - 1);
						//nestedHeirarchyDepths.RemoveAt (heirarchy.Count - 1);
					}
				} else if (nestedHeirarchyDepths[i] > heirarchy[i].Count) {
					while (nestedHeirarchyDepths[i] > heirarchy[i].Count){
						heirarchy[i].Add("default");
						//nestedHeirarchyDepths.Add(0);
					}
				}
				EditorGUI.indentLevel+=5;
				for (int j = 0; j<heirarchy[i].Count; ++j){
					heirarchy[i][j] = EditorGUILayout.TextField(heirarchy[i][j]);
				}
				EditorGUI.indentLevel-=5;
				autoLinksForThisHeirarchy[i] = EditorGUILayout.Toggle("Auto link all states",autoLinksForThisHeirarchy[i]);
				//if (GUILayout.Button("Insert")){
				//	++heirarchyDepth;
				//	heirarchy.Insert(i, new List<string>());
				//}
			}
			EditorGUI.indentLevel -= 2;

			EditorGUILayout.LabelField ("Preserve link data if you reduce the number of links?");
			preserveLinkData = EditorGUILayout.Toggle (preserveLinkData);
			int numLinks = EditorGUILayout.IntField ("", links.Count);

			if (numLinks < links.Count && !preserveLinkData) {
				while (links.Count>(numLinks)) {
					links.RemoveAt (links.Count - 1);
					//nestedHeirarchyDepths.RemoveAt (heirarchy.Count - 1);
				}
			} else if (numLinks > links.Count) {
				while (numLinks > links.Count){
					links.Add(new QLink());
					//nestedHeirarchyDepths.Add(0);
				}
			}

			if (numLinks > 0) {
				EditorGUILayout.LabelField ("Preset Links");
			}
			EditorGUI.indentLevel += 2;
			for (int i = 0; i<links.Count; ++i) {
				links[i].ToEditorView();
			}
			EditorGUI.indentLevel -= 2;

			EditorGUILayout.EndScrollView ();

			if (qTree != null) {
				EditorGUILayout.LabelField ("Warning: Generating the Q-Tree will destroy the previously generated tree!");
			}

			if (GUILayout.Button("Generate QTree")){
				qTree = new QTree(heirarchy,links,autoLinksForThisHeirarchy,learningRate,discountFactor);
				qTree.SetParents();
				//++heirarchyDepth;
				//heirarchy.Insert(0, new List<string>());
			}

			if (qTree != null) {
				treeScrollViewPosition = EditorGUILayout.BeginScrollView (treeScrollViewPosition, true, true);
				qTree.ToEditorView();
				EditorGUILayout.EndScrollView();
			}



		}
	}
}

