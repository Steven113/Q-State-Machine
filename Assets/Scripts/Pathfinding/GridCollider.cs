using UnityEngine;
using System.Collections;

public class GridCollider : MonoBehaviour {

	// Use this for initialization
	void Awake () {
		Collider col;
		if ((col = gameObject.GetComponent<Collider> ()) != null) {
			Bounds bounds = col.bounds;
			for (float x = bounds.center.x-bounds.extents.x; x<(bounds.center.x+bounds.extents.x)+AIGrid.s_cellWidth; x+=AIGrid.s_cellWidth){
				for (float z = bounds.center.z-bounds.extents.z; z<(bounds.center.z+bounds.extents.z)+AIGrid.s_cellWidth; z+=AIGrid.s_cellWidth){
					int x_p = (int)x;
					int z_p = (int)z;
					if (x_p>=0 && x_p<AIGrid.cellCanBeMovedThrough.GetLength(0) && z_p>=0 && z_p<AIGrid.cellCanBeMovedThrough.GetLength(1)){
						AIGrid.cellCanBeMovedThrough[x_p,z_p] = false;
					}
				}
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
