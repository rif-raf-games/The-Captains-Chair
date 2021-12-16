using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System;

public class ShipAreasCollider : MonoBehaviour
{
    float RoomFadeOpacity;   
    public List<ShipLevel> ShipLevels = new List<ShipLevel>();
    public List<GameObject> FloorColliders = new List<GameObject>();
    public List<CharacterEntity> CharacterEntities = new List<CharacterEntity>();

    CCPlayer Player;

    [Header("Debug")]
    public Text DebugText;
    private void Awake()
    {
        TheCaptainsChair cChair = FindObjectOfType<TheCaptainsChair>();
        RoomFadeOpacity = cChair.RoomFadeOpacity;
        Player = GetComponentInParent<CCPlayer>();        
    }

    public void ToggleShipFloors(bool val)
    {
        float alpha = (val == true ? 1f : 0f);
        for (int level = 1; level <= ShipLevels.Count; level++)
        {
            if (level == Player.GetCurFloor())
            {
                Debug.LogError("set player's floor opacity: " + level);
                //ShipLevels[level - 1].SetPlayerLevelRoomsAlpha(true);
                ShipLevels[level - 1].SetRoomsAlpha(1f, true);
            }
            else
            {
                Debug.LogError("turn level " + level + " to 0%");
                ShipLevels[level - 1].SetRoomsAlpha(alpha, true);
            }            
        }
    }
    private void Start()
    {        
        ShipLevels = FindObjectsOfType<ShipLevel>().ToList<ShipLevel>();
        ShipLevels = ShipLevels.OrderBy(o => o.name).ToList<ShipLevel>();
        GameObject[] floors = GameObject.FindGameObjectsWithTag("FloorNavMesh");
        FloorColliders = floors.ToList<GameObject>().OrderBy(o => o.name).ToList<GameObject>();
        CharacterEntities = FindObjectsOfType<CharacterEntity>().ToList<CharacterEntity>();
        
        for(int level = 1; level <= ShipLevels.Count; level++)
        {
            if (level == Player.GetCurFloor())
            {
                // Debug.Log("set player's floor opacity: " + level);
                //ShipLevels[level - 1].SetPlayerLevelRoomsAlpha( true);
                ShipLevels[level - 1].SetRoomsAlpha(1f, true);
            }
            else //if (level < PlayerFloor)
            {
              //  Debug.Log("turn level " + level + " to 0%");
                ShipLevels[level - 1].SetRoomsAlpha(0f, true);
            }           
        }
    }    

    List<GameObject> DimmedRoomsViaRaycast = new List<GameObject>();
    private void Update()
    {
        //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Room room;
        string s = "Raycast hits:\n";
        Ray ray = new Ray(transform.parent.position + new Vector3(0f, 4f, 0f), Vector3.forward * 1f);        
        int layerMask = LayerMask.GetMask("Room");
        List<RaycastHit> hits = Physics.RaycastAll(ray, Mathf.Infinity, layerMask).ToList<RaycastHit>();
        List<GameObject> hitsGOs = new List<GameObject>();
        
        foreach(RaycastHit hit in hits )
        {
            room = hit.collider.GetComponent<Room>();
            if (room == null)
            {
                // if we're here then we should be in a 3 layers of rooms in a hallway situation.  For those rooms, put the Room on a child component 
                // so that when the Room shuts itself off the trigger on the object above will stay around
                room = hit.collider.GetComponentInChildren<Room>();
            }
            if( room == null)
            {
                // if we're here then we should be in a situation where the raycast is hitting against a collider who has it's Room on a child object that's
                // been shut off so look for a room in the DimmedRooms that has it's active flag set to false
                foreach(GameObject go in DimmedRoomsViaRaycast)
                {
                    if(go.transform.parent.gameObject == hit.collider.gameObject)
                    {
                        //Debug.Log("we've found the room in the list that should not be active: " + go.name + ": " + go.activeSelf);
                        room = go.GetComponent<Room>();
                    }
                }

            }
            hitsGOs.Add(room.gameObject);
            s += hit.collider.name + "\n";
            if(DimmedRoomsViaRaycast.Contains(room.gameObject) == false)
            {
               // Debug.Log("dim room via raycast: " + hit.collider.name);
                if(DimmedRoomsViaRaycast.Count == 1 )
                {
                  //  Debug.Log("dim the front room down to zero");
                    DimmedRoomsViaRaycast[0].GetComponent<Room>().ToggleAlpha(0f, false);                    
                }
                
                room.ToggleAlpha(RoomFadeOpacity, false/*, Room.eCollisionType.RAYCAST*/);
                DimmedRoomsViaRaycast.Add(room.gameObject);
                
            }            
        }                       

        List<GameObject> hitsToRemove = new List<GameObject>();
        bool removedRoomFromList = false;
        foreach (GameObject hit in DimmedRoomsViaRaycast)
        {            
            if(hitsGOs.Contains(hit) == false)
            {
              //  Debug.Log("turn room back on via raycast leaving: "+ hit.name);
                hitsToRemove.Add(hit);
                room = hit.GetComponent<Room>();
                room.ToggleAlpha(1f, false);
                removedRoomFromList = true;
            }
        }
        foreach(GameObject hit in hitsToRemove)
        {
           // Debug.Log("remove this: " + hit.name + " from dimmed list");
            DimmedRoomsViaRaycast.Remove(hit);
        }
        if(removedRoomFromList == true && DimmedRoomsViaRaycast.Count == 1)
        {
            room = DimmedRoomsViaRaycast[0].GetComponent<Room>();
           // Debug.Log("turn transparent room " + room.name + " back to 20%");
            room.ToggleAlpha(RoomFadeOpacity, false);
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

    private void OnTriggerEnter(Collider other)
    {//StaticStuff.PrintTriggerEnter(this.name + " We only want to handle Room colliders here.  other: " + other.name);
        //if ( !(other.gameObject.layer == LayerMask.NameToLayer("Room") || other.gameObject.layer == LayerMask.NameToLayer("Ship Level"))) { Debug.LogError("RoomCollider.OnTriggerEnter() we should NOT be colliding with this " + other.name + " that has a layer OTHER than Room and Ship Level " + LayerMask.LayerToName(other.gameObject.layer)); return; }
        if (other.gameObject.layer != LayerMask.NameToLayer("Ship Level")) { Debug.LogError("RoomCollider.OnTriggerEnter() we should NOT be colliding with this " + other.name + " that has a layer OTHER than Ship Level " + LayerMask.LayerToName(other.gameObject.layer)); return; }
        StaticStuff.PrintTriggerEnter(this.name + " RoomCollider.OnTriggerEnter() other: " + other.name + ", layer: " + other.gameObject.layer);        
                        
        ShipLevel shipLevel = other.GetComponent<ShipLevel>();        
        if(shipLevel != null)
        {
            StaticStuff.PrintTriggerEnter("collided with a shiplevel: " + other.name);              
            Player.SetFloor(shipLevel.Level, other.name);
            CheckFloorColliders();
            //shipLevel.SetPlayerLevelRoomsAlpha();
            shipLevel.SetRoomsAlpha(1f);
        }
        else
        {
            Debug.LogError("Error: ShipAreasCollider.OnTriggerEnter() is colliding with an incorrect type");
        }        
    }

    void CheckFloorColliders()
    {
      //  Debug.Log("Check Colliders");
        for (int level = 1; level <= ShipLevels.Count; level++)
        {
            if (ShipLevels[level - 1].Level > Player.GetCurFloor()) FloorColliders[level - 1].GetComponent<MeshCollider>().enabled = false;
            else FloorColliders[level - 1].GetComponent<MeshCollider>().enabled = true;
        }
    }
   
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Ship Level")) { Debug.LogError("RoomCollider.OnTriggerExit() we should NOT be colliding with this " + other.name + " that has a layer OTHER than Ship Level " + LayerMask.LayerToName(other.gameObject.layer)); return; }
        StaticStuff.PrintTriggerEnter(this.name + " RoomCollider.OnTriggerExit() other: " + other.name + ", layer: " + other.gameObject.layer);
                
        ShipLevel shipLevel = other.GetComponent<ShipLevel>();        
        if (shipLevel != null)
        {
            StaticStuff.PrintTriggerEnter("left floor collider: " + other.name);
            shipLevel.SetRoomsAlpha(0f);        
        }
        else
        {
            Debug.LogError("Error: ShipAreasCollider.OnTriggerExit() is colliding with an incorrect type");
        }

    }    
}
