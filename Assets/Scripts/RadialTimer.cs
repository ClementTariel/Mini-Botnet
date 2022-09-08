using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class RadialTimer : MonoBehaviour
{

    public float progress;

    public void startTimer(GameObject canvas, Vector3 screen_pos, Color color){
        gameObject.transform.parent.SetParent(canvas.transform);
        gameObject.GetComponent<Image>().color = color;
        gameObject.transform.parent.GetComponent<RectTransform>().anchoredPosition = new Vector2((screen_pos.x-0.5f)*canvas.GetComponent<RectTransform>().sizeDelta.x,
                                                    (screen_pos.y-0.5f)*canvas.GetComponent<RectTransform>().sizeDelta.y);
        progress = 0;
    }

    public void updateProgress(float delta){
        progress += delta;
        gameObject.GetComponent<Image>().fillAmount = progress;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
}
