using UnityEngine;
using System.Collections;
using AssemblyCSharp;

public class ProjectileScript : MonoBehaviour {

	// Use this for initialization
	public Vector3 direction; //unit vector of direction that projectile is going
	public float speed;
	public Ray intersectionRay;
	public RaycastHit raycastHit;
	public Vector3 position; // position in world coordinates
	public float distanceCovered = 0;
	public float maxDistanceToCover = 1000000;
	public float damage = 1;
	public GameObject gameObjectToSpawnOnImpact;
	public AudioClip audioClipToPlayOnSpawn;
	public AudioClip[] audioClipToPlayOnDeath;
	public FactionName factionThatFiredShot = FactionName.QHeirarchyArmy;
	public float airStabilisation = 0.25f; //a number between zero and 1, indicating how much the projectile is affected by gravity. Smaller z means the projectile falls more slowly
	public float timeSinceLastUpdate = 1000f;
	public float projectileUpdateInterval = 0.05f;
    public bool hasImpacted = false;

	void Start () {
		//position = gameObject.transform.TransformPoint(gameObject.transform.position);
		position = gameObject.transform.position;
		intersectionRay = new Ray (position, direction);
		raycastHit = new RaycastHit();
		if (audioClipToPlayOnSpawn != null) {
			AudioSource.PlayClipAtPoint(audioClipToPlayOnSpawn,gameObject.transform.position);
		}
		timeSinceLastUpdate = projectileUpdateInterval * 2;
	}
	
	// Update is called once per frame
	public void Update () {
		timeSinceLastUpdate += Time.deltaTime;
		//set ray to point to where bullet will be at end of current frame
		direction = ((gameObject.transform.position + speed * direction * Time.deltaTime + Physics.gravity * airStabilisation * Time.deltaTime * Time.deltaTime) - gameObject.transform.position).normalized;
		intersectionRay.origin = gameObject.transform.position;
		intersectionRay.direction = direction;
		if (timeSinceLastUpdate>projectileUpdateInterval){
			//timeSinceLastUpdate=0;
		if (Physics.Raycast (intersectionRay, out raycastHit, speed * Time.deltaTime, ~((1 << LayerMask.NameToLayer ("Suppression"))))) { // The projectile provides the support for influencing suppression colliders (i.e. to detect near misses), thus it ignores suppression colldiers during the first raycast since the first raycast is for direct impacts
               
                //timeSinceLastUpdate = 0f;
			//print ("The projectile hit!" + raycastHit.collider.gameObject.name);
			if (gameObjectToSpawnOnImpact != null) {
				GameObject.Instantiate (gameObjectToSpawnOnImpact, gameObject.transform.position, Quaternion.identity);
			}
			//if (raycastHit.collider.material != null && GameData.penetrationData.ContainsKey (raycastHit.collider.material)) {
					//
			//} else {


                if (raycastHit.collider.gameObject.GetComponent<ControlHealth>() != null &&
                    (!raycastHit.collider.gameObject.Equals(gameObject)) && !hasImpacted)
                {
                    hasImpacted = true;
                    raycastHit.collider.gameObject.GetComponent<ControlHealth> ().damage (damage,factionThatFiredShot,direction);
				}
                else if (raycastHit.collider.gameObject.GetComponentInParent<ControlHealth>() != null && !hasImpacted)
                {
                    hasImpacted = true;
                    raycastHit.collider.gameObject.GetComponentInParent<ControlHealth>().damage(damage, factionThatFiredShot, direction);
                }

                GameObject.Destroy (gameObject);
			//}
		} else { //if no hit, update projectile
			position = position + direction * speed * Time.deltaTime;
			gameObject.transform.position = gameObject.transform.position + direction * speed * Time.deltaTime;
			distanceCovered += speed * Time.deltaTime;
			raycastHit.point = position;
		}

		//Find all suppression spheres and apply suppression (currently unused)
		RaycastHit [] suppressionSpheresHit;
		suppressionSpheresHit = (Physics.RaycastAll (intersectionRay, speed * Time.deltaTime, ((1 << LayerMask.NameToLayer ("Suppression")))));
		if (suppressionSpheresHit != null && suppressionSpheresHit.Length > 0) {
			for (int i = 0; i<suppressionSpheresHit.Length; i++) {
				if (GameData.SuppressionSphereDictionary.ContainsKey (suppressionSpheresHit [i].collider)) {
						GameData.SuppressionSphereDictionary [suppressionSpheresHit [i].collider].suppress (damage,factionThatFiredShot, direction);
					//if (GameData.SuppressionSphereDictionary [suppressionSpheresHit [i].collider] is PlayerController) {
						//((PlayerController)GameData.SuppressionSphereDictionary [suppressionSpheresHit [i].collider]).suppress (damage);
					//} else if (GameData.SuppressionSphereDictionary [suppressionSpheresHit [i].collider] is AIController) {
					//	((AIController)GameData.SuppressionSphereDictionary [suppressionSpheresHit [i].collider]).suppress (damage,factionThatFiredShot, direction);
					//}
				}
			}
		}
		} else {
			position = position + direction /** speed * Time.deltaTime*/;
			gameObject.transform.position = gameObject.transform.position + direction /** speed * Time.deltaTime*/;
			distanceCovered += speed * Time.deltaTime;
			raycastHit.point = position;
		}

		//destroy bullet if it travels far enough to definately be beyond map boundaries
		if (distanceCovered > maxDistanceToCover) {
			GameObject.Destroy(gameObject);
		}

	}
}
