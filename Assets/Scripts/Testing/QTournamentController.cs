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

		public string dateOfTestStart;

		public static GameObject [] g_SpawnPoints;

		public void Start(){
			UnityEngine.Random.InitState (5145636);

			dateOfTestStart = DateTime.Now.Ticks.ToString();

			//graphController.Evolve ();
			Debug.Log("Num combos: "+Mathf.Pow(graphController.numGraphs,soldiers.Length));
			Logging.globalLogger.Log("Num combos: "+Mathf.Pow(graphController.numGraphs,soldiers.Length));
			g_SpawnPoints = spawnPoints;

			for (int i = 0; i < soldiers.Length; ++i) {
				//if (soldiers [(i) % graphController.numGraphs].Graph == null) {
					soldiers [(i) % graphController.numGraphs].Graph = graphController.Graphs [(i) % graphController.numGraphs];
				//} else {
				//	graphController.Graphs [(i) % graphController.numGraphs] = soldiers [(i) % graphController.numGraphs].Graph;
				//}
			}

			FactionName [] activeFactions = GameData.ActiveFactions ();
			GameData.scores.Clear ();
			for (int i = 0; i < activeFactions.Length; ++i) {
				GameData.scores.Add (activeFactions [i], 0);
			}
		}

		Dictionary<QGraph, int> numAssessments = new Dictionary<QGraph, int>();//for debug

		public void Update(){
			timeSinceRoundStart += Time.deltaTime;

			if (timeSinceRoundStart > roundLength) {
				timeSinceRoundStart = 0;
				Logging.globalLogger.Log("===============================================================================");

				if (soldierUnderConsideration_j >= ((soldiers.Length>1f)?(Mathf.Pow(graphController.numGraphs,soldiers.Length)):((float)graphController.numGraphs))) {

					float average_evolving = 0;

					float[] fitnessVals_evolving = new float[graphController.numGraphs];

					float average_nonevolving = 0;

					float[] fitnessVals_nonevolving = new float[dummySoldiers.Length];

					Logging.globalLogger.Log ("Evolving enemies");

					//for evolving enemies
					for (int i = 0; i < graphController.numGraphs; ++i) {
						Utils.SerializeFile<QGraph> ("Graph_"+dateOfTestStart+"_ID_"+ graphController.Graphs[i].ID +"_"+gameObject.transform.name+"_"+DateTime.Now.Ticks, ref graphController.Graphs[i]);

						Debug.Log ("Graph " + i + " ID: " + graphController.Graphs[i].ID + " has fitness: " + graphController.Graphs [i].TotalReward);
						average_evolving += graphController.Graphs [i].TotalReward;
						fitnessVals_evolving [i] = graphController.Graphs [i].TotalReward;
						Logging.globalLogger.Log("Graph " + i + " ID: " + graphController.Graphs[i].ID + " has fitness: " + graphController.Graphs [i].TotalReward);
						Debug.Assert (numAssessments [graphController.Graphs [i]] == numAssessments [graphController.Graphs [(i + 1) % graphController.numGraphs]], "Graph " + i+ " tested "+numAssessments [graphController.Graphs [i]] + " times, graph " + ((i + 1) % graphController.numGraphs) + " tested "+numAssessments [graphController.Graphs [(i + 1) % graphController.numGraphs]]+ " times." );
					}

					Logging.globalLogger.Log ("Static enemies");

					//for non-evolving enemies
					for (int i = 0; i < dummySoldiers.Length; ++i) {
						Debug.Log ("Graph " + i + " ID: " + dummySoldiers[i].Graph.ID + " has fitness: " + dummySoldiers[i].Graph.TotalReward);
						average_nonevolving += dummySoldiers[i].Graph.TotalReward;
						fitnessVals_nonevolving [i] = dummySoldiers[i].Graph.TotalReward;
						Logging.globalLogger.Log("Graph " + i + " ID: " + dummySoldiers[i].Graph.ID + " has fitness: " + dummySoldiers[i].Graph.TotalReward);

					}

					average_evolving /= graphController.numGraphs;

					average_nonevolving /= dummySoldiers.Length;

					float stddev_evolving = Utils.StandardDeviation (fitnessVals_evolving);

					float stddev_nonevolving = Utils.StandardDeviation (fitnessVals_nonevolving);

					Logging.globalLogger.Log ("Evolving enemies");

					Debug.Log ("Average Fitness of evolving enemy " + average_evolving);
					Logging.globalLogger.Log ("Average Fitness of evolving enemy " +average_evolving.ToString());

					Debug.Log ("StdDev of Fitness of evolving enemy " + stddev_evolving);
					Logging.globalLogger.Log ("StdDev of Fitness of evolving enemy " + stddev_evolving);

					Logging.globalLogger.Log ("Static enemies");

					Debug.Log ("Average Fitness of non-evolving enemy " + average_nonevolving);
					Logging.globalLogger.Log ("Average Fitness of non-evolving enemy " +average_nonevolving.ToString());

					Debug.Log ("StdDev of Fitness of non-evolving enemy " + stddev_nonevolving);
					Logging.globalLogger.Log ("StdDev of Fitness of non-evolving enemy " + stddev_nonevolving);

					graphController.Evolve ();

					Logging.globalLogger.Log("===============================================================================");
					Logging.globalLogger.Log("+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
					Logging.globalLogger.Log("===============================================================================");
					Debug.Log ("Evolving.");
					Logging.globalLogger.Log("Evolving.");

					numAssessments.Clear ();

					soldierUnderConsideration_j = 0;

					spawnPoints = Utils.ShuffleArray (spawnPoints);
				}

				spawnPoints = Utils.ShuffleArray (spawnPoints);

				for (int i = 0; i < dummySoldiers.Length; ++i) {
					//dummySoldiers [i].Graph.ResetCurrentNodeToRoot ();
					QSoldier qs = dummySoldiers[i].gameObject.GetComponent<QSoldier> ();
					qs.agent.Warp(spawnPoints [i % spawnPoints.Length].transform.position);
					qs.Reset ();
					dummySoldiers [i].Graph.ResetCurrentNodeToRoot ();
				}

				for (int i = 0; i < soldiers.Length; ++i) {
					
					QSoldier qs = soldiers [i].gameObject.GetComponent<QSoldier> ();
					qs.agent.Warp(spawnPoints [(i+dummySoldiers.Length) % spawnPoints.Length].transform.position);
					qs.Reset ();
				}

				//Debug.Log (GameData.scores [0] + " " + GameData.scores [1]);

				//GameData.scores = new float[]{ 0, 0 };

				FactionName [] activeFactions = GameData.ActiveFactions ();
				for (int i = 0; i < activeFactions.Length; ++i) {
					Debug.Log (activeFactions [i].ToString () + " : " + GameData.scores [activeFactions [i]]);
					Logging.globalLogger.Log (activeFactions [i].ToString () + " : " + GameData.scores [activeFactions [i]]);
					GameData.scores [activeFactions [i]] = 0;
				}

				//GameData.scores.Clear ();

				for (int i = 0; i < soldiers.Length; ++i) {
					int selectedGraph = (int)((soldierUnderConsideration_j / Mathf.Pow (graphController.numGraphs, i))) % graphController.numGraphs;
					soldiers [i].Graph = graphController.Graphs [selectedGraph];
					if (!numAssessments.ContainsKey(graphController.Graphs[selectedGraph])){
						numAssessments.Add(graphController.Graphs[selectedGraph],1);
					} else {
						numAssessments[graphController.Graphs[selectedGraph]]+=1;
					}

					soldiers [i].Reset ();

					Debug.Log ("Soldier " + i + " given graph " + selectedGraph + " " + graphController.Graphs[selectedGraph].ID);
					Logging.globalLogger.Log ("Soldier " + i + " given graph " + selectedGraph + " " + graphController.Graphs[selectedGraph].ID);

				}


				++soldierUnderConsideration_j;

				//for (int i = 0; i < soldiers.Length; ++i) {
				//	soldiers [(i) % graphController.numGraphs].Graph = graphController.Graphs [(i + soldierUnderConsideration_i)% graphController.numGraphs];
				//}
			}
		}
	}
}

