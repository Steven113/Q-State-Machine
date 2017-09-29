#define DEBUG_CONDITION
#define ORBIT_OBJECT
using System;
using UnityEngine;


public class CameraController : MonoBehaviour
{
	[SerializeField]new Camera camera;
	//movement rate of camera in units per second
	[SerializeField]float f_defaultMoveSpeed = 1f;
	//percentage speed boost when holding shift and pressing movement keys
	[SerializeField]float f_shiftSpeedBoost = 1.5f;
	//how fast the camera moves up and down the transform's up axis
	[SerializeField]float f_ascensionSpeed = 0.5f;
	//percentage speed boost when holding shift and pressing ascend/descend keys
	[SerializeField]float f_shiftSpeedBoostAscension = 3f;



	[SerializeField]Vector3 l_previousMousePos = Vector3.zero;

	[SerializeField]Vector3 f_rotationRate = Vector3.one;

	[SerializeField]KeyCode f_orbitKeyCode = KeyCode.Space;
	[SerializeField]float f_orbitSensitivity = 1f;
	[SerializeField]float f_defaultOrbitDist = 10f;
	[SerializeField]float f_minOrbitDist = 10f;
	[SerializeField]float f_mousePosZ = 10f;
	Vector3 resetPosition;
	Quaternion resetQuaterion;

	// for orbit cam
	Plane xyPlane = default(Plane);
	Ray ray = new Ray();
	float intersectDist;
	Vector3 orbitPoint;
	[SerializeField]float f_xScalingMouseMovementOrbit = 1f;
	[SerializeField]float f_yScalingMouseMovementOrbit = 1f; 
	//rotation rate by axis


	void Awake ()
	{
		
        orbitPoint = Vector3.zero - transform.position; //finds the centre of the screen
        xyPlane = new Plane (Vector3.up, Vector3.zero);
		resetPosition = gameObject.transform.position;
		resetQuaterion = gameObject.transform.rotation;
	}

	void Update ()
	{
		if (Input.GetKeyDown (KeyCode.J)) {
			gameObject.transform.position = resetPosition;
			gameObject.transform.rotation = resetQuaterion;
		}

		//only accept movement and rotation input of RMB is held down
		if (Input.GetMouseButtonDown (1) || Input.GetMouseButton (1) || Input.GetMouseButtonUp (1)) {

			//rotate camera if mouse button held down
			if (!Input.GetMouseButtonDown (1) && (Input.GetMouseButton (1) || Input.GetMouseButtonUp (1))) {
				Vector3 delta = Input.mousePosition - l_previousMousePos;
				//camera.transform.Rotate (new Vector3(delta.x * f_rotationRate.x*Time.deltaTime, delta.y * f_rotationRate.y*Time.deltaTime, delta.z * f_rotationRate.z*Time.deltaTime));
				camera.transform.RotateAround (camera.transform.position, Vector3.up, delta.x * f_rotationRate.x * Time.deltaTime);
				camera.transform.RotateAround (camera.transform.position, camera.transform.right, delta.y * f_rotationRate.y * Time.deltaTime);
			}

			l_previousMousePos = Input.mousePosition;
		
			//left/right movement
			if (Input.GetKey (KeyCode.A)) {
				if (Input.GetKey (KeyCode.LeftShift) || Input.GetKeyDown (KeyCode.RightShift)) {
					camera.transform.position = camera.transform.position - camera.transform.right * f_defaultMoveSpeed * f_shiftSpeedBoost;
				} else {
					camera.transform.position = camera.transform.position - camera.transform.right * f_defaultMoveSpeed;
				}

			} else if (Input.GetKey (KeyCode.D)) {
				if (Input.GetKey (KeyCode.LeftShift) || Input.GetKeyDown (KeyCode.RightShift)) {
					camera.transform.position = camera.transform.position + camera.transform.right * f_defaultMoveSpeed * f_shiftSpeedBoost;
				} else {
					camera.transform.position = camera.transform.position + camera.transform.right * f_defaultMoveSpeed;
				}
			}

			//forwards/backwards movement
			if (Input.GetKey (KeyCode.W)) {
				if (Input.GetKey (KeyCode.LeftShift) || Input.GetKeyDown (KeyCode.RightShift)) {
					camera.transform.position = camera.transform.position + camera.transform.forward * f_defaultMoveSpeed * f_shiftSpeedBoost;
				} else {
					camera.transform.position = camera.transform.position + camera.transform.forward * f_defaultMoveSpeed;
				}

			} else if (Input.GetKey (KeyCode.S)) {
				if (Input.GetKey (KeyCode.LeftShift) || Input.GetKeyDown (KeyCode.RightShift)) {
					camera.transform.position = camera.transform.position - camera.transform.forward * f_defaultMoveSpeed * f_shiftSpeedBoost;
				} else {
					camera.transform.position = camera.transform.position - camera.transform.forward * f_defaultMoveSpeed;
				}
			}

			//make camera ascend or descend along "up" direction of local camera transform
			if (Input.GetKey (KeyCode.E)) {
				if (Input.GetKey (KeyCode.LeftShift) || Input.GetKeyDown (KeyCode.RightShift)) {
					camera.transform.position = camera.transform.position + camera.transform.up * f_ascensionSpeed * f_shiftSpeedBoostAscension;
				} else {
					camera.transform.position = camera.transform.position + camera.transform.up * f_ascensionSpeed;
				}

			} else if (Input.GetKey (KeyCode.F)) {
				if (Input.GetKey (KeyCode.LeftShift) || Input.GetKeyDown (KeyCode.RightShift)) {
					camera.transform.position = camera.transform.position - camera.transform.up * f_ascensionSpeed * f_shiftSpeedBoostAscension;
				} else {
					camera.transform.position = camera.transform.position - camera.transform.up * f_ascensionSpeed;
				}
			}

		} else if (!(Input.GetMouseButtonDown (0) || Input.GetMouseButton (0) || Input.GetMouseButtonUp (0))) {
			

            float scroll = Input.GetAxis("Mouse ScrollWheel");

            if (scroll > 0)
            {
                camera.transform.Translate(Vector3.forward);
            }
            else if (scroll < 0)
            {
                camera.transform.Translate(Vector3.back);
            }

			if (Input.GetKey (KeyCode.Space) || Input.GetKey(KeyCode.Mouse2)) {
				
                    Vector3 screenCentre = new Vector3 (Screen.width / 2, Screen.height / 2);
				    ray = camera.ScreenPointToRay (screenCentre);
				    Debug.DrawRay(ray.origin,ray.direction,Color.green,2f);

				    if (xyPlane.Raycast(ray,out intersectDist))
                    {
                        orbitPoint = ray.origin + ray.direction.normalized * intersectDist; //centre of selected gameObject
                    }
                    else
                    {
                        orbitPoint = Vector3.zero; //centre of scene
                    }

    //				if (intersectDist<f_minOrbitDist) {
    //					intersectDist = f_defaultOrbitDist;
    //				}

				    

				    //Debug.Log ("planeIntersectPoint " + orbitPoint);
  				   // Debug.Log ("l_previousMousePos "+l_previousMousePos);
				    //Debug.Log ("Input.mousePosition "+Input.mousePosition);
					

				//Vector3 transformedPreviousMousePoint = Camera.main.ScreenToWorldPoint (new Vector3(l_previousMousePos.x,l_previousMousePos.y,f_mousePosZ) - screenCentre);
				//Vector3 transformedMousePos = Camera.main.ScreenToWorldPoint (new Vector3(Input.mousePosition.x,Input.mousePosition.y,f_mousePosZ) - screenCentre);

				//Debug.Log ("transformedPreviousMousePoint "+transformedPreviousMousePoint);
				//Debug.Log ("transformedMousePos "+transformedMousePos);

				Vector3 delta = Input.mousePosition - l_previousMousePos;

				camera.transform.RotateAround (orbitPoint, Vector3.up, delta.x * f_xScalingMouseMovementOrbit);
				camera.transform.RotateAround (orbitPoint, -camera.transform.right, delta.y * f_yScalingMouseMovementOrbit);


				//l_previousMousePos = Input.mousePosition;
			}

			l_previousMousePos = Input.mousePosition;
		}
	}


}