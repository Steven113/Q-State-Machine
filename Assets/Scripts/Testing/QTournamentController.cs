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

		public void Update(){
			timeSinceRoundStart += Time.deltaTime;

			if (timeSinceRoundStart > roundLength) {
				timeSinceRoundStart = 0;
				++soldierUnderConsideration;
				if (soldierUnderConsideration > graphController.numGraphs) {
					graphController.Evolve ();
					soldierUnderConsideration = 0;

					spawnPoints = Utils.ShuffleArray (spawnPoints);

					for (int i = 0; i < soldiers.Length; ++i) {
						soldiers [i].gameObject.transform.position = spawnPoints [i % spawnPoints.Length].transform.position;
					}

				}

				for (int i = 0; i < graphController.numGraphs; ++i) {
					soldiers [(i + soldierUnderConsideration) % graphController.numGraphs].Graph = graphController.Graphs [i];
				}
			}
		}
	}
}

