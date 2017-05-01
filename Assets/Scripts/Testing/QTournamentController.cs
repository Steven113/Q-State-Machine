using System;
using UnityEngine;

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

		int soldierUnderConsideration_i;
		int soldierUnderConsideration_j;

		public static GameObject [] g_SpawnPoints;

		public void Start(){
			//graphController.Evolve ();

			g_SpawnPoints = spawnPoints;

			for (int i = 0; i < soldiers.Length; ++i) {
				soldiers [(i) % graphController.numGraphs].Graph = graphController.Graphs [(i + soldierUnderConsideration_i)% graphController.numGraphs];
			}
		}

		public void Update(){
			timeSinceRoundStart += Time.deltaTime;

			if (timeSinceRoundStart > roundLength) {
				timeSinceRoundStart = 0;


				if (soldierUnderConsideration_i >= graphController.numGraphs) {
					graphController.Evolve ();

					Debug.Log ("Evolving.");

					soldierUnderConsideration_i = 0;

					spawnPoints = Utils.ShuffleArray (spawnPoints);
				}

				for (int i = 0; i < dummySoldiers.Length; ++i) {
					dummySoldiers [i].Graph.ResetCurrentNodeToRoot ();
					QSoldier qs = dummySoldiers[i].gameObject.GetComponent<QSoldier> ();
					qs.agent.Warp(spawnPoints [i % spawnPoints.Length].transform.position);
				}

				for (int i = 0; i < soldiers.Length; ++i) {
					soldiers [i].Graph.ResetCurrentNodeToRoot ();
					QSoldier qs = soldiers [i].gameObject.GetComponent<QSoldier> ();
					qs.agent.Warp(spawnPoints [(i+dummySoldiers.Length) % spawnPoints.Length].transform.position);
					qs.CurrentTarget = null;
					ControlHealth hc = soldiers [i].gameObject.GetComponent<ControlHealth> ();
					hc.health = hc.maxhealth;
				}

				Debug.Log (GameData.scores [0] + " " + GameData.scores [1]);

				GameData.scores = new float[]{ 0, 0 };

				for (int i = 0; i < soldiers.Length; ++i) {
					if (soldierUnderConsideration_j >= graphController.numGraphs) {
						soldierUnderConsideration_j = 0;
						++soldierUnderConsideration_i;
					}

					soldiers [i].Graph = graphController.Graphs [(soldierUnderConsideration_i * graphController.numGraphs + soldierUnderConsideration_j) % graphController.numGraphs];
					++soldierUnderConsideration_j;
				}




				//for (int i = 0; i < soldiers.Length; ++i) {
				//	soldiers [(i) % graphController.numGraphs].Graph = graphController.Graphs [(i + soldierUnderConsideration_i)% graphController.numGraphs];
				//}
			}
		}
	}
}

