using System;
using System.Collections.Generic;

namespace AssemblyCSharp
{
	public class QGraphEdge
	{
		List<string> requiredStates = new List<string>();
		List<float> float_restrictions = new List<float>();

		public int targetNode = -1;

		float interruptThreshold = 1f;

		float lastTriggeredTime = 0;

		public QGraphEdge (IEnumerable<string> requiredStates, IEnumerable<float> float_restriction_list, int targetNode)
		{
			AddRequiredStates (requiredStates);
			AddRestrictions (float_restriction_list);

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

		public void AddRestrictions(IEnumerable<float> newRestrictions){
			float_restrictions.AddRange (float_restrictions);
		}

		public int GetStateMatchLevel(IEnumerable<string> states, IEnumerable<float> values){
			int result = 0;
			foreach (string state in states){
				if (requiredStates.Contains (state)) {
					++result;
				}
			}

			int restrictionIndex = 0;

			foreach (float val in values) {
				if (restrictionIndex < float_restrictions.Count && val < float_restrictions[restrictionIndex]) {
					++result;
				} 
				++restrictionIndex;
			}

			return result;
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
	}
}

