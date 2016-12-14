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


namespace AssemblyCSharp
{
	[RequireComponent(typeof(ControlHealth))]
	public class QSoldier : QSensor
	{
		public Transform transformToDoLosChecksFrom;

		public Weapon weapon;

		public ControlHealth healthController;

		public Collection<Vector3> path = new Collection<Vector3>();

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

		List<SoldierEntity> currentlyVisibleEnemies = new List<SoldierEntity> ();

		public FactionName factionName = FactionName.QHeirarchyArmy;

		public bool currentTargetHasChanged;

		public AISoldierState soldierState;

		public Vector3 previousEnemyPos;

		public LayerMask LOSCheckLayers;

		public float horizontalRotationRate = 90f;
		public float verticalRotationRate = 30f;

		public Vector3 lookDirection; //what direction is the agent looking?

		public float timeOfLastSuppression = 0;
		public float timeBeforeForgettingSuppression = 10f;

		public float weightingWhenMovingBackOntoPath = 0.1f;

		public float speed = 1f;
		public float sprintSpeed = 1.5f;

		//inilization and update methods
		public void Awake(){
			lookDirection = gameObject.transform.forward;
			//GameData.addEntity(thisEntity)
		}

		public virtual void Update(){
			UpdateMovement ();
			UpdateOrientation ();

			timeSinceLastUpdate += Time.deltaTime;
			if (timeSinceLastUpdate > AIUpdateInterval) {
				timeSinceLastUpdate%=AIUpdateInterval;
				UpdateLOSInfo();
			}
		}

		public override List<string> getState(){
			return new List<string>();
		}

		public virtual void UpdateLOSInfo ()
		{
			currentlyVisibleEnemies.Clear ();
//			if (fireTeamController != null) {
//				fireTeamController.removeVisibleEntities (currentlyVisibleEnemies);
//			}
			for (int i = 0; i<GameData.Factions.Count; i++) {
				if (GameData.Factions [i].FactionName != factionName) {
					for (int j = 0; j<GameData.Factions[i].Soldiers.Count; j++) {
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

			if (Vector3.Distance (entity.centreOfMass.position, transformToDoLOSChecksFrom.position) > horizontalLOSDistance || Mathf.Abs (Vector3.Angle (gameObject.transform.forward, entity.centreOfMass.position - transformToDoLOSChecksFrom.position)) > horizontalFOVAngle) {
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
						if ( Vector3.Distance (entity.centreOfMass.position, transformToDoLOSChecksFrom.position) < maxDistanceForUsingHighAccuracyLOSChecks && entity.LOSCheckColliders.Length > 0 && entity.LOSCheckTransforms.Length > 0) {
							for (int i = 0; i<entity.LOSCheckColliders.Length; i++) {
								LOSRay.direction = entity.LOSCheckTransforms [i].position - transformToDoLOSChecksFrom.position;
								//if (enableDebugVisualizations) if (enableDebugVisualizations) Debug.DrawRay(transformToDoLOSChecksFrom.position, entity.LOSCheckTransforms[i].position - transformToDoLOSChecksFrom.position,Color.red,0.1f);
								if ((Physics.Raycast (LOSRay, out hit, Vector3.Distance (entity.centreOfMass.position, transformToDoLOSChecksFrom.position) + 10, LOSCheckLayers))) {
									if (hit.collider.Equals(entity.LOSCheckColliders [i])) {
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
						for (int i = 0; i<entity.LOSCheckColliders.Length; i++) {
							if (hit.collider.gameObject.Equals(entity.LOSCheckColliders [i].gameObject)) {
								return LOSResult.Visible;
							}
						}
						
						
						if (enableDebugPrint)
							Debug.Log ("MainLOSColliderNotVisible! " + entity.mainLOSCollider.gameObject.transform.root.gameObject.name + ". It hit "+hit.collider.gameObject.transform.root.gameObject.name + " " + entity.LOSCheckColliders.Length);
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
					if (path.Count>0) {
						lookDirection = weightingWhenMovingBackOntoPath * Time.deltaTime * (
							path[path.Count-1] - gameObject.transform.position) + (1 - weightingWhenMovingBackOntoPath * Time.deltaTime) * lookDirection;
					} else {
						lookDirection = weightingWhenMovingBackOntoPath * Time.deltaTime * (transformToDoLosChecksFrom.forward) + (1 - weightingWhenMovingBackOntoPath * Time.deltaTime) * lookDirection;
					}
				}
				temp = gameObject.transform.position + ((lookDirection != Vector3.zero) ? lookDirection : gameObject.transform.forward);
				temp.y = gameObject.transform.position.y;
				Quaternion tempQ = Quaternion.LookRotation (temp - gameObject.transform.position);
				gameObject.transform.rotation = Quaternion.RotateTowards (gameObject.transform.rotation, tempQ, Time.deltaTime * horizontalRotationRate);
			}


		}

		public SoldierEntity CurrentTarget {
		get {
			return currentTarget;
		}
		set {
			//rotationAmountWhenSearching *= -1;
			//pointToDoRandomizationFrom = UnityEngine.Random.Range (0, 1000);
			
			if (currentTarget != null && currentTarget.mainLOSCollider != null) {
					if (enableDebugPrint)
					Debug.Log ("Retrieving intel from " + factionName.ToString () + " about target from " + currentTarget.faction.ToString ());
				Debug.Assert (GameData.getFaction (factionName) != null);
				GameData.getFaction (factionName).getSoldierIntel (currentTarget).numSoldiersAssignedToThisTarget -= 1;
			}
			
			//if (currentTarget!=value){
			//	waypoints.Clear();
			//}
			
				amountOfTimeCurrentTargetHasBeenInLOS = intervalForUpdatingCurrentTarget * 2;
			
			if (value != null && value != currentTarget && value.mainLOSCollider!=null) {
//				if (fireTeamController!=null && !radioSilence && !currentlyVisibleEnemies.Contains(value) && Vector3.Distance(value.centreOfMass.position,gameObject.transform.position)<maxRangeForEngagement){
//					fireTeamController.PlayEventSound(RadioEventType.PlayerSpotted,soldierAudioSource);
//				}
					currentTargetHasChanged = true;
			}
			
			currentTarget = value;
			
			if (currentTarget != null && currentTarget.mainLOSCollider!=null) {
				if (enableDebugPrint) Debug.Log("Setting target to "+currentTarget.mainLOSCollider.gameObject.name + " for "+gameObject.transform.position);
					soldierState = AISoldierState.EngagingTarget;
				GameData.getFaction (factionName).getSoldierIntel (currentTarget).numSoldiersAssignedToThisTarget += 1;
					previousEnemyPos = currentTarget.centreOfMass.position;
				
			} else {
				soldierState = AISoldierState.Idle;
				amountOfTimeCurrentTargetHasBeenInLOS = -1000;
				
				
			}
			
			
			
		}
	}

		public void UpdateMovement(){
			if (path.Count > 0) {
				gameObject.transform.position = Vector3.MoveTowards (gameObject.transform.position, path [path.Count - 1], speed*Time.deltaTime);
				//Debug.Log( path [path.Count - 1]);
				if (Vector3.Distance (gameObject.transform.position, path [path.Count - 1]) < 0.01f * AIGrid.s_cellWidth) {
					path.RemoveAt(path.Count-1);
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
			weapon.stateOfWeapon = WeaponState.Reloading;
			yield return new WaitForSeconds (weapon.reloadTime);
			weapon.stateOfWeapon = WeaponState.Ready;
			weapon.InsertNewMag ();
			//animationObject.CrossFade(avatarAlias+"|"+weaponList [currentWeapon].weaponName.ToString()+" Idle");
		}
}
}

