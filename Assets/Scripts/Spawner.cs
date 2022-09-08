using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject server_prefab;
    public GameObject connection_prefab;
    public GameObject package_prefab;
    
    public Camera cam;

    public List<GameObject> servers;
    public List<GameObject> lines;

    public List<int> os_types_by_rarity;

    private float last_spawn_time;
    public float delay_between_spawns;
    public int max_exit_infinite_loop_count;

    public int sample_size;

    private float unit=-1;

    private int last_server_id = 0;

    private bool pause;
    private float start_pause_time;
    private float end_pause_time;

    public Sprite[] logoSprites;

    public void spawnPackage(int server_id, int package_os_type){
        GameObject new_package = Instantiate(package_prefab, servers[server_id].transform.position, Quaternion.identity);
        new_package.GetComponent<MiniPackageBehaviour>().sceneManager = gameObject;
        new_package.GetComponent<MiniPackageBehaviour>().assignOsType(package_os_type);
        new_package.GetComponent<MiniPackageBehaviour>().startUpload(server_id);
        GetComponent<AudioSource>().PlayOneShot(GetComponent<VariablesManager>().plop,0.3f);
    }

    public void spawnLine(Vector3 pos1, Vector3 pos2, int matrix_pos1, int matrix_pos2){
        GameObject new_line = Instantiate(connection_prefab, pos1, Quaternion.identity);
        new_line.GetComponent<ConnectionBehaviour>().matrix_pos1 = matrix_pos1;
        new_line.GetComponent<ConnectionBehaviour>().matrix_pos2 = matrix_pos2;
        new_line.GetComponent<DrawLine>().setColor(Color.green);
        new_line.GetComponent<DrawLine>().RenderLine(pos1, pos2);
        lines.Add(new_line);
        GetComponent<AudioSource>().PlayOneShot(GetComponent<VariablesManager>().pling_co,0.8f);
    }

    void randomServerSpawn(){
        List<float> result = getRandomLocation();
        int os_type = getRandomType();
        int exit_infinite_loop_count = max_exit_infinite_loop_count;
        while (exit_infinite_loop_count > 0 && servers.Count > 0 && result[2] < 3*servers[0].transform.Find("server_sprite").gameObject.GetComponent<Renderer>().bounds.size.x*servers[0].transform.Find("server_sprite").gameObject.GetComponent<Renderer>().bounds.size.y){
            result = getRandomLocation();
            exit_infinite_loop_count--;
        }
        if (servers.Count > 0 && result[2] < 3*servers[0].transform.Find("server_sprite").gameObject.GetComponent<Renderer>().bounds.size.x*servers[0].transform.Find("server_sprite").gameObject.GetComponent<Renderer>().bounds.size.y){
            Debug.Log("nothing found");
            GetComponent<VariablesManager>().game_breaker_level += 1;
            GetComponent<VariablesManager>().achievements_text.text = "\n"+"Game Breaker x"+GetComponent<VariablesManager>().game_breaker_level.ToString();
            if(GetComponent<VariablesManager>().game_breaker_level>GetComponent<VariablesManager>().best_game_breaking){
                GetComponent<VariablesManager>().best_game_breaking = GetComponent<VariablesManager>().game_breaker_level;
                PlayerPrefs.SetInt("best_game_breaking", GetComponent<VariablesManager>().best_game_breaking);
                PlayerPrefs.Save();
            }
            return;
        }else{
            if(servers.Count+1>GetComponent<VariablesManager>().largest_network){
                GetComponent<VariablesManager>().largest_network = servers.Count+1;
                PlayerPrefs.SetInt("largest_network", GetComponent<VariablesManager>().largest_network);
                PlayerPrefs.SetInt("best_game_breaking", 0);
                PlayerPrefs.Save();
            }
            int level = GetComponent<VariablesManager>().level_values.Count;
            for (int i=0; i < GetComponent<VariablesManager>().level_values.Count; i++){
                if(servers.Count+1 < GetComponent<VariablesManager>().level_values[i]){
                    level = i;
                    break;
                }
            }
            if (level < GetComponent<VariablesManager>().level_values.Count){
                GetComponent<VariablesManager>().achievements_text.text = "\n"+GetComponent<VariablesManager>().level_names[level]+" : "+(servers.Count+1).ToString()+" / "+GetComponent<VariablesManager>().level_values[level].ToString()+"  (largest network : "+GetComponent<VariablesManager>().largest_network.ToString()+(GetComponent<VariablesManager>().best_game_breaking>0?" +"+GetComponent<VariablesManager>().best_game_breaking.ToString():"")+")";

            }else{
                GetComponent<VariablesManager>().achievements_text.text = "\n"+"Game Finisher : "+(servers.Count+1).ToString()+" / "+GetComponent<VariablesManager>().level_values[GetComponent<VariablesManager>().level_values.Count-1].ToString()+"  (largest network : "+GetComponent<VariablesManager>().largest_network.ToString()+(GetComponent<VariablesManager>().best_game_breaking>0?" +"+GetComponent<VariablesManager>().best_game_breaking.ToString():"")+")";
            }

        }
        Vector3 p = new Vector3(result[0], result[1], 0);
        spawnServer(p.x,p.y,p.z,last_server_id,os_type);
        last_server_id ++;
    }

    public int getRandomType(){
        return os_types_by_rarity[Random.Range(0,os_types_by_rarity.Count)];
    }

    List<float> getRandomLocation(){
        float half_width_seen = Camera.main.orthographicSize * Screen.width / Screen.height;
        float half_height_seen = Camera.main.orthographicSize;
        float best_x = 0;
        float best_y = 0;
        float best_dist2 = -1;
        float x,y,dist2,dist2_temp;
        if (servers.Count > 0){
            best_dist2 = -1;
            for (int i=0; i<sample_size; i++){
                dist2 = float.PositiveInfinity;
                x = Random.Range(unit-half_width_seen, half_width_seen-unit);
                y = Random.Range(unit-half_height_seen, half_height_seen-2.0f*unit);
                for (int j=0; j<servers.Count; j++){
                    dist2_temp = Mathf.Pow((x-servers[j].transform.position.x),2)
                            + Mathf.Pow((y-servers[j].transform.position.y),2);
                    if (dist2_temp<dist2){
                        dist2 = dist2_temp;
                    }
                }
                if(dist2>best_dist2){
                    best_dist2 = dist2;
                    best_x = x;
                    best_y = y;
                }
            }
        }else{
            best_x = Random.Range (-0.95f, 0.95f);
            best_y = Random.Range (-0.9f, 0.9f);
        }
        return new List<float>(){best_x, best_y, best_dist2};
    }

    void spawnServer(float x, float y, float z, int server_id, int os_type){
        GameObject new_server = Instantiate(server_prefab, new Vector3(x, y, z), Quaternion.identity);
        new_server.GetComponent<ServerBehaviour>().setSceneManager(gameObject);
        new_server.GetComponent<ServerBehaviour>().server_id=server_id;
        new_server.GetComponent<ServerBehaviour>().os_type=os_type;
        new_server.transform.Find("logo").gameObject.GetComponent<SpriteRenderer>().sprite = logoSprites[os_type-1];
        servers.Add(new_server);
        GetComponent<VariablesManager>().addNewServerToTheNetwork(x,y,server_id);
        GetComponent<AudioSource>().PlayOneShot(GetComponent<VariablesManager>().complex_beep,0.3f);
    }

    void Awake()
    {
        pause = false;
        servers = new List<GameObject>(){};
        lines = new List<GameObject>(){};
        os_types_by_rarity = new List<int>(){1,1,1,1,2,3,3};//1=Win,2=Arc,3=App
        // load all frames in logoSprites array
        //logoSprites = Resources.LoadAll<Sprite>("Sprites/logos");
    }

    // Start is called before the first frame update
    void Start()
    {
        List<Vector2> pre_selected_pos = new List<Vector2>(){new Vector2(0,0),new Vector2(-1,1),new Vector2(1,1)};
        Vector3 p = new Vector3(pre_selected_pos[0][0], pre_selected_pos[0][1], 0);
        spawnServer(p.x,p.y,p.z,last_server_id,1);
        last_server_id ++;

        unit = servers[0].transform.Find("server_sprite").gameObject.GetComponent<Renderer>().bounds.size.x;

        GetComponent<VariablesManager>().setUnit(unit);

        p = new Vector3(pre_selected_pos[1][0]*2.0f*unit, pre_selected_pos[1][1]*2.0f*unit, 0);
        spawnServer(p.x,p.y,p.z,last_server_id,2);
        last_server_id ++;

        p = new Vector3(pre_selected_pos[2][0]*2.0f*unit, pre_selected_pos[2][1]*2.0f*unit, 0);
        spawnServer(p.x,p.y,p.z,last_server_id,3);
        last_server_id ++;

        GetComponent<VariablesManager>().achievements_text.text = "\n"+GetComponent<VariablesManager>().level_names[0]+" : "+(servers.Count).ToString()+" / "+GetComponent<VariablesManager>().level_values[0].ToString()+"  (largest network : "+GetComponent<VariablesManager>().largest_network.ToString()+(GetComponent<VariablesManager>().best_game_breaking>0?" +"+GetComponent<VariablesManager>().best_game_breaking.ToString():"")+")";

        last_spawn_time = Time.time;

    }

    // Update is called once per frame
    void Update()
    {
        if (pause!=GetComponent<VariablesManager>().pause){
            if (pause){
                end_pause_time = Time.time;
                last_spawn_time += end_pause_time - start_pause_time;
            }else{
                start_pause_time = Time.time;
            }
            pause=GetComponent<VariablesManager>().pause;
        }
        if (pause){

        }else{
            if (!gameObject.GetComponent<VariablesManager>().game_finished && Time.time > last_spawn_time + delay_between_spawns){
                randomServerSpawn();
                last_spawn_time = Time.time;
            }
        }
    }
    
}
