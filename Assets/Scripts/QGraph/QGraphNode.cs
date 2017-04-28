using System;
using System.Collections.Generic;

namespace AssemblyCSharp
{
	public class QGraphNode
	{

		public List<string> actions = new List<string>();
		public List<QGraphEdge> outgoingEdges = new List<QGraphEdge>();

		public QGraphNode ()
		{
		}


	}
}

