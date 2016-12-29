using UnityEngine;
using System.Collections;

public class MainMenuController : MonoBehaviour {

	bool acceptInputs = true;
	public string missionSelectSceneName;
	public string trainTeamSceneName;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void OnPlayMissionsSelect(){
		acceptInputs = false;
		Application.LoadLevel (missionSelectSceneName);

	}

	public void OnTrainTeamSelect(){
		acceptInputs = false;
		Application.LoadLevel (trainTeamSceneName);
		
	}

	public void OnExitSelect(){
		acceptInputs = false;
		Application.Quit ();
	}

}
