using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniPackageBehaviour : MonoBehaviour
{
    public Sprite[] minilogoSprites;

    public int from_server_id;
    public int to_server_id;
    public float basic_speed;
    public float loading_duration;
    public float time_stamp;
    public bool moving;
    private int os_type;
    public int attached_server;
    public int attach_position;
    public int step;

    public int last_topology_version_checked;

    public bool path_exists;

    private bool pause;
    private float start_pause_time;
    private float end_pause_time;

    public GameObject sceneManager;

    public Vector3 start_pos;
    public Vector3 target_pos;
    Vector3 direction;

    public void getAttachedToServer(int server_id){
        from_server_id = server_id;
        start_pos = sceneManager.GetComponent<VariablesManager>().getServerPositionFromId(from_server_id);
        GameObject server = sceneManager.GetComponent<Spawner>().servers[server_id];
        float offset = server.transform.Find("server_sprite").gameObject.GetComponent<Renderer>().bounds.size.x/2 + gameObject.GetComponent<Renderer>().bounds.size.x/2;
        attach_position = server.GetComponent<ServerBehaviour>().attached_packages.Count;
        transform.position = server.transform.position + new Vector3(offset + gameObject.GetComponent<Renderer>().bounds.size.x * attach_position, offset, 0);
        server.GetComponent<ServerBehaviour>().attached_packages.Add(gameObject);
        attached_server = server_id;

    }

    public void detachFromServer(){
        if (attached_server > -1 && attached_server < sceneManager.GetComponent<Spawner>().servers.Count && attach_position > -1){
            sceneManager.GetComponent<Spawner>().servers[attached_server].GetComponent<ServerBehaviour>().attached_packages.RemoveAt(attach_position);
            for (int i=attach_position; i < sceneManager.GetComponent<Spawner>().servers[attached_server].GetComponent<ServerBehaviour>().attached_packages.Count; i++){
                sceneManager.GetComponent<Spawner>().servers[attached_server].GetComponent<ServerBehaviour>().attached_packages[i].transform.position -= new Vector3(gameObject.GetComponent<Renderer>().bounds.size.x,0,0);
                sceneManager.GetComponent<Spawner>().servers[attached_server].GetComponent<ServerBehaviour>().attached_packages[i].GetComponent<MiniPackageBehaviour>().attach_position --;
            }

        }else{
            Debug.Log("alert;"+attached_server.ToString()+";"+sceneManager.GetComponent<Spawner>().servers.Count.ToString()+";"+attach_position.ToString());
        }
        attached_server = -1;
    }

    public void assignOsType(int i){
        os_type = i;
        GetComponent<SpriteRenderer>().sprite = minilogoSprites[os_type-1];
    }
    
    public void startUpload(int server_id){
        time_stamp = Time.time;
        getAttachedToServer(server_id);
    }

    void getDownloaded()
    {
        sceneManager.GetComponent<VariablesManager>().packageDelivered(to_server_id);
        if (attached_server > -1){
            detachFromServer();
        }
        sceneManager.GetComponent<VariablesManager>().playRandomBeep();
        Destroy(gameObject);
    }

    void getUploaded()
    {
        to_server_id = sceneManager.GetComponent<VariablesManager>().nextServerId(from_server_id, os_type,basic_speed, step);
        //Debug.Log(from_server_id.ToString()+";"+to_server_id.ToString());
        last_topology_version_checked = sceneManager.GetComponent<VariablesManager>().topology_version;
        if(to_server_id > -1){
            path_exists = true;
            target_pos = sceneManager.GetComponent<VariablesManager>().getServerPositionFromId(to_server_id);
            direction = (target_pos - start_pos).normalized;
            detachFromServer();
            step++;
            transform.position = start_pos;
            moving = true;
            sceneManager.GetComponent<VariablesManager>().addPackageToLine(from_server_id,to_server_id);
        }else{
            path_exists = false;
            moving =false;
        }
    }

    void Awake()
    {
        pause = false;
        moving = false;
        path_exists = true;
        attached_server = -1;
        attach_position = -1;
        last_topology_version_checked = -1;
        step=0;
    }
    // Start is called before the first frame update
    void Start()
    {
    
    }

    // Update is called once per frame
    void Update()
    {
        if (!sceneManager.GetComponent<VariablesManager>().game_finished){
            if(moving && sceneManager.GetComponent<VariablesManager>().connections[from_server_id][to_server_id]<0){
                moving = false;
                path_exists = false;
                sceneManager.GetComponent<VariablesManager>().removePackageFromLine(from_server_id,to_server_id);
                getAttachedToServer(from_server_id);
            }
            if (pause!=sceneManager.GetComponent<VariablesManager>().pause){
                if (pause){
                    end_pause_time = Time.time;
                    time_stamp += end_pause_time - start_pause_time;
                }else{
                    start_pause_time = Time.time;
                }
                pause=sceneManager.GetComponent<VariablesManager>().pause;
            }
            if (pause){

            }else{
                /*if (moving && (from_server_id<0 || to_server_id<0)){
                    Debug.Log("alert");
                    moving=false;
                    path_exists=false;
                }*/
                if (moving){
                    int traffic = sceneManager.GetComponent<VariablesManager>().connections[from_server_id][to_server_id];
                    float emergency_slow_down = 1f;
                    if (sceneManager.GetComponent<VariablesManager>().getServerFromId(from_server_id).GetComponent<ServerBehaviour>().emergency){
                        emergency_slow_down *= 2f;
                    }
                    if(sceneManager.GetComponent<VariablesManager>().getServerFromId(to_server_id).GetComponent<ServerBehaviour>().emergency ){
                        emergency_slow_down *= 2f;
                    }
                    transform.position += direction*(basic_speed/emergency_slow_down)*Time.deltaTime*3/(traffic+3);
                    if ((transform.position-start_pos).sqrMagnitude >= (target_pos-start_pos).sqrMagnitude){
                        sceneManager.GetComponent<VariablesManager>().removePackageFromLine(from_server_id,to_server_id);
                        from_server_id = to_server_id;
                        to_server_id = sceneManager.GetComponent<VariablesManager>().nextServerId(from_server_id, os_type,basic_speed, step);
                        //Debug.Log(from_server_id.ToString()+";"+to_server_id.ToString());
                        if (from_server_id == to_server_id){
                            time_stamp = Time.time;
                            moving = false;
                            path_exists = true;
                            getDownloaded();
                        }else if (to_server_id > -1){
                            start_pos = target_pos;
                            target_pos = sceneManager.GetComponent<VariablesManager>().getServerPositionFromId(to_server_id);
                            direction = (target_pos - start_pos).normalized;
                            sceneManager.GetComponent<VariablesManager>().addPackageToLine(from_server_id,to_server_id);
                        }else{
                            path_exists = false;
                            moving = false;
                            getAttachedToServer(from_server_id);
                        }

                    }
                    
                }else if(path_exists){
                    if(Time.time > time_stamp + loading_duration){
                        getUploaded();
                    }
                }else if(!(last_topology_version_checked==sceneManager.GetComponent<VariablesManager>().topology_version)){
                    getUploaded();
                }
            }
        }
    }
}
