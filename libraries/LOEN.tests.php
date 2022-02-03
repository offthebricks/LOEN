<?php
//Test models
class ShortList{
	public int $id = 1;
	public string $name = "test person";
	public string $label = "nospacehere";
	public string $property = "isprop";
	
	public function __construct(int $idx){
		$this->id = $idx;
		$this->label .= $idx;
	}
}

class FullyLoaded{
	public bool $boolfalse = false;
	public bool $booltrue = true;
	
	public int $posint = 516;
	public int $negint = -732;
	public int $zeroint = 0;

	public float $posfloat = 51.67;
	public float $negfloat = -83.21;
	public float $zerofloat = 0;

	public string $strnowhitespace = "TestPerson";
	public string $stroneline = "abc is like 123";
	public string $strmultiline = 
"abc
is like
123";
	public string $strempty = "";

	public ShortList $objshortlist;

	public $arrstr = [];
	public $arrshortlist = [];
	public $arrdictionary = [];

	public $dicint;
	public $dicstr;
	public $dicshortlist;
	
	public function __construct(bool $initialize){
		if(!$initialize){
			return;
		}
		$this->objshortlist = new ShortList(123);

		$this->arrstr = [];
		$this->arrstr[0] = $this->strnowhitespace;
		$this->arrstr[1] = $this->stroneline;
		$this->arrstr[2] = $this->strmultiline;
		$this->arrstr[3] = $this->strempty;
		$this->arrshortlist = [];
		$this->arrshortlist[0] = new ShortList(0);
		$this->arrshortlist[1] = new ShortList(1);
		$this->arrshortlist[2] = new ShortList(2);
		$this->arrshortlist[3] = new ShortList(3);
		$this->arrshortlist[4] = new ShortList(4);

		$this->arrdictionary = [];
		for($v=0; $v<5; $v++){
			$tmpdic = [];
			for ($i = 3; $i < 6; $i++){
				$tmpdic["idx = ".$i] = "value is ".($v + $i + 10);
			}
			$this->arrdictionary[] = $tmpdic;
		}

		$this->dicint = [];
		for ($i = 2; $i < 7; $i++){
			$this->dicint[$i] = $i + 10;
		}

		$this->dicstr = [];
		for ($i = 3; $i < 8; $i++){
			$this->dicstr["idx = ".$i] = "value is ".($i + 10);
		}

		$this->dicshortlist = [];
		$this->dicshortlist["list 1"] = new ShortList(1);
		$this->dicshortlist["list 2"] = new ShortList(2);
		$this->dicshortlist["list 3"] = new ShortList(3);
		$this->dicshortlist["list 4"] = new ShortList(4);
	}
}

class TestUtilities{
	public static function isAlphaNumeric($str){
		//zero length strings need to return as true in LOEN
		//on linux a variable of type int will always return false, so make it a string first
		if(!strlen($str) || ctype_alnum("".$str)){
			return TRUE;
		}
		return FALSE;
	}
	
	public static function escapeString($str, $escapeLineEndings = true){
		if(!self::isAlphaNumeric($str)){
			//escape all double quotes
			$str = '"'.str_replace('"','\"',$str).'"';
			if($escapeLineEndings){
				//escape all newlines
				$str = str_replace("\n","\\n",$str);
				//escape all carriage returns
				$str = str_replace("\r","\\r",$str);
			}
		}
		return $str;
	}
}

class EncoderTests{
	public static function TestInts(){
		$testObj = new FullyLoaded(true);
		
		$val = LOEN::encode($testObj->posint);
		TestExecuter::AssertAreEqual("+".$testObj->posint, $val);

		$val = LOEN::encode($testObj->negint);
		TestExecuter::AssertAreEqual($testObj->negint, $val);

		$val = LOEN::encode($testObj->zeroint);
		TestExecuter::AssertAreEqual("+0", $val);
	}
	
	public static function TestFloats(){
		$testObj = new FullyLoaded(true);
		
		$val = LOEN::encode($testObj->posfloat);
		TestExecuter::AssertAreEqual("+".$testObj->posfloat, $val);

		$val = LOEN::encode($testObj->negfloat);
		TestExecuter::AssertAreEqual($testObj->negfloat, $val);

		$val = LOEN::encode($testObj->zerofloat);
		TestExecuter::AssertAreEqual("+0", $val);
	}
	
	public static function TestStrings(){
		$testObj = new FullyLoaded(true);
		
		$val = LOEN::encode($testObj->strnowhitespace);
		TestExecuter::AssertAreEqual(":".$testObj->strnowhitespace, $val);

		$val = LOEN::encode($testObj->stroneline);
		TestExecuter::AssertAreEqual(":".TestUtilities::escapeString($testObj->stroneline), $val);

		$val = LOEN::encode($testObj->strmultiline);
		TestExecuter::AssertAreEqual(":".TestUtilities::escapeString($testObj->strmultiline), $val);

		$val = LOEN::encode($testObj->strempty);
		TestExecuter::AssertAreEqual(":", $val);
	}
	
	public static function TestObjectShortList(){
		$testObj = new ShortList(237);
		$check = "";

		$val = LOEN::encode($testObj);

		foreach(get_object_vars($testObj) as $prop => $value){
			if($check != ""){
				$check .= ",";
			}
			$check .= $prop.LOEN::encode($value);
		}
		$check = "{" . $check . "}";

		TestExecuter::AssertAreEqual($check, $val);
	}
	
	public static function TestStandardArray(){
		$testObj = new FullyLoaded(true);
		$check = "";

		foreach($testObj->arrstr as $item){
			if($item === ""){
				$check .= ":";
			}
			else{
				$check .= LOEN::encode($item);
			}
		}
		$check = "[" . $check . "]";

		$val = LOEN::encode($testObj->arrstr);

		TestExecuter::AssertAreEqual($check, $val);
	}
	
	public static function TestCompressedArray(){
		$testObj = new FullyLoaded(true);
		$shortlistObj = new ShortList(0);
		$check = "";

		$val = LOEN::encode($testObj->arrshortlist);

		$list = get_object_vars($shortlistObj);
		foreach($list as $prop => $value){
			$check .= ":" . $prop;
		}
		$check = "[" . $check . "]";

		$i = 0;
		foreach($testObj->arrshortlist as $obj){
			$list = get_object_vars(new ShortList($i));
			$i++;
			$subcheck = "";
			foreach($list as $prop => $value){
				$subcheck .= LOEN::encode($value);
			}
			$check .= "[" . $subcheck . "]";
		}

		$check = "<" . $check . ">";

		TestExecuter::AssertAreEqual($check, $val);
	}
	
	public static function TestDictionaries(){
		$testObj = new FullyLoaded(true);
		$check = "";

		$val = LOEN::encode($testObj->dicint);
		for($i = 2; $i < 7; $i++){
			if($i > 2){
				$check .= ",";
			}
			$check .= $i . "+" . ($i + 10);
		}
		$check = "{" . $check . "}";

		TestExecuter::AssertAreEqual($check, $val);

		$val = LOEN::encode($testObj->dicstr);
		$check = "";
		for($i = 3; $i < 8; $i++){
			if($i > 3){
				$check .= ",";
			}
			$check .= "\"idx = " . $i . "\":\"value is " . ($i + 10) . "\"";
		}
		$check = "{" . $check . "}";

		TestExecuter::AssertAreEqual($check, $val);

		$val = LOEN::encode($testObj->dicshortlist);
		$check = "";
		foreach($testObj->dicshortlist as $key => $value){
			if(strlen($check)){
				$check .= ",";
			}
			$check .= TestUtilities::escapeString($key) . LOEN::encode($value);
		}
		$check = "{" . $check . "}";

		TestExecuter::AssertAreEqual($check, $val);
	}
}

class DecoderTests{
	public static function TestFullyLoaded(){
		$testObj = new FullyLoaded(true);
		$check = json_encode($testObj);
		$testObj = LOEN::encode($testObj);
		$testObj = LOEN::decode($testObj);
		$val = json_encode($testObj);

		TestExecuter::AssertAreEqual($check, $val);
	}
}

class TestExecuter{
	private $TestClasses = [
		"EncoderTests",
		"DecoderTests"
	];
	
	public function __construct(){
		
	}
	
	public static function ExecuteTest($test){
		if(!$test){
			//run all tests
			$obj = new TestExecuter();
			$testlist = $obj->GetTestList();
			$result = "success";
			foreach($testlist as $key => $tests){
				foreach($tests as $testname){
					$check = self::ExecuteTest($key."_".$testname);
					if($check != "success"){
						$result = $check;
						break;
					}
				}
			}
			return $result;
		}
		list($key, $testname) = explode("_", $test);
		
		//run the test
		try{
			$key::$testname();
			$result = "success";
		}
		catch(Exception $e){
			$result = "<b>$test</b> ".$e->getMessage();
		}
		return $result;
	}
	
	public function GetTestList(){
		$list = [];
		foreach($this->TestClasses as $key){
			$list[$key] = get_class_methods($key);
		}
		return $list;
	}
	
	public static function AssertAreEqual($expected, $checkval){
		if($expected === $checkval){
			return;
		}
		//values don't match so return some details
		throw new Exception("failed<br/><b>Expected</b><br/><pre>$expected</pre><b>Actual</b><pre>$checkval</pre>");
	}
}

//check if one or more tests are set to be run
if(isset($_GET['test'])){
	//there is at least one test, so run and exit
	
	$test = $_GET['test'];
	if(!$test){
		echo "<h3>Test: Run All</h3>";
	}
	else{
		echo "<h3>Test: $test</h3>";
	}
	
	//prepare for the test
	include_once("LOEN.class.php");
	$starttime = microtime(true);
	$result = TestExecuter::ExecuteTest($test);
	//output the result and time elapsed
	echo "<h4>Elapsed: ".round(microtime(true) - $starttime, 6)." s</h4>";
	echo "Result:<br/>".$result;
	exit;
}
//no test(s), so show the unit test interface
?><!doctype html>
<html>
	<head>
		<title>LOEN Unit Tests</title>
		<meta charset="utf-8"/>
		<style type="text/css">
			.righttd{
				text-align: center;
			}
			table{
				width: 100%;
			}
			td{
				vertical-align: top;
			}
			iframe{
				margin-left: 25px;
				min-height: 500px;
			}
		</style>
	</head>
	<body>
		<h1>LOEN Unit Tests</h1>
		<a href="?test" target="results">Run All</a>
		<br/>
		<table><tr><td>
			<?php
			$exec = new TestExecuter();
			$list = $exec->GetTestList();
			foreach($list as $key => $tests){
				echo "<h2>$key</h2>";
				foreach($tests as $test){
					echo ": <a href='?test=".urlencode($key."_".$test)."' target='results'>$test</a><br/>";
				}
			}
			?>
		</td><td class="righttd">
			<iframe name="results"></iframe>
		</td></tr></table>
	</body>
</html>
