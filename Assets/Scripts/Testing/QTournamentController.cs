using System;
using UnityEngine;
using System.Collections.Generic;
using AI;
using Gameplay;
using QGraphLearning;

namespace AssemblyCSharp
{
	/// <summary>
	/// Coordinates the rounds of competing between QGraphAgent instances
	/// </summary>
	public class QTournamentController : MonoBehaviour
	{
		/*
		 * Agents that learn
		 */
		public QGraphAgent[] soldiers;

		/*
		 * Dummy agents have no learning capability but are still statically
		 * described in terms of the same data structure that the evolving 
		 * agents use
		 */
		public QGraphAgent[] dummySoldiers;

		public float roundLength = 120; //round length in seconds

		public float timeSinceRoundStart = 0;

		public GameObject [] spawnPoints;

		public QGraphController EvolveEntities;

		/*
		 * this value is needed for iterating through combos of soldiers between rounds.
		 * Since we need this value between rounds we keep it outside any method
		 */
		[SerializeField]int soldierUnderConsideration_j = 1;

		public string dateOfTestStartInTicks;

		public static GameObject [] g_SpawnPoints;

		float[] dummyFitness;

		public int numTestingRounds = 11;
		public int testingRound = 0;

		public int originalReinjectionInterval = 1; //how many rounds before a copy of the original

		public void Start(){
			//UnityEngine.Random.InitState (5145636);



			dummyFitness = new float[(int)((soldiers.Length>1f)?(Mathf.Pow(EvolveEntities.NumGraphs,soldiers.Length)):((float)EvolveEntities.NumGraphs))];

			dateOfTestStartInTicks = DateTime.Now.Ticks.ToString();

			//graphController.Evolve ();
			Debug.Log("Num combos: "+Mathf.Pow(EvolveEntities.NumGraphs,soldiers.Length));
			Logging.globalLogger.Log("Num combos: "+Mathf.Pow(EvolveEntities.NumGraphs,soldiers.Length));
			g_SpawnPoints = spawnPoints;

			//Array.Sort (g_SpawnPoints,new GameObjectComparer());

			//set the sequence of evolving agents that will participate in the tournament by retrieving them
			//from the list of defined qGraph-based agents
			for (int i = 0; i < soldiers.Length; ++i) {
					soldiers [(i) % EvolveEntities.NumGraphs].Graph = EvolveEntities.Graphs [(i) % EvolveEntities.NumGraphs];
			}

			/*
			 * Factions participating in tournament
			 */
			FactionName [] activeFactions = GameData.ActiveFactions ();
			//GameData.scores.Clear ();

		}

		//stores how many times a qGraph has been assessed before the evolution phase
		Dictionary<QGraph, int> numAssessments = new Dictionary<QGraph, int>();//for debug

		public void Update(){
			timeSinceRoundStart += Time.deltaTime;

			/*
			 * If round is over
			 */
			if (timeSinceRoundStart > roundLength) {
				timeSinceRoundStart = 0;
				Logging.globalLogger.Log("===============================================================================");

				/* 
				 * Even though pairs of soldiers compete (implying two iterators should be used),
				 * in the following line we use soldierUnderConsideration_j as a permutation counter. 
				 * Suppose we have n soldiers competing. That means that n * n rounds should occur.
				 * So we can get which two soldiers as follows: The first soldier would be at the
				 * index floor(soldierUnderConsideration_j/n), meaning that the first soldier changes 
				 * every n rounds. Meanwhile the second soldier is the one at index
				 * (soldierUnderConsideration_j%n), which causes the second soldier to alternate
				 * every single round
				 */

				/*
					if we have have completed all the rounds we needed to pit every QGraph against every other QGraph
				*/
				if (soldierUnderConsideration_j >= ((soldiers.Length>1f)?(Mathf.Pow(EvolveEntities.NumGraphs,soldiers.Length)):((float)EvolveEntities.NumGraphs))) {

					++testingRound;
					/*
					 * We have done as many testing rounds as we need to, end the test
					 */
					if (testingRound > numTestingRounds) {
						UnityEditor.EditorApplication.isPlaying = false;
						enabled = false;
					}

					ComputeAndStoreFitnessForEndOfRound ();

					Debug.Log ("Evolving.");
					Logging.globalLogger.Log("Evolving.");

					EvolveEntities.Evolve (originalReinjectionInterval != 0 && testingRound % originalReinjectionInterval == 0);

					Logging.globalLogger.Log ("===============================================================================");
					Logging.globalLogger.Log ("+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
					Logging.globalLogger.Log ("===============================================================================");

					numAssessments.Clear ();

					soldierUnderConsideration_j = 0;

					//UnityEngine.Random.InitState (5145636);

					//Array.Sort (g_SpawnPoints,new GameObjectComparer());

					//g_SpawnPoints = Utils.ShuffleArray (g_SpawnPoints);
				}

				g_SpawnPoints = Utils.ShuffleArray (g_SpawnPoints);


				ResetSoldierStatusAtEndOfRound ();


				//Debug.Log (GameData.scores [0] + " " + GameData.scores [1]);

				//GameData.scores = new float[]{ 0, 0 };

				RecordFactionScores ();
				//GameData.scores.Clear ();

				/*
				 * Rotate what graph is ued by each soldier
				 */
				for (int i = 0; i < soldiers.Length; ++i) {
					int selectedGraph = (int)((soldierUnderConsideration_j / Mathf.Pow (EvolveEntities.NumGraphs, i))) % EvolveEntities.NumGraphs;
					soldiers [i].Graph = EvolveEntities.Graphs [selectedGraph];
					if (!numAssessments.ContainsKey(EvolveEntities.Graphs[selectedGraph])){
						numAssessments.Add(EvolveEntities.Graphs[selectedGraph],1);
					} else {
						numAssessments[EvolveEntities.Graphs[selectedGraph]]+=1;
					}

					soldiers [i].Reset ();

					Debug.Log ("Soldier " + i + " given graph " + selectedGraph + " " + EvolveEntities.Graphs[selectedGraph].ID);
					Logging.globalLogger.Log ("Soldier " + i + " given graph " + selectedGraph + " " + EvolveEntities.Graphs[selectedGraph].ID);

				}


				++soldierUnderConsideration_j;

				//for (int i = 0; i < soldiers.Length; ++i) {
				//	soldiers [(i) % graphController.NumGraphs].Graph = graphController.Graphs [(i + soldierUnderConsideration_i)% graphController.NumGraphs];
				//}
			}
		}

		/// <summary>
		/// Computes the and store fitness for end of round.
		/// </summary>
		void ComputeAndStoreFitnessForEndOfRound ()
		{
			/*
					 * Compute and store fitness scores for evolving and non-evolving enemies
					 */float average_evolving = 0;
			float[] fitnessVals_evolving = new float[EvolveEntities.NumGraphs];
			float average_nonevolving = 0;
			float[] fitnessVals_nonevolving = new float[dummyFitness.Length];
			Logging.globalLogger.Log ("Evolving enemies");
			Logging.globalLogger.Log (dateOfTestStartInTicks + "_results.csv", "Evol,", false);
			/*
					 * Serialize QGraphs of evolving enemies while also getting the rewards they earned during
					 * the round
					 */for (int i = 0; i < EvolveEntities.NumGraphs; ++i) {
				Utils.SerializeToFileJSON<QGraph> (ref EvolveEntities.Graphs [i], "Graph_" + dateOfTestStartInTicks + "_ID_" + EvolveEntities.Graphs [i].ID + "_" + gameObject.transform.name + "_" + DateTime.Now.Ticks + ".json");
				Debug.Log ("Graph " + i + " ID: " + EvolveEntities.Graphs [i].ID + " has fitness: " + EvolveEntities.Graphs [i].TotalReward);
				average_evolving += EvolveEntities.Graphs [i].TotalReward;
				fitnessVals_evolving [i] = EvolveEntities.Graphs [i].TotalReward;
				Logging.globalLogger.Log (dateOfTestStartInTicks + "_results.csv", EvolveEntities.Graphs [i].TotalReward + ",", false);
				Logging.globalLogger.Log ("Graph " + i + " ID: " + EvolveEntities.Graphs [i].ID + " has fitness: " + EvolveEntities.Graphs [i].TotalReward);
				Debug.Assert (numAssessments [EvolveEntities.Graphs [i]] == numAssessments [EvolveEntities.Graphs [(i + 1) % EvolveEntities.NumGraphs]], "Graph " + i + " tested " + numAssessments [EvolveEntities.Graphs [i]] + " times, graph " + ((i + 1) % EvolveEntities.NumGraphs) + " tested " + numAssessments [EvolveEntities.Graphs [(i + 1) % EvolveEntities.NumGraphs]] + " times.");
			}
			Logging.globalLogger.Log ("Static enemies");
			Logging.globalLogger.Log (dateOfTestStartInTicks + "_results.csv", "Static enemies,", false);
			//for non-evolving enemies, compute fitness scores
			for (int i = 0; i < dummyFitness.Length; ++i) {
				Debug.Log ("For round " + i + " the fitness was " + dummyFitness [i]);
				average_nonevolving += dummyFitness [i];
				fitnessVals_nonevolving [i] = dummyFitness [i];
				Logging.globalLogger.Log (dateOfTestStartInTicks + "_results.csv", dummyFitness [i] + ",", false);
				Logging.globalLogger.Log ("For round " + i + " the fitness was " + dummyFitness [i]);
			}
			Logging.globalLogger.Log (dateOfTestStartInTicks + "_results.csv", Environment.NewLine, false);
			average_evolving /= EvolveEntities.NumGraphs;
			average_nonevolving /= dummyFitness.Length;
			float stddev_evolving = Utils.StandardDeviation (fitnessVals_evolving);
			float stddev_nonevolving = Utils.StandardDeviation (fitnessVals_nonevolving);
			Logging.globalLogger.Log ("Evolving enemies");
			Debug.Log ("Average Fitness of evolving enemy " + average_evolving);
			Logging.globalLogger.Log ("Average Fitness of evolving enemy " + average_evolving.ToString ());
			Debug.Log ("StdDev of Fitness of evolving enemy " + stddev_evolving);
			Logging.globalLogger.Log ("StdDev of Fitness of evolving enemy " + stddev_evolving);
			Logging.globalLogger.Log ("Static enemies");
			Debug.Log ("Average Fitness of non-evolving enemy " + average_nonevolving);
			Logging.globalLogger.Log ("Average Fitness of non-evolving enemy " + average_nonevolving.ToString ());
			Debug.Log ("StdDev of Fitness of non-evolving enemy " + stddev_nonevolving);
			Logging.globalLogger.Log ("StdDev of Fitness of non-evolving enemy " + stddev_nonevolving);
			Logging.globalLogger.Log ("===============================================================================");
			Logging.globalLogger.Log ("+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
			Logging.globalLogger.Log ("===============================================================================");
		}

		/// <summary>
		/// Resets the soldier status at end of round.
		/// </summary>
		void ResetSoldierStatusAtEndOfRound ()
		{
			for (int i = 0; i < dummySoldiers.Length; ++i) {
				//dummySoldiers [i].Graph.ResetCurrentNodeToRoot ();
				QSoldier qs = dummySoldiers [i].gameObject.GetComponent<QSoldier> ();
				qs.agent.Warp (spawnPoints [i % spawnPoints.Length].transform.position);
				qs.ResetSoldier ();
				dummySoldiers [i].Graph.ResetCurrentNodeToRoot ();
				dummyFitness [soldierUnderConsideration_j] = dummySoldiers [i].Graph.TotalReward;
				dummySoldiers [i].Graph.TotalReward = 0;
				/*
				 * Note that we DO RESET THE REWARD since it gets put in the "dummy fitness" 
				 * array before being reset
				 */
			}
			for (int i = 0; i < soldiers.Length; ++i) {
				QSoldier qs = soldiers [i].gameObject.GetComponent<QSoldier> ();
				qs.agent.Warp (spawnPoints [(i + dummySoldiers.Length) % spawnPoints.Length].transform.position);
				qs.ResetSoldier ();
				/*
				 * Note that we DO NOT RESET THE REWARD because the reward needs to be accumulated during all rounds
				 * where the agent is active
				 */
				//soldiers [i].Graph.TotalReward = 0;
			}
		}

		void RecordFactionScores ()
		{
			/*
				 * Print out faction scores to console and log file
				 */FactionName[] activeFactions = GameData.ActiveFactions ();
			for (int i = 0; i < activeFactions.Length; ++i) {
				Debug.Log (activeFactions [i].ToString () + " : " + GameData.scores [activeFactions [i]]);
				Logging.globalLogger.Log (activeFactions [i].ToString () + " : " + GameData.scores [activeFactions [i]]);
				Logging.globalLogger.Log (dateOfTestStartInTicks + "_scores.csv", GameData.scores [activeFactions [i]] + ",", false);
				GameData.scores [activeFactions [i]] = 0;
			}
			Logging.globalLogger.Log (dateOfTestStartInTicks + "_scores.csv", "", true);
		}
	}
}

