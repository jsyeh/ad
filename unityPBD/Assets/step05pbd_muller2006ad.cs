using UnityEngine;

public class step05pbd_muller2006ad : MonoBehaviour
{
    const int N=8;
    GameObject [] sphere=new GameObject[N];
    Vector3 [] gradient = new Vector3[N]; //之後由 posAD 項,算出對應的 gradient
    Vector3 [] v =new Vector3[N];
    Vector3 [] pos = new Vector3[N]; //論文中的p: 運算時會一直變動
    Vector3 [] pos0 = new Vector3[N]; //論文中的x: 最後畫出來的結果/下次的初始值
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
        if(frame%2==0) return; //希望看到與 processingPDB類似結果,所以frameRate改了一下

        solver();
        
        for(int i=1; i<N; i++){
            sphere[i].transform.position = pos0[i];
        }
    }
    float d0=1.5f;
    float angle0=0;
    int [,]a; //用來計算 Stretch Constraint的連結關係
    int [,]b; //用來計算 Bending Constraint的連結關係
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
    dfloat CjBend(int Ci, Vector3 [] pos){ //每個Constraint都有3步驟
        dfloat []posAD=new dfloat[N*3]; //(1)宣告&初始化 dfloat 變數
        for(int pi=0; pi<N; pi++){
            posAD[pi*3+0] = new dfloat(N*3, pos[pi].x);
            posAD[pi*3+1] = new dfloat(N*3, pos[pi].y);
            posAD[pi*3+2] = new dfloat(N*3, pos[pi].z);
            posAD[pi*3+0].val(pi*3+0+1)=1;
            posAD[pi*3+1].val(pi*3+1+1)=1;
            posAD[pi*3+2].val(pi*3+2+1)=1;
        }

        //(2)照著 Constraint的公式, 寫下 dfloat 的版本
        //Vector3 v1 = pos[ b[Ci,0] ] - pos[ b[Ci,1] ];
        //Vector3 v2 = pos[ b[Ci,2] ] - pos[ b[Ci,3] ];
        //return 0.01f*(Vector3.Angle(v1,v2)-angle0);//這3行會翻譯成下面10行
        dfloat v1x = posAD[ b[Ci,0]*3+0 ] - posAD[ b[Ci,1]*3+0 ];
        dfloat v1y = posAD[ b[Ci,0]*3+1 ] - posAD[ b[Ci,1]*3+1 ];
        dfloat v1z = posAD[ b[Ci,0]*3+2 ] - posAD[ b[Ci,1]*3+2 ];
        dfloat v2x = posAD[ b[Ci,2]*3+0 ] - posAD[ b[Ci,3]*3+0 ];
        dfloat v2y = posAD[ b[Ci,2]*3+1 ] - posAD[ b[Ci,3]*3+1 ];
        dfloat v2z = posAD[ b[Ci,2]*3+2 ] - posAD[ b[Ci,3]*3+2 ];
        dfloat dot = v1x*v2x+v1y*v2y+v1z*v2z;
        dfloat len1 = dfloat.dsqrt(v1x*v1x+v1y*v1y+v1z*v1z); 
        dfloat len2 = dfloat.dsqrt(v2x*v2x+v2y*v2y+v2z*v2z); 

        return dfloat.dacos(dot/len1/len2); //(3)要return 公式結果對應的dfloat變數, 裡面附送gradient值
    }
    dfloat CjStretch(int Ci, Vector3 [] pos){
        dfloat []posAD=new dfloat[N*3]; //(1)
        for(int pi=0; pi<N; pi++){
            posAD[pi*3+0] = new dfloat(N*3, pos[pi].x);
            posAD[pi*3+1] = new dfloat(N*3, pos[pi].y);
            posAD[pi*3+2] = new dfloat(N*3, pos[pi].z);
            posAD[pi*3+0].val(pi*3+0+1)=1;
            posAD[pi*3+1].val(pi*3+1+1)=1;
            posAD[pi*3+2].val(pi*3+2+1)=1;
        }

        dfloat dx = new dfloat(N*3, 0); //(2)
        dfloat dy = new dfloat(N*3, 0);
        dfloat dz = new dfloat(N*3, 0);
        for(int Pj=0; Pj<N; Pj++){
            float w=a[Ci, Pj];
            dx += posAD[Pj*3+0]*w;
            dy += posAD[Pj*3+1]*w;
            dz += posAD[Pj*3+2]*w;
        }
        return dfloat.dsqrt(dx*dx+dy*dy+dz*dz)-d0; //(3)
    }
    dfloat calcCj(int j){ //projectConstraints()會呼叫很多次 calcCj()
        if(j<N-1) return CjStretch(j,pos);
        else return CjBend(j-(N-1), pos);
    } //統一交給 calcCj()函式, 裡面會分別計算對應的 Constraint 的 dfloat值,以便得到gradient

    void projectConstraints() //solver()會呼叫 projectConstraints()
    {
        float maxMag=0;
        float []w=new float[N];
        for(int i=0;i<N;i++) w[i]=1.0f/N;

        for(int j=0; j<(N-1)+(N-2); j++){ //Stretch (N-1) + Bending (N-2), 用來計算 constraint Cj 
            dfloat gC = calcCj(j); //input:第Cj條constraint, output: gradient[8]共24個變數

            float gradientSum=0;
            for(int Xi=1; Xi<N; Xi++){ //TODO:Xi=0要釘牆上的話,應用更通用的寫法, 而不是漏掉避開
                float dx =gC.val(Xi*3), dy=gC.val(Xi*3+1), dz=gC.val(Xi*3+2);
                gradientSum += w[Xi]* (dx*dx+dy*dy+dz*dz);
            }
            if(float.IsNaN(gradientSum)) continue;
            float lambda = gC.val(0)/gradientSum; //float lambda = C(Cj)/gradientSum;

            for(int Xi=1;Xi<N;Xi++){ //TODO:Xi=0要釘牆上的話,應用更通用的寫法, 而不是漏掉避開
                gradient[Xi].x = gC.val(Xi*3+0+1);
                gradient[Xi].y = gC.val(Xi*3+1+1);
                gradient[Xi].z = gC.val(Xi*3+2+1);
                Vector3 diff= gradient[Xi] * - lambda*w[Xi];
                pos[Xi] += diff *(1-stiffness);
                if(diff.magnitude>maxMag) maxMag=diff.magnitude;
            }
        }
    }
    int solverIterators=20; //Iteration越多,越硬的感覺
    float stiffness=0.9f; //0.0f-1.0f, 這個k值在論文 3.3 最後面有解釋 diff*(1-k)^ns, 越大越軟
    // solverIterators vs. stiffness
    //   1                     0    =>軟軟的
    //  20                     0    =>很硬
    //   2                     0.5  =>軟
    //  20                     0.9 
    void solver() //Update()會呼叫 solver
    {
        Vector3 g = new Vector3(0, -0.98f/20, 0);
        for(int i=1; i<N; i++){
            v[i] += g; //Step (5) 還沒有乘上 delta T 及重量
            pos[i] = pos0[i]+v[i]; //Step (7)
            v[i] *=0.9f; //這裡加大磨擦力,讓系統慢慢停住
        }

        for(int i=0; i<solverIterators; i++){ //Step (09) Loop
            projectConstraints(); //Step (10)
        } //Step (11) End Loop

        for(int i=1; i<N; i++){ //Step (12)
            v[i] = pos[i] - pos0[i]; //Step (13)
            pos0[i] = pos[i]; //Step (14)
        } //Step (15) 
        //Step (16) updateVelocities()
    }
}
