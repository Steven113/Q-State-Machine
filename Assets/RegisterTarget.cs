using UnityEngine;
using System.Collections;

public class RegisterTarget : MonoBehaviour {

	// Use this for initialization
	void Start () {
		TargetSpawner.spawnPoints.Add (gameObject.transform.position);
	}
	
	// Update is called once per frame
	void OnDestroy () {
		TargetSpawner.spawnPoints.Remove (gameObject.transform.position);
	}
}
