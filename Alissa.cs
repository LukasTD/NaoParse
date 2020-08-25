using NaoParse.Parsing;
using NaoParse.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NaoParse
{
	internal class Alissa : Form
	{
		private IntPtr alissaHWnd;
		public Alissa()
		{
			Connect();
		}

		/// <summary>
		/// Connects to the Alissa window.
		/// </summary>
		private void Connect()
		{
			if (alissaHWnd == IntPtr.Zero)
			{
				if (!SelectPacketProvider(true))
					return;
			}

			if (!WinApi.IsWindow(alissaHWnd))
			{
				Console.WriteLine("Failed to connect, please make sure the selected packet provider is still running.");
				alissaHWnd = IntPtr.Zero;
				return;
			}

			SendAlissa(alissaHWnd, Sign.Connect);
			Console.WriteLine("Connected successfully.");
		}

		/// <summary>
		/// Tries to find a valid packet provider, asks the user to select one
		/// if there are multiple windows.
		/// </summary>
		/// <param name="selectSingle">If true a single valid candidate will be selected without prompt.</param>
		/// <returns></returns>
		private bool SelectPacketProvider(bool selectSingle)
		{
			var alissaWindows = WinApi.FindAllWindows("mod_Alissa");
			FoundWindow window = null;

			if (alissaWindows.Count == 0)
			{
				Console.WriteLine("No packet provider found.");
				return false;
			}
			else if (selectSingle && alissaWindows.Count == 1)
			{
				window = alissaWindows[0];
			}
			else
			{
				Console.WriteLine("More than one packet provider found.");
			}

			alissaHWnd = window.HWnd;

			return true;
		}

		/// <summary>
		/// Sends message to Alissa window.
		/// </summary>
		/// <param name="hWnd"></param>
		/// <param name="op"></param>
		private void SendAlissa(IntPtr hWnd, int op)
		{
			WinApi.COPYDATASTRUCT cds;
			cds.dwData = (IntPtr)op;
			cds.cbData = 0;
			cds.lpData = IntPtr.Zero;

			var cdsBuffer = Marshal.AllocHGlobal(Marshal.SizeOf(cds));
			Marshal.StructureToPtr(cds, cdsBuffer, false);

			this.InvokeIfRequired((MethodInvoker)delegate
			{
				WinApi.SendMessage(hWnd, WinApi.WM_COPYDATA, this.Handle, cdsBuffer);
			});
		}

		/// <summary>
		/// Window message handler, handles incoming data from Alissa.
		/// </summary>
		/// <param name="m"></param>
		protected override void WndProc(ref Message m)
		{
			if (m.Msg == WinApi.WM_COPYDATA)
			{
				var cds = (WinApi.COPYDATASTRUCT)Marshal.PtrToStructure(m.LParam, typeof(WinApi.COPYDATASTRUCT));

				if (cds.cbData < 12)
					return;

				var recv = (int)cds.dwData == Sign.Recv;

				var data = new byte[cds.cbData];
				Marshal.Copy(cds.lpData, data, 0, cds.cbData);

				var packet = new Packet(data, 0);
				var msg = new Msg(packet, DateTime.Now, recv);

				FrmDpsMeter.packetQueue.Enqueue(msg);
			}
			base.WndProc(ref m);
		}

		public void Disconnect()
		{
			if (alissaHWnd != IntPtr.Zero)
				SendAlissa(alissaHWnd, Sign.Disconnect);
		}
	}
}
