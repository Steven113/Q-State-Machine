using System;
using System.Collections.Generic;
using UnityEngine;

namespace AssemblyCSharp
{
	[Serializable]
	public class ConstraintMapping
	{
		Dictionary<string,Dictionary<string,bool>> mapping = new Dictionary<string,Dictionary<string,bool>>();

		public ConstraintMapping(){
		
		}

		public ConstraintMapping (List<SAConstraint> initialConstraints){
			for (int i = 0; i < initialConstraints.Count; ++i) {
				AddConstraint (initialConstraints [i].action1, initialConstraints [i].action2);
			}
		}

		public ConstraintMapping (ConstraintMapping other){
			mapping = new Dictionary<string, Dictionary<string, bool>> (other.mapping);
		}

		public void AddConstraint(string first, string second){
			if (!mapping.ContainsKey (first)) {
				mapping.Add (first, new Dictionary<string, bool> ());
			}
			mapping [first] [second] = true;

			if (!mapping.ContainsKey (second)) {
				mapping.Add (second, new Dictionary<string, bool> ());
			}
			mapping [second] [first] = true;
		}

		public bool GetConstraint(string first, string second){
			Debug.Assert (mapping.ContainsKey (first));
			Debug.Assert (mapping [first].ContainsKey (second));
			return mapping [first][second];
		}

		public void RemoveConstraint(string first, string second){
			if (mapping.ContainsKey (first)) {
				if (mapping [first].ContainsKey (second)) {
					mapping.Remove (second);
				}

				mapping.Remove (first);
			}
		}

		public bool HasConstraint(string first, string second){
			if (mapping.ContainsKey (first)) {
				if (mapping [first].ContainsKey (second)) {
					return true;
				}

				return false;
			} else {
				return false;
			}
		}
	}
}

