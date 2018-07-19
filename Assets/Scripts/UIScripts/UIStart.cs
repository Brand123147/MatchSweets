using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIStart : MonoBehaviour {

	public void OnClickExit()
    {
        Application.Quit();
    }
	public void OnClickPlay()
    {
        SceneManager.LoadScene(1);
    }


}
