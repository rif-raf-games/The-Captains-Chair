using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gizmo : MonoBehaviour
{
    public void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, transform.up);
       // Gizmos.color = Color.yellow;
       // Gizmos.DrawRay(transform.position + new Vector3(.02f, 0f, 0f), Vector3.up);*/

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward);
      //  Gizmos.color = Color.cyan;
      //  Gizmos.DrawRay(transform.position + new Vector3(.02f, 0f, 0f), Vector3.forward);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.right);
       // Gizmos.color = Color.magenta;
      //  Gizmos.DrawRay(transform.position + new Vector3(0f, 0f, -.02f), Vector3.right);

        /* Gizmos.color = Color.red;
         Quaternion rotation = Quaternion.AngleAxis(dir, Vector3.up);
         Vector3 rayDir = rotation * Vector3.forward;
         Gizmos.DrawRay(transform.position, rayDir);*/

    }
}
