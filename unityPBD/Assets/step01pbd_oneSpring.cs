using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class step01pbd_oneSpring : MonoBehaviour
{
    public GameObject ball01, ball02;
    void Start()
    {
        
    }

    void Update()
    {
        Vector3 v1 = ball01.transform.position;
        Vector3 v2 = ball02.transform.position;

        x1=v1.x; y1=v1.y; z1=v1.z; //設好控制變數
        x2=v2.x; y2=v2.y; z2=v2.z; //設好控制變數
        solvePBD();
        ball01.transform.position= new Vector3(x1,y1,z1); //更新回去
        ball02.transform.position= new Vector3(x2,y2,z2); //更新回去
    }
    float x1=-10.5f, y1=0, z1=0;
    float x2=+10.5f, y2=0, z2=0; ///總共有6個控制變數, 下面 dfloat 會存「對6個控制變數」微分的值
    void solvePBD()
    {
        float len0=5.5f; ///原始長度為 0.5
        float x=x1-x2, y=y1-y2, z=z1-z2; ///原始的公式中 用來算 cost function 的輔助變數
        float C = Mathf.Sqrt(x*x+y*y+z*z) - len0; ///原始的cost function

        //以下已利用 Automatic Differentiation 技巧, 配合 operator overloading 完成
        ///細讀 Stam 第14章, 了解 (1) 控制變數 x1,y1,z1, x2,y2,z2 有6個, 所以 dfloat<6>
        ///(2) 有些項要設1, 因為 「自己對自己微分」會得到1
        dfloat dx1 = new dfloat(x1);
        dfloat dy1 = new dfloat(y1);
        dfloat dz1 = new dfloat(z1); ///初始原值, 其他裡面都會是0
        dfloat dx2 = new dfloat(x2);
        dfloat dy2 = new dfloat(y2);
        dfloat dz2 = new dfloat(z2); ///初始原值, 其他裡面都會是0
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

    }
}
