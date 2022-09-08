using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(LineRenderer))]

public class DrawLine : MonoBehaviour
{
    private float lineWidth = 0.02f;
   
    public LineRenderer lineRenderer;
    int length;
    Vector3[] positions;
    Vector3 serverTransformPosition;
    Vector3 offset;

    public void setColor(Color color){
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
    }

    void UpdateLength(){
        

        //length = (int)Mathf.Round(_CatchWildPokemons.getDistanceToPokemon()*20)+2;
        length = 2;

        positions = new Vector3[length];
 
        lineRenderer.positionCount = length;       
    }

    public void RenderLine(Vector3 startingPos, Vector3 targetPos){
        UpdateLength();
       
        //Move through the Array

        for(int i = 0; i<length; i++){
            //Set the position here to the current location and project it in the forward direction of the object it is attached to
            offset.x = ((length-1-i)*startingPos.x + i*targetPos.x)/(length-1);
            offset.y = ((length-1-i)*startingPos.y + i*targetPos.y)/(length-1);
            offset.z = ((length-1-i)*startingPos.z + i*targetPos.z)/(length-1);
            
            positions[i] = offset;
            
            lineRenderer.SetPosition(i, positions[i]);
           
        }
    }

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        setColor(Color.black);
        //lineRenderer.SetWidth(lineWidth, lineWidth);
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        offset = new Vector3(0,0,0);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

}
