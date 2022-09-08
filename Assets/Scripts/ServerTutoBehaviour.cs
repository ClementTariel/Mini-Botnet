using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ServerTutoBehaviour : MonoBehaviour
{
    public int server_id;
    public int os_type;

    public GameObject loading_circle_prefab;
    public GameObject loading_circle;

    public GameObject sceneManager;

    public GameObject antivirus;
    public GameObject inside_loading_bar;
    public GameObject inside_loading_bar_empty;
    public GameObject outside_loading_bar;
    private Vector3 loading_bar_starting_pos;
    public GameObject server_sprite;

    public int current_work=-1;
    public bool disconnected;
    public int packages_received;

    public float antivirus_animation_speed;
    public float server_animation_speed;

    public Animator animator;

    public List<GameObject> attached_packages;

    public bool emergency=false;
    public bool overload=false;
    public float overload_time;
    public float emergency_reaction_delay;
    private float emergency_time = float.PositiveInfinity;
    public float emergency_duration;

    public void connection(bool connected){
        if (disconnected == connected){
            disconnected = !connected;
            if(disconnected){
                inside_loading_bar_empty.GetComponent<Renderer>().material.color = Color.blue;
            }else{
                inside_loading_bar_empty.GetComponent<Renderer>().material.color = Color.grey;
            }
            server_sprite.GetComponent<Animator>().SetBool("disconnected",disconnected);
        } 
    }

    void OnMouseDown(){
        sceneManager.GetComponent<TutoManager>().clickServer(server_id);
    }

    public void reset_emergency_timer(bool overload){
        emergency = true;
        emergency_time = Time.time;
        if (loading_circle){
            Destroy(loading_circle);
        }
        loading_circle = Instantiate(loading_circle_prefab, new Vector3(0,0,0), Quaternion.identity);
        Color color = overload ? Color.red : Color.green;
        loading_circle.transform.Find("loadingCircleContent").gameObject.GetComponent<RadialTimer>().startTimer(sceneManager.GetComponent<TutoManager>().canvas,Camera.main.WorldToViewportPoint(antivirus.transform.position + new Vector3(antivirus.GetComponent<Renderer>().bounds.size.x/2,antivirus.GetComponent<Renderer>().bounds.size.y/2,0)), color);
        if (overload){
            sceneManager.GetComponent<TutoManager>().playErrorSound();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        packages_received = 0;
        attached_packages = new List<GameObject>(){};
        overload_time = -emergency_reaction_delay;
        disconnected = true;
        inside_loading_bar_empty.GetComponent<Renderer>().material.color = Color.blue;
        server_sprite.GetComponent<Animator>().SetBool("disconnected",disconnected);
        inside_loading_bar_empty.transform.localScale = new Vector3(30,inside_loading_bar.transform.localScale.y,0);
        outside_loading_bar.transform.position = new Vector3(transform.position.x, transform.position.y - (19*server_sprite.GetComponent<Renderer>().bounds.size.y/32), 0);
        inside_loading_bar_empty.transform.position = new Vector3(transform.position.x, transform.position.y - (19*server_sprite.GetComponent<Renderer>().bounds.size.y/32), 0);
        inside_loading_bar.transform.position = new Vector3(transform.position.x, transform.position.y - (19*server_sprite.GetComponent<Renderer>().bounds.size.y/32), 0);
        loading_bar_starting_pos = inside_loading_bar.transform.position - new Vector3(inside_loading_bar_empty.GetComponent<Renderer>().bounds.size.x/2, 0, 0);
        inside_loading_bar.transform.localScale = new Vector3(0,1,0);
        loading_circle = null;
    }

    // Update is called once per frame
    void Update()
    {
        if(sceneManager.GetComponent<TutoManager>().work[server_id]!=current_work){
            current_work = sceneManager.GetComponent<TutoManager>().work[server_id];
            int delta = sceneManager.GetComponent<TutoManager>().max_work - current_work;
            if (delta<0){
                inside_loading_bar.transform.localScale = new Vector3(30, inside_loading_bar.transform.localScale.y, 0);
                inside_loading_bar.GetComponent<Renderer>().material.color = Color.red;
            }else{
                float emergency_animation_slowdown = 1f;
                if(emergency){
                    emergency_animation_slowdown=0.5f;
                }
                animator.speed = emergency_animation_slowdown*1.5f*(float)sceneManager.GetComponent<TutoManager>().max_work/(float)(current_work+sceneManager.GetComponent<TutoManager>().max_work);
                inside_loading_bar.transform.localScale = new Vector3(30*current_work/sceneManager.GetComponent<TutoManager>().max_work, inside_loading_bar.transform.localScale.y, 0);
                if (delta < 2){
                    inside_loading_bar.GetComponent<Renderer>().material.color = new Color(1.0f, 0.64f, 0.0f);//orange
                }else if(current_work>3){
                    inside_loading_bar.GetComponent<Renderer>().material.color = Color.yellow;
                }else if (current_work>0){
                    inside_loading_bar.GetComponent<Renderer>().material.color = Color.green;
                }
            }
            server_sprite.GetComponent<Animator>().SetBool("overload",delta<0);
            inside_loading_bar.transform.position = loading_bar_starting_pos + new Vector3(inside_loading_bar.GetComponent<Renderer>().bounds.size.x/2, 0, 0);       
        }
        if(Time.time>emergency_time+emergency_duration){
            emergency_time = float.PositiveInfinity;
            emergency = false;
            current_work=-1;
            antivirus.gameObject.SetActive(false);
            Destroy(loading_circle);
            sceneManager.GetComponent<TutoManager>().task_done=true;
            sceneManager.GetComponent<TutoManager>().increaseLevel(1);
        }else if(emergency){
            loading_circle.transform.Find("loadingCircleContent").gameObject.GetComponent<RadialTimer>().updateProgress(Time.deltaTime/emergency_duration);
        }
    }
}
