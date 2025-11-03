using System;
using System.Collections.Generic;
using GorillaTagScripts.VirtualStumpCustomMaps;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

namespace GorillaTagScripts.CustomMapSupport
{
	internal class CMSSerializer : GorillaSerializer
	{
		public void Awake()
		{
			if (CMSSerializer.instance != null)
			{
				Object.Destroy(this);
			}
			CMSSerializer.instance = this;
			CMSSerializer.hasInstance = true;
		}

		public void OnEnable()
		{
			CustomMapManager.OnMapLoadComplete.RemoveListener(new UnityAction<bool>(this.OnCustomMapLoaded));
			CustomMapManager.OnMapLoadComplete.AddListener(new UnityAction<bool>(this.OnCustomMapLoaded));
		}

		public void OnDisable()
		{
			CustomMapManager.OnMapLoadComplete.RemoveListener(new UnityAction<bool>(this.OnCustomMapLoaded));
		}

		private void OnCustomMapLoaded(bool success)
		{
			if (success)
			{
				CMSSerializer.RequestSyncTriggerHistory();
			}
		}

		public static void ResetSyncedMapObjects()
		{
			CMSSerializer.triggerHistory.Clear();
			CMSSerializer.triggerCounts.Clear();
			CMSSerializer.registeredTriggersPerScene.Clear();
			CMSSerializer.waitingForTriggerHistory = false;
			CMSSerializer.waitingForTriggerCounts = false;
		}

		public static void RegisterTrigger(string sceneName, CMSTrigger trigger)
		{
			Dictionary<byte, CMSTrigger> dictionary;
			if (CMSSerializer.registeredTriggersPerScene.TryGetValue(sceneName, out dictionary))
			{
				if (!dictionary.ContainsKey(trigger.GetID()))
				{
					dictionary.Add(trigger.GetID(), trigger);
					return;
				}
			}
			else
			{
				CMSSerializer.registeredTriggersPerScene.Add(sceneName, new Dictionary<byte, CMSTrigger> { 
				{
					trigger.GetID(),
					trigger
				} });
			}
		}

		private static bool TryGetRegisteredTrigger(byte triggerID, out CMSTrigger trigger)
		{
			trigger = null;
			foreach (KeyValuePair<string, Dictionary<byte, CMSTrigger>> keyValuePair in CMSSerializer.registeredTriggersPerScene)
			{
				if (keyValuePair.Value.TryGetValue(triggerID, out trigger))
				{
					return true;
				}
			}
			return false;
		}

		public static void UnregisterTriggers(string forScene)
		{
			CMSSerializer.registeredTriggersPerScene.Remove(forScene);
		}

		public static void ResetTrigger(byte triggerID)
		{
			CMSSerializer.triggerCounts.Remove(triggerID);
		}

		private static void RequestSyncTriggerHistory()
		{
			if (!CMSSerializer.hasInstance || !NetworkSystem.Instance.InRoom || NetworkSystem.Instance.IsMasterClient)
			{
				return;
			}
			CMSSerializer.waitingForTriggerHistory = true;
			CMSSerializer.waitingForTriggerCounts = true;
			CMSSerializer.instance.SendRPC("RequestSyncTriggerHistory_RPC", false, Array.Empty<object>());
		}

		[PunRPC]
		private void RequestSyncTriggerHistory_RPC(PhotonMessageInfo info)
		{
			GorillaNot.IncrementRPCCall(info, "RequestSyncTriggerHistory_RPC");
			if (!NetworkSystem.Instance.InRoom || !NetworkSystem.Instance.IsMasterClient)
			{
				return;
			}
			NetPlayer player = NetworkSystem.Instance.GetPlayer(info.Sender);
			if (player.CheckSingleCallRPC(NetPlayer.SingleCallRPC.CMS_RequestTriggerHistory))
			{
				return;
			}
			player.ReceivedSingleCallRPC(NetPlayer.SingleCallRPC.CMS_RequestTriggerHistory);
			byte[] array = CMSSerializer.triggerHistory.ToArray();
			base.SendRPC("SyncTriggerHistory_RPC", info.Sender, new object[] { array });
			base.SendRPC("SyncTriggerCounts_RPC", info.Sender, new object[] { CMSSerializer.triggerCounts });
		}

		[PunRPC]
		private void SyncTriggerHistory_RPC(byte[] syncedTriggerHistory, PhotonMessageInfo info)
		{
			GorillaNot.IncrementRPCCall(info, "SyncTriggerHistory_RPC");
			if (!NetworkSystem.Instance.InRoom || !info.Sender.IsMasterClient)
			{
				return;
			}
			NetPlayer player = NetworkSystem.Instance.GetPlayer(info.Sender);
			if (player.CheckSingleCallRPC(NetPlayer.SingleCallRPC.CMS_SyncTriggerHistory))
			{
				return;
			}
			player.ReceivedSingleCallRPC(NetPlayer.SingleCallRPC.CMS_SyncTriggerHistory);
			if (!CMSSerializer.waitingForTriggerHistory)
			{
				return;
			}
			CMSSerializer.triggerHistory.Clear();
			if (!syncedTriggerHistory.IsNullOrEmpty<byte>())
			{
				CMSSerializer.triggerHistory.AddRange(syncedTriggerHistory);
			}
			CMSSerializer.waitingForTriggerHistory = false;
			foreach (string text in CMSSerializer.scenesWaitingForTriggerHistory)
			{
				CMSSerializer.ProcessTriggerHistory(text);
			}
			CMSSerializer.scenesWaitingForTriggerHistory.Clear();
		}

		[PunRPC]
		private void SyncTriggerCounts_RPC(Dictionary<byte, byte> syncedTriggerCounts, PhotonMessageInfo info)
		{
			GorillaNot.IncrementRPCCall(info, "SyncTriggerCounts_RPC");
			if (!NetworkSystem.Instance.InRoom || !info.Sender.IsMasterClient)
			{
				return;
			}
			NetPlayer player = NetworkSystem.Instance.GetPlayer(info.Sender);
			if (player.CheckSingleCallRPC(NetPlayer.SingleCallRPC.CMS_SyncTriggerCounts))
			{
				return;
			}
			player.ReceivedSingleCallRPC(NetPlayer.SingleCallRPC.CMS_SyncTriggerCounts);
			if (!CMSSerializer.waitingForTriggerCounts)
			{
				return;
			}
			CMSSerializer.triggerCounts.Clear();
			if (syncedTriggerCounts != null && syncedTriggerCounts.Count > 0)
			{
				CMSSerializer.triggerCounts = syncedTriggerCounts;
			}
			CMSSerializer.waitingForTriggerCounts = false;
			foreach (string text in CMSSerializer.scenesWaitingForTriggerCounts)
			{
				CMSSerializer.ProcessTriggerCounts(text);
			}
			CMSSerializer.scenesWaitingForTriggerCounts.Clear();
		}

		public static void ProcessSceneLoad(string sceneName)
		{
			if (CMSSerializer.waitingForTriggerHistory)
			{
				CMSSerializer.scenesWaitingForTriggerHistory.Add(sceneName);
			}
			else
			{
				CMSSerializer.ProcessTriggerHistory(sceneName);
			}
			if (CMSSerializer.waitingForTriggerCounts)
			{
				CMSSerializer.scenesWaitingForTriggerCounts.Add(sceneName);
				return;
			}
			CMSSerializer.ProcessTriggerCounts(sceneName);
		}

		private static void ProcessTriggerHistory(string forScene)
		{
			Dictionary<byte, CMSTrigger> dictionary;
			if (CMSSerializer.registeredTriggersPerScene.TryGetValue(forScene, out dictionary))
			{
				foreach (byte b in CMSSerializer.triggerHistory)
				{
					CMSTrigger cmstrigger;
					if (dictionary.TryGetValue(b, out cmstrigger))
					{
						cmstrigger.Trigger(1.0, false, true);
					}
				}
			}
			UnityEvent<string> onTriggerHistoryProcessedForScene = CMSSerializer.OnTriggerHistoryProcessedForScene;
			if (onTriggerHistoryProcessedForScene == null)
			{
				return;
			}
			onTriggerHistoryProcessedForScene.Invoke(forScene);
		}

		private static void ProcessTriggerCounts(string forScene)
		{
			Dictionary<byte, CMSTrigger> dictionary;
			if (CMSSerializer.registeredTriggersPerScene.TryGetValue(forScene, out dictionary))
			{
				List<byte> list = new List<byte>();
				foreach (KeyValuePair<byte, byte> keyValuePair in CMSSerializer.triggerCounts)
				{
					CMSTrigger cmstrigger;
					if (dictionary.TryGetValue(keyValuePair.Key, out cmstrigger))
					{
						if (cmstrigger.numAllowedTriggers > 0)
						{
							cmstrigger.SetTriggerCount(keyValuePair.Value);
						}
						else
						{
							list.Add(keyValuePair.Key);
						}
					}
				}
				foreach (byte b in list)
				{
					CMSSerializer.triggerCounts.Remove(b);
				}
			}
		}

		public static void RequestTrigger(byte triggerID)
		{
			if (!CMSSerializer.hasInstance)
			{
				return;
			}
			if (!NetworkSystem.Instance.InRoom || NetworkSystem.Instance.IsMasterClient)
			{
				double num = (double)Time.time;
				if (NetworkSystem.Instance.InRoom)
				{
					num = PhotonNetwork.Time;
					CMSSerializer.instance.SendRPC("ActivateTrigger_RPC", true, new object[]
					{
						triggerID,
						NetworkSystem.Instance.LocalPlayer.ActorNumber
					});
				}
				CMSSerializer.instance.ActivateTrigger(triggerID, num, true);
				return;
			}
			CMSSerializer.instance.SendRPC("RequestTrigger_RPC", false, new object[] { triggerID });
		}

		[PunRPC]
		private void RequestTrigger_RPC(byte triggerID, PhotonMessageInfo info)
		{
			GorillaNot.IncrementRPCCall(info, "RequestTrigger_RPC");
			if (!NetworkSystem.Instance.InRoom || !NetworkSystem.Instance.IsMasterClient)
			{
				return;
			}
			NetPlayer player = NetworkSystem.Instance.GetPlayer(info.Sender);
			RigContainer rigContainer;
			if (!VRRigCache.Instance.TryGetVrrig(player, out rigContainer) || !rigContainer.Rig.fxSettings.callSettings[11].CallLimitSettings.CheckCallTime(Time.unscaledTime))
			{
				return;
			}
			CMSTrigger cmstrigger;
			if (CMSSerializer.TryGetRegisteredTrigger(triggerID, out cmstrigger))
			{
				if (!cmstrigger.CanTrigger())
				{
					return;
				}
				Vector3 position = cmstrigger.gameObject.transform.position;
				RigContainer rigContainer2;
				if (!VRRigCache.Instance.TryGetVrrig(info.Sender, out rigContainer2))
				{
					return;
				}
				if ((rigContainer2.Rig.bodyTransform.position - position).sqrMagnitude > cmstrigger.validationDistanceSquared)
				{
					return;
				}
			}
			base.SendRPC("ActivateTrigger_RPC", true, new object[]
			{
				triggerID,
				info.Sender.ActorNumber
			});
			this.ActivateTrigger(triggerID, info.SentServerTime, false);
		}

		[PunRPC]
		private void ActivateTrigger_RPC(byte triggerID, int originatingPlayer, PhotonMessageInfo info)
		{
			GorillaNot.IncrementRPCCall(info, "ActivateTrigger_RPC");
			if (!NetworkSystem.Instance.InRoom || !info.Sender.IsMasterClient)
			{
				return;
			}
			if (info.SentServerTime < 0.0 || info.SentServerTime > 4294967.295)
			{
				return;
			}
			double num = (double)PhotonNetwork.GetPing() / 1000.0;
			if (!Utils.ValidateServerTime(info.SentServerTime, Math.Max(10.0, num * 2.0)))
			{
				return;
			}
			if (!CMSSerializer.ActivateTriggerCallLimiter.CheckCallTime(Time.unscaledTime))
			{
				return;
			}
			this.ActivateTrigger(triggerID, info.SentServerTime, NetworkSystem.Instance.LocalPlayer.ActorNumber == originatingPlayer);
		}

		private void ActivateTrigger(byte triggerID, double triggerTime = -1.0, bool originatedLocally = false)
		{
			CMSTrigger cmstrigger;
			bool flag = CMSSerializer.TryGetRegisteredTrigger(triggerID, out cmstrigger);
			if (!double.IsFinite(triggerTime))
			{
				triggerTime = -1.0;
			}
			byte b;
			bool flag2 = CMSSerializer.triggerCounts.TryGetValue(triggerID, out b);
			bool flag3 = !flag || cmstrigger.numAllowedTriggers > 0;
			if (flag2)
			{
				CMSSerializer.triggerCounts[triggerID] = ((b == byte.MaxValue) ? byte.MaxValue : (b += 1));
			}
			else if (flag3)
			{
				CMSSerializer.triggerCounts.Add(triggerID, 1);
			}
			CMSSerializer.triggerHistory.Remove(triggerID);
			CMSSerializer.triggerHistory.Add(triggerID);
			if (flag)
			{
				cmstrigger.Trigger(triggerTime, originatedLocally, false);
			}
		}

		[OnEnterPlay_SetNull]
		private static volatile CMSSerializer instance;

		[OnEnterPlay_Set(false)]
		private static bool hasInstance;

		private static Dictionary<string, Dictionary<byte, CMSTrigger>> registeredTriggersPerScene = new Dictionary<string, Dictionary<byte, CMSTrigger>>();

		private static List<byte> triggerHistory = new List<byte>();

		private static Dictionary<byte, byte> triggerCounts = new Dictionary<byte, byte>();

		private static bool waitingForTriggerHistory;

		private static List<string> scenesWaitingForTriggerHistory = new List<string>();

		private static bool waitingForTriggerCounts;

		private static List<string> scenesWaitingForTriggerCounts = new List<string>();

		private static CallLimiter ActivateTriggerCallLimiter = new CallLimiter(50, 1f, 0.5f);

		public static UnityEvent<string> OnTriggerHistoryProcessedForScene = new UnityEvent<string>();
	}
}
