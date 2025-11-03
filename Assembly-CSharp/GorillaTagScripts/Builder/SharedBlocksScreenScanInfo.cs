using System;
using TMPro;
using UnityEngine;

namespace GorillaTagScripts.Builder
{
	public class SharedBlocksScreenScanInfo : SharedBlocksScreen
	{
		public override void OnUpPressed()
		{
		}

		public override void OnDownPressed()
		{
		}

		public override void OnSelectPressed()
		{
			this.terminal.OnLoadMapPressed();
		}

		public override void Show()
		{
			base.Show();
			this.DrawScreen();
		}

		private void DrawScreen()
		{
			if (this.terminal.SelectedMap == null)
			{
				this.mapIDText.text = "MAP ID: NONE";
				return;
			}
			this.mapIDText.text = "MAP ID: " + SharedBlocksTerminal.MapIDToDisplayedString(this.terminal.SelectedMap.MapID);
		}

		[SerializeField]
		private TMP_Text mapIDText;
	}
}
