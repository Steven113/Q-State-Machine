using System;
using System.Collections.Generic;
using UnityEngine;
using QGraphLearning;

namespace AssemblyCSharp
{
	[Serializable]
	/// <summary>
	/// Encapsulates a list of constrains on actions that cannot happen at the same time
	/// </summary>
	public class ConstraintMapping
	{
		List<SAConstraint> mapping = new List<SAConstraint>();

		public ConstraintMapping(){
		
		}

		public ConstraintMapping (List<SAConstraint> initialConstraints){
			for (int i = 0; i < initialConstraints.Count; ++i) {
				AddConstraint (initialConstraints [i].action1, initialConstraints [i].action2);
			}
		}

		public ConstraintMapping (ConstraintMapping other){
			mapping = new List<SAConstraint> (other.mapping);
		}

		public void AddConstraint(string first, string second){
			mapping.Add (new SAConstraint (first, second));
		}

		/// <summary>
		/// Is there a constraint preventing these two actions from happening at the same time?
		/// </summary>
		/// <returns><c>true</c>, if constraint was gotten, <c>false</c> otherwise.</returns>
		/// <param name="first">First.</param>
		/// <param name="second">Second.</param>
		public bool GetConstraint(string first, string second){
			for (int i = 0; i < mapping.Count; ++i) {
				if ((mapping [i].action1.Equals (first) && mapping [i].action2.Equals (second)) || (mapping [i].action2.Equals (first) && mapping [i].action1.Equals (second))) {
					return true;
				}
			}
			return false;
		}

		public void RemoveConstraint(string first, string second){
			for (int i = 0; i < mapping.Count; ++i) {
				if ((mapping [i].action1.Equals (first) && mapping [i].action2.Equals (second)) || (mapping [i].action2.Equals (first) && mapping [i].action1.Equals (second))) {
					mapping.RemoveAt (i);
				}
			}

		}
	}
}

