//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34209
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using UnityEngine;


namespace AssemblyCSharp
{
    public class GrenadeProjectile : ProjectileScript
    {
        public float blastRadius = 1;

        public void Update()
        {
            direction = ((gameObject.transform.position + speed * direction * Time.deltaTime + airStabilisation*Physics.gravity * Time.deltaTime * Time.deltaTime) - gameObject.transform.position).normalized;
            intersectionRay.origin = gameObject.transform.position;
            intersectionRay.direction = direction;
            speed += (Physics.gravity * Time.deltaTime).magnitude;

			RaycastHit [] suppressionSpheresHit;
			suppressionSpheresHit = (Physics.RaycastAll (intersectionRay, speed * Time.deltaTime, ((1 << LayerMask.NameToLayer ("Suppression")))));
//			if (suppressionSpheresHit != null && suppressionSpheresHit.Length > 0) {
//				for (int i = 0; i<suppressionSpheresHit.Length; i++) {
//					if (GameData.SuppressionSphereDictionary.ContainsKey (suppressionSpheresHit [i].collider)) {
//						if (GameData.SuppressionSphereDictionary [suppressionSpheresHit [i].collider] is PlayerController) {
//							//((PlayerController)GameData.SuppressionSphereDictionary [suppressionSpheresHit [i].collider]).suppress (damage);
//						} else if (GameData.SuppressionSphereDictionary [suppressionSpheresHit [i].collider] is AIController) {
//							((AIController)GameData.SuppressionSphereDictionary [suppressionSpheresHit [i].collider]).suppress (damage,factionThatFiredShot, direction);
//						}
//					}
//				}
//			}

			if (Physics.Raycast(intersectionRay, out raycastHit, speed * Time.deltaTime,~((1 << LayerMask.NameToLayer("Suppression"))) & ~((1 << LayerMask.NameToLayer("Weapons")))))
            {
                if (gameObjectToSpawnOnImpact != null)
                {
                    GameObject.Instantiate(gameObjectToSpawnOnImpact, gameObject.transform.position, Quaternion.identity);
                }
                //if (GameData.penetrationData.ContainsKey(raycastHit.collider.material)){

                //} else {

               // if (raycastHit.collider.gameObject.GetComponent<ControlHealth>())
               // {
				//	raycastHit.collider.gameObject.GetComponent<ControlHealth>().damage(damage,factionThatFiredShot,direction);
                //}

                if (audioClipToPlayOnDeath != null && audioClipToPlayOnDeath.Length > 0)
                {
                    AudioSource.PlayClipAtPoint(audioClipToPlayOnDeath[(int)UnityEngine.Random.Range(0, audioClipToPlayOnDeath.Length - 1)], gameObject.transform.position);
                }
                GameObject.Destroy(gameObject);
                //}

                Collider[] objectsInBlastRange = Physics.OverlapSphere(raycastHit.point, blastRadius);

				Collider[] suppressionSpheresInBlastRange = Physics.OverlapSphere(raycastHit.point, blastRadius,((1 << LayerMask.NameToLayer ("Suppression"))));

				for (int i =0; i<suppressionSpheresInBlastRange.Length; ++i){
					if (GameData.SuppressionSphereDictionary.ContainsKey(suppressionSpheresInBlastRange[i])){
						GameData.SuppressionSphereDictionary[suppressionSpheresInBlastRange[i]].suppress(damage,factionThatFiredShot,direction.normalized);
					}
				}

                for (int i = 0; i < objectsInBlastRange.Length; i++)
                {
                    //print (objectsInBlastRange[i].gameObject.name + " is in the blast range!");
					if (Physics.Raycast(raycastHit.point, (objectsInBlastRange[i].gameObject.transform.position - raycastHit.point).normalized, out raycastHit, blastRadius,~((1 << LayerMask.NameToLayer ("Suppression")))))
                    {
                        if (objectsInBlastRange[i].gameObject.GetComponent<ControlHealth>())
                        {
							objectsInBlastRange[i].gameObject.GetComponent<ControlHealth>().damage(Mathf.Clamp((damage / Mathf.Pow(Vector3.Distance(gameObject.transform.position, raycastHit.point), 2)),0,damage), factionThatFiredShot,direction);
                        }
                    }
                }
            }
            else
            {
                position = position + direction * speed * Time.deltaTime + Physics.gravity * Time.deltaTime * Time.deltaTime;
                gameObject.transform.position = gameObject.transform.position + direction * speed * Time.deltaTime + Physics.gravity * Time.deltaTime * Time.deltaTime;
                distanceCovered += speed * Time.deltaTime;
            }

            if (distanceCovered > maxDistanceToCover)
            {
                GameObject.Destroy(gameObject);
            }

        }
    }
}

