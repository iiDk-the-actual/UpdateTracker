using System;
using GorillaExtensions;
using GorillaGameModes;
using GorillaNetworking;

namespace GorillaTagScripts.VirtualStumpCustomMaps
{
	public class VirtualStumpModeSelectButton : ModeSelectButton
	{
		public override void ButtonActivationWithHand(bool isLeftHand)
		{
			if (this.warningScreen.ShouldShowWarning)
			{
				this.warningScreen.Show();
			}
			else
			{
				GorillaComputer.instance.SetGameModeWithoutButton(this.gameMode);
			}
			if (GorillaComputer.instance.IsPlayerInVirtualStump() && RoomSystem.JoinedRoom && NetworkSystem.Instance.LocalPlayer.IsMasterClient && NetworkSystem.Instance.SessionIsPrivate)
			{
				if (GameMode.ActiveGameMode.IsNull())
				{
					GameMode.ChangeGameMode(this.gameMode);
					return;
				}
				if (GameMode.ActiveGameMode.GameType().ToString().ToLower() != this.gameMode.ToLower())
				{
					GameMode.ChangeGameMode(this.gameMode);
				}
			}
		}
	}
}
