用 AD 實作 PBD (OpenGL)
======================
這裡使用 OpenGL/GLUT 的3D繪圖環境, 
嘗試實作 Position Based Dynamic 論文裡面最簡單的單一彈簧 (one stretch spring)。
其中 gradient 計算方式使用 Stam 書中第14章的 Automatic Differentiation C++ 版本。

這個程式的目的, 是要確認我們真的了解 Stam 書中的 AD 程式碼的意思。
經過多次審視, 確認我們真的了解了。

- Reference: Matthias Müller, et. al, [Position Based Dynamics](https://matthias-research.github.io/pages/publications/posBasedDyn.pdf), *VRIPHYS*, (2006) 
- Reference: Jos Stam, *The Art of Fluid Animation*, 2016. 

OpenGL/GLUT環境
---------------

這裡的 3D 繪圖環境使用 OpenGL/GLUT, 必須要下載 freeglut。
`google: freeglut windows download` 可以找到下載連結。

- https://www.transmissionzero.co.uk/software/freeglut-devel/
- https://www.transmissionzero.co.uk/files/software/development/GLUT/freeglut-MinGW.zip

只要解壓縮 freeglut 目錄即可。我放在 C:\Users\User\Desktop\freeglut 目錄中。

使用 Windows 舊版的 CodeBlocks 17.12 (GNU C++ Compiler)來編譯。 
因為 CodeBlocks 的 GLUT Project 精靈需要有 `C:\Users\User\Desktop\freeglut\lib\libglut32.a`
所以要手動先將 `libfreeglut.a` 複製成 `libglut32.a`。
GLUT 專案可以在 CodeBlocks 裡 `File`-`New`-`Project` 選 `GLUT` 專案。

```C++
#include <GL/glut.h> ///Stam 是用 C/C++ 配 GLUT
///關於 CodeBlocks 使用 freeglut 要安裝的東西
///freglut can be download by google: freeglut windows download
///https://www.transmissionzero.co.uk/software/freeglut-devel/
///https://www.transmissionzero.co.uk/files/software/development/GLUT/freeglut-MinGW.zip
/// Unzip freeglut to C:\Users\user\Desktop\freeglut
/// copy C:\Users\User\Desktop\freeglut\lib\libfreeglut.a to
///      C:\Users\User\Desktop\freeglut\lib\glut32.a
///In CodeBlocks 17.12 (not 20.03, 20.03 will have problems)
/// File-New-Project, GLUT Project, GLUT path: C:\Users\user\Desktop\freeglut

```

關於 OpenGL/GLUT 的程式, 有 4個部分: 變數宣告、`display()`函式、 `keyboard()`函式、`main()`函式, 

變數宣告部分, 有 2 組座標(3D點), 所以宣告下面 6 個 global 變數來繪製。
 
```C++
float x1, y1, z1;
float x2, y2, z2;
```

在 `display()` 函式裡, 依照座標來畫。先畫2點間的連線, 再畫 2個小圓球。

```C++
void display()
{
    glClear(GL_COLOR_BUFFER_BIT|GL_DEPTH_BUFFER_BIT);
    glBegin(GL_LINES); ///畫2點間的連線
        glVertex3f(x1,y1,z1);
        glVertex3f(x2,y2,z2);
    glEnd();
    glPushMatrix(); ///畫第1個球
        glTranslatef(x1,y1,z1);
        glutSolidSphere(0.05, 30,30);
    glPopMatrix();
    glPushMatrix(); ///畫第2個球
        glTranslatef(x2,y2,z2);
        glutSolidSphere(0.05, 30,30);
    glPopMatrix();
    glutSwapBuffers();
}
```

在 `keyboard()` 函式裡, 只要按任意鍵, 就會呼叫後面會介紹的 `solvePBD()` 函式, 
並馬上呼叫 `glutPostRedisplay()` 來重繪畫面。

```C++
void keyboard(unsigned char key, int x, int y)
{
    solvePBD();///按keyboard時, 執行一次 solvePBD()
    glutPostRedisplay();///順手重畫畫面
}
```

在 `main()` 函式裡, 設定好 GLUT, 並註冊 `display()` 函式 及 `keyboard()` 函式。

```C++
int main(int argc, char**argv)
{
    glutInit(&argc, argv);
    glutCreateWindow("glutPDB");
    glutDisplayFunc(display);
    glutKeyboardFunc(keyboard);
    glutMainLoop();
}
```

關於 AD 程式 及 PBD 程式
-----------------------

在 Position Based Dynamics 程式中, 會需要使用 Automatic Differentiation (AD) 來計算 cost function 的 gradient 值。
這裡使用 Stam 書中第14章的 AD 程式, 也就是我們在這個程式想要驗證的部分。

程式中使用 C++ `template<int N>` 的方法, 建立一個 `class dfloat`, 並利用 operator overloading 的技巧, 來讓之後求解 AD gradient 的程式可以簡單。

在使用時, 如下方所示, 如果有 6 個控制變數(control variables), 就再宣告 6組微分用的變數。
舉例子, 如果有個控制變數 `float x1=-0.5`, 那要宣告 `dfloat<6> dx1(x1)`, 它可以同時把 `x1` 的值設定成 `dx1` 的 `.val(0)`。
所以 來宣告對應的整組微分用的變數。

因為 `class dfloat` 已經做好 operator overloading, 
所以直接寫運算的程式, 計算 cost function `C`的值時,  就可以順便算出 `C` 對應的 gradient值。

如下方 `solvePBD()` 函式, 列出 Müller 論文中 Figure 2 對應的公式, 就能算出 gradient, 並照公式(5)來調整座標的位置。

- https://matthias-research.github.io/pages/publications/posBasedDyn.pdf

```C++
float x1=-0.5, y1=0, z1=0;
float x2=+0.5, y2=0, z2=0; ///總共有6個控制變數, 下面 dfloat<6>會存「對6個控制變數」微分的值
void solvePBD()
{
///TODO: 細讀 Stam's The Art of Fluid Dynamics 第14章, 了解 AD 的細節/實作意義
///讀Position Based Dynamics論文, 了解怎麼用 gradient 來改座標
///https://matthias-research.github.io/pages/publications/posBasedDyn.pdf
    ///float C = abs(v1-v2) - 原始的長度
    float len0=0.5; ///原始長度為 0.5
    float x=x1-x2, y=y1-y2, z=z1-z2; ///原始的公式中 用來算 cost function 的輔助變數
    float C = sqrt(x*x+y*y+z*z) - len0; ///原始的cost function

    ///細讀 Stam 第14章, 了解 (1) 控制變數 x1,y1,z1, x2,y2,z2 有6個, 所以 dfloat<6>
    ///(2) 有些項要設1, 因為 「自己對自己微分」會得到1
    dfloat<6> dx1(x1), dy1(y1), dz1(z1); ///初始原值, 其他裡面都會是0
    dfloat<6> dx2(x2), dy2(y2), dz2(z2); ///初始原值, 其他裡面都會是0
    dx1.val(1)=1; ///[1]項 是對 變數1 微分
    dy1.val(2)=1; ///[2]項 是對 變數2 微分
    dz1.val(3)=1; ///[3]項 是對 變數3 微分
    dx2.val(4)=1; ///[4]項 是對 變數4 微分
    dy2.val(5)=1; ///[5]項 是對 變數5 微分
    dz2.val(6)=1; ///[6]項 是對 變數6 微分
    ///沒設定的,都會是0

    ///這裡的 dx 是前面 x=x1-x2 去做 dfloat 計算的意思
    dfloat<6> dx=dx1-dx2, dy=dy1-dy2, dz=dz1-dz2; ///AD公式, 用來算 cost function 的輔助變數
    dfloat<6> gC = dsqrt(dx*dx+dy*dy+dz*dz) - len0; ///AD公式, cost function, gC[1]..gC[6]是gradient

    float len2=0; ///posBasedDyn.pdf 的公式(5)
    for(int i=1; i<=6; i++){
        printf(" %.3f", gC.val(i) );
        len2 += gC.val(i)*gC.val(i); ///要算出分母 (gradient的長度平方)
    }
    x1+=(-C/len2)*gC.val(1); ///posBasedDyn.pdf 的公式(5) 算出 delta P 回去改 P
    y1+=(-C/len2)*gC.val(2);
    z1+=(-C/len2)*gC.val(3);
    x2+=(-C/len2)*gC.val(4);
    y2+=(-C/len2)*gC.val(5);
    z2+=(-C/len2)*gC.val(6);
    printf(" (%.3f %.3f %.3f), (%.3f %.3f %.3f)\n", x1,y1,z1, x2,y2,z2);
    ///長度 最後真的能保持在彈簧的 原始長度
}
```

下面則是整套 `class dfloat` 的宣告。

```C++
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
        dfloat c; 
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
```
