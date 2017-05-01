using System;
using UnityEngine;
using AssemblyCSharp;


	public class HealthTextController : MonoBehaviour
	{
		public TextMesh mesh;

		public ControlHealth healthController;

		public void Update(){
			mesh.text = healthController.health.ToString();
		mesh.transform.forward = gameObject.transform.forward;
		}
	}


