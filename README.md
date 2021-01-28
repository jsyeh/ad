AD 自動微分
===========

Introduction (介紹)
-------------------

這裡試著照著 Stam 書中第14章的內容, 實作 Automatic Differentiation C++ 版本

Automatic Differentiation (C++ implementation), Chapter 14 in Stam's book.

- glutPBD 目錄, 利用 ad.cpp 的 class dfloat<6> 來解 Position Based Dynamics 問題, 並用 OpenGL/GLUT 繪製
- unityPBD 目錄, 利用 Unity/C# 改寫 ad 的 operator overloading, 成功在 step01pbd_oneSpring.cs 做出簡單 Position Based Dynamics
- ad.cpp 將 Stam 的程式, 做對應的修改, 可在 CodeBlocks 20.03 (g++ 8.1.0) 及 CodeBlocks 17.12 (g++ 5.1.0) compile 成功
- ad_explain.cpp 解釋程碼的意思、我做的修改/修改原因
- ~~ad_old.cpp 原本 Stam 在書中的程式碼, 無法編譯~~

- Reference: Jos Stam, *The Art of Fluid Animation*, 2016. 


TODO
----

- 在 Position Based Dynamics 論文中, 可以模擬衣料(彈簧) 或流體。我們曾在 2020年3-4月,試著在 Processing 及 Unity 中, 實作出 Position Based Dynamics, 因為解 gradient 的程式太複雜, 所以沒辦法把頂點變多
- 現在如果能將 AD 拿來 Position Based Dynamics 裡面使用, 則可以 (1) 驗證 AD 的實作/理解無誤(OK), (2) 讓 Position Based Dynamics 變得更簡單、實作可以更快、可以解的變數更多
- Adjoint 方法, 有 2篇論文, 我們讀 Stam 的 *Notes on Adjoint Control of Graphical Simulation* 有理解它的重要精神, 也把大部分公式看懂了。它可以利用 *前面到後面, 後面到前面, 循環進行Optimizer* 來求解。不過我們還沒能力實作。


More Detail (更多細節)
----------------------

- 在 Stam 書裡面 Chap 14 介紹的 Automatic Differentiation (AD), 能非常輕易的算出複雜函式的 gradient值, 也就是 `f[1], f[2]..., f[N]` 的值 
- 使用 controls 控制變數, cost function, cost function 的 gradient 來更新 控制變數
- 如果 controls 控制變數有 2 個, 就寫 `dfloat<2>`
- 如果 controls 控制變數有 6 個, 就寫 `dfloat<6>`
- 在 dfloat 裡面, `val[0]` 是變數的原值, `val[1]`...`val[N]` 是對 N 個變數微分的值

```C++
/// The codes are modified from Figure 14.5 in Chapter 14 of Stam's book (The art of fluid animation)
/// 程式修改自 Stam 書中第14章(Figure 14.5), 使用 C++ operator overloading
/// 讓 Automatic Differentiation 變得簡單易理解/易完成。
/// 但是直接把書中的程式碼打出來編譯, 會出現許多編譯錯誤 (compile errors)
/// 可能是 C++ compiler 的版本問題。所以我做了對應的修改。

/// 修改1: 有些函式 return 回傳增加 & 變 reference 參考, 讓 lvalue 可被修改
/// 修改2: 有些參數 加了 const, 承諾 rvalue 不會被修改
/// 修改3: 有些地方便用 friend 讓函式可以在 class 裡被宣告
/// 修改4: main() 裡面改成 u.val(1)=1; v.val(2)=1

#include <stdio.h>

template <int N> class dfloat
{///Automatic Differentiation (AD) with N controls
public:
    float v[N+1]; /// change this to your favorite "pretend real."
    /// and all references to floats. Use a typedef or a define or whatever.
    dfloat(){ ///初始化, 都設成0
        for( int i=0; i<=N; i++ ) v[i]=0.0f;
    }
    dfloat(float s){ ///初始第1個原值, 其他(微分值)都設0
        v[0]=s;
        for( int i=1; i<=N; i++ ) v[i]=0.0f;
    }
    float &val(){ /// 這裡增加 & 變 reference 參考, 讓 lvalue 可被修改
        return v[0];
    }
    float &val(int i){ /// 這裡增加 & 變 reference 參考, 讓 lvalue 可被修改
        return v[i];
    }
    void val(float s){ ///上面是整數index(取出),下面是float值(設定)
        v[0]=s;
    }
    void val(int i, float s){ ///設定值
        v[i]=s;
    }

    /// 這裡增加 & 變 reference 參考, 讓 lvalue 可被修改
    dfloat & operator = ( const dfloat & a ){ /// 這裡增加 const, 承諾 rvalue 不會被修改
        for( int i=0; i<=N; i++ ) v[i] = a.v[i];
        return *this;
    }

    dfloat & operator = ( float s ){
        v[0]=s;
        for( int i=1; i<=N; i++ ) v[i]=0.0f;
        return *this;
    }

    dfloat & operator += (const dfloat & a){ /// 這裡增加 const, 承諾 rvalue 不會被修改
        for( int i=0; i<=N; i++ ) v[i] += a.v[i];
        return *this;
    }
    dfloat & operator -= (const dfloat & a){ /// 這裡增加 const, 承諾 rvalue 不會被修改
        for( int i=0; i<=N; i++ ) v[i] -= a.v[i];
        return *this;
    }
    dfloat & operator *= (const dfloat & a){ /// 這裡增加 const, 承諾 rvalue 不會被修改
        for( int i=1; i<=N; i++ ) v[i] = v[i]*a.v[0]+ v[0]*a.v[i];
        v[0] *= a.v[0];
        return *this;
    }
    dfloat & operator /= (const dfloat & a){ /// 這裡增加 const, 承諾 rvalue 不會被修改
        float g = a.v[0]*a.v[0];
        for( int i=1; i<=N; i++ ) v[i] = (v[i]*a.v[0]-v[0]*a.v[i])/g;
        v[0] /= a.v[0];
        return *this;
    }
    dfloat & operator += (float s){
        v[0] += s;
        return *this;
    }
    dfloat & operator -= (float s){
        v[0] -= s;
        return *this;
    }
    dfloat & operator *= (float s){
        for( int i=0; i<=N; i++ ) v[i] *= s;
        return *this;
    }
    dfloat & operator /= (float s){
        for( int i=0; i<=N; i++ ) v[i] /=s;
        return *this;
    }
    dfloat operator - (void){
        dfloat c;
        for( int i=0; i<=N; i++ ) c.v[i] = -v[i];
        return c;
    }

    ///https://stackoverflow.com/questions/4622330/operator-overloading-member-function-vs-non-member-function
    /// 以下增加 friend, 讓 operator overloading 可以輸入 (左,右) 2個參數
    friend dfloat operator + (const dfloat &a, const dfloat &b){ /// 這裡增加 const, 承諾 rvalue 不會被修改
        dfloat c; ///https://stackoverflow.com/questions/13544364/over-loading-operator-must-take-either-zero-or-one-arguments
        for( int i=0; i<=N; i++ ) c.v[i] = a.v[i]+b.v[i];
        return c;
    }
    friend dfloat operator - (const dfloat &a, const dfloat &b){ /// 這裡增加 const, 承諾 rvalue 不會被修改
        dfloat c;
        for( int i=0; i<=N; i++ ) c.v[i] = a.v[i]-b.v[i];
        return c;
    }
    friend dfloat operator * (const dfloat &a, const dfloat &b){ /// 這裡增加 const, 承諾 rvalue 不會被修改
        dfloat c;
        c.v[0] = a.v[0] * b.v[0];
        for( int i=1; i<=N; i++ ) c.v[i] = a.v[i]*b.v[0] + a.v[0]*b.v[i];
        return c;
    }
    friend dfloat operator / (const dfloat &a, const dfloat &b){ /// 這裡增加 const, 承諾 rvalue 不會被修改
        dfloat c;
        c.v[0] = a.v[0] / b.v[0];
        float g = b.v[0]*b.v[0];
        for( int i=1; i<=N; i++ ) c.v[i] = (a.v[i]*b.v[0]-a.v[0]*b.v[i])/g;
        return c;
    }

    friend dfloat operator + (float s, const dfloat &a ){ /// 這裡增加 const, 承諾 rvalue 不會被修改
        dfloat c;
        c.v[0] = s + a.v[0];
        for( int i=1; i<=N; i++ ) c.v[i] = a.v[i];
        return c;
    }
    friend dfloat operator + (const dfloat &a, float s ){ /// 這裡增加 const, 承諾 rvalue 不會被修改
        dfloat c;
        c.v[0] = a.v[0] + s;
        for( int i=1; i<=N; i++ ) c.v[i] = a.v[i];
        return c;
    }
    friend dfloat operator - (float s, const dfloat &a){ /// 這裡增加 const, 承諾 rvalue 不會被修改
        dfloat c;
        c.v[0] = s - a.v[0];
        for( int i=1; i<=N; i++ ) c.v[i] = -a.v[i];
        return c;
    }
    friend dfloat operator - (const dfloat &a, float s){ /// 這裡增加 const, 承諾 rvalue 不會被修改
        dfloat c;
        c.v[0] = a.v[0] - s;
        for( int i=1; i<=N; i++ ) c.v[i] = a.v[i];
        return c;
    }
    friend dfloat operator * (float s, const dfloat &a){ /// 這裡增加 const, 承諾 rvalue 不會被修改
        dfloat c;
        for( int i=0; i<=N; i++ ) c.v[i] = s*a.v[i];
        return c;
    }
    friend dfloat operator * (const dfloat &a, float s){ /// 這裡增加 const, 承諾 rvalue 不會被修改
        dfloat c;
        for( int i=0; i<=N; i++ ) c.v[i] = a.v[i]*s;
        return c;
    }
    friend dfloat operator / (float s, const dfloat &a){ /// 這裡增加 const, 承諾 rvalue 不會被修改
        dfloat c;
        c.v[0] = s/a.v[0];
        float g = a.v[0]*a.v[0];
        for( int i=1; i<=N; i++ ) c.v[i] = -s*a.v[i]/g;
        return c;
    }
    friend dfloat operator / (const dfloat &a, float s){ /// 這裡增加 const, 承諾 rvalue 不會被修改
        dfloat c;
        for( int i=0; i<=N; i++ ) c.v[i] = a.v[i]/s;
        return c;
    }
    friend dfloat dsqrt (const dfloat & a){ /// 這裡增加 const, 承諾 rvalue 不會被修改
        dfloat c;
        c.v[0] = sqrtf(a.v[0]);
        for( int i=1; i<=N; i++ ) c.v[i] = 0.5f * a.v[i] / c.v[0];
        return c;
    }
    friend dfloat dacos(const dfloat & a){ /// 這裡增加 const, 承諾 rvalue 不會被修改
        dfloat c;
        c.v[0] = (float) acos(a.v[0]);
        float g = -1.0f/sqrtf(1-a.v[0]*a.v[0]);
        for( int i=1; i<=N; i++ ) c.v[i] = a.v[i] * g;
        return c;
    }
};

int toy1() ///簡化的程式, 見 Stam 書 Figure 14.3 示意圖
{
    float x, y, z; ///variables
    float u, v;    ///two controls
    float f;       ///cost variable

    x = y + z + u*u; ///statement #1
    y = z*y + x*v;   ///statement #2

    f = x*x + y*y + u*u +v*v; ///cost function
    return 0;
}

int toy2() ///手動完成 Automatic Differentiation 的公式, 見 Stam 書 Figure 14.4 示意圖
{
    float x, y, z, dx[2], dy[2], dz[2]; ///variables
    float u, v, du[2], dv[2];           ///two controls
    float f, df[2];                     ///cost variable

    dx[0]=dx[1]=dy[0]=dy[1]=dz[0]=dz[1]=df[0]=df[1]=0;
    du[0]=1; du[1]=0; dv[0]=0; dv[1]=1;
    ///因為u對u微分為1, v對v微分為1 (自己對自己微分,會得到1)
    ///陣列中, [0]是對u微分的結果, [1]都是對v微分的結果

    x = y + z + u*u; ///最後使用 Stam's AD class時, dx[0] 是這行
       dx[0] = dy[0] + dz[0] + 2*u*du[0]; ///       dx[1] 是這行
       dx[1] = dy[1] + dz[1] + 2*u*du[1]; ///       dx[2] 是這行

    y = z*y + x*v; ///最後使用 Stam's AD class 時,   dy[0] 是這行
       dy[0] = z*dy[0] + y*dz[0] + v*dx[0] + x*dv[0]; ///dy[1] 是這行
       dy[1] = z*dy[1] + y*dz[1] + v*dx[1] + x*dv[1]; ///dy[2] 是這行
    f = x*x + y*y + u*u + v*v; ///最後使用 Stam's AD class時,   df[0] 是這行
       df[0] = 2*x*dx[0] + 2*y*dy[0] + 2*u*du[0] + 2*v*dv[0]; ///df[1] 是這行
       df[1] = 2*x*dx[1] + 2*y*dy[1] + 2*u*du[1] + 2*v*dv[1]; ///df[2] 是這行
    return 0;
}

// 在 Stam 書, 上面的 dfloat class 被放在 ad.h 裡, 方便 #include "ad.h" 來使用
int main() ///利用 ad.h 完成 Automatic Differentiation, 見 Stam 書 Figure 14.6 示意圖
{
    dfloat<2> x, y; /// variables
    dfloat<2> u, v; /// controls
    dfloat<2> f;    /// cost variable

    u.val(1) = 1.0f; v.val(2) = 1.0f; ///這是程式的 Input, 表示想要在這裡, 算出 f 函式的 gradient
	///這裡 Stam 書中有錯, 因為 val(0) 應是值本身, val(1)是對u微分, val(1)是對v微分
	///其中 u.val(1) = 1 表示 u 對 u 微分, 是1
	//      v.val(2) = 1 表示 v 對 v 微分, 是1

    x = y + x + u*u; /// statement #1
    y = x*y + x*v;   /// statement #2
    f = x*x + y*y + u*u + v*v; /// cost function
	///這裡可以使用 Stam 寫的 Automatic Differentiation 簡化求解的程式

    printf("%.3f %.3f .3f\n", x.val(0), x.val(1), x.val(2) ); ///印出數值, 了解 AD 後的結果
    printf("%.3f %.3f .3f\n", y.val(0), y.val(1), y.val(2) ); ///印出數值, 了解 AD 後的結果
    printf("%.3f %.3f .3f\n", f.val(0), f.val(1), f.val(2) ); ///印出數值, 了解 AD 後的結果
    ///這裡就是 f 值, f gradient 值, 等價於前面 toy2() 的 f, df[0], df[1]
}
```
