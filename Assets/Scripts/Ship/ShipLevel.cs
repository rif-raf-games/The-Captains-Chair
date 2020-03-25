using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ShipLevel : MonoBehaviour
{
    List<Room> LevelRooms = new List<Room>();
    public int Level;

    private void Awake()
    {
        Level = int.Parse(this.name[this.name.Length - 1].ToString());
    }
    // Start is called before the first frame update
    void Start()
    {
        LevelRooms = GetComponentsInChildren<Room>().ToList<Room>();
    }   

    List<Elevator> GatherElevators()
    {
        List<Elevator> elevatorsToChangeAlpha = new List<Elevator>();
        Elevator[] allElevators = FindObjectsOfType<Elevator>();
        foreach(Elevator e in allElevators)
        {
            if (e.CheckIfSameFloor(Level)) elevatorsToChangeAlpha.Add(e);
        }
        return elevatorsToChangeAlpha;
    }
    
    public void SetRoomsAlpha(float alpha, bool skipLerp = false)
    {
        //Debug.Log("---------------------------SetRoomsAlpha() alpha: " + alpha);
        foreach (Room room in LevelRooms)
        {
            room.ToggleAlpha(alpha, skipLerp);
        }

        List<Elevator> elevatorsToChangeAlpha = GatherElevators();
        foreach(Elevator e in elevatorsToChangeAlpha)
        {
            e.SetupAlphaLerp(alpha, skipLerp);
        }
    }
   /* public void SetPlayerLevelRoomsAlpha( bool skipLerp = false)
    {
       // Debug.Log("---------------------------------SetPlayerLevelRoomsAlpha()");
        //int layerMask = LayerMask.GetMask("Ship Area Collider");
        foreach (Room room in LevelRooms)
        {
            room.ToggleAlpha(1f, skipLerp);
        }
    }*/

    public void DEBUG_SetAlpha(float alpha)
    {
        foreach (Room room in LevelRooms)
        {
            room.ToggleAlpha(alpha, true);
        }
    }
    public void DEBUG_SetTransparent()
    {
        foreach (Room room in LevelRooms)
        {
            room.DEBUG_SetTransparent();
        }
    }
    public void DEBUG_SetFade()
    {
        foreach (Room room in LevelRooms)
        {
            room.DEBUG_SetFade();
        }
    }
    public void DEBUG_SetOpaque()
    {
        foreach (Room room in LevelRooms)
        {
            room.DEBUG_SetOpaque();
        }
    }
    public void DEBUG_SetShader(string shaderName, Shader shader)
    {
        Debug.Log("------ShipLevel.DEBUG_SetShader(): " + shaderName);
        foreach (Room room in LevelRooms)
        {
            room.DEBUG_SetShader(shaderName, shader);
        }
    }
}
