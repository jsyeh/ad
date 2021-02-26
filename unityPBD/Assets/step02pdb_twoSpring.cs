using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class step02pdb_twoSpring : MonoBehaviour
{
    float x1=-10f, y1=0, z1=0;
    float x2=0f, y2=0, z2=0; 
    float x3=+10f, y3=0, z3=0; ///總共有9個控制變數, 下面 dfloat 會存「對9個控制變數」微分的值
    public GameObject ball01, ball02, ball03;
    // Start is called before the first frame update
    void Start()
    {
        //https://docs.unity3d.com/ScriptReference/GameObject.CreatePrimitive.html
        ball01=GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ball01.transform.position = new Vector3(x1,y1,z1);
        ball01.transform.SetParent(this.transform);
        ball02=GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ball02.transform.position = new Vector3(x2,y2,z2);
        ball02.transform.SetParent(this.transform);
        ball03=GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ball03.transform.position = new Vector3(x3,y3,z3);
        ball03.transform.SetParent(this.transform);
        //dfloat a = new dfloat(30);//原來另外一個程式裡寫的 class dfloat 這裡可以用到
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 v1 = ball01.transform.position;
        Vector3 v2 = ball02.transform.position;
        Vector3 v3 = ball03.transform.position;

        x1=v1.x; y1=v1.y; z1=v1.z; //設好控制變數
        x2=v2.x; y2=v2.y; z2=v2.z; //設好控制變數
        x3=v3.x; y3=v3.y; z3=v3.z; //設好控制變數
        solvePBD();
        ball01.transform.position= new Vector3(x1,y1,z1); //更新回去
        ball02.transform.position= new Vector3(x2,y2,z2); //更新回去
        ball03.transform.position= new Vector3(x3,y3,z3); //更新回去
    }
    void solvePBD()
    {
        float len0=10f; ///原始長度為 0.5
        float x=x1-x2, y=y1-y2, z=z1-z2; ///原始的公式中 用來算 cost function 的輔助變數
        float C = Mathf.Sqrt(x*x+y*y+z*z) - len0; ///原始的cost function
        {
            x=x2-x3;
            y=y2-y3;
            z=z2-z3;
            C += Mathf.Sqrt(x*x+y*y+z*z) - len0;
        }

        //以下已利用 Automatic Differentiation 技巧, 配合 operator overloading 完成
        ///細讀 Stam 第14章, 了解 (1) 控制變數 x1,y1,z1, x2,y2,z2 有6個, 所以 dfloat<6>
        ///(2) 有些項要設1, 因為 「自己對自己微分」會得到1
        dfloat dx1 = new dfloat(9,x1);
        dfloat dy1 = new dfloat(9,y1);
        dfloat dz1 = new dfloat(9,z1); ///初始原值, 其他裡面都會是0
        dfloat dx2 = new dfloat(9,x2);
        dfloat dy2 = new dfloat(9,y2);
        dfloat dz2 = new dfloat(9,z2); ///初始原值, 其他裡面都會是0
        dfloat dx3 = new dfloat(9,x3);
        dfloat dy3 = new dfloat(9,y3);
        dfloat dz3 = new dfloat(9,z3); ///初始原值, 其他裡面都會是0
        dx1.val(1)=1; ///[1]項 是對 變數1 微分
        dy1.val(2)=1; ///[2]項 是對 變數2 微分
        dz1.val(3)=1; ///[3]項 是對 變數3 微分
        dx2.val(4)=1; ///[4]項 是對 變數4 微分
        dy2.val(5)=1; ///[5]項 是對 變數5 微分
        dz2.val(6)=1; ///[6]項 是對 變數6 微分
        dx3.val(7)=1; ///[7]項 是對 變數7 微分
        dy3.val(8)=1; ///[8]項 是對 變數8 微分
        dz3.val(9)=1; ///[9]項 是對 變數9 微分
        ///沒設定的,都會是0

        ///這裡的 dx 是前面 x=x1-x2 去做 dfloat 計算的意思
        dfloat dx=dx1-dx2, dy=dy1-dy2, dz=dz1-dz2; ///AD公式, 用來算 cost function 的輔助變數
        dfloat gC = dfloat.dsqrt(dx*dx+dy*dy+dz*dz) - len0; ///AD公式, cost function, gC[1]..gC[6]是gradient

        {
            dx=dx2-dx3;
            dy=dy2-dy3;
            dz=dz2-dz3; ///AD公式, 用來算 cost function 的輔助變數
            gC += dfloat.dsqrt(dx*dx+dy*dy+dz*dz) - len0; ///AD公式, cost function, gC[1]..gC[6]是gradient
        } //可能出錯的地方: (1) stretch 的 dx 的正負方向, (2) cost function是相加嗎? (3) 要恢復1個月前的記憶
        //在公式10,公式11裡, 右邊有單位向量

        float len2=0; ///posBasedDyn.pdf 的公式(5)
        for(int i=1; i<=9; i++){
            len2 += gC.val(i)*gC.val(i); ///要算出分母 (gradient的長度平方)
        }
        len2 = Mathf.Sqrt(len2);
        x1+=(-C/len2)*gC.val(1); ///posBasedDyn.pdf 的公式(5) 算出 delta P 回去改 P
        y1+=(-C/len2)*gC.val(2);
        z1+=(-C/len2)*gC.val(3);
        x2+=(-C/len2)*gC.val(4);
        y2+=(-C/len2)*gC.val(5);
        z2+=(-C/len2)*gC.val(6);
        x3+=(-C/len2)*gC.val(7);
        y3+=(-C/len2)*gC.val(8);
        z3+=(-C/len2)*gC.val(9);
    }
}
