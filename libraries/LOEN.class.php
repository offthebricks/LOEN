<?php
/*
Copyright 2020 OffTheBricks - https://github.com/offthebricks/LOEN

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

 http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

//LOEN - Lean Object Encoding Notation

class LOEN{
	private $compressionEnabled = TRUE;
	private $escapeLineEndings = TRUE;
	
	public function __construct($enableArrayCompression=TRUE,$enableEscapeLineEndings=TRUE){
		$this->compressionEnabled = $enableArrayCompression;
		$this->escapeLineEndings = $enableEscapeLineEndings;
	}
	
	private static function isAlphaNumeric($str){
		//zero length strings need to return as true in LOEN
		//on linux a variable of type int will always return false, so make it a string first
		if(!strlen($str) || ctype_alnum("".$str)){
			return TRUE;
		}
		return FALSE;
	}
	
	private static function isAssocArr(array $arr){
		if($arr === []){
			return FALSE;
		}
		return array_keys($arr) !== range(0, count($arr) - 1);
	}
	
	private static function skipWhitespace(&$str){
		$pos = 0; $check = "";
		
		do{
			if($pos >= strlen($str)){
				throw new Exception("missing closing character - end of encoding found");
			}
			$check = substr($str,$pos,1);
			$pos++;
		}while(!strlen(trim($check)));
		
		if($pos > 1){
			$str = substr($str,$pos-1);
		}
		
		return $check;
	}
	
###################################################################
	
	public static function encode($obj,$compressArrays=TRUE,$escapeLineEndings=TRUE){
		return self::encodeObject($obj,$compressArrays,$escapeLineEndings);
	}
	
	public static function stringify($obj,$compressArrays=TRUE,$escapeLineEndings=TRUE){
		return self::encodeObject($obj,$compressArrays,$escapeLineEndings);
	}
	
	private static function encodeObject($obj,$compressArrays,$escapeLineEndings){
		$loen = new self($compressArrays,$escapeLineEndings);
		return $loen->doEncode($obj);
	}
	
	public function doEncode($obj){
		$str = "";
		
		if($obj === NULL){
			$str .= "=n";
		}
		else if($obj === TRUE){
			$str .= "=t";
		}
		else if($obj === FALSE){
			$str .= "=f";
		}
		else if(is_object($obj)){
			$list = get_object_vars($obj);
			foreach($list as $prop => $value){
				if($str){
					$str .= ",";
				}
				$tmp = $this->doEncode($value);
				//if the value is already prefix with a non-alphanumeric characer, don't need the colon
				if(!self::isAlphaNumeric(substr($tmp,0,1))){
					$str .= $this->escapeString($prop).$tmp;
				}
				else{
					$str .= $this->escapeString($prop).":".$tmp;
				}
			}
			$str = "{".$str."}";
		}
		else if(is_array($obj)){
			//check if array does not have sequential numeric keys
			if(self::isAssocArr($obj)){
				$str = $this->doEncode((object)$obj);
			}
			else{
				$str = $this->encode_array($obj);
			}
		}
		else if(is_numeric($obj) && !is_string($obj)){
			if($obj >= 0){
				$obj = "+".$obj;
			}
			$str = $obj;
		}
		else if(is_string($obj)){
			$str = ":".$this->escapeString($obj);
		}
		else{
			throw new \Exception("unknown type passed for encoding");
		}
		return $str;
	}
	
	private function encode_array($arr){
		if(!$arr){
			return "";
		}
		if(sizeof($arr) == 1){
			return "[".$this->doEncode($arr[0])."]";
		}
		$str = NULL;
		$keys = []; $compress = $this->compressionEnabled;
		if($compress){
			for($i=0; $i<2; $i++){
				if(is_object($arr[$i])){
					$check = get_object_vars($arr[$i]);
				}
				else if(is_array($arr[$i]) && self::isAssocArr($arr[$i])){
					$check = $arr[$i];
				}
				else{
					$compress = FALSE;
					break;
				}
				//check if keys are the same in the first two array values
				if($i == 0){
					$keys = array_keys($check);
					continue;
				}
				$check = array_keys($check);
				foreach($keys as $v => $key){
					if(!isset($check[$v]) || $key != $check[$v]){
						$compress = FALSE;
						break;
					}
				}
			}
			if($compress){
				foreach($keys as $key){
					$tmp = $this->escapeString($key);
					if($str === NULL){
						$str = "";
					}
					if(self::isAlphaNumeric(substr($tmp,0,1))){
						$tmp = ":".$tmp;
					}
					$str .= $tmp;
				}
				$str = "[".$str."]";
			}
		}
		foreach($arr as $i => $val){
			if($str === NULL){
				$str = "";
			}
			if($compress){
				$substr = NULL;
				foreach($keys as $key){
					if(is_object($val)){
						$tmp = $this->doEncode($val->$key);
					}
					else{
						$tmp = $this->doEncode($val[$key]);
					}
					if($substr === NULL){
						$substr = "";
					}
					if(self::isAlphaNumeric(substr($tmp,0,1))){
						//assume this is a string
						$tmp = ":".$tmp;
					}
					$substr .= $tmp;
				}
				$str .= "[".$substr."]";
			}
			else{
				$str .= $this->doEncode($val);
			}
		}
		if($compress){
			return "<".$str.">";
		}
		return "[".$str."]";
	}
	
	private function escapeString($str){
		if(!self::isAlphaNumeric($str)){
			//escape all double quotes
			$str = '"'.str_replace('"','\"',$str).'"';
			if($this->escapeLineEndings){
				//escape all newlines
				$str = str_replace("\n","\\n",$str);
				//escape all carriage returns
				$str = str_replace("\r","\\r",$str);
			}
		}
		return $str;
	}
	
###################################################################
	
	public static function decode($str){
		if(!strlen($str)){
			return "";
		}
		try{
			return self::parseSegment($str);
		}
		catch(\Exception $e){
			if($e->getMessage()){
				throw $e;
			}
			throw new \Exception("error in encoding - invalid separator or closing - ".substr($str,0,15)."...");
		}
	}
	public static function parse($str){
		return self::decode($str);
	}
	
	private static function parseSegment(&$str){
		$type = self::skipWhitespace($str);
		if(!self::isAlphaNumeric($type)){
			$str = substr($str,1);
			switch($type){
				case "{":
					return self::parseObject($str);
				case "[":
					return self::parseArray($str);
				case "<":
					return self::parseCompressedArray($str);
				case '"':
					return self::parseQuotedString($str);
				case ":":
					break;
				case "-":
					$str = "-".$str;
				case "+":
				case "=":
					return self::parseValue($str,TRUE);
				//catch content closing or separator
				case ",":
				case "}":
				case "]":
				case ">":
					//need to restore 'type' to signal the calling code
					$str = $type.$str;
					throw new \Exception("");
				default:
					throw new \Exception("invalid segment character: ($type) ".substr($str,0,15)."...");
					break;
			}
		}
		
		return self::parseValue($str);
	}
	
	private static function parseValue(&$str,$equalflag=FALSE){
		//check for next symbol
		$i = 0;
		$res = "";
		$char = self::skipWhitespace($str);
		switch($char){
			case '"':
			case '{':
			case '[':
				return self::parseSegment($str);
			case '-':
			case '.':
				break;
			default:
				if(!self::isAlphaNumeric($char)){
					return "";
				}
				break;
		}
		$str = substr($str,1);
		$strlen = strlen($str);
		while((self::isAlphaNumeric($char) || $char == "." || (!strlen($res) && $char == "-")) && $i <= $strlen){
			$res .= $char;
			$char = substr($str,$i,1);
			$i++;
		}
		$str = substr($str,strlen($res)-1);
		if($equalflag){
			if($res == "n" || $res == "null"){
				return NULL;
			}
			if($res == "t" || $res == "true"){
				return TRUE;
			}
			if($res == "f" || $res == "false"){
				return FALSE;
			}
			if(!is_numeric($res)){
				throw new \Exception("invalid value found in numeric field: ($res)");
			}
			return (float)$res;
		}
		return $res;
	}
	
	private static function parseQuotedString(&$str){
		$res = NULL;
		$pos = 0;
		do{
			$pos = strpos($str,'"',$pos);
			//check if quote is preceded by a backslash '\' - might not handle double backslash before quote
			if(substr($str,$pos-1,1) == "\\"){
				//increment pos so to look for the next double quote
				$pos++;
			}
			//no slash so grab value
			else{
				$res = substr($str,0,$pos);
				$str = substr($str,$pos+1);
			}
		}while($res === NULL);
		//replace all escaped double quotes with regular double quotes
		$res = str_replace('\"','"',$res);
		//replace all escaped newlines with regular newlines
		$res = str_replace("\\n","\n",$res);
		//replace all escaped carriage returns with regular carriage returns
		$res = str_replace("\\r","\r",$res);
		
		return $res;
	}
	
	private static function parseObject(&$str){
		$res = new \stdClass();
		$check = "";
		
		do{
			//handle any whitespace or commas before the property name
			$prop = "";
			if(!strlen(trim($check)) || !self::isAlphaNumeric($check)){
				$check = self::skipWhitespace($str);
			}
			else{
				$prop = $check;
				$check = substr($str,0,1);
			}
			//if the property is double quoted
			if($check == '"'){
				$str = substr($str,1);
				$prop = self::parseQuotedString($str);
			}
			//if this is a normal property without quotes
			else{
				//look for first non-alphanumeric value to isolate the property name
				while(self::isAlphaNumeric($check)){
					$prop .= $check;
					$str = substr($str,1);
					
					if(!strlen($str)){
						throw new \Exception("error in encoding - missing closing '}' - end of encoding found");
					}
					$check = trim(substr($str,0,1));
				}
			}
			if(strlen($prop)){
				try{
					$tmp = self::parseSegment($str);
					$res->$prop = $tmp;
				}
				catch(\Exception $e){
					if($e->getMessage()){
						throw $e;
					}
					$check = substr($str,0,1);
					if($check == ","){
						$str = substr($str,1);
					}
					else if($check != "}"){
						throw new \Exception("error in encoding detected (expected '}') - ".substr($str,0,15)."...");
					}
				}
			}
			$check = substr($str,0,1);
			$str = substr($str,1);
		}while($check != "}");
		
		return $res;
	}
	
	private static function parseArray(&$str){
		$res = [];
		do{
			try{
				$tmp = self::parseSegment($str);
				$res[] = $tmp;
			}
			catch(\Exception $e){
				if($e->getMessage()){
					throw $e;
				}
				$check = substr($str,0,1);
				if($check == ","){
					$str = substr($str,1);
				}
				else if($check != "]"){
					throw new \Exception("error in encoding detected (expected ']', got '$check') - ".substr($str,0,15)."...");
				}
			}
		}while(substr($str,0,1) != "]");
		$str = substr($str,1);
		return $res;
	}
	
	private static function parseCompressedArray(&$str){
		$res = $arr = $keys = [];
		do{
			if(!$keys){
				try{
					$keys = self::parseSegment($str);		//will always be array
				}
				catch(\Exception $e){
					if($e->getMessage()){
						throw $e;
					}
					throw new \Exception("error in compressed array keys decoding - ".substr($str,0,15)."...");
				}
			}
			else{
				try{
					$tmp = self::parseSegment($str);		//will always be array
					$arr[] = $tmp;
				}
				catch(\Exception $e){
					if($e->getMessage()){
						throw $e;
					}
					$check = substr($str,0,1);
					if($check == ","){
						$str = substr($str,1);
					}
					else if($check != ">"){
						throw new \Exception("error in encoding detected (expected '>') - ".substr($str,0,15)."...");
					}
				}
			}
		}while(substr($str,0,1) != ">");
		$str = substr($str,1);
		
		foreach($arr as $data){
			$row = [];
			foreach($keys as $i => $key){
				if(!array_key_exists($i,$data)){
					throw new \Exception("missing key ($key) in compressed array");
				}
				$row[$key] = $data[$i];
			}
			$res[] = (object)$row;
		}
		return $res;
	}
}
?>
