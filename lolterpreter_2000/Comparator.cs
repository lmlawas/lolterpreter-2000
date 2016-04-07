using System;
using System.Collections;
using Gtk;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using lolterpreter_2000;

namespace lolterpreter_2000
{
	public class Comparator
	{
		public static Tuple<string, int> performComparison(int index, int line, List<Token> tokens, MainWindow window){
			List<Token> expression = new List<Token>();
			Stack calculator = new Stack();
			int j = index;
			int flag = j-1;
			while(tokens[j].getCategory() != "Line Delimiter" && tokens [j].getLexeme () != "BTW" && tokens[j].getCategory() != "Newline Suppressor"){
				/* AN */
				if(tokens[j].getCategory() == "Conjunctor"){
					j++;
					continue;
				}
				if(tokens[j].getLexeme()=="BTW"){
					j++;
					break; //Leensey:added this to catch <Comparison> <op1> AN <op2> BTW <comment>
				}
				else if(tokens[j].getLexeme()=="!"){
					if(tokens[flag].getLexeme()=="VISIBLE"){
						j++;
						break;
					}
					else{
						window.print("\nError("+line+"): '!' is not used validly!");
						return null;	//different error
					}
				}
				/* NESTABLE OPERATIONS */
				else if(
					(tokens[j].getCategory() == "Equality Comparison")	||
					(tokens[j].getCategory() == "Inequality Comparison")	||
					(tokens[j].getCategory() == "Addition Operator")	||
					(tokens[j].getCategory() == "Subtraction Operator")	||
					(tokens[j].getCategory() == "Multiplication Operator")||
					(tokens[j].getCategory() == "Division Operator")	||
					(tokens[j].getCategory() == "Modulo Operator")		||
					(tokens[j].getCategory() == "Max Operator")		||
					(tokens[j].getCategory() == "Min Operator")		||
					(tokens[j].getCategory() == "Logical AND")		||
					(tokens[j].getCategory() == "Logical OR")			||
					(tokens[j].getCategory() == "Logical XOR")		||
					(tokens[j].getCategory() == "Logical NOT")
				){
					expression.Add(tokens[j]);
				}

				/* INFINITE ARITY */
				else if((tokens[j].getCategory() == "String Concatenator")){
					var result = InfiniteArity.smoosh(j, line, tokens, window);
					if (result == null) {
						return null;
					}
					expression.Add(new Token(result.Item1, "String Literal"));
					j = result.Item2;
				}
				else if((tokens[j].getCategory() == "Infinite arity AND") || (tokens[j].getCategory() == "Infinite arity OR")){
					var result = InfiniteArity.any_all(j, line, tokens, window);
					if (result == null) {
						return null;
					}
					expression.Add(new Token((result.Item1 == true.ToString() )? "WIN":"FAIL", "Boolean Literal"));
					j = result.Item2;
				}

				/* LITERALS */
				else if(
					(tokens[j].getCategory() == "Integer Literal") 	||
					(tokens[j].getCategory() == "Float Literal") 	||
					(tokens[j].getCategory() == "String Literal") 	||
					(tokens[j].getCategory() == "Boolean Literal")
				){
					expression.Add(tokens[j]);
				}
				/* VARIABLES */
				else if(tokens[j].getCategory() == "Identifier" || tokens[j].getCategory() == "Implicit Variable"){
					if(window.var_basket.ContainsKey(tokens[j].getLexeme())){
						expression.Add(tokens[j]);
					}else{
						window.print("Error("+line+"): "+tokens[j].getLexeme()+" is not declared!");
						return null;
					}
				}
				/* error operand */
				else{
					window.print("Error("+line+"): "+tokens[j].getLexeme()+" not a valid operand!");
					return null;
				}
				j++;
			}
			if(tokens[j-1].getCategory() == "Conjunctor"){
				window.print("Error("+line+"): Expected another operand after 'AN', before newline!");
				return null;
			}

			expression.Reverse();
			/*
			CHECK IF THERE IS 'AN'


			problem w/ mod of
			*/
			for (int i=0, len = expression.Count; i<len; i++) {
				if((expression[i].getCategory() == "Equality Comparison")){
					try{
						bool res = ( calculator.Pop ().ToString() == calculator.Pop ().ToString() );
						calculator.Push(res);
					}catch (Exception e) {
						window.print("Error("+line+"): Invalid expression!");
						return null;
					}
				}
				else if((expression[i].getCategory() == "Inequality Comparison")){

					try{
						bool res = (calculator.Pop().ToString() != calculator.Pop().ToString());
						calculator.Push(res);
					}
					catch (Exception e) {
						window.print("Error("+line+"): Invalid expression!");
						return null;
					}
				}
				else if((expression[i].getCategory() == "Addition Operator")){
					try{
						string a = calculator.Pop().ToString().Replace("\"", ""), b = calculator.Pop().ToString().Replace("\"", "");
						int x=0, y=0; float c=0, d=0;

						if(int.TryParse(a, out x) && int.TryParse(b, out y)){
							calculator.Push(x+y);
						}else if(float.TryParse(a, out c) && float.TryParse(b, out d)){
							float res = c+d;
							calculator.Push((res % 1 != 0)? res.ToString(): res.ToString()+".0");
						}
						else{
							window.print("Error("+line+"): Cannot perform SUM OF '"+a+"' AN '"+b+"' \n");
						}
					}
					catch (Exception e) {
						window.print("Error("+line+"): Invalid expression!");
						return null;
					}
				}
				else if((expression[i].getCategory() == "Subtraction Operator")){
					try{
						string a = calculator.Pop().ToString().Replace("\"", ""), b = calculator.Pop().ToString().Replace("\"", "");
						int x=0, y=0; float c=0, d=0;

						if(int.TryParse(a, out x) && int.TryParse(b, out y)){
							calculator.Push(x-y);
						}else if(float.TryParse(a, out c) && float.TryParse(b, out d)){
							float res = c-d;
							calculator.Push((res % 1 != 0)? res.ToString(): res.ToString()+".0");
						}
						else{
							window.print("Error("+line+"): Cannot perform DIFF OF '"+a+"' AN '"+b+"' \n");
						}
					}
					catch (Exception e) {
						window.print("Error("+line+"): Invalid expression!");
						return null;
					}
				}
				else if((expression[i].getCategory() == "Multiplication Operator")){
					try{
						string a = calculator.Pop().ToString().Replace("\"", ""), b = calculator.Pop().ToString().Replace("\"", "");
						int x=0, y=0; float c=0, d=0;

						if(int.TryParse(a, out x) && int.TryParse(b, out y)){
							calculator.Push(x*y);
						}else if(float.TryParse(a, out c) && float.TryParse(b, out d)){
							float res = c*d;
							calculator.Push((res % 1 != 0)? res.ToString(): (res.ToString()+".0"));
						}
						else{
							window.print("Error("+line+"): Cannot perform PRODUKT OF '"+a+"' AN '"+b+"' \n");
						}
					}
					catch (Exception e) {
						window.print("Error("+line+"): Invalid expression!");
						return null;
					}
				}
				else if((expression[i].getCategory() == "Division Operator")){
					try{
						string a = calculator.Pop().ToString().Replace("\"", ""), b = calculator.Pop().ToString().Replace("\"", "");
						int x=0, y=0; float c=0, d=0;

						if(int.TryParse(a, out x) && int.TryParse(b, out y)){
							calculator.Push(x/y);
						}else if(float.TryParse(a, out c) && float.TryParse(b, out d)){
							float res = c/d;
							calculator.Push((res % 1 != 0)? res.ToString(): (res.ToString()+".0"));
						}
						else{
							window.print("Error("+line+"): Cannot perform QUOSHUNT OF '"+a+"' AN '"+b+"' \n");
						}
					}
					catch (Exception e) {
						window.print("Error("+line+"): Invalid expression!");
						return null;
					}
				}
				else if((expression[i].getCategory() == "Modulo Operator")){
					try{
						string a = calculator.Pop().ToString().Replace("\"", ""), b = calculator.Pop().ToString().Replace("\"", "");
						int x=0, y=0; float c=0, d=0;

						if(int.TryParse(a, out x) && int.TryParse(b, out y)){
							calculator.Push(x%y);
						}else if(float.TryParse(a, out c) && float.TryParse(b, out d)){
							float res = c%d;
							calculator.Push((res % 1 != 0)? res.ToString(): (res.ToString()+".0"));
						}
						else{
							window.print("Error("+line+"): Cannot perform MOD OF '"+a+"' AN '"+b+"' \n");
						}
					}
					catch (Exception e) {
						window.print("Error("+line+"): Invalid expression!");
						return null;
					}
				}
				else if((expression[i].getCategory() == "Max Operator")){
					try{
						string a = calculator.Pop().ToString().Replace("\"", ""), b = calculator.Pop().ToString().Replace("\"", "");
						int x=0, y=0; float c=0, d=0;

						if(int.TryParse(a, out x) && int.TryParse(b, out y)){
							calculator.Push((x>y)? x:y);
						}else if(float.TryParse(a, out c) && float.TryParse(b, out d)){
							float res = (c>d)? c:d;
							calculator.Push((res % 1 != 0)? res.ToString(): (res.ToString()+".0"));
						}
						else{
							window.print("Error("+line+"): Cannot perform BIGGR OF '"+a+"' AN '"+b+"' \n");
						}
					}
					catch (Exception e) {
						window.print("Error("+line+"): Invalid expression!");
						return null;
					}
				}
				else if((expression[i].getCategory() == "Min Operator")){
					try{
						try{
							string a = calculator.Pop().ToString().Replace("\"", ""), b = calculator.Pop().ToString().Replace("\"", "");
							int x=0, y=0; float c=0, d=0;

							if(int.TryParse(a, out x) && int.TryParse(b, out y)){
								calculator.Push((x<y)? x:y);
							}else if(float.TryParse(a, out c) && float.TryParse(b, out d)){
								float res = (c<d)? c:d;
								calculator.Push((res % 1 != 0)? res.ToString(): (res.ToString()+".0"));
							}
							else{
								window.print("Error("+line+"): Cannot perform SMALLR OF '"+a+"' AN '"+b+"' \n");
							}
						}
						catch (Exception e) {
							window.print("Error("+line+"): Invalid expression!");
							return null;
						}
					}
					catch (Exception e) {
						window.print("Error("+line+"): Invalid expression!");
						return null;
					}
				}
				else if((expression[i].getCategory() == "Logical AND")){

					try{
						bool a = (bool)calculator.Pop(), b = (bool)calculator.Pop();
						calculator.Push(a && b);
					}
					catch (Exception e) {
						window.print("Error("+line+"): Invalid expression!");
						return null;
					}
				}
				else if((expression[i].getCategory() == "Logical OR")){

					try{
						bool a = (bool)calculator.Pop(), b = (bool)calculator.Pop();
						calculator.Push(a || b);
					}
					catch (Exception e) {
						window.print("Error("+line+"): Invalid expression!");
						return null;
					}
				}
				else if((expression[i].getCategory() == "Logical XOR")){

					try{
						bool a = (bool)calculator.Pop(), b = (bool)calculator.Pop();
						calculator.Push(a ^ b);
					}
					catch (Exception e) {
						window.print("Error("+line+"): Invalid expression!");
						return null;
					}
				}
				else if((expression[i].getCategory() == "Logical NOT")){

					try{
						bool a = (bool) calculator.Pop();
						calculator.Push(!a);
					}
					catch (Exception e) {
						window.print("Error("+line+"): Invalid expression!");
						return null;
					}
				}
				/* OPERAND */
				else{
					/* boolean */
					if(expression[i].getCategory() == "Boolean Literal"){
						calculator.Push(expression[i].getLexeme() == "WIN"? true: false);
					}

					/*YARN, NUMBR, NUMBAR*/
					else if (
						(expression[i].getCategory() == "String Literal") 	||
						(expression[i].getCategory() == "Integer Literal") 	||
						(expression[i].getCategory() == "Float Literal")
					){
						calculator.Push(expression[i].getLexeme());
					}

					else if(expression[i].getCategory() == "Identifier" || expression[i].getCategory() == "Implicit Variable"){
						if(window.var_basket[expression[i].getLexeme()] == "WIN" || window.var_basket[expression[i].getLexeme()] == "FAIL" ){
							calculator.Push(window.var_basket[expression[i].getLexeme()] == "WIN" ? true: false);
						}
						else{
							calculator.Push(window.var_basket[expression[i].getLexeme()]);
						}
					}else
					{
						window.print("Error("+line+"): "+expression[i].getLexeme()+" invalid operand!");
					}
				}

			}
			if(calculator.Count != 1){
				window.print("Error("+line+"): Invalid expression!");
				return null;
			}
			return new Tuple<string, int>(calculator.Pop().ToString(), j-1); // j-1 para mabilang yung line delimiter sa syntax analyzer
		}
	}
}
