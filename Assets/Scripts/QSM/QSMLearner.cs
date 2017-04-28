using System;
using System.Collections.Generic;

namespace AssemblyCSharp
{
	public class QSMLearner
	{
		public List<string> possibleStates = new List<string>();
		public List<string> possibleActions = new List<string>();
		public float learningRate = 0.1f;
		public float discountFactor = 0.3f;

		public QSMLearner (List<string> possibleStates, List<string> possibleActions, float learningRate, float discountFactor)
		{
			this.possibleStates = possibleStates;
			this.possibleActions = possibleActions;
			this.discountFactor = discountFactor;
			this.learningRate = learningRate;
		}
	}
}

