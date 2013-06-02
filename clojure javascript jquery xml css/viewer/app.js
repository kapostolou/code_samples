var last_selected=5;
var type_table=new Object();
$(function() {
console.log("fsfs");
$.ajax({
       url: "types.xml"
    }).
	done
	(function( text) {
var  xmlDoc = $.parseXML( text )
    $xml = $( text );
 
    var $name1=$("#object-name-1");
	var $rigid1=$("#object-rigid-1");
	var $side1=$("#object-side-1");
	var $name2=$("#object-name-2");
	var $rigid2=$("#object-rigid-2");
	var $side2=$("#object-side-2");
	var $coarse1=$("#object-coarse-1");
	var $coarse2=$("#object-coarse-2");
	
	
	var $shown1=$("#shown-object-1");
	var $shown2=$("#shown-object-2");
    $out_func_r = $("#func-r-name");
	$out_rever_r = $("#func-r-rever");
	$out_func_c = $("#func-c-name");
	$out_rever_c = $("#func-c-rever");
	$out_test = $("#test-wanted");
	$out_test_i = $("#i-test-wanted");
	$out_ms_test = $("#ms-test");
	$out_final_test = $("#final-test");
	$out_final_coarse = $("#final-coarse");
	$out_final_i_test = $("#final-i-test");
	$out_kill_first = $("#kill-first");
	$out_kill_second = $("#kill-second");
    $thingies = $xml.find("name");
	console.log(text);
	var iiii=0;
	$thingies.each(function()
	{
	console.log(iiii++);
	 var self=$(this);
	      var new_elem_3=$("<div></div>", {"text": self.attr("value"), "id": self.attr("value") }).addClass("td-like").on("click",function(){
		  if(last_selected!=5)last_selected.removeClass("selecte");
		  console.log("lol");last_selected=$(this);
		  last_selected.addClass("selecte");
		  var id1=last_selected.attr("id");
		  var obj1=type_table[id1];
		  var side1=obj1["side"];
		  var rigid1=obj1["rigid"];
		  var coarse1=obj1["coarse"];
		   $shown1.fadeOut(30, function()
		   {
		      $name1.text(id1);
			  $side1.text(side1);
			  $rigid1.text(rigid1);
			  $coarse1.text(coarse1);
			  $shown1.fadeIn(300, function(){});
		   });
		  });
		  var new_elem_4=$("<div></div>", {"text": self.attr("value"), "id": self.attr("value") }).addClass("td-like").on("mouseover",function(){console.log("lolad");
		  var id2=$(this).attr("id");
		  var id1=last_selected.attr("id");
		  $out.fadeOut(30, function(){
		    
				
				var key_= id1+" "+id2;

			$out_func_r.html(mega[key_]["func_r"]);
			$out_rever_r.html(mega[key_]["rever_r"]);
			$out_test.html(mega[key_]["test"]);
			$out_test_i.html(mega[key_]["test-i"]);
			$out_ms_test.html(mega[key_]["ms-test"]);
			$out_func_c.html(mega[key_]["func_c"]);
			$out_rever_c.html(mega[key_]["rever_c"]);
			$out_final_test.html(mega[key_]["final_test"]);
			$out_final_coarse.html(mega[key_]["final_coarse"]);
			$out_final_i_test.html(mega[key_]["final_i_test"]);
			$out_kill_first.html(mega[key_]["kill-first"]);
			$out_kill_second.html(mega[key_]["kill-second"]);
			
			$out.fadeIn(300,function(){}	);});
			
			
		  var obj2=type_table[id2];
		  var side2=obj2["side"];var rigid2=obj2["rigid"];var coarse2=obj2["coarse"];
		   if(id1===id2)$shown2.fadeOut(300);
		   else {$shown2.fadeOut(300, function()
		   {
		      $name2.text(id2);
			  $side2.text(side2);
			  $rigid2.text(rigid2);
			  $coarse2.text(coarse2);
			  $shown2.fadeIn(300, function(){});
		   });
			}
			
		  });
	 //$list_.append(new_elem);
	 //$list_2.append(new_elem_2);
	$("#select-object-1").append(new_elem_3);
	$("#select-object-2").append(new_elem_4);
	});
	last_selected=$("#select-object-1").children().eq(0);
	
	$objects = $xml.find( "Object_" );
	$objects.each(function(){
	var self=$(this);
	var name=self.find("name").attr("value");
	var side=self.find("side").attr("value");
	var coarse=self.find("coarse").attr("value");
	var rigid=self.find("rigid").attr("value");
	var obje=new Object();
	obje["rigid"]=rigid;
	obje["coarse"]=coarse;
	obje["side"]=side;
	type_table[name]=obje;
	console.log("mesa");
	});
	console.log(type_table);
	

	//console.log($thingies);
}

);

$.ajax({
       url: "clojure-generated-data.xml"
    }).
	done
	(function( text) {
	var  xmlDoc = $.parseXML( text )
	$xml = $( text ),
    mega= new Object(),
	$list_ = $( "#lol0" ),
	$list_2 = $( "#lol1" );
    
    $thingies = $xml.find( "F-Call" ),
	$out = $("#all-funs");
	//var string_o="";
	
	
	$thingies.each(function()
	{
	 var self=$(this);
	 var func_r= self.find("rigid-test").attr("value");
	 var rever_r= self.find("reverse-rigid").attr("value");	 
	 var func_c= self.find("coarse-test").attr("value");
	 var rever_c= self.find("reverse-coarse").attr("value");	 
	 
	 var test= self.find("test").attr("value");	 
	 var test_i= self.find("test-i").attr("value");
	 var ms_test= self.find("ms-test").attr("value");	 
	 
	 var t1= self.find("type1").attr("value");	 
	 var t2= self.find("type2").attr("value");	 
     
	 var final_test= self.find("final-test").attr("value");	 
	 var final_i_test= self.find("final-intersect").attr("value");
	 var final_coarse= self.find("final-coarse").attr("value");	 
	 var kill_first= self.find("kill-first").attr("value");
	 var kill_second= self.find("kill-second").attr("value");	 
	 
	 var obje_v=
	 {
	 "func_r":func_r,
	 "rever_r":rever_r,
	 "func_c":func_c,
	 "rever_c":rever_c,
	 "test":test,
	 "test-i":test_i,
	 "ms-test":ms_test,
	 "final_test":final_test,
	 "final_i_test":final_i_test,
	 "final_coarse":final_coarse,
	 "kill-first":kill_first,
	 "kill-second":kill_second
	 };
	 console.log(t1+" "+func_r+" "+func_c);
	 var obje_k=t1+" "+t2;
	 
	 mega[obje_k]=obje_v;
	 
	});
	
	
	console.log(mega);
}


);

//console.log("haahahah"+last_selected.attr());



});
