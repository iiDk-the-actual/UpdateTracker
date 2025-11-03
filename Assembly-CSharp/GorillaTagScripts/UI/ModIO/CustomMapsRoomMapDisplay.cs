using System;
using System.Threading.Tasks;
using GorillaTagScripts.VirtualStumpCustomMaps;
using Modio;
using Modio.Mods;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace GorillaTagScripts.UI.ModIO
{
	public class CustomMapsRoomMapDisplay : MonoBehaviour
	{
		public void Start()
		{
			this.roomMapNameText.text = this.noRoomMapString;
			this.roomMapStatusText.text = this.notLoadedStatusString;
			this.roomMapLabelText.gameObject.SetActive(true);
			this.roomMapNameText.gameObject.SetActive(true);
			this.roomMapStatusLabelText.gameObject.SetActive(false);
			this.roomMapStatusText.gameObject.SetActive(false);
			NetworkSystem.Instance.OnMultiplayerStarted += this.OnJoinedRoom;
			NetworkSystem.Instance.OnReturnedToSinglePlayer += this.OnDisconnectedFromRoom;
			CustomMapManager.OnRoomMapChanged.AddListener(new UnityAction<ModId>(this.OnRoomMapChanged));
			CustomMapManager.OnMapLoadStatusChanged.AddListener(new UnityAction<MapLoadStatus, int, string>(this.OnMapLoadProgress));
			CustomMapManager.OnMapLoadComplete.AddListener(new UnityAction<bool>(this.OnMapLoadComplete));
		}

		public void OnDestroy()
		{
			NetworkSystem.Instance.OnMultiplayerStarted -= this.OnJoinedRoom;
			NetworkSystem.Instance.OnReturnedToSinglePlayer -= this.OnDisconnectedFromRoom;
			CustomMapManager.OnRoomMapChanged.RemoveListener(new UnityAction<ModId>(this.OnRoomMapChanged));
		}

		private void OnJoinedRoom()
		{
			this.UpdateRoomMap();
		}

		private void OnDisconnectedFromRoom()
		{
			this.UpdateRoomMap();
		}

		private void OnRoomMapChanged(ModId roomMapModId)
		{
			this.UpdateRoomMap();
		}

		private async Task UpdateRoomMap()
		{
			ModId currentRoomMap = CustomMapManager.GetRoomMapId();
			if (currentRoomMap == ModId.Null)
			{
				this.roomMapNameText.text = this.noRoomMapString;
				this.roomMapStatusLabelText.gameObject.SetActive(false);
				this.roomMapStatusText.gameObject.SetActive(false);
			}
			else
			{
				ValueTuple<Error, Mod> valueTuple = await ModIOManager.GetMod(currentRoomMap, false, null);
				Error item = valueTuple.Item1;
				Mod item2 = valueTuple.Item2;
				if (item)
				{
					this.roomMapNameText.text = string.Format("FAILED TO GET MOD INFO.\n({0})", item.Code);
				}
				else
				{
					this.roomMapNameText.text = item2.Name;
					this.roomMapStatusLabelText.gameObject.SetActive(true);
					if (CustomMapLoader.IsMapLoaded(currentRoomMap))
					{
						this.roomMapStatusText.text = this.readyToPlayStatusString;
						this.roomMapStatusText.color = this.readyToPlayStatusStringColor;
					}
					else if (CustomMapManager.IsLoading(currentRoomMap._id))
					{
						this.roomMapStatusText.text = this.loadingStatusString;
						this.roomMapStatusText.color = this.loadingStatusStringColor;
					}
					else
					{
						this.roomMapStatusText.text = this.notLoadedStatusString;
						this.roomMapStatusText.color = this.notLoadedStatusStringColor;
					}
					this.roomMapStatusText.gameObject.SetActive(true);
				}
			}
		}

		private void OnMapLoadComplete(bool success)
		{
			if (success)
			{
				this.roomMapStatusText.text = this.readyToPlayStatusString;
				this.roomMapStatusText.color = this.readyToPlayStatusStringColor;
				return;
			}
			this.roomMapStatusText.text = this.loadFailedStatusString;
			this.roomMapStatusText.color = this.loadFailedStatusStringColor;
		}

		private void OnMapLoadProgress(MapLoadStatus status, int progress, string message)
		{
			if (status - MapLoadStatus.Downloading <= 1)
			{
				this.roomMapStatusText.text = this.loadingStatusString;
				this.roomMapStatusText.color = this.loadingStatusStringColor;
			}
		}

		[SerializeField]
		private TMP_Text roomMapLabelText;

		[SerializeField]
		private TMP_Text roomMapNameText;

		[SerializeField]
		private TMP_Text roomMapStatusLabelText;

		[SerializeField]
		private TMP_Text roomMapStatusText;

		[SerializeField]
		private string noRoomMapString = "NONE";

		[SerializeField]
		private string notLoadedStatusString = "NOT LOADED";

		[SerializeField]
		private string loadingStatusString = "LOADING...";

		[SerializeField]
		private string readyToPlayStatusString = "READY!";

		[SerializeField]
		private string loadFailedStatusString = "LOAD FAILED";

		[SerializeField]
		private Color notLoadedStatusStringColor = Color.red;

		[SerializeField]
		private Color loadingStatusStringColor = Color.yellow;

		[SerializeField]
		private Color readyToPlayStatusStringColor = Color.green;

		[SerializeField]
		private Color loadFailedStatusStringColor = Color.red;
	}
}
