#include <GL/glut.h>
///���� CodeBlocks �ϥ� freeglut �n�w�˪��F��
///freglut can be download by google: freeglut windows download
///https://www.transmissionzero.co.uk/software/freeglut-devel/
///https://www.transmissionzero.co.uk/files/software/development/GLUT/freeglut-MinGW.zip
/// Unzip freeglut to C:\Users\user\Desktop\freeglut
/// copy C:\Users\User\Desktop\freeglut\lib\libfreeglut.a to
///      C:\Users\User\Desktop\freeglut\lib\glut32.a
///In CodeBlocks 17.12 (not 20.03, 20.03 will have problems)
/// File-New-Project, GLUT Project, GLUT path: C:\Users\user\Desktop\freeglut

///�U�� _NO_OLDNAMES �o��, �O���F�׶} math.h�̳��M�]�ϥΨ쪺 y1
#define _NO_OLDNAMES
#include <math.h>
#include <stdio.h>
/// The codes are modified from Figure 14.5 in Chapter 14 of Stam's book (The art of fluid animation)
/// �{���ק�� Stam �Ѥ���14��(Figure 14.5), �ϥ� C++ operator overloading
/// �� Automatic Differentiation �ܱo²����z��/�������C
/// ���O������Ѥ����{���X���X�ӽsĶ, �|�X�{�\�h�sĶ���~ (compile errors)
/// �i��O C++ compiler ���������D�C�ҥH�ڰ��F�������ק�C

/// �ק�1: ���� return �^�Ǫ��a��, �W�[ & �� reference �Ѧ�, �� lvalue �i�Q�ק�
/// �ק�2: ���ǰѼ� �[�F const, �ӿ� rvalue ���|�Q�ק�
/// �ק�3: ���Ǧa��K�� friend ���禡�i�H�b class �̳Q�ŧi

template <int N> class dfloat
{
public:
    float v[N+1]; /// change this to your favorite "pretend real."
    /// and all references to floats. Use a typedef or a define or whatever.
    dfloat(){ ///��l��, ���]��0
        for( int i=0; i<=N; i++ ) v[i]=0.0f;
    }
    dfloat(float s){ ///��l�Ʋ�1�ӭ�, ��L���]0
        v[0]=s;
        for( int i=1; i<=N; i++ ) v[i]=0.0f;
    }
    float &val(){ /// �o�̼W�[ & �� reference �Ѧ�, �� lvalue �i�Q�ק�
        return v[0];
    }
    float &val(int i){ /// �o�̼W�[ & �� reference �Ѧ�, �� lvalue �i�Q�ק�
        return v[i];
    }
    void val(float s){ ///�W���O���index(���X),�U���Ofloat��(�]�w)
        v[0]=s;
    }
    void val(int i, float s){ ///�]�w��
        v[i]=s;
    }

    /// �o�̼W�[ & �� reference �Ѧ�, �� lvalue �i�Q�ק�
    dfloat & operator = ( const dfloat & a ){ /// �o�̼W�[ const, �ӿ� rvalue ���|�Q�ק�
        for( int i=0; i<=N; i++ ) v[i] = a.v[i];
        return *this;
    }

    dfloat & operator = ( float s ){
        v[0]=s;
        for( int i=1; i<=N; i++ ) v[i]=0.0f;
        return *this;
    }

    dfloat & operator += (const dfloat & a){ /// �o�̼W�[ const, �ӿ� rvalue ���|�Q�ק�
        for( int i=0; i<=N; i++ ) v[i] += a.v[i];
        return *this;
    }
    dfloat & operator -= (const dfloat & a){ /// �o�̼W�[ const, �ӿ� rvalue ���|�Q�ק�
        for( int i=0; i<=N; i++ ) v[i] -= a.v[i];
        return *this;
    }
    dfloat & operator *= (const dfloat & a){ /// �o�̼W�[ const, �ӿ� rvalue ���|�Q�ק�
        for( int i=1; i<=N; i++ ) v[i] = v[i]*a.v[0]+ v[0]*a.v[i];
        v[0] *= a.v[0];
        return *this;
    }
    dfloat & operator /= (const dfloat & a){ /// �o�̼W�[ const, �ӿ� rvalue ���|�Q�ק�
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
    /// �H�U�W�[ friend, �� operator overloading �i�H��J (��,�k) 2�ӰѼ�
    friend dfloat operator + (const dfloat &a, const dfloat &b){ /// �o�̼W�[ const, �ӿ� rvalue ���|�Q�ק�
        dfloat c; ///https://stackoverflow.com/questions/13544364/over-loading-operator-must-take-either-zero-or-one-arguments
        for( int i=0; i<=N; i++ ) c.v[i] = a.v[i]+b.v[i];
        return c;
    }
    friend dfloat operator - (const dfloat &a, const dfloat &b){ /// �o�̼W�[ const, �ӿ� rvalue ���|�Q�ק�
        dfloat c;
        for( int i=0; i<=N; i++ ) c.v[i] = a.v[i]-b.v[i];
        return c;
    }
    friend dfloat operator * (const dfloat &a, const dfloat &b){ /// �o�̼W�[ const, �ӿ� rvalue ���|�Q�ק�
        dfloat c;
        c.v[0] = a.v[0] * b.v[0];
        for( int i=1; i<=N; i++ ) c.v[i] = a.v[i]*b.v[0] + a.v[0]*b.v[i];
        return c;
    }
    friend dfloat operator / (const dfloat &a, const dfloat &b){ /// �o�̼W�[ const, �ӿ� rvalue ���|�Q�ק�
        dfloat c;
        c.v[0] = a.v[0] / b.v[0];
        float g = b.v[0]*b.v[0];
        for( int i=1; i<=N; i++ ) c.v[i] = (a.v[i]*b.v[0]-a.v[0]*b.v[i])/g;
        return c;
    }
    friend dfloat operator + (float s, const dfloat &a ){ /// �o�̼W�[ const, �ӿ� rvalue ���|�Q�ק�
        dfloat c;
        c.v[0] = s + a.v[0];
        for( int i=1; i<=N; i++ ) c.v[i] = a.v[i];
        return c;
    }
    friend dfloat operator + (const dfloat &a, float s ){ /// �o�̼W�[ const, �ӿ� rvalue ���|�Q�ק�
        dfloat c;
        c.v[0] = a.v[0] + s;
        for( int i=1; i<=N; i++ ) c.v[i] = a.v[i];
        return c;
    }
    friend dfloat operator - (float s, const dfloat &a){ /// �o�̼W�[ const, �ӿ� rvalue ���|�Q�ק�
        dfloat c;
        c.v[0] = s - a.v[0];
        for( int i=1; i<=N; i++ ) c.v[i] = -a.v[i];
        return c;
    }
    friend dfloat operator - (const dfloat &a, float s){ /// �o�̼W�[ const, �ӿ� rvalue ���|�Q�ק�
        dfloat c;
        c.v[0] = a.v[0] - s;
        for( int i=1; i<=N; i++ ) c.v[i] = a.v[i];
        return c;
    }
    friend dfloat operator * (float s, const dfloat &a){ /// �o�̼W�[ const, �ӿ� rvalue ���|�Q�ק�
        dfloat c;
        for( int i=0; i<=N; i++ ) c.v[i] = s*a.v[i];
        return c;
    }
    friend dfloat operator * (const dfloat &a, float s){ /// �o�̼W�[ const, �ӿ� rvalue ���|�Q�ק�
        dfloat c;
        for( int i=0; i<=N; i++ ) c.v[i] = a.v[i]*s;
        return c;
    }
    friend dfloat operator / (float s, const dfloat &a){ /// �o�̼W�[ const, �ӿ� rvalue ���|�Q�ק�
        dfloat c;
        c.v[0] = s/a.v[0];
        float g = a.v[0]*a.v[0];
        for( int i=1; i<=N; i++ ) c.v[i] = -s*a.v[i]/g;
        return c;
    }
    friend dfloat operator / (const dfloat &a, float s){ /// �o�̼W�[ const, �ӿ� rvalue ���|�Q�ק�
        dfloat c;
        ///c.v[0] = s/a.v[0]; //TODO:�o�̧ڦ�����,�ݦ^�h�P�Bad.cpp��ad_explain.cpp
        ///float g = a.v[0]*a.v[0]; //TODO
        for( int i=1; i<=N; i++ ) c.v[i] = a.v[i]/s; ///TODO
        return c;
    }
    friend dfloat dsqrt (const dfloat & a){ /// �o�̼W�[ const, �ӿ� rvalue ���|�Q�ק�
        dfloat c;
        c.v[0] = sqrtf(a.v[0]);
        for( int i=1; i<=N; i++ ) c.v[i] = 0.5f * a.v[i] / c.v[0];
        return c;
    }
    friend dfloat dacos(const dfloat & a){ /// �o�̼W�[ const, �ӿ� rvalue ���|�Q�ק�
        dfloat c;
        c.v[0] = (float) acos(a.v[0]);
        float g = -1.0f/sqrtf(1-a.v[0]*a.v[0]);
        for( int i=1; i<=N; i++ ) c.v[i] = a.v[i] * g;
        return c;
    }
};

int toy1() ///²�ƪ��{��, �� Stam �� Figure 14.3 �ܷN��
{
    float x, y, z; ///variables
    float u, v;    ///two controls
    float f;       ///cost variable

    x = y + z + u*u;  ///statement #1
    y = z*y + x*v;    ///statement #2

    f = x*x + y*y + u*u +v*v;  ///cost function
    return 0;
}

int toy2() ///��ʧ��� Automatic Differentiation ������, �� Stam �� Figure 14.4 �ܷN��
{
    float x, y, z, dx[2], dy[2], dz[2];
    float u, v, du[2], dv[2];
    float f, df[2];

    dx[0]=dx[1]=dy[0]=dy[1]=dz[0]=dz[1]=df[0]=df[1]=0;
    du[0]=1; du[1]=0; dv[0]=0; dv[1]=1;
    ///�]��u��u�L����1, v��v�L����1 (�ۤv��ۤv�L��,�|�o��1)
    ///�o�̪��}�C��, [0]���O��u�L��, [1]���O��v�L��

    x = y + z + u*u;
       dx[0] = dy[0] + dz[0] + 2*u*du[0];
       dx[1] = dy[1] + dz[1] + 2*u*du[1];

    y = z*y + x*v;
       dy[0] = z*dy[0] + y*dz[0] + v*dx[0] + x*dv[0];
       dy[1] = z*dy[1] + y*dz[1] + v*dx[1] + x*dv[1];
    f = x*x + y*y + u*u + v*v;
       df[0] = 2*x*dx[0] + 2*y*dy[0] + 2*u*du[0] + 2*v*dv[0];
       df[1] = 2*x*dx[1] + 2*y*dy[1] + 2*u*du[1] + 2*v*dv[1];
    return 0;
}
///Q: �ثe + - �|���K, ���令 - + �N�z��, ������?
///�t�~, �ڧ��ܼƥ����������ܼ�, ���ܼƭ� (ex. x1, x2) �èS�� solvePBD()���ϥ�, ���X�z
float x1=+0.5, y1=0, z1=0;
float x2=-0.5, y2=0, z2=0; ///�`�@��6�ӱ����ܼ�, �U�� dfloat<6>�|�s�u��6�ӱ����ܼơv�L������
dfloat<6> dx1, dy1, dz1;///�̭����|�O0
dfloat<6> dx2, dy2, dz2;///�̭����|�O0
///TODO: Ū Stam's The Art of Fluid Dynamics ��14��, �F�� AD ���Ӹ`/��@�N�q
///�] Stam�@�w�O�� C/C++ �t GLUT
void solvePBD()
{
///ŪPosition Based Dynamics�פ�, �F�ѫ��� gradient �ӧ�y��
///https://matthias-research.github.io/pages/publications/posBasedDyn.pdf
    ///float C = abs(v1-v2) - ��l������
    float dx=x1-x2, dy=y1-y2, dz=z1-z2; ///��l�������� �ΨӺ� cost function �����U�ܼ�
    float C = sqrt(dx*dx+dy*dy+dz*dz) - 0.5; ///��l��cost function, ��l���׬� 0.5

    ///��Ū Stam ��14��, �F�� (1) �����ܼ� x1,y1,z1, x2,y2,z2 ��6��, �ҥH dfloat<6>
    ///(2) ���Ƕ��n�]1, �]�� �u�ۤv��ۤv�L���v�|�o��1
    dx1.val(0)=1; ///[0]�� �O�� �ܼ�0 �L��
    dy1.val(1)=1; ///[1]�� �O�� �ܼ�1 �L��
    dz1.val(2)=1; ///[2]�� �O�� �ܼ�2 �L��
    dx2.val(3)=1; ///[3]�� �O�� �ܼ�3 �L��
    dy2.val(4)=1; ///[4]�� �O�� �ܼ�4 �L��
    dz2.val(5)=1; ///[5]�� �O�� �ܼ�5 �L��
    ///�S�]�w��,���|�O0

    dfloat<6> ddx=dx1-dx2, ddy=dy1-dy2, ddz=dz1-dz2; ///�ΨӺ� cost function �����U�ܼ�
    dfloat<6> gC = dsqrt(ddx*ddx+ddy*ddy+ddz*ddz) - 0.5;

    float len2=0; ///posBasedDyn.pdf ������(5)
    for(int i=0; i<6; i++){
        ///printf(" %.3f", gC.val(i) ); ///Q: ���k���Ȥ����
        len2 += gC.val(i)*gC.val(i); ///�n��X����������
    }
    x1+=(-C/len2)*gC.val(0); ///posBasedDyn.pdf ������(5) ��X delta P �^�h�� P
    x2+=(-C/len2)*gC.val(3);
    printf(" (%.3f %.3f %.3f), (%.3f %.3f %.3f)\n", x1,y1,z1, x2,y2,z2);
    ///���� �̫�u����O���b �۵M���� 0.5
    ///Q: ���������k��?
}
void display()
{
    glClear(GL_COLOR_BUFFER_BIT|GL_DEPTH_BUFFER_BIT);
    glBegin(GL_LINES); ///�e2�I�����s�u
        glVertex3f(x1,y1,z1);
        glVertex3f(x2,y2,z2);
    glEnd();
    glPushMatrix(); ///�e�k�䪺�y
        glTranslatef(x1,y1,z1);
        glutSolidSphere(0.05, 30,30);
    glPopMatrix();
    glPushMatrix(); ///�e���䪺�y
        glTranslatef(x2,y2,z2);
        glutSolidSphere(0.05, 30,30);
    glPopMatrix();
    glutSwapBuffers();
}
void keyboard(unsigned char key, int x, int y)
{
    solvePBD();///��keyboard��, ����@�� solvePBD()
    glutPostRedisplay();///���⭫�e�e��
}
int main(int argc, char**argv)
{
    glutInit(&argc, argv);
    glutCreateWindow("glutPDB");
    glutDisplayFunc(display);
    glutKeyboardFunc(keyboard);
    glutMainLoop();
}
