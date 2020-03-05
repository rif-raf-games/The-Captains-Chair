using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class CamFollow : MonoBehaviour
{
    enum eCamState { FOLLOW, SHIP_VIEW, TRANSITION };
    eCamState CurCamState = eCamState.FOLLOW;

    private CCPlayer Player;
    public CharacterEntity EntityToFollow;
    CharacterEntity EntityWasFollowing;
    Vector3 PrevCamPos;
    float PrevCamRot;
    public Vector3 CamOffset;

    Vector3 ShipViewCamPos = new Vector3(-28.7f, 40f, 263f);
    
    Vector3 LerpStart, LerpEnd;

    private void Start()
    {
        Player = FindObjectOfType<CCPlayer>();
    }
    private void LateUpdate()
    {
        //Debug.Log("LateUpdate");
        if(EntityToFollow != null)
        {
            transform.position = EntityToFollow.transform.position + CamOffset;
        }   
        if(DebugText != null)
        {
            DebugText.text = CurCamState.ToString();
        }
    }

    public void SetupNewCamFollow(CharacterEntity newEntityToFollow, Vector3 newCamOffset)
    {        
        if(CurCamState != eCamState.FOLLOW ) Debug.LogError("Trying to set up a new cam follow: " + newEntityToFollow.name + " but the camera is not in FOLLOW mode");        

        this.EntityToFollow = newEntityToFollow;
        StartCoroutine(LerpCamFollow(newCamOffset));
    }

    Vector3 PrevMousePos;
    private void Update()
    {
        if(CurCamState == eCamState.SHIP_VIEW)
        {
            if(Input.GetMouseButtonDown(0))
            {
                PrevMousePos = Input.mousePosition;
            }
            else if(Input.GetMouseButton(0))
            {
                Vector3 diff = (Input.mousePosition - PrevMousePos) * ShipZoomedMoveSpeed;
                PrevMousePos = Input.mousePosition;

                Camera.main.transform.position += new Vector3(diff.x, -diff.y, 0f);
            }
        }
    }

    private void OnGUI()
    {
        switch(CurCamState)
        {
            case eCamState.FOLLOW:
                if (GUI.Button(new Rect(Screen.width - 100, 0, 100, 50), "zoom out"))
                {
                    if (Player.IsInFreeRoam() == false) { Debug.LogWarning("Trying to start a zoom but we're not in free roam"); return; }
                    BeginZoomOut();
                }
                break;
            case eCamState.TRANSITION:
                break;
            case eCamState.SHIP_VIEW:
                if (GUI.Button(new Rect(Screen.width - 100, 0, 100, 50), "zoom in"))
                {
                    BeginZoomIn();
                }
                break;
        }        
    }
    
    void BeginZoomIn()
    {
        CurCamState = eCamState.TRANSITION;
        StartCoroutine(LerpCamZoom(Camera.main.transform.position, PrevCamPos, Camera.main.transform.eulerAngles.x, PrevCamRot, eCamState.FOLLOW));
    }
    void BeginZoomOut()
    {
        CurCamState = eCamState.TRANSITION;
        Player.SetNavMeshDest(Player.transform.position);
        Player.ToggleMovementBlocked(true);        

        EntityWasFollowing = EntityToFollow;
        EntityToFollow = null;
        PrevCamPos = Camera.main.transform.position;
        PrevCamRot = Camera.main.transform.eulerAngles.x;
        StartCoroutine(LerpCamZoom(PrevCamPos, ShipViewCamPos, PrevCamRot, 0f, eCamState.SHIP_VIEW));
    }

    public float CamTransitionTime = 3f;
    public float ShipZoomedMoveSpeed = .1f;
    public Text DebugText;
    IEnumerator LerpCamZoom(Vector3 lerpPosStart, Vector3 lerpPosEnd, float lerpRotStart, float lerpRotEnd, eCamState lerpEndState)
    {
        Debug.Log("LerpCamZoom() lerpPosStart: " + lerpPosStart.ToString("F2") + ", lerpPosEnd: " + lerpPosEnd.ToString("F2"));
        float lerpStartTime = Time.time;
        float lerpPercentage = 0f;
        while (lerpPercentage < 1f)
        {
            float lerpTime = Time.time - lerpStartTime;
            lerpPercentage = lerpTime / CamTransitionTime;
            Vector3 curPos = Vector3.Lerp(lerpPosStart, lerpPosEnd, lerpPercentage);
            float rot = Mathf.Lerp(lerpRotStart, lerpRotEnd, lerpPercentage);
            if(lerpPercentage >= 1f)
            {
                curPos = lerpPosEnd;
                rot = lerpRotEnd;
            }
            Camera.main.transform.position = curPos;
            Camera.main.transform.eulerAngles = new Vector3(rot, Camera.main.transform.eulerAngles.y, Camera.main.transform.eulerAngles.z);
            yield return new WaitForEndOfFrame();
        }
        CurCamState = lerpEndState;
        if(CurCamState == eCamState.FOLLOW)
        {
            EntityToFollow = EntityWasFollowing;
            Player.ToggleMovementBlocked(false);
        }
    }

    IEnumerator LerpCamFollow(Vector3 newCamOffset)
    {
        LerpStart = transform.position - EntityToFollow.transform.position;
        LerpEnd = newCamOffset;                
        float timer = 0f;
        while (timer < 1f)
        {
            CamOffset = Vector3.Lerp(LerpStart, LerpEnd, timer);
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        CamOffset = newCamOffset;
    }         
}
