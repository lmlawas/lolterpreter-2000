using System;
using System.Collections;
using Gtk;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using lolterpreter_2000;
namespace lolterpreter_2000
{
	public class LexicalAnalyzer
	{
		private static string[] regex = {
			"^HAI$",
			"^KTHXBYE$",
			"^I HAS A$",
			"^BTW$",
			"^OBTW$",
			"^TLDR$",
			"^ITZ$",
			"^R$",
			"^SUM OF$",
			"^DIFF OF$",
			"^PRODUKT OF$",
			"^QUOSHUNT OF$",
			"^MOD OF$",
			"^BIGGR OF$",
			"^SMALLR OF$",
			"^BOTH OF$",
			"^EITHER OF$",
			"^WON OF$",
			"^NOT$",
			"^ALL OF$",
			"^ANY OF$",
			"^MKAY$",
			"^BOTH SAEM$",
			"^DIFFRINT$",
			"^SMOOSH$",
			"^AN$",
			"^MAEK$",
			"^A$",
			"^IS NOW A$",
			"^VISIBLE$",
			"^!$",
			"^GIMMEH$",
			"^O RLY\\?$",
			"^YA RLY$",
			"^MEBBE$",
			"^NO WAI$",
			"^OIC$",
			"^WTF\\?$",
			"^OMG$",
			"^OMGWTF$",
			"^IM IN YR$",
			"^UPPIN$",
			"^NERFIN$",
			"^YR$",
			"^TIL$",
			"^WILE$",
			"^IM OUTTA YR$",
			"^HOW DUZ I$",
			"^IF U SAY SO$",
			"^GTFO$",
			@"^(-?[0-9]+)$",
			@"^(-?[0-9]+\.[0-9]+)$",
			"^(\\\"((\\\\.)|[^\\\\\\\\\\\"])*\\\")$",
			"^(WIN|FAIL)$",
			"^(NUMBR|NUMBAR|YARN|TROOF)$",
			"^IT$",
			@"^(\w[A-Za-z0-9_]*)$",
			@"^\\n$"
		};

		private static string[] classi = {
			"Code Delimiter",
			"Code Delimiter",
			"Variable Declaration",
			"1-Line Comment",
			"Start of N-line Comment",
			"End of N-line Comment",
			"Variable Initialization",
			"Variable Assignment",
			"Addition Operator",
			"Subtraction Operator",
			"Multiplication Operator",
			"Division Operator",
			"Modulo Operator",
			"Max Operator",
			"Min Operator",
			"Logical AND",
			"Logical OR",
			"Logical XOR",
			"Logical NOT",
			"Infinite arity AND",
			"Infinite arity OR",
			"Operation Delimiter",
			"Equality Comparison",
			"Inequality Comparison",
			"String Concatenator",
			"Conjunctor",
			"Binary Explicit Typecast",
			"Explicit Typecast Conjunctor",
			"Explicit Typecast",
			"Output Function",
			"Newline Suppressor",
			"Input Function",
			"Start If-Else",
			"If Start",
			"Else If start",
			"Else Start",
			"End If-Else",
			"Switch Case",
			"Case",
			"Case Default",
			"Loop Start",
			"Increment Operator",
			"Decrement Operator",
			"Stepsize Conjunctor",
			"Boolean Evaluator",
			"Boolean Evaluator",
			"Loop End",
			"Function Start",
			"Function End",
			"Break Keyword",
			"Integer Literal",
			"Float Literal",
			"String Literal",
			"Boolean Literal",
			"Type Literal",
			"Implicit Variable",
			"Identifier",
			"Line Delimiter"
		};

		public static string classify(string token){
			for (int i = 0, len = regex.Length; i < len; i++) {
				if (Regex.Match (token, regex [i]).Success) {
					return classi [i];
				}
			}

			return "not_classified";
		}


		// gets UNCLASSIFIED tokens
		public static List<string> getUnclassTokens(string code, MainWindow window){
			List<string> tokens = new List<string> ();
			char[] d = {'\n'};
			string[] lines = code.Split (d);

			// Break down tokens here ================================================
			int i = 0;
			int commentFlag = 0;

			for(int k=0,lenny = lines.Length; k<lenny; k+=1){
				string s = lines [k];


				Queue string_lit = new Queue();		// to handle strings in one line
				i++;								// line counter

				if (k == lenny - 1 && commentFlag == 1) {
					window.print ("Error("+i+"): Code must end with KTHXBYE!");
					return null;
				}

				if(s.Trim() == " " || s.Trim() == ""){
					tokens.Add ("\\n");
					continue;
				}

				/* FOR handling strings in a line */
				if(Regex.Match(s.Trim(), "^((([^\"]*\"[^\"]*)([^\"]*\"[^\"]*)(([^\"]*\"[^\"]*)([^\"]*\"[^\"]*))*)|([^\"]*))$").Success){
					MatchCollection strings = Regex.Matches (s, "(\\\"((\\\\.)|[^\\\\\\\\\\\"])*\\\")");
					foreach (Match m in strings) {
						if(new Regex("(\\\"((\\\\.)|[^\\\\\\\\\\\"])*\\\")").IsMatch(m.ToString ().Trim ())){
							string_lit.Enqueue(m.ToString ().Trim ());
						}
					}
				}
				/* FOR Error detection in string delimiters */
				else {
					window.clearConsole ();
					window.print ("Error("+i+"): Expected \" before newline.");
					return null;
				}

				/* Classifiy tokens */
				string whitespace = @"[ \t]+";
				string[] line = Regex.Split(s.Trim(), whitespace);
				int slitFlag = 0;
				for(int j=0, len = line.Length; j<len; j++){

					/*Handling comments*/
					if(Regex.Match(line[j], "^TLDR$").Success){
						if (j == 0) {
							commentFlag = 0;
							if (line.Length > 1) {
								window.print ("Error("+i+"): TLDR cannot be use within a line with existing statements after it!");
								return null;
							}
						} else {
							window.print ("Error("+i+"): TLDR cannot be use within a line with existing statements before it!");
							return null;
						}
					}
					else if(Regex.Match(line[j], "^OBTW$").Success){
						if (j == 0) {
							commentFlag = 1;
						} else {
							window.print ("Error("+i+"): OBTW cannot be use within a line with existing statements before it!");
							return null;
						}
					}
					else if(commentFlag == 1){
						continue;
					}

					try{
						/*Acceptable strings*/
						if (Regex.Match (line [j], "^(\"[^\"]*\")$").Success) {
							tokens.Add (string_lit.Dequeue ().ToString ());
							continue;
						} else if (Regex.Match (line [j], "^\"[^\"]+$").Success && slitFlag == 0) {
							tokens.Add (string_lit.Dequeue ().ToString ());
							slitFlag = 1;
							continue;
						} else if (Regex.Match (line [j], "^[^\"]+\"$").Success && slitFlag == 1) {
							slitFlag = 0;
							continue;
						} else if (Regex.Match (line [j], "^\"$").Success) {
							if (slitFlag == 0) {
								tokens.Add (string_lit.Dequeue ().ToString ());
							}
							slitFlag = (slitFlag == 0) ? 1 : 0;
							continue;
						}
						/* Including the '!' operator */
						if (Regex.Match (line [j], "^(\"[^\"]*\"!)$").Success) {
							tokens.Add (string_lit.Dequeue().ToString());
							tokens.Add ("!");
							continue;
						}  else if (Regex.Match (line [j], "^[^\"]+\"!$").Success && slitFlag == 1) {
							slitFlag = 0;
							tokens.Add ("!");
							continue;
						} else if (Regex.Match (line [j], "^\"!$").Success && slitFlag == 1) {
							slitFlag = 0;
							tokens.Add ("!");
							continue;
						}

						/*for wrong strings*/
						else if ((Regex.Match (line [j], "^[^\"]+\"$").Success) ||
							(Regex.Match (line [j], "^\"[^\"]+$").Success) ||
							(Regex.Match (line [j], "^[^\"]+\"[^\"]+$").Success) ||
							(Regex.Match (line [j], "^((.+\"[^\"]*\".+)|(\"[^\"]*\".+)|(.+\"[^\"]*\"))$").Success)) {
							window.clearConsole ();
							window.print ("Error("+i+"): Expected space between string literals! ");
							return null;
						}
						else if(slitFlag == 1){
							continue;
						}
						else if(Regex.Match(line[j], "^IF$").Success && Regex.Match(line[j+1], "^U$").Success && Regex.Match(line[j+2], "^SAY$").Success && Regex.Match(line[j+3], "^SO$").Success){
							tokens.Add(line[j]+" "+line[j+1]+" "+line[j+2]+" "+line[j+3]);
							j += 3;
						}
						/* For 3-word keywords */
						else if(
							(Regex.Match(line[j], "^I$").Success && line[j+1]!=null && Regex.Match(line[j+1], "^HAS$").Success && line[j+2]!=null && Regex.Match(line[j+2], "^A$").Success)
							|| (Regex.Match(line[j], "^IS$").Success && line[j+1]!=null && Regex.Match(line[j+1], "^NOW$").Success && line[j+2]!=null && Regex.Match(line[j+2], "^A$").Success)
							|| (Regex.Match(line[j], "^IM$").Success && line[j+1]!=null && Regex.Match(line[j+1], "^IN$").Success && line[j+2]!=null && Regex.Match(line[j+2], "^YR$").Success)
							|| (Regex.Match(line[j], "^IM$").Success && line[j+1]!=null && Regex.Match(line[j+1], "^OUTTA$").Success && line[j+2]!=null && Regex.Match(line[j+2], "^YR$").Success)
							|| (Regex.Match(line[j], "^HOW$").Success && line[j+1]!=null && Regex.Match(line[j+1], "^DUZ$").Success && line[j+2]!=null && Regex.Match(line[j+2], "^I$").Success)
						){
							tokens.Add(line[j]+" "+line[j+1]+" "+line[j+2]);
							j += 2;
						}
						/* For 2-word keywords */
						else if( (Regex.Match(line[j], "^SUM$").Success && line[j+1]!=null &&  Regex.Match(line[j+1], "^OF$").Success)
							|| (Regex.Match(line[j], "^DIFF$").Success && line[j+1]!=null &&  Regex.Match(line[j+1], "^OF$").Success)
							|| (Regex.Match(line[j], "^PRODUKT$").Success && line[j+1]!=null &&  Regex.Match(line[j+1], "^OF$").Success)
							|| (Regex.Match(line[j], "^QUOSHUNT$").Success && line[j+1]!=null &&  Regex.Match(line[j+1], "^OF$").Success)
							|| (Regex.Match(line[j], "^MOD$").Success && line[j+1]!=null &&  Regex.Match(line[j+1], "^OF$").Success)
							|| (Regex.Match(line[j], "^BIGGR$").Success && line[j+1]!=null &&  Regex.Match(line[j+1], "^OF$").Success)
							|| (Regex.Match(line[j], "^SMALLR$").Success && line[j+1]!=null &&  Regex.Match(line[j+1], "^OF$").Success)
							|| (Regex.Match(line[j], "^BOTH$").Success && line[j+1]!=null &&  Regex.Match(line[j+1], "^OF$").Success)
							|| (Regex.Match(line[j], "^EITHER$").Success && line[j+1]!=null &&  Regex.Match(line[j+1], "^OF$").Success)
							|| (Regex.Match(line[j], "^WON$").Success && line[j+1]!=null &&  Regex.Match(line[j+1], "^OF$").Success)
							|| (Regex.Match(line[j], "^ALL$").Success && line[j+1]!=null &&  Regex.Match(line[j+1], "^OF$").Success)
							|| (Regex.Match(line[j], "^ANY$").Success && line[j+1]!=null &&  Regex.Match(line[j+1], "^OF$").Success)
							|| (Regex.Match(line[j], "^BOTH$").Success && line[j+1]!=null &&  Regex.Match(line[j+1], "^SAEM$").Success)
							|| (Regex.Match(line[j], "^O$").Success && line[j+1]!=null &&  Regex.Match(line[j+1], "^RLY?").Success)
							|| (Regex.Match(line[j], "^YA$").Success && line[j+1]!=null &&  Regex.Match(line[j+1], "^RLY$").Success)
							|| (Regex.Match(line[j], "^NO$").Success && line[j+1]!=null &&  Regex.Match(line[j+1], "^WAI$").Success)
						){
							tokens.Add(line[j] + " " + line[j+1]);
							j += 1;
						}
						/* For 3-word keywords */
						else if(Regex.Match(line[j], "^BTW$").Success){
							tokens.Add(line[j]);
							break;
						}
						/* For others */
						else{
							tokens.Add(line[j]);
						}
					}catch{
						window.clearConsole ();
						window.print ("Error("+i+"): Invalid statement!");
						return null;
					}


				}
				tokens.Add ("\\n"); // ginawa ko lang para ma distinguish yung newline since tinanggal
				// na siya dun sa pag trim at pag split
			}
			return tokens;
		}
	}
}
