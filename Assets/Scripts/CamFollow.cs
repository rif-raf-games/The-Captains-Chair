using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamFollow : MonoBehaviour
{
    public CharacterEntity EntityToFollow;
    public Vector3 CamOffset;

    Vector3 LerpStart, LerpEnd;

    private void LateUpdate()
    {
        transform.position = EntityToFollow.transform.position + CamOffset;
    }

    public void SetupNewCamFollow(CharacterEntity newEntityToFollow, Vector3 newCamOffset)
    {
        Debug.Log("SetupNewCamFollow() newEntityToFollow: " + newEntityToFollow.name + ", newCamOffset: " + newCamOffset.ToString("F2"));
        Debug.Log("curOffset: " + CamOffset.ToString("F2"));
        Debug.Log("newOffset on new entity: " + (transform.position - newEntityToFollow.transform.position).ToString("F2"));
        Debug.Log("newCamOffset: " + newCamOffset.ToString("F2"));
       
        this.EntityToFollow = newEntityToFollow;

        StartCoroutine(LerpCamera(newCamOffset));
    }

    IEnumerator LerpCamera(Vector3 newCamOffset)
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
