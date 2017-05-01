using System;
using UnityEngine;
using System.Collections.Generic;

namespace AssemblyCSharp
{
	/*
	 * controls the rounds that the ai plays against each other.
	 */
	public class QTournamentController : MonoBehaviour
	{
		public QGraphAgent[] soldiers;

		public QGraphAgent[] dummySoldiers;

		public float roundLength = 120; //round length in seconds

		public float timeSinceRoundStart = 0;

		public GameObject [] spawnPoints;

		public QGraphController graphController;

		//int soldierUnderConsideration_i;
		[SerializeField]int soldierUnderConsideration_j = 1;

		public static GameObject [] g_SpawnPoints;

		public void Start(){
			//graphController.Evolve ();
			Debug.Log("Num combos: "+Mathf.Pow(graphController.numGraphs,soldiers.Length));
			g_SpawnPoints = spawnPoints;

			for (int i = 0; i < soldiers.Length; ++i) {
				soldiers [(i) % graphController.numGraphs].Graph = graphController.Graphs [(i)% graphController.numGraphs];
			}
		}

		Dictionary<QGraph, int> numAssessments = new Dictionary<QGraph, int>();//for debug

		public void Update(){
			timeSinceRoundStart += Time.deltaTime;

			if (timeSinceRoundStart > roundLength) {
				timeSinceRoundStart = 0;


				if (soldierUnderConsideration_j >= ((soldiers.Length>1f)?(Mathf.Pow(graphController.numGraphs,soldiers.Length)):((float)graphController.numGraphs))) {

					for (int i = 0; i < graphController.numGraphs; ++i) {
						Debug.Assert (numAssessments [graphController.Graphs [i]] == numAssessments [graphController.Graphs [(i + 1) % graphController.numGraphs]], "Graph " + i+ " tested "+numAssessments [graphController.Graphs [i]] + " times, graph " + ((i + 1) % graphController.numGraphs) + " tested "+numAssessments [graphController.Graphs [(i + 1) % graphController.numGraphs]]+ " times." );
					}

					graphController.Evolve ();

					Debug.Log ("Evolving.");

					numAssessments.Clear ();

					soldierUnderConsideration_j = 0;

					spawnPoints = Utils.ShuffleArray (spawnPoints);
				}

				spawnPoints = Utils.ShuffleArray (spawnPoints);

				for (int i = 0; i < dummySoldiers.Length; ++i) {
					dummySoldiers [i].Graph.ResetCurrentNodeToRoot ();
					QSoldier qs = dummySoldiers[i].gameObject.GetComponent<QSoldier> ();
					qs.agent.Warp(spawnPoints [i % spawnPoints.Length].transform.position);
				}

				for (int i = 0; i < soldiers.Length; ++i) {
					
					QSoldier qs = soldiers [i].gameObject.GetComponent<QSoldier> ();
					qs.agent.Warp(spawnPoints [(i+dummySoldiers.Length) % spawnPoints.Length].transform.position);
					qs.CurrentTarget = null;
					ControlHealth hc = soldiers [i].gameObject.GetComponent<ControlHealth> ();
					hc.health = hc.maxhealth;
				}

				Debug.Log (GameData.scores [0] + " " + GameData.scores [1]);

				GameData.scores = new float[]{ 0, 0 };

				for (int i = 0; i < soldiers.Length; ++i) {
					int selectedGraph = (int)((soldierUnderConsideration_j / Mathf.Pow (graphController.numGraphs, i))) % graphController.numGraphs;
					soldiers [i].Graph = graphController.Graphs [selectedGraph];
					if (!numAssessments.ContainsKey(graphController.Graphs[selectedGraph])){
						numAssessments.Add(graphController.Graphs[selectedGraph],1);
					} else {
						numAssessments[graphController.Graphs[selectedGraph]]+=1;
					}

					soldiers [i].Graph.ResetCurrentNodeToRoot ();

					Debug.Log ("Soldier " + i + " given graph " + selectedGraph);

				}


				++soldierUnderConsideration_j;

				//for (int i = 0; i < soldiers.Length; ++i) {
				//	soldiers [(i) % graphController.numGraphs].Graph = graphController.Graphs [(i + soldierUnderConsideration_i)% graphController.numGraphs];
				//}
			}
		}
	}
}

