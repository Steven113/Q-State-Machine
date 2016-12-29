using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using AssemblyCSharp;
using UnityEngine.UI;

[RequireComponent(typeof (HealingPotionDistributor))]
public class PlayerManager : MonoBehaviour {

	public List<string> AITextAssetLocations;
	
	public List<Transform> prefabSpawnPoints;
	
	public List<RobotPrefabMapping> prefabMap;
	
	public TextAsset referenceForPlayerData;

	public List<MeshRenderer> meshRendererToEnableUponSelection;
	
	public List<RobotType> robotTypes = new List<RobotType>();

	public List<QConjectureAgent> qConAgents = new List<QConjectureAgent>(5);

	public List<QSoldier> qSoldiers = new List<QSoldier>(5);

	public static int numberOfRespawns = 0;

	public Text respawnsLeftText;

	public int selectedAgent = 0;

	public float rewardRate = 1;
	public float punishRate = -1;

	public Text healthText;
	public Text ammoText;
	public Text robotTypeText;
	public Text robotActionsText;

	HealingPotionDistributor healingPotionDistributor;

	// Use this for initialization
	void Awake () {
		Debug.Assert (referenceForPlayerData.text != ""); 
//		for (int i = 0; i<meshRendererToEnableUponSelection.Count; ++i) {
//			meshRendererToEnableUponSelection[i].enabled = false;
//		}
		string [] playerDataLines = (referenceForPlayerData.text.Split (Environment.NewLine.ToCharArray ()));

		string [] robotTypeStrings = playerDataLines [0].Split (" ".ToCharArray ());
		Debug.Assert (int.TryParse (playerDataLines [2], out numberOfRespawns));
//		for (int i = 0; i<robotTypeStrings.Length; ++i) {
//			TextAsset tempTAsset = Resources.Load(AITextAssetLocations[i]) as TextAsset;
//			Debug.Assert(tempTAsset.text!=Environment.NewLine && tempTAsset.text!="");
//				QConjectureLearner tempQ = null;
//				Debug.Assert(Utils.DeserializeFile("Assets/Resources/"+AITextAssetLocations[i] + ".txt",ref tempQ));
//				//conjectureLearners.Add(tempQ);
//			//}
//		}

		healingPotionDistributor = gameObject.GetComponent<HealingPotionDistributor> ();

		Debug.Assert (robotTypeStrings.Length == AITextAssetLocations.Count);

		for (int i = 0; i<robotTypeStrings.Length; ++i) {
			bool hasSpawned = false;
			for (int j = 0; j<prefabMap.Count && !hasSpawned; ++j){
				if (prefabMap[j].robotType.ToString() == robotTypeStrings[i]){
					GameObject temp = (GameObject)GameObject.Instantiate(prefabMap[j].prefabToSpawn);
					robotTypes.Add(prefabMap[j].robotType);
					temp.transform.position = prefabSpawnPoints[i].position;
//					temp.transform.up = temp.transform.right;

					temp.transform.forward = (temp.transform.position-SuicideDroneController.targetPos).normalized;

					QConjectureAgent qcA = temp.GetComponent<QConjectureAgent>();
					qConAgents.Add(qcA);

					meshRendererToEnableUponSelection.Add(temp.GetComponent<QSoldier>().suppressionSphere.GetComponent<MeshRenderer>());

					QConjectureLearner tempQ = null;
					Debug.Assert(Utils.DeserializeFile(AITextAssetLocations[i] + ".txt",ref tempQ));

					qcA.learner = tempQ;

					//qcA.learner.

					QSoldier tempQS = temp.GetComponent<QSoldier>();
					tempQS.healthController.destroyOnDeath = false;
					tempQS.autoSetReward = false;

					healingPotionDistributor.soldiersToGivePotionsTo.Add(tempQS);
					//tempQS.autoSetReward = false;
					qSoldiers.Add(tempQS);

					hasSpawned = true;
				}
			}
		}

		meshRendererToEnableUponSelection [selectedAgent].enabled = true;

	}
	
	// Update is called once per frame
	void Update () {
		respawnsLeftText.text = "Respawns Left: " + numberOfRespawns;
		healthText.text ="Health: " + (qSoldiers [selectedAgent].healthController.health > 0 ? qSoldiers [selectedAgent].healthController.health : 0) + "/" + qSoldiers [selectedAgent].healthController.maxhealth;
		ammoText.text = "Ammo: " + (qSoldiers [selectedAgent].weapon.magazines.Count == 0 ? 0 : qSoldiers [selectedAgent].weapon.magazines [qSoldiers [selectedAgent].weapon.currentMag]) + "/" + qSoldiers [selectedAgent].weapon.magSize;
		robotTypeText.text = robotTypes [selectedAgent].ToString ();

		int numActions = qSoldiers [selectedAgent].currentActionSet.Count;
		string actionStr = "";
		for (int i = 0; i<numActions; ++i) {
			actionStr += qSoldiers [selectedAgent].currentActionSet[i];
			if (i<numActions-1){
				actionStr += " - ";
			}
		}

		actionStr.Replace ('_',' ');

		robotActionsText.text = actionStr;

		if (numberOfRespawns == 0) {
			for (int i = 0; i<5; ++i){
				qSoldiers[i].healthController.destroyOnDeath = true;
			}
		}

		if (Input.GetKeyDown (KeyCode.Q)) {
			RewardCurrentAgent();
		}

		if (Input.GetKeyDown (KeyCode.E)) {
			PunishCurrentAgent();
		}


	}

	public void SelectNextAgent(){
		meshRendererToEnableUponSelection [selectedAgent].enabled = false;
		++selectedAgent;
		selectedAgent %= qSoldiers.Count;
		meshRendererToEnableUponSelection [selectedAgent].enabled = true;
	}

	public void SelectPreviousAgent(){
		meshRendererToEnableUponSelection [selectedAgent].enabled = false;
		selectedAgent = selectedAgent + qSoldiers.Count - 1;
		selectedAgent %= qSoldiers.Count;
		meshRendererToEnableUponSelection [selectedAgent].enabled = true;
	}

	public void PunishCurrentAgent(){
		qConAgents [selectedAgent].RewardAgent (punishRate);
	}

	public void RewardCurrentAgent(){
		qConAgents [selectedAgent].RewardAgent (rewardRate);
	}

}
