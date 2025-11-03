using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization.SmartFormat.PersistentVariables;

namespace GorillaTagScripts.Builder
{
	public class SharedBlocksVotingStation : MonoBehaviour
	{
		private void Start()
		{
			this.SetupLocalization();
			BuilderTable builderTable;
			if (BuilderTable.TryGetBuilderTableForZone(this.tableZone, out builderTable))
			{
				this.table = builderTable;
				this.table.OnMapLoaded.AddListener(new UnityAction<string>(this.OnLoadedMapChanged));
				this.table.OnMapCleared.AddListener(new UnityAction(this.OnMapCleared));
				this.OnLoadedMapChanged(this.table.GetCurrentMapID());
			}
			else
			{
				GTDev.LogWarning<string>("No Builder Table found for Voting Station", null);
			}
			base.GetComponentsInChildren<MeshRenderer>(false, this.meshes);
			this.upVoteButton.onPressButton.AddListener(new UnityAction(this.OnUpVotePressed));
			this.downVoteButton.onPressButton.AddListener(new UnityAction(this.OnDownVotePressed));
			ZoneManagement instance = ZoneManagement.instance;
			instance.onZoneChanged = (Action)Delegate.Combine(instance.onZoneChanged, new Action(this.OnZoneChanged));
			this.OnZoneChanged();
		}

		private void OnDestroy()
		{
			this.upVoteButton.onPressButton.RemoveListener(new UnityAction(this.OnUpVotePressed));
			this.downVoteButton.onPressButton.RemoveListener(new UnityAction(this.OnDownVotePressed));
			if (this.table != null)
			{
				this.table.OnMapLoaded.RemoveListener(new UnityAction<string>(this.OnLoadedMapChanged));
				this.table.OnMapCleared.RemoveListener(new UnityAction(this.OnMapCleared));
			}
			if (ZoneManagement.instance != null)
			{
				ZoneManagement instance = ZoneManagement.instance;
				instance.onZoneChanged = (Action)Delegate.Remove(instance.onZoneChanged, new Action(this.OnZoneChanged));
			}
		}

		private void SetupLocalization()
		{
			if (this._statusLocText == null)
			{
				Debug.LogError("[LOCALIZATION::SHARED_BLOCKS_VOTING_STATION] Trying to set up Localization, but [_statusLocText] is NULL");
				return;
			}
			if (this._screenLocText == null)
			{
				Debug.LogError("[LOCALIZATION::SHARED_BLOCKS_VOTING_STATION] Trying to set up Localization, but [_screenLocText] is NULL");
				return;
			}
			string text = "voting-status-index";
			string text2 = "map-name-index";
			string text3 = "map-name";
			this._statusIndexVar = this._statusLocText.StringReference[text] as IntVariable;
			this._mapDisplayIndexVar = this._screenLocText.StringReference[text2] as IntVariable;
			this._mapNameVar = this._screenLocText.StringReference[text3] as StringVariable;
			if (this._statusIndexVar == null)
			{
				Debug.LogError("[LOCALIZATION::SHARED_BLOCKS_VOTING_STATION] Failed to find [IntVariable] with var-name [" + text + "]");
			}
			if (this._mapDisplayIndexVar == null)
			{
				Debug.LogError("[LOCALIZATION::SHARED_BLOCKS_VOTING_STATION] Failed to find [IntVariable] with var-name [" + text2 + "]");
			}
			if (this._mapNameVar == null)
			{
				Debug.LogError("[LOCALIZATION::SHARED_BLOCKS_VOTING_STATION] Failed to find [StringVariable] with var-name [" + text3 + "]");
			}
		}

		private void OnZoneChanged()
		{
			bool flag = ZoneManagement.instance.IsZoneActive(this.tableZone);
			foreach (MeshRenderer meshRenderer in this.meshes)
			{
				meshRenderer.enabled = flag;
			}
		}

		private void OnUpVotePressed()
		{
			if (this.voteInProgress)
			{
				return;
			}
			this.voteInProgress = true;
			this._statusIndexVar.Value = 2;
			this.statusText.gameObject.SetActive(false);
			if (SharedBlocksManager.IsMapIDValid(this.loadedMapID) && this.upVoteButton.enabled)
			{
				SharedBlocksManager.instance.RequestVote(this.loadedMapID, true, new Action<bool, string>(this.OnVoteResponse));
				this.upVoteButton.buttonRenderer.material = this.upVoteButton.pressedMaterial;
				this.downVoteButton.buttonRenderer.material = this.buttonDefaultMaterial;
				this.upVoteButton.enabled = false;
				this.downVoteButton.enabled = true;
			}
		}

		private void OnDownVotePressed()
		{
			if (this.voteInProgress)
			{
				return;
			}
			this.voteInProgress = true;
			this._statusIndexVar.Value = 2;
			this.statusText.gameObject.SetActive(false);
			if (SharedBlocksManager.IsMapIDValid(this.loadedMapID) && this.downVoteButton.enabled)
			{
				SharedBlocksManager.instance.RequestVote(this.loadedMapID, false, new Action<bool, string>(this.OnVoteResponse));
				this.upVoteButton.buttonRenderer.material = this.buttonDefaultMaterial;
				this.downVoteButton.buttonRenderer.material = this.downVoteButton.pressedMaterial;
				this.upVoteButton.enabled = true;
				this.downVoteButton.enabled = false;
			}
		}

		private void OnVoteResponse(bool success, string message)
		{
			this.voteInProgress = false;
			if (success)
			{
				this._statusIndexVar.Value = 0;
				this.statusText.gameObject.SetActive(true);
			}
			else
			{
				int num;
				if (int.TryParse(message, out num))
				{
					this._statusIndexVar.Value = num;
				}
				else
				{
					this.statusText.text = message;
					Debug.Log("[LOCALIZATION::SHARED_BLOCKS_VOTING_STATION] WARNING: Passing in a non-int value for the [message]. This will not be localized!");
				}
				this.statusText.gameObject.SetActive(true);
				if (!this.loadedMapID.IsNullOrEmpty())
				{
					this.upVoteButton.buttonRenderer.material = this.buttonDefaultMaterial;
					this.downVoteButton.buttonRenderer.material = this.buttonDefaultMaterial;
					this.upVoteButton.enabled = true;
					this.downVoteButton.enabled = true;
				}
			}
			this.clearStatusTime = Time.time + this.clearStatusDelay;
			this.waitingToClearStatus = true;
		}

		private void LateUpdate()
		{
			if (this.waitingToClearStatus && Time.time > this.clearStatusTime)
			{
				this.waitingToClearStatus = false;
				this._statusIndexVar.Value = 2;
				this.statusText.gameObject.SetActive(false);
			}
		}

		private void OnLoadedMapChanged(string mapID)
		{
			this.loadedMapID = mapID;
			this.statusText.gameObject.SetActive(false);
			this.UpdateScreen();
		}

		private void OnMapCleared()
		{
			this.loadedMapID = null;
			this.statusText.gameObject.SetActive(false);
			this.UpdateScreen();
		}

		private void UpdateScreen()
		{
			if (!this.loadedMapID.IsNullOrEmpty() && SharedBlocksManager.IsMapIDValid(this.loadedMapID))
			{
				this._mapDisplayIndexVar.Value = 1;
				this._mapNameVar.Value = SharedBlocksTerminal.MapIDToDisplayedString(this.loadedMapID);
				this.upVoteButton.enabled = true;
				this.downVoteButton.enabled = true;
				this.upVoteButton.buttonRenderer.material = this.buttonDefaultMaterial;
				this.downVoteButton.buttonRenderer.material = this.buttonDefaultMaterial;
				return;
			}
			this._mapDisplayIndexVar.Value = 0;
			this.upVoteButton.enabled = false;
			this.downVoteButton.enabled = false;
			this.upVoteButton.buttonRenderer.material = this.buttonDisabledMaterial;
			this.downVoteButton.buttonRenderer.material = this.buttonDisabledMaterial;
		}

		public const int VOTING_STATUS_INDEX_SUCCESS = 0;

		public const int VOTING_STATUS_INDEX_NOT_LOGGED_IN = 1;

		public const int VOTING_STATUS_INDEX_EMPTY = 2;

		private const int MAP_DISPLAY_INDEX_NONE = 0;

		private const int MAP_DISPLAY_INDEX_NAMED_MAP = 1;

		[SerializeField]
		private TMP_Text screenText;

		[SerializeField]
		private TMP_Text statusText;

		[SerializeField]
		private GorillaPressableButton upVoteButton;

		[SerializeField]
		private GorillaPressableButton downVoteButton;

		[SerializeField]
		private GTZone tableZone = GTZone.monkeBlocksShared;

		[SerializeField]
		private Material buttonDefaultMaterial;

		[SerializeField]
		private Material buttonDisabledMaterial;

		[Header("Localization Setup")]
		[SerializeField]
		private LocalizedText _statusLocText;

		[SerializeField]
		private LocalizedText _screenLocText;

		private BuilderTable table;

		private string loadedMapID = string.Empty;

		private bool voteInProgress;

		private bool waitingToClearStatus;

		private float clearStatusTime;

		private float clearStatusDelay = 2f;

		private IntVariable _statusIndexVar;

		private IntVariable _mapDisplayIndexVar;

		private StringVariable _mapNameVar;

		private List<MeshRenderer> meshes = new List<MeshRenderer>(12);
	}
}
