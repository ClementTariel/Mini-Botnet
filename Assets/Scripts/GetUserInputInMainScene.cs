using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GetUserInputInMainScene : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey("escape"))
        {
            Application.Quit();
        }
        if (Input.GetKeyUp("space") && !GetComponent<VariablesManager>().game_finished)
        {
            GetComponent<VariablesManager>().setPauseValue(!gameObject.GetComponent<VariablesManager>().pause);
        }
        if (Input.GetKeyDown("q") && GetComponent<VariablesManager>().game_finished){
            SceneManager.LoadScene("menu");
        }

    }
}
