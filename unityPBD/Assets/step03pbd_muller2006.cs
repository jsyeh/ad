using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class step03pbd_muller2006 : MonoBehaviour
{
    const int N=8;
    GameObject [] sphere=new GameObject[N];
    Vector3 [] gradient = new Vector3[N];
    Vector3 [] v =new Vector3[N];
    Vector3 [] pos = new Vector3[N];
    Vector3 [] pos0 = new Vector3[N];
    int frame=0;
    void Start()
    {
        for(int i=0; i<N; i++){
            sphere[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere[i].transform.position = new Vector3(i*1.5f, 0, 0);
            gradient[i] = new Vector3();
            v[i]=new Vector3();
            pos[i]=sphere[i].transform.position;
            pos0[i]=sphere[i].transform.position;
        }
        genStretchBendingConstraint();
    }

    void Update()
    {
        frame++;
        if(frame%2==0) return;
        solver();
        for(int i=1; i<N; i++){
            sphere[i].transform.position = pos0[i];
        }
    }
    float C(int Cj){
        if(Cj<N-1) return Ci(Cj,pos);    
        else return Cbendi(Cj-(N-1), pos);
    }
    float d0=1.5f;
    float Ci(int i, Vector3 [] pos){
        Vector3 ans=new Vector3();
        for(int j=0;j<N;j++){//TODO: 變成N
            float w=a[i,j];
            ans += pos[j]*w;
        }
        return ans.sqrMagnitude - d0*d0;
    }
    float angle0=0;
    int [,]a;
    int [,]b;
    void genStretchBendingConstraint(){
        a = new int[N-1,N];
        for(int i=0;i<N-1;i++){
            for(int j=0;j<N;j++){
            if(i==j) a[i,j]=-1;
            else if(i+1==j) a[i,j]=1;
            else a[i,j]=0;
            }
        }
        b = new int[N-2,4];
        for(int i=0;i<N-2;i++){
            b[i,0]=i+1;
            b[i,1]=i;
            b[i,2]=i+2;
            b[i,3]=i+1;
        }
    }

    float Cbendi(int i, Vector3 [] pos){
        Vector3 v1 = pos[ b[i,0] ] - pos[ b[i,1] ];
        Vector3 v2 = pos[ b[i,2] ] - pos[ b[i,3] ];
        return 0.01f*(Vector3.Angle(v1,v2)-angle0);
    }
    void calcGradient(int Cj, int Xi){//input:第幾個constraint, 第幾個點
        float d=0.001f;//for gradient distance//未來:需要依需求,讓d變得更小,才可讓gradient sum為0的問題不出現
        float f1, f0;
        pos[Xi].x+=d;
        f1=C(Cj);
        pos[Xi].x-=d*2;
        f0=C(Cj);
        pos[Xi].x+=d;
        gradient[Xi].x=(f1-f0)/2/d;

        pos[Xi].y+=d;
        f1=C(Cj);
        pos[Xi].y-=d*2;
        f0=C(Cj);
        pos[Xi].y+=d;
        gradient[Xi].y=(f1-f0)/2/d;

        pos[Xi].z+=d;
        f1=C(Cj);
        pos[Xi].z-=d*2;
        f0=C(Cj);
        pos[Xi].z+=d;
        gradient[Xi].z=(f1-f0)/2/d;
    }

    void projectConstraints()
    {
        float maxMag=0;
        float []w=new float[N];//{0.2,0.2,0.2,0.2,0.2};//0.33333, 0.33333, 0.33333};
        for(int i=0;i<N;i++) w[i]=1.0f/N;
        for(int Cj=0; Cj<(N-1)+(N-2); Cj++){
            //partial gradient C[?]
            for(int Xi=1;Xi<N;Xi++){
                calcGradient(Cj, Xi);
            }
            float gradientSum=0;
            for(int Xi=1;Xi<N;Xi++){
                gradientSum += w[Xi]*gradient[Xi].sqrMagnitude;
            }
            if(gradientSum<0.0000001){
                print("gradient sum very small");
            continue;
            }
            float lambda = C(Cj)/gradientSum;
            for(int Xi=1;Xi<N;Xi++){  //更新
                Vector3 diff= gradient[Xi] * - lambda*w[Xi];
                pos[Xi] += diff;
                if(diff.magnitude>maxMag) maxMag=diff.magnitude;
            }
        }
    }
    void solver()
    {
        Vector3 g = new Vector3(0, -0.98f/20, 0);
        for(int i=1; i<N; i++){
            v[i] += g;//Step (5) 還沒有乘上 delta T 及重量
            pos[i] = pos0[i]+v[i];//Step (7)
            v[i] *=0.999999f;
        }

        projectConstraints();

        for(int i=1; i<N; i++){
            v[i] = pos[i] - pos0[i];//Step (13)
            pos0[i] = pos[i];//Step (14)
        }
    }
}
