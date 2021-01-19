AD 自動微分
===========
這裡試著照著 Stam 書中第14章的內容, 實作 Automatic Differentiation C++ 版本

Automatic Differentiation (C++ implementation), Chapter 14 in Stam's book.

- ad.cpp 將 Stam 的程式, 做對應的修改, 可在 CodeBlocks 20.03 (g++ 8.1.0) compile 成功
- ad_explain.cpp 解釋我做的修改, 還有原因
- ad_old.cpp 原本 Stam 在書中的程式碼, 無法編譯

- Reference: Jos Stam, *The Art of Fluid Animation*, 2016. 

TODO
====
- 在 Stam 書裡面 Chap 14 介紹的 Automatic Differentiation (AD),能非常輕易的算出複雜函式的 gradient值, 也就是 `f[0], f[1], f[2]...` 的值
- 在 Position Based Dynamics 論文中, 可以模擬衣料(彈簧) 或流體。我們曾在 2020年3-4月,試著在 Processing 及 Unity 中, 實作出 Position Based Dynamics, 因為解 gradient 的程式太複雜, 所以沒辦法把頂點變多
- 現在如果能將 AD 拿來 Position Based Dynamics 裡面使用, 則可以 (1) 驗證 AD 的實作/理解無誤, (2) 讓 Position Based Dyanmics 變得更簡單、實作可以更快、可以解的變數更多
- Adjoint 方法, 有 2篇論文, 我們讀 Stam 的 *Notes on Adjoint Control of Graphical Simulation* 有理解它的重要精神, 也把大部分公式看懂了。它可以利用 *前面到後面, 後面到前面, 循環進行Optimizer* 來求解。不過我們還沒能力實作。

```C++
/// The codes are modified from Figure 14.5 in Chapter 14 of Stam's book (The art of fluid animation)
/// 程式修改自 Stam 書中第14章(Figure 14.5), 使用 C++ operator overloading
/// 讓 Automatic Differentiation 變得簡單易理解/易完成。
/// 但是直接把書中的程式碼打出來編譯, 會出現許多編譯錯誤 (compile errors)
/// 可能是 C++ compiler 的版本問題。所以我做了對應的修改。

/// 修改1: 有些 return 回傳的地方, 增加 & 變 reference 參考, 讓 lvalue 可被修改
/// 修改2: 有些參數 加了 const, 承諾 rvalue 不會被修改
/// 修改3: 有些地方便用 friend 讓函式可以在 class 裡被宣告

#include <stdio.h>

template <int N> class dfloat
{
public:
    float v[N+1];
    dfloat(){
        for( int i=0; i<=N; i++) v[i]=0.0f;
    }
    dfloat(float s){
        v[0]=s;
        for( int i=1; i<=N; i++) v[i]=0.0f;
    }
    float &val(){ /// 這裡增加 & 變 reference 參考, 讓 lvalue 可被修改
        return v[0];
    }
    float &val(int i){ /// 這裡增加 & 變 reference 參考, 讓 lvalue 可被修改
        return v[i];
    }
    void val(float s){
        v[0]=s;
    }
    void val(int i, float s){
        v[i]=s;
    }

    /// 這裡增加 & 變 reference 參考, 讓 lvalue 可被修改
    dfloat & operator = ( const dfloat & a ){ /// 這裡增加 const, 承諾 rvalue 不會被修改
        for(int i=0; i<=N; i++) v[i] = a.v[i];
        return *this;
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
    }
};

int toy1() ///簡化的程式, 見 Stam 書 Figure 14.3 示意圖 
{
    float x, y, z;
    float u, v;
    float f;

    x = y + z + u*u;
    y = z*y + x*v;

    f = x*x + y*y + u*u +v*v;
    return 0;
}

int toy2() ///手動完成 Automatic Differentiation 的公式, 見 Stam 書 Figure 14.4 示意圖
{
    float x, y, z, dx[2], dy[2], dz[2];
    float u, v, du[2], dv[2];
    float f, df[2];

    dx[0]=dx[1]=dy[0]=dy[1]=dz[0]=dz[1]=df[0]=df[1]=0;
    du[0]=1; du[1]=0; dv[0]=0; dv[1]=1;

    x = y + z + u*u;
       dx[0] = dy[0] + dz[0] + 2*u*du[0];
       dx[1] = dy[1] + dz[1] + 2*u*du[1];

    y = z*y + x*v;
       dy[0] = z*dy[0] + y*dz[0] + v*dx[0] + x*dv[0];
       dy[1] = z*dy[1] + y*dz[1] + v*dx[1] + x*dv[1];
    f = x*x + y*y + u*u +v*v;
       df[0] = 2*x*dx[0] + 2*y*dy[0] + 2*u*du[0] + 2*v*dv[0];
       df[1] = 2*x*dx[1] + 2*y*dy[1] + 2*u*du[1] + 2*v*dv[1];
    return 0;
}
// 在 Stam 書, 上面的 dfloat class 被放在 ad.h 裡, 方便 #include "ad.h" 來使用
int main() ///利用 ad.h 完成 Automatic Differentiation, 見 Stam 書 Figure 14.6 示意圖
{
    dfloat<2> x, y;
    dfloat<2> u, v;
    dfloat<2> f;

    u.val(0) = 1.0f; v.val(1) = 1.0f; ///這是程式的 Input, 表示想要在這裡, 算出 f 函式的 gradient

    x = y + x + u*u;
    y = x*y + x*v;
    f = x*x + y*y + u*u + v*v; ///這裡可以使用 Stam 寫的 Automatic Differentiation 簡化求解的程式

    printf("%.3f %.3f\n", x.val(0), x.val(1)); ///印出數值, 了解 AD 後的結果
    printf("%.3f %.3f\n", y.val(0), y.val(1)); ///印出數值, 了解 AD 後的結果
    printf("%.3f %.3f\n", f.val(0), f.val(1)); ///印出數值, 了解 AD 後的結果
    ///這裡就是 f 的 gradient 值, 等價於前面 toy2() 的 df[0] 及 df[1]
}
```
