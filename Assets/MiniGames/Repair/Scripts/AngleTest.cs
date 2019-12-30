using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngleTest : MonoBehaviour
{
    public void OnDrawGizmos()
    {
        Color color = Color.blue;
        Debug.DrawRay(transform.position, transform.forward*4, color);
        color = Color.red;
        Debug.DrawRay(transform.position, transform.right * 4, color);
    }
}
