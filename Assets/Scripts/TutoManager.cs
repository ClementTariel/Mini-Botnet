using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TutoManager : MonoBehaviour
{
    public GameObject canvas;

    public List<int> work;
    public int max_work;

    public Sprite[] logoSprites;

    public GameObject server_0;
    public GameObject server_1;

    public List<Text> texts;
    public int text_level;

    public bool task_done = true;

    public float unit;

    public int server_selected;
    public Vector3 serverTransformPosition;

    private List<Vector3> server_positions;

    public bool connection_exists;

    public GameObject line;

    public List<GameObject> packages;

    public AudioSource audioSource;

    public AudioClip pling_deco;
    public AudioClip pling_co;
    public AudioClip plop;
    public AudioClip complex_beep;
    public AudioClip beep21;
    public AudioClip beep22;
    public AudioClip beep23;
    public AudioClip error_sound;

    private bool package_delivered=false;

    public Vector3 mousePos;

    public float basic_speed;
    private float start_travel_time=float.PositiveInfinity;
    private float travel_duration;

    public float loading_time;

    private int attached_packages = 0;

    public void spawnLine(){
        line.GetComponent<DrawLine>().RenderLine(server_positions[0], server_positions[1]);
    }

    public void clickServer(int server_id){
        if (server_selected < 0){
            server_selected = server_id;
            serverTransformPosition = server_positions[server_id];
            return;
        }
        if (server_selected == server_id){
            server_selected = -1;
            return;
        }
        if (!connection_exists ){
            if((text_level==3 || text_level==9)){
                connection_exists = true;
                server_0.GetComponent<ServerTutoBehaviour>().connection(connection_exists);
                server_1.GetComponent<ServerTutoBehaviour>().connection(connection_exists);
                line.SetActive(connection_exists);
                line.GetComponent<DrawLine>().setColor(Color.green);
                audioSource.PlayOneShot(pling_co,0.8f);
            }else{
                audioSource.PlayOneShot(pling_deco,0.8f);
            }   
        }else{
            if(text_level==5 || (text_level>=9 && text_level<=15)){
                audioSource.PlayOneShot(pling_deco,0.8f);
            }else{
                connection_exists = false;
                server_0.GetComponent<ServerTutoBehaviour>().connection(connection_exists);
                server_1.GetComponent<ServerTutoBehaviour>().connection(connection_exists);
                line.SetActive(connection_exists);
                audioSource.PlayOneShot(pling_deco,0.8f);
            }
            
        }
        server_selected = -1;
    }

    public void playErrorSound(){
        audioSource.PlayOneShot(error_sound,1f);
    }

    public void increaseLevel(int i){
        texts[text_level].gameObject.SetActive(false);
        text_level+=i;
        if (text_level == 1){
            task_done=true;
        }
        if (text_level == 2){
            task_done=true;
            server_0.SetActive(true);
            audioSource.PlayOneShot(complex_beep,0.3f);
            server_1.SetActive(true);
            audioSource.PlayOneShot(complex_beep,0.3f);
        }
        if (text_level == 3){
            task_done=false;
        }
        if (text_level == 4){
            task_done=true;
        }
        if (text_level == 5){
            task_done=true;
        }
        if (text_level == 6){
            task_done=false;
        }
        if (text_level == 7){
            task_done=true;
        }
        if (text_level == 8){
            task_done=true;
        }
        if (text_level == 9){
            task_done=false;
            packages[0].SetActive(true);
            audioSource.PlayOneShot(plop,0.3f);
            travel_duration = (server_positions[0]-server_positions[1]).magnitude*5/(basic_speed*3);
        }
        if (text_level == 10){
            start_travel_time = Time.time;
            task_done=true;
        }
        if (text_level == 11){
            if(package_delivered){
                text_level++;
            }else{
                task_done=false;
            }
        }
        if (text_level == 12){
            task_done=true;
        }
        if (text_level == 13){
            task_done=true;
        }
        if (text_level == 14){
            task_done=false;
            packages[1].SetActive(true);
            audioSource.PlayOneShot(plop,0.3f);
            packages[2].SetActive(true);
            audioSource.PlayOneShot(plop,0.3f);
            packages[3].SetActive(true);
            audioSource.PlayOneShot(plop,0.3f);
            server_0.GetComponent<ServerTutoBehaviour>().reset_emergency_timer(true);
            travel_duration = (server_positions[0]-server_positions[1]).magnitude*14/(basic_speed*3);
            start_travel_time = Time.time + loading_time;
            attached_packages = 3;
            server_0.GetComponent<ServerTutoBehaviour>().antivirus.gameObject.SetActive(true);
        }
        if (text_level == 15){
            task_done=true;
        }
        if (text_level == 16){
            task_done=false;
        }
        texts[text_level].gameObject.SetActive(true);
    }

    void Awake(){
        text_level=0;
        for (int i=0; i<texts.Count; i++){
            texts[i].gameObject.SetActive(false);
        }
        work = new List<int>(){0,0};
        server_selected = -1;
        connection_exists = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        server_0.transform.Find("logo").gameObject.GetComponent<SpriteRenderer>().sprite = logoSprites[0];
        server_1.transform.Find("logo").gameObject.GetComponent<SpriteRenderer>().sprite = logoSprites[1];
        
        unit = server_0.transform.Find("server_sprite").gameObject.GetComponent<Renderer>().bounds.size.x;

        server_positions = new List<Vector3>(){new Vector3(-2.0f*unit, 0, 0), new Vector3(2.0f*unit, 0, 0)};

        server_0.transform.position = server_positions[0];
        server_1.transform.position = server_positions[1];

        spawnLine();
        
        float offset = server_1.transform.Find("server_sprite").gameObject.GetComponent<Renderer>().bounds.size.x/2 + packages[0].GetComponent<Renderer>().bounds.size.x/2;
        packages[0].transform.position = server_1.transform.position + new Vector3(offset, offset, 0);
        packages[1].transform.position = server_0.transform.position + new Vector3(offset, offset, 0);
        packages[2].transform.position = server_0.transform.position + new Vector3(offset + packages[1].GetComponent<Renderer>().bounds.size.x, offset, 0);
        packages[3].transform.position = server_0.transform.position + new Vector3(offset + packages[1].GetComponent<Renderer>().bounds.size.x * 2, offset, 0);
        
        increaseLevel(1);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp("q")){
            SceneManager.LoadScene("menu");
        }
        if (Input.GetKeyUp("return") && text_level==16){
            SceneManager.LoadScene("main_scene");
        }
        if (Input.GetKeyUp("return") && task_done){
            increaseLevel(1);
        }

        if (server_selected<0){
            GetComponent<DrawLine>().lineRenderer.enabled = false;
        }else{
            GetComponent<DrawLine>().lineRenderer.enabled = true;
            mousePos = Input.mousePosition;
            mousePos = Camera.main.ScreenToWorldPoint(mousePos);
            GetComponent<DrawLine>().RenderLine(serverTransformPosition,mousePos);
        }

        if((text_level==3 || text_level==9) && connection_exists){
            task_done=true;
            increaseLevel(1);
        }
        if(text_level==6 && !connection_exists){
            task_done=true;
            increaseLevel(1);
        }

        work[0]=0;
        work[1]=0;
        if(connection_exists){
            work[0]++;
            work[1]++;
        }
        if(text_level==9){
            work[1]+=2;
        }
        if(text_level==14){
            work[0]+=2*attached_packages;
        }

        if (start_travel_time<Time.time){
            if (text_level>=14 && attached_packages>0){
                attached_packages=0;
                server_0.GetComponent<ServerTutoBehaviour>().reset_emergency_timer(false);
            }
            if (text_level<14){
                if(start_travel_time+travel_duration<Time.time){
                    start_travel_time = float.PositiveInfinity;
                    packages[0].SetActive(false);
                    audioSource.PlayOneShot(beep22,0.6f);
                    task_done = true;
                    package_delivered=true;
                    if (text_level==11){
                        increaseLevel(1);
                    }
                }else{
                    work[0]+=1;
                    work[1]+=1;
                    packages[0].transform.position = (1/travel_duration)*(server_0.transform.position*(Time.time-start_travel_time) + server_1.transform.position*(start_travel_time+travel_duration-Time.time));
                }
            }else{
                if(start_travel_time+travel_duration<Time.time){
                    start_travel_time = float.PositiveInfinity;
                    packages[1].SetActive(false);
                    audioSource.PlayOneShot(beep21,0.6f);
                    packages[2].SetActive(false);
                    audioSource.PlayOneShot(beep22,0.6f);
                    packages[3].SetActive(false);
                    audioSource.PlayOneShot(beep23,0.6f);
                    package_delivered=true;
                }else if(start_travel_time<Time.time){
                    work[0]+=3;
                    work[1]+=3;
                    packages[1].transform.position = (1/travel_duration)*(server_1.transform.position*(Time.time-start_travel_time) + server_0.transform.position*(start_travel_time+travel_duration-Time.time)) + new Vector3(-packages[1].GetComponent<Renderer>().bounds.size.x/4,0,0);
                    packages[2].transform.position = (1/travel_duration)*(server_1.transform.position*(Time.time-start_travel_time) + server_0.transform.position*(start_travel_time+travel_duration-Time.time));
                    packages[3].transform.position = (1/travel_duration)*(server_1.transform.position*(Time.time-start_travel_time) + server_0.transform.position*(start_travel_time+travel_duration-Time.time)) + new Vector3(packages[1].GetComponent<Renderer>().bounds.size.x/4,0,0);
                }
            }
            
        }
    }
}
