using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class ShipAreasCollider : MonoBehaviour
{
    float RoomFadeOpacity;
    float FloorFadeOpacity;
    public List<ShipLevel> ShipLevels = new List<ShipLevel>();
    public List<GameObject> FloorColliders = new List<GameObject>();
    public int PlayerFloor = 0;

    [Header("Debug")]
    public Text DebugText;
    private void Awake()
    {
        TheCaptainsChair cChair = FindObjectOfType<TheCaptainsChair>();
        RoomFadeOpacity = cChair.RoomFadeOpacity;
        FloorFadeOpacity = cChair.FloorFadeOpacity;
    }
   
    private void Start()
    {        
        ShipLevels = FindObjectsOfType<ShipLevel>().ToList<ShipLevel>();
        ShipLevels = ShipLevels.OrderBy(o => o.name).ToList<ShipLevel>();
        GameObject[] floors = GameObject.FindGameObjectsWithTag("FloorNavMesh");
        FloorColliders = floors.ToList<GameObject>().OrderBy(o => o.name).ToList<GameObject>();
        /*foreach(GameObject go in FloorColliders)
        {
            go.GetComponent<MeshRenderer>().material.shader = UnityEngine.Shader.Find("RifRafStandard");
        }*/

        int layerMask = LayerMask.GetMask("Ship Area Collider");
        foreach (ShipLevel level in ShipLevels)
        {
            BoxCollider box = level.GetComponent<BoxCollider>();            
            Collider[] colliders = Physics.OverlapBox(box.bounds.center, box.size/2, level.transform.rotation, layerMask);
            if(colliders.Length == 1)
            {
                int levelNum = level.Level;
                Debug.Log("the player is on this level: " + levelNum);
                if (PlayerFloor != 0) Debug.LogError("ERROR: The player thinks it's on two different levels of the ship.");
                PlayerFloor = levelNum;                
            }            
        }
        for(int level = 1; level <= ShipLevels.Count; level++)
        {
            if (level == PlayerFloor)
            {
                Debug.Log("set player's room opacity: " + level);
                ShipLevels[level - 1].SetPlayerLevelRoomsAlpha(RoomFadeOpacity, true);
            }
            else if (level < PlayerFloor)
            {
                Debug.Log("turn level " + level + " to 20%");
                ShipLevels[level - 1].SetRoomsAlpha(RoomFadeOpacity, true);
            }
            else
            {
                Debug.Log("turn level " + level + " totally transparent");
                ShipLevels[level - 1].SetRoomsAlpha(FloorFadeOpacity, true);
            }
        }
    }

    public float offset = 4f;
    // public void OnDrawGizmos()
    //{
      //  float l = 20f;
        // Gizmos.color = Color.green;
        // Gizmos.DrawLine(transform.parent.position, transform.parent.position + Vector3.up * l );
     //   Vector3 offsetPos = new Vector3(0f, offset, 0f);
    //    Gizmos.color = Color.blue;
    //    Gizmos.DrawLine(transform.parent.position + offsetPos, transform.parent.position + offsetPos + Vector3.forward * l);
       /* Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.parent.position, transform.parent.position + Vector3.right * l);
        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.parent.position, transform.parent.position + Vector3.down * l);
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.parent.position, transform.parent.position + Vector3.back * l);
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.parent.position, transform.parent.position + Vector3.left * l); */  
   // }

    List<GameObject> DimmedRoomsViaRaycast = new List<GameObject>();
    private void Update()
    {
        //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        string s = "Raycast hits:\n";
        Ray ray = new Ray(transform.parent.position + new Vector3(0f, 4f, 0f), Vector3.forward * 1f);        
        int layerMask = LayerMask.GetMask("Room");
        List<RaycastHit> hits = Physics.RaycastAll(ray, Mathf.Infinity, layerMask).ToList<RaycastHit>();
        List<GameObject> hitsGOs = new List<GameObject>();

        foreach(RaycastHit hit in hits )
        {
            hitsGOs.Add(hit.collider.gameObject);
            s += hit.collider.name + "\n";
            if(DimmedRoomsViaRaycast.Contains(hit.collider.gameObject) == false)
            {
                Debug.Log("dim room via raycast: " + hit.collider.name);
                DimmedRoomsViaRaycast.Add(hit.collider.gameObject);
                Room room = hit.collider.GetComponent<Room>();
                room.ToggleAlpha(RoomFadeOpacity, false/*, Room.eCollisionType.RAYCAST*/);
            }            
        }                
        string t = "Dimmed: ";
        foreach (GameObject hit in DimmedRoomsViaRaycast) t += hit.name + ",";
        t += "\nhits: ";
        foreach (RaycastHit hit in hits) t += hit.collider.name + ",";
        Debug.Log(t);

        List<GameObject> hitsToRemove = new List<GameObject>();
        foreach (GameObject hit in DimmedRoomsViaRaycast)
        {
            //Debug.Log("num hits: " + hits.Count);
            if(hitsGOs.Contains(hit) == false)
            {
                Debug.Log("turn room back on via raycast leaving");
                hitsToRemove.Add(hit);
                Room room = hit.GetComponent<Room>();
                room.ToggleAlpha(1f, false);
            }
        }
        foreach(GameObject hit in hitsToRemove)
        {
            Debug.Log("remove this: " + hit.name + " from dimmed list");
            DimmedRoomsViaRaycast.Remove(hit);
        }

        if(DebugText != null)
        {
            DebugText.text = s + "\n";
            DebugText.text += "DimmedRoomsViaRaycast:\n";
            foreach (GameObject hit in DimmedRoomsViaRaycast)
            {
                DebugText.text += hit.name + "\n";
            }
        }
    }

    void CheckFloorColliders()
    {
        Debug.Log("Check Colliders");
        for (int level = 1; level <= ShipLevels.Count; level++)
        {
            if (ShipLevels[level - 1].Level > PlayerFloor) FloorColliders[level - 1].GetComponent<MeshCollider>().enabled = false;
            else FloorColliders[level - 1].GetComponent<MeshCollider>().enabled = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {//StaticStuff.PrintTriggerEnter(this.name + " We only want to handle Room colliders here.  other: " + other.name);
        if ( !(other.gameObject.layer == LayerMask.NameToLayer("Room") || other.gameObject.layer == LayerMask.NameToLayer("Ship Level"))) { Debug.LogError("RoomCollider.OnTriggerEnter() we should NOT be colliding with this " + other.name + " that has a layer OTHER than Room and Ship Level " + LayerMask.LayerToName(other.gameObject.layer)); return; }
        StaticStuff.PrintTriggerEnter(this.name + " RoomCollider.OnTriggerEnter() other: " + other.name + ", layer: " + other.gameObject.layer);        
                
        Room room = other.GetComponent<Room>();
        ShipLevel shipLevel = other.GetComponent<ShipLevel>();
        if (room != null) //{ Debug.LogError("No Room component on the thing we collided with: " + this.name + " , Collider other: " + other.name); return; }
        {                        
            if(room.GetComponentInParent<ShipLevel>().Level == PlayerFloor)
            {
                Debug.LogWarning(this.name + " OK, we've just collided with a room " + other.name + " and it's on our floor so change opacity to 1f");
                room.ToggleAlpha(1f);
            }            
            else
            {
                Debug.LogWarning(this.name + " collided with a room " + other.name + " but it's not on the same floor so bail");
            }
        }        
        else if(shipLevel != null)
        {
            Debug.LogWarning("collided with a shiplevel: " + other.name);            
            PlayerFloor = shipLevel.Level;
            CheckFloorColliders();
            shipLevel.SetPlayerLevelRoomsAlpha(RoomFadeOpacity);
        }
        else
        {
            Debug.LogError("Error: ShipAreasCollider.OnTriggerEnter() is colliding with an incorrect type");
        }        
    }

    
    private void OnTriggerExit(Collider other)
    {
        if ( !(other.gameObject.layer == LayerMask.NameToLayer("Room") || other.gameObject.layer == LayerMask.NameToLayer("Ship Level"))) { Debug.LogError("RoomCollider.OnTriggerExit() we should NOT be colliding with this " + other.name + " that has a layer OTHER than Room and Ship Level " + LayerMask.LayerToName(other.gameObject.layer)); return; }
        StaticStuff.PrintTriggerEnter(this.name + " RoomCollider.OnTriggerExit() other: " + other.name + ", layer: " + other.gameObject.layer);
        
        Room room = other.GetComponent<Room>();
        ShipLevel shipLevel = other.GetComponent<ShipLevel>();
        if (room != null) 
        {                                    
            if (room.GetComponentInParent<ShipLevel>().Level == PlayerFloor)
            {
                Debug.LogWarning(this.name + " OK, we've just left a collision with a room " + other.name + " and it's on the player floor so set it to RoomFadeOpacity");
                room.ToggleAlpha(RoomFadeOpacity);
            }
            else
            {
                Debug.LogWarning(this.name + " left a collision with a room " + other.name + " but it's not on our floor so bail");
            }
        }
        else if (shipLevel != null)
        {
            Debug.LogWarning("left floor collider: " + other.name);
            if (shipLevel.Level > PlayerFloor) shipLevel.SetRoomsAlpha(FloorFadeOpacity);
            else shipLevel.SetRoomsAlpha(RoomFadeOpacity);
        }
        else
        {
            Debug.LogError("Error: ShipAreasCollider.OnTriggerExit() is colliding with an incorrect type");
        }

    }
}
