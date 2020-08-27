using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class CamFollow : MonoBehaviour
{
    enum eCamState { FOLLOW, SHIP_VIEW, TRANSITION };
    eCamState CurCamState = eCamState.FOLLOW;

    public CharacterEntity EntityToFollow;
    public Vector3 CamOffset;    
    public CameraSetting[] CameraSettings;
    public int CameraSettingIndex = 0;

    private CCPlayer Player;    
    CharacterEntity EntityWasFollowing;
    Vector3 PrevCamPos;
    float PrevCamRot;    
    Vector3 ShipViewCamPos = new Vector3(-23.7f, 40f, 295f);    
    Vector3 LerpStart, LerpEnd;
    ShipAreasCollider ShipAreasCollider;
    
    private void Start()
    {
        Player = FindObjectOfType<CCPlayer>();
        this.ShipAreasCollider = FindObjectOfType<ShipAreasCollider>();
        FindObjectOfType<MCP>().AssignCameraToggleListeners(this);
    }
    private void LateUpdate()
    {
        //Debug.Log("LateUpdate");
        if(EntityToFollow != null)
        {
            transform.position = EntityToFollow.transform.position + CamOffset;
        }           
    }

    IEnumerator LerpCamFollow(Vector3 newCamOffset, Quaternion newCamRot)
    {
        Quaternion lerpRotStart = transform.rotation;
        Quaternion lerpRotEnd = newCamRot;
        LerpStart = transform.position - EntityToFollow.transform.position;
        LerpEnd = newCamOffset;
        float timer = 0f;
        while (timer < 1f)
        {
            CamOffset = Vector3.Lerp(LerpStart, LerpEnd, timer);
            transform.rotation = Quaternion.Lerp(lerpRotStart, lerpRotEnd, timer);
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        CamOffset = newCamOffset;
        transform.rotation = lerpRotEnd;
    }

    public bool ShouldShowCameraToggle()
    {
        return (!(CameraSettings == null || CameraSettings.Length < 1));        
    }
    public void OnClickCameraToggle()
    {
        if (CameraSettings == null || CameraSettings.Length < 1 || CurCamState != eCamState.FOLLOW) return;
        CameraSettingIndex = (CameraSettingIndex + 1) % CameraSettings.Length;
        Vector3 newPos = CameraSettings[CameraSettingIndex].Position;
        Quaternion newRot = Quaternion.Euler(CameraSettings[CameraSettingIndex].Rotation);
        StartCoroutine(LerpCamFollow(newPos, newRot));
    }  

    public void SetupNewCamFollow(CharacterEntity newEntityToFollow, Vector3 newCamOffset, Quaternion newCamRot)
    {        
        if(CurCamState != eCamState.FOLLOW ) Debug.LogError("Trying to set up a new cam follow: " + newEntityToFollow.name + " but the camera is not in FOLLOW mode");        

        this.EntityToFollow = newEntityToFollow;
        StartCoroutine(LerpCamFollow(newCamOffset, newCamRot));
    }

    Vector3 PrevMousePos;
    bool PinchActive = false;
    Vector3[] StartTouchPos = new Vector3[2];
    Vector2[] LastFingerPos = new Vector2[2];   // these values are used for determining control based on finger gestures
    Vector2[] CurFingerPos = new Vector2[2];
    Vector2[] CurFingerDelt = new Vector2[2];
    float[] FingerMag = new float[2];
    float[] FingerDir = new float[2];
    public Text DebugText;
    float TapTimer = 0f;
    int TapCount = 0;
    private void Update()
    {
        if(CurCamState == eCamState.FOLLOW)
        {
            if (Input.touchCount != 0)
            {
                for(int i=0; i<Input.touchCount; i++)
                {
                    if (i >= 2) break; // only deal with 2 fingers
                    if (Input.touches[i].phase == TouchPhase.Began) StartTouchPos[i] = Input.touches[i].position;
                }
            }
            if (Input.touchCount == 2)
            {
                for (int i = 0; i < 2; i++)
                {
                    LastFingerPos[i] = CurFingerPos[i];
                    CurFingerPos[i] = Input.touches[i].position;
                    CurFingerDelt[i] = Input.touches[i].deltaPosition;
                    FingerMag[i] = Vector2.SqrMagnitude(CurFingerDelt[i]);
                    FingerDir[i] = Vector2.Angle(CurFingerDelt[i], Vector3.right);
                }
                if (FingerDir[1] > 90f && Player.IsInFreeRoam() == true)
                {
                    BeginZoomOut();
                }
            }
        }
        else if(CurCamState == eCamState.SHIP_VIEW)
        {
            if(Input.GetMouseButtonDown(0))
            {
                PrevMousePos = Input.mousePosition;
                TapTimer = 0f;
            }
            else if(Input.GetMouseButton(0))
            {
                Vector3 diff = (Input.mousePosition - PrevMousePos) * ShipZoomedMoveSpeed;
                PrevMousePos = Input.mousePosition;
                Camera.main.transform.position += new Vector3(diff.x, -diff.y, 0f);

                TapTimer += Time.deltaTime;
            }
            else if(Input.GetMouseButtonUp(0))
            {
                if (TapTimer < .2f)
                {
                    TapCount++;
                    if(TapCount == 2 )
                    {
                        BeginZoomIn();
                        TapCount = 0;
                    }
                }
                else TapCount = 0;
                TapTimer = 0f;
            }
        }

        if(DebugText != null)
        {
            DebugText.text = "Cam state: " + CurCamState.ToString() + "\n";
            if (Input.touchCount == 0) DebugText.text += "no fingers";
            else if (Input.touchCount > 2) DebugText.text += "too many fingers";
            else
            {
                // DebugText.text += "finger 1: " + Input.touches[0].phase + ", delt: " + CurFingerDelt[0].ToString("F2") + ", Mag: " + FingerMag[0].ToString("F2") + ", Dir: " + FingerDir[0].ToString("F2") + "\n";
                DebugText.text += "finger 1 dir: " + FingerDir[0].ToString("F2") + "\n";
                if (Input.touchCount == 2)
                {
                    // DebugText.text += "finger 2: " + Input.touches[1].phase + ", delt: " + CurFingerDelt[1].ToString("F2") + ", Mag: " + FingerMag[1].ToString("F2") + ", Dir: " + FingerDir[1].ToString("F2") + "\n";
                    DebugText.text += "finger 2 dir: " + FingerDir[1].ToString("F2") + "\n";
                    
                }
            }

        }
    }

    /*private void OnGUI()
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
    }*/
    
    void BeginZoomIn()
    {
         CurCamState = eCamState.TRANSITION;Camera.main.orthographic = false;
        StartCoroutine(LerpCamZoom(Camera.main.transform.position, PrevCamPos, Camera.main.transform.eulerAngles.x, PrevCamRot, eCamState.FOLLOW));
    }
    void BeginZoomOut()
    {
        CurCamState = eCamState.TRANSITION;
        Player.SetNavMeshDest(Player.transform.position);
        Player.ToggleMovementBlocked(true);

        this.ShipAreasCollider.ToggleShipFloors(true);

        EntityWasFollowing = EntityToFollow;
        EntityToFollow = null;
        PrevCamPos = Camera.main.transform.position;
        PrevCamRot = Camera.main.transform.eulerAngles.x;
        StartCoroutine(LerpCamZoom(PrevCamPos, ShipViewCamPos, PrevCamRot, 0f, eCamState.SHIP_VIEW));
    }

    public float CamTransitionTime = 3f;
    public float ShipZoomedMoveSpeed = .1f;
   // public Text DebugText;
    IEnumerator LerpCamZoom(Vector3 lerpPosStart, Vector3 lerpPosEnd, float lerpRotStart, float lerpRotEnd, eCamState lerpEndState)
    {
       // Debug.Log("LerpCamZoom() lerpPosStart: " + lerpPosStart.ToString("F2") + ", lerpPosEnd: " + lerpPosEnd.ToString("F2"));
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
            this.ShipAreasCollider.ToggleShipFloors(false);            
        }        
    }

    [System.Serializable]
    public class CameraSetting
    {
        public Vector3 Position;
        public Vector3 Rotation;
    }
}
