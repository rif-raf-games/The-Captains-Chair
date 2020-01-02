using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class RepairDebug : MonoBehaviour
{
    float ModelHeight = 0.5542563f;
    Color[] Colors = { Color.red, Color.green, Color.blue, Color.yellow, Color.cyan, Color.magenta };
    public void OnDrawGizmos()
    {
        int angle;
        Quaternion q;
        Vector3 rayDir;
        Vector3 pos = transform.position + new Vector3(0f, ModelHeight / 2f, 0f);
        /*for (dir = 30; dir < 360; dir += 60)
        { // 30, 90, 150, 210, 270, 330
            Color color = Colors[(dir - 30) / 60];
             
            q = Quaternion.AngleAxis(dir, Vector3.up);
            rayDir = q * Vector3.right;
            Debug.DrawRay(pos, rayDir * 4, color);
        }*/

        angle = 0;
        q = Quaternion.AngleAxis(angle, Vector3.up);
        rayDir = q * Vector3.right;
        Debug.DrawRay(pos, rayDir * 4, Color.white);

        angle = 180;
        q = Quaternion.AngleAxis(angle, Vector3.up);
        rayDir = q * Vector3.right;
        Debug.DrawRay(pos, rayDir * 4, Color.red);        
    }
    
    private void Update()
    {
        Vector3 dir;
        
        dir = pt2.transform.position - pt1.transform.position;               
        float rotTest = Quaternion.FromToRotation(Vector3.right, dir).eulerAngles.y;
        debugText.text = "from green to blue (blue - green): " + rotTest.ToString("F2") + "\n\n";
        
        plane.transform.eulerAngles = new Vector3(0f, rotDir, 0f);
        debugText.text += rotDir + "\n";
        debugText.text += Mathf.RoundToInt(plane.transform.eulerAngles.y) + "\n";

    }
    public Text debugText;
    public GameObject pt1, pt2, plane;

    public float rotDir = 0f;
    private void OnGUI()
    {
        if(GUI.Button(new Rect(0,0,100,100), "left"))
        {
            rotDir -= 30;
            //cube.transform.eulerAngles = new Vector3(0f, rotDir, 0f);
        }
        if (GUI.Button(new Rect(100, 0, 100, 100), "right"))
        {
            rotDir += 30;
            //cube.transform.eulerAngles = new Vector3(0f, rotDir, 0f);
        }
    }


    float GetAngle(Vector3 dir, bool adjust)
    {
        float rot = Vector3.Angle(dir, Vector3.right);
        if (adjust) rot = 360f - rot;
        if (rot >= 360f) rot = rot - 360f;
        return rot;
    }
    
}
