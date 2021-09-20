using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuOverall : MonoBehaviour
{

    public void StartHost()
    {
        SceneManager.LoadScene("MenuOnline_Host");
    }

    public void StartClient()
    {
        SceneManager.LoadScene("MenuOnline_Client");
    }
}
