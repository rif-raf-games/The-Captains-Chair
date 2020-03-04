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

    
    public void SetRoomsAlpha(float alpha, bool skipLerp = false)
    {
        Debug.Log("---------------------------SetRoomsAlpha() alpha: " + alpha);
        foreach (Room room in LevelRooms)
        {
            room.ToggleAlpha(alpha, skipLerp);
        }
    }
    public void SetPlayerLevelRoomsAlpha(float alpha, bool skipLerp = false)
    {
        Debug.Log("---------------------------------SetPlayerLevelRoomsAlpha() alpha: " + alpha);
        int layerMask = LayerMask.GetMask("Ship Area Collider");
        foreach (Room room in LevelRooms)
        {
            BoxCollider box = room.gameObject.GetComponent<BoxCollider>();
            Collider[] colliders = Physics.OverlapBox(box.bounds.center, box.size / 2, transform.rotation, layerMask);
            if (colliders.Length == 1) room.ToggleAlpha(1f, skipLerp);
            else room.ToggleAlpha(alpha, skipLerp);
        }
    }

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
    public void DEBUG_SetShader(string shader)
    {
        Debug.Log("------ShipLevel.DEBUG_SetShader(): " + shader);
        foreach (Room room in LevelRooms)
        {
            room.DEBUG_SetShader(shader);
        }
    }
}
