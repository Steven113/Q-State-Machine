using UnityEngine;
using System.Collections;
using AssemblyCSharp;
using System.Collections.ObjectModel;

public class AIGrid : MonoBehaviour
{

	public int worldWidth = 100;
	public int worldHeight = 100;
	public static Vector3[] LOSDirections = null;
	public static Vector3[] SearchDirectionOffsets = null; //to make the code more reusable, when expanding in the 8 cardinal directions we just add these values to the current node position
	public static float[] SearchDirectionDistances = null;
	public static Vector3 worldCentre = Vector3.zero;
	public static bool[,] cellCanBeMovedThrough = new bool[0, 0];
	public static float[,,] visibilityDistances = new float[0, 0, 0];
	public static float[,,] fValues = new float[0, 0, 0];
	public static float[,,] gValues = new float[0, 0, 0];
	public static float[,,] rhsValues = new float[0, 0, 0];
	public static double[,] numUses = new double[0, 0];
	public static int s_cellWidth = 0;
	public int cellWidth = 1;
	public static double numPathFindingSearches = 0;
	public static bool debugSearchActive = false;

	// Use this for initialization
	void Awake ()
	{
		//useful to keep for testing equations
//		for (int i = 0; i<20; ++i) {
//			Vector3 vect1 = UnityEngine.Random.insideUnitSphere*(i+1);
//			Vector3 vect2 = UnityEngine.Random.insideUnitSphere*(i+1);
//			Debug.Log("Magnitude from vector3" + (vect1-vect2).magnitude);
//			Debug.Log("Magnitude from appoximation" + GetSquareDistanceEstimate(vect1,vect2));
//		}

		s_cellWidth = cellWidth;
		Debug.Assert (s_cellWidth > 0);
		LOSDirections = new Vector3[] {
			Vector3.left,
			(Vector3.left + Vector3.forward),
			Vector3.forward,
			(Vector3.right + Vector3.forward),
			Vector3.right,
			(Vector3.right + Vector3.back),
			Vector3.back,
			(Vector3.left + Vector3.back)
		}; //cardinalDirections
		SearchDirectionOffsets = new Vector3[]{
			Vector3.left,
			Vector3.left + Vector3.forward,
			Vector3.forward,
			Vector3.right + Vector3.forward, 
			Vector3.right,
			Vector3.right + Vector3.back,
			Vector3.back,
			Vector3.back + Vector3.left
		};

		SearchDirectionDistances = new float[8];

		for (int i = 0; i<SearchDirectionDistances.Length; ++i) {
			SearchDirectionDistances [i] = Vector3.Magnitude (SearchDirectionOffsets [i]);
		}

		cellCanBeMovedThrough = new bool[worldWidth, worldHeight];

		for (int i = 0; i<worldWidth; ++i) {
			for (int j = 0; j<worldHeight; ++j) {
				cellCanBeMovedThrough [i, j] = true;
			}
		}
		visibilityDistances = new float[worldWidth, worldHeight, 8];
		fValues = new float[worldWidth, worldHeight, 2];
		gValues = new float[worldWidth, worldHeight, 2];
		rhsValues = new float[worldWidth, worldHeight, 2];
		for (int i = 0; i<worldWidth; ++i) {
			for (int j = 0; j<worldHeight; ++j) {
				fValues [i, j, 0] = float.PositiveInfinity/100;
				fValues [i, j, 1] = float.PositiveInfinity/100;

				gValues [i, j, 0] = float.PositiveInfinity/100;
				gValues [i, j, 1] = float.PositiveInfinity/100;

				rhsValues [i, j, 0] = float.PositiveInfinity/100;
				rhsValues [i, j, 1] = float.PositiveInfinity/100;

				cellCanBeMovedThrough [i, j] = true;
			}
		}
		numUses = new double[worldWidth, worldHeight];
	}

	void Start ()
	{
		for (int i = 0; i<cellCanBeMovedThrough.GetLength(0); ++i) {
			for (int j = 0; j<cellCanBeMovedThrough.GetLength(1); ++j) {

				for (int k = 0; k<LOSDirections.Length; ++k) {
						
					if (cellCanBeMovedThrough [i, j]) {
						//Debug.DrawRay(new Vector3(i*cellWidth,0,j*cellWidth),Vector3.forward*cellWidth,Color.red,3f);
						//Debug.DrawRay(new Vector3(i*cellWidth,0,j*cellWidth),Vector3.right*cellWidth,Color.red,3f);
						Vector3 losPos = new Vector3 (i, 0, j);
						losPos += LOSDirections [k];
						while (((int)losPos.x>=0 && (int)losPos.x<AIGrid.cellCanBeMovedThrough.GetLength(0) && (int)losPos.z>=0 && (int)losPos.z<AIGrid.cellCanBeMovedThrough.GetLength(1)) && AIGrid.cellCanBeMovedThrough[(int)losPos.x,(int)losPos.z]) {
							visibilityDistances [i, j, k] += SearchDirectionDistances[k];
							losPos += LOSDirections [k];
						}
						//visibilityDistances [i, j, k] = visibilityDistances [i, j, k] > 0 ? visibilityDistances [i, j, k] : cellWidth;
					} else {
						visibilityDistances [i, j, k] = 0;
					}
				}
				//}
			}
		}
	}
	// Update is called once per frame
	//void Update () {
	//
	//}

	public static bool findPath (Vector3 start, Vector3 end, out Collection<Vector3> path, bool useStandardAStar = false)
	{
		++AIGrid.numPathFindingSearches;
		path = new Collection<Vector3> ();
		//first check that start and end are within bounds
		if (start.x >= 0 && start.x < AIGrid.cellCanBeMovedThrough.GetLength (0) && start.z >= 0 && start.z < AIGrid.cellCanBeMovedThrough.GetLength (1)) {
			if (end.x >= 0 && end.x < AIGrid.cellCanBeMovedThrough.GetLength (0) && end.z >= 0 && end.z < AIGrid.cellCanBeMovedThrough.GetLength (1)) {
				if (!AIGrid.cellCanBeMovedThrough [(int)end.x, (int)end.z]) {
					Debug.Log ("Cannot move to destination cell");
					return false;
				}
				for (int i = 0; i<cellCanBeMovedThrough.GetLength(0); ++i) {
					for (int j = 0; j<cellCanBeMovedThrough.GetLength(1); ++j) {
							
						if (cellCanBeMovedThrough [i, j]) {
							Debug.DrawRay (new Vector3 (i * s_cellWidth, 0, j * s_cellWidth), Vector3.forward * s_cellWidth, Color.green);
							Debug.DrawRay (new Vector3 (i * s_cellWidth, 0, j * s_cellWidth), Vector3.right * s_cellWidth, Color.green);
						}

					}
				}


				NodeList<PathFindingNode> openList = new NodeList<PathFindingNode> ();
				bool reachedEnd = false;
				openList.Add (new PathFindingNode (null, start, start, end, ref reachedEnd, useStandardAStar,false));

				int numIter = 0;
				int maxNumIter = cellCanBeMovedThrough.GetLength (0) * cellCanBeMovedThrough.GetLength (1);
				while (openList.Count>0 && !reachedEnd) {
					++numIter;
					if (numIter > maxNumIter) {
						Debug.Log ("Num iterations max exceeded");
						break;
					}
					PathFindingNode temp = openList [0];
					for (int i = 0; i< SearchDirectionOffsets.Length; ++i) {
						Vector3 newPos = temp.pos + SearchDirectionOffsets [i];
						if ((int)newPos.x == (int)end.x && (int)newPos.z == (int)end.z)
							return true;
						if ((int)newPos.x >= 0 && (int)newPos.x < AIGrid.cellCanBeMovedThrough.GetLength (0) && (int)newPos.z >= 0 && (int)newPos.z < AIGrid.cellCanBeMovedThrough.GetLength (1) && AIGrid.cellCanBeMovedThrough [(int)newPos.x, (int)newPos.z]) {
							PathFindingNode node = new PathFindingNode (temp, newPos, start, end, ref reachedEnd, useStandardAStar,false);
							if (numUses [(int)newPos.x, (int)newPos.z] < numPathFindingSearches || fValues [(int)newPos.x, (int)newPos.z, (useStandardAStar ? 0 : 1)] > node.f) {
								numUses [(int)newPos.x, (int)newPos.z] = numPathFindingSearches;
								fValues [(int)newPos.x, (int)newPos.z, useStandardAStar ? 0 : 1] = node.f;
								openList.Add (node);
							}
						}
					}
					openList.Remove (temp);
				}
				Debug.Log ("OpenList empty");
				return false;
			} else {
				Debug.Log ("End out of bounds");
				return false;
			}
		} else {
			Debug.Log ("Start out of bounds");
			return false;
		}
	}

	public static IEnumerator findPathCoroutine (Vector3 start, Vector3 end, bool useStandardAStar = false)
	{
		debugSearchActive = true;
		++AIGrid.numPathFindingSearches;
		//path = new Collection<Vector3> ();
		//first check that start and end are within bounds
		bool keepSearching = true;
		if (start.x >= 0 && start.x < AIGrid.cellCanBeMovedThrough.GetLength (0) && start.z >= 0 && start.z < AIGrid.cellCanBeMovedThrough.GetLength (1)) {
			if (end.x >= 0 && end.x < AIGrid.cellCanBeMovedThrough.GetLength (0) && end.z >= 0 && end.z < AIGrid.cellCanBeMovedThrough.GetLength (1)) {
				if (!AIGrid.cellCanBeMovedThrough [(int)end.x, (int)end.z]) {
					Debug.Log ("Cannot move to destination cell");
					//return;
				}
				for (int i = 0; i<cellCanBeMovedThrough.GetLength(0); ++i) {
					for (int j = 0; j<cellCanBeMovedThrough.GetLength(1); ++j) {
						
						if (cellCanBeMovedThrough [i, j]) {
							Debug.DrawRay (new Vector3 (i * s_cellWidth, 0, j * s_cellWidth), Vector3.forward * s_cellWidth, Color.green, 100f);
							Debug.DrawRay (new Vector3 (i * s_cellWidth, 0, j * s_cellWidth), Vector3.right * s_cellWidth, Color.green, 100f);
						}
						
					}
				}
				
				
				NodeList<PathFindingNode> openList = new NodeList<PathFindingNode> ();
				bool reachedEnd = false;
				openList.Add (new PathFindingNode (null, start, start, end, ref reachedEnd, useStandardAStar,false));
				
				int numIter = 0;
				int maxNumIter = cellCanBeMovedThrough.GetLength (0) * cellCanBeMovedThrough.GetLength (1);
				int numExpansions = 0;
				PathFindingNode temp = openList [0];
				gValues[(int)(temp.pos.x), (int)(temp.pos.z),useStandardAStar ? 0 : 1] = 0;
				while (openList.Count>0 && !reachedEnd) {
					++numIter;
					if (numIter > maxNumIter) {
						Debug.Log ("Num iterations max exceeded");
						break;
					}
					temp = openList [0];

					//float dot = -1;
					//int index = -1;

					bool consistent = false;


					Vector3 newPos = Vector3.zero;

//					for (int i = 0; i< SearchDirectionOffsets.Length; ++i) {
//						float t_d = Vector3.Dot (SearchDirectionOffsets [i], end - temp.pos);
//						if (t_d > dot) {
//							dot = t_d;
//							index = i;
//						}
//					}
//
//
//
//					newPos = temp.pos + SearchDirectionOffsets [index];
//					if ((int)newPos.x == (int)end.x && (int)newPos.z == (int)end.z) {
//						reachedEnd = true;
//						break;
//					}
//					if ((int)newPos.x >= 0 && (int)newPos.x < AIGrid.cellCanBeMovedThrough.GetLength (0) && (int)newPos.z >= 0 && (int)newPos.z < AIGrid.cellCanBeMovedThrough.GetLength (1) && AIGrid.cellCanBeMovedThrough [(int)newPos.x, (int)newPos.z]) {
//						PathFindingNode node = null;
//						if (!useStandardAStar && Vector3.Dot (newPos - temp.pos, end - temp.pos) < 0  && temp.deltaLosDistance > 0 && visibilityDistances [(int)temp.pos.x, (int)temp.pos.z, index] > (temp.deltaLosDistance * temp.deltaLosDistance)) {
//							float dist = (temp.deltaLosDistance * temp.deltaLosDistance);
//							node = new PathFindingNode (temp, newPos, start, end, ref reachedEnd, useStandardAStar);
//							for (float k = 1; k<dist-1; ++k) {
//								fValues [(int)(temp.pos.x + k * SearchDirectionOffsets [index].x), (int)(temp.pos.z + k * SearchDirectionOffsets [index].z),useStandardAStar?0:1] = ((k / dist) * node.f) < fValues [(int)(temp.pos.x + k * SearchDirectionOffsets [index].x), (int)(temp.pos.z + k * SearchDirectionOffsets [index].z),useStandardAStar?0:1] ? ((k / dist) * node.f) : fValues [(int)(temp.pos.x + k * SearchDirectionOffsets [index].x), (int)(temp.pos.z + k * SearchDirectionOffsets [index].z),useStandardAStar?0:1];
//								//numUses [(int)(temp.pos.x + k * SearchDirectionOffsets [index].x), (int)(temp.pos.z + k * SearchDirectionOffsets [index].z)] = numPathFindingSearches;
//							}
//						} else {
//							node = new PathFindingNode (temp, newPos, start, end, ref reachedEnd, useStandardAStar);
//						}
//						if (reachedEnd) { //adding to the openlist is expensive, and we've found the end, so we break now
//							break;
//						}
//
//						if (Mathf.Approximately (node.f, fValues [(int)newPos.x, (int)newPos.z,useStandardAStar?0:1])) {
//							consistent = true;
//						}
//
//						if (numUses [(int)newPos.x, (int)newPos.z] < numPathFindingSearches || fValues [(int)newPos.x, (int)newPos.z,useStandardAStar?0:1] > node.f) {
//							numUses [(int)newPos.x, (int)newPos.z] = numPathFindingSearches;
//							fValues [(int)newPos.x, (int)newPos.z,useStandardAStar?0:1] = node.f;
//							openList.Add (node);
//							++numExpansions;
//						}
//
//					}
					//bool consistent = false;

//					float minG = float.PositiveInfinity;
//					for (int i = 0; i< SearchDirectionOffsets.Length; ++i) {
//						if (visibilityDistances [(int)temp.pos.x, (int)temp.pos.z, i]>0){
//							float tempG = gValues[(int)(temp.pos.x+SearchDirectionOffsets[i].x), (int)(temp.pos.z+SearchDirectionOffsets[i].z),useStandardAStar ? 0 : 1] + SearchDirectionDistances[i];
//							minG = minG>tempG?tempG:minG;
//						}
//					}
//
////					if (minG==gValues[(int)temp.pos.x, (int)temp.pos.z,useStandardAStar ? 0 : 1]){
////						//gValues[(int)temp.pos.x, (int)temp.pos.z,useStandardAStar ? 0 : 1]= minG;
////						consistent= true;
////					} else
//					if (numIter>2){
//					if (minG>gValues[(int)temp.pos.x, (int)temp.pos.z,useStandardAStar ? 0 : 1]){
//						gValues[(int)temp.pos.x, (int)temp.pos.z,useStandardAStar ? 0 : 1]= minG;
//						consistent= true;
//					} else {
//						gValues[(int)temp.pos.x, (int)temp.pos.z,useStandardAStar ? 0 : 1]= float.PositiveInfinity/100;
//					}
//					}

					if (!consistent) {
					for (int i = 0; i< SearchDirectionOffsets.Length; ++i) {
						//if (i == index)
						//	continue;
						//newPos = temp.pos + SearchDirectionOffsets [i];
						//Debug.Log("Exploring "+newPos + (useStandardAStar?"AStar":"LOS*"));
						if ((int)newPos.x == (int)end.x && (int)newPos.z == (int)end.z) {
							reachedEnd = true;
							break;
						}
						bool validPos = false;
						//if ((int)newPos.x >= 0 && (int)newPos.x < AIGrid.cellCanBeMovedThrough.GetLength (0) && (int)newPos.z >= 0 && (int)newPos.z < AIGrid.cellCanBeMovedThrough.GetLength (1) && AIGrid.cellCanBeMovedThrough [(int)newPos.x, (int)newPos.z]) {
						PathFindingNode node = null;
						bool foundShortCutExpansion = false;
						//jump search when using LOS*. We attempt to exponentially increase the jump distance, to reduce the expansion amount


							float dotTemp = Vector3.Dot (newPos - temp.pos, end - temp.pos);
							float dotTemp2 = Vector3.Dot (newPos - temp.pos, end - (temp.previous!=null?temp.previous.pos:start));
							if (openList.Count > 1 && !useStandardAStar && dotTemp>-0.5f && dotTemp2>=-0.5f &&  temp.deltaLosDistance > 1 && visibilityDistances [(int)temp.pos.x, (int)temp.pos.z, i] > (temp.deltaLosDistance * temp.deltaLosDistance)) {
							float dist = (temp.deltaLosDistance * temp.deltaLosDistance);
							newPos = temp.pos + SearchDirectionOffsets [i] * dist;
							if ((int)newPos.x >= 0 && (int)newPos.x < AIGrid.cellCanBeMovedThrough.GetLength (0) && (int)newPos.z >= 0 && (int)newPos.z < AIGrid.cellCanBeMovedThrough.GetLength (1) && AIGrid.cellCanBeMovedThrough [(int)newPos.x, (int)newPos.z]) {
								foundShortCutExpansion = true;	
								validPos = true;
								node = new PathFindingNode (temp, newPos, start, end, ref reachedEnd, useStandardAStar,true);
								for (float k = 1; k<dist-1; ++k) {
									fValues [(int)(temp.pos.x + k * SearchDirectionOffsets [i].x), (int)(temp.pos.z + k * SearchDirectionOffsets [i].z), useStandardAStar ? 0 : 1] = (((k - 1) / dist) * node.f) < fValues [(int)(temp.pos.x + k * SearchDirectionOffsets [i].x), (int)(temp.pos.z + k * SearchDirectionOffsets [i].z), useStandardAStar ? 0 : 1] ? (((k - 1) / dist) * node.f) : fValues [(int)(temp.pos.x + k * SearchDirectionOffsets [i].x), (int)(temp.pos.z + k * SearchDirectionOffsets [i].z), useStandardAStar ? 0 : 1];
									numUses[(int)(temp.pos.x+k*SearchDirectionOffsets[i].x),(int)(temp.pos.z+k*SearchDirectionOffsets[i].z)] = numPathFindingSearches;
								}

										if (numUses [(int)newPos.x, (int)newPos.z] < numPathFindingSearches || fValues [(int)newPos.x, (int)newPos.z, useStandardAStar ? 0 : 1] > node.f) {
											numUses [(int)newPos.x, (int)newPos.z] = numPathFindingSearches;
											fValues [(int)newPos.x, (int)newPos.z, useStandardAStar ? 0 : 1] = node.f;
											gValues [(int)newPos.x, (int)newPos.z, useStandardAStar ? 0 : 1] = node.g;
											openList.Add (node);
											++numExpansions;
										}

							}
						} 

						if (!foundShortCutExpansion) {
							newPos = temp.pos + SearchDirectionOffsets [i];
							if ((int)newPos.x >= 0 && (int)newPos.x < AIGrid.cellCanBeMovedThrough.GetLength (0) && (int)newPos.z >= 0 && (int)newPos.z < AIGrid.cellCanBeMovedThrough.GetLength (1) && AIGrid.cellCanBeMovedThrough [(int)newPos.x, (int)newPos.z]) {
								validPos = true;
									node = new PathFindingNode (temp, newPos, start, end, ref reachedEnd, useStandardAStar,false);
							}

						}
						if (reachedEnd) { //adding to the openlist is expensive, and we've found the end, so we break now
							break;
						}

							consistent= false;
							if (true){
							float minG = float.PositiveInfinity;
							for (int j = 0; j< SearchDirectionOffsets.Length; ++j) {
									if ((int)(newPos.x+SearchDirectionOffsets[i].x) >= 0 && (int)(newPos.x+SearchDirectionOffsets[i].x) < AIGrid.cellCanBeMovedThrough.GetLength (0) && (int)(newPos.z+SearchDirectionOffsets[i].z) >= 0 && (int)(newPos.z+SearchDirectionOffsets[i].z) < AIGrid.cellCanBeMovedThrough.GetLength (1) && AIGrid.cellCanBeMovedThrough [(int)(newPos.x+SearchDirectionOffsets[i].x), (int)(newPos.z+SearchDirectionOffsets[i].z)]) {
									float tempG = gValues[(int)(temp.pos.x+SearchDirectionOffsets[i].x), (int)(temp.pos.z+SearchDirectionOffsets[i].z),useStandardAStar ? 0 : 1] + SearchDirectionDistances[j];
									minG = minG>tempG?tempG:minG;
								}
							}
							
							//					if (minG==gValues[(int)temp.pos.x, (int)temp.pos.z,useStandardAStar ? 0 : 1]){
							//						//gValues[(int)temp.pos.x, (int)temp.pos.z,useStandardAStar ? 0 : 1]= minG;
							//						consistent= true;
							//					} else
							if (numIter>2){
								if (minG>gValues[(int)temp.pos.x, (int)temp.pos.z,useStandardAStar ? 0 : 1]){
									gValues[(int)temp.pos.x, (int)temp.pos.z,useStandardAStar ? 0 : 1]= minG;
									consistent= true;
								} else {
									gValues[(int)temp.pos.x, (int)temp.pos.z,useStandardAStar ? 0 : 1]= float.PositiveInfinity/100;
								}
							}
						}
						if (validPos && !consistent) {
							if (numUses [(int)newPos.x, (int)newPos.z] < numPathFindingSearches || fValues [(int)newPos.x, (int)newPos.z, useStandardAStar ? 0 : 1] > node.f) {
								numUses [(int)newPos.x, (int)newPos.z] = numPathFindingSearches;
								fValues [(int)newPos.x, (int)newPos.z, useStandardAStar ? 0 : 1] = node.f;
								gValues [(int)newPos.x, (int)newPos.z, useStandardAStar ? 0 : 1] = node.g;
								openList.Add (node);
								++numExpansions;
							}
						}
						//}
					}
					}
					openList.Remove (temp);
					yield return new WaitForSeconds (0.01f);
				}
				Collection<Vector3> path = new Collection<Vector3> ();
				if (temp != null) {
					CreatePath (temp, ref path, start, end, true, useStandardAStar ? Color.green : Color.magenta, 30f);
				}
				Debug.Log ((useStandardAStar ? "AStar" : "LOS*") + " Expansions: " + numExpansions + " " + reachedEnd);
				Debug.Log ("OpenList empty");
				//return false;
			} else {
				Debug.Log ("End out of bounds");
				//return false;
			}
		} else {
			Debug.Log ("Start out of bounds");
			//return false;
		}
		debugSearchActive = false;
		//MonoBehaviour.StartCoroutine (start, end, !useStandardAStar);
	}

	public static void CreatePath (PathFindingNode node, ref Collection<Vector3> path, Vector3 start, Vector3 end, bool visualizePath, Color pathCol = default(Color), float pathDrawTime = 0f)
	{
		PathFindingNode temp = node;
		path.Add (end);
		while (temp.previous!=null) {
			path.Insert (0, temp.pos);
			if (visualizePath) {
				Debug.DrawRay (temp.pos, temp.previous.pos - temp.pos, pathCol, pathDrawTime);
			}
			temp = temp.previous;
		}
	}

	public static float GetManhattanDistance (Vector3 start, Vector3 end)
	{
		return Mathf.Abs (start.x - end.x) + Mathf.Abs (start.y - end.y) + Mathf.Abs (start.z - end.z);
	}

//	public static float GetSquareDistanceEstimate(Vector3 start, Vector3 end){
//		float displacement_x = start.x > end.x ? (start.x - end.x) : (end.x - start.x);
//		float displacement_y = start.y > end.y ? (start.y - end.y) : (end.y - start.y);
//		float displacement_z = start.z > end.z ? (start.z - end.z) : (end.z - start.z);
//		float max_displacement = 0;
//		max_displacement = displacement_x > max_displacement ? displacement_x : max_displacement;
//		max_displacement = displacement_y > max_displacement ? displacement_y : max_displacement;
//		max_displacement = displacement_z > max_displacement ? displacement_z : max_displacement;
//		float averageDisplacement = (displacement_x + displacement_y + displacement_z) * (1f / 3f);
//		return (max_displacement-averageDisplacement)*(displacement_x+displacement_y+displacement_z);
	//}

	public static float GetDeltaMax(Vector3 start, Vector3 end, float diagonalCost, float nonDiagonalCost){
		float displacement_x = start.x > end.x ? (start.x - end.x) : (end.x - start.x);
		float displacement_y = start.y > end.y ? (start.y - end.y) : (end.y - start.y);
		float displacement_z = start.z > end.z ? (start.z - end.z) : (end.z - start.z);

		float max_displacement = 0;
		max_displacement = displacement_x > max_displacement ? displacement_x : max_displacement;
		max_displacement = displacement_y > max_displacement ? displacement_y : max_displacement;
		max_displacement = displacement_z > max_displacement ? displacement_z : max_displacement;

		float min_displacement = 0;
		min_displacement = displacement_x < max_displacement ? displacement_x : max_displacement;
		min_displacement = displacement_y < max_displacement ? displacement_y : max_displacement;
		min_displacement = displacement_z < max_displacement ? displacement_z : max_displacement;

		return diagonalCost * min_displacement + nonDiagonalCost * max_displacement;
	}
}
