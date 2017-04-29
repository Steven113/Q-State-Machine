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

		public float roundLength = 120; //round length in seconds

		public float timeSinceRoundStart = 0;

		public GameObject [] spawnPoints;

		public QGraphController graphController;

		int soldierUnderConsideration;

		public static GameObject [] g_SpawnPoints;

		public void Start(){
			//graphController.Evolve ();

			g_SpawnPoints = spawnPoints;

			for (int i = 0; i < soldiers.Length; ++i) {
				soldiers [(i) % graphController.numGraphs].Graph = graphController.Graphs [(i + soldierUnderConsideration)% graphController.numGraphs];
			}
		}

		public void Update(){
			timeSinceRoundStart += Time.deltaTime;

			if (timeSinceRoundStart > roundLength) {
				timeSinceRoundStart = 0;
				++soldierUnderConsideration;

				for (int i = 0; i < soldiers.Length; ++i) {
					QSoldier qs = soldiers [i].gameObject.GetComponent<QSoldier> ();
					qs.agent.Warp(spawnPoints [i % spawnPoints.Length].transform.position);
					qs.CurrentTarget = null;
					ControlHealth hc = soldiers [i].gameObject.GetComponent<ControlHealth> ();
					hc.health = hc.maxhealth;
				}

				Debug.Log (GameData.scores [0] + " " + GameData.scores [1]);

				GameData.scores = new float[]{ 0, 0 };

				if (soldierUnderConsideration > graphController.numGraphs) {
					graphController.Evolve ();

					Debug.Log ("Evolving.");

					soldierUnderConsideration = 0;

					spawnPoints = Utils.ShuffleArray (spawnPoints);



				}

				for (int i = 0; i < soldiers.Length; ++i) {
					soldiers [(i) % graphController.numGraphs].Graph = graphController.Graphs [(i + soldierUnderConsideration)% graphController.numGraphs];
				}
			}
		}
	}
}

