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
                Debug.Log("set player's floor opacity: " + level);
                //ShipLevels[level - 1].SetPlayerLevelRoomsAlpha(true);
                ShipLevels[level - 1].SetRoomsAlpha(1f, true);
            }
            else
            {
                Debug.Log("turn level " + level + " to 0%");
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
                        
        ShipLevel shipLevel = other.GetComponent<ShipLevel>();        
        if(shipLevel != null)
        {
            StaticStuff.PrintTriggerEnter("collided with a shiplevel: " + other.name);            
            Player.SetFloor(shipLevel.Level);
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

    /* public GameObject Stu;
    private void OnGUI()
    {
        if(GUI.Button(new Rect(0,0,100,100), "0"))
        {
            Debug.Log("Stu name: " + Stu.name);
            Renderer[] childRs = Stu.GetComponentsInChildren<Renderer>();
            //SkinnedMeshRenderer[] smrs = Stu.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (Renderer mr in childRs)
            {
                Debug.Log(mr.name);
                List<Material> mrMaterials = new List<Material>();
                mr.GetMaterials(mrMaterials);
                //Debug.Log(mrMaterials.Count);
                foreach (Material material in mrMaterials)
                {
                    Debug.Log(material.name);
                    material.shader = UnityEngine.Shader.Find("RifRafStandard");
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.color = new Color(material.color.r, material.color.g, material.color.b, 0f);
                }
            }
        }
        if (GUI.Button(new Rect(0, 100, 100, 100), "1"))
        {
            Debug.Log("Stu name: " + Stu.name);
            Renderer[] childRs = Stu.GetComponentsInChildren<Renderer>();            
            foreach (Renderer mr in childRs)
            {
                Debug.Log(mr.name);
                List<Material> mrMaterials = new List<Material>();
                mr.GetMaterials(mrMaterials);
                //Debug.Log(mrMaterials.Count);
                foreach (Material material in mrMaterials)
                {
                    Debug.Log(material.name);
                    material.shader = UnityEngine.Shader.Find("RifRafStandard");
                    material.SetOverrideTag("RenderType", "");
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetInt("_ZWrite", 1);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = -1;
                    material.color = new Color(material.color.r, material.color.g, material.color.b, 1f);
                }
            }
        }
    }*/
#if false
    int ShaderIndex = 0;
    int RenderModeIndex = 0;
    int LevelSelect = 3;
    int QualityIndex = 0;
    float DBGAlpha = 1f;
    int SortModeIndex = 0;  
   Vector3 SortAxis = new Vector3(0f, 0f, 1f);
    //QualityIndex = QualitySettings.GetQualityLevel();
       // SortModeIndex = (int)Camera.main.transparencySortMode;
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
}
