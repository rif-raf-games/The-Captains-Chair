using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderRecolor : MonoBehaviour
{
    public Color Color1 = Color.red;
    public Color Color2 = Color.blue;
    public bool Draw = true;
   // public bool rot = false;
   // public bool drawA = true;
   //  public bool drawB = true;
    private void OnDrawGizmos()
    {
        if (Draw == false) return;
        Gizmos.color = Color1;
        if (GetComponent<BoxCollider>() != null)
        {
            BoxCollider box = GetComponent<BoxCollider>();                                   
            Gizmos.DrawWireCube(box.bounds.center, box.bounds.size);                        
        }
        else if (GetComponent<SphereCollider>() != null)
        {           
            SphereCollider sc = GetComponent<SphereCollider>();            
            Gizmos.DrawWireSphere(sc.bounds.center, sc.radius);
        }
        // z = 2
        // y = 1
        // x = 0
        else if (GetComponent<CapsuleCollider>() != null)
        {
            Vector3 center, start, end, top, bot, right, left;
            CapsuleCollider cc = GetComponent<CapsuleCollider>();            
            
            center = cc.transform.TransformPoint(cc.center);
            Gizmos.color = Color2;
            Gizmos.DrawWireSphere(center, .5f);
            switch(cc.direction)
            {
                case 2: // z
                    start = center + cc.height / 2 * cc.transform.forward;
                    Gizmos.DrawWireSphere(start, .5f);
                    end = center - cc.height / 2 * cc.transform.forward;
                    Gizmos.DrawWireSphere(end, .5f);
                    Gizmos.color = Color1;
                    top = center + cc.radius * cc.transform.up;
                    Gizmos.DrawWireSphere(top, .5f);
                    bot = center - cc.radius * cc.transform.up;
                    Gizmos.DrawWireSphere(bot, .5f);
                    right = center + cc.radius * cc.transform.right;
                    Gizmos.DrawWireSphere(right, .5f);
                    left = center - cc.radius * cc.transform.right;
                    Gizmos.DrawWireSphere(left, .5f);
                    break;
                case 1: // y
                    start = center + cc.height / 2 * cc.transform.up;
                    Gizmos.DrawWireSphere(start, .5f);
                    end = center - cc.height / 2 * cc.transform.up;
                    Gizmos.DrawWireSphere(end, .5f);
                    Gizmos.color = Color1;
                    top = center + cc.radius * cc.transform.forward;
                    Gizmos.DrawWireSphere(top, .5f);
                    bot = center - cc.radius * cc.transform.forward;
                    Gizmos.DrawWireSphere(bot, .5f);
                    right = center + cc.radius * cc.transform.right;
                    Gizmos.DrawWireSphere(right, .5f);
                    left = center - cc.radius * cc.transform.right;
                    Gizmos.DrawWireSphere(left, .5f);
                    break;
                case 0: // x
                    start = center + cc.height / 2 * cc.transform.right;
                    Gizmos.DrawWireSphere(start, .5f);
                    end = center - cc.height / 2 * cc.transform.right;
                    Gizmos.DrawWireSphere(end, .5f);
                    Gizmos.color = Color1;
                    top = center + cc.radius * cc.transform.up;
                    Gizmos.DrawWireSphere(top, .5f);
                    bot = center - cc.radius * cc.transform.up;
                    Gizmos.DrawWireSphere(bot, .5f);
                    right = center + cc.radius * cc.transform.right;
                    Gizmos.DrawWireSphere(right, .5f);
                    left = center - cc.radius * cc.transform.right;
                    Gizmos.DrawWireSphere(left, .5f);
                    break;

            }


            
            
            
        }
    }
}
