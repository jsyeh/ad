//https://matthias-research.github.io/pages/publications/posBasedDyn.pdf
//照著上面論文的演算法實作, 應該是 better PBD
int N=8;//vertex number頂點數量
PVector [] gradient=new PVector[N];
PVector [] pos=new PVector[N];
PVector [] pos0=new PVector[N];
PVector [] v=new PVector[N];
float d0=30, angle0=radians(0); 
boolean bSolving=true;
void setup(){
  size(500,500);
  for(int i=0;i<N;i++){
    pos[i]=new PVector( 150+i*30, 100);//new PVector(random(150,350),random(150,350));
    pos0[i]=new PVector( 150+i*30, 100);//new PVector(random(150,350),random(150,350));
    v[i] = new PVector();
    gradient[i]=new PVector();
  }
  genStretchBendingConstraint();
}
void printNAN(){
  for(int i=0; i<N; i++) print(pos[i]);
  println();
}
void myLine(PVector p1, PVector p2){
  line(p1.x,p1.y,p2.x,p2.y);
}
float Ci(int i, PVector [] pos){
  PVector ans=new PVector();
  for(int j=0;j<N;j++){//TODO: 變成N
    float w=a[i][j];
    ans.add( PVector.mult(pos[j], w) );
  }
  return ans.magSq()-d0*d0;
}
//float C1(PVector [] pos){//input: 3*pos
//  PVector v1 = PVector.sub(pos[1],pos[0]);
//  return v1.magSq()-d0*d0;
//}
//float C2(PVector [] pos){
//  PVector v2 = PVector.sub(pos[2],pos[1]);
//  return v2.magSq()-d0*d0;
//}
//int [][]b={
//  {1,0,2,1},
//  {2,1,3,2},
//  {3,2,4,3}
//};
//int [][]a={
//  {-1,1,0,0,0},
//  {0,-1,1,0,0},
//  {0,0,-1,1,0},
//  {0,0,0,-1,1} };
int [][]a;
int [][]b;
void genStretchBendingConstraint(){
  a = new int[N-1][N];
  for(int i=0;i<N-1;i++){
    for(int j=0;j<N;j++){
      if(i==j) a[i][j]=-1;
      else if(i+1==j) a[i][j]=1;
      else a[i][j]=0;
    }
  }
  b = new int[N-2][4];
  for(int i=0;i<N-2;i++){
    b[i][0]=i+1;
    b[i][1]=i;
    b[i][2]=i+2;
    b[i][3]=i+1;
  }
}
float Cbendi(int i, PVector [] pos){
  PVector v1=PVector.sub(pos[ b[i][0] ],pos[ b[i][1] ]);
  PVector v2=PVector.sub(pos[ b[i][2] ],pos[ b[i][3] ]);
  return PVector.angleBetween(v1,v2)-angle0;
}
//float Cbend(PVector[] pos){//acos() in the future
//  PVector v1=PVector.sub(pos[1],pos[0]);
//  PVector v2=PVector.sub(pos[2],pos[1]);
//  return PVector.angleBetween(v1,v2)-angle0;
//}
//float Canother(int Cj){
//  return Ci(Cj, pos);
//}
float C(int Cj){
  if(Cj<N-1) return Ci(Cj,pos);    
  //if(Cj==0) return Ci(0,pos);//C1(pos);
  //if(Cj==1) return Ci(1,pos);//C2(pos);
  //if(Cj==2) return Ci(2,pos);
  //if(Cj==3) return Ci(3,pos);
  
  else return Cbendi(Cj-(N-1), pos);
  //if(Cj==4) return Cbendi(0,pos);//Cbend(pos);
  //if(Cj==5) return Cbendi(1,pos);
  //if(Cj==6) return Cbendi(2,pos);
  //return 0;
}
void calcGradient(int Cj, int Xi){//input:第幾個constraint, 第幾個點
  float d=0.001;//for gradient distance//未來:需要依需求,讓d變得更小,才可讓gradient sum為0的問題不出現
  float f1, f0;
  pos[Xi].x+=d;
  f1=C(Cj);
  pos[Xi].x-=d*2;
  f0=C(Cj);
  pos[Xi].x+=d;
  gradient[Xi].x=(f1-f0)/2/d;

  pos[Xi].y+=d;
  f1=C(Cj);
  pos[Xi].y-=d*2;
  f0=C(Cj);
  pos[Xi].y+=d;
  gradient[Xi].y=(f1-f0)/2/d;

  pos[Xi].z+=d;
  f1=C(Cj);
  pos[Xi].z-=d*2;
  f0=C(Cj);
  pos[Xi].z+=d;
  gradient[Xi].z=(f1-f0)/2/d;
}
void solver(){//Input: pos*3
  float maxMag=0;
  float []w=new float[N];//{0.2,0.2,0.2,0.2,0.2};//0.33333, 0.33333, 0.33333};
  for(int i=0;i<N;i++) w[i]=1.0/N;
  for(int Cj=0; Cj<(N-1)+(N-2); Cj++){//for(int Cj=0; Cj<7; Cj++){//7: N=5, (5-1)+(5-2)=>7
    //partial gradient C[?]
    for(int Xi=1;Xi<N;Xi++){
      calcGradient(Cj, Xi);
    }
    float gradientSum=0;
    for(int Xi=1;Xi<N;Xi++){
      gradientSum += w[Xi]*gradient[Xi].magSq();
    }
    if(gradientSum<0.0000001){
      println("gradient sum very small");
      continue;
    }
    float lambda = C(Cj)/gradientSum;
    for(int Xi=1;Xi<N;Xi++){  //更新
      PVector diff=PVector.mult(gradient[Xi], -lambda*w[Xi]);
      pos[Xi].add(diff);
      if(diff.mag()>maxMag) maxMag=diff.mag();
    }
  }
  if(maxMag<0.1) bSolving=false;//距離夠小,就停止計算
}
void draw(){
  PVector g=new PVector(0, 0.98);//還沒有乘上 delta T 及重量
  //printNAN();
  if(bSolving){
    for(int i=1; i<N; i++){
      v[i].add(g);//step (5) 還沒有乘上 delta T 及重量
      //step (6) damping 還沒有做
      pos[i].x = pos0[i].x + v[i].x; //step (7)
      pos[i].y = pos0[i].y + v[i].y;
    }
    //step (8) collision constration 還沒有做 
    solver();//step (9)-(11)

    for(int i=1; i<N; i++){
      v[i].x = pos[i].x - pos0[i].x;//step (13)
      v[i].y = pos[i].y - pos0[i].y;
      pos0[i].x = pos[i].x;//step (14)
      pos0[i].y = pos[i].y;
    }
    //step (16) 是碰撞時的 friction 及 恢復係數
  }
  background(128);
  for(int i=0;i<N-1;i++) myLine(pos[i], pos[i+1]);
  for(int i=0;i<N;i++){
    if(i==N-1) fill(255,0,0);
    else fill(255);
    ellipse(pos[i].x, pos[i].y, 20,20);
  }
}
void mousePressed(){
  bSolving=true;
}
void keyPressed(){
  if(key=='c' || key=='C'){//把跑掉的東西,放回中心點
    PVector c=new PVector();
    PVector c2=new PVector(width/2,height/2);
    for(int i=0;i<N;i++){
      c.add(pos[i]);
    }
    c.div(N);
    for(int i=0;i<N;i++){
      pos[i].sub(c).add(c2);
    }
  }  
}
