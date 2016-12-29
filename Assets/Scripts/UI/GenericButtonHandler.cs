using UnityEngine;
using System.Collections;

public class GenericButtonHandler : MonoBehaviour {

	bool acceptInput = true;

	public void LoadLevel(string levelName){
		if (acceptInput) {
			acceptInput = false;
			Application.LoadLevel(levelName);
		}
	}

	public void Exit(){
		if (acceptInput) {
			acceptInput = false;
			Application.Quit();
		}
	}
}
