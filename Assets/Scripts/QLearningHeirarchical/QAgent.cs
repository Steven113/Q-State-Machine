using UnityEngine;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using AssemblyCSharp;

public abstract class QAgent : MonoBehaviour {

	public string qFileName; // file describing initial agent
	// Use this for initialization
	//public bool useStandarAStar = true;
	//Vector3 end = Vector3.zero;

	//get the action(s) a agent should perform given it's current state
	public abstract List<string> GetAction (List<string> state, List<float> variables); // we use the list of strings as a return parameter as we want to facilitate concept-action-mapping

	//give the agent a reward instantly. The agent will add a reduced amount of the given reward value for the reward for it's given state/action pair
	public abstract bool RewardAgent (float reward); 

	public QSensor stateDetector;

	public abstract void Reset();
	//void Start () {


		//StartCoroutine(AIGrid.findPathCoroutine(gameObject.transform.position,end,false));
	//}
	
	// Update is called once per frame
	//void Update () {
//		if (useStandarAStar && !AIGrid.debugSearchActive) {
//			end = new Vector3 (UnityEngine.Random.Range (0, AIGrid.cellCanBeMovedThrough.GetLength (0)), 0, UnityEngine.Random.Range (0, AIGrid.cellCanBeMovedThrough.GetLength (1)));;
//			while (!AIGrid.cellCanBeMovedThrough[(int)end.x,(int)end.z]) {
//				end = new Vector3 (UnityEngine.Random.Range (0, AIGrid.cellCanBeMovedThrough.GetLength (0)), 0, UnityEngine.Random.Range ((9*AIGrid.cellCanBeMovedThrough.GetLength (1))/10, AIGrid.cellCanBeMovedThrough.GetLength (1)));
//			}
//		}
//		Debug.DrawRay (end, Vector3.up * 40, Color.blue);
//		if (!AIGrid.debugSearchActive) {
//			Collection<Vector3> path = new Collection<Vector3> ();
//			AIGrid.debugSearchActive = true;
//
//			//if (useStandarAStar){
//			StartCoroutine(AIGrid.findPathCoroutine(gameObject.transform.position,end,useStandarAStar,useStandarAStar));
//			//} else {
//				//StartCoroutine(AIGrid.findPathCoroutine(gameObject.transform.position,end,useStandarAStar));
//			//}
//
//			//StartCoroutine(AIGrid.findPathCoroutine(gameObject.transform.position,end,useStandarAStar));
//			useStandarAStar = !useStandarAStar;
//		}
	//}
}
