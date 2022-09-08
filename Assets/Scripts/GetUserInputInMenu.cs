using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GetUserInputInMenu : MonoBehaviour
{

    public GameObject readyText;
    public GameObject helpText;
    public GameObject creditsText;
    public Text introText;
    private bool ready_available = false;

    // Start is called before the first frame update
    void Start()
    {
        ready_available = true;
        float high_score = PlayerPrefs.GetFloat("high_score", 0);
        int largest_network = PlayerPrefs.GetInt("largest_network", 3);
        int best_game_breaking = PlayerPrefs.GetInt("best_game_breaking", 0);
        introText.text = introText.text + "\n [highscore : "+high_score.ToString("000 000.000")+" BTC]";
        introText.text = introText.text + "\n [largest network built : "+largest_network.ToString()+(best_game_breaking>0?"(+"+best_game_breaking.ToString()+")":"")+" servers]";
    }

    // Update is called once per frame
    void Update()
    {
        if (ready_available && Input.GetKeyDown("t"))
        {
            ready_available = false;
            SceneManager.LoadScene("tuto");
        }
        if (ready_available && Input.GetKeyDown("h"))
        {
            //Debug.Log("a key was pressed");
            ready_available = false;
            readyText.SetActive(false);
            helpText.SetActive(true);
        }
        if (ready_available && Input.GetKeyDown("c"))
        {
            ready_available = false;
            readyText.SetActive(false);
            creditsText.SetActive(true);
        }
        if (Input.GetKeyDown("q"))
        {
            helpText.SetActive(false);
            creditsText.SetActive(false);
            readyText.SetActive(true);
            ready_available = true;
        }

        if (ready_available && Input.GetKeyDown("y"))
        {
            SceneManager.LoadScene("main_scene");
        }
        
        if (Input.GetKey("escape"))
        {
            Application.Quit();
        }

    }
}
