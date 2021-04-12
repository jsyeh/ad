using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testBending : MonoBehaviour
{
    public GameObject []Sphere = new GameObject[4];
    static Vector3[] P=new Vector3[4];
    public float updateP(){
        for(int i=0; i<4; i++){
            P[i] = Sphere[i].transform.position;
        }
        Vector3 v1=P[1]-P[0], v2=P[2]-P[0], v3=P[3]-P[0];
        Vector3 n1 = Vector3.Cross(v1,v2), n2 = Vector3.Cross(v1,v3);
        n1.Normalize(); n2.Normalize();
        return Mathf.Acos( Vector3.Dot(n1,n2)) *180/3.1415926f;
    }
    LineRenderer lineRenderer;
    void Start()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        print(updateP());
        Vector3 [] positions = new Vector3[4];
        for(int i=0; i<4; i++){
            positions[i] = new Vector3(P[i].x, P[i].y, P[i].z);
        }
        lineRenderer.positionCount = 4;
        lineRenderer.SetPositions( positions);
    }

    void Update()
    {
        //print(updateP());
    }
}
