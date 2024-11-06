// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// using Force;

// namespace GameUI
// {
//     public class ForceFieldVisualizer : MonoBehaviour
//     {   

//         [Header("Visuals")]
//         [Range(0,1)]
//         public float alpha = 0.3f;
//         [Range(0,5)]
//         public float sizeMult = 0.2f;

//         [Range(0.5f, 10)] [Tooltip("WARNING: if spacing is small, and area large, number of points will be enormous. This WILL freeze your computer!")]
//         public float spacing = 1f;

//         bool IsInside(Collider c, Vector3 point)
//     	{
//     		Vector3 closest = c.ClosestPoint(point);
//     		// Because closest=point if point is inside - not clear from docs I feel
//     		return closest == point;
//     	}

//         void OnDrawGizmos()
//         {
//             if(!enabled) return; // gizmos work even when script is disabled unless explicit.
            
//             // Get the outer bounds of all the currents
//             // Iterate over x,y,z inside those bounds
//             // for each point, check if its inside any of them
//             //      for each "yes", collect a sum vector
//             //          draw the sum vector

//             // Gotta find the global min/max corners of _all_ the currents
//             Bounds worldBounds = new Bounds();
//             var fields = new List<IForceField>();
//             var colliders = new List<Collider>();
//             for(int i=0; i < transform.childCount; i++)
//             {
//                 var child = transform.GetChild(i);
//                 if(child.TryGetComponent<IForceField>(out IForceField current))
//                 {
//                     var col = child.GetComponent<Collider>();
//                     // Gotta make sure we dont create bounds out of nowhere
//                     // new Bounds() above creates one by default at 0,0,0 of size 1
//                     // But if we only have _one_ current box, then we need to overwrite that
//                     if(i == 0) worldBounds = col.bounds;
//                     // Otherwise we extend it.
//                     else worldBounds.Encapsulate(col.bounds);
//                     fields.Add(current);
//                     colliders.Add(col);
//                 }
//             }

//             // Lets not create new vectors for 1k+ points...
//             Vector3 worldPoint = new Vector3();
//             Vector3 localCurrent = new Vector3();
//             for(var x=worldBounds.min[0]; x<worldBounds.max[0]; x+=spacing)
//                 for(var y=worldBounds.min[1]; y<worldBounds.max[1]; y+=spacing)
//                     for(var z=worldBounds.min[2]; z<worldBounds.max[2]; z+=spacing)
//                     {
//                         localCurrent.x=0; localCurrent.y=0; localCurrent.z=0;
//                         worldPoint.x = x; worldPoint.y = y; worldPoint.z = z;
//                         // collect currents from multiple boxes that the point is inside of
//                         for(int i=0; i<fields.Count; i++)
//                         {
//                             if(IsInside(colliders[i], worldPoint)) localCurrent += fields[i].GetForceAt(worldPoint);
//                         }
//                         // Avoid drawing if there is no current here.
//                         if(localCurrent.x != 0 || localCurrent.y != 0 || localCurrent.z != 0)
//                         {
//                             Gizmos.color = new Color(localCurrent.x, localCurrent.y, localCurrent.z, alpha);
//                             Gizmos.DrawRay(worldPoint, localCurrent * sizeMult);
//                             Gizmos.DrawSphere(worldPoint, 0.02f);
//                         }
                        
//                     }
//         }
//     }
// }