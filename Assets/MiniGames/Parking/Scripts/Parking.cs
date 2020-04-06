﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEditor;
using UnityEngine.SceneManagement;

public class Parking : MiniGame
{    
    enum eTouchState { NONE, CLICK, RELEASE, HOLD };    // available mouse/finger touch states
    eTouchState TouchState; // current touch state
    float InputTimer;   // timer used to keep track of input state
    public float HoldTime = .25f; // how long until a CLICK becomes a HOLD
    public float RaiserLowerTime = .2f;
    public float DirChangeBuffer = .3f;

    enum eGameState { OFF, NORMAL, ROTATE_PAD };
    eGameState CurGameState = eGameState.OFF;
    
    public ParkingShip ActiveShip = null;
    ParkingShip ClickedShip = null;
    bool TouchingRotatePad = false;

    Vector3 CurTouchPos = Vector3.zero;
    Vector3 LastTouchPos = Vector3.zero;

    List<ParkingShip> AllShips = new List<ParkingShip>();
    List<ParkingShip> TargetShips = new List<ParkingShip>();
    public GameObject LiftPad;
    Quaternion LerpRotStart, LerpRotEnd;
    float LerpStartTime, LerpDurationTime;

    public Text ResultsText;
    [Header("Debug")]
    public Text DebugText;

    public override void Init(MiniGameMCP mcp)
    {
        //Debug.Log("Parking.Init()");
        base.Init(mcp);
        if (ResultsText == null) ResultsText = MCP.ResultsText;
        if (DebugText == null) DebugText = MCP.DebugText;
        ResultsText.gameObject.SetActive(false);
    }

    private void Start()
    {        
        if (IsSolo == true)
        {
            SoundFX soundFX = FindObjectOfType<SoundFX>();
            SoundFXPlayer.Init(soundFX);
            ResultsText.gameObject.SetActive(false);
            BeginPuzzle();
        }
    }

    public override void BeginPuzzle()
    {        
        TouchState = eTouchState.NONE;
        CurGameState = eGameState.NORMAL;
        ContainGO = new GameObject();

        AllShips.Clear();
        AllShips = FindObjectsOfType<ParkingShip>().ToList();
        foreach(ParkingShip ship in AllShips)
        {
            if(ship.ShipType == ParkingShip.eParkingShipType.TARGET)
            {
                TargetShips.Add(ship);
                GameObject sphere = CreateSphere(ship.transform, "target", Color.cyan, false);
                sphere.transform.position += new Vector3(0f, ship.GetComponent<BoxCollider>().size.y, 0f);
                sphere.transform.localScale = new Vector3(.2f, .2f, .2f);
                sphere.transform.parent = ship.transform;
            }
        }
    }

    public void RotateGridPlatform()
    {
        RotateShipList.Clear();
        foreach (GameObject sphere in DebugSpheres)
        {
            Destroy(sphere);
        }
        DebugSpheres.Clear();
        //Debug.Log("check rot platform");
        BoxCollider box = RotatePlatform.GetComponent<BoxCollider>();
        Vector3 size = box.size;
        //Debug.Log("before size: " + size.ToString("F2"));
        size -= new Vector3(.1f, 0f, .1f);
        //Debug.Log("after size: " + size.ToString("F2"));
        Collider[] colliders = Physics.OverlapBox(RotatePlatform.transform.position, size / 2, RotatePlatform.transform.rotation);
        if (colliders != null && colliders.Length > 0)
        {
            foreach (Collider c in colliders)
            {
                //Debug.Log("collided with: " + c.name);                                
                if (c.GetComponent<ParkingShip>() != null) RotateShipList.Add(c.GetComponent<ParkingShip>());
            }
        }

        bool allShipsContained = true;
        foreach (ParkingShip ship in RotateShipList)
        {
            List<Vector3> checkPoints = new List<Vector3>();
            //GameObject go;
            Vector3 moveVec;
            BoxCollider shipBox = ship.GetComponent<BoxCollider>();
            Bounds b = shipBox.bounds;
            Vector3 shipMax = b.max;
            Vector3 shipMin = b.min;

            // min check spot                
            ContainGO.transform.position = shipMin;
            moveVec = shipBox.bounds.center - shipMin;
            ContainGO.transform.Translate(moveVec * .1f);
            //CreateSphere(ContainGO.transform, "_rawMin", Color.red);
            ContainGO.transform.position = new Vector3(ContainGO.transform.position.x, RotateBox01.bounds.center.y, ContainGO.transform.position.z);
            // CreateSphere(ContainGO.transform, "_checkMin", Color.blue);
            checkPoints.Add(ContainGO.transform.position);

            // max check spot                
            ContainGO.transform.position = shipMax;
            moveVec = shipBox.bounds.center - shipMax;
            ContainGO.transform.Translate(moveVec * .1f);
            // CreateSphere(ContainGO.transform, "_rawMax", Color.red);
            ContainGO.transform.position = new Vector3(ContainGO.transform.position.x, RotateBox01.bounds.center.y, ContainGO.transform.position.z);
            //  CreateSphere(ContainGO.transform, "_checkMax", Color.blue);
            checkPoints.Add(ContainGO.transform.position);

            bool inBox01 = RotateBox01.bounds.Contains(checkPoints[0]) && RotateBox01.bounds.Contains(checkPoints[1]);
            bool inBox02 = RotateBox02.bounds.Contains(checkPoints[0]) && RotateBox02.bounds.Contains(checkPoints[1]);
            if (inBox01 || inBox02)
            {
                //Debug.Log("ship: " + ship.name + " is fully contained within one of the rotate boxes");
            }
            else
            {
                //Debug.Log("ship: " + ship.name + " is NOT fully contained within any of the rotate boxes so can't rotate");
                allShipsContained = false;
                break;
            }
        }
        //Debug.Log("are all ships contained?: " + allShipsContained.ToString());
        if (allShipsContained == true || RotateShipList.Count == 0)
        {
            SoundFXPlayer.Play("RotatePad");
            foreach (ParkingShip ship in RotateShipList)
            {
                ship.transform.parent = RotateParent.transform;
            }
            LerpRotStart = RotateParent.transform.rotation;
            RotateParent.transform.Rotate(0f, 90f, 0f);
            LerpRotEnd = RotateParent.transform.rotation;
            RotateParent.transform.Rotate(0f, -90f, 0f);
            LerpStartTime = Time.time;
            LerpDurationTime = .5f;
            CurGameState = eGameState.ROTATE_PAD;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("update");
        if (DebugText != null) DebugText.text = CurGameState.ToString();
        if (CurGameState == eGameState.OFF) return;
        if(CurGameState == eGameState.ROTATE_PAD)
        {
            float lerpTime = Time.time - LerpStartTime;
            float lerpPercentage = lerpTime / LerpDurationTime;
            RotateParent.transform.rotation = Quaternion.Lerp(LerpRotStart, LerpRotEnd, lerpPercentage);
            if(lerpPercentage >= 1f)
            {
                RotateParent.transform.rotation = LerpRotEnd;
                CurGameState = eGameState.NORMAL;

                List<ParkingShip> ships = new List<ParkingShip>();
                ships = RotateParent.GetComponentsInChildren<ParkingShip>().ToList();
                //Debug.Log("putting " + ships.Count + " back");
                foreach(ParkingShip ship in ships)
                {
                    ship.transform.parent = this.transform;
                }
            }
            return;
        }
        float mouseMoveAngle = Mathf.NegativeInfinity;
        ParkingShip.eMoveDir curMoveDir = ParkingShip.eMoveDir.NONE;
        float MoveDistance = 0f;
        if (Input.GetMouseButtonDown(0))
        {
            LayerMask mask = LayerMask.GetMask("Parking Ship");
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask))
            {
               // Debug.Log("Clicked on a ship.");
                ParkingShip ship = hit.collider.gameObject.GetComponent<ParkingShip>();
                if (ship == null) { Debug.LogError("Clicked on a ship with no ParkingShip component: " + hit.collider.name); return; }
                //SetActiveShip(ship);
                ClickedShip = ship;
                TouchState = eTouchState.CLICK;
                InputTimer = 0f;
            }
            mask = LayerMask.GetMask("Parking Rotate Platform");
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask))
            {
                TouchingRotatePad = true;
            }
        }
        else if (Input.GetMouseButton(0))
        {
            InputTimer += Time.deltaTime;
            if (TouchState == eTouchState.CLICK)
            {                
                if (InputTimer >= HoldTime)
                {
                    TouchState = eTouchState.HOLD;
                    SetActiveShip(ClickedShip);
                    ActiveShip.BeginHold(RaiserLowerTime);
                    CurTouchPos = Input.mousePosition;
                    LastTouchPos = Input.mousePosition;
                    TouchingRotatePad = false;
                }
            }
            else if (TouchState == eTouchState.HOLD && ActiveShip.GetState() == ParkingShip.eParkingShipState.RAISED)
            {
                CurTouchPos = Input.mousePosition;
                mouseMoveAngle = Vector3.Angle(CurTouchPos - LastTouchPos, Vector3.right);
                if (mouseMoveAngle >= 47.5f && mouseMoveAngle <= 132.5f) curMoveDir = ParkingShip.eMoveDir.VERTICAL;
                else if (mouseMoveAngle <= 42.5f || mouseMoveAngle >= 137.5f) curMoveDir = ParkingShip.eMoveDir.HORIZONTAL;
                else curMoveDir = ParkingShip.eMoveDir.NONE;
                if (curMoveDir == ParkingShip.eMoveDir.HORIZONTAL) MoveDistance = CurTouchPos.x - LastTouchPos.x;
                else if (curMoveDir == ParkingShip.eMoveDir.VERTICAL) MoveDistance = CurTouchPos.y - LastTouchPos.y;
                if (ActiveShip.GetMoveDir() == ParkingShip.eMoveDir.NONE && curMoveDir != ParkingShip.eMoveDir.NONE && MoveDistance != 0f)
                {
                    ActiveShip.SetMoveDir(curMoveDir);
                }
                if (ActiveShip.GetMoveDir() != ParkingShip.eMoveDir.NONE && curMoveDir != ParkingShip.eMoveDir.NONE && MoveDistance != 0f)
                {
                    if (ActiveShip.GetMoveDir() != curMoveDir)
                    {
                        Vector3 closestCenteredPos = GetClosestCenteredPoint(ActiveShip.gameObject);
                        //float dec;
                        // want to change directions of movement eh?
                        if (ActiveShip.GetMoveDir() == ParkingShip.eMoveDir.HORIZONTAL)
                        {   // want to start moving the ship vertically
                            //float newX = GetClosestRoundedVal(ActiveShip.transform.position.x, out dec);
                            float diffX = Mathf.Abs(closestCenteredPos.x - ActiveShip.transform.position.x);
                            //Debug.Log("went from X: " + ActiveShip.transform.position.x.ToString("F2") + " to this spot: " + closestCenteredPos.ToString("F2") + ", diffX was: " + diffX.ToString("F2"));
                            if (diffX < DirChangeBuffer)
                            {
                                ActiveShip.transform.position = new Vector3(closestCenteredPos.x, ActiveShip.transform.position.y, ActiveShip.transform.position.z);
                                ActiveShip.SetMoveDir(ParkingShip.eMoveDir.VERTICAL);
                            }
                        }
                        else if (ActiveShip.GetMoveDir() == ParkingShip.eMoveDir.VERTICAL)
                        {   // want to start moving shipt horizontally
                            //float newZ = GetClosestRoundedVal(ActiveShip.transform.position.z, out dec);
                            float diffZ = Mathf.Abs(closestCenteredPos.z - ActiveShip.transform.position.z);
                            //Debug.Log("went from Z: " + ActiveShip.transform.position.z.ToString("F2") + " to this spot: " + closestCenteredPos.ToString("F2") + ", diffZ was: " + diffZ.ToString("F2"));
                            if (diffZ < DirChangeBuffer)
                            {
                                ActiveShip.transform.position = new Vector3(ActiveShip.transform.position.x, ActiveShip.transform.position.y, closestCenteredPos.z);
                                ActiveShip.SetMoveDir(ParkingShip.eMoveDir.HORIZONTAL);
                            }
                        }
                    }
                    if (ActiveShip.GetMoveDir() == curMoveDir)
                    {
                        bool validMove = true;
                        Vector3 moveDelta = Vector3.zero;
                        Vector3 lastWorld = Camera.main.ScreenToWorldPoint(new Vector3(LastTouchPos.x, LastTouchPos.y, Camera.main.transform.position.y));
                        Vector3 curWorld = Camera.main.ScreenToWorldPoint(new Vector3(CurTouchPos.x, CurTouchPos.y, Camera.main.transform.position.y));
                        if (ActiveShip.GetMoveDir() == ParkingShip.eMoveDir.HORIZONTAL)
                        {   // if we're HORIZONTAL, only take into account the x delta
                            moveDelta.x = curWorld.x - lastWorld.x;
                            if (Mathf.Abs(moveDelta.x) > .95f) validMove = false;   // if moved too far, not a valid move                        
                        }
                        else if (ActiveShip.GetMoveDir() == ParkingShip.eMoveDir.VERTICAL)
                        {
                            // if we're VERTICAL, only take into account the z delta
                            moveDelta.z = curWorld.z - lastWorld.z;
                            if (Mathf.Abs(moveDelta.z) > .95f) validMove = false; // if moved too far, not a valid move                                     
                        }
                        if (validMove == false)
                        {
                            Debug.LogWarning("WARNING: move too fast: " + moveDelta.ToString("F3")); // bail if we're trying to move too far
                        }
                        else
                        {
                            ActiveShip.Move(moveDelta);
                        }
                    }
                }
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            InputTimer += Time.deltaTime;
            if (ActiveShip != null)
            {
                //Debug.Log("GMU() BeginLower: " + InputTimer);
                ActiveShip.BeginLower(RaiserLowerTime);
            }            
            if (TouchingRotatePad == true && InputTimer <= HoldTime)
            {
               // Debug.Log("GMU() Rotate: " + InputTimer);
                RotateGridPlatform();
            }
            TouchingRotatePad = false;
            InputTimer = 0f;
            SetActiveShip(null);
            TouchState = eTouchState.NONE;
        }

        if (DebugText != null)
        {
            string s = (ActiveShip == null ? "null ActiveShip" : ActiveShip.name) + "\n";
            DebugText.text = s;            
        }
        LastTouchPos = CurTouchPos;
    }

    void SetActiveShip(ParkingShip ship)
    {
        //if (ship != null) Debug.Log("SetActiveShip() " + ship.name);
        ActiveShip = ship;
        ClickedShip = null;
    }

    public void SetBoardPiecesInEditor()
    {       
        Debug.Log("Parking.SetBoardPiecesInEditor()");        
        List<GameObject> parkingBoardItems = GameObject.FindGameObjectsWithTag("Parking Board Item").ToList();
        
        Debug.Log("num items: " + parkingBoardItems.Count);

        foreach(GameObject item in parkingBoardItems)
        {
           // Debug.Log("**********************************************");
            BoxCollider box = item.GetComponent<BoxCollider>();
            //Debug.Log("item: " + item.name + ", is at: " + item.transform.position.ToString("F2") + ", euler: " + item.transform.eulerAngles.ToString("F2"));
            float newRot = GetNewRot(item.transform.eulerAngles.y);
            float rotDiff = newRot - item.transform.eulerAngles.y;
            Vector3 center = box.bounds.center;
            item.transform.RotateAround(center, Vector3.up, rotDiff);
            //GetClosestCenteredPoint(item);
            item.transform.position = GetClosestCenteredPoint(item);                        
        }
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
    }

    public Vector3 GetClosestCenteredPoint(GameObject item )
    {
       // Debug.Log("GetClosestCenteredPoint() " + item.name);
        float dec;
        BoxCollider box = item.GetComponent<BoxCollider>();
        Vector3 size = box.size;
        //Debug.Log("orig size: " + size.ToString("F2"));
        float rotY = item.transform.eulerAngles.y;
        if (Mathf.Approximately(rotY, 90f) || Mathf.Approximately(rotY, 270f))
        {            
            size = new Vector3(size.z, size.y, size.x);
        }
        //Debug.Log("after size: " + size.ToString("F2"));
        Vector3 pos = item.transform.position;
        //Debug.Log("pos: " + pos.ToString("F2"));
        float offsetX = (((int)size.x) % 2 == 1 ? .5f : 0f);
        float offsetZ = (((int)size.z) % 2 == 1 ? .5f : 0f);

        float newX = GetClosestRoundedVal(item.transform.position.x, out dec, offsetX);
        float newZ = GetClosestRoundedVal(item.transform.position.z, out dec, offsetZ);

        return new Vector3(newX, item.transform.position.y, newZ);        
    }
    public float GetClosestRoundedVal(float curVal, out float dec, float offset = 0f)
    {
        curVal += offset;
        float newVal = Mathf.Floor(curVal);
        dec = curVal - newVal;
        if (dec != 0f)
        {
            if (dec > .5f) newVal = newVal + 1f;
        }
        newVal -= offset;
        return newVal;
    }
    float GetNewRot(float rot)
    {
        int remainder = (int)(rot / 90f);
        int newRot = (int)(remainder * 90);
        if ((rot - newRot) > 45f) newRot += 90;
        return newRot;
    }

    public GameObject RotatePlatform;
    public BoxCollider RotateBox01;
    public BoxCollider RotateBox02;
    public GameObject RotateParent;
    List<GameObject> DebugSpheres = new List<GameObject>();
    public List<ParkingShip> RotateShipList = new List<ParkingShip>();
    public List<ParkingShip> LiftPadShipList = new List<ParkingShip>();
    GameObject ContainGO;    
       
    bool AllShipsLowered()
    {
        foreach(ParkingShip ship in AllShips)
        {
            if (ship.GetState() != ParkingShip.eParkingShipState.LOWERED) return false;
        }
        return true;
    }

    public void CheckGameFinish()
    {
        bool allLowered = AllShipsLowered();
        if(allLowered == false)
        {
            Debug.LogError("All ships should be lowered at this point");
            return;
        }
                
        LiftPadShipList.Clear();
        foreach (GameObject sphere in DebugSpheres)
        {
            Destroy(sphere);
        }
        DebugSpheres.Clear();

        BoxCollider box = LiftPad.GetComponent<BoxCollider>();
        Vector3 size = box.size;

        if (TargetShips.Count == 0)
        {
            StartCoroutine(ShowResults("There are no TARGET ships defined", false));
            return;
        }

        bool allTargetShipsContainedLiftPad = true;
        foreach (ParkingShip ship in TargetShips)
        {
            List<Vector3> checkPoints = new List<Vector3>();
            // GameObject go;
            Vector3 moveVec;
            BoxCollider shipBox = ship.GetComponent<BoxCollider>();
            Bounds b = shipBox.bounds;
            Vector3 shipMax = b.max;
            Vector3 shipMin = b.min;

            // min check spot                
            ContainGO.transform.position = shipMin;
            moveVec = shipBox.bounds.center - shipMin;
            ContainGO.transform.Translate(moveVec * .1f);
            //CreateSphere(ContainGO.transform, "_rawMin", Color.red);
            ContainGO.transform.position = new Vector3(ContainGO.transform.position.x, RotateBox01.bounds.center.y, ContainGO.transform.position.z);
            //CreateSphere(ContainGO.transform, "_checkMin", Color.blue);
            checkPoints.Add(ContainGO.transform.position);

            // max check spot                
            ContainGO.transform.position = shipMax;
            moveVec = shipBox.bounds.center - shipMax;
            ContainGO.transform.Translate(moveVec * .1f);
            // CreateSphere(ContainGO.transform, "_rawMax", Color.red);
            ContainGO.transform.position = new Vector3(ContainGO.transform.position.x, RotateBox01.bounds.center.y, ContainGO.transform.position.z);
            //CreateSphere(ContainGO.transform, "_checkMax", Color.blue);
            checkPoints.Add(ContainGO.transform.position);

            bool inLiftPad = box.bounds.Contains(checkPoints[0]) && box.bounds.Contains(checkPoints[1]);
            if (inLiftPad)
            {
                Debug.Log("ship: " + ship.name + " is fully contained within the lift pad");
            }
            else
            {
                Debug.Log("ship: " + ship.name + " is NOT fully contained within the lift pad so can't lift no matter what");
                allTargetShipsContainedLiftPad = false;
                break;
            }
        }
        Debug.Log("are all TARGET ships contained in the lift?: " + allTargetShipsContainedLiftPad);
        string result;
        if (allTargetShipsContainedLiftPad == true)
        {
            result = "All TARGET ships are on the Lift Pad, so you win";
            StartCoroutine(ShowResults(result, allTargetShipsContainedLiftPad));
        }
        //else result = "Not all TARGET ships are on the Lift Pad, so keep trying.";        
        //StartCoroutine(ShowResults("FIX THIS IT'S AN ENDGAME HACK", true));
    }
    private void OnGUI()
    {        
        //Debug.Log("OnGUI(): " + this.name);
       /* if(GUI.Button(new Rect(0,0,100,100), "Rotate Platform"))
        {
            RotateGridPlatform();
        }
        if(GUI.Button(new Rect(0,100,100,100), "Check Finish"))
        {
            CheckGameFinish();
        }*/
        if (GUI.Button(new Rect(Screen.width - 100, 0, 100, 100), "Main Menu"))
        {
            SceneManager.LoadScene("ParkingDemo");
        }        
    }

    IEnumerator ShowResults(string result, bool success)
    {
        CurGameState = eGameState.OFF;
        ResultsText.gameObject.SetActive(true);
        ResultsText.text = result;
        yield return new WaitForSeconds(3f);
        ResultsText.gameObject.SetActive(false);
        if(success == true)
        {
            if (MCP != null) MCP.PuzzleFinished();
            else SceneManager.LoadScene("ParkingDemo");//Debug.Log("We're not part of an MCP so figure out what next to do");
        }      
        else
        {
            CurGameState = eGameState.NORMAL;
        }
    }

    
    
    

    GameObject CreateSphere(Transform t, string end, Color color, bool addToDebugSpheres = true)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = t.position;
        Destroy(sphere.GetComponent<SphereCollider>());
        sphere.transform.localScale = new Vector3(.25f, .25f, .25f);
        sphere.GetComponent<MeshRenderer>().material.color = color;
        sphere.name = t.name + end;
        if(addToDebugSpheres) DebugSpheres.Add(sphere);
        return sphere;
    }
}
