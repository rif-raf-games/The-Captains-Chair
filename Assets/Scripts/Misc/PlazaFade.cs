using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlazaFade : MonoBehaviour
{
    CCPlayer Captain;
    int PlayerPlazaMask;
    // Start is called before the first frame update
    void Start()
    {
       // Debug.Log("PlazaFade.Start()");
        Captain = FindObjectOfType<CCPlayer>();
        PlayerPlazaMask = 1 << LayerMask.NameToLayer("Player");
        PlayerPlazaMask |= (1 << LayerMask.NameToLayer("Plaza_Fade"));

        GameObject[] gos = FindObjectsOfType<GameObject>();
        foreach(GameObject go in gos)
        {
            if(go.layer == LayerMask.NameToLayer("Plaza_Fade") && go.GetComponent<BoxCollider>() != null)
            {
                //Debug.Log("swapping collider on: " + go.name);
                Destroy(go.GetComponent<BoxCollider>());
                go.AddComponent<MeshCollider>();
            }
        }
    }

    public Text DebugText;
    public GameObject OriginSphere, HitSphere;
    public Vector3 Offset = Vector3.zero;
    public GameObject CurrentFadedBuilding = null;
    public bool Method = true;
    // FixedUpdate is called once per frame
    void FixedUpdate()
    {
        Vector3 origin, direction;       
        origin = Captain.transform.position + Offset;
        direction = this.transform.position - origin;
        //origin = this.transform.position;
        //direction = (Captain.transform.position + Offset) - this.transform.position;        
        OriginSphere.transform.position = origin;

        RaycastHit hit;
        if( Physics.Raycast(origin, direction, out hit, Mathf.Infinity, PlayerPlazaMask) == true)
        {
            Debug.DrawRay(this.transform.position, direction * hit.distance, Color.yellow);                        
            string s = "hit: " + hit.collider.name;
            DebugText.text = s;
            HitSphere.transform.position = hit.point;            
            
            if(hit.collider.gameObject.layer == LayerMask.NameToLayer("Plaza_Fade"))
            {
                CurrentFadedBuilding = hit.collider.gameObject;
                MeshRenderer mr = hit.collider.GetComponent<MeshRenderer>();
                mr.enabled = false;
            }
            else if(CurrentFadedBuilding != null)
            {
                MeshRenderer mr = CurrentFadedBuilding.GetComponent<MeshRenderer>();
                mr.enabled = true;
                CurrentFadedBuilding = null;
            }                        
        }
        else if(CurrentFadedBuilding != null)
        {
            MeshRenderer mr = CurrentFadedBuilding.GetComponent<MeshRenderer>();
            mr.enabled = true;
            CurrentFadedBuilding = null;
        }
    }
}