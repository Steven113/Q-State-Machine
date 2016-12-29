using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SuicideSpawner : MonoBehaviour {

	public GameObject suicideDronePrefab;

	public static List<Vector3> suicideSpawns = new List<Vector3>();

	public static int numSuicideDronesInPlay = 0;

	public int numSuicideDronesAtATime = 3;



	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (numSuicideDronesInPlay < numSuicideDronesAtATime) {
			int spawnToUse = UnityEngine.Random.Range(0,suicideSpawns.Count-1);
			GameObject.Instantiate(suicideDronePrefab,suicideSpawns[spawnToUse],Quaternion.identity);
		}
	}
}
