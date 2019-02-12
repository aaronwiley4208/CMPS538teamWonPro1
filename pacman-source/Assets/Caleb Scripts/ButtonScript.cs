using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void StartButton()
    {
        SceneManager.LoadScene("Game Scene");
    }

    public void HelpButton()
    {
        SceneManager.LoadScene("Help Scene");
    }

    public void CreditsButton()
    {
        SceneManager.LoadScene("Credits Scene");
    }

    public void MainMenuButton()
    {
        SceneManager.LoadScene("Start Menu");
    }

    public void ResultsButton()
    {
        SceneManager.LoadScene("Results Scene");
    }
}
