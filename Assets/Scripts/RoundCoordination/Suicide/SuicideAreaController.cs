using UnityEngine;
using System.Collections;
using AssemblyCSharp;
using UnityEngine.UI;
using Gameplay;
using AI;

public class SuicideAreaController : MonoBehaviour {

	public int numberOfLives = 10;

	public static int numLivesLeft;

	public Collider area;


	public Text livesLeftText;

	public bool endLevelWhenLivesRunOut = false;

	public string levelToLoadWhenLivesRunOut;

	// Use this for initialization
	void Start () {
		if (endLevelWhenLivesRunOut) {
			numLivesLeft = numberOfLives;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (endLevelWhenLivesRunOut) {
			livesLeftText.text = "Lives left: " + numLivesLeft;
		} else {
			livesLeftText.text = "Drones that got through: " + numLivesLeft;
		}
		Faction faction = GameData.getFaction (FactionName.SuicideFaction);
		for (int i = 0; i<faction.Soldiers.Count; ++i) {
			if (area.bounds.Contains(faction.Soldiers[i].centreOfMass.position)){
				faction.Soldiers[i].controlHealth.damage(int.MaxValue);
				--PlayerManager.numberOfRespawns;
				PlayerManager.numberOfRespawns = PlayerManager.numberOfRespawns>0?PlayerManager.numberOfRespawns:0;
				if (endLevelWhenLivesRunOut) {
				--numLivesLeft;
				} else {
					++numLivesLeft;
				}
				//--i;
			}
		}

		if (endLevelWhenLivesRunOut && numLivesLeft == 0) {
			Application.LoadLevel(levelToLoadWhenLivesRunOut);
		}
	}
}
