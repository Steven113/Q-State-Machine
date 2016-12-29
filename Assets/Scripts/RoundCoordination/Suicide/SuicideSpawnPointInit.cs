using UnityEngine;
using System.Collections;

public class SuicideSpawnPointInit : MonoBehaviour {

	// Use this for initialization
	void Start () {
		SuicideSpawner.suicideSpawns.Add (gameObject.transform.position);
	}
	
	// Update is called once per frame
	void OnDestroy () {
		SuicideSpawner.suicideSpawns.Remove (gameObject.transform.position);
	}
}
