using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IAPTest : MonoBehaviour
{
    public GameObject Congrats;
    
    // Start is called before the first frame update
    void Start()
    {
        if(StaticStuff.HasUnlockedFullGame == true)
        {
            Congrats.SetActive(true);
        }        
        else
        {
            Congrats.SetActive(false);
            StartCoroutine(IAPTestDelay());
        }        
    }

    public void IAPPurchaseSuccessful()
    {
       // Debug.Log("IAPTest.IAPPurchaseSuccessful()");
        Congrats.SetActive(true);
    }

    public void IAPTransactionsRestored()
    {

    }

    IEnumerator IAPTestDelay()
    {
        yield return new WaitForSeconds(1);
        FindObjectOfType<MCP>().StartIAPPanel();
    }

    public void OnClickMM()
    {
        FindObjectOfType<MCP>().LoadNextScene("Front End Launcher");
    }       
}
