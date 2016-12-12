//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34209
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using UnityEngine;


namespace AssemblyCSharp
{
	public class PathFindingNode : IComparable<PathFindingNode>
	{
		public Vector3 pos = Vector3.down;
		public PathFindingNode previous = null;
		public float g = 0;
		public float f = 0;
		public float h = 0;
		public float losDistance = 0;
		public float deltaLosDistance = 0;
		public float deltaH = 0;
		public bool isJumpPoint = false;

		public PathFindingNode (PathFindingNode previous, Vector3 pos, Vector3 start, Vector3 end, ref bool reachedEnd, bool useStandardAStar, bool isJumpPoint)
		{
			this.isJumpPoint = isJumpPoint;
			reachedEnd = false;
			this.previous = previous;
			this.pos = pos;

			//we calculate h first, because we can determine when calculating h whether the end is visible, and therefore jump to it
			Vector3 startEndDir = (end - pos);
			
			h = startEndDir.sqrMagnitude;
			h = h*(h/AIGrid.GetManhattanDistance (end, pos));

			startEndDir = startEndDir.normalized;
			
			float dot = -1;
			int dirToUse = -1;
			for (int i = 0; i<AIGrid.LOSDirections.Length; ++i) {
				float temp = Vector3.Dot(startEndDir,AIGrid.LOSDirections[i]);
				if (temp>dot){
					dot = temp;
					dirToUse = i;
				}
			}

			losDistance = (0.1f * AIGrid.visibilityDistances [(int)pos.x, (int)pos.y, dirToUse] + 0.175f * AIGrid.visibilityDistances [(int)pos.x, (int)pos.y, (dirToUse + 1) % AIGrid.LOSDirections.Length] + 0.275f * AIGrid.visibilityDistances [(int)pos.x, (int)pos.y, (dirToUse + 2) % AIGrid.LOSDirections.Length] + 0.275f * AIGrid.visibilityDistances [(int)pos.x, (int)pos.y, (dirToUse + 6) % AIGrid.LOSDirections.Length] + 0.175f * AIGrid.visibilityDistances [(int)pos.x, (int)pos.y, (dirToUse + 7) % AIGrid.LOSDirections.Length] + 0f * AIGrid.visibilityDistances [(int)pos.x, (int)pos.y, (dirToUse + 5) % AIGrid.LOSDirections.Length] + 0f * AIGrid.visibilityDistances [(int)pos.x, (int)pos.y, (dirToUse + 3) % AIGrid.LOSDirections.Length]);

			if (dot==1 && losDistance >= h*dot) {
				Debug.DrawRay(pos,AIGrid.LOSDirections[dirToUse]*losDistance,Color.black,30f);
				Debug.DrawRay(pos,startEndDir*losDistance,Color.magenta,30f);
				Debug.Log("Found jump point!");
				pos = end;
				reachedEnd = true;
			}

			if (!reachedEnd) {

				if (previous != null) {
					Debug.DrawRay(pos,previous.pos-pos, useStandardAStar?Color.blue:Color.red,10f);
					g = Vector2.Distance (previous.pos, pos) + previous.g;
					//losDistance-=previous.losDistance;
				}

				if (!useStandardAStar){
					//float heurDist = AIGrid.GetManhattanDistance(start,end);
					if ((h*h)>losDistance){
					//if (h<heurDist){
						//float div = Mathf.Abs(h-0.5f*heurDist);
						//div*=2;
						//div = 1-div;
						float div = (losDistance/(h*h));
						//float div = h/(end-start).magnitude;
						//div *= (1-(h/(end-start).magnitude));
						//float multiplier = (((h-(previous!=null?previous.h:0)))/((losDistance -(previous!=null?previous.losDistance:0)/** dot*/)));
						//deltaLosDistance = previous!=null?(0.5f*(losDistance-previous.losDistance)-previous.deltaLosDistance):0;
						//deltaH = previous!=null?(0.5f*(h-previous.h)-previous.deltaH):0;
						//deltaLosDistance = previous!=null?(0.5f*previous.deltaLosDistance+0.5f*(previous!=null?losDistance-previous.losDistance:losDistance)):((previous!=null?losDistance-previous.losDistance:losDistance));
						//deltaH = previous!=null?(0.5f*previous.deltaH+0.5f*(previous!=null?h-previous.h:h)):((previous!=null?h-previous.h:h));
						deltaLosDistance = (previous!=null?losDistance*dot-previous.losDistance:losDistance*dot);
						deltaH = (previous!=null?h-previous.h:h);
						//float multiplier = ((((previous!=null?h-previous.h:h)))/((previous!=null?((losDistance -(previous.losDistance))):losDistance/** dot*/)));
						//float multiplier = -deltaLosDistance/deltaH;
						//float multiplier = (((h))/((losDistance -(previous!=null?previous.losDistance:0)/** dot*/)));
						//float multiplier = (h)/((losDistance/** dot*/));
						float multiplier = (deltaH-deltaLosDistance);
						//multiplier=multiplier<0?(multiplier>=-1?(1-multiplier)*(1-multiplier):multiplier*multiplier):multiplier;
						multiplier = multiplier<2?multiplier:2; //experimenatlly disabled
						//multiplier = multiplier>0?1:multiplier; //experimenatlly disabled
						float div2 = h/AIGrid.GetManhattanDistance(start,end);
						//float div2 = (h)/(h+g);
						div*=div;
						div2 = div2<0?0:div2;
						div2 = div2>1?1:div2;
						h=(1-div2)*h + (div2)*(h*div+(1-div)*h*(multiplier)*dot);
						//h=(div2)*h + (1-div2)*(h*div+(1-div)*h*(multiplier)*dot);
						//h=0.5f*h + 0.5f*(h*div+(1-div)*h*(multiplier)*dot);
					}
				}



				//Debug.DrawRay(pos,Vector3.up*h,Color.cyan);

				f = g+h;
				//InitWorldGrid.fValues [(int)pos.x, (int)pos.y] = f;
			}




		}

		public int CompareTo(PathFindingNode other){
			if (Mathf.Approximately (this.f, other.f)) {
				return 0;
			} else if (this.f < other.f) {
				return -1;
			} else {
				return 1;
			}
		}
	}
}

