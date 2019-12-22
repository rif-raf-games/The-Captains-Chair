using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepairTest : MonoBehaviour
{
    float ModelHeight = 0.5542563f; // This was calculated elsewhere for use here
                                    //public Transform RayRoot;
                                    // Start is called before the first frame update

    public void OnDrawGizmos()
    {
        Vector3 pos = transform.position + new Vector3(0f, ModelHeight / 2f, 0f);
        for (int dir = 30; dir < 360; dir += 60)
        {            
            Color color = Colors[(dir - 30) / 60];

            Quaternion q = Quaternion.AngleAxis(dir, -Vector3.up);
            Vector3 rayDir = q * Vector3.right;
            Debug.DrawRay(pos, rayDir * 4, color, 5f);            
        }        
    }
    public void OnGUI()
    {
        /*if (GUI.Button(new Rect(0, 0, 100, 100), "test"))
        {
            Vector3 pos = transform.position + new Vector3(0f, ModelHeight / 2f, 0f);
            for (int dir = 30; dir < 360; dir += 60)
            {
                Color color = Colors[(dir - 30) / 60];
                Debug.Log("dir: " + dir + " has color: " + color.ToString());
                Quaternion q = Quaternion.AngleAxis(dir, Vector3.up);
                Vector3 rayDir = q * Vector3.right;
                Debug.DrawRay(pos, rayDir * 4, color, 5f);

                //RaycastHit hit;
                bool hitOurself = false;
                Vector3 p1 = pos;
                Vector3 p2 = pos + 2f * (rayDir.normalized);
                RaycastHit[] hits = Physics.CapsuleCastAll(p1, p2, .5f, rayDir, Mathf.Infinity);
               // Debug.Log("**************************check dir: " + dir + " has this many hits: " + hits.Length);
                foreach (RaycastHit hit in hits)
                {
                    // Debug.Log("check hit: " + hit.collider.name);
                    if (hit.collider == this)
                    {
                        hitOurself = true;
                        continue;
                    }
                    //Debug.Log("checked hit: " + hit.collider.name + ", hitOurself? " + hitOurself);
                }                
            }
            RaycastHit hit2;
            Physics.Raycast(pos, transform.up, out hit2, Mathf.Infinity);
           // if (hit2.collider == null) Debug.Log("no up collider");
           // else Debug.Log("hit this collider: " + hit2.collider.name);

            Debug.DrawRay(pos, transform.forward, Color.black, 5f);
        }*/
        
            
    }
    void Start()
    {
        RaycastHit hit;
        
        /*Vector3 underPos = transform.position + new Vector3(0f, -10f, 0f);
        Vector3 overPos = transform.position + new Vector3(0f, 10f, 0f);
        Vector3 underHit = Vector3.negativeInfinity;
        Vector3 overHit = Vector3.positiveInfinity;
        Physics.Raycast(underPos, transform.up, out hit, Mathf.Infinity);
        if(hit.collider != null)
        {
            underHit = hit.point;
        }
        Physics.Raycast(overPos, -transform.up, out hit, Mathf.Infinity);
        if(hit.collider != null)
        {
            overHit = hit.point;
        }
        ModelHeight = Vector3.Distance(underHit, overHit);
        Debug.Log("height of model: " + ModelHeight);*/
        /* Debug.Log("underHit: " + underHit.ToString("F4"));
         Debug.Log("overHit: " + overHit.ToString("F4"));
         Vector3 mid = (overHit - underHit) / 2f;
         Debug.Log("mid: " + mid.ToString("F4"));
         Debug.Log("height of model: " + ModelHeight);

         MeshCollider mc = this.GetComponentInChildren<MeshCollider>();
         Debug.Log("center: " + mc.bounds.center.ToString("F4"));
         Debug.Log("min: " + mc.bounds.min.ToString("F4"));
         Debug.Log("max: " + mc.bounds.max.ToString("F4"));
         Debug.Log("extents: " + mc.bounds.extents.ToString("F4"));
         Debug.Log("size: " + mc.bounds.size.ToString("F4"));        
         int x = 5;
         x++;*/
    }

    Color[] Colors = { Color.red, Color.green, Color.blue, Color.yellow, Color.cyan, Color.magenta };
    // Update is called once per frame
    void Update()
    {
        /*for (int dir = 30; dir < 360; dir += 60)
        {
            Color color = Colors[(dir - 30) / 60];

            Vector3 pos = transform.position + new Vector3(0f, ModelHeight / 2f, 0f);
            Quaternion q = Quaternion.AngleAxis(dir, -Vector3.up);
            Vector3 rayDir = q * Vector3.right;
            Debug.DrawRay(pos, rayDir * 4, color);
            //   Debug.Log("check dir: " + dir + " with pos: " + pos.ToString("F3"));
        }*/
        //Vector3 startDir = (this.name.Contains("01") ? Vector3.forward : Vector3.right);
        /* if(this.name.Contains("01"))
         //if(true)
         {
             for (int dir = 0; dir < 360; dir += 60)
             {
                 Color color = Colors[(dir) / 60];

                 Vector3 pos = transform.position + new Vector3(0f, ModelHeight / 2f, 0f);
                 Quaternion q = Quaternion.AngleAxis(dir, Vector3.up);
                 Vector3 rayDir = q * Vector3.forward;
                 Debug.DrawRay(pos, rayDir*4, color);
                 //Debug.Log("check dir: " + dir + " with pos: " + pos.ToString("F3"));
             }
         }        
        // Debug.Log("*************");
         else if(this.name.Contains("02"))
         {
             for (int dir = 30; dir < 360; dir += 60)
             {
                 Color color = Colors[(dir-30) / 60];

                 Vector3 pos = transform.position + new Vector3(0f, ModelHeight/2f, 0f);
                 Quaternion q = Quaternion.AngleAxis(dir, Vector3.up);
                 Vector3 rayDir = q * Vector3.right;
                 Debug.DrawRay(pos, rayDir*4, color);
              //   Debug.Log("check dir: " + dir + " with pos: " + pos.ToString("F3"));
             }
         }
         else if (this.name.Contains("03"))
         {
             for (int dir = 30; dir < 360; dir += 60)
             {
                 Color color = Colors[(dir - 30) / 60];

                 Vector3 pos = transform.position + new Vector3(0f, ModelHeight / 2f, 0f);
                 Quaternion q = Quaternion.AngleAxis(dir, -Vector3.up);
                 Vector3 rayDir = q * Vector3.right;
                 Debug.DrawRay(pos, rayDir * 4, color);
                 //   Debug.Log("check dir: " + dir + " with pos: " + pos.ToString("F3"));
             }
         }*/

        /*  const int count = 6;
          const float stepAngle = 360f / count;
          Vector3 startDirection = Vector3.forward;
          Vector3 rotationAxis = Vector3.up;

          for (int i = 0; i < count; i++)
          {
              Quaternion rotation = Quaternion.AngleAxis(stepAngle * i, rotationAxis);
              Vector3 rayDirection = rotation * startDirection;

              Debug.DrawRay(transform.position, rayDirection, Color.green);
          }*/
    }


    /*private void OnGUI()
    {
        if(GUI.Button(new Rect(0,0,100,100), "sphere"))
        {
            Collider[] colliders;
            colliders = Physics.OverlapSphere(transform.position, .5f);
            foreach(Collider c in colliders )
            {
                Debug.Log("collided with: " + c.name);
            }
        }       
        if (GUI.RepeatButton(new Rect(0, 100, 100, 100), "ray"))
        {           
            
        }
    }*/

    // public Transform t1, t2;
    /* private void OnGUI()
     {
         if (GUI.Button(new Rect(0, 0, 100, 100), "test"))
         {
             List<int> validDirs = new List<int>();
             for (int dir = 30; dir < 360; dir += 60)
             {

                 Color color = Colors[(dir - 30) / 60];
                 Vector3 pos = CurBookendStart.transform.position + new Vector3(0f, ModelHeight / 2f, 0f);
                 Quaternion q = Quaternion.AngleAxis(dir, -Vector3.up);
                 Vector3 rayDir = q * Vector3.right;

                 bool hitOurself = false;
                 RaycastHit[] hits = Physics.RaycastAll(pos, rayDir, Mathf.Infinity);
                 Debug.Log("**************************check dir: " + dir + " has this many hits: " + hits.Length);
                 foreach (RaycastHit hit in hits)
                 {
                    // Debug.Log("check hit: " + hit.collider.name);
                     if (hit.collider == this)
                     {
                         hitOurself = true;
                         continue;
                     }
                     Debug.Log("checked hit: " + hit.collider.name + ", hitOurself? " + hitOurself);
                 }



             }

             Debug.Log("num validDirs: " + validDirs.Count);
             foreach(int dir in validDirs)
             {
                 Vector3 pos = CurBookendStart.transform.position + new Vector3(0f, ModelHeight / 2f, 0f);
                 Quaternion q = Quaternion.AngleAxis(dir, -Vector3.up);
                 Vector3 rayDir = q * Vector3.right;
                 Debug.DrawRay(pos, rayDir * 4, Color.yellow, 5f);
             }
         }
         if (GUI.Button(new Rect(0, 100, 100, 100), "rays"))
         {
             for (int dir = 30; dir < 360; dir += 60)
             {
                 Color color = Colors[(dir - 30) / 60];
                 Vector3 pos = CurBookendStart.transform.position + new Vector3(0f, ModelHeight / 2f, 0f);
                 Quaternion q = Quaternion.AngleAxis(dir, -Vector3.up);
                 Vector3 rayDir = q * Vector3.right;
                 Debug.DrawRay(pos, rayDir * 4, color, 5f);
             }            
         }
     }*/

    float dir = 60f;
   // public void OnDrawGizmos()
   // {
        /*Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.up);
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position + new Vector3(.02f, 0f, 0f), Vector3.up);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, transform.forward);
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position + new Vector3(.02f, 0f, 0f), Vector3.forward);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.right);
        Gizmos.color = Color.magenta;
        Gizmos.DrawRay(transform.position + new Vector3(0f, 0f, -.02f), Vector3.right);

        Gizmos.color = Color.red;
        Quaternion rotation = Quaternion.AngleAxis(dir, Vector3.up);
        Vector3 rayDir = rotation * Vector3.forward;
        Gizmos.DrawRay(transform.position, rayDir);*/

   // }
}
