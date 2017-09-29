using System;
using UnityEngine;

namespace QGraphLearning
{
	[Serializable]
	/// <summary>
	/// Represents a constraint that two actions cannot happen at the same time
	/// </summary>
	public class SAConstraint
	{
		public string action1;
		public string action2;

		public SAConstraint(string first, string second){
			this.action1 = first;
			this.action2 = action2;
		}
	}
}

