using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace GorillaTagScripts.Builder
{
	public class SharedBlocksScreenSearch : SharedBlocksScreen, IGorillaSliceableSimple
	{
		public override void OnSelectPressed()
		{
			if (SharedBlocksManager.IsMapIDValid(this.currentMapCode))
			{
				this.savedMapCode = this.currentMapCode;
				this.terminal.SelectMapIDAndOpenInfo(this.savedMapCode);
				return;
			}
			if (this.currentMapCode.Length < 8)
			{
				string text;
				if (!LocalisationManager.TryGetKeyForCurrentLocale("SHARE_BLOCKS_TERMINAL_SEARCH_ERROR_INVALID_LENGTH", out text, "INVALID MAP ID LENGTH"))
				{
					Debug.LogError("[LOCALIZATION::BUILDER_SCAN_KIOSK] Failed to get key for SHARE MY BLOCKS SEARCH TERMINAL localization [SHARE_BLOCKS_TERMINAL_SEARCH_ERROR_INVALID_LENGTH]");
				}
				this.terminal.SetStatusText(text);
				return;
			}
			string text2;
			if (!LocalisationManager.TryGetKeyForCurrentLocale("SHARE_BLOCKS_TERMINAL_SEARCH_ERROR_INVALID_ID", out text2, "INVALID MAP ID"))
			{
				Debug.LogError("[LOCALIZATION::BUILDER_SCAN_KIOSK] Failed to get key for SHARE MY BLOCKS SEARCH TERMINAL localization [SHARE_BLOCKS_TERMINAL_SEARCH_ERROR_INVALID_ID]");
			}
			this.terminal.SetStatusText(text2);
		}

		public override void OnDeletePressed()
		{
			if (this.currentMapCode.Length > 0)
			{
				this.currentMapCode = this.currentMapCode.Substring(0, this.currentMapCode.Length - 1);
				this.UpdateInput();
			}
		}

		public override void OnNumberPressed(int number)
		{
			if (this.currentMapCode.Length < 8)
			{
				this.currentMapCode += number.ToString();
				this.UpdateInput();
			}
		}

		public override void OnLetterPressed(string letter)
		{
			if (this.currentMapCode.Length < 8)
			{
				this.currentMapCode += letter;
				this.UpdateInput();
			}
		}

		public override void Show()
		{
			SharedBlocksManager.OnRecentMapIdsUpdated += this.DrawScreen;
			this.currentMapCode = string.Empty;
			this.DrawScreen();
			base.Show();
			this.RefreshPlayerCounter();
			BuilderTable table = this.terminal.GetTable();
			if (table != null)
			{
				table.OnMapLoaded.AddListener(new UnityAction<string>(this.OnMapLoaded));
				table.OnMapCleared.AddListener(new UnityAction(this.OnMapCleared));
				this.OnMapLoaded(table.GetCurrentMapID());
			}
		}

		public override void Hide()
		{
			BuilderTable table = this.terminal.GetTable();
			if (table != null)
			{
				table.OnMapLoaded.RemoveListener(new UnityAction<string>(this.OnMapLoaded));
				table.OnMapCleared.RemoveListener(new UnityAction(this.OnMapCleared));
			}
			this.statusText.text = "";
			this.statusText.gameObject.SetActive(false);
			SharedBlocksManager.OnRecentMapIdsUpdated -= this.DrawScreen;
			base.Hide();
		}

		private void OnMapLoaded(string mapID)
		{
			string text = "LOADED MAP : " + (SharedBlocksManager.IsMapIDValid(mapID) ? SharedBlocksTerminal.MapIDToDisplayedString(mapID) : "NONE");
			string text2;
			if (!LocalisationManager.TryGetKeyForCurrentLocale(SharedBlocksManager.IsMapIDValid(mapID) ? "SHARE_BLOCKS_TERMINAL_SEARCH_LOADED_LABEL" : "SHARE_BLOCKS_TERMINAL_SEARCH_LOADED_NONE", out text2, text))
			{
				Debug.LogError("[LOCALIZATION::BUILDER_SCAN_KIOSK] Failed to get key for SHARE MY BLOCKS SEARCH TERMINAL localization [SHARE_BLOCKS_TERMINAL_SEARCH_LOADED_LABEL]");
			}
			text2 = text2.Replace("{mapDisplayName}", SharedBlocksTerminal.MapIDToDisplayedString(mapID));
			this.loadedMap.text = text2;
		}

		private void OnMapCleared()
		{
			string text;
			if (!LocalisationManager.TryGetKeyForCurrentLocale("SHARE_BLOCKS_TERMINAL_SEARCH_LOADED_NONE", out text, "LOADED MAP : NONE"))
			{
				Debug.LogError("[LOCALIZATION::BUILDER_SCAN_KIOSK] Failed to get key for SHARE MY BLOCKS SEARCH TERMINAL localization [SHARE_BLOCKS_TERMINAL_SEARCH_LOADED_NONE]");
			}
			this.loadedMap.text = text;
		}

		private void UpdateInput()
		{
			string text = "MAP SEARCH : ";
			string text2;
			if (!LocalisationManager.TryGetKeyForCurrentLocale("SHARE_BLOCKS_TERMINAL_SEARCH_MAP_SEARCH", out text2, text))
			{
				Debug.LogError("[LOCALIZATION::BUILDER_SCAN_KIOSK] Failed to get key for SHARE MY BLOCKS SEARCH TERMINAL localization [SHARE_BLOCKS_TERMINAL_SEARCH_MAP_SEARCH]");
			}
			text2 += SharedBlocksTerminal.MapIDToDisplayedString(this.currentMapCode);
			this.inputText.text = text2;
		}

		public void SetMapCode(string mapCode)
		{
			if (mapCode == null)
			{
				this.currentMapCode = string.Empty;
			}
			else
			{
				this.currentMapCode = mapCode;
			}
			this.UpdateInput();
		}

		public void SetInputTextEnabled(bool enabled)
		{
			if (enabled)
			{
				this.inputText.color = Color.white;
				return;
			}
			this.inputText.color = Color.gray;
		}

		private void DrawScreen()
		{
			this.UpdateInput();
			string text;
			if (!LocalisationManager.TryGetKeyForCurrentLocale("SHARE_BLOCKS_TERMINAL_SEARCH_VOTES", out text, "RECENT VOTES"))
			{
				Debug.LogError("[LOCALIZATION::BUILDER_SCAN_KIOSK] Failed to get key for SHARE MY BLOCKS SEARCH TERMINAL localization [SHARE_BLOCKS_TERMINAL_SEARCH_VOTES]");
			}
			this.sb.Clear();
			this.sb.Append(text + "\n");
			foreach (string text2 in SharedBlocksManager.GetRecentUpVotes())
			{
				if (SharedBlocksManager.IsMapIDValid(text2))
				{
					this.sb.Append(SharedBlocksTerminal.MapIDToDisplayedString(text2));
					this.sb.Append("\n");
				}
			}
			this.recentList.text = this.sb.ToString();
			if (!LocalisationManager.TryGetKeyForCurrentLocale("SHARE_BLOCKS_TERMINAL_SEARCH_MAPS_LABEL", out text, "MY MAPS"))
			{
				Debug.LogError("[LOCALIZATION::BUILDER_SCAN_KIOSK] Failed to get key for SHARE MY BLOCKS SEARCH TERMINAL localization [SHARE_BLOCKS_TERMINAL_SEARCH_MAPS_LABEL]");
			}
			this.sb.Clear();
			this.sb.Append(text + "\n");
			foreach (string text3 in SharedBlocksManager.GetLocalMapIDs())
			{
				if (SharedBlocksManager.IsMapIDValid(text3))
				{
					this.sb.Append(SharedBlocksTerminal.MapIDToDisplayedString(text3));
					this.sb.Append("\n");
				}
			}
			this.myScanList.text = this.sb.ToString();
		}

		private void RefreshPlayerCounter()
		{
			this.terminal.RefreshLobbyCount();
			this.playerCountText.text = this.terminal.GetLobbyText();
			this.playersInLobbyWarning.gameObject.SetActive(!this.terminal.AreAllPlayersInLobby());
		}

		public void SliceUpdate()
		{
			this.RefreshPlayerCounter();
		}

		public void OnEnable()
		{
			if (!this.updating)
			{
				GorillaSlicerSimpleManager.RegisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.LateUpdate);
				this.updating = true;
			}
			this.RefreshPlayerCounter();
			RoomSystem.PlayersChangedEvent += new Action(this.PlayersChangedEvent);
		}

		private void PlayersChangedEvent()
		{
			this.RefreshPlayerCounter();
		}

		public void OnDisable()
		{
			if (this.updating)
			{
				GorillaSlicerSimpleManager.UnregisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.LateUpdate);
				this.updating = false;
			}
			RoomSystem.PlayersChangedEvent -= new Action(this.PlayersChangedEvent);
		}

		[SerializeField]
		private TMP_Text loadedMap;

		[SerializeField]
		private TMP_Text inputText;

		[SerializeField]
		private TMP_Text statusText;

		[SerializeField]
		private TMP_Text recentList;

		[SerializeField]
		private TMP_Text myScanList;

		[SerializeField]
		private TMP_Text playerCountText;

		[SerializeField]
		private TMP_Text playersInLobbyWarning;

		private string currentMapCode;

		private string savedMapCode;

		private StringBuilder sb = new StringBuilder();

		private bool updating;
	}
}
