using Articy.The_Captain_s_Chair;
using Articy.The_Captain_s_Chair.GlobalVariables;
using Articy.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LockPicking : MonoBehaviour
{
    public Diode Diode;
    public GameObject CenterBlock;
    public List<Ring> Rings;
    public List<Gate> Gates;
    public List<PathNode> StartNodes;
    public List<PathNode> DeathNodes;
    public Ring CurTouchedRing = null;
    float LargestRingDiameter;
    Vector3 LastWorldTouchPos = Vector3.zero;
    float LastCenterToWorldAngle;
    public Text GameResultText;

    public void InitFromProcessing( Diode diode, GameObject centerBlock, List<Ring> rings, List<Gate> gates, List<PathNode> startNodes, List<PathNode> deathNodes )    
    {
        this.Diode = diode;
        CenterBlock = centerBlock;
        Rings = rings;
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
    public PathNode DebugStartNode;
    // Start is called before the first frame update
    void Start()
    {
        CenterBlock.transform.position = new Vector3(CenterBlock.transform.position.x, 0f, CenterBlock.transform.position.z);              
        LargestRingDiameter = Rings[Rings.Count-1].GetComponent<MeshCollider>().bounds.extents.x;

        StartGame();
    }
    
    void StartGame()
    {
        Diode.Moving = true;
        GameResultText.gameObject.SetActive(false);
        if (DebugStartNode != null)
        {
            Diode.SetStartNode(DebugStartNode);
        }
        else
        {
            int startIndex = Random.Range(0, StartNodes.Count);
            Diode.SetStartNode(StartNodes[startIndex]);
        }

        foreach(Gate g in Gates)
        {
            g.gameObject.SetActive(true);
        }
    }

    public void CollectGate(Gate gate)
    {
       // Debug.Log("found gate: " + gate.name);
        gate.gameObject.SetActive(false);
        bool allGatesFound = true;
        foreach(Gate g in Gates)
        {
            if(g.gameObject.activeSelf == true)
            {
                allGatesFound = false;
                break;
            }
        }
        if(allGatesFound == true)
        {
            Debug.Log("Found All gates.  start over");
            StartCoroutine(EndGame("You Won.", true));
        }
    }

    public void CheckDeathNode(PathNode pathNode)
    {
       // Debug.Log("CheckDeathNode: " + pathNode.name);
        if (DeathNodes.Contains(pathNode))
        {
            Debug.Log("it's a death node!");
            StartCoroutine(EndGame("You Lost.", false));
        }
        else Debug.Log("not a death node");
    }

    IEnumerator EndGame(string endGameString, bool success)
    {
        GameResultText.gameObject.SetActive(true);
        GameResultText.text = endGameString;
        Diode.Moving = false;
        yield return new WaitForSeconds(3);
        if(success == true)
        {
            ArticyGlobalVariables.Default.Mini_Games.Returning_From_Mini_Game = true;
            ArticyGlobalVariables.Default.Mini_Games.Mini_Game_Success = true;
            Mini_Game_Jump jumpSave = ArticyDatabase.GetObject<Mini_Game_Jump>("Mini_Game_Data_Container");
            SceneManager.LoadScene(jumpSave.Template.Next_Game_Scene.Scene_Name);
        }
        else
        {
            StartGame();
        }
        //StartCoroutine(HandleEndGame());
    }

    /*IEnumerator HandleEndGame()
    {
        
        StartGame();        
    }*/

    
                    
    // Update is called once per frame
    void Update()
    {
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
    }
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

    private void Awake()
    {
        Physics.autoSimulation = true;
    }
}
