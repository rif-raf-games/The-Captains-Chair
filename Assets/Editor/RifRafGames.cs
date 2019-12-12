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
        //int touchControlMask = LayerMask.GetMask("Lockpick Touch Control");

        GameObject rootGO = Selection.activeGameObject;                
        LockPicking lp = rootGO.AddComponent<LockPicking>();
        GameObject gameResultText = GameObject.Find("GameResultText");          
        if (gameResultText != null) lp.GameResultText = gameResultText.GetComponent<Text>();
        EditorWindow.focusedWindow.SendEvent(e);

        GameObject centerBlock = rootGO.transform.GetChild(0).gameObject;
        centerBlock.AddComponent<MeshCollider>();        
        centerBlock.layer = ringLayer;
        centerBlock.transform.position = Vector3.zero;

        GameObject centerNode = rootGO.transform.GetChild(1).gameObject;

        GameObject diodeGO = rootGO.transform.GetChild(2).gameObject;
        diodeGO.AddComponent<SphereCollider>();
        Diode diode = diodeGO.AddComponent<Diode>();
        Rigidbody rb = diode.gameObject.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;
        float diodeRadius = diode.GetComponent<SphereCollider>().radius;
        GameObject debugText = GameObject.Find("DebugText");
        if (debugText != null) diode.DebugText = debugText.GetComponent<Text>();        
        diode.LastPosition = new GameObject("Last Position");        

        List<GameObject> ringObjectRoots = new List<GameObject>();
        for(int i=3; i<rootGO.transform.childCount; i++)
        {          
            ringObjectRoots.Add(rootGO.transform.GetChild(i).gameObject);
        }
        Debug.Log("this puzzle has " + ringObjectRoots.Count + " rings.");
       
        List<Ring> rings = new List<Ring>();
        List<PathNode> startNodes = new List<PathNode>();
        List<PathNode> deathNodes = new List<PathNode>();
        List<Gate> gates = new List<Gate>();        
        int numGates = 0;
        float gateSize = diode.GetComponent<SphereCollider>().radius * 2f;
        Vector3 gateScale = new Vector3(gateSize, gateSize, gateSize);
        Gate gate = Resources.Load<Gate>("Gate");

        foreach (GameObject touchControl in ringObjectRoots)
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
                    else
                    {
                        Gate g = Object.Instantiate<Gate>(gate, nodeTransform);
                        g.transform.localScale = gateScale;
                        g.name = "Gate " + (numGates++).ToString("D2");
                        gates.Add(g);
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
                if (touchControl == ringObjectRoots[ringObjectRoots.Count - 1] && end.gameObject.activeSelf == true)
                {
                    end.GetComponent<SphereCollider>().isTrigger = true;
                    deathNodes.Add(end);
                }
            }
            ring.GetComponent<Ring>().InitPaths(paths);
        }        
        lp.InitFromProcessing(diode, centerBlock, rings, gates, startNodes, deathNodes);
        diode.LastPosition.transform.parent = diode.transform.parent;       
        
        // not sure why we need this but for some reason the last expand event we try to send on the ring doesn't work
        Selection.activeObject = ringObjectRoots[ringObjectRoots.Count-1].gameObject;
        EditorWindow.focusedWindow.SendEvent(e);
        Selection.activeObject = rootGO;

        EditorSceneManager.MarkAllScenesDirty();
    }
}