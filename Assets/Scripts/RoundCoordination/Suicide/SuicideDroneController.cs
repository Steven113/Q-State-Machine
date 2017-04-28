using UnityEngine;
using System.Collections;
using AssemblyCSharp;

[RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]
[RequireComponent(typeof(CreateSoldierEntity))]
public class SuicideDroneController : MonoBehaviour {

	public static Vector3 targetPos;

	// Use this for initialization
	void Start () {
		GetComponent<UnityEngine.AI.NavMeshAgent> ().SetDestination (targetPos);
		++SuicideSpawner.numSuicideDronesInPlay;
		//GameData.addEntity (GetComponent<CreateSoldierEntity> ().entity);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnDestroy(){
	//	GameData.RemoveSoldier (GetComponent<CreateSoldierEntity> ().entity);
		--SuicideSpawner.numSuicideDronesInPlay;
		++PlayerManager.numberOfRespawns;
	}
}
