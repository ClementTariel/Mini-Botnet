using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GetUserInputEndScene : MonoBehaviour
{
    bool show_credit;
    public GameObject credits;

    // Start is called before the first frame update
    void Start()
    {
        show_credit = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("c")){
            show_credit = true;
            credits.SetActive(show_credit);
        }
        if (Input.GetKeyDown("q")){
            if (show_credit){
                show_credit = false;
                credits.SetActive(show_credit);
            }else{
                Destroy(GameObject.Find("scene_manager"));
                SceneManager.LoadScene("menu");
            }
        }

    }
}
