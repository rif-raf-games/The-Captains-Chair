using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class WanderManager : MonoBehaviour
{
    List<WanderPoint> UnusedWanderPoints = new List<WanderPoint>();
    List<WanderPoint> UsedWanderPoints = new List<WanderPoint>();
    List<Wanderer> Wanderers = new List<Wanderer>();
    
    private void Awake()    
    {
        UnusedWanderPoints = transform.GetComponentsInChildren<WanderPoint>().ToList();
        Wanderers = transform.GetComponentsInChildren<Wanderer>().ToList();
    }
    
    public WanderPoint GetWanderPoint()
    {
        int index = Random.Range(0, UnusedWanderPoints.Count);
        WanderPoint wp = UnusedWanderPoints[index];
        UnusedWanderPoints.Remove(wp);
        UsedWanderPoints.Add(wp);
        return wp;
    }

    public void ReleaseWanderPoint( WanderPoint wp)
    {
        UnusedWanderPoints.Add(wp);
        UsedWanderPoints.Remove(wp);
    }

}
