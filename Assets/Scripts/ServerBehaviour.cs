using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ServerBehaviour : MonoBehaviour
{
    public GameObject loading_circle_prefab;
    public GameObject loading_circle;

    private GameObject sceneManager;

    public GameObject antivirus;
    public GameObject inside_loading_bar;
    public GameObject inside_loading_bar_empty;
    public GameObject outside_loading_bar;
    private Vector3 loading_bar_starting_pos;
    public GameObject server_sprite;
    public int current_work=-1;
    public bool disconnected;
    private bool change_connection_state=false;
    public int packages_received;

    private bool pause;
    private float start_pause_time;
    private float end_pause_time;

    public float antivirus_animation_speed;
    public float server_animation_speed;

    public Animator animator;

    public int server_id;
    public int os_type;

    private float last_spawn_time;
    public float average_delay_between_spawns;
    private float next_delay;
    public List<GameObject> attached_packages;

    public bool emergency=false;
    public bool overload=false;
    public float overload_time;
    public float emergency_reaction_delay;
    public float emergency_time;
    public float emergency_duration;

    public void connection(bool connected){
        if (disconnected == connected){
            disconnected = !connected;
            change_connection_state = true;
            if(disconnected){
                inside_loading_bar_empty.GetComponent<Renderer>().material.color = Color.blue;
            }else{
                inside_loading_bar_empty.GetComponent<Renderer>().material.color = Color.grey;
            }
        } 
    }

    public void setSceneManager(GameObject gameObject){
        sceneManager = gameObject;
        float equalizer = 0;
        List<int> os_types_by_rarity = sceneManager.GetComponent<Spawner>().os_types_by_rarity;
        for (int i=0; i<os_types_by_rarity.Count; i++){
            if (os_types_by_rarity[i] == os_type){
                equalizer += 1;
            }
        }
        equalizer /= os_types_by_rarity.Count;
        equalizer = 1-equalizer;
        average_delay_between_spawns *= equalizer;
        next_delay = average_delay_between_spawns*Random.Range(0.8f, 1.2f);
    }

    public void receive_package(){
        packages_received++;
    }

    void randomPackageSpawn(int package_type){
        sceneManager.GetComponent<Spawner>().spawnPackage(server_id,package_type);
    }

    void OnMouseDown(){
        sceneManager.GetComponent<VariablesManager>().clickServer(server_id);
    }

    void reset_emergency_timer(bool overload){
        emergency_time = Time.time;
        if (loading_circle){
            Destroy(loading_circle);
        }
        loading_circle = Instantiate(loading_circle_prefab, new Vector3(0,0,0), Quaternion.identity);
        Color color = overload ? Color.red : Color.green;
        loading_circle.transform.Find("loadingCircleContent").gameObject.GetComponent<RadialTimer>().startTimer(sceneManager.GetComponent<VariablesManager>().getCanvas(),Camera.main.WorldToViewportPoint(antivirus.transform.position + new Vector3(antivirus.GetComponent<Renderer>().bounds.size.x/2,antivirus.GetComponent<Renderer>().bounds.size.y/2,0)), color);
        if (overload){
            sceneManager.GetComponent<VariablesManager>().playErrorSound();
        }
    }

    void Awake()
    {
        pause = false;
        last_spawn_time = Time.time;
        packages_received = 0;
        attached_packages = new List<GameObject>(){};
        overload_time = -emergency_reaction_delay;
        inside_loading_bar_empty.transform.localScale = new Vector3(30,inside_loading_bar.transform.localScale.y,0);
        outside_loading_bar.transform.position = new Vector3(transform.position.x, transform.position.y - (19*server_sprite.GetComponent<Renderer>().bounds.size.y/32), 0);
        inside_loading_bar_empty.transform.position = new Vector3(transform.position.x, transform.position.y - (19*server_sprite.GetComponent<Renderer>().bounds.size.y/32), 0);
        inside_loading_bar.transform.position = new Vector3(transform.position.x, transform.position.y - (19*server_sprite.GetComponent<Renderer>().bounds.size.y/32), 0);
        loading_bar_starting_pos = inside_loading_bar.transform.position - new Vector3(inside_loading_bar_empty.GetComponent<Renderer>().bounds.size.x/2, 0, 0);
        inside_loading_bar.transform.localScale = new Vector3(0,1,0);
        loading_circle = null;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (pause!=sceneManager.GetComponent<VariablesManager>().pause){
            if (pause){
                end_pause_time = Time.time;
                last_spawn_time += end_pause_time - start_pause_time;
                overload_time += end_pause_time - start_pause_time;
                emergency_time += end_pause_time - start_pause_time;
                antivirus.GetComponent<Animator>().speed = antivirus_animation_speed;
                transform.Find("server_sprite").GetComponent<Animator>().speed = server_animation_speed;
            }else{
                start_pause_time = Time.time;
                antivirus_animation_speed = antivirus.GetComponent<Animator>().speed;
                server_animation_speed = transform.Find("server_sprite").GetComponent<Animator>().speed;
                antivirus.GetComponent<Animator>().speed = 0;
                transform.Find("server_sprite").GetComponent<Animator>().speed = 0;
            }
            pause=sceneManager.GetComponent<VariablesManager>().pause;
        }
        if(sceneManager.GetComponent<VariablesManager>().work[server_id]!=current_work){
            current_work = sceneManager.GetComponent<VariablesManager>().work[server_id];
            int delta = sceneManager.GetComponent<VariablesManager>().max_work - current_work;
            if (delta<0){
                inside_loading_bar.transform.localScale = new Vector3(30, inside_loading_bar.transform.localScale.y, 0);
                inside_loading_bar.GetComponent<Renderer>().material.color = Color.red;
            }else{
                if (!pause){
                    animator.speed = 1.5f*(float)sceneManager.GetComponent<VariablesManager>().max_work/(float)(current_work+sceneManager.GetComponent<VariablesManager>().max_work);
                }
                inside_loading_bar.transform.localScale = new Vector3(30*current_work/sceneManager.GetComponent<VariablesManager>().max_work, inside_loading_bar.transform.localScale.y, 0);
                if (delta < 2){
                    inside_loading_bar.GetComponent<Renderer>().material.color = new Color(1.0f, 0.64f, 0.0f);//orange
                }else if(current_work>3){
                    inside_loading_bar.GetComponent<Renderer>().material.color = Color.yellow;
                }else if (current_work>0){
                    inside_loading_bar.GetComponent<Renderer>().material.color = Color.green;
                }
            }
            inside_loading_bar.transform.position = loading_bar_starting_pos + new Vector3(inside_loading_bar.GetComponent<Renderer>().bounds.size.x/2, 0, 0);       
        }
        if (pause){

        }else{
            if (change_connection_state){
                server_sprite.GetComponent<Animator>().SetBool("disconnected",disconnected);
                change_connection_state = false;
            }
            if (Time.time > last_spawn_time + next_delay){
                int package_type = sceneManager.GetComponent<Spawner>().getRandomType();
                if (package_type != os_type){
                    randomPackageSpawn(package_type);
                }
                last_spawn_time = Time.time;
                int ref_value = sceneManager.GetComponent<VariablesManager>().level_values[sceneManager.GetComponent<VariablesManager>().level_values.Count-1];
                next_delay = average_delay_between_spawns*Random.Range(0.8f, 1.2f)*ref_value/(ref_value+packages_received);
            }
            if (overload && !(current_work>sceneManager.GetComponent<VariablesManager>().max_work)){
                overload=false;
                animator.SetBool("overload", overload);
                if (emergency){
                    reset_emergency_timer(overload);
                }
            }
            if (!overload && current_work>sceneManager.GetComponent<VariablesManager>().max_work){
                overload=true;
                animator.SetBool("overload", overload);
                /*if (overload_time+emergency_reaction_delay<Time.time){
                    overload_time = Time.time;
                }*/
                overload_time = Time.time;
                if (emergency){
                    reset_emergency_timer(overload);
                }
                animator.speed = 1;
            }
            if(overload && !emergency && Time.time>overload_time+emergency_reaction_delay){
                antivirus.gameObject.SetActive(true);
                reset_emergency_timer(overload);
                emergency = true;
            }
            if (emergency){
                loading_circle.transform.Find("loadingCircleContent").gameObject.GetComponent<RadialTimer>().updateProgress(Time.deltaTime/emergency_duration);
            }
            if (emergency && Time.time > emergency_time + emergency_duration){
                if(overload){
                    transform.Find("red_circle").gameObject.SetActive(true);
                    sceneManager.GetComponent<VariablesManager>().finishGame();
                }else{
                    emergency = false;
                    antivirus.gameObject.SetActive(false);
                    Destroy(loading_circle);
                }
            }
        }
    }
}
