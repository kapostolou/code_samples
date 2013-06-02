(ns a1.core)

(use 'clojure.zip)
(use 'clojure.xml)
(use 'clojure.java.io)
(use 'a1.util)
(use 'a1.process)

(defrecord F-Call-i [name reverse])

(defrecord F-Call-non-expanded [type1 type2 code1 code2 rigid-test reverse-rigid coarse-test reverse-coarse test test-i ms-test i-neg-test neg-test kill-first kill-second])
(defrecord F-Call [
                   type1 type2 code1 code2 rigid-test reverse-rigid coarse-test reverse-coarse test test-i ms-test kill-first kill-second
                   final-coarse
                   final-test
                   final-intersect
                   ])
				   
(defn construct-expanded-call [ne-f-call]
  (let [
        {
         type1 :type1
         type2 :type2
         code1 :code1
         code2 :code2
         rigid-test :rigid-test
         reverse-rigid :reverse-rigid
         coarse-test :coarse-test
         reverse-coarse :reverse-coarse
         test_ :test
         test-i_ :test-i
         ms-test :ms-test
         neg-test :neg-test
         i-neg-test :i-neg-test
         kfst :kill-first
         ksnd :kill-second
         }  ne-f-call
         test (and test_ neg-test )
         test-i (and test-i_ i-neg-test)
         final-coarse (if (or test test-i ) coarse-test "NOTHING") 
         final-test-tmp (if ms-test "MS-SPHERE" rigid-test)
         final-test (if test final-test-tmp "NOTHING" )
         final-intersect (if test-i rigid-test "NOTHING")
                     
        ]
    
    (F-Call. type1 type2 code1 code2 rigid-test reverse-rigid coarse-test reverse-coarse test test-i ms-test kfst ksnd final-coarse final-test final-intersect)
    
    )
  ) 


(defrecord F-Call-full 
  [type1 type2 
   code1 code2
   rigid-test reverse-rigid
   coarse-test reverse-coarse
   moving-sphere
   intersect-wanted
   prevent-wanted
  
   ])


(defn call [name reverse] (F-Call-i. name reverse ))
;(defrecord Object_ [name])

(defrecord Object_ [name rigid coarse side i])

(defn get-object-rigid-type [o]
  (cond (isa? o ::OBB-Rigid) "OBB" (isa? o ::Sphere-Rigid) "Sphere" (isa? o ::Triangle-Rigid) "Triangle" :default "N/A"))
  
(defn get-object-coarse-type [o]
  (cond  (isa? o ::Side-Coarse) "Side" (isa? o ::Sphere-Coarse) "Sphere" :default "N/A"))
(defn get-object-side [o]
  (cond (isa? o ::PSide) "Player" (isa? o ::ESide) "Enemy" (isa? o ::Neutral) "Neutral" :default "N/A"))


(def Types
  [::Enemy
  ::Player
  ::Obstacle
  ::RObstacle
  ::Bullet
  ::Pusher
  ::Circle
  ::CircleEn
  ::Teleporter
  ::Chain
  ::Triangle
  ::Spawn_Hit
  ])
(def coded-types (apply array-map (apply concat (map (fn [a b] [a b]) Types (iterate inc 0))  )))

(defn make-object [v]
  (let [
       funa (juxt get-object-rigid-type get-object-coarse-type get-object-side coded-types )
       [rigid coarse side i]   (funa v) 
      ]  
  (Object_. v rigid coarse side i )))  
  



 (derive-all ::PSide [::Player ::Bullet])
 (derive-all ::ESide [::Enemy ::CircleEn])
 
 (derive-all ::Pad [::Pusher ::Teleporter])
 (derive-all ::Obstruction [::Obstacle ::RObstacle ::Chain ::Triangle])
 
 (derive-all ::Neutral [::Pad ::Obstruction])
 
 (derive-all ::Sphere-Rigid [::Circle ::CircleEn])
 (derive-all ::OBB-Rigid [::Player ::Enemy ::Obstacle ::RObstacle ::Bullet ::Pusher ::Teleporter ::Chain])
 (derive-all ::Triangle-Rigid [::Triangle])

(derive-all ::Side-Coarse [::Obstacle])
(derive-all ::Sphere-Coarse (vec (for [typ Types  :when (not= typ ::Obstacle)] typ  ) ))
(derive-all ::All (vec (for [typ Types] typ  ) ))
 
 
(derive-all ::Nothing [::Spawn_Hit])






(def type-list (vec (for [type Types] (make-object type))))
(write-script (xml-commands type-list) "types")



(def pairs  ( for   [x Types y Types]  [x y] ))


(def rigid-order [ ::Triangle-Rigid ::OBB-Rigid ::Sphere-Rigid ::All])
(def rigid-table 
      [
               [[::Sphere-Rigid ::OBB-Rigid] "OBBSphere"]
               [[::OBB-Rigid ::OBB-Rigid] "OBBOBB"]
               [[::OBB-Rigid ::Triangle-Rigid] "TriangleOBB"]
               [[::Sphere-Rigid ::Triangle-Rigid] "TriangleSphere"]
               [[::Sphere-Rigid ::Sphere-Rigid] "SphereSphere"]
               [[::All ::All] "NOTHING"]
      ])

(def coarse-order [ ::Side-Coarse ::Sphere-Coarse ::All])
(def coarse-table 
      [
               [[::Side-Coarse ::Sphere-Coarse] "PlaneSphere"]
               [[::Sphere-Coarse ::Sphere-Coarse] "SphereSphere"]
               
               [[::All ::All] "NOTHING"]
      ])

(def test-table 
      [
               
             
               [::Player ::ESide]
               [::Player ::Pad]
              [::Enemy ::Bullet]
              [::CircleEn ::Obstruction]
              
      ])
(def i-table 
      [
                [::Player ::Circle]
                [::Obstruction ::Circle]
                [::Circle ::Circle]
             [::PSide ::Obstruction]
             [::ESide ::Obstruction]
               [::Obstruction ::Obstruction]
               
              
      ])
(def i-neg-table 
      [
               
             [::Enemy ::Chain]
             [::CircleEn ::Obstruction]
               
              
      ])
(def test-neg-table 
  []
)


(def ms-table 
      [
               [::Bullet ::Enemy]
             
               
              
      ])


(def kill-first-table 
      [
               [::Bullet ::Enemy]
                [::Enemy ::Bullet]
             
      ])
(def kill-second-table 
  (vec (for [x kill-first-table] 
           (vec (reverse x))
         
         ))    
  )

;(def bul (vec (for [pair pairs] ( (produce-for-this [rigid-table rigid-order]) pair)   )))

;(def bul-2 (vec (for [pair pairs] ( (produce-for-this [coarse-table coarse-order]) pair)   )))

(def bul-e (map-all 
             (for [pair pairs] pair)
             [
             (produce-for-this [rigid-table rigid-order])
             (produce-for-this [coarse-table coarse-order])
             (produce-for-this-pred test-table)
             (produce-for-this-pred i-table)
             (produce-for-this-pred ms-table)
             (produce-for-this-pred-neg i-neg-table)
             (produce-for-this-pred-neg test-neg-table)
             (produce-for-this-pred-simple kill-first-table)
             (produce-for-this-pred-simple kill-second-table)
              ]
             ))


(defn call-o [pair]
  (let [
        [t1 t2] pair
        code1 (coded-types t1)
        code2 (coded-types t2)
        [f-i-1 f-i-2 test i-test ms-test i-neg-test neg-test kill-first kill-second ] (bul-e pair)
        [call-r rever-r] [(:name f-i-1) (:reverse f-i-1)]
        [call-c rever-c] [(:name f-i-2) (:reverse f-i-2)]
        ]
    (construct-expanded-call (F-Call-non-expanded. t1 t2 code1 code2 call-r rever-r call-c rever-c test i-test ms-test i-neg-test neg-test  kill-first kill-second )) 
    )
  )
 (def final (map call-o pairs))
(write-script (xml-commands (apply vector final)) "clojure-generated-data")



;some debug/testing commands 
(println final)
(println ((produce-for-this-pred-simple kill-second-table) [::Enemy ::Bullet]))
(println ((produce-for-this-pred-simple kill-second-table) [::Bullet ::Enemy]))
(println kill-second-table) 