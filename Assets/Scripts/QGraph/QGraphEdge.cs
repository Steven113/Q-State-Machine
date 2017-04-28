using System;
using System.Collections.Generic;

namespace AssemblyCSharp
{
	public class QGraphEdge
	{
		public List<string> requiredStates = new List<string>();
		public List<float> float_restrictions = new List<float>();

		public int targetNode = -1;

		public QGraphEdge (IEnumerable<string> requiredStates, IEnumerable<float> float_restriction_list)
		{
			AddRequiredStates (requiredStates);
			AddRestrictions (float_restriction_list);
		}

		public QGraphEdge (string requiredState, float f_restriction)
		{
			requiredStates.Add(requiredState);
			float_restrictions.Add(f_restriction);
		}

		public QGraphEdge ()
		{

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

		public int GetStateMatchLevel(IEnumerable<string> states){
			int result = 0;
			foreach (string state in states){
				if (requiredStates.Contains (state)) {
					++result;
				}
			}
			return result;
		}
	}
}

