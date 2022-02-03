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

var LOEN = (function(){
	var config = {
		compressArrays: true,
		compressionEnabled: function(){
			return config.compressArrays;
		}
	};
	
	var utils = {
		escapeRegExp: function(str) {
			return str.replace(/[-\[\]\/\{\}\(\)\*\+\?\.\\\^\$\|]/g, "\\$&");
		},

		replaceAll: function(string, find, replace){
			return string.replace(new RegExp(utils.escapeRegExp(find), 'g'), replace);
		},
		
		//use charCodeAt instead of regular expression for better performance
		isAlphaNumeric: function(str){
			let i, code, len = str.length;		//zero length strings return as true
			for(i=0; i<len; i++){
				code = str.charCodeAt(i);
				if (!(code > 47 && code < 58) && // numeric (0-9)
						!(code > 64 && code < 91) && // upper alpha (A-Z)
						!(code > 96 && code < 123)){ // lower alpha (a-z)
					return false;
				}
			}
			return true;
		},
		
		isNumeric: function(value){
			return !isNaN(parseFloat(value)) && isFinite(value);
		},
		
		skipWhitespace: function(){
			let pos = 0, check = "";
			
			do{
				if(pos >= dstr.length){
					throw "missing closing character - end of encoding found";
				}
				check = dstr.substring(pos,pos+1);
				pos++;
			}while(!check.trim().length);
			
			if(pos > 1){
				dstr = dstr.substring(pos-1);
			}
			
			return check;
		}
	};
	
	var encoder = {
		encode: function(obj,compressArrays){
			if(typeof(compressArrays) === 'boolean' && !compressArrays){
				config.compressArrays = false;
			}
			
			return encoder.do_encode(obj);
		},
		
		do_encode: function(obj){
			let str = "", tmp;
			
			if(obj === null){
				str += "=n";
			}
			else if(obj === true){
				str += "=t";
			}
			else if(obj === false){
				str += "=f";
			}
			else if(typeof(obj) === 'object'){
				if(Array.isArray(obj)){
					str = encoder.encode_array(obj);
				}
				else{
					let i, prop, list = Object.getOwnPropertyNames(obj);
					for(i=0; i<list.length; i++){
						prop = list[i];
						if(typeof(obj[prop]) === 'function'){
							continue;
						}
						if(str){
							str += ",";
						}
						tmp = encoder.do_encode(obj[prop]);
						//if the value is already prefix with a non-alphanumeric characer, don't need the colon
						if(!utils.isAlphaNumeric(tmp.substring(0,1))){
							str += encoder.escapeString(prop)+tmp
						}
						else{
							str += encoder.escapeString(prop)+":"+tmp
						}
					}
					str = "{"+str+"}";
				}
			}
			else if(utils.isNumeric(obj) && typeof(obj) !== 'string'){
				if(obj >= 0){
					obj = "+"+obj;
				}
				str = ""+obj;
			}
			else if(typeof(obj) === 'string'){
				str = ":" + encoder.escapeString(obj);
			}
			else if(typeof(obj) === 'function'){
				//do nothing
			}
			else{
				throw "unknown type passed for encoding";
			}
			
			return str;
		},
		
		encode_array: function(arr){
			if(!arr){
				return "";
			}
			if(arr.length == 1){
				return "["+encoder.do_encode(arr[0])+"]";
			}
			let i, v, check, tmp, str = null, substr, keys = [], compress = config.compressionEnabled();
			if(compress){
				for(i=0; i<2; i++){
					if(arr[i] && typeof(arr[i]) === 'object'){
						check = Object.getOwnPropertyNames(arr[i]);
					}
					else{
						compress = false;
						break;
					}
					//check if keys are the same in the first two array values
					if(i == 0){
						keys = check;
						continue;
					}
					for(v=0; v<keys.length; v++){
						if(typeof(check[v]) === 'undefined' || keys[v] != check[v]){
							compress = false;
							break;
						}
					}
				}
				if(compress){
					for(v=0; v<keys.length; v++){
						if(typeof(arr[0][keys[v]]) === 'function'){
							continue;
						}
						tmp = encoder.escapeString(keys[v]);
						if(str === null){
							str = "";
						}
						if(utils.isAlphaNumeric(tmp.substring(0,1))){
							tmp = ":" + tmp;
						}
						str += tmp;
					}
					str = "["+str+"]";
				}
			}
			for(i=0; i<arr.length; i++){
				if(str === null){
					str = "";
				}
				if(compress){
					substr = null;
					for(v=0; v<keys.length; v++){
						if(substr === null){
							substr = "";
						}
						//verify that this key actually exists
						if(typeof(arr[i][keys[v]]) === 'undefined'){
							throw "missing key '" + keys[v] + "' when attempting to compress an array";
						}
						tmp = encoder.do_encode(arr[i][keys[v]]);
						if(utils.isAlphaNumeric(tmp.substring(0,1))){
							//assume this is a string
							tmp = ":" + tmp;
						}
						substr += tmp;
					}
					str += "["+substr+"]";
				}
				else{
					str += encoder.do_encode(arr[i]);
				}
			}
			if(compress){
				return "<"+str+">";
			}
			if(str === null){
				str = "";
			}
			return "["+str+"]";
		},
		
		escapeString: function(str){
			if(str.length > 0 && !utils.isAlphaNumeric(str)){
				//escape all double quotes
				str = '"'+utils.replaceAll(str,'"',"\\\"")+'"';
				//escape all newlines
				str = utils.replaceAll(str,"\n","\\n");
				//escape all carriage returns
				str = utils.replaceAll(str,"\r","\\r");
			}
			return str;
		}
	};
	
	var dstr;
	var decoder = {
		decode: function(str){
			if(typeof(str) !== 'string' || !str.length){
				return "";
			}
			dstr = str;
			try{
				return decoder.parseSegment();
			}
			catch(e){
				if(e){
					throw e;
				}
				throw "error in encoding - invalid separator or closing - "+dstr.substring(0,15)+"...";
			}
		},
		
		parseSegment: function(){
			let res = "", type = utils.skipWhitespace();
			
			if(!utils.isAlphaNumeric(type)){
				dstr = dstr.substring(1);
				switch(type){
					case "{":
						return decoder.parseObject();
					case "[":
						return decoder.parseArray();
					case "<":
						return decoder.parseCompressedArray();
					case '"':
						return decoder.parseQuotedString();
					case ":":
						break;
					case "-":
						dstr = "-"+dstr;
					case "+":
					case "=":
						return decoder.parseValue(true);
					//catch content closing or separator
					case ",":
					case "}":
					case "]":
					case ">":
						//need to restore 'type' to signal the calling code
						dstr = type + dstr;
						throw "";
					default:
						throw "invalid segment character: ("+type+") "+dstr.substring(0,15)+"...";
						break;
				}
			}
			
			return decoder.parseValue(false);
		},
		
		parseValue: function(equalflag){
			let i=0; res="", chr=utils.skipWhitespace();
			switch(chr){
				case '"':
				case '{':
				case '[':
					return decoder.parseSegment();
				case '-':
				case '.':
					break;
				default:
					if(!utils.isAlphaNumeric(chr)){
						return "";
					}
					break;
			}
			dstr = dstr.substring(1);
			while((utils.isAlphaNumeric(chr) || chr === "." || (!res.length && chr === "-")) && i <= dstr.length){
				res += chr;
				chr = dstr.substring(i,i+1);
				i++;
			}
			dstr = dstr.substring(res.length-1);
			if(equalflag){
				if(res == "n" || res == "null"){
					return null;
				}
				if(res == "t" || res == "true"){
					return true;
				}
				if(res == "f" || res == "false"){
					return false;
				}
				if(!utils.isNumeric(res)){
					throw "invalid value found in numeric field: ("+res+")";
				}
				return parseFloat(res);
			}
			return res;
		},
		
		parseQuotedString: function(){
			let res=null, pos=0, check;
			do{
				pos = dstr.indexOf('"',pos);
				//check if quote is preceded by a backslash '\' - does not handle double backslash before quote
				if(dstr.substring(pos-1,pos) === "\\"){
					//increment pos so to look for the next double quote
					pos++;
				}
				//no slash so grab value
				else{
					res = dstr.substring(0,pos);
					dstr = dstr.substring(pos+1);
				}
			}while(res === null);
			//replace all escaped double quotes with regular double quotes
			res = utils.replaceAll(res,"\\\"",'"');
			//replace all escaped newlines with regular newlines
			res = utils.replaceAll(res,"\\n","\n");
			//replace all escaped carriage returns with regular carriage returns
			res = utils.replaceAll(res,"\\r","\r");
			
			return res;
		},
		
		parseObject: function(){
			let res={}, prop, check = "", tmp;
			do{
				//handle any whitespace or commas before the property name
				prop = "";
				if(!check.length || !utils.isAlphaNumeric(check)){
					check = utils.skipWhitespace();
				}
				else{
					prop = check;
					check = dstr.substring(0,1);
				}
				//if the property is double quoted
				if(check == '"'){
					dstr = dstr.substring(1);
					prop = decoder.parseQuotedString();
				}
				//if this is a normal property without quotes
				else{
					//look for first non-alphanumeric value to isolate the property name
					while(utils.isAlphaNumeric(check)){
						prop += check;
						dstr = dstr.substring(1);
						
						if(!dstr.length){
							throw "error in encoding - missing closing '}' - end of encoding found";
						}
						check = dstr.substring(0,1).trim();
					}
				}
				
				if(prop.length){
					//use parse segment to get the value of the property
					try{
						tmp = decoder.parseSegment();
						res[prop] = tmp;
					}
					catch(e){
						if(e){
							throw e;
						}
						check = dstr.substring(0,1);
						if(check == ","){
							dstr = dstr.substring(1);
						}
						else if(check != "}"){
							throw "error in encoding detected (expected '}') - "+dstr.substring(0,15)+"...";
						}
					}
				}
				check = dstr.substring(0,1);
				dstr = dstr.substring(1);
			}while(check != "}");
			
			return res;
		},
		
		parseArray: function(){
			let res=[], check, tmp;
			do{
				try{
					tmp = decoder.parseSegment();
					res.push(tmp);
				}
				catch(e){
					if(e){
						throw e;
					}
					check = dstr.substring(0,1);
					if(check == ","){
						dstr = dstr.substring(1);
					}
					else if(check != "]"){
						throw "error in encoding detected (expected ']', got '"+check+"') - "+dstr.substring(0,15)+"...";
					}
				}
			}while(dstr.substring(0,1) != "]");
			dstr = dstr.substring(1);
			return res;
		},
		
		parseCompressedArray: function(){
			let v, i, row, check, res=[], arr=[], keys=[];
			do{
				if(!keys.length){
					try{
						keys = decoder.parseSegment();		//will always be array
					}
					catch(e){
						if(e){
							throw e;
						}
						if(keys === false){
							throw "error in compressed array keys decoding - "+dstr.substring(0,15)+"...";
						}
					}
				}
				else{
					try{
						tmp = decoder.parseSegment();
						arr.push(tmp);		//will always be array
					}
					catch(e){
						if(e){
							throw e;
						}
						check = dstr.substring(0,1);
						if(check == ","){
							dstr = dstr.substring(1);
						}
						else if(check != ">"){
							throw "error in encoding detected (expected '>') - "+dstr.substring(0,15)+"...";
						}
					}
				}
			}while(dstr.substring(0,1) != ">");
			dstr = dstr.substring(1);
			for(v=0; v<arr.length; v++){
				row = {};
				for(i=0; i<keys.length; i++){
					row[keys[i]] = arr[v][i];
				}
				res.push(row);
			}
			return res;
		}
	};
	
	return {
		encode: function(obj,compressArrays){
			return encoder.encode(obj,compressArrays);
		},
		stringify: function(obj,compressArrays){
			return encoder.encode(obj,compressArrays);
		},
		
		decode: function(str){
			return decoder.decode(str);
		},
		parse: function(str){
			return decoder.decode(str);
		}
	};
})();
