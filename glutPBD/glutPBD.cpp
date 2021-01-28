#include <GL/glut.h> ///Stam是用 C/C++ 配 GLUT
///關於 CodeBlocks 使用 freeglut 要安裝的東西
///freglut can be download by google: freeglut windows download
///https://www.transmissionzero.co.uk/software/freeglut-devel/
///https://www.transmissionzero.co.uk/files/software/development/GLUT/freeglut-MinGW.zip
/// Unzip freeglut to C:\Users\user\Desktop\freeglut
/// copy C:\Users\User\Desktop\freeglut\lib\libfreeglut.a to
///      C:\Users\User\Desktop\freeglut\lib\glut32.a
///In CodeBlocks 17.12 (not 20.03, 20.03 will have problems)
/// File-New-Project, GLUT Project, GLUT path: C:\Users\user\Desktop\freeglut

///下面 _NO_OLDNAMES 這行, 是為了避開 math.h裡竟然也使用到的 y1
#define _NO_OLDNAMES
#include <math.h>
#include <stdio.h>
/// The codes are modified from Figure 14.5 in Chapter 14 of Stam's book (The art of fluid animation)
/// 程式修改自 Stam 書中第14章(Figure 14.5), 使用 C++ operator overloading
/// 讓 Automatic Differentiation 變得簡單易理解/易完成。
/// 但是直接把書中的程式碼打出來編譯, 會出現許多編譯錯誤 (compile errors)
/// 可能是 C++ compiler 的版本問題。所以我做了對應的修改。

/// 修改1: 有些 return 回傳的地方, 增加 & 變 reference 參考, 讓 lvalue 可被修改
/// 修改2: 有些參數 加了 const, 承諾 rvalue 不會被修改
/// 修改3: 有些地方便用 friend 讓函式可以在 class 裡被宣告

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

    x = y + z + u*u;  ///statement #1
    y = z*y + x*v;    ///statement #2

    f = x*x + y*y + u*u +v*v;  ///cost function
    return 0;
}

int toy2() ///手動完成 Automatic Differentiation 的公式, 見 Stam 書 Figure 14.4 示意圖
{
    float x, y, z, dx[2], dy[2], dz[2];
    float u, v, du[2], dv[2];
    float f, df[2];

    dx[0]=dx[1]=dy[0]=dy[1]=dz[0]=dz[1]=df[0]=df[1]=0;
    du[0]=1; du[1]=0; dv[0]=0; dv[1]=1;
    ///因為u對u微分為1, v對v微分為1 (自己對自己微分,會得到1)
    ///陣列中, [0]是對u微分的結果, [1]都是對v微分的結果

    x = y + z + u*u; ///最後使用 Stam's AD class時, dx[0] 是這行
       dx[0] = dy[0] + dz[0] + 2*u*du[0]; ///       dx[1]是這行
       dx[1] = dy[1] + dz[1] + 2*u*du[1]; ///       dx[2]是這行

    y = z*y + x*v; ///最後使用 Stam's AD class時,     dy[0] 是這行
       dy[0] = z*dy[0] + y*dz[0] + v*dx[0] + x*dv[0]; ///dy[1] 是這行
       dy[1] = z*dy[1] + y*dz[1] + v*dx[1] + x*dv[1]; ///dy[2] 是這行
    f = x*x + y*y + u*u + v*v; ///最後使用 Stam's AD class時,   df[0] 是這行
       df[0] = 2*x*dx[0] + 2*y*dy[0] + 2*u*du[0] + 2*v*dv[0]; ///df[1] 是這行
       df[1] = 2*x*dx[1] + 2*y*dy[1] + 2*u*du[1] + 2*v*dv[1]; ///df[2] 是這行
    return 0;
}

///Q: 把變數全部當成控制變數, 但變數值 (ex. x1, x2) 並沒有 solvePBD()中使用, 不合理
///A: 突然想到, 是不是要把值,放在 [0]項? 這樣值就可以帶進去了耶!!! 舉幾個例子看看
///C = x*y
/// dC[0] = dx[0]*y     + x    *dy[0]
/// dC[1] = dx[1]*y     + x    *dy[1]
/// 乘法公式
/// dC[i] = dx[i]*dy[0] + dx[0]*dy[i]
/// 結論: [0]放原值, [1]...[N] 放微分的值 這樣 [0] 計算時, 堂兄弟[1]...[N]跟著算

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
void keyboard(unsigned char key, int x, int y)
{
    solvePBD();///按keyboard時, 執行一次 solvePBD()
    glutPostRedisplay();///順手重畫畫面
}
int main(int argc, char**argv)
{
    glutInit(&argc, argv);
    glutCreateWindow("glutPDB");
    glutDisplayFunc(display);
    glutKeyboardFunc(keyboard);
    glutMainLoop();
}
