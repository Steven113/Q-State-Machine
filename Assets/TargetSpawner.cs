using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AssemblyCSharp;

public class TargetSpawner : MonoBehaviour {

	public static List<Vector3> spawnPoints = new List<Vector3>();

	public GameObject targetPrefab;

	public int numTargetsAtATime = 5;

	public int numSpawns = 0;

	// Use this for initialization
	void Start () {
		spawnPoints = Utils.ShuffleList (spawnPoints);
	}
	
	// Update is called once per frame
	void Update () {
		if (Target.numTargets<numTargetsAtATime) {
			++numSpawns;
			if (numSpawns>=spawnPoints.Count){
				numSpawns = 0;
				spawnPoints = Utils.ShuffleList (spawnPoints);
			}

			GameObject.Instantiate(targetPrefab,spawnPoints[numSpawns],Quaternion.identity);

		}
	}
}
