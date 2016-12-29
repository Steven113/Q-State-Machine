using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AssemblyCSharp;
using System.Collections.ObjectModel;
using System.IO;
using System;

public class AStarTester : MonoBehaviour
{

	public List<Transform> pathTestingPoints = new List<Transform> ();
	public float[][][] results = new float[3][][]; //rows: starttime, endtime, dist between points, actual path distance
	public string fileToWriteTo;
	public List<Color> pathCol;
	public int numTests = 1;

	// Use this for initialization
	IEnumerator RunTest ()
	{

		string data = "";
		for (int m = 0; m<numTests; ++m) {
			results = new float[(int)((((pathTestingPoints.Count + 1) * pathTestingPoints.Count) / 2))][][];

			int resultL = results.Length;

			for (int i = 0; i<resultL; ++i) {
				results [i] = new float[5][];
				for (int j = 0; j<5; ++j) {
					results [i] [j] = new float[4];
				}
			}

			int testNum = 0;
			Collection<Vector3> path = new Collection<Vector3> ();
			for (int i = 0; i<pathTestingPoints.Count; ++i) {
				for (int j = i+1; j<pathTestingPoints.Count; ++j) {

					bool pathFound = false;
					float p_c = 0;
//				results[testNum][0][0] = System.DateTime.Now.Ticks;
//				pathFound = BenchmarkAIGrid.findPath_Sorted_Closed_List(pathTestingPoints[i].position,pathTestingPoints[j].position,out path);
//				results[testNum][0][1] = System.DateTime.Now.Ticks;
//				Debug.Assert(pathFound);
//				results[testNum][0][2] = AIGrid.GetManhattanDistance(pathTestingPoints[i].position,pathTestingPoints[j].position);
//				results[testNum][0][3] = CalcPathLength(path);
//
//				p_c = path.Count;
//				for (int n = 1; n<p_c; ++n){
//					Debug.DrawRay(path[n-1], path[n]-path[n-1],pathCol[0],300f);
//				}

//				results[testNum][1][0] = System.DateTime.Now.Ticks;
//				pathFound = BenchmarkAIGrid.findPath_Closed_List_Unsorted(pathTestingPoints[i].position,pathTestingPoints[j].position,out path);
//				results[testNum][1][1] = System.DateTime.Now.Ticks;
//				Debug.Assert(pathFound);
//				results[testNum][1][2] = AIGrid.GetManhattanDistance(pathTestingPoints[i].position,pathTestingPoints[j].position);
//				results[testNum][1][3] = CalcPathLength(path);
//
//				p_c = path.Count;
//				for (int n = 1; n<p_c; ++n){
//					Debug.DrawRay(path[n-1], path[n]-path[n-1],pathCol[0],300f);
//				}
					var watch = System.Diagnostics.Stopwatch.StartNew ();

					watch.Start ();
					//results[testNum][2][0] = System.DateTime.Now.Ticks;
					pathFound = BenchmarkAIGrid.findPath_Bit_Mask_Keep_List (pathTestingPoints [i].position, pathTestingPoints [j].position, out path);
					watch.Stop ();
					results [testNum] [0] [0] = watch.ElapsedTicks;
					results [testNum] [0] [1] = System.Diagnostics.Stopwatch.Frequency;
					watch.Reset ();
					Debug.Assert (pathFound);
					results [testNum] [0] [2] = AIGrid.GetManhattanDistance (pathTestingPoints [i].position, pathTestingPoints [j].position);
					results [testNum] [0] [3] = CalcPathLength (path);

					watch.Start ();
					//results[testNum][2][0] = System.DateTime.Now.Ticks;
					pathFound = BenchmarkAIGrid.findPath_Bool_Per_Cell_Keep_List (pathTestingPoints [i].position, pathTestingPoints [j].position, out path);
					watch.Stop ();
					results [testNum] [1] [0] = watch.ElapsedTicks;
					results [testNum] [1] [1] = System.Diagnostics.Stopwatch.Frequency;
					watch.Reset ();
					Debug.Assert (pathFound);
					results [testNum] [1] [2] = AIGrid.GetManhattanDistance (pathTestingPoints [i].position, pathTestingPoints [j].position);
					results [testNum] [1] [3] = CalcPathLength (path);

					watch.Start ();
					//results[testNum][2][0] = System.DateTime.Now.Ticks;
					pathFound = BenchmarkAIGrid.findPath_Bit_Mask (pathTestingPoints [i].position, pathTestingPoints [j].position, out path);
					watch.Stop ();
					results [testNum] [2] [0] = watch.ElapsedTicks;
					results [testNum] [2] [1] = System.Diagnostics.Stopwatch.Frequency;
					watch.Reset ();
					Debug.Assert (pathFound);
					results [testNum] [2] [2] = AIGrid.GetManhattanDistance (pathTestingPoints [i].position, pathTestingPoints [j].position);
					results [testNum] [2] [3] = CalcPathLength (path);

//					p_c = path.Count;
//					for (int n = 1; n<p_c; ++n) {
//						Debug.DrawRay (path [n - 1], path [n] - path [n - 1], pathCol [2], 1f);
//					}

					//yield return new WaitForSeconds (0.001f);

					//results[testNum][3][0] = System.DateTime.Now.Ticks;
					watch.Start ();
					pathFound = BenchmarkAIGrid.findPath_Bool_Per_Cell (pathTestingPoints [i].position, pathTestingPoints [j].position, out path);
					watch.Stop ();
					results [testNum] [3] [0] = watch.ElapsedTicks;
					results [testNum] [3] [1] = System.Diagnostics.Stopwatch.Frequency;
					watch.Reset ();
					Debug.Assert (pathFound);
					results [testNum] [3] [2] = AIGrid.GetManhattanDistance (pathTestingPoints [i].position, pathTestingPoints [j].position);
					results [testNum] [3] [3] = CalcPathLength (path);

//					p_c = path.Count;
//					for (int n = 1; n<p_c; ++n) {
//						Debug.DrawRay (path [n - 1], path [n] - path [n - 1], pathCol [3], 1f);
//					}

					//yield return new WaitForSeconds (0.001f);

					//results[testNum][4][0] = System.DateTime.Now.Ticks;
					watch.Start ();
					pathFound = BenchmarkAIGrid.findPath_Num_Uses (pathTestingPoints [i].position, pathTestingPoints [j].position, out path);
					watch.Stop ();
					results [testNum] [4] [0] = watch.ElapsedTicks;
					results [testNum] [4] [1] = System.Diagnostics.Stopwatch.Frequency;
					watch.Reset ();
					Debug.Assert (pathFound);
					results [testNum] [4] [2] = AIGrid.GetManhattanDistance (pathTestingPoints [i].position, pathTestingPoints [j].position);
					results [testNum] [4] [3] = CalcPathLength (path);

//					p_c = path.Count;
//					for (int n = 1; n<p_c; ++n) {
//						Debug.DrawRay (path [n - 1], path [n] - path [n - 1], pathCol [4], 1f);
//					}

					++testNum;

					//yield return new WaitForSeconds (0.001f);
				}

			
			}

		

			for (int i = 0; i<testNum; ++i) {
				for (int j = 0; j<5; ++j) {
					for (int k = 0; k<4; ++k) {
						data = data + results [i] [j] [k] + ",";
					}
				}
				data += Environment.NewLine;
			}


			Debug.Log("Completed Test: "+m);
		}

		data = data + System.Diagnostics.Stopwatch.IsHighResolution + ",";

		File.WriteAllText (fileToWriteTo+ ".csv", data);

		yield return new WaitForSeconds (0.1f);
		
		Debug.Log ("Test Done!");
	}
	
	// Update is called once per frame
	void Start ()
	{
		StartCoroutine ("RunTest");
	}

	public float CalcPathLength (Collection<Vector3> path)
	{
		float result = 0;
		float pathL = path.Count;
		for (int k = 1; k<pathL; ++k) {
			result += Vector3.Distance (path [k - 1], path [k]);
		}

		return result;
	}
}
