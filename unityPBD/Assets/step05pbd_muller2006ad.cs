using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class step05pbd_muller2006ad : MonoBehaviour
{
    const int N=8;
    GameObject [] sphere=new GameObject[N];
    Vector3 [] gradient = new Vector3[N];//之後會由 adpos 的項,算出對應的 gradient
    Vector3 [] v =new Vector3[N];
    Vector3 [] pos = new Vector3[N];//論文中的p
    Vector3 [] pos0 = new Vector3[N];//論文中的x
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
    }//Bending 要關掉的話, 可以將 return 統一變成 return 0 即可
    dfloat Cbendi_AD(int Ci, Vector3 [] pos){
        dfloat []adpos=new dfloat[N*3];
        for(int pi=0; pi<N; pi++){
            adpos[pi*3+0] = new dfloat(N*3, pos[pi].x);
            adpos[pi*3+1] = new dfloat(N*3, pos[pi].y);
            adpos[pi*3+2] = new dfloat(N*3, pos[pi].z);
            adpos[pi*3+0].val(pi*3+0+1)=1;
            adpos[pi*3+1].val(pi*3+1+1)=1;
            adpos[pi*3+2].val(pi*3+2+1)=1;
        }
        //Vector3 v1 = pos[ b[Ci,0] ] - pos[ b[Ci,1] ];
        //Vector3 v2 = pos[ b[Ci,2] ] - pos[ b[Ci,3] ];
        //return 0.01f*(Vector3.Angle(v1,v2)-angle0);
        dfloat v1x = adpos[ b[Ci,0]*3+0 ] - adpos[ b[Ci,1]*3+0 ];
        dfloat v1y = adpos[ b[Ci,0]*3+1 ] - adpos[ b[Ci,1]*3+1 ];
        dfloat v1z = adpos[ b[Ci,0]*3+2 ] - adpos[ b[Ci,1]*3+2 ];
        dfloat v2x = adpos[ b[Ci,2]*3+0 ] - adpos[ b[Ci,3]*3+0 ];
        dfloat v2y = adpos[ b[Ci,2]*3+1 ] - adpos[ b[Ci,3]*3+1 ];
        dfloat v2z = adpos[ b[Ci,2]*3+2 ] - adpos[ b[Ci,3]*3+2 ];
        dfloat dot = v1x*v2x+v1y*v2y+v1z*v2z;
        dfloat len1 = dfloat.dsqrt(v1x*v1x+v1y*v1y+v1z*v1z); 
        dfloat len2 = dfloat.dsqrt(v2x*v2x+v2y*v2y+v2z*v2z); 
        return dfloat.dacos(dot/len1/len2);
    }
    dfloat Ci_AD(int Ci, Vector3 [] pos){
        dfloat []adpos=new dfloat[N*3];
        for(int pi=0; pi<N; pi++){
            adpos[pi*3+0] = new dfloat(N*3, pos[pi].x);
            adpos[pi*3+1] = new dfloat(N*3, pos[pi].y);
            adpos[pi*3+2] = new dfloat(N*3, pos[pi].z);
            adpos[pi*3+0].val(pi*3+0+1)=1;
            adpos[pi*3+1].val(pi*3+1+1)=1;
            adpos[pi*3+2].val(pi*3+2+1)=1;
        }
        dfloat ansX = new dfloat(N*3, 0);
        dfloat ansY = new dfloat(N*3, 0);
        dfloat ansZ = new dfloat(N*3, 0);
        for(int Pj=0; Pj<N; Pj++){
            float w=a[Ci, Pj];
            ansX += adpos[Pj*3+0]*w;
            ansY += adpos[Pj*3+1]*w;
            ansZ += adpos[Pj*3+2]*w;
        }
        return dfloat.dsqrt(ansX*ansX+ansY*ansY+ansZ*ansZ)-d0;
    }
    dfloat calcGradientAD(int Cj){
        // dfloat C_AD(int Cj){
        //     if(Cj<N-1) return Ci_AD(Cj,adpos);
        //     else return Cbendi_AD(Cj-(N-1), adpos);
        // }//我們在算 C()的同時, gradient也同時算出來
        if(Cj<N-1) return Ci_AD(Cj,pos);
        else {
            dfloat ans=new dfloat(24, 0);
            return Cbendi_AD(Cj-(N-1), pos);
        }
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
        //for(int Cj=0; Cj<(N-1); Cj++){//先只試stretch
            //partial gradient C[?]
//            for(int Xi=1;Xi<N;Xi++){//TODO: 之後要把這個迴圈,變成 calGradientAd(Cj), 它會填進 gradient[8]
//                //calcGradient(Cj, Xi);
//            }
            dfloat ADgC = calcGradientAD(Cj);//input:第Cj條constraint output: gradient[8]共24個變數
            float gradientSum=0;
            for(int Xi=1; Xi<N; Xi++){//可能有個小 bug 關於 Xi=0 的最左邊的點,為什麼不動? 可設成 限制條件啊!
                float dx =ADgC.val(Xi*3), dy=ADgC.val(Xi*3+1), dz=ADgC.val(Xi*3+2);
                gradientSum += w[Xi]* (dx*dx+dy*dy+dz*dz);
            }
            // for(int Xi=1;Xi<N;Xi++){
            //     gradientSum += w[Xi]*gradient[Xi].sqrMagnitude;
            // }
            //print("gradientSum:"+gradientSum);
            if(float.IsNaN(gradientSum)) continue;
            if(gradientSum<0.0000001){
                print("gradient sum very small");
                continue;
            }
            //float lambda = C(Cj)/gradientSum;
            float lambda = ADgC.val(0)/gradientSum;
            print("lambda:"+lambda);
            for(int Xi=1;Xi<N;Xi++){  //更新, 怪怪的
                gradient[Xi].x = ADgC.val(Xi*3+0+1);
                gradient[Xi].y = ADgC.val(Xi*3+1+1);
                gradient[Xi].z = ADgC.val(Xi*3+2+1);
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
/*    void solvePBD()//這個函式,要修改成 calcGradientAd(Cj) 使用 dfloat CjAD() 來計算 Cj 及 每個 AD 的值
    {
        float len0=5.5f; ///原始長度為 0.5
        float x=x1-x2, y=y1-y2, z=z1-z2; ///原始的公式中 用來算 cost function 的輔助變數
        float C = Mathf.Sqrt(x*x+y*y+z*z) - len0; ///原始的cost function

        //以下已利用 Automatic Differentiation 技巧, 配合 operator overloading 完成
        ///細讀 Stam 第14章, 了解 (1) 控制變數 x1,y1,z1, x2,y2,z2 有6個, 所以 dfloat<6>
        ///(2) 有些項要設1, 因為 「自己對自己微分」會得到1
        dfloat dx1 = new dfloat(24, x1); //為了不同的 dfloat.N, 所以我們建構子要指明N的值是6,方便 多spring
        dfloat dy1 = new dfloat(24, y1);
        dfloat dz1 = new dfloat(24, z1); ///初始原值, 其他裡面都會是0
        dfloat dx2 = new dfloat(24, x2);
        dfloat dy2 = new dfloat(24, y2);
        dfloat dz2 = new dfloat(24, z2); ///初始原值, 其他裡面都會是0
        dx1.val(1)=1; ///[1]項 是對 變數1 微分
        dy1.val(2)=1; ///[2]項 是對 變數2 微分
        dz1.val(3)=1; ///[3]項 是對 變數3 微分
        dx2.val(4)=1; ///[4]項 是對 變數4 微分
        dy2.val(5)=1; ///[5]項 是對 變數5 微分
        dz2.val(6)=1; ///[6]項 是對 變數6 微分
        ///沒設定的,都會是0

        ///這裡的 dx 是前面 x=x1-x2 去做 dfloat 計算的意思
        dfloat dx=dx1-dx2, dy=dy1-dy2, dz=dz1-dz2; ///AD公式, 用來算 cost function 的輔助變數
        dfloat gC = dfloat.dsqrt(dx*dx+dy*dy+dz*dz) - len0; ///AD公式, cost function, gC[1]..gC[6]是gradient

        float len2=0; ///posBasedDyn.pdf 的公式(5)
        for(int i=1; i<=6; i++){
            len2 += gC.val(i)*gC.val(i); ///要算出分母 (gradient的長度平方)
        }
        x1+=(-C/len2)*gC.val(1); ///posBasedDyn.pdf 的公式(5) 算出 delta P 回去改 P
        y1+=(-C/len2)*gC.val(2);
        z1+=(-C/len2)*gC.val(3);
        x2+=(-C/len2)*gC.val(4);
        y2+=(-C/len2)*gC.val(5);
        z2+=(-C/len2)*gC.val(6);

    }*/

}
