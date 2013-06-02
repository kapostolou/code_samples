(ns a1.util)

(use 'clojure.zip)
(use 'clojure.xml)
(use 'clojure.java.io)

(defn write-script [stri name]
  (with-open [wrtr (writer (str "c:\\game-scripts\\" name ".xml") )]
    (.write wrtr stri )
    ))

(defn fix-name [n]

  (clojure.string/replace
    (str (type n))     "class a1.core." ""    )
  )

(defn fix-name-2 [n]

  (clojure.string/replace
     n     ":a1.core/" ""    
  ))

(defn xml-ready [o nam]
 (cond
  (or (seq? o) (vector? o) ) {:tag nam :attrs {} :content (vec (for [v o] (xml-ready v (keyword  (fix-name v ) )   ) )  )        }
  (map? o)  {:tag nam :attrs {} :content (vec (for [v o] (xml-ready (val v) (key v)) )  )        }
   (= 2 2) {:tag nam :attrs {:value (fix-name-2(str o))} }))

(defn xml-commands [colle] (with-out-str (emit-element (xml-ready  colle :Commands))))

(defn derive-all [parent childs]
  
  (doall(for [c childs] (derive c parent)))
  )


(defn map-all [coll fn-call]
  (let [
        mega-fn (apply juxt fn-call)
        applied (map mega-fn coll)
        res (apply concat (map (fn [c r] [c r]) coll applied))
        ] 
    (apply array-map res)
  ))

(println (map-all [1 2 3 4] [(partial + 1) (partial * 3) (partial + 1000)]))