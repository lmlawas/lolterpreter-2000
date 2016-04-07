using System;

namespace lolterpreter_2000
{
	public partial class InputBox : Gtk.Dialog
	{
		public InputBox ()
		{
			this.Build ();
		}
		protected void OnButtonOkClicked (object sender, EventArgs e)
		{
			this.Respond (Gtk.ResponseType.Ok);
		}

		protected void OnButtonCancelClicked (object sender, EventArgs e)
		{
			this.Respond (Gtk.ResponseType.Cancel);
		}

		public String Text {
			get {
				return textview1.Buffer.Text;
			}
		}
	}
}

