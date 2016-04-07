using System;
using System.Collections;
using Gtk;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using lolterpreter_2000;

namespace lolterpreter_2000
{
	public class ArithmeticOperations
	{
		public ArithmeticOperations ()
		{
		}
		/* Generate Postfix Form of Expression */
		public static Stack generate_postfix(List<Token> tokens, int i, int line, MainWindow window){
			/* List For Arithmetic Operations*/
			int flag = i-1;
			Stack arithStack = new Stack();
			List<string> arithList = new List<string>();
			Regex numbr = new Regex(@"^(-?[0-9]+)$");
			Regex numbar = new Regex (@"^(-?[0-9]+\.[0-9]+)$");

			while(tokens[i].getCategory() != "Line Delimiter"){
				if(tokens[i].getCategory() == "Addition Operator"){
					arithList.Add('+'.ToString());
					i++;
				}
				else if(tokens[i].getCategory() == "Subtraction Operator"){
					arithList.Add('-'.ToString());
					i++;
				}
				else if(tokens[i].getCategory() == "Multiplication Operator"){
					arithList.Add('*'.ToString());
					i++;
				}
				else if(tokens[i].getCategory() == "Division Operator"){
					arithList.Add('/'.ToString());
					i++;
				}
				else if(tokens[i].getCategory() == "Modulo Operator"){
					arithList.Add('%'.ToString());
					i++;
				}
				else if(tokens[i].getCategory() =="Max Operator"){
					arithList.Add('>'.ToString());
					i++;
				}
				else if(tokens[i].getCategory() =="Min Operator"){
					arithList.Add('<'.ToString());
					i++;
				}
				else if(tokens[i].getCategory() == "Integer Literal" ||
					tokens[i].getCategory() == "Float Literal"){
					arithList.Add(tokens[i].getLexeme().ToString());
					i++;
				}
				else if(tokens[i].getCategory() == "String Literal"){
					string str = tokens [i].getLexeme ().Replace ("\"", "");
					if (numbr.IsMatch(str)||numbar.IsMatch (str)) {
						arithList.Add(str);
						i++;
					} else {
						window.print ("Error(" + line + "): cannot typecast string to int or float!");
						arithStack.Clear();
						return arithStack;	//different error
					}
				}
				else if(tokens[i].getCategory() == "Identifier" || tokens[i].getCategory() == "Implicit Variable"){
					if(window.var_basket.ContainsKey (tokens [i].getLexeme ())){
						string str = window.var_basket[tokens [i].getLexeme ()].Replace("\"", "");
						if(numbr.IsMatch(str)||numbar.IsMatch (str)){
							arithList.Add(str);
							i++;
						}
						else{
							window.print ("Error(" + line + "): cannot typecast "+str+" to int or float!");
							arithStack.Clear();
							return arithStack;	//different error
						}
					}
					else{
						window.print("Error("+line+"): '" + tokens[i].getLexeme() + "' is undefined!");
						arithStack.Clear();
						return arithStack;	//different error
					}
				}
				else if(tokens[i].getCategory() == "Conjunctor"){
					if(
						(
							tokens[i-1].getCategory() == "Integer Literal"||
							tokens[i-1].getCategory() == "Float Literal"	||
							tokens[i-1].getCategory() == "String Literal"	||
							tokens[i-1].getCategory() == "Identifier"			||
							tokens[i-1].getCategory() == "Implicit Variable"
						) && (
							tokens[i+1].getCategory() == "Integer Literal"||
							tokens[i+1].getCategory() == "Float Literal"	||
							tokens[i+1].getCategory() == "String Literal"	||
							tokens[i+1].getCategory() == "Identifier"			||
							tokens[i+1].getCategory() == "Implicit Variable"||
							tokens[i+1].getCategory() =="Addition Operator"||
							tokens[i+1].getCategory() =="Subtraction Operator"||
							tokens[i+1].getCategory() =="Multiplication Operator"||
							tokens[i+1].getCategory() =="Division Operator"||
							tokens[i+1].getCategory() =="Modulo Operator"||
							tokens[i+1].getCategory() =="Max Operator"||
							tokens[i+1].getCategory() =="Min Operator"
						)
					){
						i++;
					}
					else{
						window.print("\nError("+line+"): invalid use of conjunctor!");
						arithStack.Clear();
						return arithStack;	//different error
					}
				}
				else if(tokens[i].getLexeme()=="BTW"){
					while(tokens[i].getCategory() != "Line Delimiter"){
						i++;
					}
					break;
				}
				else if(tokens[i].getLexeme()=="!"){//LEENSEY EDITED starts here
					if(tokens[flag].getLexeme()=="VISIBLE"){
						i++;
						break;
					}
					else{
						window.print("\nError("+line+"): '!' is not used validly!");
						arithStack.Clear();
						return arithStack;	//different error
					}
				}//LEENSEY EDITED ends here
				else{
					window.print("\nError("+line+"): invalid operand or operator!");
					arithStack.Clear();
					return arithStack;	//different error
				}
			}
			arithList.Reverse ();
			arithStack = evaluate_postfix(arithList, line, window);
			if(arithStack==null) return null; //stack underflow
			else if(arithStack.Count==0) return arithStack;//different error
			else{
				arithStack.Push(i);
				return arithStack;	//no error
			}
		}//end of generate_prefix()

		/* Evaluate Postfix Expression */
		private static Stack evaluate_postfix(List<string> arithList, int line, MainWindow window){
			/* Stack for Arithmetic Operations */
			Stack arithStack = new Stack();

			/* To Check if term to be popped is int or float */
			Regex numbr = new Regex(@"^(-?[0-9]+)$"); //int
			Regex numbar = new Regex (@"^(-?[0-9]+\.[0-9]+)$");	//float

			/* Variables to hold operands*/
			int n_int;
			int opi1, opi2;
			double n_float;
			double opf1, opf2;

			foreach(string term in arithList){
				if(numbr.IsMatch(term)||numbar.IsMatch(term)){
					arithStack.Push(term);
				}
				else{
					//+++start of addition
					if(term=='+'.ToString()){
						if(arithStack.Count>1){
							if (Int32.TryParse(arithStack.Peek ().ToString(), out n_int)){
								arithStack.Pop ();
								opi2 = n_int;
								if(Int32.TryParse(arithStack.Peek ().ToString(), out n_int)){
									arithStack.Pop ();
									opi1 = n_int;
									arithStack.Push(opi1+opi2);
								}
								else if(Double.TryParse(arithStack.Peek ().ToString(), out n_float)){
									arithStack.Pop ();
									opf1 = n_float;
									opf2 = Convert.ToDouble(opi2);
									arithStack.Push(opf1+opf2);
								}
								else{
									window.print("\nError("+line+"): cannot parse!");
									arithStack.Clear();
									return arithStack;	//different error
								}
							}
							else if(Double.TryParse(arithStack.Peek ().ToString(), out n_float)){
								arithStack.Pop ();
								opf2 = n_float;
								if(Double.TryParse(arithStack.Peek().ToString(), out n_float)){
									arithStack.Pop ();
									opf1 = n_float;
									arithStack.Push(opf1+opf2);
								}
								else if(Int32.TryParse(arithStack.Peek ().ToString(), out n_int)){
									arithStack.Pop ();
									opf1 = Convert.ToDouble(n_int);
									arithStack.Push(opf1+opf2);
								}
								else{
									window.print("\nError("+line+"): cannot parse!");
									arithStack.Clear();
									return arithStack;	//different error
								}
							}
							else{
								window.print("\nError("+line+"): cannot parse!");
								arithStack.Clear();
								return arithStack;	//different error
							}
						}
						else{
							return null;	//stack underflow
						}
					}//end of addition
					//---start of subtraction
					else if(term=='-'.ToString()){
						if(arithStack.Count>1){
							if (Int32.TryParse(arithStack.Peek ().ToString(), out n_int)){
								arithStack.Pop ();
								opi2 = n_int;
								if(Int32.TryParse(arithStack.Peek ().ToString(), out n_int)){
									arithStack.Pop ();
									opi1 = n_int;
									arithStack.Push(opi2-opi1);
								}
								else if(Double.TryParse(arithStack.Peek ().ToString(), out n_float)){
									arithStack.Pop ();
									opf1 = n_float;
									opf2 = Convert.ToDouble(opi2);
									arithStack.Push(opf2-opf1);
								}
								else{
									window.print("\nError("+line+"): cannot parse!");
									arithStack.Clear();
									return arithStack;	//different error
								}
							}
							else if(Double.TryParse(arithStack.Peek ().ToString(), out n_float)){
								arithStack.Pop ();
								opf2 = n_float;
								if(Double.TryParse(arithStack.Peek().ToString(), out n_float)){
									arithStack.Pop ();
									opf1 = n_float;
									arithStack.Push(opf2-opf1);
								}
								else if(Int32.TryParse(arithStack.Peek ().ToString(), out n_int)){
									arithStack.Pop ();
									opf1 = Convert.ToDouble(n_int);
									arithStack.Push(opf2-opf1);
								}
								else{
									window.print("\nError("+line+"): cannot parse!");
									arithStack.Clear();
									return arithStack;	//different error
								}
							}
							else{
								window.print("\nError("+line+"): cannot parse!");
								arithStack.Clear();
								return arithStack;	//different error
							}
						}
						else{
							return null;	//stack underflow
						}
					}//end of subtraction
					//***start of multiplication
					else if(term=='*'.ToString()){
						if(arithStack.Count>1){
							if (Int32.TryParse(arithStack.Peek ().ToString(), out n_int)){
								arithStack.Pop ();
								opi2 = n_int;
								if(Int32.TryParse(arithStack.Peek ().ToString(), out n_int)){
									arithStack.Pop ();
									opi1 = n_int;
									arithStack.Push(opi2*opi1);
								}
								else if(Double.TryParse(arithStack.Peek ().ToString(), out n_float)){
									arithStack.Pop ();
									opf1 = n_float;
									opf2 = Convert.ToDouble(opi2);
									arithStack.Push(opf2*opf1);
								}
								else{
									window.print("\nError("+line+"): cannot parse!");
									arithStack.Clear();
									return arithStack;	//different error
								}
							}
							else if(Double.TryParse(arithStack.Peek ().ToString(), out n_float)){
								arithStack.Pop ();
								opf2 = n_float;
								if(Double.TryParse(arithStack.Peek().ToString(), out n_float)){
									arithStack.Pop ();
									opf1 = n_float;
									arithStack.Push(opf2*opf1);
								}
								else if(Int32.TryParse(arithStack.Peek ().ToString(), out n_int)){
									arithStack.Pop ();
									opf1 = Convert.ToDouble(n_int);
									arithStack.Push(opf2*opf1);
								}
								else{
									window.print("\nError("+line+"): cannot parse!");
									arithStack.Clear();
									return arithStack;	//different error
								}
							}
							else{
								window.print("\nError("+line+"): cannot parse!");
								arithStack.Clear();
								return arithStack;	//different error
							}
						}
						else{
							return null;	//stack underflow
						}
					}//end of multiplication
					/////start of division
					else if(term=='/'.ToString()){
						if(arithStack.Count>1){
							if (Int32.TryParse(arithStack.Peek ().ToString(), out n_int)){
								arithStack.Pop ();
								opi2 = n_int;
								if(Int32.TryParse(arithStack.Peek ().ToString(), out n_int)){
									arithStack.Pop ();
									opi1 = n_int;
									arithStack.Push(opi2/opi1);
								}
								else if(Double.TryParse(arithStack.Peek ().ToString(), out n_float)){
									arithStack.Pop ();
									opf1 = n_float;
									opf2 = Convert.ToDouble(opi2);
									arithStack.Push(opf2/opf1);
								}
								else{
									window.print("\nError("+line+"): cannot parse!");
									arithStack.Clear();
									return arithStack;	//different error
								}
							}
							else if(Double.TryParse(arithStack.Peek ().ToString(), out n_float)){
								arithStack.Pop ();
								opf2 = n_float;
								if(Double.TryParse(arithStack.Peek().ToString(), out n_float)){
									arithStack.Pop ();
									opf1 = n_float;
									arithStack.Push(opf2/opf1);
								}
								else if(Int32.TryParse(arithStack.Peek ().ToString(), out n_int)){
									arithStack.Pop ();
									opf1 = Convert.ToDouble(n_int);
									arithStack.Push(opf2/opf1);
								}
								else{
									window.print("\nError("+line+"): cannot parse!");
									arithStack.Clear();
									return arithStack;	//different error
								}
							}
							else{
								window.print("\nError("+line+"): cannot parse!");
								arithStack.Clear();
								return arithStack;	//different error
							}
						}
						else{
							return null;//stack underflow
						}
					}//end of division
					//%%%start of modulo
					else if(term=='%'.ToString()){
						if(arithStack.Count>1){
							if (Int32.TryParse(arithStack.Peek ().ToString(), out n_int)){
								arithStack.Pop ();
								opi2 = n_int;
								if(Int32.TryParse(arithStack.Peek ().ToString(), out n_int)){
									arithStack.Pop ();
									opi1 = n_int;
									arithStack.Push(opi2%opi1);
								}
								else if(Double.TryParse(arithStack.Peek ().ToString(), out n_float)){
									arithStack.Pop ();
									opf1 = n_float;
									opf2 = Convert.ToDouble(opi2);
									arithStack.Push(opf2%opf1);
								}
								else{
									window.print("\nError("+line+"): cannot parse.");
									arithStack.Clear();
									return arithStack;	//different error
								}
							}
							else if(Double.TryParse(arithStack.Peek ().ToString(), out n_float)){
								arithStack.Pop ();
								opf2 = n_float;
								if(Double.TryParse(arithStack.Peek().ToString(), out n_float)){
									arithStack.Pop ();
									opf1 = n_float;
									arithStack.Push(opf2%opf1);
								}
								else if(Int32.TryParse(arithStack.Peek ().ToString(), out n_int)){
									arithStack.Pop ();
									opf1 = Convert.ToDouble(n_int);
									arithStack.Push(opf2%opf1);
								}
								else{
									window.print("\nError("+line+"): cannot parse.");
									arithStack.Clear();
									return arithStack;	//different error
								}
							}
							else{
								window.print("\nError("+line+"): cannot parse.");
								arithStack.Clear();
								return arithStack;	//different error
							}
						}
						else{
							return null;//stack underflow
						}
					}//end of modulo
					//>>>start of max
					else if(term=='>'.ToString()){
						if(arithStack.Count>1){
							if (Int32.TryParse(arithStack.Peek ().ToString(), out n_int)){
								arithStack.Pop ();
								opi2 = n_int;
								if(Int32.TryParse(arithStack.Peek ().ToString(), out n_int)){
									arithStack.Pop ();
									opi1 = n_int;
									arithStack.Push(opi2>opi1 ? opi2 : opi1);
								}
								else if(Double.TryParse(arithStack.Peek ().ToString(), out n_float)){
									arithStack.Pop ();
									opf1 = n_float;
									opf2 = Convert.ToDouble(opi2);
									arithStack.Push(opf2>opf1 ? opf2 : opf1);
								}
								else{
									window.print("\nError("+line+"): cannot parse!");
									arithStack.Clear();
									return arithStack;	//different error
								}
							}
							else if(Double.TryParse(arithStack.Peek ().ToString(), out n_float)){
								arithStack.Pop ();
								opf2 = n_float;
								if(Double.TryParse(arithStack.Peek().ToString(), out n_float)){
									arithStack.Pop ();
									opf1 = n_float;
									arithStack.Push(opf2>opf1 ? opf2 : opf1);
								}
								else if(Int32.TryParse(arithStack.Peek ().ToString(), out n_int)){
									arithStack.Pop ();
									opf1 = Convert.ToDouble(n_int);
									arithStack.Push(opf2>opf1 ? opf2 : opf1);
								}
								else{
									window.print("\n\nError("+line+"): cannot parse!");
									arithStack.Clear();
									return arithStack;	//different error
								}
							}
							else{
								window.print("Error("+line+"): cannot parse!");
								arithStack.Clear();
								return arithStack;	//different error
							}
						}
						else{
							return null;//stack underflow
						}
					}//end of max
					//<<<start of min
					else if(term=='<'.ToString()){
						if(arithStack.Count>1){
							if (Int32.TryParse(arithStack.Peek ().ToString(), out n_int)){
								arithStack.Pop ();
								opi2 = n_int;
								if(Int32.TryParse(arithStack.Peek ().ToString(), out n_int)){
									arithStack.Pop ();
									opi1 = n_int;
									arithStack.Push(opi2<opi1 ? opi2 : opi1);
								}
								else if(Double.TryParse(arithStack.Peek ().ToString(), out n_float)){
									arithStack.Pop ();
									opf1 = n_float;
									opf2 = Convert.ToDouble(opi2);
									arithStack.Push(opf2<opf1 ? opf2 : opf1);
								}
								else{
									window.print("\nError("+line+"): cannot parse!");
									arithStack.Clear();
									return arithStack;	//different error
								}
							}
							else if(Double.TryParse(arithStack.Peek ().ToString(), out n_float)){
								arithStack.Pop ();
								opf2 = n_float;
								if(Double.TryParse(arithStack.Peek().ToString(), out n_float)){
									arithStack.Pop ();
									opf1 = n_float;
									arithStack.Push(opf2<opf1 ? opf2 : opf1);
								}
								else if(Int32.TryParse(arithStack.Peek ().ToString(), out n_int)){
									arithStack.Pop ();
									opf1 = Convert.ToDouble(n_int);
									arithStack.Push(opf2<opf1 ? opf2 : opf1);
								}
								else{
									window.print("\nError("+line+"): cannot parse!");
									arithStack.Clear();
									return arithStack;	//different error
								}
							}
							else{
								window.print("\nError("+line+"): cannot parse!");
								arithStack.Clear();
								return arithStack;	//different error
							}
						}
						else{
							return null;//stack underflow
						}
					}//end of min
				}
			}//end of foreach
			if(arithStack.Count==1) return arithStack;//no error
			else return null;//stack underflow
		}//end of evaluate_postfix()
	}//end of ArithmeticOperations
}//lolterpreter_2000
