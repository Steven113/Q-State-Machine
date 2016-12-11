using UnityEngine;
using System.Collections;
using System.Collections.ObjectModel;

public class QAgent : MonoBehaviour {

	public string qFileName;
	// Use this for initialization
	public bool useStandarAStar = true;
	Vector3 end = Vector3.zero;

	void Start () {


		//StartCoroutine(AIGrid.findPathCoroutine(gameObject.transform.position,end,false));
	}
	
	// Update is called once per frame
	void Update () {
		if (useStandarAStar && !AIGrid.debugSearchActive) {
			end = new Vector3 (UnityEngine.Random.Range (0, AIGrid.cellCanBeMovedThrough.GetLength (0)), 0, UnityEngine.Random.Range (0, AIGrid.cellCanBeMovedThrough.GetLength (1)));;
			while (!AIGrid.cellCanBeMovedThrough[(int)end.x,(int)end.z]) {
				end = new Vector3 (UnityEngine.Random.Range (0, AIGrid.cellCanBeMovedThrough.GetLength (0)), 0, UnityEngine.Random.Range ((9*AIGrid.cellCanBeMovedThrough.GetLength (1))/10, AIGrid.cellCanBeMovedThrough.GetLength (1)));
			}
		}
		Debug.DrawRay (end, Vector3.up * 40, Color.blue);
		if (!AIGrid.debugSearchActive) {
			Collection<Vector3> path = new Collection<Vector3> ();
			AIGrid.debugSearchActive = true;

			StartCoroutine(AIGrid.findPathCoroutine(gameObject.transform.position,end,useStandarAStar));
			useStandarAStar = !useStandarAStar;
		}
	}
}
