using System;
using System.Collections.Generic;
using System.Text;
using GorillaNetworking;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization.SmartFormat.PersistentVariables;

namespace GorillaTagScripts.Builder
{
	public class SharedBlocksTerminal : MonoBehaviour
	{
		public SharedBlocksManager.SharedBlocksMap SelectedMap
		{
			get
			{
				return this.selectedMap;
			}
		}

		public bool IsTerminalLocked
		{
			get
			{
				return this.isTerminalLocked;
			}
		}

		private int playersInLobby
		{
			get
			{
				return this.lobbyTrigger.playerIDsCurrentlyTouching.Count;
			}
		}

		public bool IsDriver
		{
			get
			{
				return this.localState.driverID == NetworkSystem.Instance.LocalPlayer.ActorNumber;
			}
		}

		public BuilderTable GetTable()
		{
			return this.linkedTable;
		}

		public int GetDriverID
		{
			get
			{
				return this.localState.driverID;
			}
		}

		public static string MapIDToDisplayedString(string mapID)
		{
			if (mapID.IsNullOrEmpty())
			{
				return "____-____";
			}
			int num = 4;
			SharedBlocksTerminal.sb.Clear();
			if (mapID.Length > num)
			{
				SharedBlocksTerminal.sb.Append(mapID.Substring(0, num));
				SharedBlocksTerminal.sb.Append("-");
				SharedBlocksTerminal.sb.Append(mapID.Substring(num));
				int num2 = 9 - SharedBlocksTerminal.sb.Length;
				SharedBlocksTerminal.sb.Append('_', num2);
			}
			else
			{
				SharedBlocksTerminal.sb.Append(mapID.Substring(0));
				int num3 = num - SharedBlocksTerminal.sb.Length;
				SharedBlocksTerminal.sb.Append('_', num3);
				SharedBlocksTerminal.sb.Append("-____");
			}
			return SharedBlocksTerminal.sb.ToString();
		}

		public void Init(BuilderTable table)
		{
			if (this.hasInitialized)
			{
				return;
			}
			this.localState = new SharedBlocksTerminal.SharedBlocksTerminalState
			{
				state = SharedBlocksTerminal.TerminalState.NoStatus,
				driverID = -2
			};
			GameEvents.OnSharedBlocksKeyboardButtonPressedEvent.AddListener(new UnityAction<SharedBlocksKeyboardBindings>(this.PressButton));
			this.terminalControlButton.onPressButton.AddListener(new UnityAction(this.OnTerminalControlPressed));
			this.SetTerminalState(SharedBlocksTerminal.TerminalState.NoStatus);
			this.RefreshActiveScreen();
			this.linkedTable = table;
			table.linkedTerminal = this;
			this.linkedTable.OnMapLoaded.AddListener(new UnityAction<string>(this.OnSharedBlocksMapLoaded));
			this.linkedTable.OnMapLoadFailed.AddListener(new UnityAction<string>(this.OnSharedBlocksMapLoadFailed));
			this.linkedTable.OnMapCleared.AddListener(new UnityAction(this.OnSharedBlocksMapLoadStart));
			NetworkSystem.Instance.OnMultiplayerStarted += this.OnJoinedRoom;
			NetworkSystem.Instance.OnReturnedToSinglePlayer += this.OnReturnedToSinglePlayer;
			this.hasInitialized = true;
		}

		private void Start()
		{
			BuilderTable builderTable;
			if (!this.hasInitialized && BuilderTable.TryGetBuilderTableForZone(this.tableZone, out builderTable))
			{
				this.Init(builderTable);
				return;
			}
			Debug.LogWarning("Could not find builder table for zone " + this.tableZone.ToString());
		}

		private void LateUpdate()
		{
			if (this.localState.driverID == -2)
			{
				return;
			}
			if (GorillaComputer.instance == null)
			{
				return;
			}
			if (this.useNametags == GorillaComputer.instance.NametagsEnabled)
			{
				return;
			}
			this.useNametags = GorillaComputer.instance.NametagsEnabled;
			this.RefreshDriverNickname();
		}

		private void OnDestroy()
		{
			GameEvents.OnSharedBlocksKeyboardButtonPressedEvent.RemoveListener(new UnityAction<SharedBlocksKeyboardBindings>(this.PressButton));
			if (this.terminalControlButton != null)
			{
				this.terminalControlButton.onPressButton.RemoveListener(new UnityAction(this.OnTerminalControlPressed));
			}
			if (NetworkSystem.Instance != null)
			{
				NetworkSystem.Instance.OnMultiplayerStarted -= this.OnJoinedRoom;
				NetworkSystem.Instance.OnReturnedToSinglePlayer -= this.OnReturnedToSinglePlayer;
			}
			if (this.linkedTable != null)
			{
				this.linkedTable.OnMapLoaded.RemoveListener(new UnityAction<string>(this.OnSharedBlocksMapLoaded));
				this.linkedTable.OnMapLoadFailed.RemoveListener(new UnityAction<string>(this.OnSharedBlocksMapLoadFailed));
				this.linkedTable.OnMapCleared.RemoveListener(new UnityAction(this.OnSharedBlocksMapLoadStart));
			}
		}

		private void RefreshActiveScreen()
		{
			if (this.localState.driverID == -2)
			{
				if (this.currentScreen != this.noDriverScreen)
				{
					if (this.currentScreen != null)
					{
						this.currentScreen.Hide();
					}
					this.currentScreen = this.noDriverScreen;
					this.currentScreen.Show();
				}
				this.statusMessageText.gameObject.SetActive(false);
				return;
			}
			if (this.currentScreen != this.searchScreen)
			{
				if (this.currentScreen != null)
				{
					this.currentScreen.Hide();
				}
				this.currentScreen = this.searchScreen;
				this.currentScreen.Show();
			}
		}

		private void SetTerminalState(SharedBlocksTerminal.TerminalState state)
		{
			this.localState.state = state;
			string text = "";
			if (this.localState.driverID == -2)
			{
				this.statusMessageText.gameObject.SetActive(false);
				return;
			}
			switch (state)
			{
			case SharedBlocksTerminal.TerminalState.NoStatus:
				this.statusMessageText.gameObject.SetActive(false);
				return;
			case SharedBlocksTerminal.TerminalState.Searching:
			{
				string text2 = "SEARCHING...";
				if (!LocalisationManager.TryGetKeyForCurrentLocale("SHARE_BLOCKS_TERMINAL_STATUS_SEARCH", out text, text2))
				{
					Debug.LogError("[LOCALIZATION::BUILDER_SCAN_KIOSK] Failed to get key for MONKE BLOCKS SCAN KIOSK localization [SHARE_BLOCKS_TERMINAL_STATUS_SEARCH]");
				}
				this.SetStatusText(text);
				return;
			}
			case SharedBlocksTerminal.TerminalState.NotFound:
			{
				string text2 = "MAP NOT FOUND";
				if (!LocalisationManager.TryGetKeyForCurrentLocale("SHARE_BLOCKS_TERMINAL_STATUS_MAP_NOT_FOUND", out text, text2))
				{
					Debug.LogError("[LOCALIZATION::BUILDER_SCAN_KIOSK] Failed to get key for MONKE BLOCKS SCAN KIOSK localization [SHARE_BLOCKS_TERMINAL_STATUS_MAP_NOT_FOUND]");
				}
				this.SetStatusText(text);
				return;
			}
			case SharedBlocksTerminal.TerminalState.Found:
			{
				string text2 = "MAP FOUND. PRESS 'ENTER' TO LOAD";
				if (!LocalisationManager.TryGetKeyForCurrentLocale("SHARE_BLOCKS_TERMINAL_STATUS_MAP_FOUND", out text, text2))
				{
					Debug.LogError("[LOCALIZATION::BUILDER_SCAN_KIOSK] Failed to get key for MONKE BLOCKS SCAN KIOSK localization [SHARE_BLOCKS_TERMINAL_STATUS_MAP_FOUND]");
				}
				this.SetStatusText(text);
				return;
			}
			case SharedBlocksTerminal.TerminalState.Loading:
			{
				string text2 = "LOADING...";
				if (!LocalisationManager.TryGetKeyForCurrentLocale("SHARE_BLOCKS_TERMINAL_STATUS_LOADING", out text, text2))
				{
					Debug.LogError("[LOCALIZATION::BUILDER_SCAN_KIOSK] Failed to get key for MONKE BLOCKS SCAN KIOSK localization [SHARE_BLOCKS_TERMINAL_STATUS_LOADING]");
				}
				this.SetStatusText(text);
				return;
			}
			case SharedBlocksTerminal.TerminalState.LoadSuccess:
			{
				string text2 = "LOAD SUCCESS";
				if (!LocalisationManager.TryGetKeyForCurrentLocale("SHARE_BLOCKS_TERMINAL_STATUS_LOAD_SUCCESS", out text, text2))
				{
					Debug.LogError("[LOCALIZATION::BUILDER_SCAN_KIOSK] Failed to get key for MONKE BLOCKS SCAN KIOSK localization [SHARE_BLOCKS_TERMINAL_STATUS_LOAD_SUCCESS]");
				}
				this.SetStatusText(text);
				return;
			}
			case SharedBlocksTerminal.TerminalState.LoadFail:
			{
				string text2 = "LOAD FAILED";
				if (!LocalisationManager.TryGetKeyForCurrentLocale("SHARE_BLOCKS_TERMINAL_STATUS_LOAD_FAILED", out text, text2))
				{
					Debug.LogError("[LOCALIZATION::BUILDER_SCAN_KIOSK] Failed to get key for MONKE BLOCKS SCAN KIOSK localization [SHARE_BLOCKS_TERMINAL_STATUS_LOAD_FAILED]");
				}
				this.SetStatusText(text);
				return;
			}
			default:
				return;
			}
		}

		public void SelectMapIDAndOpenInfo(string mapID)
		{
			if (this.awaitingWebRequest)
			{
				return;
			}
			this.selectedMap = null;
			this.awaitingWebRequest = true;
			this.requestedMapID = mapID;
			this.SetTerminalState(SharedBlocksTerminal.TerminalState.Searching);
			SharedBlocksManager.instance.RequestMapDataFromID(mapID, new SharedBlocksManager.BlocksMapRequestCallback(this.OnPlayerMapRequestComplete));
		}

		private void OnPlayerMapRequestComplete(SharedBlocksManager.SharedBlocksMap response)
		{
			if (this.awaitingWebRequest)
			{
				this.awaitingWebRequest = false;
				this.requestedMapID = null;
				if (this.IsDriver)
				{
					if (response == null || response.MapID == null)
					{
						this.SetTerminalState(SharedBlocksTerminal.TerminalState.NotFound);
						return;
					}
					this.selectedMap = response;
					this.SetTerminalState(SharedBlocksTerminal.TerminalState.Found);
				}
			}
		}

		private bool CanChangeMapState(bool load, out string disallowedReason)
		{
			disallowedReason = "";
			if (!NetworkSystem.Instance.InRoom)
			{
				disallowedReason = "MUST BE IN A ROOM BEFORE  " + (load ? "" : "UN") + "LOADING A MAP.";
				string text = (load ? "SHARE_BLOCKS_TERMINAL_STATUS_DISALLOWED_ROOM_LOAD" : "SHARE_BLOCKS_TERMINAL_STATUS_DISALLOWED_ROOM_UNLOAD");
				string text2;
				if (!LocalisationManager.TryGetKeyForCurrentLocale(text, out text2, disallowedReason))
				{
					Debug.LogError("[LOCALIZATION::BUILDER_SCAN_KIOSK] Failed to get key for MONKE BLOCKS SCAN KIOSK localization [" + text + "]");
				}
				disallowedReason = text2;
				return false;
			}
			this.RefreshLobbyCount();
			if (!this.AreAllPlayersInLobby())
			{
				disallowedReason = "ALL PLAYERS IN THE ROOM MUST BE INSIDE THE LOBBY BEFORE " + (load ? "" : "UN") + "LOADING A MAP.";
				string text3 = (load ? "SHARE_BLOCKS_TERMINAL_STATUS_DISALLOWED_LOBBY_LOAD" : "SHARE_BLOCKS_TERMINAL_STATUS_DISALLOWED_LOBBY_UNLOAD");
				string text4;
				if (!LocalisationManager.TryGetKeyForCurrentLocale(text3, out text4, disallowedReason))
				{
					Debug.LogError("[LOCALIZATION::BUILDER_SCAN_KIOSK] Failed to get key for MONKE BLOCKS SCAN KIOSK localization [" + text3 + "]");
				}
				disallowedReason = text4;
				return false;
			}
			return true;
		}

		public void SetStatusText(string text)
		{
			this.statusMessageText.text = text;
			this.statusMessageText.gameObject.SetActive(true);
		}

		private bool IsLocalPlayerInLobby()
		{
			return base.isActiveAndEnabled && this.lobbyTrigger.playerIDsCurrentlyTouching.Contains(VRRig.LocalRig.creator.UserId);
		}

		public bool AreAllPlayersInLobby()
		{
			return base.isActiveAndEnabled && this.playersInLobby == this.playersInRoom;
		}

		public string GetLobbyText()
		{
			string text = "PLAYERS IN ROOM {0}\nPLAYERS IN LOBBY {1}";
			string text2;
			if (!LocalisationManager.TryGetKeyForCurrentLocale("SHARE_BLOCKS_TERMINAL_SEARCH_LOBBY_TEXT_FORMAT", out text2, text))
			{
				Debug.LogError("[LOCALIZATION::BUILDER_SCAN_KIOSK] Failed to get key for MONKE BLOCKS SCAN KIOSK localization [SHARE_BLOCKS_TERMINAL_SEARCH_LOBBY_TEXT_FORMAT]");
			}
			return string.Format(text2, this.playersInRoom, this.playersInLobby);
		}

		public void RefreshLobbyCount()
		{
			if (NetworkSystem.Instance != null && NetworkSystem.Instance.InRoom)
			{
				this.playersInRoom = NetworkSystem.Instance.RoomPlayerCount;
				return;
			}
			this.playersInRoom = 0;
		}

		public void PressButton(SharedBlocksKeyboardBindings buttonPressed)
		{
			if (!this.IsDriver)
			{
				string text;
				if (!LocalisationManager.TryGetKeyForCurrentLocale("SHARE_BLOCKS_TERMINAL_STATUS_NOT_CONTROLLER", out text, "NOT TERMINAL CONTROLLER"))
				{
					Debug.LogError("[LOCALIZATION::BUILDER_SCAN_KIOSK] Failed to get key for SHARE MY BLOCKS TERMINAL localization [SHARE_BLOCKS_TERMINAL_STATUS_NOT_CONTROLLER]");
				}
				this.SetStatusText(text);
				return;
			}
			if (this.localState.state == SharedBlocksTerminal.TerminalState.Searching || this.localState.state == SharedBlocksTerminal.TerminalState.Loading)
			{
				return;
			}
			if (buttonPressed == SharedBlocksKeyboardBindings.up)
			{
				this.OnUpButtonPressed();
				return;
			}
			if (buttonPressed == SharedBlocksKeyboardBindings.down)
			{
				this.OnDownButtonPressed();
				return;
			}
			if (buttonPressed == SharedBlocksKeyboardBindings.delete)
			{
				this.OnDeleteButtonPressed();
				return;
			}
			if (buttonPressed == SharedBlocksKeyboardBindings.enter)
			{
				this.OnSelectButtonPressed();
				return;
			}
			if (buttonPressed >= SharedBlocksKeyboardBindings.zero && buttonPressed <= SharedBlocksKeyboardBindings.nine)
			{
				this.OnNumberPressed((int)buttonPressed);
				return;
			}
			if (buttonPressed >= SharedBlocksKeyboardBindings.A && buttonPressed <= SharedBlocksKeyboardBindings.Z)
			{
				this.OnLetterPressed(buttonPressed.ToString());
			}
		}

		private void OnUpButtonPressed()
		{
			if (this.currentScreen != null)
			{
				this.currentScreen.OnUpPressed();
			}
		}

		private void OnDownButtonPressed()
		{
			if (this.currentScreen != null)
			{
				this.currentScreen.OnDownPressed();
			}
		}

		private void OnSelectButtonPressed()
		{
			if (this.localState.state == SharedBlocksTerminal.TerminalState.Found)
			{
				this.OnLoadMapPressed();
				return;
			}
			if (this.currentScreen != null)
			{
				this.currentScreen.OnSelectPressed();
			}
		}

		private void OnDeleteButtonPressed()
		{
			if (this.localState.state != SharedBlocksTerminal.TerminalState.Loading && this.localState.state != SharedBlocksTerminal.TerminalState.Searching)
			{
				this.SetTerminalState(SharedBlocksTerminal.TerminalState.NoStatus);
			}
			if (this.currentScreen != null)
			{
				this.currentScreen.OnDeletePressed();
			}
		}

		private void OnBackButtonPressed()
		{
		}

		private void OnNumberPressed(int number)
		{
			if (this.currentScreen != null)
			{
				this.currentScreen.OnNumberPressed(number);
			}
		}

		private void OnLetterPressed(string letter)
		{
			if (this.currentScreen != null)
			{
				this.currentScreen.OnLetterPressed(letter);
			}
		}

		private void OnTerminalControlPressed()
		{
			if (this.isTerminalLocked)
			{
				if (this.IsDriver)
				{
					if (NetworkSystem.Instance.InRoom)
					{
						this.linkedTable.builderNetworking.RequestBlocksTerminalControl(false);
						return;
					}
					this.SetTerminalDriver(-2);
					return;
				}
			}
			else
			{
				if (NetworkSystem.Instance.InRoom)
				{
					this.linkedTable.builderNetworking.RequestBlocksTerminalControl(true);
					return;
				}
				this.SetTerminalDriver(NetworkSystem.Instance.LocalPlayer.ActorNumber);
			}
		}

		public void OnLoadMapPressed()
		{
			if (!this.IsDriver)
			{
				string text;
				if (!LocalisationManager.TryGetKeyForCurrentLocale("SHARE_BLOCKS_TERMINAL_STATUS_NOT_CONTROLLER", out text, "NOT TERMINAL CONTROLLER"))
				{
					Debug.LogError("[LOCALIZATION::BUILDER_SCAN_KIOSK] Failed to get key for SHARE MY BLOCKS TERMINAL localization [SHARE_BLOCKS_TERMINAL_STATUS_NOT_CONTROLLER]");
				}
				this.SetStatusText(text);
				return;
			}
			if (this.currentScreen == null || this.selectedMap == null)
			{
				string text2;
				if (!LocalisationManager.TryGetKeyForCurrentLocale("SHARE_BLOCKS_TERMINAL_STATUS_NO_SELECTION", out text2, "NO MAP SELECTED"))
				{
					Debug.LogError("[LOCALIZATION::BUILDER_SCAN_KIOSK] Failed to get key for SHARE MY BLOCKS TERMINAL localization [SHARE_BLOCKS_TERMINAL_STATUS_NO_SELECTION]");
				}
				this.SetStatusText(text2);
				return;
			}
			if (this.awaitingWebRequest || this.isLoadingMap)
			{
				string text3;
				if (!LocalisationManager.TryGetKeyForCurrentLocale("SHARE_BLOCKS_TERMINAL_STATUS_IN_PROGRESS", out text3, "BLOCKS LOAD ALREADY IN PROGRESS"))
				{
					Debug.LogError("[LOCALIZATION::BUILDER_SCAN_KIOSK] Failed to get key for SHARE MY BLOCKS TERMINAL localization [SHARE_BLOCKS_TERMINAL_STATUS_IN_PROGRESS]");
				}
				this.SetStatusText("BLOCKS LOAD ALREADY IN PROGRESS");
				return;
			}
			string text4;
			if (!this.CanChangeMapState(true, out text4))
			{
				this.SetStatusText(text4);
				return;
			}
			if (this.linkedTable != null)
			{
				if (Time.time > this.lastLoadTime + this.loadMapCooldown)
				{
					string text5;
					if (!LocalisationManager.TryGetKeyForCurrentLocale("SHARE_BLOCKS_TERMINAL_STATUS_LOADING", out text5, "LOADING BLOCKS ..."))
					{
						Debug.LogError("[LOCALIZATION::BUILDER_SCAN_KIOSK] Failed to get key for SHARE MY BLOCKS TERMINAL localization [SHARE_BLOCKS_TERMINAL_STATUS_LOADING]");
					}
					this.SetStatusText("LOADING BLOCKS ...");
					this.isLoadingMap = true;
					this.lastLoadTime = Time.time;
					this.linkedTable.LoadSharedMap(this.selectedMap);
					return;
				}
				int num = Mathf.RoundToInt(this.lastLoadTime + this.loadMapCooldown - Time.time);
				string text6 = string.Format("PLEASE WAIT {0} SECONDS BEFORE LOADING ANOTHER MAP", num);
				string text7;
				if (!LocalisationManager.TryGetKeyForCurrentLocale("SHARE_BLOCKS_TERMINAL_STATUS_WAIT", out text7, text6))
				{
					Debug.LogError("[LOCALIZATION::BUILDER_SCAN_KIOSK] Failed to get key for SHARE MY BLOCKS TERMINAL localization [SHARE_BLOCKS_TERMINAL_STATUS_LOADING]");
				}
				text7 = text7.Replace("{time}", num.ToString());
				this.SetStatusText(text7);
			}
		}

		public bool IsPlayerDriver(Player player)
		{
			return player.ActorNumber == this.localState.driverID;
		}

		public bool ValidateTerminalControlRequest(bool locked, int playerNumber)
		{
			if (locked && playerNumber == -2)
			{
				return false;
			}
			if (this.localState.driverID == -2)
			{
				return locked;
			}
			return this.localState.driverID == playerNumber;
		}

		private void OnDriverNameChanged()
		{
			this.RefreshDriverNickname();
		}

		public void SetTerminalDriver(int playerNum)
		{
			if (playerNum != -2)
			{
				if (this.localState.driverID != -2 && this.localState.driverID != playerNum)
				{
					GTDev.LogWarning<string>(string.Format("Shared BlocksTerminal SetTerminalDriver cannot set {0} as driver while {1} is driver", playerNum, this.localState.driverID), null);
					return;
				}
				this.localState.driverID = playerNum;
				NetPlayer netPlayerByID = NetworkSystem.Instance.GetNetPlayerByID(playerNum);
				RigContainer rigContainer;
				if (netPlayerByID != null && VRRigCache.Instance.TryGetVrrig(netPlayerByID, out rigContainer))
				{
					this.driverRig = rigContainer.Rig;
					this.driverRig.OnPlayerNameVisibleChanged += this.OnDriverNameChanged;
				}
				this.isTerminalLocked = true;
				this.UpdateTerminalButton();
				this.RefreshActiveScreen();
				this.searchScreen.SetInputTextEnabled(this.IsDriver);
				if (this.IsDriver && this.awaitingWebRequest)
				{
					this.SetTerminalState(SharedBlocksTerminal.TerminalState.Searching);
					this.searchScreen.SetMapCode(this.requestedMapID);
				}
				else if (this.isLoadingMap)
				{
					this.SetTerminalState(SharedBlocksTerminal.TerminalState.Loading);
					this.searchScreen.SetMapCode(this.linkedTable.GetPendingMap());
				}
				else
				{
					this.SetTerminalState(SharedBlocksTerminal.TerminalState.NoStatus);
				}
			}
			else
			{
				if (this.driverRig != null)
				{
					this.driverRig.OnPlayerNameVisibleChanged -= this.OnDriverNameChanged;
					this.driverRig = null;
				}
				this.localState.driverID = -2;
				this.isTerminalLocked = false;
				this.UpdateTerminalButton();
				this.SetTerminalState(SharedBlocksTerminal.TerminalState.NoStatus);
				this.RefreshActiveScreen();
			}
			this.RefreshDriverNickname();
		}

		private void RefreshDriverNickname()
		{
			StringVariable stringVariable = this._currentDriverLoc.StringReference["playerName"] as StringVariable;
			if (this.localState.driverID == -2)
			{
				this.currentDriverLabel.gameObject.SetActive(false);
				stringVariable.Value = "";
				this.currentDriverText.text = "";
				this.currentDriverText.gameObject.SetActive(false);
				return;
			}
			bool flag = KIDManager.HasPermissionToUseFeature(EKIDFeatures.Custom_Nametags);
			if (NetworkSystem.Instance.InRoom)
			{
				NetPlayer player = NetworkSystem.Instance.GetPlayer(this.localState.driverID);
				if (player != null && this.useNametags && flag)
				{
					RigContainer rigContainer;
					if (player.IsLocal)
					{
						stringVariable.Value = player.NickName;
						this.currentDriverText.text = player.NickName;
					}
					else if (VRRigCache.Instance.TryGetVrrig(player, out rigContainer))
					{
						stringVariable.Value = rigContainer.Rig.playerNameVisible;
						this.currentDriverText.text = rigContainer.Rig.playerNameVisible;
					}
					else
					{
						stringVariable.Value = player.DefaultName;
						this.currentDriverText.text = player.DefaultName;
					}
				}
				else
				{
					stringVariable.Value = "";
					this.currentDriverText.text = "";
				}
			}
			else
			{
				stringVariable.Value = ((this.useNametags && flag) ? NetworkSystem.Instance.LocalPlayer.NickName : NetworkSystem.Instance.LocalPlayer.DefaultName);
				this.currentDriverText.text = ((this.useNametags && flag) ? NetworkSystem.Instance.LocalPlayer.NickName : NetworkSystem.Instance.LocalPlayer.DefaultName);
			}
			this.currentDriverLabel.gameObject.SetActive(true);
		}

		public bool ValidateLoadMapRequest(string mapID, int playerNum)
		{
			return playerNum == this.localState.driverID && this.AreAllPlayersInLobby() && SharedBlocksManager.IsMapIDValid(mapID);
		}

		private void OnJoinedRoom()
		{
			GTDev.Log<string>("[SharedBlocksTerminal::OnJoinedRoom] Joined a multiplayer room, resetting terminal control", null);
			this.cachedLocalPlayerID = NetworkSystem.Instance.LocalPlayer.ActorNumber;
			this.ResetTerminalControl();
		}

		private void OnReturnedToSinglePlayer()
		{
			if (this.localState.driverID != this.cachedLocalPlayerID)
			{
				this.ResetTerminalControl();
			}
			else
			{
				this.localState.driverID = NetworkSystem.Instance.LocalPlayer.ActorNumber;
			}
			this.cachedLocalPlayerID = -1;
		}

		public void ResetTerminalControl()
		{
			this.localState.driverID = -2;
			this.isTerminalLocked = false;
			this.selectedMap = null;
			this.SetTerminalState(SharedBlocksTerminal.TerminalState.NoStatus);
			this.RefreshActiveScreen();
			this.UpdateTerminalButton();
		}

		private void UpdateTerminalButton()
		{
			this.terminalControlButton.isOn = this.isTerminalLocked;
			this.terminalControlButton.UpdateColor();
		}

		private void OnSharedBlocksMapLoaded(string mapID)
		{
			if (!this.IsDriver)
			{
				this.searchScreen.SetMapCode(mapID);
			}
			if (SharedBlocksManager.IsMapIDValid(mapID))
			{
				this.SetTerminalState(SharedBlocksTerminal.TerminalState.LoadSuccess);
			}
			else if (this.localState.state != SharedBlocksTerminal.TerminalState.LoadFail)
			{
				this.SetTerminalState(SharedBlocksTerminal.TerminalState.LoadFail);
			}
			this.isLoadingMap = false;
		}

		private void OnSharedBlocksMapLoadFailed(string message)
		{
			this.SetTerminalState(SharedBlocksTerminal.TerminalState.LoadFail);
			this.SetStatusText(message);
			this.isLoadingMap = false;
		}

		private void OnSharedBlocksMapLoadStart()
		{
			if (this.linkedTable == null)
			{
				return;
			}
			if (!this.IsDriver)
			{
				this.searchScreen.SetMapCode(this.linkedTable.GetPendingMap());
				this.SetTerminalState(SharedBlocksTerminal.TerminalState.Loading);
				this.isLoadingMap = true;
				this.lastLoadTime = Time.time;
			}
		}

		public const string SHARE_BLOCKS_TERMINAL_PROMPT_KEY = "SHARE_BLOCKS_TERMINAL_PROMPT";

		public const string SHARE_BLOCKS_TERMINAL_CONTROL_BUTTON_KEY = "SHARE_BLOCKS_TERMINAL_CONTROL_BUTTON";

		public const string SHARE_BLOCKS_TERMINAL_CONTROL_BUTTON_AVAILABLE_KEY = "SHARE_BLOCKS_TERMINAL_CONTROL_BUTTON_AVAILABLE";

		public const string SHARE_BLOCKS_TERMINAL_CONTROL_BUTTON_LOCKED_KEY = "SHARE_BLOCKS_TERMINAL_CONTROL_BUTTON_LOCKED";

		public const string SHARE_BLOCKS_TERMINAL_STATUS_SEARCH_KEY = "SHARE_BLOCKS_TERMINAL_STATUS_SEARCH";

		public const string SHARE_BLOCKS_TERMINAL_STATUS_MAP_FOUND_KEY = "SHARE_BLOCKS_TERMINAL_STATUS_MAP_FOUND";

		public const string SHARE_BLOCKS_TERMINAL_STATUS_MAP_NOT_FOUND_KEY = "SHARE_BLOCKS_TERMINAL_STATUS_MAP_NOT_FOUND";

		public const string SHARE_BLOCKS_TERMINAL_STATUS_LOADING_KEY = "SHARE_BLOCKS_TERMINAL_STATUS_LOADING";

		public const string SHARE_BLOCKS_TERMINAL_STATUS_LOAD_SUCCESS_KEY = "SHARE_BLOCKS_TERMINAL_STATUS_LOAD_SUCCESS";

		public const string SHARE_BLOCKS_TERMINAL_STATUS_LOAD_FAILED_KEY = "SHARE_BLOCKS_TERMINAL_STATUS_LOAD_FAILED";

		public const string SHARE_BLOCKS_TERMINAL_STATUS_NOT_CONTROLLER_KEY = "SHARE_BLOCKS_TERMINAL_STATUS_NOT_CONTROLLER";

		public const string SHARE_BLOCKS_TERMINAL_STATUS_NO_SELECTION_KEY = "SHARE_BLOCKS_TERMINAL_STATUS_NO_SELECTION";

		public const string SHARE_BLOCKS_TERMINAL_STATUS_IN_PROGRESS_KEY = "SHARE_BLOCKS_TERMINAL_STATUS_IN_PROGRESS";

		public const string SHARE_BLOCKS_TERMINAL_STATUS_WAIT_KEY = "SHARE_BLOCKS_TERMINAL_STATUS_WAIT";

		public const string SHARE_BLOCKS_TERMINAL_STATUS_DISALLOWED_LOBBY_LOAD_KEY = "SHARE_BLOCKS_TERMINAL_STATUS_DISALLOWED_LOBBY_LOAD";

		public const string SHARE_BLOCKS_TERMINAL_STATUS_DISALLOWED_LOBBY_UNLOAD_KEY = "SHARE_BLOCKS_TERMINAL_STATUS_DISALLOWED_LOBBY_UNLOAD";

		public const string SHARE_BLOCKS_TERMINAL_STATUS_DISALLOWED_ROOM_LOAD_KEY = "SHARE_BLOCKS_TERMINAL_STATUS_DISALLOWED_ROOM_LOAD";

		public const string SHARE_BLOCKS_TERMINAL_STATUS_DISALLOWED_ROOM_UNLOAD_KEY = "SHARE_BLOCKS_TERMINAL_STATUS_DISALLOWED_ROOM_UNLOAD";

		public const string SHARE_BLOCKS_TERMINAL_SEARCH_LOADED_LABEL_KEY = "SHARE_BLOCKS_TERMINAL_SEARCH_LOADED_LABEL";

		public const string SHARE_BLOCKS_TERMINAL_SEARCH_LOADED_NONE_KEY = "SHARE_BLOCKS_TERMINAL_SEARCH_LOADED_NONE";

		public const string SHARE_BLOCKS_TERMINAL_SEARCH_MAP_SEARCH_KEY = "SHARE_BLOCKS_TERMINAL_SEARCH_MAP_SEARCH";

		public const string SHARE_BLOCKS_TERMINAL_SEARCH_VOTES_KEY = "SHARE_BLOCKS_TERMINAL_SEARCH_VOTES";

		public const string SHARE_BLOCKS_TERMINAL_SEARCH_MAPS_LABEL_KEY = "SHARE_BLOCKS_TERMINAL_SEARCH_MAPS_LABEL";

		public const string SHARE_BLOCKS_TERMINAL_SEARCH_LOBBY_TEXT_KEY = "SHARE_BLOCKS_TERMINAL_SEARCH_LOBBY_TEXT";

		public const string SHARE_BLOCKS_TERMINAL_ERROR_TITLE_KEY = "SHARE_BLOCKS_TERMINAL_ERROR_TITLE";

		public const string SHARE_BLOCKS_TERMINAL_ERROR_INSTRUCTIONS_KEY = "SHARE_BLOCKS_TERMINAL_ERROR_INSTRUCTIONS";

		public const string SHARE_BLOCKS_TERMINAL_ERROR_BACK_KEY = "SHARE_BLOCKS_TERMINAL_ERROR_BACK";

		public const string SHARE_BLOCKS_TERMINAL_INFO_TITLE_KEY = "SHARE_BLOCKS_TERMINAL_INFO_TITLE";

		public const string SHARE_BLOCKS_TERMINAL_INFO_DATA_KEY = "SHARE_BLOCKS_TERMINAL_INFO_DATA";

		public const string SHARE_BLOCKS_TERMINAL_INFO_ENTER_KEY = "SHARE_BLOCKS_TERMINAL_INFO_ENTER";

		public const string SHARE_BLOCKS_TERMINAL_OTHER_DRIVER_KEY = "SHARE_BLOCKS_TERMINAL_OTHER_DRIVER";

		public const string SHARE_BLOCKS_TERMINAL_CONTROLLER_LABEL_KEY = "SHARE_BLOCKS_TERMINAL_CONTROLLER_LABEL";

		public const string SHARE_BLOCKS_TERMINAL_SEARCH_LOBBY_TEXT_FORMAT_KEY = "SHARE_BLOCKS_TERMINAL_SEARCH_LOBBY_TEXT_FORMAT";

		public const string SHARE_BLOCKS_TERMINAL_SEARCH_ERROR_INVALID_LENGTH_KEY = "SHARE_BLOCKS_TERMINAL_SEARCH_ERROR_INVALID_LENGTH";

		public const string SHARE_BLOCKS_TERMINAL_SEARCH_ERROR_INVALID_ID_KEY = "SHARE_BLOCKS_TERMINAL_SEARCH_ERROR_INVALID_ID";

		[SerializeField]
		private GTZone tableZone = GTZone.monkeBlocksShared;

		[SerializeField]
		private TMP_Text currentMapSelectionText;

		[SerializeField]
		private TMP_Text statusMessageText;

		[SerializeField]
		private TMP_Text currentDriverText;

		[SerializeField]
		private TMP_Text currentDriverLabel;

		[SerializeField]
		private LocalizedText _currentDriverLoc;

		[SerializeField]
		private SharedBlocksScreen noDriverScreen;

		[SerializeField]
		private SharedBlocksScreenSearch searchScreen;

		[SerializeField]
		private GorillaPressableButton terminalControlButton;

		[SerializeField]
		private float loadMapCooldown = 30f;

		[SerializeField]
		private GorillaFriendCollider lobbyTrigger;

		private SharedBlocksManager.SharedBlocksMap selectedMap;

		private SharedBlocksScreen currentScreen;

		private BuilderTable linkedTable;

		public const int NO_DRIVER_ID = -2;

		private bool awaitingWebRequest;

		private string requestedMapID;

		public const string POINTER = "> ";

		public Action<bool> OnMapLoadComplete;

		private bool isTerminalLocked;

		private SharedBlocksTerminal.SharedBlocksTerminalState localState;

		private int cachedLocalPlayerID = -1;

		private bool isLoadingMap;

		private float lastLoadTime;

		private bool useNametags;

		private bool hasInitialized;

		private static StringBuilder sb = new StringBuilder();

		private VRRig driverRig;

		private static List<VRRig> tempRigs = new List<VRRig>(16);

		private int playersInRoom;

		public enum ScreenType
		{
			NO_DRIVER,
			SEARCH,
			LOADING,
			ERROR,
			SCAN_INFO,
			OTHER_DRIVER
		}

		public enum TerminalState
		{
			NoStatus,
			Searching,
			NotFound,
			Found,
			Loading,
			LoadSuccess,
			LoadFail
		}

		public class SharedBlocksTerminalState
		{
			public SharedBlocksTerminal.ScreenType currentScreen;

			public SharedBlocksTerminal.TerminalState state;

			public int driverID;
		}
	}
}
