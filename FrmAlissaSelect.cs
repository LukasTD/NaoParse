using NaoParse.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NaoParse.AlissaWindow
{
	public partial class FrmAlissaSelect : Form
	{
		public static FoundWindow Selection;
		public FrmAlissaSelect(IList<FoundWindow> windows)
		{
			InitializeComponent();

			foreach (var window in windows)
				windowSelector.Items.Add(window);
		}

		private void CancelBtn_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void ConnectBtn_Click(object sender, EventArgs e)
		{
			if (windowSelector.SelectedItem == null)
			{
				MessageBox.Show("Please select a packet provider.", Text, MessageBoxButtons.OK, MessageBoxIcon.Stop);
				return;
			}

			Selection = (FoundWindow)windowSelector.SelectedItem;
			DialogResult = DialogResult.OK;

			Close();
		}

		private void windowSelector_SelectedIndexChanged(object sender, EventArgs e)
		{
			ConnectBtn.Enabled = true;
		}
	}
}
