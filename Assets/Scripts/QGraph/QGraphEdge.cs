using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace AssemblyCSharp
{
	public class QGraphEdge
	{
		List<string> requiredStates = new List<string>();
		List<float> float_restrictions = new List<float>();
		List<float> float_mult = new List<float>();

		public int targetNode = -1;

		float interruptThreshold = 1f;

		float lastTriggeredTime = 0;

		public QGraphEdge (IEnumerable<string> requiredStates, IEnumerable<float> float_restriction_list,IEnumerable<float> float_mult_list, int targetNode)
		{
			AddRequiredStates (requiredStates);
			AddRestrictions (float_restriction_list,float_mult_list);

			this.targetNode = targetNode;
		}

		public QGraphEdge (string requiredState, float f_restriction, int targetNode)
		{
			requiredStates.Add(requiredState);
			float_restrictions.Add(f_restriction);

			this.targetNode = targetNode;
		}

		public QGraphEdge (int targetNode)
		{
			this.targetNode = targetNode;
		}

		/* copy constructor*/
		public QGraphEdge (QGraphEdge other){
			this.requiredStates = new List<string> (other.requiredStates);
			this.interruptThreshold = other.interruptThreshold;
			this.float_restrictions = new List<float> (other.float_restrictions);
			this.float_mult = new List<float> (other.float_mult);
			this.targetNode = other.targetNode;
		}

		/* Add require states. Returns number of states added*/
		public int AddRequiredStates(IEnumerable<string> newStates){
			int result = 0;
			foreach (string state in newStates){
				if (!requiredStates.Contains (state)) {
					requiredStates.Add (state);
					++result;
				}
			}
			return result;
		}

		/* Delete required states. Returns number of states deleted*/
		public void RemoveRequiredStates(IEnumerable<string> newStates){
			foreach (string state in newStates){
				if (requiredStates.Contains (state)) {
					requiredStates.Remove (state);
				}
			}
		}

		public void AddRestrictions(IEnumerable<float> newRestrictions,IEnumerable<float> newMult){
			Debug.Assert (newRestrictions.Count() == newMult.Count());
			float_restrictions.AddRange (float_restrictions);
			float_mult.AddRange(newMult);
		}

		public int GetStateMatchLevel(IEnumerable<string> states, IEnumerable<float> values){
			int result = 0;
			foreach (string state in states){
				if (requiredStates.Contains (state)) {
					++result;
				}
			}

			int restrictionIndex = 0;

			//Debug.Log (float_restrictions.Count);
			//Debug.Log (float_mult.Count);

			foreach (float val in values) {
				if (restrictionIndex < float_restrictions.Count && val < float_restrictions[restrictionIndex]*float_mult[restrictionIndex]) {
					++result;
				} 
				++restrictionIndex;
			}

			return result;
		}

		public static QGraphEdge MutateEdge(QGraphEdge edge, List<string> possibleStates, float mutationRate){
			QGraphEdge mutant = new QGraphEdge (edge);
			mutant.requiredStates = Utils.RandomlyModifyList (possibleStates, mutant.requiredStates);
			for (int i = 0; i < mutant.float_restrictions.Count; ++i) {
				mutant.float_restrictions[i] += (UnityEngine.Random.value - 0.5f) * mutationRate;
				mutant.float_mult[i]+= (UnityEngine.Random.value - 0.5f) * mutationRate;
			}

			mutant.interruptThreshold += (UnityEngine.Random.value - 0.5f) * mutationRate;
			mutant.interruptThreshold = Mathf.Clamp (mutant.interruptThreshold, 0, 1);

			return mutant;
		}

		public float InterruptThreshold {
			get {
				return interruptThreshold;
			}
			set {
				interruptThreshold = value;
			}
		}

		public List<string> RequiredStates {
			get {
				return requiredStates;
			}
			set {
				requiredStates = value;
			}
		}

		public List<float> Float_restrictions {
			get {
				return float_restrictions;
			}
			set {
				float_restrictions = value;
			}
		}

		public float LastTriggeredTime {
			get {
				return lastTriggeredTime;
			}
			set {
				lastTriggeredTime = value;
			}
		}

		public List<float> Float_mult {
			get {
				return float_mult;
			}
			set {
				float_mult = value;
			}
		}
	}
}

