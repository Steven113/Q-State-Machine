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

	public static Vector3 worldCentre = Vector3.zero;
	public static bool[,] cellCanBeMovedThrough = new bool[0, 0];
	public static float[,,] visibilityDistances = new float[0, 0, 0];
	public static float[,] fValues = new float[0, 0];
	public static double[,] numUses = new double[0, 0];
	public static int s_cellWidth = 0;
	public int cellWidth = 1;
	public static double numPathFindingSearches = 0;
	// Use this for initialization
	void Awake ()
	{
		s_cellWidth = cellWidth;
		Debug.Assert (s_cellWidth > 0);
		LOSDirections = new Vector3[] {
			Vector3.left,
			0.5f * (Vector3.left + Vector3.forward),
			Vector3.forward,
			0.5f * (Vector3.right + Vector3.forward),
			Vector3.right,
			0.5f * (Vector3.right + Vector3.back),
			Vector3.back,
			0.5f * (Vector3.left + Vector3.back)
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
		cellCanBeMovedThrough = new bool[worldWidth, worldHeight];
		visibilityDistances = new float[worldWidth, worldHeight, 8];
		fValues = new float[worldWidth, worldHeight];
		for (int i = 0; i<worldWidth; ++i) {
			for (int j = 0; j<worldHeight; ++j) {
				fValues [i, j] = float.PositiveInfinity;
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
						Vector3 losPos = new Vector3(i,0,j);
						losPos += LOSDirections[k];
						while (((int)losPos.x>=0 && (int)losPos.x<AIGrid.cellCanBeMovedThrough.GetLength(0) && (int)losPos.z>=0 && (int)losPos.z<AIGrid.cellCanBeMovedThrough.GetLength(1)) && AIGrid.cellCanBeMovedThrough[(int)losPos.x,(int)losPos.z]){
							visibilityDistances [i, j, k]+=LOSDirections[k].magnitude;
							losPos += LOSDirections[k];
						}
						visibilityDistances [i, j, k] = visibilityDistances [i, j, k]>0?visibilityDistances [i, j, k]:cellWidth;
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
					Debug.Log("Cannot move to destination cell");
					return false;
				}
				for (int i = 0; i<cellCanBeMovedThrough.GetLength(0); ++i) {
					for (int j = 0; j<cellCanBeMovedThrough.GetLength(1); ++j) {
							
							if (cellCanBeMovedThrough [i, j]) {
							Debug.DrawRay(new Vector3(i*s_cellWidth,0,j*s_cellWidth),Vector3.forward*s_cellWidth,Color.green);
							Debug.DrawRay(new Vector3(i*s_cellWidth,0,j*s_cellWidth),Vector3.right*s_cellWidth,Color.green);
						}

					}
				}


				NodeList<PathFindingNode> openList = new NodeList<PathFindingNode> ();
				bool reachedEnd = false;
				openList.Add (new PathFindingNode (null, start, start, end, ref reachedEnd,useStandardAStar));

				int numIter = 0;
				int maxNumIter = cellCanBeMovedThrough.GetLength (0) * cellCanBeMovedThrough.GetLength (1);
				while (openList.Count>0 && !reachedEnd) {
					++numIter;
					if (numIter > maxNumIter){
						Debug.Log("Num iterations max exceeded");
						break;
					}
					PathFindingNode temp = openList [0];
					for (int i = 0; i< SearchDirectionOffsets.Length; ++i) {
						Vector3 newPos = temp.pos + SearchDirectionOffsets [i];
						if ((int)newPos.x == (int)end.x && (int)newPos.z == (int)end.z) return true;
						if ((int)newPos.x >= 0 && (int)newPos.x < AIGrid.cellCanBeMovedThrough.GetLength (0) && (int)newPos.z >= 0 && (int)newPos.z < AIGrid.cellCanBeMovedThrough.GetLength (1) && AIGrid.cellCanBeMovedThrough [(int)newPos.x, (int)newPos.z]) {
							PathFindingNode node = new PathFindingNode (temp, newPos, start, end, ref reachedEnd,useStandardAStar);
							if (numUses [(int)newPos.x, (int)newPos.z] < numPathFindingSearches || fValues [(int)newPos.x, (int)newPos.z] > node.f) {
								numUses [(int)newPos.x, (int)newPos.z] = numPathFindingSearches;
								fValues [(int)newPos.x, (int)newPos.z] = node.f;
								openList.Add (node);
							}
						}
					}
					openList.Remove(temp);
				}
				Debug.Log("OpenList empty");
				return false;
			} else {
				Debug.Log("End out of bounds");
				return false;
			}
		} else {
			Debug.Log("Start out of bounds");
			return false;
		}
	}

	public static IEnumerator findPathCoroutine (Vector3 start, Vector3 end,bool useStandardAStar = false)
	{
		++AIGrid.numPathFindingSearches;
		//path = new Collection<Vector3> ();
		//first check that start and end are within bounds
		bool keepSearching = true;
		if (start.x >= 0 && start.x < AIGrid.cellCanBeMovedThrough.GetLength (0) && start.z >= 0 && start.z < AIGrid.cellCanBeMovedThrough.GetLength (1)) {
			if (end.x >= 0 && end.x < AIGrid.cellCanBeMovedThrough.GetLength (0) && end.z >= 0 && end.z < AIGrid.cellCanBeMovedThrough.GetLength (1)) {
				if (!AIGrid.cellCanBeMovedThrough [(int)end.x, (int)end.z]) {
					Debug.Log("Cannot move to destination cell");
					//return;
				}
				for (int i = 0; i<cellCanBeMovedThrough.GetLength(0); ++i) {
					for (int j = 0; j<cellCanBeMovedThrough.GetLength(1); ++j) {
						
						if (cellCanBeMovedThrough [i, j]) {
							Debug.DrawRay(new Vector3(i*s_cellWidth,0,j*s_cellWidth),Vector3.forward*s_cellWidth,Color.green,100f);
							Debug.DrawRay(new Vector3(i*s_cellWidth,0,j*s_cellWidth),Vector3.right*s_cellWidth,Color.green,100f);
						}
						
					}
				}
				
				
				NodeList<PathFindingNode> openList = new NodeList<PathFindingNode> ();
				bool reachedEnd = false;
				openList.Add (new PathFindingNode (null, start, start, end, ref reachedEnd,useStandardAStar));
				
				int numIter = 0;
				int maxNumIter = cellCanBeMovedThrough.GetLength (0) * cellCanBeMovedThrough.GetLength (1);
				int numExpansions = 0;
				PathFindingNode temp = openList [0];
				while (openList.Count>0 && !reachedEnd) {
					++numIter;
					if (numIter > maxNumIter){
						Debug.Log("Num iterations max exceeded");
						break;
					}
					temp = openList [0];
					for (int i = 0; i< SearchDirectionOffsets.Length; ++i) {
						Vector3 newPos = temp.pos + SearchDirectionOffsets [i];
						//Debug.Log("Exploring "+newPos + (useStandardAStar?"AStar":"LOS*"));
						if ((int)newPos.x == (int)end.x && (int)newPos.z == (int)end.z) {reachedEnd=true; break;}
						if ((int)newPos.x >= 0 && (int)newPos.x < AIGrid.cellCanBeMovedThrough.GetLength (0) && (int)newPos.z >= 0 && (int)newPos.z < AIGrid.cellCanBeMovedThrough.GetLength (1) && AIGrid.cellCanBeMovedThrough [(int)newPos.x, (int)newPos.z]) {
							PathFindingNode node = new PathFindingNode (temp, newPos, start, end, ref reachedEnd,useStandardAStar);
							if (reachedEnd){ //adding to the openlist is expensive, and we've found the end, so we break now
								break;
							}
							if (numUses [(int)newPos.x, (int)newPos.z] < numPathFindingSearches || fValues [(int)newPos.x, (int)newPos.z] > node.f) {
								numUses [(int)newPos.x, (int)newPos.z] = numPathFindingSearches;
								fValues [(int)newPos.x, (int)newPos.z] = node.f;
								openList.Add (node);
								++numExpansions;
							}
						}
					}
					openList.Remove(temp);
					yield return new WaitForSeconds(0.1f);
				}
				Collection<Vector3> path = new Collection<Vector3>();
				if (temp!=null){
					CreatePath(temp,ref path,start, end,true, useStandardAStar?Color.green:Color.magenta,30f);
				}
				Debug.Log((useStandardAStar?"AStar":"LOS*")+" Expansions: "+numExpansions + " " + reachedEnd);
				Debug.Log("OpenList empty");
				//return false;
			} else {
				Debug.Log("End out of bounds");
				//return false;
			}
		} else {
			Debug.Log("Start out of bounds");
			//return false;
		}
	}

	public static void CreatePath(PathFindingNode node, ref Collection<Vector3> path,Vector3 start, Vector3 end, bool visualizePath, Color pathCol = default(Color), float pathDrawTime = 0f){
		PathFindingNode temp = node;
		path.Add (end);
		while (temp.previous!=null) {
			path.Insert(0,temp.pos);
			if (visualizePath){
				Debug.DrawRay(temp.pos, temp.previous.pos-temp.pos,pathCol, pathDrawTime);
			}
			temp = temp.previous;
		}
	}

	public static float GetManhattanDistance(Vector3 start, Vector3 end){
		return Mathf.Abs (start.x - end.x) + Mathf.Abs (start.y - end.y) + Mathf.Abs (start.z - end.z);
	}
}
