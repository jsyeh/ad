# ad
Automatic Differentiation implementation in C++ from Stam's book

方向解說
=======
- 在 Stam 書裡面 Chap 14 介紹的 Automatic Differentiation (AD),能非常輕易的算出複雜函式的 gradient值, 也就是 `f[0], f[1], f[2]...` 的值
- 在 Position Based Dynamics 論文中, 可以模擬衣料(彈簧) 或流體。我們曾在 2020年3-4月,試著在 Processing 及 Unity 中, 實作出 Position Based Dynamics, 因為解 gradient 的程式太複雜, 所以沒辦法把頂點變多
- 現在如果能將 AD 拿來 Position Based Dynamics 裡面使用, 則可以 (1) 驗證 AD 的實作/理解無誤, (2) 讓 Position Based Dyanmics 變得更簡單、實作可以更快、可以解的變數更多
- Adjoint 方法, 有 2篇論文, 我們讀 Stam 的 *Notes on Adjoint Control of Graphical Simulation* 有理解它的重要精神, 也把大部分公式看懂了。它可以利用 *前面到後面, 後面到前面, 循環進行Optimizer* 來求解。不過我們還沒能力實作。
