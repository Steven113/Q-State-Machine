using UnityEngine;
using System.Collections;
using AssemblyCSharp;
using System.Collections.ObjectModel;
using System.Collections.Generic;

public class AIGrid : MonoBehaviour
{

	public int worldWidth = 100;
	public int worldHeight = 100;
	public static Vector3[] LOSDirections = null;
	public static Vector3[] SearchDirectionOffsets = null; //to make the code more reusable, when expanding in the 8 cardinal directions we just add these values to the current node position
	public static float[] SearchDirectionDistances = null;
	public static Vector3 worldCentre = Vector3.zero;

	public static Vector3[] corners = new Vector3[4];
	public static Vector3[] directionToSearchForNewCorner = new Vector3[4]; //if the corner is blocked, when finding a fleeing path, the pathfinder looks in this direction for a node near the corner that is unblocked

	public static bool[,] cellCanBeMovedThrough = new bool[0, 0];
	public static float[,,] visibilityDistances = new float[0, 0, 0];
	public static float[,,] fValues = new float[0, 0, 0];
	public static float[,,] gValues = new float[0, 0, 0];
	public static float[,,] rhsValues = new float[0, 0, 0];
	public static double[,] numUses = new double[0, 0];
	//public static float[,,] hMulipliers = new float[0, 0,0];
	public float t_hMultiplierArrayResolution = 0.1f; // how many cells vertically and horizintally a hmultiplier array cell should refer to
	public static float hMultiplierArrayResolution = 0.1f; // how many cells vertically and horizintally a hmultiplier array cell should refer to
	public static int s_cellWidth = 0;
	public int cellWidth = 1;
	public float t_heuristicMultiplierAdaptionRate = 0.1f;
	public float [] t_heuristicMultiplier = new float[2];

	public static double numPathFindingSearches = 0;
	public static bool debugSearchActive = false;

	//indicates if a agent should be given pathfinding priority. The agent at the front of the queue is the only one that can request a path
	public static Collection<object> priorityQueueByAgent = new Collection<object>();



	/*for all objects searching for a path, a record is created and stored to track the object(script)'s usage of the pathfinding thead
	 * Information such as the last start and end points are queried, to ensure that paths are not pointlessly recalculated
	 */
	public static Dictionary<object,PathFindingRecord> agentMap = new Dictionary<object, PathFindingRecord>(); 

	// Use this for initialization
	void Awake ()
	{
		//++numPathFindingSearches;
		//hMultiplierArrayResolution = t_hMultiplierArrayResolution;
		//PathFindingNode.heuristicMultiplierAdaptionRate = t_heuristicMultiplierAdaptionRate;
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
		numUses = new double[worldWidth, worldHeight];
		for (int i = 0; i<worldWidth; ++i) {
			for (int j = 0; j<worldHeight; ++j) {
				fValues [i, j, 0] = float.PositiveInfinity / 100;
				fValues [i, j, 1] = float.PositiveInfinity / 100;

				gValues [i, j, 0] = float.PositiveInfinity / 100;
				gValues [i, j, 1] = float.PositiveInfinity / 100;

				rhsValues [i, j, 0] = float.PositiveInfinity / 100;
				rhsValues [i, j, 1] = float.PositiveInfinity / 100;

				//Debug.Log(i + " " + j);
				numUses[i,j] = (double)UnityEngine.Random.Range(0f,1f);

				cellCanBeMovedThrough [i, j] = true;
			}
		}

		corners [0] = Vector3.zero;
		corners [1] = new Vector3 (1, 0, worldHeight-2);
		corners [2] = new Vector3 (worldWidth-2, 0, 1);
		corners [3] = new Vector3 (worldWidth-2, 0, worldHeight-2);

		directionToSearchForNewCorner [0] = Vector3.right;
		directionToSearchForNewCorner [1] = Vector3.back;
		directionToSearchForNewCorner [2] = Vector3.left;
		directionToSearchForNewCorner [3] = Vector3.back;

		//hMulipliers = new float[(int)(worldWidth * t_hMultiplierArrayResolution), (int)(worldHeight * t_hMultiplierArrayResolution), 2];

//		for (int i = 0; i<hMulipliers.GetLength(0); ++i) {
//			for (int j = 0; j<hMulipliers.GetLength(1); ++j) {
//				//for (int k = 0; k<hMulipliers.GetLength(2); ++k) {
//					hMulipliers[i,j,0] = 1;
//					hMulipliers[i,j,1] = 1;
//				//}
//
//			}
//		}


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
							visibilityDistances [i, j, k] += 1;
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

						for (int i = 0; i<cellCanBeMovedThrough.GetLength(0); ++i) {
							for (int j = 0; j<cellCanBeMovedThrough.GetLength(1); ++j) {
								
								if (cellCanBeMovedThrough [i, j]) {
									//Debug.DrawRay (new Vector3 (i * s_cellWidth, 0, j * s_cellWidth), Vector3.forward * s_cellWidth, Color.green, 100f);
									//Debug.DrawRay (new Vector3 (i * s_cellWidth, 0, j * s_cellWidth), Vector3.right * s_cellWidth, Color.green, 100f);
								} else {
									//Debug.DrawRay (new Vector3 (i * s_cellWidth, 0, j * s_cellWidth), Vector3.forward * s_cellWidth, Color.black, 100f);
									//Debug.DrawRay (new Vector3 (i * s_cellWidth, 0, j * s_cellWidth), Vector3.right * s_cellWidth, Color.black, 100f);
								}
								
							}
						}
	}
	// Update is called once per frame
	void Update () {
		//t_heuristicMultiplierAdaptionRate = PathFindingNode.heuristicMultiplier;
		//t_heuristicMultiplier = PathFindingNode.heuristicMultiplier;
	}

	public static bool findPath (object caller, Vector3 start, Vector3 end, out Collection<Vector3> path, bool useStandardAStar, bool useLPA)
	{
		path = new Collection<Vector3>();
		++AIGrid.numPathFindingSearches;
		float timeToRun = 0;
		if (useLPA) {
			Vector3 temp = start;
			start = end;
			end = temp;
		}

		if (!agentMap.ContainsKey(caller)){
			agentMap.Add(caller,new PathFindingRecord(agentMap.Count));
		}

		if (!priorityQueueByAgent.Contains (caller)) {
			priorityQueueByAgent.Add(caller);
		}

		if (priorityQueueByAgent [0] != caller) {
			return false;
		} else {
			priorityQueueByAgent.Add(caller);
			priorityQueueByAgent.RemoveAt(0);
		}

		PathFindingRecord pathFindingRecord = agentMap [caller];

		debugSearchActive = true;
		++AIGrid.numPathFindingSearches;
		//path = new Collection<Vector3> ();
		//first check that start and end are within bounds
		bool keepSearching = true;
		if (start.x >= 0 && start.x < AIGrid.cellCanBeMovedThrough.GetLength (0) && start.z >= 0 && start.z < AIGrid.cellCanBeMovedThrough.GetLength (1)) {
			if (end.x >= 0 && end.x < AIGrid.cellCanBeMovedThrough.GetLength (0) && end.z >= 0 && end.z < AIGrid.cellCanBeMovedThrough.GetLength (1)) {
				if (!AIGrid.cellCanBeMovedThrough [(int)end.x, (int)end.z]) {
					//Debug.Log ("Cannot move to destination cell");
					return false;
				} else if (pathFindingRecord.destination.x != float.PositiveInfinity && (int)pathFindingRecord.destination.x == (int)end.x && (int)pathFindingRecord.destination.z == (int)end.z){
					return true;
				}
//				for (int i = 0; i<cellCanBeMovedThrough.GetLength(0); ++i) {
//					for (int j = 0; j<cellCanBeMovedThrough.GetLength(1); ++j) {
//						
//						if (cellCanBeMovedThrough [i, j]) {
//							//Debug.DrawRay (new Vector3 (i * s_cellWidth, 0, j * s_cellWidth), Vector3.forward * s_cellWidth, Color.green, 100f);
//							//Debug.DrawRay (new Vector3 (i * s_cellWidth, 0, j * s_cellWidth), Vector3.right * s_cellWidth, Color.green, 100f);
//						} else {
//							//Debug.DrawRay (new Vector3 (i * s_cellWidth, 0, j * s_cellWidth), Vector3.forward * s_cellWidth, Color.black, 100f);
//							//Debug.DrawRay (new Vector3 (i * s_cellWidth, 0, j * s_cellWidth), Vector3.right * s_cellWidth, Color.black, 100f);
//						}
//						
//					}
//				}
				
				
				NodeList<PathFindingNode> openList = new NodeList<PathFindingNode> ();
				bool reachedEnd = false;
				openList.Add (new PathFindingNode (null, start, start, end, ref reachedEnd, useStandardAStar, false));
				
				int numIter = 0;
				int maxNumIter = 100;
				int numExpansions = 0;
				PathFindingNode temp = openList [0];
				gValues [(int)(temp.pos.x), (int)(temp.pos.z), useStandardAStar ? 0 : 1] = 0;
				while (openList.Count>0 && !reachedEnd) {
					float startTime = Time.realtimeSinceStartup;
					++numIter;
					if (numIter > maxNumIter) {
						//Debug.Log ("Num iterations max exceeded");
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
						for (int i = 0; i< 8; ++i) {
							//if (i == index)
							//	continue;
							//newPos = temp.pos + SearchDirectionOffsets [i];
							//Debug.Log("Exploring "+newPos + (useStandardAStar?"AStar":"LOS*"));
							if ((int)newPos.x == (int)end.x && (int)newPos.z == (int)end.z) {
								reachedEnd = true;
								break;
							}
							
							//if ((int)newPos.x == (int)end.x && (int)newPos.z == (int)end.z) {
							bool validPos = false;
							//if ((int)newPos.x >= 0 && (int)newPos.x < AIGrid.cellCanBeMovedThrough.GetLength (0) && (int)newPos.z >= 0 && (int)newPos.z < AIGrid.cellCanBeMovedThrough.GetLength (1) && AIGrid.cellCanBeMovedThrough [(int)newPos.x, (int)newPos.z]) {
							PathFindingNode node = null;
							bool foundShortCutExpansion = false;
							//jump search when using LOS*. We attempt to exponentially increase the jump distance, to reduce the expansion amount
							
							if (!useStandardAStar && numIter > 1) {
								//float dotTemp = Vector3.Dot (newPos - temp.pos, end - temp.pos);
								//float dotTemp = Vector3.Dot (LOSDirections[i],  (temp.previous != null ? temp.pos - temp.previous.pos : LOSDirections[i])); //we try to eliminate expansions by avoiding expanding back the direction we came. So we check the dot product of the expansion direction vs expansion direction of predecessor
								//float dotTemp2 = Vector3.Dot (newPos - temp.pos, end - (temp.previous != null ? temp.previous.pos : start));
								
								//if (dotTemp>-0.9f){
								//	break;
								//}
								
								//if (numIter>2 && i%2==numIter%2){
								////	break;
								//}
								
								float [] distances = new float[8];
								//newPos = temp.pos + SearchDirectionOffsets [i];
								newPos = temp.pos;
								
								Vector3 averageExpansionDirection = Vector3.zero;
								averageExpansionDirection.x = ((LOSDirections [i].x + (temp.pos.x - (temp.previous != null ? temp.previous.pos.x : start.x))) * 0.5f);
								averageExpansionDirection.y = ((LOSDirections [i].y + (temp.pos.x - (temp.previous != null ? temp.previous.pos.x : start.y))) * 0.5f);
								averageExpansionDirection.z = ((LOSDirections [i].z + (temp.pos.x - (temp.previous != null ? temp.previous.pos.x : start.z))) * 0.5f);
								float dotProductToCheckForForcedNeighbour = -1;
								int dirToCheck = 0;
								
								for (int k = 0; k<8; ++k) {
									float temp_d = 0;
									temp_d += (LOSDirections [k].x*averageExpansionDirection.x);
									temp_d += (LOSDirections [k].y*averageExpansionDirection.y);
									temp_d += (LOSDirections [k].z*averageExpansionDirection.z);
									if (temp_d < dotProductToCheckForForcedNeighbour) {
										dotProductToCheckForForcedNeighbour = temp_d;
										dirToCheck = k;
									}
								}
								
								//if (dotTemp<-0.9f){
								//	break;
								//}
								
								for (int k = 0; k<8; ++k) {
									distances [k] = visibilityDistances [(int)temp.pos.x, (int)temp.pos.z, k];
								}
								
								//bool obstacleDistributionHasChanged = false; //has the relative presence of obstacles next to the jump point changed?
								if ((int)(newPos.x + SearchDirectionOffsets [i].x) >= 0 && (int)(newPos.x + SearchDirectionOffsets [i].x) < AIGrid.cellCanBeMovedThrough.GetLength (0) && (int)(newPos.z + SearchDirectionOffsets [i].z) >= 0 && (int)(newPos.z + SearchDirectionOffsets [i].z) < AIGrid.cellCanBeMovedThrough.GetLength (1) && AIGrid.cellCanBeMovedThrough [(int)(newPos.x + SearchDirectionOffsets [i].x), (int)(newPos.z + SearchDirectionOffsets [i].z)] && visibilityDistances [(int)temp.pos.x, (int)temp.pos.z, dirToCheck] > 0) {
									foundShortCutExpansion = true;
									int numJumpPointExpansions = 0;
									bool terminateAfterNextIteration = false;
									//for the directions at 45 degrees to the search direction, either the visibility distance must either match the initial visibility in that direction or be zero too
									while ((int)(newPos.x+ SearchDirectionOffsets [i].x) >= 0 && (int)(newPos.x+ SearchDirectionOffsets [i].x) < AIGrid.cellCanBeMovedThrough.GetLength (0) && (int)(newPos.z+ SearchDirectionOffsets [i].z) >= 0 && (int)(newPos.z+ SearchDirectionOffsets [i].z) < AIGrid.cellCanBeMovedThrough.GetLength (1) && AIGrid.cellCanBeMovedThrough [(int)(newPos.x+ SearchDirectionOffsets [i].x), (int)(newPos.z+ SearchDirectionOffsets [i].z)] /*&& (distances[(i+1)%8] == visibilityDistances[(int)(newPos.x+ SearchDirectionOffsets [i].x), (int)(int)(newPos.z+ SearchDirectionOffsets [i].z),(i+1)%8] || visibilityDistances[(int)(newPos.x+ SearchDirectionOffsets [i].x), (int)(int)(newPos.z+ SearchDirectionOffsets [i].z),(i+1)%8]>=numJumpPointExpansions*SearchDirectionDistances[(i+1)%8]) && (distances[(i+7)%8] == visibilityDistances[(int)(newPos.x+ SearchDirectionOffsets [i].x), (int)(int)(newPos.z+ SearchDirectionOffsets [i].z),(i+7)%8] || visibilityDistances[(int)(int)(newPos.x+ SearchDirectionOffsets [i].x), (int)(int)(newPos.z+ SearchDirectionOffsets [i].z),(i+7)%8]>=numJumpPointExpansions*SearchDirectionDistances[(i+7)%8])*/) {
										newPos = newPos + SearchDirectionOffsets [i];
										++numJumpPointExpansions;
										bool noForcedNeighbours = true;
										int nextDirectionIndex = (i+1)%8;
										float expansionDistance = 1;
										while (expansionDistance<2 && noForcedNeighbours && (int)(newPos.x+ SearchDirectionOffsets [nextDirectionIndex].x*expansionDistance) >= 0 && (int)(newPos.x+ SearchDirectionOffsets [nextDirectionIndex].x*expansionDistance) < AIGrid.cellCanBeMovedThrough.GetLength (0) && (int)(newPos.z+ SearchDirectionOffsets [nextDirectionIndex].z*expansionDistance) >= 0 && (int)(newPos.z+ SearchDirectionOffsets [nextDirectionIndex].z*expansionDistance) < AIGrid.cellCanBeMovedThrough.GetLength (1) && AIGrid.cellCanBeMovedThrough[(int)(newPos.x+ SearchDirectionOffsets [nextDirectionIndex].x*expansionDistance),(int)(newPos.z+ SearchDirectionOffsets [nextDirectionIndex].z*expansionDistance)]){
											
											for (int n = 2; n<=2 && noForcedNeighbours; ++n){
												if (AIGrid.visibilityDistances[(int)(newPos.x+ SearchDirectionOffsets [nextDirectionIndex].x*expansionDistance),(int)(newPos.z+ SearchDirectionOffsets [nextDirectionIndex].z*expansionDistance),(nextDirectionIndex+n)%8] ==0 && AIGrid.visibilityDistances[(int)(temp.pos.x),(int)(temp.pos.z),(nextDirectionIndex+n)%8] >0){
													noForcedNeighbours = false;
													//Debug.Log("Forced Neighbours! 1 "+SearchDirectionOffsets[i]+ " " + n + " " + numIter);
												}
											}
											
											for (int n = 7; n<=6 && noForcedNeighbours; ++n){
												if (AIGrid.visibilityDistances[(int)(newPos.x+ SearchDirectionOffsets [nextDirectionIndex].x*expansionDistance),(int)(newPos.z+ SearchDirectionOffsets [nextDirectionIndex].z*expansionDistance),(nextDirectionIndex+n)%8] ==0 && AIGrid.visibilityDistances[(int)(temp.pos.x),(int)(temp.pos.z),(nextDirectionIndex+n)%8] >0){
													noForcedNeighbours = false;
													//Debug.Log("Forced Neighbours! 2 "+SearchDirectionOffsets[i]+ " " + n + " " + numIter);
												}
											}
											expansionDistance+=1;
										}
										
										nextDirectionIndex = (i+7)%8;
										expansionDistance = 1;
										
										while (expansionDistance<2 && noForcedNeighbours && (int)(newPos.x+ SearchDirectionOffsets [nextDirectionIndex].x*expansionDistance) >= 0 && (int)(newPos.x+ SearchDirectionOffsets [nextDirectionIndex].x*expansionDistance) < AIGrid.cellCanBeMovedThrough.GetLength (0) && (int)(newPos.z+ SearchDirectionOffsets [nextDirectionIndex].z*expansionDistance) >= 0 && (int)(newPos.z+ SearchDirectionOffsets [nextDirectionIndex].z*expansionDistance) < AIGrid.cellCanBeMovedThrough.GetLength (1) && AIGrid.cellCanBeMovedThrough[(int)(newPos.x+ SearchDirectionOffsets [nextDirectionIndex].x*expansionDistance),(int)(newPos.z+ SearchDirectionOffsets [nextDirectionIndex].z*expansionDistance)]){
											
											for (int n = 2; n<=2 && noForcedNeighbours; ++n){
												//Debug.DrawRay(newPos+ SearchDirectionOffsets [nextDirectionIndex]*expansionDistance,visibilityDistances[(int)(newPos.x+ SearchDirectionOffsets [nextDirectionIndex].x*expansionDistance), (int)(newPos.z+ SearchDirectionOffsets [nextDirectionIndex].z*expansionDistance),n]*LOSDirections[n],Color.yellow);
												if (AIGrid.visibilityDistances[(int)(newPos.x+ SearchDirectionOffsets [nextDirectionIndex].x*expansionDistance),(int)(newPos.z+ SearchDirectionOffsets [nextDirectionIndex].z*expansionDistance),(nextDirectionIndex+n)%8] ==0 && AIGrid.visibilityDistances[(int)(temp.pos.x),(int)(temp.pos.z),(nextDirectionIndex+n)%8] >0){
													noForcedNeighbours = false;
													//Debug.Log("Forced Neighbours! 3 "+SearchDirectionOffsets[i]+ " " + n + " " + numIter);
												}
											}
											
											for (int n = 6; n<=6 && noForcedNeighbours; ++n){
												//Debug.DrawRay(newPos+ SearchDirectionOffsets [nextDirectionIndex]*expansionDistance,visibilityDistances[(int)(newPos.x+ SearchDirectionOffsets [nextDirectionIndex].x*expansionDistance), (int)(newPos.z+ SearchDirectionOffsets [nextDirectionIndex].z*expansionDistance),n]*LOSDirections[n],Color.yellow);
												if (AIGrid.visibilityDistances[(int)(newPos.x+ SearchDirectionOffsets [nextDirectionIndex].x*expansionDistance),(int)(newPos.z+ SearchDirectionOffsets [nextDirectionIndex].z*expansionDistance),(nextDirectionIndex+n)%8] ==0 && AIGrid.visibilityDistances[(int)(temp.pos.x),(int)(temp.pos.z),(nextDirectionIndex+n)%8] >0){
													noForcedNeighbours = false;
													//Debug.Log("Forced Neighbours! 4 "+SearchDirectionOffsets[i]+ " " + n + " " + numIter);
												}
											}
											expansionDistance+=1;
											//SearchDirectionDistances
										}
										
										if (!noForcedNeighbours){
											break;
										}
										
										//numUses [(int)newPos.x, (int)newPos.z] = numPathFindingSearches;
										//if ((temp.f+numExpanded * SearchDirectionDistances[i])<(fValues [(int)newPos.x, (int)newPos.z,useStandardAStar?0:1])) fValues [(int)newPos.x, (int)newPos.z,useStandardAStar?0:1] = temp.f+numExpanded * SearchDirectionDistances[i];
										if ((int)newPos.x == (int)end.x && (int)newPos.z == (int)end.z) {
											reachedEnd = true;
											//if (temp != null) {
												CreatePath (temp, ref path, start, end, true, useStandardAStar, useLPA, useStandardAStar ? Color.green : Color.magenta, 30f);
											//}
											return true;
										}
										
										//										if (visibilityDistances[(int)temp.pos.x,(int)temp.pos.z,(int)(3f + 2*Vector3.Dot((end-newPos).normalized,Vector3.left))]>GetManhattanDistance(newPos,end)){
										//											break;
										//										}
										//numUses [(int)newPos.x, (int)newPos.z] = numPathFindingSearches;
										//
										//										obstacleDistributionHasChanged = false;
										//										if (numExpanded > 1) {
										//											for (int k = 0; k<8 && !obstacleDistributionHasChanged; ++k) {
										//												if ((!((visibilityDistances [(int)newPos.x, (int)newPos.z, k] == 0) == (distances [k] == 0)) /*|| ((visibilityDistances [(int)newPos.x, (int)newPos.z, k]-2)>(distances[k]))*/) && Vector3.Dot (LOSDirections [i], LOSDirections [k]) > (dotTemp*0.5f-0.5f)) {
										//													obstacleDistributionHasChanged = true;
										//												}
										//											}
										//
										//										}
										//										if (obstacleDistributionHasChanged) {
										//											break;
										//										}
										//if (visibilityDistances[(int)newPos.x, (int)newPos.z,(int)(3f+(Vector3.Dot((end-newPos).normalized,Vector3.left)*4f))]>GetManhattanDistance(end,newPos)){
										//	break;
										//}
									}
									//for (int n = 0; n<8; ++n){
									//	Debug.DrawRay(newPos,visibilityDistances[(int)newPos.x, (int)newPos.z,n]*LOSDirections[n],Color.yellow);
									//}
									node = new PathFindingNode (temp, newPos, start, end, ref reachedEnd, useStandardAStar, true);
									if (numUses [(int)newPos.x, (int)newPos.z] < numPathFindingSearches || fValues [(int)newPos.x, (int)newPos.z, useStandardAStar ? 0 : 1] > node.f) {
										numUses [(int)newPos.x, (int)newPos.z] = numPathFindingSearches;
										fValues [(int)newPos.x, (int)newPos.z, useStandardAStar ? 0 : 1] = node.f;
										gValues [(int)newPos.x, (int)newPos.z, useStandardAStar ? 0 : 1] = node.g;
										
										openList.Add (node);
										++numExpansions;
									}
								}
								
							}
							//							if (openList.Count > 1 && !useStandardAStar && dotTemp>-0.5f && dotTemp2>=-0.5f &&  temp.deltaLosDistance > 1 && visibilityDistances [(int)temp.pos.x, (int)temp.pos.z, i] > (temp.deltaLosDistance * temp.deltaLosDistance)) {
							//							float dist = (temp.deltaLosDistance * temp.deltaLosDistance);
							//							newPos = temp.pos + SearchDirectionOffsets [i] * dist;
							//							if ((int)newPos.x >= 0 && (int)newPos.x < AIGrid.cellCanBeMovedThrough.GetLength (0) && (int)newPos.z >= 0 && (int)newPos.z < AIGrid.cellCanBeMovedThrough.GetLength (1) && AIGrid.cellCanBeMovedThrough [(int)newPos.x, (int)newPos.z]) {
							//								foundShortCutExpansion = true;	
							//								validPos = true;
							//								node = new PathFindingNode (temp, newPos, start, end, ref reachedEnd, useStandardAStar,true);
							//								for (float k = 1; k<dist-1; ++k) {
							//									fValues [(int)(temp.pos.x + k * SearchDirectionOffsets [i].x), (int)(temp.pos.z + k * SearchDirectionOffsets [i].z), useStandardAStar ? 0 : 1] = (((k - 1) / dist) * node.f) < fValues [(int)(temp.pos.x + k * SearchDirectionOffsets [i].x), (int)(temp.pos.z + k * SearchDirectionOffsets [i].z), useStandardAStar ? 0 : 1] ? (((k - 1) / dist) * node.f) : fValues [(int)(temp.pos.x + k * SearchDirectionOffsets [i].x), (int)(temp.pos.z + k * SearchDirectionOffsets [i].z), useStandardAStar ? 0 : 1];
							//									numUses[(int)(temp.pos.x+k*SearchDirectionOffsets[i].x),(int)(temp.pos.z+k*SearchDirectionOffsets[i].z)] = numPathFindingSearches;
							//								}
							//
							//										if (numUses [(int)newPos.x, (int)newPos.z] < numPathFindingSearches || fValues [(int)newPos.x, (int)newPos.z, useStandardAStar ? 0 : 1] > node.f) {
							//											numUses [(int)newPos.x, (int)newPos.z] = numPathFindingSearches;
							//											fValues [(int)newPos.x, (int)newPos.z, useStandardAStar ? 0 : 1] = node.f;
							//											gValues [(int)newPos.x, (int)newPos.z, useStandardAStar ? 0 : 1] = node.g;
							//											openList.Add (node);
							//											++numExpansions;
							//										}
							
							//}
							//} 
							
							if (!foundShortCutExpansion) {
								newPos = temp.pos + SearchDirectionOffsets [i];
								if ((int)newPos.x >= 0 && (int)newPos.x < AIGrid.cellCanBeMovedThrough.GetLength (0) && (int)newPos.z >= 0 && (int)newPos.z < AIGrid.cellCanBeMovedThrough.GetLength (1) && AIGrid.cellCanBeMovedThrough [(int)newPos.x, (int)newPos.z]) {
									validPos = true;
									node = new PathFindingNode (temp, newPos, start, end, ref reachedEnd, useStandardAStar, false);
								}
								
							}
							if (reachedEnd) { //adding to the openlist is expensive, and we've found the end, so we break now
								break;
							}
							
							consistent = false;
							if (useLPA) {
								float minG = float.PositiveInfinity;
								for (int j = 0; j< 8; ++j) {
									if ((int)(newPos.x + SearchDirectionOffsets [i].x) >= 0 && (int)(newPos.x + SearchDirectionOffsets [i].x) < AIGrid.cellCanBeMovedThrough.GetLength (0) && (int)(newPos.z + SearchDirectionOffsets [i].z) >= 0 && (int)(newPos.z + SearchDirectionOffsets [i].z) < AIGrid.cellCanBeMovedThrough.GetLength (1) && AIGrid.cellCanBeMovedThrough [(int)(newPos.x + SearchDirectionOffsets [i].x), (int)(newPos.z + SearchDirectionOffsets [i].z)]) {
										float tempG = gValues [(int)(temp.pos.x + SearchDirectionOffsets [i].x), (int)(temp.pos.z + SearchDirectionOffsets [i].z), useStandardAStar ? 0 : 1] + SearchDirectionDistances [j];
										minG = minG > tempG ? tempG : minG;
									}
								}
								
								//					if (minG==gValues[(int)temp.pos.x, (int)temp.pos.z,useStandardAStar ? 0 : 1]){
								//						//gValues[(int)temp.pos.x, (int)temp.pos.z,useStandardAStar ? 0 : 1]= minG;
								//						consistent= true;
								//					} else
								if (numIter > 2) {
									if (minG > gValues [(int)temp.pos.x, (int)temp.pos.z, useStandardAStar ? 0 : 1]) {
										gValues [(int)temp.pos.x, (int)temp.pos.z, useStandardAStar ? 0 : 1] = minG;
										consistent = true;
									} else {
										gValues [(int)temp.pos.x, (int)temp.pos.z, useStandardAStar ? 0 : 1] = float.PositiveInfinity / 100;
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
					float endTime = Time.realtimeSinceStartup;
					timeToRun += (endTime-startTime);
					//yield return new WaitForSeconds (0.01f);
				}
				//path = new Collection<Vector3> ();

				if (temp != null) {
					CreatePath (temp, ref path, start, end, true, useStandardAStar, useLPA, useStandardAStar ? Color.green : Color.magenta, 30f);
				}
				//Debug.Log ((useStandardAStar ? "AStar" : "LOS*") + " Expansions: " + numExpansions + " " + reachedEnd + " " + timeToRun);
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
		debugSearchActive = false;
		//MonoBehaviour.StartCoroutine (start, end, !useStandardAStar);
	}

	public static IEnumerator findPathCoroutine (Vector3 start, Vector3 end, bool useStandardAStar, bool useLPA)
	{
		float timeToRun = 0;
		if (useLPA) {
			Vector3 temp = start;
			start = end;
			end = temp;
		}
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
						} else {
							Debug.DrawRay (new Vector3 (i * s_cellWidth, 0, j * s_cellWidth), Vector3.forward * s_cellWidth, Color.black, 100f);
							Debug.DrawRay (new Vector3 (i * s_cellWidth, 0, j * s_cellWidth), Vector3.right * s_cellWidth, Color.black, 100f);
						}
						
					}
				}
				

				NodeList<PathFindingNode> openList = new NodeList<PathFindingNode> ();
				bool reachedEnd = false;
				openList.Add (new PathFindingNode (null, start, start, end, ref reachedEnd, useStandardAStar, false));
				
				int numIter = 0;
				int maxNumIter = cellCanBeMovedThrough.GetLength (0) * cellCanBeMovedThrough.GetLength (1);
				int numExpansions = 0;
				PathFindingNode temp = openList [0];
				gValues [(int)(temp.pos.x), (int)(temp.pos.z), useStandardAStar ? 0 : 1] = 0;
				while (openList.Count>0 && !reachedEnd) {
					float startTime = Time.realtimeSinceStartup;
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

							//if ((int)newPos.x == (int)end.x && (int)newPos.z == (int)end.z) {
							bool validPos = false;
							//if ((int)newPos.x >= 0 && (int)newPos.x < AIGrid.cellCanBeMovedThrough.GetLength (0) && (int)newPos.z >= 0 && (int)newPos.z < AIGrid.cellCanBeMovedThrough.GetLength (1) && AIGrid.cellCanBeMovedThrough [(int)newPos.x, (int)newPos.z]) {
							PathFindingNode node = null;
							bool foundShortCutExpansion = false;
							//jump search when using LOS*. We attempt to exponentially increase the jump distance, to reduce the expansion amount

							if (!useStandardAStar && numIter > 1) {
								//float dotTemp = Vector3.Dot (newPos - temp.pos, end - temp.pos);
								//float dotTemp = Vector3.Dot (LOSDirections[i],  (temp.previous != null ? temp.pos - temp.previous.pos : LOSDirections[i])); //we try to eliminate expansions by avoiding expanding back the direction we came. So we check the dot product of the expansion direction vs expansion direction of predecessor
								//float dotTemp2 = Vector3.Dot (newPos - temp.pos, end - (temp.previous != null ? temp.previous.pos : start));

								//if (dotTemp>-0.9f){
								//	break;
								//}

								//if (numIter>2 && i%2==numIter%2){
								////	break;
								//}

								float [] distances = new float[8];
								//newPos = temp.pos + SearchDirectionOffsets [i];
								newPos = temp.pos;

								Vector3 averageExpansionDirection = ((LOSDirections [i] + (temp.pos - (temp.previous != null ? temp.previous.pos : start))) * 0.5f).normalized;
								float dotProductToCheckForForcedNeighbour = -1;
								int dirToCheck = 0;

								for (int k = 0; k<8; ++k) {
									float temp_d = Vector3.Dot (LOSDirections [k], averageExpansionDirection);
									if (temp_d < dotProductToCheckForForcedNeighbour) {
										dotProductToCheckForForcedNeighbour = temp_d;
										dirToCheck = k;
									}
								}

								//if (dotTemp<-0.9f){
								//	break;
								//}

								for (int k = 0; k<8; ++k) {
									distances [k] = visibilityDistances [(int)temp.pos.x, (int)temp.pos.z, k];
								}

								//bool obstacleDistributionHasChanged = false; //has the relative presence of obstacles next to the jump point changed?
								if ((int)(newPos.x + SearchDirectionOffsets [i].x) >= 0 && (int)(newPos.x + SearchDirectionOffsets [i].x) < AIGrid.cellCanBeMovedThrough.GetLength (0) && (int)(newPos.z + SearchDirectionOffsets [i].z) >= 0 && (int)(newPos.z + SearchDirectionOffsets [i].z) < AIGrid.cellCanBeMovedThrough.GetLength (1) && AIGrid.cellCanBeMovedThrough [(int)(newPos.x + SearchDirectionOffsets [i].x), (int)(newPos.z + SearchDirectionOffsets [i].z)] && visibilityDistances [(int)temp.pos.x, (int)temp.pos.z, dirToCheck] > 0) {
									foundShortCutExpansion = true;
									int numJumpPointExpansions = 0;
									bool terminateAfterNextIteration = false;
									//for the directions at 45 degrees to the search direction, either the visibility distance must either match the initial visibility in that direction or be zero too
									while ((int)(newPos.x+ SearchDirectionOffsets [i].x) >= 0 && (int)(newPos.x+ SearchDirectionOffsets [i].x) < AIGrid.cellCanBeMovedThrough.GetLength (0) && (int)(newPos.z+ SearchDirectionOffsets [i].z) >= 0 && (int)(newPos.z+ SearchDirectionOffsets [i].z) < AIGrid.cellCanBeMovedThrough.GetLength (1) && AIGrid.cellCanBeMovedThrough [(int)(newPos.x+ SearchDirectionOffsets [i].x), (int)(newPos.z+ SearchDirectionOffsets [i].z)] /*&& (distances[(i+1)%8] == visibilityDistances[(int)(newPos.x+ SearchDirectionOffsets [i].x), (int)(int)(newPos.z+ SearchDirectionOffsets [i].z),(i+1)%8] || visibilityDistances[(int)(newPos.x+ SearchDirectionOffsets [i].x), (int)(int)(newPos.z+ SearchDirectionOffsets [i].z),(i+1)%8]>=numJumpPointExpansions*SearchDirectionDistances[(i+1)%8]) && (distances[(i+7)%8] == visibilityDistances[(int)(newPos.x+ SearchDirectionOffsets [i].x), (int)(int)(newPos.z+ SearchDirectionOffsets [i].z),(i+7)%8] || visibilityDistances[(int)(int)(newPos.x+ SearchDirectionOffsets [i].x), (int)(int)(newPos.z+ SearchDirectionOffsets [i].z),(i+7)%8]>=numJumpPointExpansions*SearchDirectionDistances[(i+7)%8])*/) {
										newPos = newPos + SearchDirectionOffsets [i];
										++numJumpPointExpansions;
										bool noForcedNeighbours = true;
										int nextDirectionIndex = (i+1)%8;
										float expansionDistance = 1;
										while (expansionDistance<2 && noForcedNeighbours && (int)(newPos.x+ SearchDirectionOffsets [nextDirectionIndex].x*expansionDistance) >= 0 && (int)(newPos.x+ SearchDirectionOffsets [nextDirectionIndex].x*expansionDistance) < AIGrid.cellCanBeMovedThrough.GetLength (0) && (int)(newPos.z+ SearchDirectionOffsets [nextDirectionIndex].z*expansionDistance) >= 0 && (int)(newPos.z+ SearchDirectionOffsets [nextDirectionIndex].z*expansionDistance) < AIGrid.cellCanBeMovedThrough.GetLength (1) && AIGrid.cellCanBeMovedThrough[(int)(newPos.x+ SearchDirectionOffsets [nextDirectionIndex].x*expansionDistance),(int)(newPos.z+ SearchDirectionOffsets [nextDirectionIndex].z*expansionDistance)]){

											for (int n = 2; n<=2 && noForcedNeighbours; ++n){
												if (AIGrid.visibilityDistances[(int)(newPos.x+ SearchDirectionOffsets [nextDirectionIndex].x*expansionDistance),(int)(newPos.z+ SearchDirectionOffsets [nextDirectionIndex].z*expansionDistance),(nextDirectionIndex+n)%8] ==0 && AIGrid.visibilityDistances[(int)(temp.pos.x),(int)(temp.pos.z),(nextDirectionIndex+n)%8] >0){
													noForcedNeighbours = false;
													//Debug.Log("Forced Neighbours! 1 "+SearchDirectionOffsets[i]+ " " + n + " " + numIter);
												}
											}

											for (int n = 7; n<=6 && noForcedNeighbours; ++n){
												if (AIGrid.visibilityDistances[(int)(newPos.x+ SearchDirectionOffsets [nextDirectionIndex].x*expansionDistance),(int)(newPos.z+ SearchDirectionOffsets [nextDirectionIndex].z*expansionDistance),(nextDirectionIndex+n)%8] ==0 && AIGrid.visibilityDistances[(int)(temp.pos.x),(int)(temp.pos.z),(nextDirectionIndex+n)%8] >0){
													noForcedNeighbours = false;
													//Debug.Log("Forced Neighbours! 2 "+SearchDirectionOffsets[i]+ " " + n + " " + numIter);
												}
											}
											expansionDistance+=1;
										}

										nextDirectionIndex = (i+7)%8;
										expansionDistance = 1;

										while (expansionDistance<2 && noForcedNeighbours && (int)(newPos.x+ SearchDirectionOffsets [nextDirectionIndex].x*expansionDistance) >= 0 && (int)(newPos.x+ SearchDirectionOffsets [nextDirectionIndex].x*expansionDistance) < AIGrid.cellCanBeMovedThrough.GetLength (0) && (int)(newPos.z+ SearchDirectionOffsets [nextDirectionIndex].z*expansionDistance) >= 0 && (int)(newPos.z+ SearchDirectionOffsets [nextDirectionIndex].z*expansionDistance) < AIGrid.cellCanBeMovedThrough.GetLength (1) && AIGrid.cellCanBeMovedThrough[(int)(newPos.x+ SearchDirectionOffsets [nextDirectionIndex].x*expansionDistance),(int)(newPos.z+ SearchDirectionOffsets [nextDirectionIndex].z*expansionDistance)]){

											for (int n = 2; n<=2 && noForcedNeighbours; ++n){
												//Debug.DrawRay(newPos+ SearchDirectionOffsets [nextDirectionIndex]*expansionDistance,visibilityDistances[(int)(newPos.x+ SearchDirectionOffsets [nextDirectionIndex].x*expansionDistance), (int)(newPos.z+ SearchDirectionOffsets [nextDirectionIndex].z*expansionDistance),n]*LOSDirections[n],Color.yellow);
												if (AIGrid.visibilityDistances[(int)(newPos.x+ SearchDirectionOffsets [nextDirectionIndex].x*expansionDistance),(int)(newPos.z+ SearchDirectionOffsets [nextDirectionIndex].z*expansionDistance),(nextDirectionIndex+n)%8] ==0 && AIGrid.visibilityDistances[(int)(temp.pos.x),(int)(temp.pos.z),(nextDirectionIndex+n)%8] >0){
													noForcedNeighbours = false;
													//Debug.Log("Forced Neighbours! 3 "+SearchDirectionOffsets[i]+ " " + n + " " + numIter);
												}
											}
											
											for (int n = 6; n<=6 && noForcedNeighbours; ++n){
												//Debug.DrawRay(newPos+ SearchDirectionOffsets [nextDirectionIndex]*expansionDistance,visibilityDistances[(int)(newPos.x+ SearchDirectionOffsets [nextDirectionIndex].x*expansionDistance), (int)(newPos.z+ SearchDirectionOffsets [nextDirectionIndex].z*expansionDistance),n]*LOSDirections[n],Color.yellow);
												if (AIGrid.visibilityDistances[(int)(newPos.x+ SearchDirectionOffsets [nextDirectionIndex].x*expansionDistance),(int)(newPos.z+ SearchDirectionOffsets [nextDirectionIndex].z*expansionDistance),(nextDirectionIndex+n)%8] ==0 && AIGrid.visibilityDistances[(int)(temp.pos.x),(int)(temp.pos.z),(nextDirectionIndex+n)%8] >0){
													noForcedNeighbours = false;
													//Debug.Log("Forced Neighbours! 4 "+SearchDirectionOffsets[i]+ " " + n + " " + numIter);
												}
											}
											expansionDistance+=1;
											//SearchDirectionDistances
										}

										if (!noForcedNeighbours){
											break;
										}

										//numUses [(int)newPos.x, (int)newPos.z] = numPathFindingSearches;
										//if ((temp.f+numExpanded * SearchDirectionDistances[i])<(fValues [(int)newPos.x, (int)newPos.z,useStandardAStar?0:1])) fValues [(int)newPos.x, (int)newPos.z,useStandardAStar?0:1] = temp.f+numExpanded * SearchDirectionDistances[i];
										if ((int)newPos.x == (int)end.x && (int)newPos.z == (int)end.z) {
											reachedEnd = true;
											break;
										}

//										if (visibilityDistances[(int)temp.pos.x,(int)temp.pos.z,(int)(3f + 2*Vector3.Dot((end-newPos).normalized,Vector3.left))]>GetManhattanDistance(newPos,end)){
//											break;
//										}
										//numUses [(int)newPos.x, (int)newPos.z] = numPathFindingSearches;
//
//										obstacleDistributionHasChanged = false;
//										if (numExpanded > 1) {
//											for (int k = 0; k<8 && !obstacleDistributionHasChanged; ++k) {
//												if ((!((visibilityDistances [(int)newPos.x, (int)newPos.z, k] == 0) == (distances [k] == 0)) /*|| ((visibilityDistances [(int)newPos.x, (int)newPos.z, k]-2)>(distances[k]))*/) && Vector3.Dot (LOSDirections [i], LOSDirections [k]) > (dotTemp*0.5f-0.5f)) {
//													obstacleDistributionHasChanged = true;
//												}
//											}
//
//										}
//										if (obstacleDistributionHasChanged) {
//											break;
//										}
										//if (visibilityDistances[(int)newPos.x, (int)newPos.z,(int)(3f+(Vector3.Dot((end-newPos).normalized,Vector3.left)*4f))]>GetManhattanDistance(end,newPos)){
										//	break;
										//}
									}
									//for (int n = 0; n<8; ++n){
									//	Debug.DrawRay(newPos,visibilityDistances[(int)newPos.x, (int)newPos.z,n]*LOSDirections[n],Color.yellow);
									//}
									node = new PathFindingNode (temp, newPos, start, end, ref reachedEnd, useStandardAStar, true);
									if (numUses [(int)newPos.x, (int)newPos.z] < numPathFindingSearches || fValues [(int)newPos.x, (int)newPos.z, useStandardAStar ? 0 : 1] > node.f) {
										numUses [(int)newPos.x, (int)newPos.z] = numPathFindingSearches;
										fValues [(int)newPos.x, (int)newPos.z, useStandardAStar ? 0 : 1] = node.f;
										gValues [(int)newPos.x, (int)newPos.z, useStandardAStar ? 0 : 1] = node.g;

										openList.Add (node);
										++numExpansions;
									}
								}

							}
//							if (openList.Count > 1 && !useStandardAStar && dotTemp>-0.5f && dotTemp2>=-0.5f &&  temp.deltaLosDistance > 1 && visibilityDistances [(int)temp.pos.x, (int)temp.pos.z, i] > (temp.deltaLosDistance * temp.deltaLosDistance)) {
//							float dist = (temp.deltaLosDistance * temp.deltaLosDistance);
//							newPos = temp.pos + SearchDirectionOffsets [i] * dist;
//							if ((int)newPos.x >= 0 && (int)newPos.x < AIGrid.cellCanBeMovedThrough.GetLength (0) && (int)newPos.z >= 0 && (int)newPos.z < AIGrid.cellCanBeMovedThrough.GetLength (1) && AIGrid.cellCanBeMovedThrough [(int)newPos.x, (int)newPos.z]) {
//								foundShortCutExpansion = true;	
//								validPos = true;
//								node = new PathFindingNode (temp, newPos, start, end, ref reachedEnd, useStandardAStar,true);
//								for (float k = 1; k<dist-1; ++k) {
//									fValues [(int)(temp.pos.x + k * SearchDirectionOffsets [i].x), (int)(temp.pos.z + k * SearchDirectionOffsets [i].z), useStandardAStar ? 0 : 1] = (((k - 1) / dist) * node.f) < fValues [(int)(temp.pos.x + k * SearchDirectionOffsets [i].x), (int)(temp.pos.z + k * SearchDirectionOffsets [i].z), useStandardAStar ? 0 : 1] ? (((k - 1) / dist) * node.f) : fValues [(int)(temp.pos.x + k * SearchDirectionOffsets [i].x), (int)(temp.pos.z + k * SearchDirectionOffsets [i].z), useStandardAStar ? 0 : 1];
//									numUses[(int)(temp.pos.x+k*SearchDirectionOffsets[i].x),(int)(temp.pos.z+k*SearchDirectionOffsets[i].z)] = numPathFindingSearches;
//								}
//
//										if (numUses [(int)newPos.x, (int)newPos.z] < numPathFindingSearches || fValues [(int)newPos.x, (int)newPos.z, useStandardAStar ? 0 : 1] > node.f) {
//											numUses [(int)newPos.x, (int)newPos.z] = numPathFindingSearches;
//											fValues [(int)newPos.x, (int)newPos.z, useStandardAStar ? 0 : 1] = node.f;
//											gValues [(int)newPos.x, (int)newPos.z, useStandardAStar ? 0 : 1] = node.g;
//											openList.Add (node);
//											++numExpansions;
//										}

							//}
							//} 

							if (!foundShortCutExpansion) {
								newPos = temp.pos + SearchDirectionOffsets [i];
								if ((int)newPos.x >= 0 && (int)newPos.x < AIGrid.cellCanBeMovedThrough.GetLength (0) && (int)newPos.z >= 0 && (int)newPos.z < AIGrid.cellCanBeMovedThrough.GetLength (1) && AIGrid.cellCanBeMovedThrough [(int)newPos.x, (int)newPos.z]) {
									validPos = true;
									node = new PathFindingNode (temp, newPos, start, end, ref reachedEnd, useStandardAStar, false);
								}

							}
							if (reachedEnd) { //adding to the openlist is expensive, and we've found the end, so we break now
								break;
							}

							consistent = false;
							if (useLPA) {
								float minG = float.PositiveInfinity;
								for (int j = 0; j< SearchDirectionOffsets.Length; ++j) {
									if ((int)(newPos.x + SearchDirectionOffsets [i].x) >= 0 && (int)(newPos.x + SearchDirectionOffsets [i].x) < AIGrid.cellCanBeMovedThrough.GetLength (0) && (int)(newPos.z + SearchDirectionOffsets [i].z) >= 0 && (int)(newPos.z + SearchDirectionOffsets [i].z) < AIGrid.cellCanBeMovedThrough.GetLength (1) && AIGrid.cellCanBeMovedThrough [(int)(newPos.x + SearchDirectionOffsets [i].x), (int)(newPos.z + SearchDirectionOffsets [i].z)]) {
										float tempG = gValues [(int)(temp.pos.x + SearchDirectionOffsets [i].x), (int)(temp.pos.z + SearchDirectionOffsets [i].z), useStandardAStar ? 0 : 1] + SearchDirectionDistances [j];
										minG = minG > tempG ? tempG : minG;
									}
								}
							
								//					if (minG==gValues[(int)temp.pos.x, (int)temp.pos.z,useStandardAStar ? 0 : 1]){
								//						//gValues[(int)temp.pos.x, (int)temp.pos.z,useStandardAStar ? 0 : 1]= minG;
								//						consistent= true;
								//					} else
								if (numIter > 2) {
									if (minG > gValues [(int)temp.pos.x, (int)temp.pos.z, useStandardAStar ? 0 : 1]) {
										gValues [(int)temp.pos.x, (int)temp.pos.z, useStandardAStar ? 0 : 1] = minG;
										consistent = true;
									} else {
										gValues [(int)temp.pos.x, (int)temp.pos.z, useStandardAStar ? 0 : 1] = float.PositiveInfinity / 100;
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
					float endTime = Time.realtimeSinceStartup;
					timeToRun += (endTime-startTime);
					yield return new WaitForSeconds (0.01f);
				}
				Collection<Vector3> path = new Collection<Vector3> ();
				if (temp != null) {
					CreatePath (temp, ref path, start, end, true, useStandardAStar, useLPA, useStandardAStar ? Color.green : Color.magenta, 30f);
				}

				Debug.Log ((useStandardAStar ? "AStar" : "LOS*") + " Expansions: " + numExpansions + " " + reachedEnd + " " + timeToRun);
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

	public static void CreatePath (PathFindingNode node, ref Collection<Vector3> path, Vector3 start, Vector3 end, bool reversePath, bool visualizePath, bool useStandardAStar, Color pathCol = default(Color), float pathDrawTime = 0f)
	{
		//float finalG = node.g;
		PathFindingNode temp = node;
		path.Add (end);
		while (temp.previous!=null) {
//			hMulipliers[(int)(temp.previous.pos.x*hMultiplierArrayResolution),(int)(temp.previous.pos.z*hMultiplierArrayResolution),useStandardAStar?0:1] = PathFindingNode.heuristicMultiplierAdaptionRate*((finalG - temp.previous.g)/temp.previous.h)+ (1-PathFindingNode.heuristicMultiplierAdaptionRate)*hMulipliers[(int)(temp.previous.pos.x*hMultiplierArrayResolution),(int)(temp.previous.pos.z*hMultiplierArrayResolution),useStandardAStar?0:1];
//			if (hMulipliers[(int)(temp.previous.pos.x*hMultiplierArrayResolution),(int)(temp.previous.pos.z*hMultiplierArrayResolution),useStandardAStar?0:1]<0){
//				hMulipliers[(int)(temp.previous.pos.x*hMultiplierArrayResolution),(int)(temp.previous.pos.z*hMultiplierArrayResolution),useStandardAStar?0:1] = 1;
//			}
			//Debug.Log(hMulipliers[(int)(temp.previous.pos.x*hMultiplierArrayResolution),(int)(temp.previous.pos.z*hMultiplierArrayResolution),useStandardAStar?0:1]);
			path.Insert (0, temp.pos);
			if (visualizePath) {
				Debug.DrawRay (temp.pos, temp.previous.pos - temp.pos, pathCol, pathDrawTime);
			}
			temp = temp.previous;
		}

		if (reversePath) {
			//Collection<PathFindingNode> tempP = new Collection<PathFindingNode>();
			Vector3[] tempP = new Vector3[path.Count];
			path.CopyTo(tempP,0);
			path.Clear();
			for (int i = tempP.Length; i<=0; ++i){
				path.Add(tempP[i]);
			}
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

	public static float GetDeltaMax (Vector3 start, Vector3 end, float diagonalCost, float nonDiagonalCost)
	{
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
		//return max_displacement;
		return diagonalCost * (min_displacement) + nonDiagonalCost * (max_displacement - min_displacement);
	}

	public static bool findExplorationPath (object caller, Vector3 start, out Collection<Vector3> path, int numExplorationNodes){
		++numPathFindingSearches;
		path = new Collection<Vector3> ();
		if (start.x >= 0 && start.x < AIGrid.cellCanBeMovedThrough.GetLength (0) && start.z >= 0 && start.z < AIGrid.cellCanBeMovedThrough.GetLength (1) && AIGrid.cellCanBeMovedThrough[(int)start.x,(int)start.z]) {
			Vector3 tempPos = start;
			int nextNode = -1; // index of next node
			double minDifferenceNumberUses = 0; //we need to know the difference between the number of pathfinding uses versus the last time a node was used
			for (int j = 0; j<numExplorationNodes; ++j){
				nextNode = -1;
				minDifferenceNumberUses = 0;
				for (int i = 0; i<8; ++i){
					if ((int)(tempPos.x + SearchDirectionOffsets[i].x) >= 0 && (int)(tempPos.x + SearchDirectionOffsets[i].x) < AIGrid.cellCanBeMovedThrough.GetLength (0) && (int)(tempPos.z + SearchDirectionOffsets[i].z) >= 0 && (int)(tempPos.z + SearchDirectionOffsets[i].z) < AIGrid.cellCanBeMovedThrough.GetLength (1) && AIGrid.cellCanBeMovedThrough[(int)(tempPos.x + SearchDirectionOffsets[i].x),(int)(tempPos.z + SearchDirectionOffsets[i].z)]){
						double tempF = ((numPathFindingSearches - AIGrid.numUses[(int)(tempPos.x + SearchDirectionOffsets[i].x),(int)(tempPos.z + SearchDirectionOffsets[i].z)]));
						tempF = tempF + UnityEngine.Random.Range(0,(float)tempF);
						if (tempF>minDifferenceNumberUses){
							nextNode = i;
						}
					}
				}

				if (nextNode!=-1){

					tempPos = tempPos + SearchDirectionOffsets[nextNode];
					AIGrid.numUses[(int)(tempPos.x),(int)(tempPos.z)] = numPathFindingSearches;
					path.Insert(0,tempPos);
				} else {
					return false;
				}
			}

			return true;
		} else {
			return false;
		}
	}

	public static bool findExplorationPoint (object caller, Vector3 start, out Vector3 end){
		++numPathFindingSearches;
		//path = new Collection<Vector3> ();
		end = new Vector3(Random.Range(0,cellCanBeMovedThrough.GetLength(0)-1),0,Random.Range(0,cellCanBeMovedThrough.GetLength(1)-1));
		if (start.x >= 0 && start.x < AIGrid.cellCanBeMovedThrough.GetLength (0) && start.z >= 0 && start.z < AIGrid.cellCanBeMovedThrough.GetLength (1) && AIGrid.cellCanBeMovedThrough[(int)start.x,(int)start.z]) {
			//Vector3 tempPos = start;
			int nextNode = -1; // index of next node
			double minDifferenceNumberUses = 0; //we need to know the difference between the number of pathfinding uses versus the last time a node was used

			while (end.x >= 0 && end.x < AIGrid.cellCanBeMovedThrough.GetLength (0) && end.z >= 0 && end.z < AIGrid.cellCanBeMovedThrough.GetLength (1) && !AIGrid.cellCanBeMovedThrough[(int)end.x,(int)end.z]) {
				end.x = Random.Range(0,cellCanBeMovedThrough.GetLength(0)-1);
				end.z = Random.Range(0,cellCanBeMovedThrough.GetLength(1)-1);
				AIGrid.numUses[(int)end.x,(int)end.z] = UnityEngine.Random.Range(0f,1f);
			}
			//}
			
			return true;
		} else {
			return false;
		}
	}

	public static bool findFleeingPath(object caller, Vector3 start, out Collection<Vector3> path){
		path = new Collection<Vector3> ();
		if (start.x >= 0 && start.x < AIGrid.cellCanBeMovedThrough.GetLength (0) && start.z >= 0 && start.z < AIGrid.cellCanBeMovedThrough.GetLength (1) && AIGrid.cellCanBeMovedThrough [(int)start.x, (int)start.z]) {
			float dist = -1;
			int cornerIndex = -1;
			for (int i = 0; i<corners.Length; ++i) {
				float dist_temp = Vector3.Distance (corners [i], start);
				if (dist_temp > dist) {
					dist = dist_temp;
					cornerIndex = i;
				}
			}

			Vector3 end = corners [cornerIndex];

			while (end.x >= 0 && end.x < AIGrid.cellCanBeMovedThrough.GetLength (0) && end.z >= 0 && end.z < AIGrid.cellCanBeMovedThrough.GetLength (1) && !AIGrid.cellCanBeMovedThrough [(int)end.x, (int)end.z]) {
				end.x += directionToSearchForNewCorner [cornerIndex].x;
				end.z += directionToSearchForNewCorner [cornerIndex].z;
			}
			return findPath (caller, start, end, out path, false, true);
		} else {
			return false;
		}
	}

	public static bool findFleeingPoint(object caller, Vector3 start, out Vector3 end){
		end = Vector3.zero;
		if (start.x >= 0 && start.x < AIGrid.cellCanBeMovedThrough.GetLength (0) && start.z >= 0 && start.z < AIGrid.cellCanBeMovedThrough.GetLength (1) && AIGrid.cellCanBeMovedThrough [(int)start.x, (int)start.z]) {
			float dist = -1;
			int cornerIndex = -1;
			for (int i = 0; i<corners.Length; ++i){
				float dist_temp = Vector3.Distance(corners[i],start); //dot product makes us prefer points away from the point we are fleeing from
				if (dist_temp>dist){
					dist = dist_temp;
					cornerIndex = i;
				}
			}
			
			end = corners[cornerIndex];
			
			while (!AIGrid.cellCanBeMovedThrough [(int)end.x, (int)end.z]){
				
				end.x += directionToSearchForNewCorner[cornerIndex].x;
				end.z += directionToSearchForNewCorner[cornerIndex].z;
				
				if (!(end.x >= 0 && end.x < AIGrid.cellCanBeMovedThrough.GetLength (0) && end.z >= 0 && end.z < AIGrid.cellCanBeMovedThrough.GetLength (1))){
					dist = -1;
					cornerIndex = -1;
					for (int i = 0; i<corners.Length; ++i){
						float dist_temp = Vector3.Distance(corners[i],start);
						if (dist_temp>dist){
							dist = dist_temp;
							cornerIndex = i;
						}
					}
				}
			}
			Debug.DrawRay(end, Vector3.up,Color.red,30f);
			return AIGrid.cellCanBeMovedThrough [(int)end.x, (int)end.z];
		} else {
			return false;
		}
	}

	public static bool findFleeingPoint(object caller, Vector3 start, Vector3 pointToFleeFrom, out Vector3 end){
		end = Vector3.zero;
		if (start.x >= 0 && start.x < AIGrid.cellCanBeMovedThrough.GetLength (0) && start.z >= 0 && start.z < AIGrid.cellCanBeMovedThrough.GetLength (1) && AIGrid.cellCanBeMovedThrough [(int)start.x, (int)start.z]) {
			float dist = -1;
			int cornerIndex = -1;
			for (int i = 0; i<corners.Length; ++i){
				float dist_temp = Vector3.Dot (corners[i]-start,corners[i]-pointToFleeFrom)*Vector3.Distance(corners[i],start); //dot product makes us prefer points away from the point we are fleeing from
				if (dist_temp>dist){
					dist = dist_temp;
					cornerIndex = i;
				}
			}
			
			end = corners[cornerIndex];
			
			while (!AIGrid.cellCanBeMovedThrough [(int)end.x, (int)end.z]){

				end.x += directionToSearchForNewCorner[cornerIndex].x;
				end.z += directionToSearchForNewCorner[cornerIndex].z;

				if (!(end.x >= 0 && end.x < AIGrid.cellCanBeMovedThrough.GetLength (0) && end.z >= 0 && end.z < AIGrid.cellCanBeMovedThrough.GetLength (1))){
					dist = -1;
					cornerIndex = -1;
					for (int i = 0; i<corners.Length; ++i){
						float dist_temp = Vector3.Distance(corners[i],start);
						if (dist_temp>dist){
							dist = dist_temp;
							cornerIndex = i;
						}
					}
				}
			}
			Debug.DrawRay(end, Vector3.up,Color.red,30f);
			return AIGrid.cellCanBeMovedThrough [(int)end.x, (int)end.z];
		} else {
			return false;
		}
	}

	/*for performance tests*/
//	public static bool findPath<ClosedListType> (Vector3 start, Vector3 end, out Collection<Vector3> path, bool useStandardAStar = true)
//	{
//		path = new Collection<Vector3>();
//		++AIGrid.numPathFindingSearches;
//		float timeToRun = 0;
//		
//		debugSearchActive = true;
//		++AIGrid.numPathFindingSearches;
//		//path = new Collection<Vector3> ();
//		//first check that start and end are within bounds
//		bool keepSearching = true;
//		if (start.x >= 0 && start.x < AIGrid.cellCanBeMovedThrough.GetLength (0) && start.z >= 0 && start.z < AIGrid.cellCanBeMovedThrough.GetLength (1)) {
//			if (end.x >= 0 && end.x < AIGrid.cellCanBeMovedThrough.GetLength (0) && end.z >= 0 && end.z < AIGrid.cellCanBeMovedThrough.GetLength (1)) {
//				if (!AIGrid.cellCanBeMovedThrough [(int)end.x, (int)end.z]) {
//					//Debug.Log ("Cannot move to destination cell");
//					return false;
//				}
//
//				
//				
//				NodeList<PathFindingNode> openList = new NodeList<PathFindingNode> ();
//				bool reachedEnd = false;
//				openList.Add (new PathFindingNode (null, start, start, end, ref reachedEnd, useStandardAStar, false));
//				
//				int numIter = 0;
//				int maxNumIter = 100;
//				int numExpansions = 0;
//				PathFindingNode temp = openList [0];
//				gValues [(int)(temp.pos.x), (int)(temp.pos.z), useStandardAStar ? 0 : 1] = 0;
//				while (openList.Count>0 && !reachedEnd) {
//					float startTime = Time.realtimeSinceStartup;
//					++numIter;
//					if (numIter > maxNumIter) {
//						Debug.Log ("NMMT");
//						break;
//					}
//					temp = openList [0];
//					
//					bool consistent = false;
//					
//					Vector3 newPos = temp.pos;
//
//			
//
//					for (int i = 0; i<8; ++i){
//							if ((int)newPos.x == (int)end.x && (int)newPos.z == (int)end.z) {
//						CreatePath (temp, ref path, start, end, true, useStandardAStar, false, useStandardAStar ? Color.green : Color.magenta, 30f);
//								reachedEnd = true;
//						return true;
//							}
//							
//							//if ((int)newPos.x == (int)end.x && (int)newPos.z == (int)end.z) {
//							bool validPos = false;
//							//if ((int)newPos.x >= 0 && (int)newPos.x < AIGrid.cellCanBeMovedThrough.GetLength (0) && (int)newPos.z >= 0 && (int)newPos.z < AIGrid.cellCanBeMovedThrough.GetLength (1) && AIGrid.cellCanBeMovedThrough [(int)newPos.x, (int)newPos.z]) {
//							PathFindingNode node = null;
//							//jump search when using LOS*. We attempt to exponentially increase the jump distance, to reduce the expansion amount
//
//							
//							
//								newPos = temp.pos + SearchDirectionOffsets [i];
//								if ((int)newPos.x >= 0 && (int)newPos.x < AIGrid.cellCanBeMovedThrough.GetLength (0) && (int)newPos.z >= 0 && (int)newPos.z < AIGrid.cellCanBeMovedThrough.GetLength (1) && AIGrid.cellCanBeMovedThrough [(int)newPos.x, (int)newPos.z]) {
//									validPos = true;
//									node = new PathFindingNode (temp, newPos, start, end, ref reachedEnd, useStandardAStar, false);
//								}
//								
//
////							if (reachedEnd) { //adding to the openlist is expensive, and we've found the end, so we break now
////								break;
////							}
//							
//								if (numUses [(int)newPos.x, (int)newPos.z] < numPathFindingSearches || fValues [(int)newPos.x, (int)newPos.z, useStandardAStar ? 0 : 1] > node.f) {
//									numUses [(int)newPos.x, (int)newPos.z] = numPathFindingSearches;
//									fValues [(int)newPos.x, (int)newPos.z, useStandardAStar ? 0 : 1] = node.f;
//									gValues [(int)newPos.x, (int)newPos.z, useStandardAStar ? 0 : 1] = node.g;
//									openList.Add (node);
//									++numExpansions;
//								}
//
//					}
//
//					openList.Remove (temp);
//					float endTime = Time.realtimeSinceStartup;
//					timeToRun += (endTime-startTime);
//					//yield return new WaitForSeconds (0.01f);
//				}
//				//path = new Collection<Vector3> ();
//				
//				////if (temp != null) {
//				//	CreatePath (temp, ref path, start, end, true, useStandardAStar, false, useStandardAStar ? Color.green : Color.magenta, 30f);
//				//}
//				//Debug.Log ((useStandardAStar ? "AStar" : "LOS*") + " Expansions: " + numExpansions + " " + reachedEnd + " " + timeToRun);
//				Debug.Log ("OpenList empty");
//				return false;
//			} else {
//				Debug.Log ("End out of bounds");
//				return false;
//			}
//		} else {
//			Debug.Log ("Start out of bounds");
//			return false;
//		}
//		debugSearchActive = false;
//		//MonoBehaviour.StartCoroutine (start, end, !useStandardAStar);
//	}

}
