using System;
using System.Collections;
using Gtk;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using lolterpreter_2000;

namespace lolterpreter_2000
{
	public class SyntaxAnalyzer
	{

		public static void analyze(List<Token> tokens, MainWindow window){

			Stack arithStack = new Stack();
			window.var_basket = new Dictionary<string, string>();

			/* Initialize Implicit Variable */
			window.var_basket.Add("IT", "[null]");
			window.addToSymbolTable("IT", "[null]");

			window.clearConsole ();

			/*********************************** SYNTAX ANALYSIS ***********************************/
			int line = 1;
			// flags
			bool pass = true;
			string[] keywords = {
				"I",
				"HAS",
				"A",
				"SUM",
				"DIFF",
				"PRODUKT",
				"QUOSHUNT",
				"MOD",
				"BIGGR",
				"SMALLR",
				"BOTH",
				"EITHER",
				"WON",
				"NOT",
				"ALL",
				"ANY",
				"OF",
				"MKAY",
				"BOTH",
				"SAEM",
				"IS",
				"NOW",
				"O",
				"RLY",
				"YA",
				"RLY",
				"NO",
				"WAI",
				"IM",
				"IN",
				"YR",
				"IM",
				"OUTTA",
				"YR",
				"HOW",
				"DUZ",
				"IF",
				"U",
				"SAY",
				"SO"
			};
			/* check for wrong identifiers */
			for(int i=0, len = tokens.Count; i<len; i++){
				if (tokens [i].getCategory() == "Identifier") {
					foreach(string s in keywords){
						if(tokens [i].getLexeme() == s){
							pass = false;
							window.print ("Error: Cannot use `"+tokens [i].getLexeme()+"` as variable!");
						}
					}
				}
				else if(tokens [i].getCategory() == "not_classified"){
					pass = false;
					window.print ("Error: Invalid token `"+tokens [i].getLexeme()+"`!");
				}
			}

			bool insideIF = false; bool insideSC = false; bool search = false;
			for (int i=0, len = tokens.Count; i<len && pass; i++) {

				/****************************** SOME STUFF ******************************/
				if (tokens[i].getLexeme() == "HAI") {
					if(tokens[i+1].getLexeme() != "BTW" && tokens[i+1].getCategory() !="Line Delimiter"){
						window.clearConsole();
						window.print("Error("+line+"): Invalid keywords after HAI!");
						break;
					}else{
						continue;
					}
				}
				if(tokens[i].getLexeme() == "BTW" || tokens[i].getLexeme() == "KTHXBYE")
					continue;
				if(tokens[i].getCategory() == "Line Delimiter"){
					line++;
					continue;
				}
				if (tokens [i].getCategory() == "Start of N-line Comment") {
					while(tokens[i].getCategory()!="End of N-line Comment"){
						if(tokens[i].getCategory() == "Line Delimiter") line++;
						i++;
					}
					continue;
				}

				/******************* SWITCH FINDING CASE *******************/
				if( search && (tokens[i].getCategory() == "Case" || tokens[i].getCategory() == "Case Default")){
					if(tokens[i].getCategory() == "Case" ){
						i+=1;
						if(
							(tokens [i].getCategory () == "Integer Literal") ||
							(tokens [i].getCategory () == "Float Literal") ||
							(tokens [i].getCategory () == "String Literal") ||
							(tokens [i].getCategory () == "Boolean Literal")
						){
							if(window.var_basket["IT"] == tokens[i].getLexeme()){
								insideSC = true;
								search = false;
								continue;
							}
						}else{
							window.print("Error("+line+"): Cannot use `"+tokens[i].getLexeme()+"` as a case value for OMG!");
						}

						continue;
					}else{
						insideSC = true;
						search = false;
						continue;
					}
				}
				if( search && (tokens[i].getCategory() == "End If-Else")){
					search = false;
					continue;
				}
				if(search){
					continue;
				}

				/*********************** HANDLE CASE ***********************/
				if(insideSC && tokens[i].getCategory() == "Break Keyword"){
					insideSC = false;
					// find OIC and start there!
					try{
						while(tokens[i].getCategory() != "End If-Else"){
							if(tokens[i].getCategory() == "Line Delimiter"){
								line++;
							}
							i++;
						}
						continue;
					}catch (Exception e) {
						window.print("Error("+line+"): Missing `OIC`!");
					}
				}
				if(insideSC && (tokens[i].getCategory() == "Case" || tokens[i].getCategory() == "Case Default")){
					if(tokens[i].getCategory() == "Case"){
						i+=1;
						continue;
					}else{
						continue;
					}
				}
				if(insideSC && tokens[i].getCategory() == "End If-Else"){
					insideSC = false;
					continue;
				}


				/*********************** IF-ELSE HANDLERS ***********************/
				if(insideIF && (tokens[i].getCategory() == "Else Start" || tokens[i].getCategory() == "End If-Else")){
					insideIF = false;
					// find OIC and start there!
					while(tokens[i].getCategory() != "End If-Else"){
						if(tokens[i].getCategory() == "Line Delimiter"){
							line++;
						}
						i++;
					}
					continue;
				}

				/********************************************
				*	START OF PROPER
				********************************************/

				/*===== start of I HAS A =====*/
				if(tokens[i].getCategory() == "Variable Declaration"){
					if(tokens[i+1].getCategory() == "Identifier"){
						/* I HAS A var_ident ITZ ? */
						if (tokens [i + 2].getCategory () == "Variable Initialization") {
							/* ? = literal*/
							if (
								(tokens [i + 3].getCategory () == "Integer Literal") ||
								(tokens [i + 3].getCategory () == "Float Literal") ||
								(tokens [i + 3].getCategory () == "String Literal") ||
								(tokens [i + 3].getCategory () == "Boolean Literal")) {
								window.var_basket.Add (tokens [i + 1].getLexeme (), tokens [i + 3].getLexeme ());
								window.addToSymbolTable (tokens [i + 1].getLexeme (), tokens [i + 3].getLexeme ());
								i += 3;
							}//end of I HAS A var_ident ITZ literal

							/* ? = var_ident*/
							else if (tokens [i + 3].getCategory () == "Identifier" || tokens [i + 3].getCategory () == "Implicit Variable") {
								if (window.var_basket.ContainsKey (tokens [i + 3].getLexeme ())) {
									window.var_basket.Add (tokens [i + 1].getLexeme (), window.var_basket [tokens [i + 3].getLexeme ()]);
									window.addToSymbolTable (tokens [i + 1].getLexeme (), window.var_basket [tokens [i + 3].getLexeme ()]);
									i += 3;
								} else {
									window.print ("Error(" + line + "): '" + tokens [i + 3].getLexeme () + "' not declared!");
									break;
								}
							}//end of I HAS A var_ident ITZ var_ident

							/* ? = comparison && boolean */
							else if(
								tokens[i+3].getCategory() == "Equality Comparison"  ||
								tokens[i+3].getCategory() == "Inequality Comparison"||
								tokens[i+3].getCategory() == "Logical AND" ||
								tokens[i+3].getCategory() == "Logical OR" ||
								tokens[i+3].getCategory() == "Logical XOR" ||
								tokens[i+3].getCategory() == "Logical NOT"
							){
								var result = Comparator.performComparison(i+3, line, tokens, window);
								if (result == null) {
									break;
								}
								// store in Identifier
								window.var_basket.Add(tokens[i+1].getLexeme(), (result.Item1 == true.ToString() )? "WIN":"FAIL");
								window.addToSymbolTable(tokens[i+1].getLexeme(), window.var_basket[tokens[i+1].getLexeme()]);
								// new index
								i = result.Item2;
							}//end of I HAS A var_ident ITZ comparison or boolean

							/* ? = concatenation */
							else if(tokens[i+3].getCategory() == "String Concatenator"){
								var result = InfiniteArity.smoosh(i+3, line, tokens, window);
								if(result == null){
									break;
								}
								// store in Identifier
								window.var_basket.Add(tokens[i+1].getLexeme(), result.Item1);
								window.addToSymbolTable(tokens[i+1].getLexeme(), window.var_basket[tokens[i+1].getLexeme()]);
								// new index
								i = result.Item2;
							}//end of I HAS A var_ident ITZ concatenation

							/* ? = any_all */
							else if(tokens[i+3].getCategory() == "Infinite arity AND" || tokens[i+3].getCategory() == "Infinite arity OR"){
								var result = InfiniteArity.any_all(i+3, line, tokens, window);
								if(result == null){
									break;
								}
								// store in Identifier
								window.var_basket.Add(tokens[i+1].getLexeme(), (result.Item1 == true.ToString() )? "WIN":"FAIL");
								window.addToSymbolTable(tokens[i+1].getLexeme(), window.var_basket[tokens[i+1].getLexeme()]);
								// new index
								i = result.Item2;
							}//end of I HAS A var_ident ITZ any all

							/* ? = arithmetic_expression */
							else if(
								(tokens[i+3].getCategory() == "Addition Operator") 			||
								(tokens[i+3].getCategory() == "Subtraction Operator") 		||
								(tokens[i+3].getCategory() == "Multiplication Operator") 		||
								(tokens[i+3].getCategory() == "Division Operator") 			||
								(tokens[i+3].getCategory() =="Modulo Operator") 				||
								(tokens[i+3].getCategory() =="Max Operator")   				||
								(tokens[i+3].getCategory() =="Min Operator")){
								arithStack = ArithmeticOperations.generate_postfix(tokens, i+3, line, window);
								if(arithStack==null){
									window.print("\nError("+line+"): unbalanced stack!");//stack underflow
									break;
								}
								else if(arithStack.Count==0) break;	//different error
								else{
									int j = i+1;
									i = (int) arithStack.Pop() - 1;
									window.var_basket[tokens[j].getLexeme()] = arithStack.Pop().ToString();
									window.addToSymbolTable (tokens [j].getLexeme (), window.var_basket [tokens [j].getLexeme ()]);
								}
							}//end of I HAS A var_ident ITZ arith_exp

							else {
								window.print ("Error(" + line + "): '" + tokens [i + 3].getLexeme () + "' cannot be used to initialize!");
								break;
							}
						}
						/* I HAS A var_ident */
						else if (tokens [i + 2].getCategory () == "Line Delimiter" || tokens [i + 2].getLexeme () == "BTW") {
							if(window.var_basket.ContainsKey(tokens [i + 1].getLexeme ())){
								window.print("Error("+line+"): "+tokens[i+1].getLexeme()+" is already declared!");
								break;
							}
							window.var_basket.Add (tokens [i + 1].getLexeme (), "[null]");
							window.addToSymbolTable (tokens [i + 1].getLexeme (), window.var_basket [tokens [i + 1].getLexeme ()]);
							i += 1;
						}//end of I HAS A var_ident

						else {
							window.print("Error("+line+"): "+tokens[i+2].getLexeme()+" invalid keyword!");
							break;
						}
					}else{
						window.print("Error("+line+"): Invalid Variable Identifier!");
						break;
					}
				}/*===== end of I HAS A =====*/

				/*===== var R ? =====*/
				else if(tokens[i].getCategory() == "Identifier" || tokens[i].getCategory() == "Implicit Variable"){
					if(tokens[i+1].getCategory() == "Variable Assignment"){
						/* ? = literal */
						if(
							(tokens[i+2].getCategory() == "Integer Literal") ||
							(tokens[i+2].getCategory() == "Float Literal") ||
							(tokens[i+2].getCategory() == "String Literal") ||
							(tokens[i+2].getCategory() == "Boolean Literal")
						){
							/* IF EXIST */
							if(window.var_basket.ContainsKey(tokens[i].getLexeme())){
								window.var_basket[tokens[i].getLexeme()] = tokens[i+2].getLexeme();
								window.addToSymbolTable(tokens[i].getLexeme(), tokens[i+2].getLexeme());
								i += 2;
							}else{
								window.print("Error("+line+"): '"+tokens[i].getLexeme()+"' not declared!");
								break;
							}
						}//end of VAR R literal

						/* ? = var_ident */
						else if( tokens[i+2].getCategory() == "Identifier" || tokens[i+2].getCategory() == "Implicit Variable" ){
							if(window.var_basket.ContainsKey(tokens[i+2].getLexeme())){
								/* IF EXIST */
								if(window.var_basket.ContainsKey(tokens[i].getLexeme())){
									window.var_basket[tokens[i].getLexeme()] = window.var_basket[tokens[i+2].getLexeme()];
									window.addToSymbolTable(tokens[i].getLexeme(), window.var_basket[tokens[i+2].getLexeme()]);
									i += 2;
								}else{
									window.print("Error("+line+"): '"+tokens[i].getLexeme()+"' not declared!");
									break;
								}
							}else{
								window.print("Error("+line+"): '"+tokens[i+2].getLexeme()+"' not declared!");
								break;
							}
						}//end of VAR R var_ident

						/* ? = boolean or comparison */
						else if(
							tokens[i+2].getCategory() == "Equality Comparison"  ||
							tokens[i+2].getCategory() == "Inequality Comparison"||
							tokens[i+2].getCategory() == "Logical AND" ||
							tokens[i+2].getCategory() == "Logical OR" ||
							tokens[i+2].getCategory() == "Logical XOR" ||
							tokens[i+2].getCategory() == "Logical NOT"
						){
							var result = Comparator.performComparison(i+2, line, tokens, window);
							if (result == null) {
								break;
							}

							// store in Identifier
							window.var_basket[tokens[i].getLexeme()] = (result.Item1 == true.ToString() )? "WIN":"FAIL";
							window.addToSymbolTable(tokens[i].getLexeme(), window.var_basket[tokens[i].getLexeme()]);
							// new index
							i = result.Item2;
						}//end of VAR R boolean or comparison

						/* arity */
						
							else if(tokens[i+2].getCategory() == "Infinite arity AND" || tokens[i+2].getCategory() == "Infinite arity OR"){
								var result = InfiniteArity.any_all(i+2, line, tokens, window);
								if(result == null){
									break;
								}
								try{
									// store in Identifier
									window.var_basket[tokens[i].getLexeme()] = (result.Item1 == true.ToString() )? "WIN":"FAIL";
									window.addToSymbolTable(tokens[i].getLexeme(), window.var_basket[tokens[i].getLexeme()]);
									// new index
									i = result.Item2;
								}
								catch{
									window.print("Something wrong!!");
								}
								
							}

						
						/* ? = string concatenation */
						else if(tokens[i+2].getCategory() == "String Concatenator" ){
							var result = InfiniteArity.smoosh(i+2, line, tokens, window);
							if (result == null) {
								break;
							}
							// store in Identifier
							window.var_basket[tokens[i].getLexeme()] = result.Item1;
							window.addToSymbolTable(tokens[i].getLexeme(), window.var_basket[tokens[i].getLexeme()]);
							// new index
							i = result.Item2;
						}//end of VAR R string concatenation

						/* ? = arithmetic_expression */
						else if(
							(tokens[i+2].getCategory() == "Addition Operator") 			||
							(tokens[i+2].getCategory() == "Subtraction Operator") 		||
							(tokens[i+2].getCategory() == "Multiplication Operator") 		||
							(tokens[i+2].getCategory() == "Division Operator") 			||
							(tokens[i+2].getCategory() =="Modulo Operator") 				||
							(tokens[i+2].getCategory() =="Max Operator")   				||
							(tokens[i+2].getCategory() =="Min Operator")){
							arithStack = ArithmeticOperations.generate_postfix(tokens, i+2, line, window);
							if(arithStack==null){
								window.print("\nError("+line+"): unbalanced stack!");//stack underflow
								break;
							}
							else if(arithStack.Count==0) break;	//different error
							else{
								/* IF EXIST */
								if(window.var_basket.ContainsKey(tokens[i].getLexeme())){
									int j = i; //if var R arith_exp
									i = (int) arithStack.Pop() - 1;
									window.var_basket[tokens[j].getLexeme()] = arithStack.Pop().ToString();
									window.addToSymbolTable (tokens [j].getLexeme (), window.var_basket [tokens [j].getLexeme ()]);
								}
								else{
									window.print("\nError("+line+"): '"+tokens[i].getLexeme()+"' not declared!");
									break;
								}
							}
						}//end of VAR R arith_exp

						else{
							window.print("Error("+line+"): '"+tokens[i+2].getLexeme()+"' cannot be assign to "+tokens[i].getLexeme()+"!");
							break;
						}
					}
					/* VAriable lang, store sa it */
					else if(tokens[i+1].getCategory() == "Line Delimiter" || tokens[i+1].getLexeme() == "BTW"){
						if (window.var_basket.ContainsKey (tokens [i].getLexeme ())) {
							window.var_basket["IT"] = window.var_basket[tokens [i].getLexeme ()];
						} else {
							window.print("\nError("+line+"): '"+tokens[i].getLexeme()+"' not declared!");
							break;
						}
					}
					else{
						window.print("Error("+line+"): "+tokens[i+2].getLexeme()+" is an invalid argument!");
						break;
					}
				}/*===== end of var R ? =====*/

				/*===== start of VISIBLE ? =====*/
				else if (tokens[i].getCategory() == "Output Function"){
					while(tokens[i].getLexeme()!="\n"){
						/* ? = var_ident */
						if(tokens[i+1].getCategory() == "Identifier" || tokens[i+1].getCategory() == "Implicit Variable" ){
							if(window.var_basket.ContainsKey(tokens[i+1].getLexeme())){						
								window.print(window.var_basket[tokens[i+1].getLexeme()].Replace("\"", ""));
								i+=1;
							}else{
								window.print("Error("+line+"): '"+tokens[i+1].getLexeme()+"' is undefined!");
								break;
							}
						}//end of VISIBLE var_ident

						/* ? = literal */
						else if(
								(tokens[i+1].getCategory() == "Integer Literal") ||
								(tokens[i+1].getCategory() == "Float Literal")   ||
								(tokens[i+1].getCategory() == "String Literal")  ||
								(tokens[i+1].getCategory() == "Boolean Literal")
							){						
							window.print(tokens[i+1].getLexeme().Replace("\"", ""));
							i+=1;				
						}//end of VISIBLE literal


						/* ? = comparison or boolean */
						else if(
								tokens[i+1].getCategory() == "Equality Comparison"  ||
								tokens[i+1].getCategory() == "Inequality Comparison"||
								tokens[i+1].getCategory() == "Logical AND" ||
								tokens[i+1].getCategory() == "Logical OR" ||
								tokens[i+1].getCategory() == "Logical XOR" ||
								tokens[i+1].getCategory() == "Logical NOT"
							){
							var result = Comparator.performComparison(i+1, line, tokens, window);
							if (result == null) {
								break;
							}
							i = result.Item2;
							window.print((result.Item1 == true.ToString()? "WIN": "FAIL"));						
						}//end of VISIBLE boolean or comparison

						/*===== start of Arity Logic =====*/
						else if(tokens[i+1].getCategory() == "Infinite arity AND" || tokens[i+1].getCategory() == "Infinite arity OR"){
							var result = InfiniteArity.any_all(i+1, line, tokens, window);
							if (result == null) {
								break;
							}
							// new index
							i = result.Item2;
							// store in it				
							window.print((result.Item1 == true.ToString()? "WIN": "FAIL"));
						}
						/*===== end of Arity Logic =====*/

						/* ? = string concatenation */
						else if(tokens[i+1].getCategory() == "String Concatenator"){
							var result = InfiniteArity.smoosh(i+1, line, tokens, window);
							if (result == null) {
								break;
							}
							i = result.Item2;						
							window.print(result.Item1.Replace("\"", ""));						
						}//end of VISIBLE smoosh


						/* ? = arithmetic_expression */
						else if(
							(tokens[i+1].getCategory() == "Addition Operator") 			||
							(tokens[i+1].getCategory() == "Subtraction Operator") 		||
							(tokens[i+1].getCategory() == "Multiplication Operator") 		||
							(tokens[i+1].getCategory() == "Division Operator") 			||
							(tokens[i+1].getCategory() =="Modulo Operator") 				||
							(tokens[i+1].getCategory() =="Max Operator")   				||
							(tokens[i+1].getCategory() =="Min Operator")){
							arithStack = ArithmeticOperations.generate_postfix(tokens, i+1, line, window);
							if(arithStack==null){
								window.print("\nError("+line+"): unbalanced stack!");//stack underflow
								break;
							}
							else if(arithStack.Count==0) break;	//different error
							else{
								i = (int) arithStack.Pop() - 1;
								window.print(arithStack.Pop().ToString());							
							}
						}//end of VISIBLE arith_exp

						if(tokens[i+1].getCategory () == "Newline Suppressor") {
							if(tokens[i+2].getLexeme () == "\n") {								
							}
							else{
								window.print("Error("+line+"): invalid use of newline suppressor!");
								break;
							}
						}					

						else{
							window.print("Error("+line+"): invalid argument for output!");
							break;
						}
					}//end of while loop
					if(tokens[i-1].getCategory () == "Newline Suppressor") {}
					else{ window.print("\n"); }
				}/*===== end of VISIBLE ? =====*/


				/*===== start of GIMMEH var_ident =====*/
				else if (tokens[i].getCategory() == "Input Function"){
					if(tokens[i+1].getCategory() == "Identifier" || tokens[i+1].getCategory() == "Implicit Variable"){

						if(window.var_basket.ContainsKey(tokens[i+1].getLexeme())){
							InputBox cid = new InputBox ();
							if (cid.Run () == (int)Gtk.ResponseType.Ok) {
								window.var_basket[tokens[i+1].getLexeme()] = "\""+cid.Text+"\"";
								window.addToSymbolTable( tokens[i+1].getLexeme(), window.var_basket[tokens[i+1].getLexeme()]);
							}
							cid.Destroy ();
							i += 1;
						}else{
							window.print("Error("+line+"): '"+tokens[i+1].getLexeme()+"' is undefined!");
							break;
						}
					}else{
						window.print("Error("+line+"): invalid argument for input!");
						break;
					}
				}/*===== end of GIMMEH var_ident =====*/

				/*===== start of arith_exp =====*/
				else if(
					(tokens[i].getCategory() == "Addition Operator") 			||
					(tokens[i].getCategory() == "Subtraction Operator") 		||
					(tokens[i].getCategory() == "Multiplication Operator") 		||
					(tokens[i].getCategory() == "Division Operator") 			||
					(tokens[i].getCategory() =="Modulo Operator") 				||
					(tokens[i].getCategory() =="Max Operator")   				||
					(tokens[i].getCategory() =="Min Operator")){
					arithStack = ArithmeticOperations.generate_postfix(tokens, i, line, window);
					if(arithStack==null){
						window.print("\nError("+line+"): unbalanced stack!");//stack underflow
						break;
					}
					else if(arithStack.Count==0) break;	//different error
					else{
						i = (int) arithStack.Pop() - 1;
						window.var_basket["IT"] = arithStack.Pop().ToString();
						window.addToSymbolTable ("IT", window.var_basket ["IT"]);
					}
				}/*===== end of arith_exp =====*/

				/*===== start of BOOLEAN/COMPARISON =====*/
				else if(
						tokens[i].getCategory() == "Equality Comparison"  ||
						tokens[i].getCategory() == "Inequality Comparison"||
						tokens[i].getCategory() == "Logical AND" ||
						tokens[i].getCategory() == "Logical OR" ||
						tokens[i].getCategory() == "Logical XOR" ||
						tokens[i].getCategory() == "Logical NOT"
					){
					var result = Comparator.performComparison(i, line, tokens, window);
					if (result == null) {
						break;
					}
					// new index
					i = result.Item2;
					// store in it
					window.var_basket["IT"] = (result.Item1 == true.ToString() )? "WIN":"FAIL";
					window.addToSymbolTable("IT", window.var_basket["IT"]);
				}
				/*===== end of BOOLEAN =====*/

				/*===== start of CONCATENATION =====*/
				else if(tokens[i].getCategory() == "String Concatenator"){
					var result = InfiniteArity.smoosh(i, line, tokens, window);
					if(result == null){
						break;
					}

					// store in it
					window.var_basket["IT"] = result.Item1;
					window.addToSymbolTable("IT", window.var_basket["IT"]);
					// new index
					i = result.Item2;
				}/*===== end of CONCATENATION =====*/

				/*===== start of Arity Logic =====*/
				else if(tokens[i].getCategory() == "Infinite arity AND" || tokens[i].getCategory() == "Infinite arity OR"){
					var result = InfiniteArity.any_all(i, line, tokens, window);
					if (result == null) {
						break;
					}
					// new index
					i = result.Item2;
					// store in it
					window.var_basket["IT"] = (result.Item1 == true.ToString() )? "WIN":"FAIL";
					window.addToSymbolTable("IT", window.var_basket["IT"]);
				}
				/*===== end of Arity Logic =====*/

				/*===== start of IF-ELSE =====*/
				else if(tokens[i].getCategory() == "Start If-Else"){
					/* check if IT is bool */
					if((window.var_basket["IT"] == "WIN" || window.var_basket["IT"] == "\"WIN\"" || window.var_basket["IT"] != "0") && (window.var_basket["IT"] != "FAIL")){
						i ++;
						bool err = false;
						// find YA RLY
						while(tokens[i].getCategory() != "If Start"){
							if(tokens[i].getCategory() == "Line Delimiter"){
								line++;
							}
							else if(	tokens[i].getCategory() != "1-Line Comment" &&
									tokens[i].getCategory() != "Start of N-line Comment" &&
									tokens[i].getCategory() != "End of N-line Comment"){

								window.clearConsole();
								window.print("Error("+line+"): "+tokens[i].getLexeme() +" is invalid after  `O RLY?`!");
								err = true;
								break;
							}
							i++;
						}

						if(err){
							break;
						}
						// toggle flag
						insideIF = true;
						continue;
					}else{
						i++;
						int flagforYRL = 0;
						// find NO WAI
						while(tokens[i].getCategory() != "Else Start"){
							if(tokens[i].getCategory() == "If Start"){
								flagforYRL += 1;
							}
							else if(tokens[i].getCategory() == "Line Delimiter"){
								line++;
							}
							i++;
						}
						if (flagforYRL != 1) {
							window.clearConsole();
							window.print("Error("+line+"): Cannot find `YA RLY` or multiple occurences after `O RLY?`!");
							break;
						}
						// toggle flag
						insideIF = true;
						continue;
					}
				}
				/*===== end of IF-ELSE =====*/

				/*===== start of SWITCH CASE =====*/
				else if(tokens[i].getCategory() == "Switch Case"){
					search = true;
					continue;
				}
				/*===== end of SWITCH CASE =====*/


				else if(
					(tokens[i].getCategory() == "Integer Literal") ||
					(tokens[i].getCategory() == "Float Literal")   ||
					(tokens[i].getCategory() == "String Literal")  ||
					(tokens[i].getCategory() == "Boolean Literal")
				){
					window.var_basket ["IT"] = tokens [i].getLexeme ();
				}
				/* error starting keyword */
				else{
					window.print("Error("+line+"): Invalid use of keyword `"+tokens[i].getLexeme()+"`!");
				}
			}
			// window.print("\n\n-----------------------\nLines: "+ (--line));
		}


	}
}
