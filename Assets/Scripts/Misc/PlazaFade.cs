using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class PlazaFade : MonoBehaviour
{
    CCPlayer Captain;
    int PlazaMask;
    // Start is called before the first frame update
    void Start()
    {
       // Debug.Log("PlazaFade.Start()");
        Captain = FindObjectOfType<CCPlayer>();
        PlazaMask = 1 << LayerMask.NameToLayer("Plaza_Fade");
        //PlazaMask |= (1 << LayerMask.NameToLayer("Player"));

        GameObject[] gos = FindObjectsOfType<GameObject>();
        foreach(GameObject go in gos)
        {
            go.AddComponent<Room>();
            if (go.layer == LayerMask.NameToLayer("Plaza_Fade") && go.GetComponent<BoxCollider>() != null)
            {
                //Debug.Log("swapping collider on: " + go.name);
                DestroyImmediate(go.GetComponent<BoxCollider>());
                go.AddComponent<MeshCollider>();                
            }
        }
    }

    public Text DebugText;
    //public GameObject OriginSphere, HitSphere;
    public Vector3 Offset = Vector3.zero;
    //public GameObject CurrentFadedBuilding = null;
    public List<Collider> CurrentFadedBuildings = new List<Collider>();
    public bool Method = true;
   // bool waitedAFrame = false;
    // FixedUpdate is called once per frame
    void FixedUpdate()
    {
        
        Vector3 origin, direction;       
        origin = Captain.transform.position + Offset;
        direction = this.transform.position - origin;
       
        //OriginSphere.transform.position = origin;

        List<RaycastHit> hits = Physics.RaycastAll(origin, direction, Mathf.Infinity, PlazaMask).ToList();
        List<Collider> hitsColliders = new List<Collider>();        
        foreach(RaycastHit hit in hits)
        {
            hitsColliders.Add(hit.collider);
            if (CurrentFadedBuildings.Contains(hit.collider) == false)
            {
                CurrentFadedBuildings.Add(hit.collider);
                //hit.collider.GetComponent<MeshRenderer>().enabled = false;
                //Debug.Log("fade out building: " + hit.collider.name);
                hit.collider.GetComponent<Room>().ToggleAlpha(.3f, false, true);
            }
        }
        List<Collider> collidersToRemove = new List<Collider>();
        foreach(Collider c in CurrentFadedBuildings)
        {
            if(hitsColliders.Contains(c) == false)
            {
                collidersToRemove.Add(c);                
            }
        }
        foreach(Collider c in collidersToRemove)
        {
           // Debug.Log("fade up: " + c.name);
            //c.GetComponent<MeshRenderer>().enabled = true;
            c.GetComponent<Room>().ToggleAlpha(1f, false, true);
            CurrentFadedBuildings.Remove(c);
        }        
        string s = CurrentFadedBuildings.Count + "\n";
        foreach(Collider c in CurrentFadedBuildings)
        {
            s += c.name + "\n";
        }
       // DebugText.text = s;
    }
}/*Debug.DrawRay(this.transform.position, direction * hit.distance, Color.yellow);                        
            string s = "hit: " + hit.collider.name;
            DebugText.text = s;
            HitSphere.transform.position = hit.point;    */
//origin = this.transform.position;
//direction = (Captain.transform.position + Offset) - this.transform.position;        
