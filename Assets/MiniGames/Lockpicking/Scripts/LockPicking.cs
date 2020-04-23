using Articy.The_Captain_s_Chair;
using Articy.The_Captain_s_Chair.GlobalVariables;
using Articy.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;
using UnityEditor;

public class LockPicking : MiniGame
{
    enum eGameState { OFF, ON };
    eGameState CurGameState = eGameState.OFF;

    public Diode Diode;
    public GameObject CenterBlock;
    public List<Ring> Rings;
    public List<float> RingCamYs = new List<float>();
    public Gate GatePrefab;
    public List<Gate> Gates;
    public List<Gate> GatesThisGame = new List<Gate>();
    public List<PathNode> StartNodes;
    public List<PathNode> DeathNodes;
    public Ring CurTouchedRing = null;
    float LargestRingDiameter;
    Vector3 LastWorldTouchPos = Vector3.zero;
    float LastCenterToWorldAngle;
    public Text ResultsText;
    public Text DebugText;

    Diode EvilDiodePrefab;
    float GameTime;
    [Header("Evil Diode Tuning")]
    public List<Diode> EvilDiodes = new List<Diode>();
    public int MaxEvilDiodes = 5;               // Number of enemy diodes (0-x) max in the game
    public List<float> EvilDiodeSpeeds = new List<float>(); // starting speed of each evil diode
    public float TimeBeforeEvilSpawn = 3f;      // Time since game start for enemy diode to appear
    public float TimeBetweenEvilSpawns = 2f;    // Time between enemy diodes spawning
    public float EvilRespawnTime = 1f;              // Delay time between an enemy diode falling out of the board and it spawning again from the first ring
    public float NumSpawnAtATime = 2f;          // Number of enemy diodes that spawn at a time
    [Header("Diode Speed Tuning")]
    public float MAX_DIODE_SPEED = 40f;         // Maximum speed a Diode can go
    public float SpeedAdjTime = 0f;             // Time goes on
    public float SpeedAdjNumGates = 1f;         // Number of gates collected        
    
    public float AdjustDiodeSpeed(float startSpeed)
    {
        // time adjustment
        float timeAdj = SpeedAdjTime * GameTime * .01f;   
        // num gates collected adjustment
        float gatesAdj = SpeedAdjNumGates * (Gates.Count - GatesThisGame.Count);        

        float newSpeed = startSpeed + timeAdj + gatesAdj;
        if (newSpeed > MAX_DIODE_SPEED) newSpeed = MAX_DIODE_SPEED;
        return newSpeed;
    }
    public void CheckDeathNode(Diode diode, PathNode pathNode)
    {
        if (IsDeathNode(pathNode))
        {
            if (diode.Evil == false) StartCoroutine(EndGame("You Lost.", false));
            else StartCoroutine(EvilDiodeRespawn(diode));
        }
    }
    IEnumerator EvilDiodeRespawn(Diode evilDiode)
    {
        yield return new WaitForSeconds(EvilRespawnTime);
        bool startNodeFound = false;
        while (startNodeFound == false)
        {
            yield return new WaitForEndOfFrame();
            startNodeFound = SpawnEvilDiodes(evilDiode);
        }
    }

    public override void Init(MiniGameMCP mcp, string sceneName)
    {
        //Debug.Log("LockPicking.Init()");
        base.Init(mcp, sceneName);
        if (ResultsText == null) ResultsText = MCP.ResultsText;
        if (DebugText == null) DebugText = MCP.DebugText;
        ResultsText.gameObject.SetActive(false);
    }

    public override void Awake()
    {
        base.Awake();
        Physics.autoSimulation = true;
        Diode.SetLockPickingComponent(this);
        EvilDiodePrefab = Resources.Load<Diode>("LockPicking/Evil Diode");

        List<Diode> diodes = FindObjectsOfType<Diode>().ToList();
        foreach(Diode d in diodes)
        {
            if (d.Evil == true) EvilDiodes.Add(d);
        }
        foreach(Diode d in EvilDiodes)
        {
            d.SetLockPickingComponent(this);
        }
    }

    private void Start()
    {
        for (int i = 0; i < Rings.Count; i++)
        {
            Ring r = Rings[i];
            MeshCollider mc = r.transform.parent.transform.parent.GetComponent<MeshCollider>();
            Bounds b = mc.bounds;
            Vector3 brWorld = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, Camera.main.transform.position.y));
            Vector3 size = brWorld * 2f;
            float ratio = size.x / b.size.x;
            float newY = Camera.main.transform.position.y / ratio;
            //Debug.Log(i);
            RingCamYs.Add(newY);
        }
        if (IsSolo == true)
        {
            ResultsText.gameObject.SetActive(false);
            BeginPuzzleStartTime();
        }                
    }
    // Start is called before the first frame update
    public override void BeginPuzzleStartTime()
    {
        //Debug.Log("Lockpicking.BeginPuzzle()");   
        base.BeginPuzzleStartTime();
        CenterBlock.transform.position = new Vector3(CenterBlock.transform.position.x, 0f, CenterBlock.transform.position.z);              
        LargestRingDiameter = Rings[Rings.Count-1].GetComponent<MeshCollider>().bounds.extents.x;

        StartGame();
    }    

    void StartGame()
    {
        SetGameState(eGameState.ON); 
        Diode.Moving = true;
        ResultsText.gameObject.SetActive(false);
        GameTime = 0f;
        // Diode tuning stuff
        EvilSpawnBegan = false;
        EvilDiodeTimer = 0f;
        foreach(Diode d in EvilDiodes)
        {
            Destroy(d.gameObject);
        }
        EvilDiodes.Clear();

        //List<PathNode> availStartNodes = new List<PathNode>(StartNodes);
        GatesThisGame.Clear();
        GatesThisGame = new List<Gate>(Gates);
        if (Diode.DebugStartNode != null)
        {
            Diode.ResetDiodeForGame(Diode.DebugStartNode);
        }
        else
        {
            int startIndex = Random.Range(0, StartNodes.Count);
            Diode.ResetDiodeForGame(StartNodes[startIndex]);
           // usedPathNodes.Add(StartNodes[startIndex]);
        }        

        foreach(Gate g in Gates)
        {
            g.gameObject.SetActive(true);
        }
    }

    eGameState DialogueSaveState;
    public override void ResetPostDialogueState()
    {
        base.ResetPostDialogueState();
        CurGameState = DialogueSaveState;
    }
    void SetGameState(eGameState newState)
    {
        DialogueSaveState = newState;
        if (DialogueActive == false)
        {
            CurGameState = newState;
        }
    }

    public void CollectGate(Gate gate)
    {
        // Debug.Log("found gate: " + gate.name);
        GatesThisGame.Remove(gate);
        gate.gameObject.SetActive(false);
        //if (true)
        if(GatesThisGame.Count == 0)        
        {
            StartCoroutine(EndGame("You Won.", true));
        }        
    }

    public void HitEvilDiode(Diode evilDiode)        
    {
        StartCoroutine(EndGame("You were killed by an evil diode.", false));
    }
    public bool IsDeathNode(PathNode pathNode)
    {
        return DeathNodes.Contains(pathNode);
    }
           

    IEnumerator EndGame(string endGameString, bool success)
    {
        if (MCP != null) MCP.SavePuzzlesProgress(success);
        if(success == true) EndPuzzleTime(true);
        SetGameState(eGameState.OFF); 
        ResultsText.gameObject.SetActive(true);
        ResultsText.text = endGameString;
        Diode.Moving = false;
        foreach (Diode d in EvilDiodes) d.Moving = false;
        yield return new WaitForSeconds(3);
        ResultsText.gameObject.SetActive(false);
        if (success == true)
        {
            if (MCP != null) MCP.PuzzleFinished();
            else SceneManager.LoadScene("LockPickingDemo");//else Debug.Log("We're not part of an MCP so figure out what next to do");
        }
        else
        {
            StartGame();
        }
        //StartCoroutine(HandleEndGame());
    }

    private void FixedUpdate()
    {
        if (CurGameState == eGameState.OFF || DialogueActive == true) return;
        foreach (Diode d in EvilDiodes)
        {
            d.DiodeFixedUpdate();
        }
        Diode.DiodeFixedUpdate();
        RotateRings();
    }

    // Update is called once per frame
    void Update()
    {
        if (DebugText != null)
        {
            DebugText.text = CurGameState.ToString() + "\n";
            DebugText.text += PuzzleStartTime + "\n";
            /*DebugText.text = "";
            DebugText.text = "GameTime: " + GameTime.ToString("F2") + "\n";
            DebugText.text += "\n";
            DebugText.text += "EvilSpanBegan: " + EvilSpawnBegan + "\n";
            DebugText.text += "Num Evil Diodes: " + EvilDiodes.Count + "\n";
            DebugText.text += "MaxEvilDiodes: " + MaxEvilDiodes + "\n";
            DebugText.text += "EvilDiodeTimer: " + EvilDiodeTimer.ToString("F2") + "\n";
            DebugText.text += "\n";
            DebugText.text += "Diode speed: " + Diode.CurSpeed.ToString("F2") + "\n";*/
        }
        if (CurGameState == eGameState.OFF) return;
        GameTime += Time.deltaTime;
        if (Input.GetMouseButtonDown(0)) // initial press
        {                        
            LayerMask mask = LayerMask.GetMask("Lockpick Touch Control");
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask))
            {
                CurTouchedRing = hit.collider.gameObject.transform.GetChild(0).transform.GetChild(0).GetComponent<Ring>();
                LastWorldTouchPos = GetWorldPointFromMouse();
                LastCenterToWorldAngle = GetAngle(LastWorldTouchPos, LastWorldTouchPos.z < 0f);
            }
        }        
        else if (Input.GetMouseButton(0) && CurTouchedRing != null)
        {            
            Vector3 touchVec = GetWorldPointFromMouse();
            Vector3 dragVec = touchVec - LastWorldTouchPos;
            float touchDragAngleDiff = Mathf.Abs(Vector3.SignedAngle(dragVec, touchVec, Vector3.up));
            if (touchDragAngleDiff > 90) touchDragAngleDiff = 180f - touchDragAngleDiff;                
            float centerToWorldTouchPosAngle = GetAngle(touchVec, touchVec.z < 0f);

            float worldMoveDist = Vector3.Distance(touchVec, LastWorldTouchPos);
            if (worldMoveDist > 0f)
            {
                float speed = touchDragAngleDiff * worldMoveDist * 12f;
                if (centerToWorldTouchPosAngle > LastCenterToWorldAngle) speed = -speed;
                float speedAdj = 1f / (CurTouchedRing.GetComponent<MeshCollider>().bounds.extents.x / LargestRingDiameter);
                CurTouchedRing.SetTouchRotateSpeed(speed * speedAdj);
            }
            else
            {
                CurTouchedRing.SetTouchRotateSpeed(0f);
            }

            LastWorldTouchPos = touchVec;
            LastCenterToWorldAngle = centerToWorldTouchPosAngle;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (CurTouchedRing != null)
            {
                CurTouchedRing.ResetRotateSpeed();
                CurTouchedRing = null;
            }            
        }

        // evil diode stuff
        if(EvilDiodes.Count < MaxEvilDiodes)
        {
            if (EvilSpawnBegan == false)
            {
                EvilDiodeTimer += Time.deltaTime;
                if (EvilDiodeTimer >= TimeBeforeEvilSpawn)
                {
                    EvilSpawnBegan = true;
                    EvilDiodeTimer = 0f;
                    SpawnEvilDiodes();
                }
            }
            else if (EvilSpawnBegan == true)
            {
                EvilDiodeTimer += Time.deltaTime;
                if (EvilDiodeTimer >= TimeBetweenEvilSpawns)
                {
                    EvilDiodeTimer = 0f;
                    SpawnEvilDiodes();
                }
            }
        }        
        
    }
    bool SpawnEvilDiodes(Diode respawnDiode = null)
    {
        //int numToSpawn = (int)Mathf.Min((float)NumSpawnAtATime, (float)(MaxEvilDiodes - EvilDiodes.Count));
        int numToSpawn;
        if (respawnDiode == null) numToSpawn = (int)Mathf.Min((float)NumSpawnAtATime, (float)(MaxEvilDiodes - EvilDiodes.Count));
        else numToSpawn = 1;
        Debug.Log("SpawnEvilDiodes() numToSpawn: " + numToSpawn);
        if(numToSpawn == 0)
        {
            Debug.Log("there's no room to spawn any more evil nodes");
            return false;
        }
        List<PathNode> availStartNodes = new List<PathNode>(StartNodes);
        if (StartNodes.Contains(Diode.CurPath.Start)) availStartNodes.Remove(Diode.CurPath.Start);
        foreach(Diode d in EvilDiodes) if(StartNodes.Contains(d.CurPath.Start)) availStartNodes.Remove(d.CurPath.Start);
        Debug.Log("num avail start Nodes: " + availStartNodes.Count);        
        for (int i=0; i<numToSpawn; i++)
        {
            if(availStartNodes.Count == 0)
            {
                Debug.Log("No more available nodes to spawn an evil diode");
                return false;
            }
            Diode d;
            if (respawnDiode == null )
            {
                d = Object.Instantiate(EvilDiodePrefab, this.transform.parent);
                EvilDiodes.Add(d);
                d.SetLockPickingComponent(this);
                d.StartSpeed = EvilDiodeSpeeds[EvilDiodes.Count - 1];
            }
            else
            {
                d = respawnDiode;
            }
            //Diode d = Object.Instantiate(EvilDiodePrefab, this.transform.parent);
            //EvilDiodes.Add(d);
            //d.SetLockPickingComponent(this);
            //d.StartSpeed = EvilDiodeSpeeds[EvilDiodes.Count - 1];
            d.CurSpeed = d.StartSpeed;
            d.Moving = true;
            if (d.DebugStartNode != null)
            {
                d.ResetDiodeForGame(d.DebugStartNode);
            }
            else
            {
                int startIndex = Random.Range(0, availStartNodes.Count);
                PathNode evilNode = availStartNodes[startIndex];
                availStartNodes.Remove(evilNode);
                d.ResetDiodeForGame(evilNode);
            }
        }
        return true;
    }

    

    bool EvilSpawnBegan;
    float EvilDiodeTimer;
    
    // ou can rotate a direction Vector3 with a Quaternion by multiplying the quaternion with the direction(in that order)
    //    Then you just use Quaternion.AngleAxis to create the rotation

    public void RotateRings()
    {
        foreach (Ring r in Rings)
        {
            r.Rotate();
        }
    }
    
    // monote - add this to StaticStuff
    Vector3 GetWorldPointFromMouse()
    {
        Vector3 mousePos = Input.mousePosition;
        Vector3 m = new Vector3(mousePos.x, mousePos.y, 10f);
        return Camera.main.ScreenToWorldPoint(m);
    }
    float GetAngle(Vector3 dir, bool adjust)
    {
        float rot = Vector3.Angle(dir, Vector3.right);
        if (adjust) rot = 360f - rot;
        if (rot >= 360f) rot = rot - 360f;
        return rot;
    }

    

    public void InitFromInitializing(Diode diode, GameObject centerBlock, Gate gatePrefab, List<Ring> rings)
    {
        this.Diode = diode;
        CenterBlock = centerBlock;
        GatePrefab = gatePrefab;
        Rings = rings;
    }
    public void InitFromProcessing( /*Diode diode, GameObject centerBlock, List<Ring> rings,*/ List<Gate> gates, List<PathNode> startNodes, List<PathNode> deathNodes)
    {
        Gates = gates;
        StartNodes = startNodes;
        DeathNodes = deathNodes;

        float speed = 10f;
        foreach (Ring r in Rings)
        {
            r.SetDefaultRotateSpeed(speed);
            speed = -speed;
        }
    }

    public void ProcessBoardSetup()
    {
        GameObject go = GameObject.Find("goatfucker");
        Debug.Log(go == null);
        Debug.Log("process lockpicking board setup");
        float nodeRadius = Diode.GetComponent<SphereCollider>().radius / 3f;
        int ringLayerMask = LayerMask.GetMask("Lockpick Ring");        
        float gateSize = Diode.GetComponent<SphereCollider>().radius * 2f;
        Vector3 gateScale = new Vector3(gateSize, gateSize, gateSize);
        int numGates = 0;
        List<Gate> gates = new List<Gate>();
        List<Transform> activeNodes = new List<Transform>();
        List<PathNode> startNodes = new List<PathNode>();
        List<PathNode> deathNodes = new List<PathNode>();

        foreach (Ring ring in Rings)
        {
            foreach (Transform nodeTransform in ring.transform)
            {
                if (nodeTransform == ring.transform.GetChild(0)) continue;
                nodeTransform.gameObject.SetActive(true);
                nodeTransform.GetComponent<SphereCollider>().isTrigger = false;
                if (nodeTransform.childCount != 0)
                {
                    foreach (Transform child in nodeTransform)
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
                        Gate g = Object.Instantiate<Gate>(GatePrefab, nodeTransform);
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
            foreach (Transform node in sortedList)
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
                paths[i].End = sortedList[(i * 2)].gameObject.GetComponent<PathNode>();
                paths[i].Init(ring.GetComponent<Ring>());

                if (ring == Rings[0] && paths[i].Start.name.Contains("Start")) startNodes.Add(paths[i].Start);
                if (ring == Rings[Rings.Count - 1] && paths[i].End.name.Contains("End"))
                {
                    paths[i].End.GetComponent<SphereCollider>().isTrigger = true;
                    deathNodes.Add(paths[i].End);
                }
            }
            ring.GetComponent<Ring>().InitPaths(paths);
            activeNodes.Clear();
        }
        InitFromProcessing(gates, startNodes, deathNodes);

#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(Screen.width - 100, 0, 100, 100), "Main Menu"))
        {
            EndPuzzleTime(false);
            SceneManager.LoadScene("LockPickingDemo");
        }
    }
}
