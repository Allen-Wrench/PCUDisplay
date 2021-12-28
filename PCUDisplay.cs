using System;
using System.Collections.Generic;
using System.IO;
using Sandbox.Game.Entities;
using Sandbox.Game.World;
using Sandbox.Graphics;
using Sandbox.Graphics.GUI;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.Input;
using VRage.Utils;
using VRageMath;

namespace PCUDisplay
{
	[MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
	public class PCUDisplay : MySessionComponentBase
	{
		public PCUDisplay()
		{
			Static = this;
			textHeight = new Vector2(0f, MyGuiManager.GetFontHeight("Debug", 1f));
			minMouseCoord = MyGuiManager.GetMinMouseCoord();
			maxMouseCoord = MyGuiManager.GetMaxMouseCoord();
		}

		public override void Draw()
		{
			if (ShowPCU && lastusedgrid != null)
			{
				MyGuiManager.DrawString("Debug", gridName, textPos, 0.8f, new Color?(gridPCUColor), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, float.PositiveInfinity, false);
				MyGuiManager.DrawString("Debug", gridPCU, textPos + textHeight, 1f, new Color?(gridPCUColor), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, float.PositiveInfinity, false);
			}
		}

		public void GetConnectedGrids(MyCubeGrid cubegrid)
		{
			if (lastusedgridgroup.Count > 0)
			{
				foreach (MyCubeGrid myCubeGrid in lastusedgridgroup)
				{
					myCubeGrid.OnGridChanged -= GetGridsPCU;
				}
				lastusedgridgroup.Clear();
			}
			MyCubeGridGroups.Static.GetGroups(GridLinkTypeEnum.Physical).GetGroupNodes(cubegrid, lastusedgridgroup);
			foreach (MyCubeGrid myCubeGrid2 in lastusedgridgroup)
			{
				myCubeGrid2.OnGridChanged += GetGridsPCU;
			}
			GetGridsPCU(cubegrid);
		}

		public void GetGridsPCU(MyCubeGrid grid)
		{
			int num = 0;
			foreach (MyCubeGrid myCubeGrid in lastusedgridgroup)
			{
				if (myCubeGrid != null)
				{
					num += myCubeGrid.BlocksPCU;
				}
			}
			gridPCU = num.ToString();
			gridName = lastusedgrid.DisplayName;
			nameSize = MyGuiManager.MeasureString("Debug", gridName, 0.8f);
			pcuSize = MyGuiManager.MeasureString("Debug", gridPCU, 1f);
			if (pcuSize.X > nameSize.X)
			{
				drawSize.X = pcuSize.X;
			}
			else
			{
				drawSize.X = nameSize.X;
			}
			textHeight.X = nameSize.X / 2f - pcuSize.X / 2f;
			drawSize.Y = nameSize.Y + pcuSize.Y;
			gridPCUColor = Color.Lime;
			if ((double)num > (double)pcuLimit * 0.8)
			{
				gridPCUColor = Color.GreenYellow;
			}
			if ((double)num > (double)pcuLimit * 0.9)
			{
				gridPCUColor = Color.Yellow;
			}
			if (num >= pcuLimit)
			{
				gridPCUColor = Color.Red;
			}
		}

		public override void HandleInput()
		{
			if (MyInput.Static.IsAnyCtrlKeyPressed() && MyInput.Static.IsNewKeyPressed(keyBind))
			{
				ShowPCU = !ShowPCU;
				if (ShowPCU && MyCurrentGrid != null)
				{
					lastusedgrid = MyCurrentGrid;
					if (pcuLimit == 0)
					{
						SetPCULimit();
					}
				}
				if (ShowPCU && lastusedgrid != null)
				{
					lastusedgrid.OnHierarchyUpdated += GetConnectedGrids;
					lastusedgrid.OnClose += delegate (MyEntity ent)
					{
						Off();
					};
					GetConnectedGrids(lastusedgrid);
				}
				if (!ShowPCU && lastusedgrid != null)
				{
					lastusedgrid.OnHierarchyUpdated -= GetConnectedGrids;
				}
				return;
			}
			if (MyInput.Static.IsAnyShiftKeyPressed() && MyInput.Static.IsNewKeyPressed(keyBind))
			{
				SetPCULimit();
				return;
			}
			if (ShowPCU && MyInput.Static.IsAnyAltKeyPressed() && MyInput.Static.IsAnyShiftKeyPressed() && MyInput.Static.IsRightMousePressed())
			{
				Vector2 normalizedMousePosition = MyGuiManager.GetNormalizedMousePosition(MyInput.Static.GetMousePosition(), MyInput.Static.GetMouseAreaSize());
				textPos = Vector2.Clamp(normalizedMousePosition, minMouseCoord, maxMouseCoord - drawSize);
			}
		}

		public override void LoadData()
		{
			base.LoadData();
			if (!MyAPIGateway.Utilities.FileExistsInLocalStorage("PCUDisplay_Config.xml", typeof(PCUDisplay)))
			{
				Settings = new Config();
				return;
			}
			using (TextReader textReader = MyAPIGateway.Utilities.ReadFileInLocalStorage("PCUDisplay_Config.xml", typeof(PCUDisplay)))
			{
				Settings = MyAPIGateway.Utilities.SerializeFromXML<Config>(textReader.ReadToEnd());
			}
		}

		public void Off()
		{
			if (ShowPCU && lastusedgrid != null)
			{
				lastusedgrid.OnHierarchyUpdated -= GetConnectedGrids;
			}
			if (lastusedgridgroup.Count > 0)
			{
				foreach (MyCubeGrid myCubeGrid in lastusedgridgroup)
				{
					myCubeGrid.OnGridChanged -= GetGridsPCU;
				}
				lastusedgridgroup.Clear();
			}
			gridName = "-";
			gridPCU = "-";
			gridPCUColor = Color.White;
			ShowPCU = false;
		}

		public override void SaveData()
		{
			base.SaveData();
			using (TextWriter textWriter = MyAPIGateway.Utilities.WriteFileInLocalStorage("PCUDisplay_Config.xml", typeof(PCUDisplay)))
			{
				textWriter.Write(MyAPIGateway.Utilities.SerializeToXML(Settings));
			}
		}

		private void SetPCULimit()
		{
			MyGuiSandbox.AddScreen(new InputDialog<int>(delegate (int num)
			{
				pcuLimit = num;
				SaveData();
			}, 0, "Enter max PCU limit (optional: for display color)", new Vector2(0.5f), 20, 0.2f));
		}

		protected override void UnloadData()
		{
			base.UnloadData();
			if (ShowPCU)
			{
				Off();
			}
		}

		private MyCubeGrid MyCurrentGrid
		{
			get
			{
				MyCubeBlock myCubeBlock = MySession.Static.LocalHumanPlayer.Controller.ControlledEntity as MyCubeBlock;
				if (myCubeBlock == null)
				{
					return null;
				}
				return myCubeBlock.CubeGrid;
			}
		}

		private Config Settings
		{
			get
			{
				return new Config(textPos, pcuLimit, keyBind);
			}
			set
			{
				textPos = value.ScreenPosition;
				pcuLimit = value.PCULimit;
				keyBind = value.Keybind;
			}
		}

		private Vector2 drawSize;

		private string gridName;

		private string gridPCU;

		private Color gridPCUColor;

		private MyKeys keyBind;

		private MyCubeGrid lastusedgrid;

		private List<MyCubeGrid> lastusedgridgroup = new List<MyCubeGrid>();

		private Vector2 maxMouseCoord;

		private Vector2 minMouseCoord;

		private Vector2 nameSize;

		private int pcuLimit;

		private Vector2 pcuSize;

		private bool ShowPCU;

		public static PCUDisplay Static;

		private Vector2 textHeight;

		private Vector2 textPos;

		[Serializable]
		public class Config
		{
			public Config()
			{
				ScreenPosition = default;
				PCULimit = 0;
				Keybind = MyKeys.NumPad0;
			}

			public Config(Vector2 pos, int pcuLimit, MyKeys keybind)
			{
				ScreenPosition = pos;
				PCULimit = pcuLimit;
				Keybind = keybind;
			}

			public MyKeys Keybind;

			public int PCULimit;

			public Vector2 ScreenPosition;
		}
	}
}
