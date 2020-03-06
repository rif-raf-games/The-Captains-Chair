using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System;

public class ShipAreasCollider : MonoBehaviour
{
    float RoomFadeOpacity;
   // float FloorFadeOpacity;
    public List<ShipLevel> ShipLevels = new List<ShipLevel>();
    public List<GameObject> FloorColliders = new List<GameObject>();
    public int PlayerFloor = 0;

    [Header("Debug")]
    public Text DebugText;
    private void Awake()
    {
        TheCaptainsChair cChair = FindObjectOfType<TheCaptainsChair>();
        RoomFadeOpacity = cChair.RoomFadeOpacity;
      //  FloorFadeOpacity = cChair.FloorFadeOpacity;

        QualityIndex = QualitySettings.GetQualityLevel();
        SortModeIndex = (int)Camera.main.transparencySortMode;
    }

    int ShaderIndex = 0;
    int RenderModeIndex = 0;
    int LevelSelect = 3;
    int QualityIndex = 0;
    float DBGAlpha = 1f;
    int SortModeIndex = 0;  
   Vector3 SortAxis = new Vector3(0f, 0f, 1f);

#if false              
    private void OnGUI()
    {
        int buttonW = 100;
        int buttonH = 50;
        
        // shaders
        List<Shader> shaderList = new List<Shader>() { RifRaf, Standard, LegSpec, LegVLit, LegDiff/*, NewSurface, RifRafNewSurface*/ };
        string[] shaders = { "RifRaf Standard", "Standard", "Leg Spec", "Leg VLit", "Leg Diffuse"/*, "Custom NewSurf", "RifRafNewSurf"*/ };
        string[] shaderNames = { "RifRafStandard", "Standard", "Legacy Shaders/Transparent/Specular", "Legacy Shaders/Transparent/VertexLit",
                               "Legacy Shaders/Transparent/Diffuse"/*, "Custom/NewSurfaceShader",  "Custom/RifRafNewSurfaceShader"*/ };
        float oldShaderIndex = ShaderIndex;
        ShaderIndex = GUI.SelectionGrid(new Rect(0, 0, buttonW, shaders.Length * buttonH), ShaderIndex, shaders, 1);
        if (oldShaderIndex != ShaderIndex) ShipLevels[LevelSelect].DEBUG_SetShader(shaderNames[ShaderIndex], shaderList[ShaderIndex]);

        // Render mode
        string[] renderModes = { "Opaque", "Fade", "Transparent" };
        int oldRenderIndex = RenderModeIndex;
        RenderModeIndex = GUI.SelectionGrid(new Rect(110, 0, buttonW, renderModes.Length * buttonH), RenderModeIndex, renderModes, 1);
        if(oldRenderIndex != RenderModeIndex)
        {
            Debug.Log("setting render mode: " + renderModes[RenderModeIndex]);
            if(RenderModeIndex == 0) ShipLevels[LevelSelect].DEBUG_SetOpaque();
            if(RenderModeIndex == 1) ShipLevels[LevelSelect].DEBUG_SetFade();
            if(RenderModeIndex == 2) ShipLevels[LevelSelect].DEBUG_SetTransparent();
        }        

        // alpha
        float oldAlpha = DBGAlpha;
        DBGAlpha = GUI.VerticalSlider(new Rect(220, 10, 100, Screen.height - 20), DBGAlpha, 1f, 0f);
        if (oldAlpha != DBGAlpha) ShipLevels[LevelSelect].DEBUG_SetAlpha(DBGAlpha);        
        GUI.TextField(new Rect(235, 0, 85, 20), DBGAlpha.ToString("F3"));

        // level
        string[] levels = new string[] { "Floor 1", "Floor 2", "Floor 3", "Floor 4" };
        LevelSelect = GUI.SelectionGrid(new Rect(350,0,levels.Length*buttonW,buttonH), LevelSelect, levels, 4);

        CCPlayer player = GetComponentInParent<CCPlayer>();
        if(GUI.Button(new Rect(350 + levels.Length * buttonW + 20, 0, buttonW, buttonH), "Moveable: " + !player.DEBUG_BlockMovement))
        {
            player.DEBUG_BlockMovement = !player.DEBUG_BlockMovement;
        }

        // Quality
        int oldQuality = QualityIndex;
        int numQualities = QualitySettings.names.Length;
        QualityIndex = GUI.SelectionGrid(new Rect(350, buttonH+10, numQualities*buttonW, buttonH), QualityIndex, QualitySettings.names, numQualities);
        if (oldQuality != QualityIndex) QualitySettings.SetQualityLevel(QualityIndex);

        // sort index
        int oldSortIndex = SortModeIndex;
        string[] enumNames = Enum.GetNames(typeof(TransparencySortMode));        
        SortModeIndex = GUI.SelectionGrid(new Rect(350, (buttonH*2) + 20, enumNames.Length * buttonW, buttonH), SortModeIndex, enumNames, enumNames.Length);
        if (oldSortIndex != SortModeIndex) Camera.main.transparencySortMode = (TransparencySortMode)SortModeIndex;               

        Vector3 oldSortAxis = SortAxis;
        SortAxis.x = GUI.HorizontalSlider(new Rect(890, (buttonH * 2) + 20, Screen.width - 10 - 890, 20), SortAxis.x, -180f, 180f);
        SortAxis.y = GUI.HorizontalSlider(new Rect(890, (buttonH * 2) + 50, Screen.width - 10 - 890, 20), SortAxis.y, -180f, 180f);
        SortAxis.z = GUI.HorizontalSlider(new Rect(890, (buttonH * 2) + 80, Screen.width - 10 - 890, 20), SortAxis.z, -180f, 180f);        
        if(GUI.Button(new Rect(1070, buttonH + 10, buttonW, buttonH), "Cap-Cam"))
        {            
            SortAxis = GetComponentInParent<CCPlayer>().transform.position - Camera.main.transform.position;
        }
        if (GUI.Button(new Rect(1170, buttonH + 10, buttonW, buttonH), "Cam-Camp"))
        {
            SortAxis = Camera.main.transform.position - GetComponentInParent<CCPlayer>().transform.position;
        }

        if (oldSortAxis != SortAxis) Camera.main.transparencySortAxis = SortAxis;
        GUI.TextField(new Rect(760, (buttonH * 2) + 20, 125, 20), SortAxis.ToString("F1"));
    }
    
    public Material SampleMaterial;
    public Shader RifRaf;
    public Shader Standard;    
    public Shader LegSpec;
    public Shader LegVLit;
    public Shader LegDiff;       
#endif

    private void Start()
    {        
        ShipLevels = FindObjectsOfType<ShipLevel>().ToList<ShipLevel>();
        ShipLevels = ShipLevels.OrderBy(o => o.name).ToList<ShipLevel>();
        GameObject[] floors = GameObject.FindGameObjectsWithTag("FloorNavMesh");
        FloorColliders = floors.ToList<GameObject>().OrderBy(o => o.name).ToList<GameObject>();        

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
                Debug.Log("set player's floor opacity: " + level);
                ShipLevels[level - 1].SetPlayerLevelRoomsAlpha(/*1f,*/ true);
            }
            else //if (level < PlayerFloor)
            {
                Debug.Log("turn level " + level + " to 0%");
                ShipLevels[level - 1].SetRoomsAlpha(0f, true);
            }
            /*else
            {
                Debug.Log("turn level " + level + " totally transparent");
                ShipLevels[level - 1].SetRoomsAlpha(0f, true);
            }*/
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
                Debug.Log("dim room via raycast: " + hit.collider.name);
                if(DimmedRoomsViaRaycast.Count == 1 )
                {
                    Debug.Log("dim the front room down to zero");
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
                Debug.Log("turn room back on via raycast leaving: "+ hit.name);
                hitsToRemove.Add(hit);
                room = hit.GetComponent<Room>();
                room.ToggleAlpha(1f, false);
                removedRoomFromList = true;
            }
        }
        foreach(GameObject hit in hitsToRemove)
        {
            Debug.Log("remove this: " + hit.name + " from dimmed list");
            DimmedRoomsViaRaycast.Remove(hit);
        }
        if(removedRoomFromList == true && DimmedRoomsViaRaycast.Count == 1)
        {
            room = DimmedRoomsViaRaycast[0].GetComponent<Room>();
            Debug.Log("turn transparent room " + room.name + " back to 20%");
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
                
        //Room room = other.GetComponent<Room>();
        ShipLevel shipLevel = other.GetComponent<ShipLevel>();
        /*if (room != null) //{ Debug.LogError("No Room component on the thing we collided with: " + this.name + " , Collider other: " + other.name); return; }
        {                        
            if(room.GetComponentInParent<ShipLevel>().Level == PlayerFloor)
            {
                if(DimmedRoomsViaRaycast.Contains(room.gameObject))
                {
                    Debug.LogWarning("ok we were going to make " + room.name + " opaque but it's on the DimmedRoomsViaRaycast so don't do it");
                }
                else
                {
                    StaticStuff.PrintTriggerEnter(this.name + " OK, we've just collided with a room " + other.name + " and it's on our floor so change opacity to 1f");
                    room.ToggleAlpha(1f);
                }                
            }            
            else
            {
                StaticStuff.PrintTriggerEnter(this.name + " collided with a room " + other.name + " but it's not on the same floor so bail");
            }
        }  */      
        //else 
        if(shipLevel != null)
        {
            StaticStuff.PrintTriggerEnter("collided with a shiplevel: " + other.name);            
            PlayerFloor = shipLevel.Level;
            CheckFloorColliders();
            shipLevel.SetPlayerLevelRoomsAlpha(/*RoomFadeOpacity*/);
        }
        else
        {
            Debug.LogError("Error: ShipAreasCollider.OnTriggerEnter() is colliding with an incorrect type");
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

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Ship Level")) { Debug.LogError("RoomCollider.OnTriggerExit() we should NOT be colliding with this " + other.name + " that has a layer OTHER than Ship Level " + LayerMask.LayerToName(other.gameObject.layer)); return; }
        StaticStuff.PrintTriggerEnter(this.name + " RoomCollider.OnTriggerExit() other: " + other.name + ", layer: " + other.gameObject.layer);
        
        //Room room = other.GetComponent<Room>();
        ShipLevel shipLevel = other.GetComponent<ShipLevel>();
        /*if (room != null) 
        {                                    
            if (room.GetComponentInParent<ShipLevel>().Level == PlayerFloor)
            {
                StaticStuff.PrintTriggerEnter(this.name + " OK, we've just left a collision with a room " + other.name + " and it's on the player floor so set it to RoomFadeOpacity");
                room.ToggleAlpha(RoomFadeOpacity);
            }
            else
            {
                StaticStuff.PrintTriggerEnter(this.name + " left a collision with a room " + other.name + " but it's not on our floor so bail");
            }
        }*/
        //else 
        if (shipLevel != null)
        {
            StaticStuff.PrintTriggerEnter("left floor collider: " + other.name);
            shipLevel.SetRoomsAlpha(0f);
            //if (shipLevel.Level > PlayerFloor) shipLevel.SetRoomsAlpha(FloorFadeOpacity);
            //else shipLevel.SetRoomsAlpha(RoomFadeOpacity);
        }
        else
        {
            Debug.LogError("Error: ShipAreasCollider.OnTriggerExit() is colliding with an incorrect type");
        }

    }
}
