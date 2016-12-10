using UnityEngine;
using System.Collections;
using System.Collections.ObjectModel;

public class QAgent : MonoBehaviour {

	public string qFileName;
	// Use this for initialization
	void Start () {
		Collection<Vector3> path = new Collection<Vector3> ();
		Vector3 end = new Vector3 (UnityEngine.Random.Range (0, AIGrid.cellCanBeMovedThrough.GetLength (0)), 0, UnityEngine.Random.Range (0, AIGrid.cellCanBeMovedThrough.GetLength (1)));;
		while (!AIGrid.cellCanBeMovedThrough[(int)end.x,(int)end.z]) {
			end = new Vector3 (UnityEngine.Random.Range (0, AIGrid.cellCanBeMovedThrough.GetLength (0)), 0, UnityEngine.Random.Range (0, AIGrid.cellCanBeMovedThrough.GetLength (1)));
		}
		Debug.DrawRay (end, Vector3.up * 40, Color.blue,500f);
		StartCoroutine(AIGrid.findPathCoroutine(gameObject.transform.position,end,true));
		StartCoroutine(AIGrid.findPathCoroutine(gameObject.transform.position,end,false));
	}
	
	// Update is called once per frame
	void Update () {

	}
}
