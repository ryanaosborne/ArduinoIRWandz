using System;
using System.Collections.Generic;
using System.Windows.Forms;




namespace WiiWandz 
{
	public partial class MultipleWiiWandzForm : Form
	{
		
		

		public MultipleWiiWandzForm()
		{
			InitializeComponent();

           
        }

		private void MultipleWiimoteForm_Load(object sender, EventArgs e)
		{
            
            int index = 1;
            TabPage tp = new TabPage("Arduino IR " + index);
            tabWiimotes.TabPages.Add(tp);

            WiimoteInfo wi = new WiimoteInfo();
            tp.Controls.Add(wi);
            


        }

        
    }
}
