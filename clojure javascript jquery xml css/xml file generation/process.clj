(ns a1.process)

(defmacro dipatch-data-inner
  ([l elems order]
   (list 
     (list 'isa? l `(nth ~elems 0 ))   
     (list 'call `(nth ~elems 1 )  `(order-types (nth ~elems 0 ) ~order) )
     (list 'isa? l `(vec (reverse (nth ~elems 0 ))))   
     (list 'call `(nth ~elems 1 )  `(order-types (vec (reverse (nth ~elems 0 ))) ~order) )
     
     )
    
  ))

(defmacro dipatch-data
  ([l clauses order]
  
   
      `(cond ~@(apply concat (for [c clauses] (macroexpand `(dipatch-data-inner ~l ~c ~order) ) )))  
  ))


(defn produce [a v]
  (let [[table order] a]
  (eval (list 'dipatch-data v table order)))
  )

(defn produce-for-this [a]
  (partial produce a)
  )



(defn order-types [v table]
  (let
    [
     ordered-bases  (apply array-map (vec (apply concat (vec (map (fn [a b] [a b]) table (iterate inc 0))  ))))
     base-1 (first  (for [x table :when (isa? (nth v 0) x)] x ))
     base-2 (first  (for [x table :when (isa? (nth v 1) x)] x ))
           
    ]
        (
    >  (get ordered-bases base-1) (get ordered-bases base-2) ) 
  ))


(defmacro dipatch-data-inner-pred
  ([pair table-pair]
   (list 
     (list 'isa? pair table-pair)   
     true
     ;`(= 1 1)
     ;table
     (list 'isa? pair `(vec (reverse ~table-pair)))   
    true
     
     )
    
  ))

(defmacro dipatch-data-inner-pred-simple
  ([pair table-pair]
   (list 
     (list '= pair table-pair)   
     true
     ;`(= 1 1)
     ;table
        
    
     
     )
    
  ))



(defmacro dipatch-data-pred
  ([pair table ]
  
   
      `(cond ~@(apply concat (for [table-pair table] (macroexpand `(dipatch-data-inner-pred ~pair ~table-pair ) ) )) :default false)  
  ))

(defmacro dipatch-data-pred-simple
  ([pair table ]
  
   
      `(cond ~@(apply concat (for [table-pair table] (macroexpand `(dipatch-data-inner-pred-simple ~pair ~table-pair ) ) )) :default false)  
  ))

(defn produce-pred [table_ pair]
  (let [table table_]
  (eval (list 'dipatch-data-pred pair table)))
  )

(defn produce-pred-simple [table_ pair]
  (let [table table_]
  (eval (list 'dipatch-data-pred-simple pair table)))
  )



(defn produce-for-this-pred [table]
  (partial produce-pred table)
  )

(defn produce-for-this-pred-simple [table]
  (partial produce-pred-simple table)
  )


(defn produce-for-this-pred-neg [table]
  (fn [pair] (if ((produce-for-this-pred table) pair) false true))
  )




