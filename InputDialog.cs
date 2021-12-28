using System;
using System.Text;
using Sandbox;
using Sandbox.Game.GUI;
using Sandbox.Game.Localization;
using Sandbox.Graphics.GUI;
using VRage;
using VRage.Audio;
using VRage.Game;
using VRage.Input;
using VRage.Utils;
using VRageMath;

namespace PCUDisplay
{
	public class InputDialog<T> : MyGuiScreenBase
	{
		public InputDialog(Action<T> callBack, T defaultValue, string caption = "", Vector2 position = default(Vector2), int maxLength = 20, float textBoxWidth = 0.2f)
			: base(new Vector2?(position), new Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR * MySandboxGame.Config.UIBkOpacity), new Vector2?(new Vector2(0.5f, 0.3f)), true, null, 0f, 0f, null)
		{
			maxTextLength = maxLength;
			this.caption = caption;
			this.callBack = callBack;
			this.defaultValue = defaultValue;
			RecreateControls(true);
			OnEnterCallback = delegate ()
			{
				OnOk(okButton);
			};
			CanBeHidden = true;
			CanHideOthers = true;
			CloseButtonEnabled = true;
		}

		private void CallResultCallback(T val)
		{
			if (val != null)
			{
				callBack(val);
			}
		}

		public override string GetFriendlyName()
		{
			return "InputDialog";
		}

		public override void HandleInput(bool receivedFocusInThisUpdate)
		{
			HandleInput(receivedFocusInThisUpdate);
			if (receivedFocusInThisUpdate)
			{
				return;
			}
			if (MyControllerHelper.IsControl(MyControllerHelper.CX_GUI, MyControlsGUI.BUTTON_X, MyControlStateType.NEW_PRESSED, false, false))
			{
				OnOk(null);
			}
			if (MyControllerHelper.IsControl(MyControllerHelper.CX_GUI, MyControlsGUI.BUTTON_B, MyControlStateType.NEW_PRESSED, false, false))
			{
				OnCancel(null);
			}
		}

		private void OnCancel(MyGuiControlButton button)
		{
			CloseScreen(false);
		}

		private void OnOk(MyGuiControlButton button)
		{
			if (textBox.GetTextLength() > 0)
			{
				StringBuilder stringBuilder = new StringBuilder();
				textBox.GetText(stringBuilder);
				try
				{
					object obj = Convert.ChangeType(stringBuilder.ToString(), typeof(T));
					if (obj is T)
					{
						CallResultCallback((T)obj);
						CloseScreen(false);
					}
					else
					{
						MyGuiAudio.PlaySound(MyGuiSounds.HudUnable);
					}
				}
				catch
				{
					MyGuiAudio.PlaySound(MyGuiSounds.HudErrorMessage);
				}
			}
		}

		public override void RecreateControls(bool constructor)
		{
			RecreateControls(constructor);
			AddCaption(caption, new Vector4?(Color.White.ToVector4()), new Vector2?(new Vector2(0f, 0.003f)), 0.8f);
			MyGuiControlSeparatorList myGuiControlSeparatorList = new MyGuiControlSeparatorList();
			myGuiControlSeparatorList.AddHorizontal(new Vector2(0f, 0f) - new Vector2(m_size.Value.X * 0.78f / 2f, m_size.Value.Y / 2f - 0.075f), m_size.Value.X * 0.78f, 0f, null);
			Controls.Add(myGuiControlSeparatorList);
			MyGuiControlSeparatorList myGuiControlSeparatorList2 = new MyGuiControlSeparatorList();
			myGuiControlSeparatorList2.AddHorizontal(new Vector2(0f, 0f) - new Vector2(m_size.Value.X * 0.78f / 2f, -m_size.Value.Y / 2f + 0.123f), m_size.Value.X * 0.78f, 0f, null);
			Controls.Add(myGuiControlSeparatorList2);
			textBox = new MyGuiControlTextbox(new Vector2?(new Vector2(0f, -0.027f)), null, maxTextLength, null, 0.8f, MyGuiControlTextboxType.Normal, MyGuiControlTextboxStyleEnum.Default, false);
			textBox.SetText(new StringBuilder(defaultValue.ToString()));
			textBox.Size = new Vector2(0.385f, 1f);
			Controls.Add(textBox);
			okButton = new MyGuiControlButton(null, MyGuiControlButtonStyleEnum.Default, null, null, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.Ok), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_CURSOR_OVER, new Action<MyGuiControlButton>(OnOk), GuiSounds.MouseClick, 1f, null, false, false, false, null);
			cancelButton = new MyGuiControlButton(null, MyGuiControlButtonStyleEnum.Default, null, null, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.Cancel), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_CURSOR_OVER, new Action<MyGuiControlButton>(OnCancel), GuiSounds.MouseClick, 1f, null, false, false, false, null);
			Vector2 value = new Vector2(0.002f, m_size.Value.Y / 2f - 0.071f);
			Vector2 value2 = new Vector2(0.018f, 0f);
			okButton.Position = value - value2;
			cancelButton.Position = value + value2;
			okButton.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipNewsletter_Ok));
			cancelButton.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipOptionsSpace_Cancel));
			Controls.Add(okButton);
			Controls.Add(cancelButton);
		}

		private Action<T> callBack;

		private MyGuiControlButton cancelButton;

		private string caption;

		private T defaultValue;

		private int maxTextLength;

		private MyGuiControlButton okButton;

		private MyGuiControlTextbox textBox;
	}
}
