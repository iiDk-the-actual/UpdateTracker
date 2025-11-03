using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GorillaGameModes;
using GorillaNetworking;
using TMPro;
using UnityEngine;

namespace GorillaTagScripts.VirtualStumpCustomMaps
{
	public class CustomMapModeSelector : GameModeSelectorButtonLayout
	{
		private void Awake()
		{
			CustomMapModeSelector.instances.AddIfNew(this);
		}

		public void OnEnable()
		{
			if (GorillaComputer.instance != null)
			{
				this.SetupButtons();
				GorillaComputer.instance.SetGameModeWithoutButton(CustomMapModeSelector.defaultGamemodeForLoadedMap.ToString());
			}
			RoomSystem.JoinedRoomEvent += new Action(this.OnJoinedRoom);
			NetworkSystem.Instance.OnMasterClientSwitchedEvent += this.OnRoomHostSwitched;
			NetworkSystem.Instance.OnReturnedToSinglePlayer += this.OnDisconnected;
			this.roomHostDescriptionText.SetActive(false);
			this.roomHostText.gameObject.SetActive(false);
			if (NetworkSystem.Instance.InRoom && NetworkSystem.Instance.SessionIsPrivate)
			{
				this.OnRoomHostSwitched(NetworkSystem.Instance.MasterClient);
			}
		}

		public void OnDisable()
		{
			RoomSystem.JoinedRoomEvent -= new Action(this.OnJoinedRoom);
			NetworkSystem.Instance.OnMasterClientSwitchedEvent -= this.OnRoomHostSwitched;
			NetworkSystem.Instance.OnReturnedToSinglePlayer -= this.OnDisconnected;
		}

		private void OnJoinedRoom()
		{
			this.OnRoomHostSwitched(NetworkSystem.Instance.MasterClient);
		}

		private void OnRoomHostSwitched(NetPlayer newRoomHost)
		{
			if (!NetworkSystem.Instance.InRoom || !NetworkSystem.Instance.SessionIsPrivate)
			{
				return;
			}
			CustomMapModeSelector.reusableString = this.notInRoomHostString;
			if (!newRoomHost.IsNull)
			{
				this.roomHostDescriptionText.SetActive(true);
				CustomMapModeSelector.reusableString = newRoomHost.DefaultName;
				if (GorillaComputer.instance.NametagsEnabled && KIDManager.HasPermissionToUseFeature(EKIDFeatures.Custom_Nametags))
				{
					RigContainer rigContainer;
					if (newRoomHost.IsLocal)
					{
						CustomMapModeSelector.reusableString = newRoomHost.NickName;
					}
					else if (VRRigCache.Instance.TryGetVrrig(newRoomHost, out rigContainer))
					{
						CustomMapModeSelector.reusableString = rigContainer.Rig.playerNameVisible;
					}
				}
			}
			this.roomHostText.text = this.roomHostLabel + CustomMapModeSelector.reusableString;
			this.roomHostText.gameObject.SetActive(true);
		}

		private void OnDisconnected()
		{
			this.roomHostText.gameObject.SetActive(false);
			this.roomHostDescriptionText.SetActive(false);
		}

		public static void ResetButtons()
		{
			CustomMapModeSelector.gamemodes = new List<GameModeType> { GameModeType.Casual };
			CustomMapModeSelector.defaultGamemodeForLoadedMap = GameModeType.Casual;
			foreach (CustomMapModeSelector customMapModeSelector in CustomMapModeSelector.instances)
			{
				customMapModeSelector.SetupButtons();
			}
			GorillaComputer.instance.SetGameModeWithoutButton(CustomMapModeSelector.defaultGamemodeForLoadedMap.ToString());
		}

		public static void SetAvailableGameModes(int[] availableModes, int defaultMode)
		{
			CustomMapModeSelector.gamemodes.Clear();
			CustomMapModeSelector.gamemodes.Add(GameModeType.Casual);
			if (availableModes != null)
			{
				foreach (int num in availableModes)
				{
					CustomMapModeSelector.gamemodes.Add((GameModeType)num);
				}
			}
			CustomMapModeSelector.defaultGamemodeForLoadedMap = (GameModeType)defaultMode;
			foreach (CustomMapModeSelector customMapModeSelector in CustomMapModeSelector.instances)
			{
				customMapModeSelector.SetupButtons();
			}
			GorillaComputer.instance.SetGameModeWithoutButton(CustomMapModeSelector.defaultGamemodeForLoadedMap.ToString());
		}

		public override async void SetupButtons()
		{
			int count = 0;
			while (GorillaComputer.instance == null)
			{
				await Task.Delay(100);
			}
			bool flag = GorillaTagger.Instance.offlineVRRig.zoneEntity.currentZone != this.zone;
			foreach (GameModeType gameModeType in CustomMapModeSelector.gamemodes)
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
		}

		public static void RefreshHostName()
		{
			foreach (CustomMapModeSelector customMapModeSelector in CustomMapModeSelector.instances)
			{
				customMapModeSelector.OnRoomHostSwitched(NetworkSystem.Instance.MasterClient);
			}
		}

		[SerializeField]
		private TMP_Text roomHostText;

		[SerializeField]
		private GameObject roomHostDescriptionText;

		[SerializeField]
		private string notInRoomHostString = "-NOT IN ROOM-";

		[SerializeField]
		private string roomHostLabel = "ROOM HOST: ";

		private static List<GameModeType> gamemodes = new List<GameModeType> { GameModeType.Casual };

		private static GameModeType defaultGamemodeForLoadedMap = GameModeType.Casual;

		private static List<CustomMapModeSelector> instances = new List<CustomMapModeSelector>();

		private static string reusableString = "";
	}
}
