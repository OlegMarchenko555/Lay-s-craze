using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
        StartCoroutine("StartMenu");
	}
	
    IEnumerator StartMenu()
    {
        yield return new WaitForSeconds(2.0f);
        SceneManager.LoadScene("02-Menu");
    }

	// Update is called once per frame
	void Update () {
		
	}
}
