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
using System.Collections.Generic;
using AI;


namespace Weapons
{

	[Serializable]
	public class WeaponItem
	{
		public WeaponName weaponName;
		public Transform barrelStart;
		public Transform barrelEnd;
		public GameObject weaponGameObject;
		public Transform aimedInTransform;
		public Transform aimedInFov;
		public List<GameObject> gameObjectsThatArePartOfModel;
		public Weapon weapon;
		public bool enabled = true;
		public WeaponFireTypes weaponFireType = WeaponFireTypes.Normal;
		//public bool isBeingRaised = false;
		bool isAimedDown;
		bool visible;
		public float xAngleRotationFactor = 0;
		public float yAngleRotationFactor = 0;
		public AudioSource fireSoundSource;
		public AudioClip fireSound;
		public AudioClip reloadStartClip;
		public float reloadStartClipTime;
		public AudioClip reloadEndClip;
		public float reloadEndClipTime;
		float timeWhenLastSoundWasRemoved = 0;
		public int maxNumSounds = 10;
		public int numSounds = 0; //num fire sounds playing at a given point in time
		public float soundRemovalInterval = 0.1f;
		public float standardDistanceBetweenGunTransformAndCamera = 0;
		public float fireDelayTime = 0.1f;
		public List<MeshRenderer> meshRenderersOfWeapon = new List<MeshRenderer>();

		public WeaponItem ()
		{
			gameObjectsThatArePartOfModel = new List<GameObject> ();
			this.disableWeaponVisibility ();
			if (weaponGameObject != null) {
				if (weaponGameObject.GetComponent<MeshRenderer> () != null) {
					meshRenderersOfWeapon.Add (weaponGameObject.GetComponent<MeshRenderer> ());

				}
			}

			for (int i = 0; i<gameObjectsThatArePartOfModel.Count; i++) {
				if (gameObjectsThatArePartOfModel[i]!=null){
					if (gameObjectsThatArePartOfModel[i].GetComponent<MeshRenderer>()!=null){
						meshRenderersOfWeapon.Add (gameObjectsThatArePartOfModel[i].GetComponent<MeshRenderer> ());
					}
				}
			}

		}

		public void disableWeaponVisibility(){
			for (int i = 0; i<meshRenderersOfWeapon.Count; ++i) {
				meshRenderersOfWeapon[i].enabled = false;
			}
			visible = false;
		}

		public void enableWeaponVisibility(){
			for (int i = 0; i<meshRenderersOfWeapon.Count; ++i) {
				meshRenderersOfWeapon[i].enabled = true;
			}
			visible = true;
		}

		public void disableWeapon(){
			enabled = false;
		}

		public void enableWeapon(){
			enabled = true;
		}

		public bool fire(FactionName factionThatFiredShot,bool isAiming = false, bool fireButtonHeldDown = true, bool fireButtonPressed = false){
			if (enabled && weapon.willFire(fireButtonHeldDown,fireButtonPressed)){
				if ((Time.time-timeWhenLastSoundWasRemoved)>soundRemovalInterval && numSounds>0){
					timeWhenLastSoundWasRemoved = Time.time;
					numSounds-=1;
				}
				if (fireSoundSource!=null && fireSound!=null && weapon.magazines.Count>0 && weapon.magazines[weapon.currentMag]>0 && numSounds<maxNumSounds){
					fireSoundSource.PlayOneShot(fireSound);
					numSounds+=1;

				}
				bool result = weapon.fire(barrelEnd,barrelStart,factionThatFiredShot, isAiming);
				if (weapon.discardOnFire && weapon.stateOfWeapon != WeaponState.Dropped){
					weapon.stateOfWeapon = WeaponState.Dropped;
					weapon.dropWeapon(barrelEnd.position);
					//gameObject.rem
				}
				return result;
			}

				return false;
		}

		//allows the AI to simulate raising their rifles higher by providing a alternate start barrel position
		public bool fire(FactionName factionThatFiredShot, Vector3 startBarrelOffset, bool isAiming = false){
			if (enabled ){
				if ((Time.time-timeWhenLastSoundWasRemoved)>soundRemovalInterval && numSounds>0){
					timeWhenLastSoundWasRemoved = Time.time;
					numSounds-=1;
				}
				if (fireSoundSource!=null && fireSound!=null && weapon.magazines.Count>0 && weapon.magazines[weapon.currentMag]>0 && numSounds<maxNumSounds){
					fireSoundSource.PlayOneShot(fireSound);
					numSounds+=1;
					
				}
				barrelEnd.position +=startBarrelOffset;
				bool result = weapon.fire(barrelEnd,barrelStart,factionThatFiredShot,isAiming);
				barrelStart.position-=startBarrelOffset;
				if (weapon.discardOnFire && weapon.stateOfWeapon != WeaponState.Dropped){
					weapon.stateOfWeapon = WeaponState.Dropped;
					weapon.dropWeapon(barrelEnd.position);
					//gameObject.rem
				}
				return result;
			}
			
			return false;
		}

		public bool AIfire(FactionName factionThatFiredShot, Vector3 barrelStart, Vector3 barrelEnd, bool isAiming = false){
			if (enabled) {
				if (weapon.getCurrentFireMode().timeSinceLastShot>weapon.getCurrentFireMode().fireRate){
				if ((Time.time - timeWhenLastSoundWasRemoved) > soundRemovalInterval && numSounds > 0) {
					timeWhenLastSoundWasRemoved = Time.time;
					numSounds -= 1;
				}
				if (fireSoundSource != null && fireSound != null && weapon.magazines.Count > 0 && weapon.magazines [weapon.currentMag] > 0 && numSounds < maxNumSounds) {
					fireSoundSource.PlayOneShot (fireSound);
					numSounds += 1;
					
				}
			}
				//barrelEnd.position +=startBarrelOffset;
				bool result = weapon.fire(barrelEnd,barrelStart,factionThatFiredShot,isAiming);
				//barrelStart.position-=startBarrelOffset;
				if (weapon.discardOnFire && weapon.stateOfWeapon != WeaponState.Dropped){
					weapon.stateOfWeapon = WeaponState.Dropped;
					weapon.dropWeapon(barrelEnd);
					//gameObject.rem
				}
				return result;
			}
			
			return false;
		}

		public Weapon Weapon {
			get {
				return weapon;
			}
			set {
				weapon = value;
			}
		}
	}
}

