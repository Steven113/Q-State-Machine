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
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.AI;

namespace AssemblyCSharp
{
	[RequireComponent (typeof(ControlHealth))]
	public class QSoldier : QSensor
	{
		public static List<QSensor> allQSensors = new List<QSensor> ();
		//public static List<Camera> sensorCameras = new List<Camera>(); //we want to be able to switch between agents and cameras
		public static int currentAgent = 0;

		//public Transform transformToDoLosChecksFrom;

		public bool useCheatConditions = false;

		public float previousTargetHealth = 0;

		public UnityEngine.AI.NavMeshAgent agent;
		public QAgent qAgent;
		public List<string> currentActionSet = new List<string> ();
		public Weapon weapon;
		public ControlHealth healthController;
		public Collection<Vector3> path = new Collection<Vector3> ();

		public float reward = 0;
		public bool autoSetReward = true;

		public int smallFoodCount = 2;
		//for small heals
		public int bigFoundCount = 1;
		//for big heals

		public float ActionUpdateInterval = 0.5f;
		public float timeSinceLastActionUpdate = 1000f;

		//Data for controlling update rate
		public float AIUpdateInterval = 0.1f;
		public float timeSinceLastUpdate = 1000f;
		public float intervalForUpdatingCurrentTarget = 0.07f;
		public float timeSinceLastUpdatingCurrentTarget = 1000f;
		public float intervalForUpdatingPath = 0.2f;
		public float timeSinceLastUpdatingCurrentPath = 1000f;
		public float intervalForUpdatingAssaultPathOffset = 5f;
		public float timeSinceLastUpdatingAssaultPathOffset = 1000f;
		public float intervalForUpdatingSuppressionPathOffset = 5f;
		public float timeSinceLastUpdatingSuppressionPathOffset = 1000f;
		public float intervalForUpdatingFlankingPathOffset = 5f;
		public float timeSinceLastUpdatingFlankingPathOffset = 1000f;
		
		//Data related to LOS checks
		//public SoldierEntity thisEntity;
		public float verticalLOSDistance = 100;
		public float horizontalLOSDistance = 10000;
		public float maxRangeForEngagement = 300;
		public float verticalFOVAngle = 30f;
		public float horizontalFOVAngle = 60f;
		public float peripheralVisionDistance = 2f;
		public Transform transformToDoLOSChecksFrom;
		public float angleToDetermineWhetherTwoSoldiersAreLinedUp = 10f;
		Quaternion angleOfLookVector = Quaternion.identity;
		public RaycastHit hit;
		public RaycastHit hit2;
		public Ray LOSRay;
		public float amountOfTimeBeforePlayerIsConsideredLost = 5f;
		public float maxDistanceForUsingHighAccuracyLOSChecks = 100f;


		
		//Data related to combat behaviour
		public SoldierEntity currentTarget = null;
		public LOSResult currentTargetIsVisible = LOSResult.Invisible;
		public float amountOfTimeCurrentTargetHasBeenInLOS;
		public bool enableDebugPrint = false;
		public bool enableDebugVisualizations = false;
		public List<SoldierEntity> currentlyVisibleEnemies = new List<SoldierEntity> ();
		//public FactionName thisEntity.faction = FactionName.QHeirarchyArmy;
		public bool currentTargetHasChanged;
		public AISoldierState soldierState;
		public Vector3 previousEnemyPos;
		public LayerMask LOSCheckLayers;
		public float horizontalRotationRate = 90f;
		public float verticalRotationRate = 30f;
		public Vector3 lookDirection;
		//what direction is the agent looking?

		public float timeOfLastSuppression = 0;
		public float timeBeforeForgettingSuppression = 10f;
		public float weightingWhenMovingBackOntoPath = 0.1f;
		public float speed = 1f;
		public float sprintSpeed = 1.5f;
		bool friendlySoldierInLineOfFire;
		SoldierEntity thisEntity;
		float maxAngleOffsetForFiringShotsNotLinedUpWithBarrel = 10f;
		float amountOfTimePlayerMustBeVisibleBeforeShooting = 0.1f;

		public GameObject smallHealEffect;
		public GameObject bigHealEffect;

		public Collider suppressionSphere;

		double shuffleIndex = 0;

		[SerializeField]int numActionsBusyWith = 0;

		public BoxCollider mapBounds;

		NavMeshHit nav_hit;

		float [] expectedScores = new float[]{0,0};

		public override void Reward (float reward)
		{
			qAgent.RewardAgent (reward);
		}

		bool exploring = false;

		//inilization and update methods
		public void Awake ()
		{
			lookDirection = gameObject.transform.forward;
			agent = gameObject.GetComponent<UnityEngine.AI.NavMeshAgent> ();
			agent.updateRotation = false;
			allQSensors.Add (this);
			if (suppressionSphere != null) {
				GameData.SuppressionSphereDictionary.Add (suppressionSphere, healthController);
			}
			currentActionSet = new List<string> ();
			//GameData.addEntity(thisEntity)

		}

		public void Start ()
		{
			thisEntity = GetComponent<CreateSoldierEntity> ().entity;
		}

		public virtual void Update ()
		{
			//if (healthController.health > 0) {
			UpdateMovement ();
			UpdateOrientation ();
			UpdateFriendlyFireFlag ();
			timeSinceLastUpdatingCurrentPath += Time.deltaTime;
			timeSinceLastUpdate += Time.deltaTime;
			timeSinceLastActionUpdate += Time.deltaTime;

			if (GameData.scores [0] != expectedScores [0] || GameData.scores [1] != expectedScores [1]) {
				CurrentTarget = null;
				expectedScores [0] = GameData.scores [0];
				expectedScores [1] = GameData.scores [1];
			}

			if (autoSetReward) {
				reward += (healthController.health / healthController.maxhealth) * (Time.deltaTime);
				float scoreDiff = ((GameData.scores [(int)thisEntity.faction] - GameData.scores [((int)thisEntity.faction + 1) % GameData.scores.Length]) * Time.deltaTime) * 0.01f;
				//if (scoreDiff<0){
				reward += scoreDiff;
				//}

				if (autoSetReward && currentTargetIsVisible == LOSResult.Visible && weapon.stateOfWeapon == WeaponState.Reloading) {
					reward -= (Time.deltaTime * 10);
				}

				if (currentTarget != null && currentTarget.mainLOSCollider != null) {
					if (previousTargetHealth < currentTarget.controlHealth.health) {
						qAgent.RewardAgent (10);
						previousTargetHealth = currentTarget.controlHealth.maxhealth;
					}
					if (currentTarget.controlHealth.health < previousTargetHealth) {
						reward += (previousTargetHealth - currentTarget.controlHealth.health);
						previousTargetHealth = currentTarget.controlHealth.health;

					}
				}
			}

			if (timeSinceLastUpdate > AIUpdateInterval) {
				timeSinceLastUpdate %= AIUpdateInterval;
				UpdateLOSInfo ();

			}

			if (timeSinceLastActionUpdate > ActionUpdateInterval) {

				//if (!(weapon.stateOfWeapon == WeaponState.Reloading)) {
					timeSinceLastActionUpdate %= ActionUpdateInterval; // this statement gets moved here so that only if the next action is requested do we restart the timer before requesting an action
					if (autoSetReward) {
						qAgent.RewardAgent (reward); //we call the q agent reward directly, as we don't want to pointlessly add another function call to the stack by calling the reward method of this class
						reward = 0;
					}
					currentActionSet = qAgent.GetAction (getState (), getStateValues ());
					++shuffleIndex;
				//}
			}
			//thisEntity.faction
			//if (shuffleIndex % 2 == 0) {
			if (currentActionSet.Contains ("Shoot") && weapon.stateOfWeapon == WeaponState.Ready && (!useCheatConditions || (weapon.magazines.Count > 0 && weapon.magazines [weapon.currentMag] > 0))) {
				if (autoSetReward && (weapon.magazines.Count == 0 || weapon.magazines [weapon.currentMag] == 0)) {
					reward -= Time.deltaTime;
				} else if ((currentTarget != null && currentTarget.mainLOSCollider != null)) {
					reward += Time.deltaTime;
				}
				weapon.fire ((currentTarget == null || currentTarget.mainLOSCollider == null)?weapon.barrelEnd.position:((currentTarget.mainLOSCollider.gameObject.transform.position-weapon.barrelStart.position).normalized*(weapon.barrelEnd.position-weapon.barrelStart.position).magnitude) + weapon.barrelStart.position, weapon.barrelStart.position, thisEntity.faction, false);
			} else if (currentActionSet.Contains ("Reload") && weapon.stateOfWeapon != WeaponState.Reloading && (!useCheatConditions || (weapon.magazines.Count == 0 || weapon.magazines [weapon.currentMag] < weapon.magSize * 0.5f))) {
				StartCoroutine (Reload ());
			}
			if (currentActionSet.Contains ("Turn180")){
				gameObject.transform.Rotate (new Vector3 (0, 180, 0));
			}

//			} else {
//				if (currentActionSet.Contains ("SHOOT") && weapon.stateOfWeapon == WeaponState.Ready) {
//					weapon.fire(weapon.barrelEnd.position,weapon.barrelStart.position,thisEntity.faction,false);
//				} else if (currentActionSet.Contains ("RELOAD") && weapon.stateOfWeapon!=WeaponState.Reloading) {
//					StartCoroutine(Reload());
//				}
//			}

			if (currentActionSet.Contains ("BigHeal")) {
				heal (false);
			} else if (currentActionSet.Contains ("Heal")) {
				heal (true);
			}

			if (healthController.health <= 0) {
				Respawn ();
			}

			//AttackTarget ();

//			List<string> state = getState ();
//			String str = "";
//			for (int i = 0; i<state.Count; ++i) {
//				str = str + state[i] +" ";
//			}
//			Debug.Log(str);
			//}
		}

		public override List<float> getStateValues ()
		{
			List<float> data = new List<float> ();
			data.Add (healthController.health / healthController.maxhealth);
			data.Add ((weapon.magazines.Count>0)?(weapon.magazines [weapon.currentMag] / weapon.magSize):0);
			data.Add (currentlyVisibleEnemies.Count);
			data.Add (healthController.suppressionLevel);
			data.Add (smallFoodCount);
			data.Add (bigFoundCount);
			return data;
		}

		public override List<string> getState ()
		{
			List<string> result = new List<string> ();
			
//			if (weapon.weaponName == WeaponName.Rifle) {
//				result.Add ("RIFLE");
//			} else if (weapon.weaponName == WeaponName.Rocket_Launcher) {
//				result.Add ("ROCKET_LAUNCHER");
//			}
//
//			if (currentTargetIsVisible == LOSResult.Visible) {
//				result.Add ("ENEMY_VISIBLE");
//			}
//
//			if (weapon.stateOfWeapon == WeaponState.Reloading) {
//				result.Add ("RELOADING");
//			}
//
//			throw new NotImplementedException ();

			return result;
		}

		public virtual void UpdateLOSInfo ()
		{
			currentlyVisibleEnemies.Clear ();
//			if (fireTeamController != null) {
//				fireTeamController.removeVisibleEntities (currentlyVisibleEnemies);
//			}
			for (int i = 0; i < GameData.Factions.Count; i++) {
				if (GameData.Factions [i].FactionName != thisEntity.faction) {
					for (int j = 0; j < GameData.Factions [i].Soldiers.Count; j++) {
						LOSResult soldierIsVisible = LOSResult.Invisible;
						if (currentTarget == null || currentTarget != GameData.Factions [i].Soldiers [j]) {
							if (GameData.Factions [i].Soldiers [j] != null && GameData.Factions [i].Soldiers [j].mainLOSCollider != null &&
							    !GameData.Factions [i].Soldiers [j].isNeutral) {
								soldierIsVisible = isInLineOfSight (GameData.Factions [i].Soldiers [j]);
								if (soldierIsVisible == LOSResult.Visible) {
									if (!currentlyVisibleEnemies.Contains (GameData.Factions [i].Soldiers [j])) {
										currentlyVisibleEnemies.Add (GameData.Factions [i].Soldiers [j]);
									}
									if (currentTarget == null || currentTarget.mainLOSCollider == null ||
									    (Vector3.Distance (currentTarget.mainLOSCollider.transform.position, transformToDoLOSChecksFrom.position) >
									    Vector3.Distance (GameData.Factions [i].Soldiers [j].mainLOSCollider.transform.position, transformToDoLOSChecksFrom.position))) {
										
										if (currentTarget != null) {
											if (!currentlyVisibleEnemies.Contains (currentTarget)) {
												currentlyVisibleEnemies.Add (currentTarget);
											}
										}
										
										if (enableDebugPrint)
											Debug.Log ("Target spotted!");
										CurrentTarget = GameData.Factions [i].Soldiers [j];
										
										
										
									} else {
//										if (currentTarget != null) {
//											if (!currentlyVisibleEnemies.Contains (currentTarget)) {
//												currentlyVisibleEnemies.Add (currentTarget);
//											}
//										}
									}
									//  if (fireTeamController!=null)
									//  {
									// fireTeamController.ReportThreat(GameData.Factions[i].Soldiers[j]);
									// }
								}
							}
						}
					}
				}
			}
			
//			if (fireTeamController != null) {
//				fireTeamController.loadVisibleEntities (currentlyVisibleEnemies);
//			}
		}

		public virtual LOSResult isInLineOfSight (SoldierEntity entity)
		{

			Color randCol = new Color (UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);

			if (Vector3.Distance (entity.centreOfMass.position, transformToDoLOSChecksFrom.position) > horizontalLOSDistance || Mathf.Abs (Vector3.Angle (weapon.barrelEnd.position - weapon.barrelStart.position, entity.centreOfMass.position - transformToDoLOSChecksFrom.position)) > horizontalFOVAngle) {
				if (enableDebugPrint)
					Debug.Log ("Not withing FOV angle!");
				if (Vector3.Distance (entity.centreOfMass.position, transformToDoLOSChecksFrom.position) < peripheralVisionDistance) {
					return LOSResult.PeripheralVisionTriggered;
				} else {
					return LOSResult.Invisible;
				}
			} else {
				LOSRay.origin = transformToDoLOSChecksFrom.position;
				LOSRay.direction = entity.centreOfMass.position - transformToDoLOSChecksFrom.position;
				if (enableDebugVisualizations)
					Debug.DrawRay (LOSRay.origin, LOSRay.direction, randCol, 0.1f);
				
				if ((Physics.Raycast (LOSRay, out hit, Vector3.Distance (entity.centreOfMass.position, transformToDoLOSChecksFrom.position) + 10, LOSCheckLayers))) {
					if (hit.collider == entity.mainLOSCollider) {
						//if (enableDebugVisualizations) if (enableDebugVisualizations) Debug.DrawRay(transformToDoLOSChecksFrom.position,entity.centreOfMass.position - transformToDoLOSChecksFrom.position,Color.red,0.1f);
						if (Vector3.Distance (entity.centreOfMass.position, transformToDoLOSChecksFrom.position) < maxDistanceForUsingHighAccuracyLOSChecks && entity.LOSCheckColliders.Length > 0 && entity.LOSCheckTransforms.Length > 0) {
							for (int i = 0; i < entity.LOSCheckColliders.Length; i++) {
								LOSRay.direction = entity.LOSCheckTransforms [i].position - transformToDoLOSChecksFrom.position;
								//if (enableDebugVisualizations) if (enableDebugVisualizations) Debug.DrawRay(transformToDoLOSChecksFrom.position, entity.LOSCheckTransforms[i].position - transformToDoLOSChecksFrom.position,Color.red,0.1f);
								if ((Physics.Raycast (LOSRay, out hit, Vector3.Distance (entity.centreOfMass.position, transformToDoLOSChecksFrom.position) + 10, LOSCheckLayers))) {
									if (hit.collider.Equals (entity.LOSCheckColliders [i])) {
										//currentlyVisibleEnemies.Add(entity);
										return LOSResult.Visible;
									}
								}
							}
						} else {
							//currentlyVisibleEnemies.Add(entity);
							if (enableDebugPrint)
								Debug.Log ("High accuracy visibility checks skipped!");
							return LOSResult.Visible;
						}
					} else {
						for (int i = 0; i < entity.LOSCheckColliders.Length; i++) {
							if (hit.collider.gameObject.Equals (entity.LOSCheckColliders [i].gameObject)) {
								return LOSResult.Visible;
							}
						}
						
						
						if (enableDebugPrint)
							Debug.Log ("MainLOSColliderNotVisible! " + entity.mainLOSCollider.gameObject.transform.root.gameObject.name + ". It hit " + hit.collider.gameObject.transform.root.gameObject.name + " " + entity.LOSCheckColliders.Length);
						//if (enableDebugVisualizations) if (enableDebugVisualizations) Debug.DrawRay(transformToDoLOSChecksFrom.position,hit.point - transformToDoLOSChecksFrom.position,Color.green,0.1f);
						return LOSResult.Invisible;
					}
				}
				if (enableDebugPrint)
					Debug.Log ("Rays didn't hit anything!");
				return LOSResult.Invisible;
				//}
			}
		}

		public virtual void UpdateOrientation ()
		{
			Vector3 temp = Vector3.zero;
			if (currentTarget != null && currentTarget.mainLOSCollider != null && amountOfTimeCurrentTargetHasBeenInLOS > 0 /*&& (currentTarget.centreOfMass.position-transformToDoLOSChecksFrom.position).magnitude<lookDirection.magnitude*/) {
				temp = currentTarget.mainLOSCollider.transform.position;
				temp.y = gameObject.transform.position.y;
				Quaternion tempQ = Quaternion.LookRotation (temp - gameObject.transform.position);
				gameObject.transform.rotation = Quaternion.RotateTowards (gameObject.transform.rotation, tempQ, Time.deltaTime * horizontalRotationRate);
			} else if (currentlyVisibleEnemies.Count > 0 && currentlyVisibleEnemies [0].mainLOSCollider != null && (currentlyVisibleEnemies [0].centreOfMass.position - transformToDoLOSChecksFrom.position).magnitude < lookDirection.magnitude) {
				temp = currentlyVisibleEnemies [0].mainLOSCollider.transform.position;
				temp.y = gameObject.transform.position.y;
				Quaternion tempQ = Quaternion.LookRotation (temp - gameObject.transform.position);
				gameObject.transform.rotation = Quaternion.RotateTowards (gameObject.transform.rotation, tempQ, Time.deltaTime * horizontalRotationRate);
			} else { /*if (agent.hasPath)*/
				if ((Time.time - timeOfLastSuppression) > timeBeforeForgettingSuppression) {
					if (agent.hasPath) {
						lookDirection = weightingWhenMovingBackOntoPath * Time.deltaTime * (
						    agent.steeringTarget - agent.transform.position).normalized + (1 - weightingWhenMovingBackOntoPath * Time.deltaTime) * lookDirection;
					} else {
						lookDirection = weightingWhenMovingBackOntoPath * Time.deltaTime * (gameObject.transform.forward) + (1 - weightingWhenMovingBackOntoPath * Time.deltaTime) * lookDirection;
					}
				}
				temp = agent.transform.position + ((lookDirection != Vector3.zero) ? lookDirection : gameObject.transform.forward);
				temp.y = gameObject.transform.position.y;
				Quaternion tempQ = Quaternion.LookRotation (temp - gameObject.transform.position);
				gameObject.transform.rotation = Quaternion.RotateTowards (gameObject.transform.rotation, tempQ, Time.deltaTime * horizontalRotationRate);
			}


		}

		public SoldierEntity CurrentTarget {
			get {
				if (currentTarget != null && currentTarget.mainLOSCollider == null) {
					return null;
				}
				return currentTarget;
			}
			set {
				//rotationAmountWhenSearching *= -1;
				//pointToDoRandomizationFrom = UnityEngine.Random.Range (0, 1000);
			
				if (currentTarget != null && currentTarget.mainLOSCollider != null) {
					if (enableDebugPrint)
						Debug.Log ("Retrieving intel from " + thisEntity.faction.ToString () + " about target from " + currentTarget.faction.ToString ());
					Debug.Assert (GameData.getFaction (thisEntity.faction) != null);
					GameData.getFaction (thisEntity.faction).getSoldierIntel (currentTarget).numSoldiersAssignedToThisTarget -= 1;
				}
			
				//if (currentTarget!=value){
				//	waypoints.Clear();
				//}
			
				amountOfTimeCurrentTargetHasBeenInLOS = intervalForUpdatingCurrentTarget * 2;
			
				if (value != null && value != currentTarget && value.mainLOSCollider != null) {
//				if (fireTeamController!=null && !radioSilence && !currentlyVisibleEnemies.Contains(value) && Vector3.Distance(value.centreOfMass.position,gameObject.transform.position)<maxRangeForEngagement){
//					fireTeamController.PlayEventSound(RadioEventType.PlayerSpotted,soldierAudioSource);
//				}
					currentTargetHasChanged = true;
					previousTargetHealth = value.controlHealth.health;
				}
			
				currentTarget = value;
			
				if (currentTarget != null && currentTarget.mainLOSCollider != null) {
					if (enableDebugPrint)
						Debug.Log ("Setting target to " + currentTarget.mainLOSCollider.gameObject.name + " for " + gameObject.transform.position);
					soldierState = AISoldierState.EngagingTarget;
					GameData.getFaction (thisEntity.faction).getSoldierIntel (currentTarget).numSoldiersAssignedToThisTarget += 1;
					previousEnemyPos = currentTarget.centreOfMass.position;
				
				} else {
					soldierState = AISoldierState.Idle;
					amountOfTimeCurrentTargetHasBeenInLOS = -1000;
				
				
				}
			
			
			
			}
		}

		public void UpdateMovement ()
		{
			
			if (timeSinceLastUpdatingCurrentPath > intervalForUpdatingPath) {
				timeSinceLastUpdatingCurrentPath %= intervalForUpdatingPath;
				if (currentActionSet != null) {
					if (currentTarget == null || currentTarget.mainLOSCollider == null) {
						if (currentActionSet.Contains ("Explore") && !exploring) {
							exploring = true;
							//origin for search
							Vector3 scaledBounds = mapBounds.gameObject.transform.TransformPoint(mapBounds.size);
							Vector3 searchPoint = new Vector3 (mapBounds.gameObject.transform.position.x + (UnityEngine.Random.value - 0.5f )* 2 * scaledBounds.x, mapBounds.gameObject.transform.position.y + (UnityEngine.Random.value - 0.5f )* 2 * scaledBounds.y, mapBounds.gameObject.transform.position.z + (UnityEngine.Random.value - 0.5f )* 2 * scaledBounds.z);

							//Debug.DrawRay (searchPoint, Vector3.up*100, Color.red, 30f);

							NavMesh.SamplePosition (searchPoint, out nav_hit, float.PositiveInfinity, agent.areaMask);

							agent.SetDestination (nav_hit.position);

						} else if (currentActionSet.Contains ("GoToCover")) {
							exploring = false;
							NavMesh.FindClosestEdge (agent.transform.position, out nav_hit, agent.areaMask);
							agent.SetDestination (nav_hit.position);
						}
					} else {
						if (currentActionSet.Contains ("Flee")) {
							exploring = false;
							//if (currentTarget != null) {
							Vector3 searchPoint = new Vector3 (mapBounds.center.x + (UnityEngine.Random.value - 0.5f )* 2 * mapBounds.size.x, mapBounds.center.y + (UnityEngine.Random.value - 0.5f )* 2 * mapBounds.size.y, mapBounds.center.z + (UnityEngine.Random.value - 0.5f )* 2 * mapBounds.size.z);

								if (Vector3.Dot (searchPoint - agent.transform.position, currentTarget.mainLOSCollider.transform.position - agent.transform.position) > 0) {
									searchPoint = -searchPoint;
								}

								NavMesh.SamplePosition (searchPoint, out nav_hit, float.PositiveInfinity, agent.areaMask);

								agent.SetDestination (nav_hit.position);
						} else if (currentActionSet.Contains ("MoveTo") && currentTarget != null && currentTarget.centreOfMass != null) {
							exploring = false;
							agent.SetDestination (currentTarget.centreOfMass.position);
							//AIGrid.findPath (this, gameObject.transform.position, currentTarget.centreOfMass.position, out path, false, false);
						}
					}
				}

			}
//			else {
//				if (CurrentTarget.mainLOSCollider!=null){
//					//TO-DO
//				} else {
//					AIGrid.findExplorationPath(this,gameObject.transform.position,out path, 5);
//					//Debug.Log(path.Count);
//				}
//			}


		}

		public IEnumerator Reload ()
		{
//			if (autoSetReward) {
//				reward += (1f - ((weapon.magazines.Count > 0 ? weapon.magazines [weapon.currentMag] : 0) / weapon.magSize)); // we give reward based on how many bullets are in mag, to reward reloads where the mag is empty or near empty
//				reward -= 0.5f;
//			}
			++numActionsBusyWith;
			weapon.stateOfWeapon = WeaponState.Reloading;
			yield return new WaitForSeconds (weapon.reloadTime);
			--numActionsBusyWith;
			weapon.stateOfWeapon = WeaponState.Ready;
			weapon.InsertNewMag ();


			//animationObject.CrossFade(avatarAlias+"|"+weaponName.ToString()+" Idle");
		}

		public virtual void AttackTarget ()
		{
			
			SoldierEntity temp = null;
			if (currentTarget != null && currentTarget.mainLOSCollider != null /*&& amountOfTimeCurrentTargetHasBeenInLOS >= fireDelayTime*/) {
				temp = currentTarget;
			} else if (currentlyVisibleEnemies.Count > 0) {
				temp = currentlyVisibleEnemies [0];
			}
			
			
			if (temp != null) {
				//code for aiming towards player goes here
				
				//code for firing
				
				//if (enableDebugVisualizations) if (enableDebugVisualizations) Debug.DrawRay(barrelStart.position,barrelEnd.position - barrelStart.position,Color.red,0.1f);
				if (enableDebugVisualizations)
				if (enableDebugVisualizations)
					Debug.DrawRay (weapon.barrelStart.position, temp.centreOfMass.position - weapon.barrelStart.position, Color.blue, 0.1f);
				if (enableDebugPrint)
					Debug.Log ("Vector3.Angle(weaponList[currentWeapon].barrelEnd.position - weaponList[currentWeapon].barrelStart.position,temp.centreOfMass.position - weaponList[currentWeapon].barrelStart.position) " + Vector3.Angle (weapon.barrelEnd.position - weapon.barrelStart.position, temp.centreOfMass.position - weapon.barrelStart.position));
				if (weapon.stateOfWeapon == WeaponState.Ready && !friendlySoldierInLineOfFire && Vector3.Distance (temp.centreOfMass.position, transformToDoLOSChecksFrom.position) < maxRangeForEngagement && maxAngleOffsetForFiringShotsNotLinedUpWithBarrel > Mathf.Abs (Vector3.Angle (weapon.barrelEnd.position - weapon.barrelStart.position, temp.centreOfMass.position - weapon.barrelStart.position)) && (currentTargetIsVisible == LOSResult.Visible || amountOfTimeCurrentTargetHasBeenInLOS < amountOfTimePlayerMustBeVisibleBeforeShooting)) {
					if (weapon.fire (weapon.barrelEnd, weapon.barrelStart, thisEntity.faction, false)) {
						//animationObject.CrossFade(avatarAlias+"|"+weaponName.ToString()+" Reload");
						//weapon.stateOfWeapon = WeaponState.Reloading;
					}// else {
					
					//}
				} else {
					if (enableDebugPrint)
						Debug.Log ("Target not in path of weapon!");
				}
			} else {
				if (enableDebugPrint)
					Debug.Log ("No target");
			}
		}

		protected virtual void UpdateFriendlyFireFlag ()
		{
			friendlySoldierInLineOfFire = false;
			SoldierEntity temp = null;
			if (currentTarget != null && currentTarget.mainLOSCollider != null /*&& amountOfTimeCurrentTargetHasBeenInLOS > weapon.fireDelayTime*/) {
				temp = currentTarget;
			} else if (currentlyVisibleEnemies.Count > 0) {
				temp = currentlyVisibleEnemies [0];
			}
			//			if (fireTeamController != null) {
			//				for (int i= 0; i<fireTeamController.fireTeamMembers.Count; i++) {
			//					LOSRay.direction = weapon.barrelEnd.position - weapon.barrelStart.position;
			//					LOSRay.origin = weapon.barrelStart.position;
			//					if (fireTeamController.fireTeamMembers [i] != this && fireTeamController.fireTeamMembers [i].thisEntity.mainLOSCollider.Raycast (LOSRay, out hit, float.PositiveInfinity) && Vector3.Distance (hit.point, weapon.barrelStart.position) < Vector3.Distance (temp.centreOfMass.position, weapon.barrelStart.position)) {
			//						if (enableDebugPrint) Debug.Log ("Friendly soldier in the way!");
			//						friendlySoldierInLineOfFire = true;
			//						break;
			//					}
			//				}
			//			}
			if (temp != null && temp.centreOfMass != null) {
				for (int i = 0; i < GameData.getFaction (thisEntity.faction).Soldiers.Count; ++i) {
					LOSRay.direction = weapon.barrelEnd.position - weapon.barrelStart.position;
					LOSRay.origin = weapon.barrelStart.position;
					if (GameData.getFaction (thisEntity.faction).Soldiers [i] != thisEntity
					    && GameData.getFaction (thisEntity.faction).Soldiers [i].mainLOSCollider.Raycast (LOSRay, out hit, float.PositiveInfinity) && Vector3.Distance (hit.point, weapon.barrelStart.position) < Vector3.Distance (temp.centreOfMass.position, weapon.barrelStart.position)) {
						if (enableDebugPrint)
							Debug.Log ("Friendly soldier in the way!");
						friendlySoldierInLineOfFire = true;
						break;
					}
				}
			}
			
		}

		public void heal (bool useSmallHeal)
		{
			float initialHealth = healthController.health;
			if (useSmallHeal && smallFoodCount > 0) {
				--smallFoodCount;
				healthController.heal (healthController.maxhealth * 0.25f);
				GameObject.Instantiate (smallHealEffect, gameObject.transform.position, Quaternion.identity);
			} else if (!useSmallHeal && bigFoundCount > 0) {
				--bigFoundCount;
				healthController.heal (healthController.maxhealth * 0.5f);
				GameObject.Instantiate (bigHealEffect, gameObject.transform.position, Quaternion.identity);
			}
			if (autoSetReward)
				reward += ((healthController.health - initialHealth) / healthController.maxhealth); //give reward based on change in health
		}

		public virtual void OnDestroy ()
		{
			allQSensors.Remove (this); //we can be sure that this is in the list, since we add it to the list upon initialisation
			GameData.SuppressionSphereDictionary.Remove (suppressionSphere);
		}

		public void Respawn ()
		{
			//qAgent.RewardAgent (-10);
			//reward = 0f;
			if (autoSetReward) {
				reward -= 10;
			}
			Vector3 spawnPoint = new Vector3 (1, 0, 1);
			++GameData.scores [((int)(thisEntity.faction) + 1) % GameData.scores.Length];
			//Debug.Log (thisEntity.faction.ToString () + " " + (((int)(thisEntity.faction) + 1) % GameData.scores.Length));
			//Debug.Assert (AIGrid.findFleeingPoint (this, gameObject.transform.position, out spawnPoint));
			//Debug.Log ("Spawning at " + spawnPoint);
			agent.Warp ((QTournamentController.g_SpawnPoints[UnityEngine.Random.Range(0,QTournamentController.g_SpawnPoints.Length)].transform.position));
			healthController.health = healthController.maxhealth;
			CurrentTarget = null;
		}

		public override int GetBusyWithAction ()
		{
			return numActionsBusyWith;
		}
	}
}

