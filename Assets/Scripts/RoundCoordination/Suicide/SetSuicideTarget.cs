using UnityEngine;
using System.Collections;

public class SetSuicideTarget : MonoBehaviour {

	// Use this for initialization
	void Start () {
		SuicideDroneController.targetPos = gameObject.transform.position;

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
