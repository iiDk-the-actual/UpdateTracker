using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GorillaGameModes;
using GorillaLocomotion;
using GorillaNetworking;
using Photon.Pun;
using Photon.Realtime;
using PlayFab;
using UnityEngine;

public class Gorillanalytics : MonoBehaviour
{
	private IEnumerator Start()
	{
		PlayFabTitleDataCache.Instance.GetTitleData("GorillanalyticsChance", delegate(string s)
		{
			double num;
			if (double.TryParse(s, out num))
			{
				this.oneOverChance = num;
			}
		}, delegate(PlayFabError e)
		{
		}, false);
		for (;;)
		{
			yield return new WaitForSeconds(this.interval);
			if ((double)Random.Range(0f, 1f) < 1.0 / this.oneOverChance && PlayFabClientAPI.IsClientLoggedIn())
			{
				this.UploadGorillanalytics();
			}
		}
		yield break;
	}

	private void UploadGorillanalytics()
	{
		try
		{
			string text;
			string text2;
			string text3;
			this.GetMapModeQueue(out text, out text2, out text3);
			Vector3 position = GTPlayer.Instance.headCollider.transform.position;
			Vector3 averagedVelocity = GTPlayer.Instance.AveragedVelocity;
			this.uploadData.version = NetworkSystemConfig.AppVersion;
			this.uploadData.upload_chance = this.oneOverChance;
			this.uploadData.map = text;
			this.uploadData.mode = text2;
			this.uploadData.queue = text3;
			this.uploadData.player_count = (int)(PhotonNetwork.InRoom ? PhotonNetwork.CurrentRoom.PlayerCount : 0);
			this.uploadData.pos_x = position.x;
			this.uploadData.pos_y = position.y;
			this.uploadData.pos_z = position.z;
			this.uploadData.vel_x = averagedVelocity.x;
			this.uploadData.vel_y = averagedVelocity.y;
			this.uploadData.vel_z = averagedVelocity.z;
			this.uploadData.cosmetics_owned = string.Join(";", CosmeticsController.instance.unlockedCosmetics.Select((CosmeticsController.CosmeticItem c) => c.itemName));
			this.uploadData.cosmetics_worn = string.Join(";", CosmeticsController.instance.currentWornSet.items.Select((CosmeticsController.CosmeticItem c) => c.itemName));
			GorillaServer.Instance.UploadGorillanalytics(this.uploadData);
		}
		catch (Exception ex)
		{
			Debug.LogError(ex);
		}
	}

	private void GetMapModeQueue(out string map, out string mode, out string queue)
	{
		if (!PhotonNetwork.InRoom)
		{
			map = "none";
			mode = "none";
			queue = "none";
			return;
		}
		object obj = null;
		Room currentRoom = PhotonNetwork.CurrentRoom;
		if (currentRoom != null)
		{
			currentRoom.CustomProperties.TryGetValue("gameMode", out obj);
		}
		string gameMode = ((obj != null) ? obj.ToString() : null) ?? "";
		GTZone gtzone = GorillaTagger.Instance.offlineVRRig.zoneEntity.currentZone;
		if (gtzone == GTZone.cityNoBuildings || gtzone == GTZone.cityWithSkyJungle || gtzone == GTZone.mall)
		{
			gtzone = GTZone.city;
		}
		if (gtzone == GTZone.tutorial)
		{
			gtzone = GTZone.forest;
		}
		if (gtzone == GTZone.ghostReactorTunnel)
		{
			gtzone = GTZone.ghostReactor;
		}
		map = gtzone.ToString().ToLower();
		if (NetworkSystem.Instance.SessionIsPrivate)
		{
			map += "private";
		}
		mode = this.modes.FirstOrDefault((string s) => gameMode.Contains(s)) ?? "unknown";
		queue = this.queues.FirstOrDefault((string s) => gameMode.Contains(s)) ?? "unknown";
	}

	public float interval = 60f;

	public double oneOverChance = 4320.0;

	public PhotonNetworkController photonNetworkController;

	public GameModeZoneMapping gameModeData;

	public List<string> maps;

	public List<string> modes;

	public List<string> queues;

	private readonly Gorillanalytics.UploadData uploadData = new Gorillanalytics.UploadData();

	private class UploadData
	{
		public string version;

		public double upload_chance;

		public string map;

		public string mode;

		public string queue;

		public int player_count;

		public float pos_x;

		public float pos_y;

		public float pos_z;

		public float vel_x;

		public float vel_y;

		public float vel_z;

		public string cosmetics_owned;

		public string cosmetics_worn;
	}
}
