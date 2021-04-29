using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class step04pbd_muller2006ad : MonoBehaviour
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
    }//Bending 要關掉的話, 可以將 return 統一變成 return 0 即可
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
            for(int Xi=1;Xi<N;Xi++){//TODO: 之後要把這個迴圈,變成 calGradientAd(Cj), 它會填進 gradient[8]
                calcGradient(Cj, Xi);
            }
            //calcGradientAD(Cj);//input:第Cj條constraint output: gradient[8]共24個變數
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
/*class dfloat8
{//需要每個都加上public, 不能用 public:
    static int N=8*3; //要加 static, 後面才能用
    float [] v;// = new float[N+1]; // change this to your favorite "pretend real."
    // and all references to floats. Use a typedef or a define or whatever.
    public dfloat8(int N0){ //初始化, 都設成0
        N = N0;
        v = new float[N+1];
        for( int i=0; i<=N; i++ ) v[i]=0.0f;
    }
    public dfloat8(int N0, float s){ //初始第1個原值, 其他(微分值)都設0
        N=N0;
        v = new float[N+1];
        v[0]=s;
        for( int i=1; i<=N; i++ ) v[i]=0.0f;
    }
    public ref float val(){ //reference 參考
        return ref v[0]; //這裡有改, 因為要一致
    }
    public ref float val(int i){ //reference 參考
        return ref v[i]; //這裡有改, 因為要一致
    }
    public void val(float s){ //上面是整數index(取出),下面是float值(設定)
        v[0]=s;
    }
    public void val(int i, float s){ //設定值
        v[i]=s;
    }

    //https://stackoverflow.com/questions/599367/why-can-not-be-overloaded-in-c
    //"operator = is applied to references 原因是因為 = 用在 reference, 就簡單多了!
    //Note also that, unlike C++, the = assignment operator cannot be overloaded in C#.
    //if you overload a binary operator, such as +, += is also overloaded. 
    //所以我把整套 =, +=, -=, *=, /= 都刪掉了! 
    public static dfloat8 operator - (dfloat8 a){ //負號
        dfloat8 c=new dfloat8(N);
        for( int i=0; i<=N; i++ ) c.v[i] = -a.v[i];
        return c;
    }

    //friend(C++) 改成 public static(C#)
    public static dfloat8 operator + (dfloat8 a, dfloat8 b)
    {
        dfloat8 c = new dfloat8(N); 
        for( int i=0; i<=N; i++ ) c.v[i] = a.v[i]+b.v[i];
        return c;
    }
    public static dfloat8 operator - (dfloat8 a, dfloat8 b)
    {
        dfloat8 c = new dfloat8(N);
        for( int i=0; i<=N; i++ ) c.v[i] = a.v[i]-b.v[i];
        return c;
    }
    public static dfloat8 operator * (dfloat8 a, dfloat8 b)
    {
        dfloat8 c = new dfloat8(N);
        c.v[0] = a.v[0] * b.v[0];
        for( int i=1; i<=N; i++ ) c.v[i] = a.v[i]*b.v[0] + a.v[0]*b.v[i];
        return c;
    }
    public static dfloat8 operator / (dfloat8 a, dfloat8 b)
    {
        dfloat8 c = new dfloat8(N);
        c.v[0] = a.v[0] / b.v[0];
        float g = b.v[0]*b.v[0];
        for( int i=1; i<=N; i++ ) c.v[i] = (a.v[i]*b.v[0]-a.v[0]*b.v[i])/g;
        return c;
    }
    public static dfloat8 operator + (float s, dfloat8 a )
    {
        dfloat8 c = new dfloat8(N);
        c.v[0] = s + a.v[0];
        for( int i=1; i<=N; i++ ) c.v[i] = a.v[i];
        return c;
    }
    public static dfloat8 operator + (dfloat8 a, float s )
    {
        dfloat8 c = new dfloat8(N);
        c.v[0] = a.v[0] + s;
        for( int i=1; i<=N; i++ ) c.v[i] = a.v[i];
        return c;
    }
    public static dfloat8 operator - (float s, dfloat8 a)
    {
        dfloat8 c = new dfloat8(N);
        c.v[0] = s - a.v[0];
        for( int i=1; i<=N; i++ ) c.v[i] = -a.v[i];
        return c;
    }
    public static dfloat8 operator - (dfloat8 a, float s)
    {
        dfloat8 c = new dfloat8(N);
        c.v[0] = a.v[0] - s;
        for( int i=1; i<=N; i++ ) c.v[i] = a.v[i];
        return c;
    }
    public static dfloat8 operator * (float s, dfloat8 a)
    {
        dfloat8 c = new dfloat8(N);
        for( int i=0; i<=N; i++ ) c.v[i] = s*a.v[i];
        return c;
    }
    public static dfloat8 operator * (dfloat8 a, float s)
    {
        dfloat8 c = new dfloat8(N);
        for( int i=0; i<=N; i++ ) c.v[i] = a.v[i]*s;
        return c;
    }
    public static dfloat8 operator / (float s, dfloat8 a)
    {
        dfloat8 c = new dfloat8(N);
        c.v[0] = s/a.v[0];
        float g = a.v[0]*a.v[0];
        for( int i=1; i<=N; i++ ) c.v[i] = -s*a.v[i]/g;
        return c;
    }
    public static dfloat8 operator / (dfloat8 a, float s)
    {
        dfloat8 c = new dfloat8(N);
        for( int i=0; i<=N; i++ ) c.v[i] = a.v[i]/s;
        return c;
    }
    public static dfloat8 dsqrt (dfloat8 a)
    {
        dfloat8 c = new dfloat8(N);
        c.v[0] = Mathf.Sqrt(a.v[0]);
        for( int i=1; i<=N; i++ ) c.v[i] = 0.5f * a.v[i] / c.v[0];
        return c;
    }
    public static dfloat8 dacos(dfloat8 a)
    {
        dfloat8 c = new dfloat8(N);
        c.v[0] = (float) Mathf.Acos(a.v[0]);
        float g = -1.0f/Mathf.Sqrt(1-a.v[0]*a.v[0]);
        for( int i=1; i<=N; i++ ) c.v[i] = a.v[i] * g;
        return c;
    }
}
*/