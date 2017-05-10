using System;
using UnityEngine;

namespace AssemblyCSharp
{
	[Serializable]
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

