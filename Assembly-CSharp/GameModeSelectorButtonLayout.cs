using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GorillaGameModes;
using GorillaNetworking;
using UnityEngine;

public class GameModeSelectorButtonLayout : MonoBehaviour
{
	private void OnEnable()
	{
		this.SetupButtons();
		NetworkSystem.Instance.OnJoinedRoomEvent += this.SetupButtons;
	}

	private void OnDisable()
	{
		NetworkSystem.Instance.OnJoinedRoomEvent -= this.SetupButtons;
	}

	public virtual async void SetupButtons()
	{
		int count = 0;
		while (GorillaComputer.instance == null)
		{
			await Task.Delay(100);
		}
		bool flag = GorillaTagger.Instance.offlineVRRig.zoneEntity.currentZone != this.zone;
		foreach (GameModeType gameModeType in GameMode.GameModeZoneMapping.GetModesForZone(this.zone, NetworkSystem.Instance.SessionIsPrivate))
		{
			if (count == this.currentButtons.Count)
			{
				this.currentButtons.Add(Object.Instantiate<ModeSelectButton>(this.pf_button, base.transform));
			}
			ModeSelectButton modeSelectButton = this.currentButtons[count];
			modeSelectButton.transform.localPosition = new Vector3((float)count * -0.15f, 0f, 0f);
			modeSelectButton.transform.localRotation = Quaternion.Euler(0f, 0f, -90f);
			modeSelectButton.WarningScreen = this.warningScreen;
			modeSelectButton.SetInfo(gameModeType.ToString(), GameMode.GameModeZoneMapping.GetModeName(gameModeType), GameMode.GameModeZoneMapping.IsNew(gameModeType), GameMode.GameModeZoneMapping.GetCountdown(gameModeType));
			modeSelectButton.gameObject.SetActive(true);
			count++;
			flag |= GorillaComputer.instance.currentGameMode.Value.ToUpper() == gameModeType.ToString().ToUpper();
		}
		for (int i = count; i < this.currentButtons.Count; i++)
		{
			this.currentButtons[i].gameObject.SetActive(false);
		}
		if (!flag)
		{
			GorillaComputer.instance.SetGameModeWithoutButton(this.currentButtons[0].gameMode);
		}
	}

	[SerializeField]
	protected ModeSelectButton pf_button;

	[SerializeField]
	protected GTZone zone;

	[SerializeField]
	protected PartyGameModeWarning warningScreen;

	protected List<ModeSelectButton> currentButtons = new List<ModeSelectButton>();
}
