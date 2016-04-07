using System;

namespace lolterpreter_2000
{
	public class Token
	{
		private string lexeme;
		private string category;

		public Token (string lex, string classi)
		{
			this.lexeme = lex;
			this.category = classi;
		}

		public string getLexeme(){
			return this.lexeme;
		}

		public string getCategory(){
			return this.category;
		}
	}
}

