using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.SceneManagement;

public class VariablesManager : MonoBehaviour
{

    public bool pause;

    public GameObject canvas;

    public AudioSource audioSource;

    public AudioClip plop;
    public AudioClip[] loud_beeps;
    public AudioClip[] quiet_beeps;
    public AudioClip error_sound;
    public AudioClip pling_deco;
    public AudioClip pling_co;
    public AudioClip complex_beep;
    public AudioClip police_siren;

    public List<string> level_names;
    public List<int> level_values;
    public int game_breaker_level = 0;

    public int max_work;

    public float score;
    public float mine_rate;

    public bool game_finished;
    public bool score_printed_in_final_screen;

    public List<List<int>> connections;

    public List<List<int>> positions;
    public List<List<float>> positionsOnHold;

    private float unit=-1f;

    public List<int> work;

    public Text score_text;
    public Text achievements_text;
    public GameObject you_got_arrested_msg;

    private int server_selected = -1;

    public Vector3 serverTransformPosition;
    public Vector3 mousePos;
    public int intMouseX=-1;
    public int intMouseY=-1;

    private bool line_ok=true;

    public List<int> serversInTheWay;

    public int topology_version = 0;

    public float high_score;
    public int largest_network;
    public int best_game_breaking;

    public int intPosition(float x){
        if (unit<0){
            return -1;
        }
        int i = (int)Mathf.Round(x/unit);
        i*=2;
        if(i<0){
            i = -(i+1);
        }
        return i;
    }

    public void registerServerPosition(float x, float y, int server_id){
        int i = intPosition(x);
        if(i==-1){
            positionsOnHold.Add(new List<float>(){});
            positionsOnHold[positionsOnHold.Count-1].Add(x);
            positionsOnHold[positionsOnHold.Count-1].Add(y);
            positionsOnHold[positionsOnHold.Count-1].Add((float)server_id);
            return;
        }
        int j = intPosition(y);
        while(i>=positions.Count){
            positions.Add(new List<int>(){});
        }
        for (int k=0; k<positions.Count; k++){
            while(j>=positions[k].Count){
                positions[k].Add(-1);
            }
        }
        positions[i][j] = server_id;
    }

    public int getServerAtPosition(int i, int j){
        while(i>=positions.Count){
            positions.Add(new List<int>(){});
        }
        for (int k=0; k<positions.Count; k++){
            while(j>=positions[k].Count){
                positions[k].Add(-1);
            }
        }
        return positions[i][j];
    }

    public void setUnit(float u){
        unit = u;
        for (int k=0; k<positionsOnHold.Count; k++){
            float x = positionsOnHold[k][0];
            float y = positionsOnHold[k][1];
            int server_id = (int)positionsOnHold[k][2];
            registerServerPosition(x,y,server_id);
        }
    }

    public void playErrorSound(){
        audioSource.PlayOneShot(error_sound,1f);
    }

    public void playRandomBeep(){
        int beep_index = Random.Range(0,loud_beeps.Length+quiet_beeps.Length);
        if (beep_index < loud_beeps.Length){
            audioSource.PlayOneShot(loud_beeps[beep_index],0.2f);
        }else{
            audioSource.PlayOneShot(quiet_beeps[beep_index-loud_beeps.Length],0.6f);
        }
    }

    public void setPauseValue(bool value){
        pause = value;
    }

    public void finishGame(){
        game_finished = true;
        you_got_arrested_msg.SetActive(true);
        pause = true;
        audioSource.PlayOneShot(police_siren,1f);
    }

    public void packageDelivered(int delivery_server){
        score += mine_rate*max_work;
        getServerFromId(delivery_server).GetComponent<ServerBehaviour>().receive_package();
    }

    public void changeTopology(){
        topology_version++;
        intMouseX = -1;
        intMouseY = -1;
    }

    public void addPackageToLine(int start, int end){
        connections[start][end]++;
    }

    public void removePackageFromLine(int start, int end){
        connections[start][end]--;
    }

    public int nextServerId(int current_id, int os_type, float basic_speed, int step){
        List<bool> next = new List<bool>(){};
        List<int> os_types = new List<int>(){};
        List<int> path = new List<int>(){};
        List<float> distances = new List<float>(){};
        for (int i=0; i < GetComponent<Spawner>().servers.Count; i++){
            os_types.Add(GetComponent<Spawner>().servers[i].GetComponent<ServerBehaviour>().os_type);
            path.Add(-1);
            if (i==current_id){
                next.Add(true);
                distances.Add(0);
                if(os_types[i]==os_type){
                    return current_id;
                }
            }else{
                next.Add(false);
                distances.Add(float.PositiveInfinity);
            }
        }
        List<List<float>> weights = new List<List<float>>(){};
        for (int i=0; i<next.Count; i++){
            weights.Add(new List<float>());
            for (int j=0; j<next.Count; j++){
                if (i!=j && connections[i][j]>-1){
                    float len = (getServerPositionFromId(i)-getServerPositionFromId(j)).magnitude;
                    if(i==current_id || j==current_id){
                        float emergency_slow_down = 1f;
                        if(getServerFromId(current_id).GetComponent<ServerBehaviour>().emergency){
                            emergency_slow_down *= 2f;
                        }
                        if(work[i+j-current_id]+1>max_work || getServerFromId(i+j-current_id).GetComponent<ServerBehaviour>().emergency){
                            emergency_slow_down *= 2f;
                        }
                        float speed = (basic_speed/emergency_slow_down)*3/(1+connections[i][j]+3);
                        weights[weights.Count-1].Add(len*((basic_speed/speed)+step)/(step+1));
                    }else{
                        weights[weights.Count-1].Add(len);
                    }
                }else{
                    weights[weights.Count-1].Add(float.PositiveInfinity);
                }
            }
        }
        List<List<float>> weights_with_traffic = new List<List<float>>(){};
        for (int i=0; i<next.Count; i++){
            weights_with_traffic.Add(new List<float>());
            for (int j=0; j<next.Count; j++){
                if (i!=j && connections[i][j]>-1){
                    float len = (getServerPositionFromId(i)-getServerPositionFromId(j)).magnitude;
                    float emergency_slow_down = 1f;
                    if(i==current_id || j==current_id){
                        if(getServerFromId(current_id).GetComponent<ServerBehaviour>().emergency){
                            emergency_slow_down *= 2f;
                        }
                        if(work[i+j-current_id]+1>max_work || getServerFromId(i+j-current_id).GetComponent<ServerBehaviour>().emergency){
                            emergency_slow_down *= 2f;
                        }
                    }else{
                        if(work[i]+1>max_work || getServerFromId(i).GetComponent<ServerBehaviour>().emergency){
                            emergency_slow_down *= 2f;
                        }
                        if(work[j]+1>max_work || getServerFromId(j).GetComponent<ServerBehaviour>().emergency){
                            emergency_slow_down *= 2f;
                        }
                    }
                    float speed = (basic_speed/emergency_slow_down)*3/(1+connections[i][j]+3);
                    weights_with_traffic[weights_with_traffic.Count-1].Add(len*(basic_speed/speed));
                }else{
                    weights_with_traffic[weights_with_traffic.Count-1].Add(float.PositiveInfinity);
                }
            }
        }
        int current_server;
        int best_server = -1;
        float best_dist = float.PositiveInfinity;
        int next_count = 1;
        while(next_count>0){
            int k = 0;
            while(!next[k]){
                k++;
            }
            current_server = k;
            for (int i=current_server+1; i<next.Count; i++){
                if (next[i] && distances[i]<distances[current_server]){
                    current_server = i;
                }
            }
            if (distances[current_server]>best_dist){
                return best_server;
            }
            for (int i=0; i<next.Count; i++){
                int j = current_server;
                int depth = 0;
                while(path[j]!=-1){
                    depth++;
                    j=path[j];
                }
                float local_weigth = (weights[current_server][i]*(depth+step) + weights_with_traffic[current_server][i])/(depth+step+1);
                if (distances[current_server]+local_weigth<distances[i]){
                    distances[i] = distances[current_server]+local_weigth;
                    next[i] = true;
                    path[i] = current_server;
                    if (os_types[i] == os_type && distances[i] < best_dist){
                        best_dist = distances[i];
                        j=i;
                        while(path[j]!=current_id){
                            j = path[j];
                        }
                        best_server = j;
                    }
                }
            }

            next[current_server] = false;
            next_count = 0;
            for (int i=0; i<next.Count; i++){
                if (next[i]){
                    next_count++;
                }
            }
        }
        return best_server;
    }

    /*public int nextServerId(int current_id, int os_type){
        //
        //to do
        //fix the algo it does not work proprely
        //
        List<int> seen = new List<int>(){};
        List<int> milestones = new List<int>(){-1,-1};
        List<int> candidates = new List<int>(){};
        for (int i=0; i < GetComponent<Spawner>().servers.Count; i++){
            if (GetComponent<Spawner>().servers[i].GetComponent<ServerBehaviour>().os_type == os_type){
                if (i == current_id){
                    return current_id;
                }
                seen.Add(i);//=GetComponent<Spawner>().servers[i].GetComponent<ServerBehaviour>().server_id
                milestones[milestones.Count-1] += 1;
            }
        }
        int new_server_count = seen.Count;
        while (new_server_count > 0 && candidates.Count == 0){
            new_server_count = 0;
            milestones.Add(milestones[milestones.Count-1]);
            for (int i=milestones[milestones.Count-3]+1; i < milestones[milestones.Count-2]+1; i++){
                int analysed_server = seen[i];
                for (int j=0; j<connections[seen[i]].Count; j++){
                    if(connections[seen[i]][j]>-1){
                        bool server_already_seen = false;
                        for (int k=0; k<seen.Count; k++){
                            if (j==seen[k]){
                                server_already_seen = true;
                                break;
                            }
                        }
                        if (server_already_seen){
                            continue;
                        }
                        if (j == current_id){
                            candidates.Add(seen[i]);
                        }
                        seen.Add(j);
                        new_server_count++;
                        milestones[milestones.Count-1] += 1;
                    }
                }
            }
        }
        if (candidates.Count > 0){
            int i = 0;
            //Debug.Log("start");
            //Debug.Log(candidates);
            for (int k=0; k<candidates.Count; k++){
                //Debug.Log(candidates[i].ToString()+";"+work[candidates[i]].ToString()+";"+candidates[k].ToString()+";"+work[candidates[k]].ToString()+";");
                if (work[candidates[k]]<work[candidates[i]]){
                    i=k;
                }
            }
            //Debug.Log(i);
            //Debug.Log("end");
            //return candidates[Random.Range(0,candidates.Count)];
            return candidates[i];
        }
        return -1;
    }*/

    public void clickServer(int server_id){
        if(!game_finished){
            if (server_selected < 0){
                server_selected = server_id;
                serverTransformPosition = getSelectedServerPosition();
                return;
            }
            if (server_selected == server_id){
                server_selected = -1;
                return;
            }
            int pos1=server_selected;
            int pos2=server_id;

            if (connections[pos1][pos2] < 0 && connections[pos2][pos1] < 0){
                if (line_ok){
                    connections[pos1][pos2] = 0;
                    connections[pos2][pos1] = 0;
                    GetComponent<Spawner>().spawnLine(getServerPositionFromId(server_selected), getServerPositionFromId(server_id), pos1, pos2);
                    changeTopology();
                }else{
                    audioSource.PlayOneShot(pling_deco,0.8f);
                }
            }else{
                connections[pos1][pos2] = -1;
                connections[pos2][pos1] = -1;
                for (int i=0; i < GetComponent<Spawner>().lines.Count; i++){
                    if((GetComponent<Spawner>().lines[i].GetComponent<ConnectionBehaviour>().matrix_pos1 == pos1 &&
                        GetComponent<Spawner>().lines[i].GetComponent<ConnectionBehaviour>().matrix_pos2 == pos2)
                    || (GetComponent<Spawner>().lines[i].GetComponent<ConnectionBehaviour>().matrix_pos1 == pos2 &&
                        GetComponent<Spawner>().lines[i].GetComponent<ConnectionBehaviour>().matrix_pos2 == pos1))
                    {
                        Destroy(GetComponent<Spawner>().lines[i]);
                        GetComponent<Spawner>().lines.RemoveAt(i);
                        changeTopology();
                        audioSource.PlayOneShot(pling_deco,0.8f);
                        break;
                    }
                }
                
            }
        }
        server_selected = -1;
    }

    public GameObject getCanvas(){
        return canvas;
    }

    public GameObject getServerFromId(int server_id){
        return GetComponent<Spawner>().servers[server_id];
    }

    public int getSelectedServer(){
        return server_selected;
    }

    public Vector3 getServerPositionFromId(int server_id){
        return getServerFromId(server_id).transform.position;
    }

    public Vector3 getSelectedServerPosition(){
        return getServerPositionFromId(server_selected);
    }

    public void addNewServerToTheNetwork(float x, float y, int server_id){
        if (connections== null){
            connections = new List<List<int>>(){};
        }
        connections.Add(new List<int>(){});
        for (int i=0; i<connections.Count-1; i++){
            connections[i].Add(-1);
            connections[connections.Count-1].Add(-1);
        }
        connections[connections.Count-1].Add(-1);
        work.Add(0);
        registerServerPosition(x, y, server_id);
        changeTopology();
        for (int i=0; i<connections.Count; i++){
            for (int j=0; j<i; j++){
                if (connections[i][j]>-1){
                    Vector3 pos1 = getServerPositionFromId(i);
                    Vector3 pos2 = getServerPositionFromId(j);
                    Vector3 dir = new Vector3(pos1.x - pos2.x, pos1.y - pos2.y, 0);
                    dir = (1/dir.magnitude)*dir;
                    Vector3 ortho = new Vector3(dir.y, -dir.x, 0);
                    float dist = (x-pos1.x)*ortho.x + (y-pos1.y)*ortho.y;
                    float dot_product1 = (x-pos1.x)*dir.x + (y-pos1.y)*dir.y;
                    float dot_product2 = (x-pos2.x)*dir.x + (y-pos2.y)*dir.y;             
                    if (dist<0){
                        dist = -dist;
                    }
                    if(dot_product1*dot_product2<0 && 2*dist<unit){
                        connections[i][j] = -1;
                        connections[j][i] = -1;
                        for (int k=0; k < GetComponent<Spawner>().lines.Count; k++){
                            if((GetComponent<Spawner>().lines[k].GetComponent<ConnectionBehaviour>().matrix_pos1 == i &&
                                GetComponent<Spawner>().lines[k].GetComponent<ConnectionBehaviour>().matrix_pos2 == j)
                            || (GetComponent<Spawner>().lines[k].GetComponent<ConnectionBehaviour>().matrix_pos1 == j &&
                                GetComponent<Spawner>().lines[k].GetComponent<ConnectionBehaviour>().matrix_pos2 == i))
                            {
                                Destroy(GetComponent<Spawner>().lines[k]);
                                GetComponent<Spawner>().lines.RemoveAt(k);
                                changeTopology();
                                audioSource.PlayOneShot(pling_deco,0.8f);
                                return;
                            }
                        }
                    }
                }
            }
        }
    }

    bool HasMouseMoved()
    {
        return (Input.GetAxis("Mouse X") != 0) || (Input.GetAxis("Mouse Y") != 0);
    }

    void Awake()
    {
        level_names = new List<string>(){"baby AI","small AI","big AI","world's conqueror AI"};
        level_values = new List<int>(){4,8,16,32};
        audioSource = GetComponent<AudioSource>();
        score = 0;
        connections = new List<List<int>>(){};
        positions = new List<List<int>>(){};
        positionsOnHold = new List<List<float>>(){};
        serversInTheWay = new List<int>(){};
        work = new List<int>(){};
        game_finished = false;
        score_printed_in_final_screen = false;
        pause = false;
        intMouseX = -1;
        intMouseY = -1;
        high_score = PlayerPrefs.GetFloat("high_score", 0);
        largest_network = PlayerPrefs.GetInt("largest_network", 3);
        best_game_breaking = PlayerPrefs.GetInt("best_game_breaking", 0);
    }

    // Start is called before the first frame update
    void Start()
    {
        score_text.text = "\n    "+(0).ToString("000 000.000") + " BTC (highscore : "+high_score.ToString("000 000.000")+" BTC)";
    }

    // Update is called once per frame
    void Update()
    {
        if (!game_finished){
            if (server_selected<0){
                GetComponent<DrawLine>().lineRenderer.enabled = false;
                intMouseX = -1;
                intMouseY = -1;
            }else{
                GetComponent<DrawLine>().lineRenderer.enabled = true;
                mousePos = Input.mousePosition;
                mousePos = Camera.main.ScreenToWorldPoint(mousePos);
                Vector3 dir = new Vector3(mousePos.x - serverTransformPosition.x, mousePos.y - serverTransformPosition.y, 0);
                dir = (1/dir.magnitude)*dir;
                Vector3 ortho = new Vector3(dir.y, -dir.x, 0);
                line_ok = true;
                if (unit>0){
                    int k = intPosition(serverTransformPosition.x);
                    int l = intPosition(serverTransformPosition.y);
                    int i = intPosition(mousePos.x);
                    int j = intPosition(mousePos.y);
                    int server_hovered = -1;
                    for(int c=0; c<work.Count; c++){
                        Vector3 pos = getServerPositionFromId(c);
                        float delta_x = (mousePos.x-pos.x);
                        if (delta_x<0){
                            delta_x=-delta_x;
                        }
                        float delta_y = (mousePos.y-pos.y);
                        if (delta_y<0){
                            delta_y=-delta_y;
                        }
                        if(delta_x<=unit/2 && delta_y<=unit/2){
                            mousePos = pos;
                            if(intMouseX != intPosition(mousePos.x) || intMouseY != intPosition(mousePos.y)){
                                intMouseX = -1;
                                intMouseY = -1;
                            }
                            server_hovered = c;
                            break;
                        }
                    }
                    if (intMouseX != i || intMouseY != j){
                        intMouseX = i;
                        intMouseY = j;
                        serversInTheWay = new List<int>(){};
                        //to make sure the list isnt empty
                        i = getServerAtPosition(0,0);
                        for (i=0; i<positions.Count; i++){
                            for (j=0; j<positions[0].Count; j++){
                                int server_id = getServerAtPosition(i,j);
                                if(server_id>-1 && server_id!=server_selected){
                                    Vector3 pos = getServerPositionFromId(server_id);
                                    float dist = (pos.x-serverTransformPosition.x)*ortho.x + (pos.y-serverTransformPosition.y)*ortho.y;
                                    if (dist<0){
                                        dist = -dist;
                                    }
                                    if (dist<2*unit){
                                        serversInTheWay.Add(server_id);
                                    }
                                }
                            }
                        }
                    }
                    
                    for (int c=0; c<serversInTheWay.Count; c++){
                        int server_id = serversInTheWay[c];
                        Vector3 pos = getServerPositionFromId(server_id);
                        float dist = (pos.x-serverTransformPosition.x)*ortho.x + (pos.y-serverTransformPosition.y)*ortho.y;
                        float dot_product1 = (pos.x-mousePos.x)*dir.x + (pos.y-mousePos.y)*dir.y;
                        float dot_product2 = (pos.x-serverTransformPosition.x)*dir.x + (pos.y-serverTransformPosition.y)*dir.y;
                        if (dist<0){
                            dist = -dist;
                        }
                        if (dist*dist<unit*unit/4 && dot_product1*dot_product2<0){
                            float dist2 = (mousePos.x-pos.x)*(mousePos.x-pos.x) + (mousePos.y-pos.y)*(mousePos.y-pos.y);
                            line_ok = (server_hovered==c);
                            if(!line_ok){
                                break;
                            }
                        }
                    }
                    if (line_ok){
                        for (int p=0; p<connections.Count; p++){
                            for (int q=0; q<p; q++){
                                if (connections[p][q]>-1){
                                    Vector3 pos1 = getServerPositionFromId(p);
                                    Vector3 pos2 = getServerPositionFromId(q);
                                    Vector3 dir2 = new Vector3(pos1.x - pos2.x, pos1.y - pos2.y, 0);
                                    Vector3 ortho2 = new Vector3(dir2.y, -dir2.x, 0);
                                    float dot_product1 = (mousePos.x-pos1.x)*ortho.x + (mousePos.y-pos1.y)*ortho.y;
                                    float dot_product2 = (mousePos.x-pos2.x)*ortho.x + (mousePos.y-pos2.y)*ortho.y;
                                    float dot_product3 = (mousePos.x-pos1.x)*ortho2.x + (mousePos.y-pos1.y)*ortho2.y;
                                    float dot_product4 = (serverTransformPosition.x-pos1.x)*ortho2.x + (serverTransformPosition.y-pos1.y)*ortho2.y;
                                    if (dot_product1*dot_product2<0 && dot_product3*dot_product4<0){
                                        line_ok=false;
                                        break;
                                    }
                                }           
                            }
                            if(!line_ok){
                                break;
                            }
                        }
                    }
                }
                GetComponent<DrawLine>().setColor(line_ok?Color.black:Color.red);
                GetComponent<DrawLine>().RenderLine(serverTransformPosition,mousePos);
            }
            for (int i=0; i<work.Count; i++){
                work[i] = 2*getServerFromId(i).GetComponent<ServerBehaviour>().attached_packages.Count;
                bool disconnected = true;
                for (int j=0; j<connections[i].Count; j++){
                    if (connections[i][j] > -1){
                        disconnected = false;
                        work[i] += 1;
                        work[i] += connections[i][j];
                        work[i] += connections[j][i];
                    } 
                }
                getServerFromId(i).GetComponent<ServerBehaviour>().connection(!disconnected);
                if (!pause && !disconnected){
                    score += mine_rate*Time.deltaTime/(work[i]+1);
                    if(score>high_score){
                        high_score = score;
                        PlayerPrefs.SetFloat("high_score", high_score);
                        PlayerPrefs.Save();
                    }
                    score_text.text = "\n    "+score.ToString("000 000.000") + " BTC (highscore : "+high_score.ToString("000 000.000")+" BTC)";
                } 
            }
        }

    }
}
