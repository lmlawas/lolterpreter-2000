using System;
using System.Collections;
using Gtk;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using lolterpreter_2000;

public partial class MainWindow: Gtk.Window
{
	protected Gtk.ListStore lexemeListStore;  	// these are the storage for lexemes
	protected Gtk.ListStore symbolListStore;	// these one is for the symbol table

	public Dictionary<string, string> var_basket;
	protected string code; 		// this is the code to be analyzed



	public MainWindow (): base (Gtk.WindowType.Toplevel)
	{
		Build ();
		initTreeViews ();
		initModifications ();
	}

	/*
	* 	METHODS FOR CONTROLLING STUFF IN GUI --------------------------------------
	*/
	protected void clearTrees(){
		lexemeListStore.Clear ();
		symbolListStore.Clear ();
	}
	public void clearCode(){
		codeView.Buffer.Text = "";
	}
	public void clearConsole(){
		consoleView.Buffer.Text = "";
	}

	public void print (string message){
		consoleView.Buffer.Text += message;
	}
	protected void addToLexemes (String lexeme, String classification){
		lexemeListStore.AppendValues (lexeme, classification);
	}

	public void addToSymbolTable (String identifier, String value){
		symbolListStore.AppendValues (identifier, value);
	}


	/*
	*	METHODS FOR PREPARING STUFF IN GUI ----------------------------------------
	*/
	protected void initTreeViews(){
		Gtk.TreeViewColumn lexCol = new Gtk.TreeViewColumn ();
		Gtk.TreeViewColumn classCol = new Gtk.TreeViewColumn ();
		lexCol.Title = "Lexeme";
		classCol.Title = "Classification";
		lexTree.AppendColumn (lexCol);
		lexTree.AppendColumn (classCol);

		// Create model for lexTree
		lexemeListStore = new Gtk.ListStore (typeof (string), typeof (string));
		lexTree.Model = lexemeListStore;

		// Create columns for symbolTree
		Gtk.TreeViewColumn identCol = new Gtk.TreeViewColumn ();
		Gtk.TreeViewColumn valCol = new Gtk.TreeViewColumn ();
		identCol.Title = "Identifier";
		valCol.Title = "Value";
		symbolTree.AppendColumn (identCol);
		symbolTree.AppendColumn (valCol);

		// Create model for symbolTree
		symbolListStore = new Gtk.ListStore (typeof (string), typeof (string));
		symbolTree.Model = symbolListStore;

		// Renderer for lexemeTree
		Gtk.CellRendererText lexCell = new Gtk.CellRendererText ();
		Gtk.CellRendererText classCell = new Gtk.CellRendererText ();
		lexCol.PackStart (lexCell, true);
		classCol.PackStart (classCell, true);
		lexCol.AddAttribute (lexCell, "text", 0);
		classCol.AddAttribute (classCell, "text", 1);

		// Renderer for symbolTree
		Gtk.CellRendererText identCell = new Gtk.CellRendererText ();
		Gtk.CellRendererText valueCell = new Gtk.CellRendererText ();
		identCol.PackStart (identCell, true);
		valCol.PackStart (valueCell, true);
		identCol.AddAttribute (identCell, "text", 0);
		valCol.AddAttribute (valueCell, "text", 1);

	}
	protected void initModifications(){
		//Edit font
		lexTree.ModifyFont(Pango.FontDescription.FromString ("Monospace 12"));
		symbolTree.ModifyFont(Pango.FontDescription.FromString ("Monospace 12"));

		// including console and code views
		codeView.ModifyFont (Pango.FontDescription.FromString ("Monospace 12"));
		consoleView.ModifyFont (Pango.FontDescription.FromString ("Monospace 13"));
		Gdk.Color w = new Gdk.Color ();
		Gdk.Color.Parse ("white", ref w );
		consoleView.ModifyText (StateType.Normal , w);
		consoleView.ModifyBase(StateType.Normal, new Gdk.Color(0x00,0x00,0x00));
		consoleView.Editable = false;
	}

	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}

	protected void OnFileButtonClicked (object sender, EventArgs e)
	{
		Gtk.FileChooserDialog lolfile = new Gtk.FileChooserDialog("Choose a LOLCODE file", this, FileChooserAction.Open,"Cancel",ResponseType.Cancel,"Open",ResponseType.Accept);
		lolfile.Filter = new FileFilter ();
		lolfile.Filter.AddPattern ("*.lol");

		if (lolfile.Run() == (int)ResponseType.Accept)
		{
			StreamReader lolcodeFile = File.OpenText (lolfile.Filename);
			codeView.Buffer.Text = lolcodeFile.ReadToEnd ();
			lolcodeFile.Close ();
		}
		lolfile.Destroy();
	}

	protected void executeCode (object sender, EventArgs e)
	{
		string code = codeView.Buffer.Text;
		clearConsole ();
		if (code == "") {
			clearConsole ();
			print ("No code to execute!");
		} else {
			/*
				check the whole code
			*/
			clearTrees();
			clearConsole();
			Regex whole_code = new Regex(@"^(\s*BTW.*\n\s*|\s*OBTW(\s)*(.|\n)*\n(\s)*TLDR(\s)*\n\s*)*(\n)*HAI\s*\n((.|\s)*\s*\n)?KTHXBYE\s*(\n(\s|\n)*((\s*BTW\s*(.)*)|(\s*OBTW(\n|(.))*\nTLDR\n*))*)?$");
			if (whole_code.IsMatch (code)) {
				Interpret();
			} else {
				print("Fatal error!");
			}


		}
	}
	/************************
		INTERPRET CODE!
	*************************/
	private void Interpret(){
		clearConsole ();
		List<Token> tokens = lexAnalyze ();
		if (tokens != null) {
			 SyntaxAnalyzer.analyze(tokens, this);
		}
		else{
			print("Error!");
		}

	}

	/* *******************
	*  LEXICAL ANALYSIS
	** *******************/
	private List<Token> lexAnalyze(){
		List<Token> tokenList = new List<Token> ();
		List<string> unclass = LexicalAnalyzer.getUnclassTokens(codeView.Buffer.Text, this);

		if (unclass != null) {
			// Classify these @#$%@#% tokens
			for (int i = 0, len = unclass.Count; i < len; i++) {
				tokenList.Add (new Token (unclass [i], LexicalAnalyzer.classify (unclass [i])));
			}
		} else {
			return null;
		}
//		 Display in Lexemes TreeView
		 foreach(Token t in tokenList){
		 	if(t.getCategory() != "Line Delimiter"){
		 		if (t.getCategory () == "String Literal") {
		 			addToLexemes ("\"", "String Delimiter");
		 			addToLexemes (t.getLexeme().Replace("\"", ""), "String Literal");
		 			addToLexemes ("\"", "String Delimiter");
		 		} else {
		 			addToLexemes (t.getLexeme(), t.getCategory());
		 		}
		 	}
		 }
		 return tokenList;
	}



}
