
<script type="text/javascript" async src="https://cdnjs.cloudflare.com/ajax/libs/mathjax/3.2.2/es5/tex-mml-chtml.min.js">
</script>
<script type="text/x-mathjax-config">
 MathJax.Hub.Config({
 tex2jax: {
 inlineMath: [['$', '$'] ],
 displayMath: [ ['$$','$$'], ["\\[","\\]"] ]
 }
 });
</script>

# Parser

SimpleDB.NETでのASTの生成を行うためのパーサーの仕様を示します。

## パーサーの仕様

Database design and implementation の9.4 (p246)を参照のこと。
<!-- TEXで書いてほしい。"<定数> := StrTok|IntTok" -->
$ \text{<Constant>} := \text{StrTok}|\text{IntTok} $
$ \text{<Expression>} := \text{<Field>}|\text{<Constant>} $
$ \text{<Term>} := \text{<Expression>}|\text{<Expression>} $
$ \text{<Predicate>} := \text{<Term>} [\text{AND <Predicate>} ]$
