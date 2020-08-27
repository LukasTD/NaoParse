using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Drawing;
using Be.Timvw.Framework.ComponentModel;
using NaoParse.Util;
using System.Data;
using NaoParse.AlissaWindow;
using NaoParse.Parsing;
using System.Threading;
using System.Reflection;

namespace NaoParse
{
	public partial class FrmDpsMeter : Form
	{
		private SortableBindingList<DamageMeterRow> AttackerList = new SortableBindingList<DamageMeterRow>();
		private Stopwatch watch = new Stopwatch();
		private float totalDamage;
		private SafeDictionary<long, string> characters = new SafeDictionary<long, string>();
		private HashSet<int> packetIdSet = new HashSet<int>();

		// pale, source : https://github.com/exectails/MabiPale2
		public static Queue<Msg> packetQueue = new Queue<Msg>();
		private Alissa invisWindow;

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

			foreach (DataGridViewColumn column in encounterDataGridView.Columns)
			{
				column.SortMode = DataGridViewColumnSortMode.NotSortable;
			}

			encounterDataGridView.Columns[1].DefaultCellStyle.Format = "N0";
			encounterDataGridView.Columns[2].DefaultCellStyle.Format = "P1";

			// double buffering
			typeof(DataGridView).InvokeMember("DoubleBuffered", BindingFlags.NonPublic |
			BindingFlags.Instance | BindingFlags.SetProperty, null,
			encounterDataGridView, new object[] { true });

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

			// Creating an invisible message receiver window in a separate thread
			Thread invisWindowThread = new Thread(new ThreadStart(() =>
			{
				invisWindow = new Alissa();
				invisWindow.Show();
				invisWindow.Visible = false;
				System.Windows.Threading.Dispatcher.Run();
			}));

			// Starting the thread
			invisWindowThread.SetApartmentState(ApartmentState.STA);
			invisWindowThread.IsBackground = true;
			invisWindowThread.Start();

			backgroundWorker1.RunWorkerAsync();
		}

		// timer tick
		private void timer1_Tick(object sender, EventArgs e)
		{
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
			var list = new BindingList<DamageMeterRow>(AttackerList.OrderByDescending(x => x.DamageSum).ToList());
			var s = "";
			foreach (var a in list)
			{
				s += $"{a.Name} | {a.DamageSum} | {a.DamagePercent.ToString("P1", CultureInfo.CurrentCulture)} | {a.MaxHit}\n";
			}
			if (s != "")
			{
				s = s.Remove(s.Length - 1);
				var result = "Name | Damage | % | Max Hit\n" + s;
				Clipboard.SetText(result);
			}
		}

		private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
		{
			while (true)
			{
				Thread.Sleep(250);

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
					Console.Write("damage packet");
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

					var attacker = new DamageMeterRow
					{
						entityId = payload.Attacker,
						Name = name,
						DamageSum = payload.Damage,
						skillId = SkillIdHelper.GetSkillName((ushort)payload.SkillId),
					};

					WriteDamageLog(attacker);
				}
			}
		}

		private void WriteDamageLog(DamageMeterRow a)
		{
			SortableBindingList<DamageMeterRow> updateList;
			// if we got new combat data, and we clicked reset, reset the meter
			if (!watch.IsRunning)
			{
				totalDamage = 0;
				updateList = new SortableBindingList<DamageMeterRow>();
				watch.Restart();
			}
			else
				updateList = new SortableBindingList<DamageMeterRow>(AttackerList.ToList());

			totalDamage += a.DamageSum; // adding to the total damage of the parser

			bool found = false;
			foreach (var attacker in updateList)
			{
				if (attacker.Name == a.Name)
				{
					found = true;
					attacker.DamageSum += a.DamageSum;
					if (attacker.highestDamage < a.DamageSum)
					{
						attacker.highestDamage = a.DamageSum;
						attacker.skillId = a.skillId;
					}
				}
				attacker.DamagePercent = attacker.DamageSum / totalDamage;
			}

			if (!found)
			{
				var row = new DamageMeterRow
				{
					Name = a.Name,
					DamageSum = a.DamageSum,
					highestDamage = a.DamageSum,
					entityId = a.entityId,
					skillId = a.skillId,
					DamagePercent = a.DamageSum / totalDamage
				};
				updateList.Add(row);
			}

			// sorting the list as descending
			AttackerList = new SortableBindingList<DamageMeterRow>(updateList.OrderByDescending(x => x.DamagePercent).ToList());

			this.InvokeIfRequired((MethodInvoker)delegate
			{
				// total damage display
				label2.Text = $"- {totalDamage:n0} DMG";
				encounterDataGridView.DataSource = AttackerList;
			});
		}

		private void FrmDpsMeter_FormClosed(object sender, FormClosedEventArgs e)
		{
			if (invisWindow != null)
				invisWindow.Disconnect();
		}
	}

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
