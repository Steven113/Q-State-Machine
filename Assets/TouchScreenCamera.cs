using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TouchScreenCamera : MonoBehaviour {

	public Vector2 minCorner;
	public Vector2 maxCorner;

	public Vector2 cameraSpeed = Vector3.zero;

	public float deaccellerationRate = 1f;

	public float maxCameraSpeed = 10f;

	public float cameraAccellerationFactor = 0.1f;
	public List<Vector3> previousTouchPos = new List<Vector3>(10);// we know that the player can only touch the screen in at most ten places at once

	public Vector3 debugPrevMousePos = Vector3.zero;

	// Use this for initialization
	void Awake () {
		for (int i = 0; i<10; ++i) {
			previousTouchPos.Add(Vector3.zero);
		}
	}
	
	// Update is called once per frame
	void Update () {
		Touch [] touches = Input.touches;
		int T_L = touches.Length;

		Vector3 speedSlowdown = Vector3.zero;
		int numFingersDraggingOnScreen = 0;

		for (int i = 0; i<T_L; ++i) {
			if (touches[i].phase == TouchPhase.Began){
				previousTouchPos[touches[i].fingerId%10] = touches[i].position;
			} else if (touches[i].phase == TouchPhase.Moved){
				speedSlowdown+=((Vector3)touches[i].position-previousTouchPos[touches[i].fingerId%10]);
				++numFingersDraggingOnScreen;
				previousTouchPos[touches[i].fingerId%10] = touches[i].position;
			}


		}

		if (Input.GetMouseButtonDown (0)) {
			debugPrevMousePos = Input.mousePosition;
		} else if (Input.GetMouseButton (0)) {
			speedSlowdown = Input.mousePosition - debugPrevMousePos;
			numFingersDraggingOnScreen=1;
			debugPrevMousePos = Input.mousePosition;
		}

		if (numFingersDraggingOnScreen > 0) {
			speedSlowdown /= numFingersDraggingOnScreen;
			speedSlowdown *= cameraAccellerationFactor;

			if (cameraSpeed.magnitude<maxCameraSpeed){

			cameraSpeed.x += (Time.deltaTime * speedSlowdown.x);
			cameraSpeed.y += (Time.deltaTime * speedSlowdown.y);
			}
		}
		if (numFingersDraggingOnScreen==0) {
			if (cameraSpeed.magnitude > deaccellerationRate * Time.deltaTime){
			cameraSpeed -= cameraSpeed.normalized * deaccellerationRate * Time.deltaTime;
			} else {
				cameraSpeed.x = 0;
				cameraSpeed.y = 0;
			}
		}


		
		Vector3 deltaPos = Vector3.zero;
		deltaPos.x = (-cameraSpeed.x * Time.deltaTime);
		deltaPos.z = (-cameraSpeed.y * Time.deltaTime);
//		if (deltaPos != Vector3.zero) {
//			Debug.Log (deltaPos);
//		}
		gameObject.transform.position += new Vector3(deltaPos.x,0,deltaPos.z);
		float clampZ = Mathf.Clamp (gameObject.transform.position.z, minCorner.y, maxCorner.y);
		float clampX = Mathf.Clamp (gameObject.transform.position.x, minCorner.x, maxCorner.x);
		if (clampX != gameObject.transform.position.x || clampZ != gameObject.transform.position.z) {
			cameraSpeed = Vector2.zero;
		}
		gameObject.transform.position = new Vector3 (clampX, gameObject.transform.position.y, clampZ);
	}
}
