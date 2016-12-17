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
    public class RocketProjectileScript : ProjectileScript
    {
        public float blastRadius = 5;

        //raycastHit raycastHit;

        public AudioSource blastSoundSource;

        new void Update()
        {
            //Check if rocket passes through AI suppression sphere
            intersectionRay.origin = gameObject.transform.position;
            intersectionRay.direction = direction.normalized;

			RaycastHit [] suppressionSpheresHit;
			suppressionSpheresHit = (Physics.RaycastAll (intersectionRay, speed * Time.deltaTime, ((1 << LayerMask.NameToLayer ("Suppression")))));
//			if (suppressionSpheresHit != null && suppressionSpheresHit.Length > 0) {
//				for (int i = 0; i<suppressionSpheresHit.Length; i++) {
//					if (GameData.SuppressionSphereDictionary.ContainsKey (suppressionSpheresHit [i].collider)) {
//						if (GameData.SuppressionSphereDictionary [suppressionSpheresHit [i].collider] is PlayerController) {
//							//((PlayerController)GameData.SuppressionSphereDictionary [suppressionSpheresHit [i].collider]).suppress (damage);
//						} else if (GameData.SuppressionSphereDictionary [suppressionSpheresHit [i].collider] is AIController) {
//							((AIController)GameData.SuppressionSphereDictionary [suppressionSpheresHit [i].collider]).suppress ((damage),factionThatFiredShot, direction);
//						}
//					}
//				}
//			}
            //Debug.DrawRay (intersectionRay.origin, intersectionRay.direction*speed * Time.deltaTime,Color.red,5);
            //check if rocket has hit something that is not a suppression collider or a weapon object
            if (Physics.Raycast(intersectionRay, out raycastHit, speed * Time.deltaTime, ~((1 << LayerMask.NameToLayer("Suppression"))) & ~((1 << LayerMask.NameToLayer("Weapons")))))
            {

                // if (raycastHit.point!=null && raycastHit.distance!=null){
                //print("Rocket hits " + raycastHit.collider.gameObject.name + intersectionRay.direction);
                // }
                //if (GameData.penetrationData.ContainsKey(raycastHit.collider.material)){
                // if ((raycastHit.collider!=null)){
                // print (raycastHit.collider.name);
                // }
                if (gameObjectToSpawnOnImpact != null)
                {
                    GameObject.Instantiate(gameObjectToSpawnOnImpact, gameObject.transform.position + intersectionRay.direction * raycastHit.distance, gameObject.transform.rotation);
                }
                //Check if rocket explosion overlaps suppression spheres of the AI, so that explosion suppresses them too
                Collider[] suppressionSpheresInBlastRange = Physics.OverlapSphere(raycastHit.point, blastRadius,((1 << LayerMask.NameToLayer ("Suppression"))));

				for (int i =0; i<suppressionSpheresInBlastRange.Length; ++i){
					if (GameData.SuppressionSphereDictionary.ContainsKey(suppressionSpheresInBlastRange[i])){
						GameData.SuppressionSphereDictionary[suppressionSpheresInBlastRange[i]].suppress(damage,factionThatFiredShot,direction.normalized);
					}
				}
                //get physical objects in blast range i.e. players, boxes, buildings
                Collider[] objectsInBlastRange = Physics.OverlapSphere(raycastHit.point, blastRadius);

                for (int i = 0; i < objectsInBlastRange.Length; i++)
                {
                    // print(objectsInBlastRange[i].gameObject.name + " is in the blast range!");
                    //do raycast to check that there is no obstacle between explosion and object in blast range. If no obstruction, do blast damage that is inverse of square of distance between object and explosion point
                    if (Physics.Raycast(raycastHit.point, (objectsInBlastRange[i].gameObject.transform.position - raycastHit.point).normalized, out raycastHit, blastRadius,~((1 << LayerMask.NameToLayer("Suppression"))) & ~((1 << LayerMask.NameToLayer("Weapons")))))
                    {
                        if (objectsInBlastRange[i].gameObject.GetComponent<ControlHealth>())
                        {
							objectsInBlastRange[i].gameObject.GetComponent<ControlHealth>().damage(Mathf.Clamp(damage / Mathf.Pow(Vector3.Distance(gameObject.transform.position, raycastHit.point), 2),0,damage), factionThatFiredShot,(objectsInBlastRange[i].gameObject.transform.position - raycastHit.point));
                        }
                    }
                }
                //position = position + direction * speed * Time.deltaTime;
                gameObject.transform.position = raycastHit.point;
                //distanceCovered += speed * Time.deltaTime;
                //} else {

                //if (blastSoundSource != null && audioClipToPlayOnDeath != null && audioClipToPlayOnDeath.Length > 0)
                //{
                //    blastSoundSource.PlayOneShot(audioClipToPlayOnDeath[(int)UnityEngine.Random.Range(0, audioClipToPlayOnDeath.Length - 1)]);
                //}
                if (audioClipToPlayOnDeath != null && audioClipToPlayOnDeath.Length > 0)
                {
                    AudioSource.PlayClipAtPoint(audioClipToPlayOnDeath[(int)UnityEngine.Random.Range(0, audioClipToPlayOnDeath.Length - 1)], gameObject.transform.position);
                }

                GameObject.Destroy(gameObject);
                //}
            }
            else
            {
                position = position + direction * speed * Time.deltaTime;
                gameObject.transform.position = gameObject.transform.position + direction * speed * Time.deltaTime;
                distanceCovered += speed * Time.deltaTime;
            }

            if (distanceCovered > maxDistanceToCover)
            {
                GameObject.Destroy(gameObject);
            }

        }

        public void OnDestroy()
        {
            //throw new NullReferenceException();
        }
    }


}