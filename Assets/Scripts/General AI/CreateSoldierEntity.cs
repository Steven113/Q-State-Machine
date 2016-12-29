﻿using UnityEngine;
using System.Collections;
using AssemblyCSharp;

public class CreateSoldierEntity : MonoBehaviour {

	public SoldierEntity entity; 

	// Use this for initialization
	void Start () {
		GameData.addEntity (entity);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnDestroy(){
		GameData.RemoveSoldier (entity);
	}
}
