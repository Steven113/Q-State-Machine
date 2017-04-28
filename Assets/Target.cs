using UnityEngine;
using System.Collections;

public class Target : MonoBehaviour {

	public static int numTargets;

	// Use this for initialization
	void Start () {
		++numTargets;
	}
	
	// Update is called once per frame
	void OnDestroy () {
		--numTargets;
	}
}
