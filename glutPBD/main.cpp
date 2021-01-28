#define _NO_OLDNAMES
#include <GL/glut.h>
///freglut can be download by google: freeglut windows download
///https://www.transmissionzero.co.uk/software/freeglut-devel/
///https://www.transmissionzero.co.uk/files/software/development/GLUT/freeglut-MinGW.zip
/// Unzip freeglut to C:\Users\user\Desktop\freeglut
/// copy C:\Users\User\Desktop\freeglut\lib\libfreeglut.a to
///      C:\Users\User\Desktop\freeglut\lib\glut32.a
///In CodeBlocks 17.12 (not 20.03, 20.03 will have problems)
/// File-New-Project, GLUT Project, GLUT path: C:\Users\user\Desktop\freeglut

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
float x1= 0.5, y1=0, z1=0;
float x2=-0.5, y2=0, z2=0;
dfloat<3> dx1, dy1, dz1;///裡面都會是0
dfloat<3> dx2, dy2, dz2;///裡面都會是0
void display()
{
    glClear(GL_COLOR_BUFFER_BIT|GL_DEPTH_BUFFER_BIT);
    glPushMatrix();
        glTranslatef(x1,y1,z1);
        glutSolidSphere(0.1, 30,30);
    glPopMatrix();
    glPushMatrix();
        glTranslatef(x2,y2,z2);
        glutSolidSphere(0.1, 30,30);
    glPopMatrix();
    glutSwapBuffers();
}
int main(int argc, char**argv)
{
    ///float C = abs(v1-v2) - ??
    ///float dx=x1-x2, dy=y1-y2, dz=z1-z2;
    ///float C = sqrt(dx*dx+dy*dy+dz*dz) - 0.8;
    dx1.val(0)=x1; ///Q: 真的是這樣做嗎?
    dx2.val(0)=x2;
    ///沒設定的,都會是0

    dfloat<3> dx=dx1-dx2, dy=dy1-dy2, dz=dz1-dz2;
    dfloat<3> C = dsqrt(dx*dx+dy*dy+dz*dz) - 0.8;
    printf("%.3f %.3f %.3f\n", C.val(0), C.val(1), C.val(2));
    ///TODO: 不確定是不是對的, 要讀論文,看接下來怎麼用 gradient來改座標
    ///因 Stam一定是用 C/C++ 配 GLUT

    glutInit(&argc, argv);
    glutCreateWindow("glutPDB");
    glutDisplayFunc(display);
    glutMainLoop();
}
