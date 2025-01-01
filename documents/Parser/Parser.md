# Parser

SimpleDB.NETでのASTの生成を行うためのパーサーの仕様を示します。

## パーサーの仕様

Database design  and implementation の9.4 (p246)を参照のこと。

```math
\begin{equation}
\begin{aligned}
& \text{{<}Constant{>}} &&:= \text{StrTok}|\text{IntTok}
\\
& \text{{<}Expression{>}} &&:= \text{{<}Field{>}}|\text{{<}Constant{>}}
\\
& \text{{<}Term{>}}       &&:= \text{{<}Expression{>}} = \text{{<}Expression{>}}
\\
& \text{{<}Predicate{>}}  &&:= \text{{<}Term{>}} [\text{ AND {<}Predicate{>} } ]
\end{aligned}
\end{equation} 
```
