using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AssemblyCSharp;

public class HealingPotionDistributor : MonoBehaviour {

	public List<QSoldier> soldiersToGivePotionsTo = new List<QSoldier>();

	public float intervalForGivingLargePotions = 90f;
	public float intervaleForGivingSmallPotions = 30f;
	public float timeSinceLastGivingLargePotion = 0f;
	public float timeSinceLastGivingSmallPotion = 0f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		timeSinceLastGivingLargePotion += Time.deltaTime;

		if (timeSinceLastGivingLargePotion > intervalForGivingLargePotions) {
			timeSinceLastGivingLargePotion%=intervalForGivingLargePotions;
			for (int i = 0; i<soldiersToGivePotionsTo.Count; ++i){
				++soldiersToGivePotionsTo[i].bigFoundCount;
			}
		}

		timeSinceLastGivingSmallPotion += Time.deltaTime;
		
		if (timeSinceLastGivingSmallPotion > intervaleForGivingSmallPotions) {
			timeSinceLastGivingSmallPotion%=intervaleForGivingSmallPotions;
			for (int i = 0; i<soldiersToGivePotionsTo.Count; ++i){
				++soldiersToGivePotionsTo[i].smallFoodCount;
			}
		}
	}
}
