using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testCollision : MonoBehaviour
{
    GameObject big, SphereP0, SpereP, SphereQc;
    // Start is called before the first frame update
    void Start()
    {
        big = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        big.transform.localScale=new Vector3(4,4,4);//半徑是2
        SphereP0 = GameObject.CreatePrimitive(PrimitiveType.Sphere);//\mathbf{o} : origin of the line
        SphereP0.transform.position = new Vector3(3,0,0);//還沒撞進去
        SpereP = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        SpereP.transform.position = new Vector3(1,1,0);//撞進去
        SphereQc = GameObject.CreatePrimitive(PrimitiveType.Sphere);//{\displaystyle \mathbf {x} }\mathbf {x}  : points on the sphere
        SphereQc.transform.localScale=new Vector3(0.3f, 0.3f, 0.3f);//半徑是0.3,小
    }

    // Update is called once per frame
    void CollisionConstraint(){//from Wikipedia https://en.wikipedia.org/wiki/Line%E2%80%93sphere_intersection
        Vector3 c = big.transform.position;//center
        Vector3 o = SphereP0.transform.position;
        Vector3 u = SpereP.transform.position - SphereP0.transform.position;
        u.Normalize();// direction of line 
        print(u);
        float r = big.transform.localScale.x/2f;//半徑
        float inside= Mathf.Pow(Vector3.Dot(u,(o-c)),2 )  - (Vector3.Dot(o-c, o-c) - r*r );
        float d = - (Vector3.Dot(u, o-c)) - Mathf.Sqrt(inside);
        Vector3 Qc = o + d*u;
        SphereQc.transform.position = Qc;
        //float d = Mathf.Sqrt(pos0[jvalue].x - Qc.x);
        //Vector3 u = pos[j] - pos0[j];
        //Vector3 Qc = pos0[j] + d*u;
        //if(d<r)

    }
    void Update()
    {
        CollisionConstraint();
    }
}
