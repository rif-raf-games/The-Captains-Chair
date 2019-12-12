using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
public class RifRafGames
{
   [MenuItem("RifRaf Games/Process Lockpick Puzzle")]
   public static void ProcessLockpickPuzzle()
    {
        
        if(Selection.activeGameObject == null )
        {
            Debug.LogError("RifRaf Error: No GameObject selected.");
            return;
        }
        if(Selection.gameObjects.Length != 1 )
        {
            Debug.LogError("RifRaf Error: You must have only 1 GameObject selected.  You have " + Selection.gameObjects.Length + " selected.");
            return;
        }
        if(Selection.activeGameObject.name.Contains("Lock_Picking_Puzzle") == false)
        {
            Debug.LogError("RifRaf Error: Incorrect GameObject selected. " + Selection.activeGameObject.name + " does not have \"Lock_Picking_Puzzle\" in it's name.");
            return;
        }
        Debug.Log("Huzzah! Lets process this Lockpicking game.");

        Event e = new Event();
        e.keyCode = KeyCode.RightArrow;
        e.type = EventType.KeyDown;

        int touchControlLayer = LayerMask.NameToLayer("Lockpick Touch Control");
        int ringLayer = LayerMask.NameToLayer("Lockpick Ring");
        int pathNodeLayer = LayerMask.NameToLayer("Lockpick Path Node");
        int collisionControlLayer = LayerMask.NameToLayer("Lockpick Collision Control");
        int ringLayerMask = LayerMask.GetMask("Lockpick Ring");
        int touchControlMask = LayerMask.GetMask("Lockpick Touch Control");

        GameObject rootGO = Selection.activeGameObject;                
        LockPicking lp = rootGO.AddComponent<LockPicking>();
        EditorWindow.focusedWindow.SendEvent(e);

        GameObject centerBlock = rootGO.transform.GetChild(0).gameObject;
        centerBlock.AddComponent<MeshCollider>();        
        centerBlock.layer = ringLayer;
        centerBlock.transform.position = Vector3.zero;

        GameObject centerNode = rootGO.transform.GetChild(1).gameObject;

        GameObject diodeGO = rootGO.transform.GetChild(2).gameObject;
        diodeGO.AddComponent<SphereCollider>();
        Diode diode = diodeGO.AddComponent<Diode>();        
        float diodeRadius = diode.GetComponent<SphereCollider>().radius;
        GameObject debugText = GameObject.Find("DebugText");
        if (debugText != null) diode.DebugText = debugText.GetComponent<Text>();
        GameObject lastPosition = GameObject.Find("LastPosition");
        if (lastPosition != null) diode.LastPosition = lastPosition;

        List<GameObject> ringObjectRoots = new List<GameObject>();
        for(int i=3; i<rootGO.transform.childCount; i++)
        {          
            ringObjectRoots.Add(rootGO.transform.GetChild(i).gameObject);
        }
        Debug.Log("this puzzle has " + ringObjectRoots.Count + " rings.");

        List<Ring> rings = new List<Ring>();
        List<PathNode> startNodes = new List<PathNode>();
        List<PathNode> deathNodes = new List<PathNode>();        
        
        foreach(GameObject touchControl in ringObjectRoots)
        {
            touchControl.AddComponent<MeshCollider>();
            touchControl.GetComponent<MeshRenderer>().enabled = false;
            touchControl.layer = touchControlLayer;
            Selection.activeObject = touchControl;
            EditorWindow.focusedWindow.SendEvent(e);

            GameObject collisionControl = touchControl.transform.GetChild(0).gameObject;
            //collisionControl.AddComponent<MeshCollider>();
            collisionControl.GetComponent<MeshRenderer>().enabled = false;
            collisionControl.layer = collisionControlLayer;
            Selection.activeObject = collisionControl;
            EditorWindow.focusedWindow.SendEvent(e);

            GameObject ring = collisionControl.transform.GetChild(0).gameObject;
            ring.AddComponent<MeshCollider>();
            ring.AddComponent<Ring>();            
            ring.layer = ringLayer;
            rings.Add(ring.GetComponent<Ring>());
            //Selection.activeObject = ring;           
            //EditorWindow.focusedWindow.SendEvent(e);

            int numPathNodes = ring.transform.childCount;            
            int numPaths = numPathNodes / 3;
            Debug.Log("ring " + ring.name + " has " + numPathNodes + " path nodes representing " + numPaths + " paths.");            
            foreach(Transform nodeTransform in ring.transform)
            {
                //Debug.Log("*************checking node: " + node.name);     
                nodeTransform.gameObject.AddComponent<PathNode>();
                nodeTransform.gameObject.layer = pathNodeLayer;
                SphereCollider sc = nodeTransform.gameObject.AddComponent<SphereCollider>();
                sc.radius = diodeRadius/3f;
                Collider[] colliders = Physics.OverlapSphere(nodeTransform.position, sc.radius, ringLayerMask);  
                if(nodeTransform.name.Contains("Middle"))
                {                    
                    if(colliders.Length == 0)
                    {
                        nodeTransform.gameObject.SetActive(false);
                    }
                }
                foreach (Collider c in colliders)
                {
                    //Debug.Log("colliding with: " + c.name);
                    if (nodeTransform.name.Contains("Start") || nodeTransform.name.Contains("End"))
                    {
                        if(c.transform == ring.transform )
                        {                            
                            nodeTransform.gameObject.SetActive(false);
                            break;
                        }
                    }                   
                }                
            }

            Ring.LockpickRingPath[] paths = new Ring.LockpickRingPath[numPaths];
            for (int i=0; i<numPaths; i++)
            {
                paths[i] = new Ring.LockpickRingPath();
                PathNode start = ring.transform.GetChild(i + (numPaths * 2)).gameObject.GetComponent<PathNode>();
                PathNode mid = ring.transform.GetChild(i + (numPaths)).gameObject.GetComponent<PathNode>();
                PathNode end = ring.transform.GetChild(i).gameObject.GetComponent<PathNode>();
                if (start.gameObject.activeSelf == true )
                {
                    paths[i].Start = start;
                    paths[i].End = (mid.gameObject.activeSelf == true ? mid : end);                    
                }
                else
                {
                    paths[i].Start = mid;
                    paths[i].End = end;
                }
                paths[i].Init(ring.GetComponent<Ring>());

                if (touchControl == ringObjectRoots[0] && start.gameObject.activeSelf == true) startNodes.Add(start);
                if (touchControl == ringObjectRoots[ringObjectRoots.Count - 1] && end.gameObject.activeSelf == true) deathNodes.Add(end);                        
                //bug.Log("path: " + i + " on ring: " + ring.name + ": " + paths[i].Start.name + " -> " + paths[i].End.name);
                //Debug.Log("start: " + start.name + ", mid: " + mid.name + ", end: " + end.name);
            }
            ring.GetComponent<Ring>().InitPaths(paths);
        }

        /* List<float> startDists = new List<float> { 0.064582795f, 0.08132881f, 0.12363285f, 0.16715005f };
         List<float> endDists = new List<float> { 0.092904925f, 0.12526965f, 0.1580112f };
         int ringIndex = 0;
         foreach (Ring ring in rings)
          {
              foreach(Ring.LockpickRingPath p in ring.Paths)
              {                                 
                  if(p.Start.name.Contains("Start"))
                  {
                     p.Start.transform.position = p.Start.transform.position + (startDists[ringIndex] * -p.Start.transform.forward.normalized);
                  }
                  if(ringIndex != rings.Count-1 && p.End.name.Contains("End"))
                 {
                     p.End.transform.position = p.End.transform.position + (endDists[ringIndex] * -p.End.transform.forward.normalized);                   
                 }
              }
             ringIndex++;
              
          }*/

        lp.InitFromProcessing(diode, centerBlock, rings, startNodes, deathNodes);

        // not sure why we need this but for some reason the last expand event we try to send on the ring doesn't work
        Selection.activeObject = ringObjectRoots[ringObjectRoots.Count-1].gameObject;
        EditorWindow.focusedWindow.SendEvent(e);
        Selection.activeObject = rootGO;

        EditorSceneManager.MarkAllScenesDirty();
    }
}
// Diode d = Object.Instantiate(diode).GetComponent<Diode>();
// d.transform.parent = p.Start.transform.parent;
//d.transform.position = hit.point;

