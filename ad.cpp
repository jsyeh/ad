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
    float &val(){
        return v[0];
    }
    float &val(int i){
        return v[i];
    }
    void val(float s){
        v[0]=s;
    }
    void val(int i, float s){
        v[i]=s;
    }

    dfloat & operator = ( const dfloat & a ){
        for(int i=0; i<=N; i++) v[i] = a.v[i];
        return *this;
    }

    dfloat & operator = ( float s ){
        v[0]=s;
        for(int i=1; i<=N; i++) v[i]=0.0f;
        return *this;
    }

    dfloat & operator += (const dfloat & a){
        for(int i=0; i<=N; i++) v[i] += a.v[i];
        return *this;
    }
    dfloat & operator -= (const dfloat & a){
        for(int i=0; i<=N; i++) v[i] -= a.v[i];
        return *this;
    }
    dfloat & operator *= (const dfloat & a){
        for(int i=1; i<=N; i++) v[i] = v[i]*a.v[0]+ v[0]*a.v[i];
        v[0] *= a.v[0];
        return *this;
    }
    dfloat & operator /= (const dfloat & a){
        float g = a.v[0]*a.v[0];
        for( int i=1; i<=N; i++) v[i] = (v[i]*a.v[0]-v[0]*a.v[i])/g;
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
        for(int i=0; i<=N; i++)v[i] *= s;
        return *this;
    }
    dfloat & operator /= (float s){
        for(int i=0; i<=N; i++) v[i] /=s;
        return *this;
    }
    dfloat operator - (void){
        dfloat c;
        for( int i=0; i<=N; i++) c.v[i] = -v[i];
        return c;
    }
    friend dfloat operator + (const dfloat &a, const dfloat &b){
        dfloat c;//https://stackoverflow.com/questions/13544364/over-loading-operator-must-take-either-zero-or-one-arguments
        for(int i=0; i<=N; i++) c.v[i] = a.v[i]+b.v[i];
        return c;
    }
    friend dfloat operator - (const dfloat &a, const dfloat &b){
        dfloat c;
        for(int i=0; i<=N; i++) c.v[i] = a.v[i]-b.v[i];
        return c;
    }
    friend dfloat operator * (const dfloat &a, const dfloat &b){
        dfloat c;
        c.v[0] = a.v[0]* b.v[0];
        for(int i=1; i<=N; i++) c.v[i] = a.v[i]*b.v[0] + a.v[0]*b.v[i];
        return c;
    }
    friend dfloat operator / (const dfloat &a, const dfloat &b){
        dfloat c;
        c.v[0] = a.v[0]/b.v[0];
        float g = b.v[0]*b.v[0];
        for(int i=1; i<=N; i++) c.v[i] = (a.v[i]*b.v[0]-a.v[0]*b.v[i])/g;
        return c;
    }
    //https://stackoverflow.com/questions/4622330/operator-overloading-member-function-vs-non-member-function
    friend dfloat operator + (float s, const dfloat &a ){
        dfloat c;
        c.v[0] = s+a.v[0];
        for(int i=1; i<=N; i++) c.v[i] = a.v[i];
        return c;
    }
    friend dfloat operator + (const dfloat &a, float s ){
        dfloat c;
        c.v[0] = a.v[0]+s;
        for(int i=1; i<=N; i++) c.v[i] = a.v[i];
        return c;
    }
    friend dfloat operator - (float s, const dfloat &a){
        dfloat c;
        c.v[0] = s-a.v[0];
        for(int i=1; i<=N; i++) c.v[i] = -a.v[i];
        return c;
    }
    friend dfloat operator - (const dfloat &a, float s){
        dfloat c;
        c.v[0] = a.v[0]-s;
        for(int i=1; i<=N; i++) c.v[i] = a.v[i];
        return c;
    }
    friend dfloat operator * (float s, const dfloat &a){
        dfloat c;
        for(int i=0; i<=N; i++) c.v[i] = s*a.v[i];
        return c;
    }
    friend dfloat operator * (const dfloat &a, float s){
        dfloat c;
        for(int i=0; i<=N; i++) c.v[i] = a.v[i]*s;
        return c;
    }
    friend dfloat operator / (float s, const dfloat &a){
        dfloat c;
        c.v[0] = s/a.v[0];
        float g = a.v[0]*a.v[0];
        for(int i=1; i<=N; i++)c.v[i] = -s*a.v[i]/g;
        return c;
    }
    friend dfloat operator / (const dfloat &a, float s){
        dfloat c;
        c.v[0] = s/a.v[0];
        float g = a.v[0]*a.v[0];
        for(int i=1; i<=N; i++)c.v[i] = -s*a.v[i]/g;
        return c;
    }
    friend dfloat dsqrt (const dfloat & a){
        dfloat c;
        c.v[0] = sqrtf(a.v[0]);
        for(int i=1; i<=N; i++) c.v[i] = 0.5f*a.v[i]/c.v[0];
        return c;
    }
    dfloat dacos(const dfloat & a){
        dfloat c;
        c.v[0] = (float) acos(a.v[0]);
        float g = -1.0f/sqrtf(1-a.v[0]*a.v[0]);
        for( int i=1; i<=N; i++) c.v[i] = a.v[i]*g;
        return c;
    }
};
int toy1()
{
    float x, y, z;
    float u, v;
    float f;

    x = y + z + u*u;
    y = z*y + x*v;

    f = x*x + y*y + u*u +v*v;
    return 0;
}

int toy2()
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
int main()
{
    dfloat<2> x, y;
    dfloat<2> u, v;
    dfloat<2> f;

    u.val(0) = 1.0f; v.val(1) = 1.0f;

    x = y + x + u*u;
    y = x*y + x*v;
    f = x*x + y*y + u*u + v*v;

}
