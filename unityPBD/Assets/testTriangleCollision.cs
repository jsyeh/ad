using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testTriangleCollision : MonoBehaviour
{
    // Start is called before the first frame update
    GameObject []sphere = new GameObject[3];
    GameObject sphereQ;
    Vector3 [] p = new Vector3[3];
    Vector3 q, N;
    void Start()
    {
        p[0]=new Vector3(0,0,0);
        p[1]=new Vector3(0,5,0);
        p[2]=new Vector3(5,0,0);
        for(int i=0; i<3; i++){
            sphere[i]=GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere[i].transform.position = p[i];
        }
        sphereQ=GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphereQ.transform.position = new Vector3(1,1,1);
        //Step02 算外積 N
        N = Vector3.Cross(p[1]-p[0], p[2]-p[1]);
    }

    // Update is called once per frame
    void Update()
    {   //Step01 先取出大家的座標
        for(int i=0; i<3; i++){
            p[i] = sphere[i].transform.position;
        }
        q = sphereQ.transform.position;

        //Step04 算 C
        float C = Vector3.Dot(q-p[0], N);
        print(C);//看起來都對, 我們要算C的正負號

        //Step05 我們把負的C都擠到 0 去, 正的不動 (TODO要想一下)
        //使用AD把東西做正確的推擠

        //Step05 把大家的座標放回去(目前還沒有推回去,先不會用到)
        for(int i=0; i<3; i++){
            sphere[i].transform.position = p[i];
        }
        sphereQ.transform.position = q;
    }
}
