using UnityEngine;
using System.Collections;

public class DestroyAfterDelay : MonoBehaviour {

	public float delay;
	public float timeSinceSpawn = 0;
	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		timeSinceSpawn += Time.deltaTime;
		if (timeSinceSpawn > delay) {
			GameObject.Destroy(gameObject);
		}
	}
}
