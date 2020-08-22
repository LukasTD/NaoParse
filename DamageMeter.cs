using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Drawing;
using Be.Timvw.Framework.ComponentModel;
using NaoParse.Util; // Pale
using System.Data;
using System.Runtime.InteropServices;
using NaoParse.Parsing; // Pale
using System.Threading;

namespace NaoParse
{
	public partial class FrmDpsMeter : Form
	{
		private SortableBindingList<DamageMeterRow> AttackerList = new SortableBindingList<DamageMeterRow>();
		private Dictionary<string, DamageCount> players = new Dictionary<string, DamageCount>();
		private Stopwatch watch = new Stopwatch();
		private float totalDamage;
		private SafeDictionary<long, string> characters = new SafeDictionary<long, string>();
		private HashSet<int> packetIdSet = new HashSet<int>();

		// pale, source : https://github.com/exectails/MabiPale2
		private IntPtr alissaHWnd;
		private Queue<Msg> packetQueue;

		public FrmDpsMeter()
		{
			InitializeComponent();
		}

		private void FrmDpsMeter_Load(object sender, EventArgs e)
		{
			// data grid style
			encounterDataGridView.DataSource = AttackerList;
			encounterDataGridView.Columns[1].HeaderText = "Damage";
			encounterDataGridView.Columns[2].HeaderText = "%";
			encounterDataGridView.Columns[3].HeaderText = "Max Hit";
			encounterDataGridView.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
			encounterDataGridView.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
			encounterDataGridView.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
			encounterDataGridView.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			encounterDataGridView.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
			encounterDataGridView.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
			encounterDataGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(65, 93, 137);
			encounterDataGridView.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
			encounterDataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Tahoma", 14, FontStyle.Bold, GraphicsUnit.Pixel);
			encounterDataGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
			encounterDataGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Sunken;
			encounterDataGridView.RowsDefaultCellStyle.Font = new Font("Bahnschrift", 13, FontStyle.Regular, GraphicsUnit.Pixel);
			encounterDataGridView.RowsDefaultCellStyle.BackColor = Color.FromArgb(90, 93, 107);
			encounterDataGridView.RowsDefaultCellStyle.ForeColor = Color.White;
			encounterDataGridView.BackgroundColor = Color.FromArgb(72, 74, 84);
			encounterDataGridView.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(72, 74, 84);
			encounterDataGridView.EnableHeadersVisualStyles = false;

			// don't allow user sorting, maybe revisit in the future.
			foreach (DataGridViewColumn column in encounterDataGridView.Columns)
			{
				column.SortMode = DataGridViewColumnSortMode.NotSortable;
			}

			// other styles
			menuStrip1.Renderer = new ToolStripProfessionalRenderer(new MyColorTable());
			menuStrip1.BackColor = Color.FromArgb(65, 93, 137);
			menuStrip1.ForeColor = Color.White;
			menuStrip1.Font = new Font("Tahoma", 12F, FontStyle.Bold, GraphicsUnit.Pixel);
			alwaysOnTopToolStripMenuItem.Checked = true;
			TopMost = true;
			alwaysOnTopToolStripMenuItem.CheckOnClick = true;
			alwaysOnTopToolStripMenuItem.BackColor = Color.FromArgb(65, 93, 137);
			alwaysOnTopToolStripMenuItem.ForeColor = Color.White;
			alwaysOnTopToolStripMenuItem.Font = new Font("Tahoma", 12F, FontStyle.Bold, GraphicsUnit.Pixel);

			opacityToolStripMenuItem.BackColor = Color.FromArgb(65, 93, 137);
			opacityToolStripMenuItem.ForeColor = Color.White;
			opacityToolStripMenuItem.Font = new Font("Tahoma", 12F, FontStyle.Bold, GraphicsUnit.Pixel);

			exportToolStripMenuItem.BackColor = Color.FromArgb(65, 93, 137);
			exportToolStripMenuItem.ForeColor = Color.White;
			exportToolStripMenuItem.Font = new Font("Tahoma", 12F, FontStyle.Bold, GraphicsUnit.Pixel);

			Opacity = this.Opacity = (100 - 0) / (double)100;
			toolStripComboBox1.Text = (100 - (Opacity * 100)).ToString();

			timer1.Enabled = true;
			timer1.Interval = 250;

			packetQueue = new Queue<Msg>();
			backgroundWorker1.RunWorkerAsync();
			// connects to alissa
			Connect();
		}

		// timer tick
		private void timer1_Tick(object sender, EventArgs e)
		{
			// total damage display
			label2.Text = $"- {String.Format("{0:n0}", totalDamage)} DMG";

			// updates time if stopwatch is running
			if (watch.IsRunning)
			{
				label1.Text = string.Format("{0:hh\\:mm\\:ss}", watch.Elapsed);
			}
		}

		// pressing of the reset button
		private void resetBttn_Click(object sender, EventArgs e)
		{
			if (watch.IsRunning)
			{
				watch.Stop();
			}
		}

		// always on top toggle
		private void alwaysOnTopToolStripMenuItem_Click(object sender, EventArgs e)
		{
			TopMost ^= true;
		}

		// changing opacity
		private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			Opacity = (100 - int.Parse(toolStripComboBox1.SelectedItem.ToString())) / (double)100;
		}

		// copying log to cliboard
		private void exportToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var list = new BindingList<DamageMeterRow>(AttackerList.OrderByDescending(x => x.GetDamageCount().DamageSum).ToList());
			var s = "";
			foreach (var a in list)
			{
				s += $"{a.Name} | {a.DamageSum} | {a.DamagePercent} | {a.MaxHit}\n";
			}
			if (s != "")
			{
				s = s.Remove(s.Length - 1);
				var result = "Name | Damage | % | Max Hit\n" + s;
				Clipboard.SetText(result);
			}
		}

		private void writeDamageLog(string attackerName, float damage, string skillId)
		{
			// if we got new combat data, and we clicked reset, reset the meter
			if (!watch.IsRunning)
			{
				AttackerList.Clear();
				players.Clear();
				totalDamage = 0;
				watch.Restart();
			}

			totalDamage += damage; // adding to the total damage of the parser

			// if we don't have the player object in the dictionary
			if (!players.ContainsKey(attackerName))
			{
				var d = new DamageCount
				{
					DamageSum = damage,
					HighestDamage = damage,
					SkillId = skillId
				};
				players.Add(attackerName, d);
				var row = new DamageMeterRow(d, attackerName);
				AttackerList.Add(row);
			}
			else // existing player
			{
				players[attackerName].DamageSum += damage;
				if (players[attackerName].HighestDamage < damage)
				{
					players[attackerName].HighestDamage = damage;
					players[attackerName].SkillId = skillId;
				}
			}

			// updating the damage % based on all players
			foreach (var p in players)
			{
				p.Value.DamagePercent = (p.Value.DamageSum / totalDamage).ToString("P1", CultureInfo.CurrentCulture);
			}

			// sorting the list as descending
			encounterDataGridView.Sort(encounterDataGridView.Columns[2], ListSortDirection.Descending);
		}

		private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
		{
			while (true)
			{
				Thread.Sleep(250);

				if (!WinApi.IsWindow(alissaHWnd))
					Disconnect();

				var count = packetQueue.Count;
				if (count == 0)
					continue;

				var newPackets = new List<Msg>();
				for (int i = 0; i < count; ++i)
				{
					Msg packet;
					lock (packetQueue)
						packet = packetQueue.Dequeue();

					if (packet == null)
						continue;

					newPackets.Add(packet);
				}

				foreach (var packet in newPackets)
				{
					if (packet.Received)
						onRecv(packet);
				}
			}
		}

		private void onRecv(Msg msg)
		{
			switch (msg.Op)
			{
				case Op.ChannelCharacterInfoRequestR:
					{
						bool hasCreature = msg.Packet.GetBool();
						if (hasCreature)
						{
							var eid = msg.Packet.GetLong();
							if (EntityId.IsCharacter(eid))
							{
								msg.Packet.GetByte();
								string name = msg.Packet.GetString();
								characters.SafeSet(eid, name);
							}
						}
						break;
					}
				case Op.PuppetControl:
					{
						if (msg.Packet.Peek() == PacketElementType.Short)
						{
							ushort skillId = msg.Packet.GetUShort();
							if (skillId != (ushort)SkillId.PierrotMarionette && skillId != (ushort)SkillId.ColossusMarionette)
							{
								return;
							}

							var eid = msg.Packet.GetLong();
							characters[eid] = characters.SafeGet(msg.Packet.Id);
						}
						break;
					}
				case Op.EntityAppears:
					{
						var eid = msg.Packet.GetLong();
						if (EntityId.IsCharacter(eid))
						{
							msg.Packet.GetByte();
							var name = msg.Packet.GetString();
							characters.SafeSet(eid, name);
						}
						break;
					}
				case Op.EntitiesAppear:
					{
						var count = msg.Packet.GetShort();
						for (var i = 0; i < count; i++)
						{
							var type = msg.Packet.GetShort();
							msg.Packet.GetInt();
							var data = msg.Packet.GetBin();
							Packet subMsg = new Packet(data, 0);
							var eid = subMsg.GetLong();
							if (EntityId.IsCharacter(eid))
							{
								if (type == 16)
								{
									subMsg.GetByte();
									var name = subMsg.GetString();
									characters.SafeSet(eid, name);
								}
							}
						}
						break;
					}
				case Op.CombatActionPack:
					ProcessCombatPacket(msg);
					break;
			}
		}

		private void ProcessCombatPacket(Msg msg)
		{

			int id = msg.Packet.GetInt();
			if (packetIdSet.Contains(id))
			{
				return;
			}
			else
			{
				packetIdSet.Add(id);
			}
			int prevId = msg.Packet.GetInt();

			byte hit = msg.Packet.GetByte();
			if (hit == 0)
			{
				return;
			}
			byte maxHits = msg.Packet.GetByte();
			msg.Packet.GetByte();
			msg.Packet.GetByte();

			var count = msg.Packet.GetInt();
			var payload = new CombatActionPayload();
			for (int i = 0; i < count; ++i)
			{
				var len = msg.Packet.GetInt();
				if (msg.Packet.Peek() != PacketElementType.Bin)
				{
					msg.Packet.GetLong();
					count = msg.Packet.GetInt();
					len = msg.Packet.GetInt();
				}
				var buff = msg.Packet.GetBin();

				var actionPacket = new Packet(buff, 0);
				actionPacket.GetInt();

				var creatureEntityId = actionPacket.GetLong();
				payload.CreatureEntityId = creatureEntityId;
				var type = (CombatActionType)actionPacket.GetByte();
				payload.Type = type;

				var attackeraction = len < 86 && type != 0; // Hot fix, TODO: Proper check of type.
				payload.IsAttackerAction = attackeraction;

				short stun = actionPacket.GetShort();
				ushort skillId = actionPacket.GetUShort();
				actionPacket.GetShort();
				if (actionPacket.Peek() == PacketElementType.Short)
					actionPacket.GetShort(); // [200300, NA258 (2017-08-19)] ? 

				// AttackerAction
				if (attackeraction)
				{
					payload.SkillId = (SkillId)skillId;
					if (actionPacket.Peek() != PacketElementType.None)
					{
						long target = actionPacket.GetLong();

						var options = new List<uint>();
						var topt = actionPacket.GetInt();
						for (uint foo2 = 1; foo2 < 0x80000000;)
						{
							if ((topt & foo2) != 0)
								options.Add(foo2);
							foo2 <<= 1;
						}
						var strOptions = string.Join(", ", options.Select(a =>
						{
							var en = (AttackerOptions)a;
							return "0x" + a.ToString("X2") + (en.ToString() != a.ToString() ? "(" + en + ")" : "");
						}));


						actionPacket.GetByte();
						actionPacket.GetByte();
						actionPacket.GetInt();
						int x = actionPacket.GetInt();
						int y = actionPacket.GetInt();
						long prop = 0L;
						if (actionPacket.NextIs(PacketElementType.Long))
						{
							prop = actionPacket.GetLong();

						}
					}
				}
				// TargetAction
				else
				{
					// Target actions might end here, widnessed with a packet
					// that had "97" as the previous short.
					if (actionPacket.Peek() != PacketElementType.None)
					{
						// Target used Defense or Counter
						if (type.HasFlag(CombatActionType.Unk) || type.HasFlag(CombatActionType.Defended) || type.HasFlag(CombatActionType.CounteredHit) || type.HasFlag((CombatActionType)0x73) || type.HasFlag((CombatActionType)0x13))
						{
							var attackerEntityId = actionPacket.GetLong();
							actionPacket.GetInt();
							actionPacket.GetByte();
							actionPacket.GetByte();
							actionPacket.GetInt();
							var x = actionPacket.GetInt();
							var y = actionPacket.GetInt();
						}

						if (actionPacket.Peek() == PacketElementType.Long) // fighter counter ?
						{
							actionPacket.GetLong();
						}

						var options = new List<uint>();
						var topt = actionPacket.GetInt();
						for (uint foo2 = 1; foo2 < 0x80000000;)
						{
							if ((topt & foo2) != 0)
								options.Add(foo2);
							foo2 <<= 1;
						}
						payload.TargetOptions = new List<TargetOptions>();
						foreach (TargetOptions option in options)
						{
							payload.TargetOptions.Add(option);
						}
						var strOptions = string.Join(", ", options.Select(a =>
						{
							var en = (TargetOptions)a;
							return "0x" + a.ToString("X2") + (en.ToString() != a.ToString() ? "(" + en + ")" : "");
						}));

						float damage = actionPacket.GetFloat();
						payload.Damage = damage;
						float woundDamage = actionPacket.GetFloat();
						payload.WoundDamage = woundDamage;
						int manaDamage = actionPacket.GetInt();
						payload.ManaDamage = manaDamage;

						if (actionPacket.NextIs(PacketElementType.Int))
							actionPacket.GetInt(); // [210100, NA280 (2018-06-14)]

						float xDiff = actionPacket.GetFloat();
						float yDiff = actionPacket.GetFloat();
						if (actionPacket.NextIs(PacketElementType.Float))
						{
							float newX = actionPacket.GetFloat();
							float newY = actionPacket.GetFloat();

							// [190200, NA203 (22.04.2015)]
							if (actionPacket.Peek() == PacketElementType.Int)
							{
								actionPacket.GetInt();
							}
						}

						while (actionPacket.NextIs(PacketElementType.Int))
						{ // ??
							actionPacket.GetInt();
						}

						byte effectiveFlags = actionPacket.GetByte();
						int delay = actionPacket.GetInt();
						long attacker = actionPacket.GetLong();
						payload.Attacker = attacker;
					}
				}

				// pushing payload to logfile
				if (payload.SkillId != SkillId.None && characters.ContainsKey(payload.Attacker))
				{
					string name = characters.SafeGet(payload.Attacker);
					if (name == null)
					{
						return;
					}

					BeginInvoke((MethodInvoker)delegate
					{
						writeDamageLog(name, payload.Damage, SkillIdHelper.GetSkillName((ushort)payload.SkillId));
					});
				}
			}
		}

		#region Pale Code
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

				lock (packetQueue)
					packetQueue.Enqueue(msg);
			}
			base.WndProc(ref m);
		}

		// disconnets alissa when the program is closed
		private void FrmDpsMeter_FormClosed(object sender, FormClosedEventArgs e)
		{
			Disconnect();
		}
		private void Disconnect()
		{
			if (alissaHWnd != IntPtr.Zero)
				SendAlissa(alissaHWnd, Sign.Disconnect);
		}
	}
	#endregion // pale, source : https://github.com/exectails/MabiPale2

	#region MyColorTable
	public class MyColorTable : ProfessionalColorTable
	{
		public override Color ToolStripDropDownBackground {
			get {
				return Color.FromArgb(65, 93, 137);
			}
		}

		public override Color ImageMarginGradientBegin {
			get {
				return Color.FromArgb(65, 93, 137);
			}
		}

		public override Color ImageMarginGradientMiddle {
			get {
				return Color.FromArgb(65, 93, 137);
			}
		}

		public override Color ImageMarginGradientEnd {
			get {
				return Color.FromArgb(65, 93, 137);
			}
		}

		public override Color MenuBorder {
			get {
				return Color.Black;
			}
		}

		public override Color MenuItemBorder {
			get {
				return Color.Black;
			}
		}

		public override Color MenuItemSelected {
			get {
				return Color.Navy;
			}
		}

		public override Color MenuStripGradientBegin {
			get {
				return Color.FromArgb(65, 93, 137);
			}
		}

		public override Color MenuStripGradientEnd {
			get {
				return Color.FromArgb(65, 93, 137);
			}
		}

		public override Color MenuItemSelectedGradientBegin {
			get {
				return Color.Navy;
			}
		}

		public override Color MenuItemSelectedGradientEnd {
			get {
				return Color.Navy;
			}
		}

		public override Color MenuItemPressedGradientBegin {
			get {
				return Color.FromArgb(65, 93, 137);
			}
		}

		public override Color MenuItemPressedGradientEnd {
			get {
				return Color.FromArgb(65, 93, 137);
			}
		}
	}
	#endregion
}
