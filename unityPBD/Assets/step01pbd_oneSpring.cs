using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class step01pbd_oneSpring : MonoBehaviour
{
    public GameObject ball01, ball02;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 v1 = ball01.transform.position;
        Vector3 v2 = ball02.transform.position;
        Vector3 dp = v1-v2;
        float C = dp.magnitude - 1;
        //之後要改寫樓上這行, 會變成 AD 版本
    }
}
//google: C# temlate 找到 C++ Templates vs. C# Generics 的差別
//https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/generics/differences-between-cpp-templates-and-csharp-generics
//"C# does not attempt to provide all of the functionality that C++ templates provide. "
//"C# does not allow non-type template parameters, such as template C<int i> {}."
//所以我們就不能用 Stam's AD 了

class dfloat
{//TODO: C# 需要每個都加上public, 不能用 public:
public
    const int N=2;//TODO: 改成寫死, 不要用 template<int N>
    //float v[N+1];
    float []v=new float[N+1];
    //TODO: 改變陣列的寫法
    dfloat(){
        for( int i=0; i<=N; i++) v[i]=0.0f;
    }
    dfloat(float s){
        v[0]=s;
        for( int i=1; i<=N; i++) v[i]=0.0f;
    }
/*    ref float val(){ /// 這裡增加 & 變 reference 參考, 讓 lvalue 可被修改
        return v[0];
    }
    ref float val(int i){ /// 這裡增加 & 變 reference 參考, 讓 lvalue 可被修改
        return v[i];
    }
    void val(float s){
        v[0]=s;
    }
    void val(int i, float s){
        v[i]=s;
    }
    //TODO: & 要改用 ref
    /// 這裡增加 & 變 reference 參考, 讓 lvalue 可被修改
    ref dfloat  operator = ( ref dfloat a ){ /// 這裡增加 const, 承諾 rvalue 不會被修改
        for(int i=0; i<=N; i++) v[i] = a.v[i];
        return *this;//Q: operator還不會改, Q: const還不會改, 是否刪掉就好?
    }

    /// 這裡增加 & 變 reference 參考, 讓 lvalue 可被修改
    dfloat & operator = ( float s ){
        v[0]=s;
        for(int i=1; i<=N; i++) v[i]=0.0f;
        return *this;
    }

    /// 這裡增加 & 變 reference 參考, 讓 lvalue 可被修改
    dfloat & operator += (const dfloat & a){ /// 這裡增加 const, 承諾 rvalue 不會被修改
        for(int i=0; i<=N; i++) v[i] += a.v[i];
        return *this;
    }
    /// 這裡增加 & 變 reference 參考, 讓 lvalue 可被修改
    dfloat & operator -= (const dfloat & a){ /// 這裡增加 const, 承諾 rvalue 不會被修改
        for(int i=0; i<=N; i++) v[i] -= a.v[i];
        return *this;
    }
    /// 這裡增加 & 變 reference 參考, 讓 lvalue 可被修改
    dfloat & operator *= (const dfloat & a){ /// 這裡增加 const, 承諾 rvalue 不會被修改
        for(int i=1; i<=N; i++) v[i] = v[i]*a.v[0]+ v[0]*a.v[i];
        v[0] *= a.v[0];
        return *this;
    }
    /// 這裡增加 & 變 reference 參考, 讓 lvalue 可被修改
    dfloat & operator /= (const dfloat & a){ /// 這裡增加 const, 承諾 rvalue 不會被修改
        float g = a.v[0]*a.v[0];
        for( int i=1; i<=N; i++) v[i] = (v[i]*a.v[0]-v[0]*a.v[i])/g;
        v[0] /= a.v[0];
        return *this;
    }
    /// 這裡增加 & 變 reference 參考, 讓 lvalue 可被修改
    dfloat & operator += (float s){
        v[0] += s;
        return *this;
    }
    /// 這裡增加 & 變 reference 參考, 讓 lvalue 可被修改
    dfloat & operator -= (float s){
        v[0] -= s;
        return *this;
    }
    /// 這裡增加 & 變 reference 參考, 讓 lvalue 可被修改
    dfloat & operator *= (float s){
        for(int i=0; i<=N; i++)v[i] *= s;
        return *this;
    }
    /// 這裡增加 & 變 reference 參考, 讓 lvalue 可被修改
    dfloat & operator /= (float s){
        for(int i=0; i<=N; i++) v[i] /=s;
        return *this;
    }
    dfloat operator - (void){
        dfloat c;
        for( int i=0; i<=N; i++) c.v[i] = -v[i];
        return c;
    }
    /// 以下增加 friend, 讓 operator overloading 可以輸入 (左,右) 2個參數
    friend dfloat operator + (const dfloat &a, const dfloat &b){ /// 這裡增加 const, 承諾 rvalue 不會被修改
        dfloat c; ///https://stackoverflow.com/questions/13544364/over-loading-operator-must-take-either-zero-or-one-arguments
        for(int i=0; i<=N; i++) c.v[i] = a.v[i]+b.v[i];
        return c;
    }
    friend dfloat operator - (const dfloat &a, const dfloat &b){ /// 這裡增加 const, 承諾 rvalue 不會被修改
        dfloat c;
        for(int i=0; i<=N; i++) c.v[i] = a.v[i]-b.v[i];
        return c;
    }
    friend dfloat operator * (const dfloat &a, const dfloat &b){ /// 這裡增加 const, 承諾 rvalue 不會被修改
        dfloat c;
        c.v[0] = a.v[0]* b.v[0];
        for(int i=1; i<=N; i++) c.v[i] = a.v[i]*b.v[0] + a.v[0]*b.v[i];
        return c;
    }
    friend dfloat operator / (const dfloat &a, const dfloat &b){ /// 這裡增加 const, 承諾 rvalue 不會被修改
        dfloat c;
        c.v[0] = a.v[0]/b.v[0];
        float g = b.v[0]*b.v[0];
        for(int i=1; i<=N; i++) c.v[i] = (a.v[i]*b.v[0]-a.v[0]*b.v[i])/g;
        return c;
    }
    ///https://stackoverflow.com/questions/4622330/operator-overloading-member-function-vs-non-member-function
    friend dfloat operator + (float s, const dfloat &a ){ /// 這裡增加 const, 承諾 rvalue 不會被修改
        dfloat c;
        c.v[0] = s+a.v[0];
        for(int i=1; i<=N; i++) c.v[i] = a.v[i];
        return c;
    }
    friend dfloat operator + (const dfloat &a, float s ){ /// 這裡增加 const, 承諾 rvalue 不會被修改
        dfloat c;
        c.v[0] = a.v[0]+s;
        for(int i=1; i<=N; i++) c.v[i] = a.v[i];
        return c;
    }
    friend dfloat operator - (float s, const dfloat &a){ /// 這裡增加 const, 承諾 rvalue 不會被修改
        dfloat c;
        c.v[0] = s-a.v[0];
        for(int i=1; i<=N; i++) c.v[i] = -a.v[i];
        return c;
    }
    friend dfloat operator - (const dfloat &a, float s){ /// 這裡增加 const, 承諾 rvalue 不會被修改
        dfloat c;
        c.v[0] = a.v[0]-s;
        for(int i=1; i<=N; i++) c.v[i] = a.v[i];
        return c;
    }
    friend dfloat operator * (float s, const dfloat &a){ /// 這裡增加 const, 承諾 rvalue 不會被修改
        dfloat c;
        for(int i=0; i<=N; i++) c.v[i] = s*a.v[i];
        return c;
    }
    friend dfloat operator * (const dfloat &a, float s){ /// 這裡增加 const, 承諾 rvalue 不會被修改
        dfloat c;
        for(int i=0; i<=N; i++) c.v[i] = a.v[i]*s;
        return c;
    }
    friend dfloat operator / (float s, const dfloat &a){ /// 這裡增加 const, 承諾 rvalue 不會被修改
        dfloat c;
        c.v[0] = s/a.v[0];
        float g = a.v[0]*a.v[0];
        for(int i=1; i<=N; i++)c.v[i] = -s*a.v[i]/g;
        return c;
    }
    friend dfloat operator / (const dfloat &a, float s){ /// 這裡增加 const, 承諾 rvalue 不會被修改
        dfloat c;
        c.v[0] = s/a.v[0];
        float g = a.v[0]*a.v[0];
        for(int i=1; i<=N; i++)c.v[i] = -s*a.v[i]/g;
        return c;
    }
    friend dfloat dsqrt (const dfloat & a){ /// 這裡增加 const, 承諾 rvalue 不會被修改
        dfloat c;
        c.v[0] = sqrtf(a.v[0]);
        for(int i=1; i<=N; i++) c.v[i] = 0.5f*a.v[i]/c.v[0];
        return c;
    }
    dfloat dacos(const dfloat & a){ /// 這裡增加 const, 承諾 rvalue 不會被修改
        dfloat c;
        c.v[0] = (float) acos(a.v[0]);
        float g = -1.0f/sqrtf(1-a.v[0]*a.v[0]);
        for( int i=1; i<=N; i++) c.v[i] = a.v[i]*g;
        return c;
    }*/
};