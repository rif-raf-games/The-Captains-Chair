using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using System.Linq;

public class RifRafGames
{
   [MenuItem("RifRaf Games/Initialize Lockpick Puzzle Objects")]
   public static void InitializeLockpickPuzzleObjects()
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
        GameObject rootGO = Selection.activeGameObject;
        if(rootGO.GetComponent<LockPicking>() != null)
        {
            Debug.LogError("You've already initialized this puzzle.");
            return;
        }
        Debug.Log("Huzzah! Lets init these Lockpicking objects.");

        Event e = new Event();
        e.keyCode = KeyCode.RightArrow;
        e.type = EventType.KeyDown;

        int touchControlLayer = LayerMask.NameToLayer("Lockpick Touch Control");
        int ringLayer = LayerMask.NameToLayer("Lockpick Ring");
        int pathNodeLayer = LayerMask.NameToLayer("Lockpick Path Node");
        int collisionControlLayer = LayerMask.NameToLayer("Lockpick Collision Control");
        int ringLayerMask = LayerMask.GetMask("Lockpick Ring");        
                   
        LockPicking lp = rootGO.AddComponent<LockPicking>();
        rootGO.AddComponent<LockpickBoardSetup>();
        GameObject gameResultText = GameObject.Find("GameResultText");          
        if (gameResultText != null) lp.GameResultText = gameResultText.GetComponent<Text>();
        EditorWindow.focusedWindow.SendEvent(e);

        GameObject centerBlock = rootGO.transform.GetChild(0).gameObject;
        centerBlock.AddComponent<MeshCollider>();        
        centerBlock.layer = ringLayer;
        centerBlock.transform.position = Vector3.zero;

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
        float gateSize = diode.GetComponent<SphereCollider>().radius * 2f;
        Vector3 gateScale = new Vector3(gateSize, gateSize, gateSize);
        Gate gate = Resources.Load<Gate>("Gate");
        gate.name = "GetPrefab";

        foreach (GameObject touchControl in ringObjectRoots)
        {
            touchControl.AddComponent<MeshCollider>();
            touchControl.GetComponent<MeshRenderer>().enabled = false;
            touchControl.layer = touchControlLayer;
            Selection.activeObject = touchControl;
            EditorWindow.focusedWindow.SendEvent(e);

            GameObject collisionControl = touchControl.transform.GetChild(0).gameObject;            
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

           // Debug.LogWarning("we have to handle the new channel blocks");
            GameObject blockContainer = ring.transform.GetChild(0).gameObject;
            Debug.Log("this many blocks in this ring: " + blockContainer.transform.childCount);
            foreach(Transform block in blockContainer.transform)
            {
                block.gameObject.AddComponent<BoxCollider>();
                block.gameObject.layer = ringLayer;
            }

            int numPathNodes = ring.transform.childCount-1;                       
            Debug.Log("ring " + ring.name + " has " + numPathNodes + " path nodes.");
            foreach(Transform nodeTransform in ring.transform)
            {
                if (nodeTransform == ring.transform.GetChild(0)) continue;
                //Debug.Log("*************checking node: " + node.name);     
                nodeTransform.gameObject.AddComponent<PathNode>();
                nodeTransform.gameObject.layer = pathNodeLayer;
                SphereCollider sc = nodeTransform.gameObject.AddComponent<SphereCollider>();
                sc.radius = diodeRadius/3f;
                Collider[] colliders = Physics.OverlapSphere(nodeTransform.position, sc.radius, ringLayerMask);                          
            }                        
        }        
        lp.InitFromInitializing(diode, centerBlock, gate, rings);        
        diode.LastPosition.transform.parent = diode.transform.parent;       
        
        // not sure why we need this but for some reason the last expand event we try to send on the ring doesn't work
        Selection.activeObject = ringObjectRoots[ringObjectRoots.Count-1].gameObject;
        EditorWindow.focusedWindow.SendEvent(e);
        Selection.activeObject = rootGO;

        EditorSceneManager.MarkAllScenesDirty();
    }

   /* [MenuItem("RifRaf Games/Process Lockpick Puzzle")]
    public static void ProcessLockpickPuzzle()
    {
        if (Selection.activeGameObject == null)
        {
            Debug.LogError("RifRaf Error: No GameObject selected.");
            return;
        }
        if (Selection.gameObjects.Length != 1)
        {
            Debug.LogError("RifRaf Error: You must have only 1 GameObject selected.  You have " + Selection.gameObjects.Length + " selected.");
            return;
        }
        if (Selection.activeGameObject.name.Contains("Lock_Picking_Puzzle") == false)
        {
            Debug.LogError("RifRaf Error: Incorrect GameObject selected. " + Selection.activeGameObject.name + " does not have \"Lock_Picking_Puzzle\" in it's name.");
            return;
        }
        Debug.Log("Huzzah! Lets process this puzzle.");

        int ringLayerMask = LayerMask.GetMask("Lockpick Ring");
        

        GameObject rootGO = Selection.activeGameObject;
        LockPicking lp = rootGO.GetComponent<LockPicking>();
        List<GameObject> ringObjectRoots = new List<GameObject>();
        
        for(int i = 3; i < rootGO.transform.childCount-1; i++)
        {
            ringObjectRoots.Add(rootGO.transform.GetChild(i).gameObject);
        }
        Debug.Log("we have " + ringObjectRoots.Count + " rings");        

        GameObject diodeGO = rootGO.transform.GetChild(2).gameObject;
        float diodeRadius = diodeGO.GetComponent<SphereCollider>().radius;
        float nodeRadius = diodeRadius / 3f;
        List<Gate> gates = new List<Gate>();
        int numGates = 0;
        float gateSize = diodeGO.GetComponent<SphereCollider>().radius * 2f;
        Vector3 gateScale = new Vector3(gateSize, gateSize, gateSize);
        Gate gate = Resources.Load<Gate>("Gate");
        List<Ring> rings = new List<Ring>();
        List<Transform> activeNodes = new List<Transform>();
        List<PathNode> startNodes = new List<PathNode>();
        List<PathNode> deathNodes = new List<PathNode>();
        foreach (GameObject touchControl in ringObjectRoots)
        {
            GameObject ring = touchControl.transform.GetChild(0).GetChild(0).gameObject;
            rings.Add(ring.GetComponent<Ring>());
            //Debug.Log("ring name: " + ring.name + " has " + ring.transform.childCount + " children.");

            foreach (Transform nodeTransform in ring.transform)
            {
                if (nodeTransform == ring.transform.GetChild(0)) continue;
                
                nodeTransform.gameObject.SetActive(true);
                nodeTransform.GetComponent<SphereCollider>().isTrigger = false;
                if(nodeTransform.childCount != 0)
                {
                    foreach(Transform child in nodeTransform)
                    {
                        Object.DestroyImmediate(child.gameObject);
                    }
                }

                Collider[] colliders = Physics.OverlapSphere(nodeTransform.position, nodeRadius, ringLayerMask);
                if (nodeTransform.name.Contains("Middle"))
                {
                    if (colliders.Length == 0 || colliders.Length == 2)
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
                        if (c.transform.parent.parent == ring.transform)
                        {
                            nodeTransform.gameObject.SetActive(false);
                            break;
                        }
                    }
                }
                if (nodeTransform.gameObject.activeSelf == true) activeNodes.Add(nodeTransform);
            }
            Debug.Log("****************ring " + ring.name + "'s stats:");
            Debug.Log("num gates so far: " + numGates);
            int numPaths = activeNodes.Count / 2;
            Debug.Log("num active nodes: " + activeNodes.Count + " means " + numPaths + " paths.");            
            List<Transform> sortedList = activeNodes.OrderBy(o => o.name.Substring(o.name.Length - 2)).ToList<Transform>();
            foreach(Transform node in sortedList)
            {
                string subString = node.name.Substring(node.name.Length - 2);
                Debug.Log("i'm active: " + node.name + " with subString: " + subString);
            }
            // setup the paths
            Ring.LockpickRingPath[] paths = new Ring.LockpickRingPath[numPaths];
            for (int i = 0; i < numPaths; i++)
            {
                paths[i] = new Ring.LockpickRingPath();
                paths[i].Start = sortedList[(i * 2) + 1].gameObject.GetComponent<PathNode>();
                paths[i].End =   sortedList[(i * 2)].gameObject.GetComponent<PathNode>();
                paths[i].Init(ring.GetComponent<Ring>());

                if (touchControl == ringObjectRoots[0] && paths[i].Start.name.Contains("Start")) startNodes.Add(paths[i].Start);
                if (touchControl == ringObjectRoots[ringObjectRoots.Count - 1] && paths[i].End.name.Contains("End"))
                {
                    paths[i].End.GetComponent<SphereCollider>().isTrigger = true;
                    deathNodes.Add(paths[i].End);
                }
            }
            ring.GetComponent<Ring>().InitPaths(paths);            
            activeNodes.Clear();
        }
        lp.InitFromProcessing(rings, gates, startNodes, deathNodes);
    }*/
}