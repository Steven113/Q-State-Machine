using System;
using UnityEngine;

namespace AssemblyCSharp
{
	/*
	 * controls the rounds that the ai plays against each other.
	 */
	public class QTournamentController
	{
		public QSoldier[] soldiers;

		public float roundLength = 60; //round length in seconds

		public GameObject [] spawnPoints;
	}
}

