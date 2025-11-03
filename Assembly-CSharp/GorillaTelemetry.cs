using System;
using System.Buffers;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using GorillaGameModes;
using GorillaNetworking;
using JetBrains.Annotations;
using KID.Model;
using Newtonsoft.Json;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.EventsModels;
using UnityEngine;

public static class GorillaTelemetry
{
	static GorillaTelemetry()
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary["User"] = null;
		dictionary["EventType"] = null;
		dictionary["ZoneId"] = null;
		dictionary["SubZoneId"] = null;
		GorillaTelemetry.gZoneEventArgs = dictionary;
		Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
		dictionary2["User"] = null;
		dictionary2["EventType"] = null;
		GorillaTelemetry.gNotifEventArgs = dictionary2;
		GorillaTelemetry.LastZone = GTZone.none;
		GorillaTelemetry.LastSubZone = GTSubZone.none;
		GorillaTelemetry.LastZoneEventType = GTZoneEventType.none;
		Dictionary<string, object> dictionary3 = new Dictionary<string, object>();
		dictionary3["User"] = null;
		dictionary3["EventType"] = null;
		dictionary3["game_mode"] = null;
		GorillaTelemetry.gGameModeStartEventArgs = dictionary3;
		Dictionary<string, object> dictionary4 = new Dictionary<string, object>();
		dictionary4["User"] = null;
		dictionary4["EventType"] = null;
		dictionary4["Items"] = null;
		GorillaTelemetry.gShopEventArgs = dictionary4;
		GorillaTelemetry.gSingleItemParam = new CosmeticsController.CosmeticItem[1];
		GorillaTelemetry.gSingleItemBuilderParam = new BuilderSetManager.BuilderSetStoreItem[1];
		Dictionary<string, object> dictionary5 = new Dictionary<string, object>();
		dictionary5["User"] = null;
		dictionary5["EventType"] = null;
		dictionary5["AgeCategory"] = null;
		dictionary5["VoiceChatEnabled"] = null;
		dictionary5["CustomUsernameEnabled"] = null;
		dictionary5["JoinGroups"] = null;
		GorillaTelemetry.gKidEventArgs = dictionary5;
		Dictionary<string, object> dictionary6 = new Dictionary<string, object>();
		dictionary6["User"] = null;
		dictionary6["WamGameId"] = null;
		dictionary6["WamMachineId"] = null;
		GorillaTelemetry.gWamGameStartArgs = dictionary6;
		Dictionary<string, object> dictionary7 = new Dictionary<string, object>();
		dictionary7["User"] = null;
		dictionary7["WamGameId"] = null;
		dictionary7["WamMachineId"] = null;
		dictionary7["WamMLevelNumber"] = null;
		dictionary7["WamGoodMolesShown"] = null;
		dictionary7["WamHazardMolesShown"] = null;
		dictionary7["WamLevelMinScore"] = null;
		dictionary7["WamLevelScore"] = null;
		dictionary7["WamHazardMolesHit"] = null;
		dictionary7["WamGameState"] = null;
		GorillaTelemetry.gWamLevelEndArgs = dictionary7;
		Dictionary<string, object> dictionary8 = new Dictionary<string, object>();
		dictionary8["CustomMapName"] = null;
		dictionary8["CustomMapModId"] = null;
		dictionary8["LowestFPS"] = null;
		dictionary8["LowestFPSDrawCalls"] = null;
		dictionary8["LowestFPSPlayerCount"] = null;
		dictionary8["AverageFPS"] = null;
		dictionary8["AverageDrawCalls"] = null;
		dictionary8["AveragePlayerCount"] = null;
		dictionary8["HighestFPS"] = null;
		dictionary8["HighestFPSDrawCalls"] = null;
		dictionary8["HighestFPSPlayerCount"] = null;
		dictionary8["PlaytimeInSeconds"] = null;
		GorillaTelemetry.gCustomMapPerfArgs = dictionary8;
		Dictionary<string, object> dictionary9 = new Dictionary<string, object>();
		dictionary9["User"] = null;
		dictionary9["CustomMapName"] = null;
		dictionary9["CustomMapModId"] = null;
		dictionary9["CustomMapCreator"] = null;
		dictionary9["MinPlayerCount"] = null;
		dictionary9["MaxPlayerCount"] = null;
		dictionary9["PlaytimeOnMap"] = null;
		dictionary9["PrivateRoom"] = null;
		GorillaTelemetry.gCustomMapTrackingMetrics = dictionary9;
		Dictionary<string, object> dictionary10 = new Dictionary<string, object>();
		dictionary10["User"] = null;
		dictionary10["CustomMapName"] = null;
		dictionary10["CustomMapModId"] = null;
		dictionary10["CustomMapCreator"] = null;
		GorillaTelemetry.gCustomMapDownloadMetrics = dictionary10;
		Dictionary<string, object> dictionary11 = new Dictionary<string, object>();
		dictionary11["User"] = null;
		dictionary11["ghost_game_id"] = null;
		dictionary11["event_timestamp"] = null;
		dictionary11["initial_cores_balance"] = null;
		dictionary11["number_of_players"] = null;
		dictionary11["start_at_beginning"] = null;
		dictionary11["seconds_into_shift_at_join"] = null;
		dictionary11["floor_joined"] = null;
		dictionary11["player_rank"] = null;
		dictionary11["is_private_room"] = null;
		GorillaTelemetry.gGhostReactorShiftStartArgs = dictionary11;
		Dictionary<string, object> dictionary12 = new Dictionary<string, object>();
		dictionary12["User"] = null;
		dictionary12["ghost_game_id"] = null;
		dictionary12["event_timestamp"] = null;
		dictionary12["final_cores_balance"] = null;
		dictionary12["total_cores_collected_by_player"] = null;
		dictionary12["total_cores_collected_by_group"] = null;
		dictionary12["total_cores_spent_by_player"] = null;
		dictionary12["total_cores_spent_by_group"] = null;
		dictionary12["gates_unlocked"] = null;
		dictionary12["died"] = null;
		dictionary12["items_purchased"] = null;
		dictionary12["shift_cut"] = null;
		dictionary12["play_duration"] = null;
		dictionary12["started_late"] = null;
		dictionary12["time_started"] = null;
		dictionary12["reason"] = null;
		dictionary12["max_number_in_game"] = null;
		dictionary12["end_number_in_game"] = null;
		dictionary12["items_picked_up"] = null;
		dictionary12["revives"] = null;
		dictionary12["num_shifts_played"] = null;
		GorillaTelemetry.gGhostReactorShiftEndArgs = dictionary12;
		Dictionary<string, object> dictionary13 = new Dictionary<string, object>();
		dictionary13["User"] = null;
		dictionary13["ghost_game_id"] = null;
		dictionary13["event_timestamp"] = null;
		dictionary13["initial_cores_balance"] = null;
		dictionary13["number_of_players"] = null;
		dictionary13["start_at_beginning"] = null;
		dictionary13["seconds_into_shift_at_join"] = null;
		dictionary13["player_rank"] = null;
		dictionary13["floor"] = null;
		dictionary13["preset"] = null;
		dictionary13["modifier"] = null;
		dictionary13["is_private_room"] = null;
		GorillaTelemetry.gGhostReactorFloorStartArgs = dictionary13;
		Dictionary<string, object> dictionary14 = new Dictionary<string, object>();
		dictionary14["User"] = null;
		dictionary14["ghost_game_id"] = null;
		dictionary14["event_timestamp"] = null;
		dictionary14["final_cores_balance"] = null;
		dictionary14["total_cores_collected_by_player"] = null;
		dictionary14["total_cores_collected_by_group"] = null;
		dictionary14["total_cores_spent_by_player"] = null;
		dictionary14["total_cores_spent_by_group"] = null;
		dictionary14["gates_unlocked"] = null;
		dictionary14["died"] = null;
		dictionary14["items_purchased"] = null;
		dictionary14["shift_cut"] = null;
		dictionary14["play_duration"] = null;
		dictionary14["started_late"] = null;
		dictionary14["time_started"] = null;
		dictionary14["max_number_in_game"] = null;
		dictionary14["end_number_in_game"] = null;
		dictionary14["items_picked_up"] = null;
		dictionary14["revives"] = null;
		dictionary14["floor"] = null;
		dictionary14["preset"] = null;
		dictionary14["modifier"] = null;
		dictionary14["chaos_seeds_collected"] = null;
		dictionary14["objectives_completed"] = null;
		dictionary14["section"] = null;
		dictionary14["xp_gained"] = null;
		GorillaTelemetry.gGhostReactorFloorEndArgs = dictionary14;
		Dictionary<string, object> dictionary15 = new Dictionary<string, object>();
		dictionary15["User"] = null;
		dictionary15["ghost_game_id"] = null;
		dictionary15["event_timestamp"] = null;
		dictionary15["tool"] = null;
		dictionary15["tool_level"] = null;
		dictionary15["cores_spent"] = null;
		dictionary15["shiny_rocks_spent"] = null;
		dictionary15["floor"] = null;
		dictionary15["preset"] = null;
		GorillaTelemetry.gGhostReactorToolPurchasedArgs = dictionary15;
		Dictionary<string, object> dictionary16 = new Dictionary<string, object>();
		dictionary16["User"] = null;
		dictionary16["ghost_game_id"] = null;
		dictionary16["event_timestamp"] = null;
		dictionary16["new_rank"] = null;
		dictionary16["floor"] = null;
		dictionary16["preset"] = null;
		GorillaTelemetry.gGhostReactorRankUpArgs = dictionary16;
		Dictionary<string, object> dictionary17 = new Dictionary<string, object>();
		dictionary17["User"] = null;
		dictionary17["ghost_game_id"] = null;
		dictionary17["event_timestamp"] = null;
		dictionary17["tool"] = null;
		GorillaTelemetry.gGhostReactorToolUnlockArgs = dictionary17;
		Dictionary<string, object> dictionary18 = new Dictionary<string, object>();
		dictionary18["User"] = null;
		dictionary18["ghost_game_id"] = null;
		dictionary18["event_timestamp"] = null;
		dictionary18["tool"] = null;
		dictionary18["new_level"] = null;
		dictionary18["shiny_rocks_spent"] = null;
		dictionary18["juice_spent"] = null;
		GorillaTelemetry.gGhostReactorPodUpgradePurchasedArgs = dictionary18;
		Dictionary<string, object> dictionary19 = new Dictionary<string, object>();
		dictionary19["User"] = null;
		dictionary19["ghost_game_id"] = null;
		dictionary19["event_timestamp"] = null;
		dictionary19["type"] = null;
		dictionary19["tool"] = null;
		dictionary19["new_level"] = null;
		dictionary19["juice_spent"] = null;
		dictionary19["grift_spent"] = null;
		dictionary19["cores_spent"] = null;
		dictionary19["floor"] = null;
		dictionary19["preset"] = null;
		GorillaTelemetry.gGhostReactorToolUpgradeArgs = dictionary19;
		Dictionary<string, object> dictionary20 = new Dictionary<string, object>();
		dictionary20["User"] = null;
		dictionary20["ghost_game_id"] = null;
		dictionary20["event_timestamp"] = null;
		dictionary20["unlock_time"] = null;
		dictionary20["chaos_seeds_in_queue"] = null;
		dictionary20["floor"] = null;
		dictionary20["preset"] = null;
		GorillaTelemetry.gGhostReactorChaosSeedStartArgs = dictionary20;
		Dictionary<string, object> dictionary21 = new Dictionary<string, object>();
		dictionary21["User"] = null;
		dictionary21["ghost_game_id"] = null;
		dictionary21["event_timestamp"] = null;
		dictionary21["juice_collected"] = null;
		dictionary21["cores_processed_by_overdrive"] = null;
		GorillaTelemetry.gGhostReactorChaosJuiceCollectedArgs = dictionary21;
		Dictionary<string, object> dictionary22 = new Dictionary<string, object>();
		dictionary22["User"] = null;
		dictionary22["ghost_game_id"] = null;
		dictionary22["event_timestamp"] = null;
		dictionary22["shiny_rocks_used"] = null;
		dictionary22["chaos_seeds_in_queue"] = null;
		dictionary22["floor"] = null;
		dictionary22["preset"] = null;
		GorillaTelemetry.gGhostReactorOverdrivePurchasedArgs = dictionary22;
		Dictionary<string, object> dictionary23 = new Dictionary<string, object>();
		dictionary23["User"] = null;
		dictionary23["ghost_game_id"] = null;
		dictionary23["event_timestamp"] = null;
		dictionary23["shiny_rocks_spent"] = null;
		dictionary23["final_credits"] = null;
		dictionary23["floor"] = null;
		dictionary23["preset"] = null;
		GorillaTelemetry.gGhostReactorCreditsRefillPurchasedArgs = dictionary23;
		Dictionary<string, object> dictionary24 = new Dictionary<string, object>();
		dictionary24["User"] = null;
		dictionary24["total_play_time"] = null;
		dictionary24["room_play_time"] = null;
		dictionary24["session_play_time"] = null;
		dictionary24["interval_play_time"] = null;
		dictionary24["terminal_total_time"] = null;
		dictionary24["terminal_interval_time"] = null;
		dictionary24["time_holding_gadget_type_total"] = null;
		dictionary24["time_holding_gadget_type_interval"] = null;
		dictionary24["tags_holding_gadget_type_total"] = null;
		dictionary24["tags_holding_gadget_type_interval"] = null;
		dictionary24["tags_holding_own_gadgets_total"] = null;
		dictionary24["tags_holding_own_gadgets_interval"] = null;
		dictionary24["tags_holding_others_gadgets_total"] = null;
		dictionary24["tags_holding_others_gadgets_interval"] = null;
		dictionary24["resource_collected_total"] = null;
		dictionary24["resource_collected_interval"] = null;
		dictionary24["rounds_played_total"] = null;
		dictionary24["rounds_played_interval"] = null;
		dictionary24["unlocked_nodes"] = null;
		dictionary24["number_of_players"] = null;
		dictionary24["si_purchase_type"] = null;
		dictionary24["si_shiny_rock_cost"] = null;
		dictionary24["si_tech_points_purchased"] = null;
		GorillaTelemetry.gSuperInfectionArgs = dictionary24;
		GameObject gameObject = new GameObject("GorillaTelemetryBatcher");
		Object.DontDestroyOnLoad(gameObject);
		gameObject.AddComponent<GorillaTelemetry.BatchRunner>();
	}

	public static void EnqueueTelemetryEvent(string eventName, object content, [CanBeNull] string[] customTags = null)
	{
		if (content == null || string.IsNullOrWhiteSpace(eventName) || !GorillaServer.Instance.CheckIsMothershipTelemetryEnabled())
		{
			return;
		}
		GorillaTelemetry.telemetryEventsQueueMothership.Enqueue(new MothershipAnalyticsEvent
		{
			event_name = eventName,
			event_timestamp = DateTime.UtcNow.ToString("O"),
			body = JsonConvert.SerializeObject(content),
			custom_tags = ((customTags != null && customTags.Length != 0) ? GorillaTelemetry.SerializeCustomTags(customTags) : string.Empty)
		});
	}

	[Obsolete("EnqueueTelemetryEventPlayFab is deprecated. Use EnqueueTelemetryEvent instead.")]
	private static void EnqueueTelemetryEventPlayFab(EventContents eventContent)
	{
		if (!GorillaServer.Instance.CheckIsPlayFabTelemetryEnabled())
		{
			return;
		}
		GorillaTelemetry.telemetryEventsQueuePlayFab.Enqueue(eventContent);
	}

	private static void QueueTelemetryEventPlayFab(EventContents eventContent)
	{
		GorillaTelemetry.telemetryEventsQueuePlayFab.Enqueue(eventContent);
	}

	private static void FlushPlayFabTelemetry()
	{
		int count = GorillaTelemetry.telemetryEventsQueuePlayFab.Count;
		if (count == 0)
		{
			return;
		}
		EventContents[] array = ArrayPool<EventContents>.Shared.Rent(count);
		try
		{
			int i;
			for (i = 0; i < count; i++)
			{
				EventContents eventContents;
				array[i] = (GorillaTelemetry.telemetryEventsQueuePlayFab.TryDequeue(out eventContents) ? eventContents : null);
			}
			if (i == 0)
			{
				ArrayPool<EventContents>.Shared.Return(array, false);
			}
			else
			{
				WriteEventsRequest writeEventsRequest = new WriteEventsRequest();
				writeEventsRequest.Events = GorillaTelemetry.GetEventListForArrayPlayFab(array, i);
				PlayFabEventsAPI.WriteTelemetryEvents(writeEventsRequest, delegate(WriteEventsResponse result)
				{
				}, delegate(PlayFabError error)
				{
				}, null, null);
			}
		}
		finally
		{
			ArrayPool<EventContents>.Shared.Return(array, false);
		}
	}

	private static void FlushMothershipTelemetry()
	{
		int count = GorillaTelemetry.telemetryEventsQueueMothership.Count;
		if (count == 0)
		{
			return;
		}
		MothershipAnalyticsEvent[] array = ArrayPool<MothershipAnalyticsEvent>.Shared.Rent(count);
		try
		{
			int j;
			for (j = 0; j < count; j++)
			{
				MothershipAnalyticsEvent mothershipAnalyticsEvent;
				array[j] = (GorillaTelemetry.telemetryEventsQueueMothership.TryDequeue(out mothershipAnalyticsEvent) ? mothershipAnalyticsEvent : null);
			}
			if (j == 0)
			{
				ArrayPool<MothershipAnalyticsEvent>.Shared.Return(array, false);
			}
			else
			{
				MothershipWriteEventsRequest mothershipWriteEventsRequest = new MothershipWriteEventsRequest
				{
					title_id = MothershipClientApiUnity.TitleId,
					deployment_id = MothershipClientApiUnity.DeploymentId,
					env_id = MothershipClientApiUnity.EnvironmentId,
					events = new AnalyticsRequestVector(GorillaTelemetry.GetEventListForArrayMothership(array, j))
				};
				MothershipClientApiUnity.WriteEvents(MothershipClientContext.MothershipId, mothershipWriteEventsRequest, delegate(MothershipWriteEventsResponse resp)
				{
				}, delegate(MothershipError err, int i)
				{
				});
			}
		}
		finally
		{
			ArrayPool<MothershipAnalyticsEvent>.Shared.Return(array, false);
		}
	}

	private static List<EventContents> GetEventListForArrayPlayFab(EventContents[] array, int count)
	{
		int num = 0;
		for (int i = 0; i < count; i++)
		{
			if (array[i] != null)
			{
				num++;
			}
		}
		List<EventContents> list;
		if (!GorillaTelemetry.gListPoolPlayFab.TryGetValue(num, out list))
		{
			list = new List<EventContents>(num);
			GorillaTelemetry.gListPoolPlayFab.TryAdd(num, list);
		}
		else
		{
			list.Clear();
		}
		for (int j = 0; j < count; j++)
		{
			if (array[j] != null)
			{
				list.Add(array[j]);
			}
		}
		return list;
	}

	private static List<MothershipAnalyticsEvent> GetEventListForArrayMothership(MothershipAnalyticsEvent[] array, int count)
	{
		int num = 0;
		for (int i = 0; i < count; i++)
		{
			if (array[i] != null)
			{
				num++;
			}
		}
		List<MothershipAnalyticsEvent> list;
		if (!GorillaTelemetry.gListPoolMothership.TryGetValue(num, out list))
		{
			list = new List<MothershipAnalyticsEvent>(num);
			GorillaTelemetry.gListPoolMothership.TryAdd(num, list);
		}
		else
		{
			list.Clear();
		}
		string code = LocalisationManager.CurrentLanguage.Identifier.Code;
		for (int j = 0; j < count; j++)
		{
			if (array[j] != null)
			{
				list.Add(array[j]);
			}
		}
		return list;
	}

	private static bool IsConnected()
	{
		if (!NetworkSystem.Instance.InRoom)
		{
			return false;
		}
		if (GorillaTelemetry.gPlayFabAuth == null)
		{
			GorillaTelemetry.gPlayFabAuth = PlayFabAuthenticator.instance;
		}
		return !(GorillaTelemetry.gPlayFabAuth == null);
	}

	private static bool IsConnectedToPlayfab()
	{
		if (GorillaTelemetry.gPlayFabAuth == null)
		{
			GorillaTelemetry.gPlayFabAuth = PlayFabAuthenticator.instance;
		}
		return !(GorillaTelemetry.gPlayFabAuth == null);
	}

	private static bool IsConnectedIgnoreRoom()
	{
		if (GorillaTelemetry.gPlayFabAuth == null)
		{
			GorillaTelemetry.gPlayFabAuth = PlayFabAuthenticator.instance;
		}
		return !(GorillaTelemetry.gPlayFabAuth == null);
	}

	private static string PlayFabUserId()
	{
		return GorillaTelemetry.gPlayFabAuth.GetPlayFabPlayerId();
	}

	private static string SerializeCustomTags(string[] customTags)
	{
		string text = string.Empty;
		if (customTags != null && customTags.Length != 0)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			for (int i = 0; i < customTags.Length; i++)
			{
				dictionary.Add(string.Format("tag{0}", i + 1), customTags[i]);
			}
			text = JsonConvert.SerializeObject(dictionary);
		}
		return text;
	}

	public static void EnqueueZoneEvent(GTZone zone, GTSubZone subZone, GTZoneEventType zoneEvent)
	{
		if (!GorillaTelemetry.IsConnected())
		{
			return;
		}
		if (zone == GorillaTelemetry.LastZone && subZone == GorillaTelemetry.LastSubZone && zoneEvent == GorillaTelemetry.LastZoneEventType)
		{
			return;
		}
		if (!GorillaServer.Instance.CheckIsTZE_Enabled())
		{
			return;
		}
		string text = GorillaTelemetry.PlayFabUserId();
		string name = zoneEvent.GetName<GTZoneEventType>();
		string name2 = zone.GetName<GTZone>();
		string name3 = subZone.GetName<GTSubZone>();
		bool sessionIsPrivate = NetworkSystem.Instance.SessionIsPrivate;
		Dictionary<string, object> dictionary = GorillaTelemetry.gZoneEventArgs;
		dictionary["User"] = text;
		dictionary["EventType"] = name;
		dictionary["ZoneId"] = name2;
		dictionary["SubZoneId"] = name3;
		dictionary["IsPrivateRoom"] = sessionIsPrivate;
		GorillaTelemetry.EnqueueTelemetryEventPlayFab(new EventContents
		{
			Name = "telemetry_zone_event",
			EventNamespace = GorillaTelemetry.EVENT_NAMESPACE,
			Payload = dictionary
		});
		GorillaTelemetry.EnqueueTelemetryEvent("telemetry_zone_event", dictionary, null);
	}

	public static void PostGameModeEvent(GTGameModeEventType gameModeEvent, GameModeType gameMode)
	{
		if (!GorillaTelemetry.IsConnected())
		{
			return;
		}
		string text = GorillaTelemetry.PlayFabUserId();
		string name = gameModeEvent.GetName<GTGameModeEventType>();
		string name2 = gameMode.GetName<GameModeType>();
		Dictionary<string, object> dictionary = GorillaTelemetry.gGameModeStartEventArgs;
		dictionary["User"] = text;
		dictionary["EventType"] = name;
		dictionary["game_mode"] = name2;
		GorillaTelemetry.EnqueueTelemetryEventPlayFab(new EventContents
		{
			Name = "game_mode_played_event",
			EventNamespace = GorillaTelemetry.EVENT_NAMESPACE,
			Payload = dictionary
		});
		GorillaTelemetry.EnqueueTelemetryEvent("game_mode_played_event", dictionary, null);
	}

	public static void PostShopEvent(VRRig playerRig, GTShopEventType shopEvent, CosmeticsController.CosmeticItem item)
	{
		GorillaTelemetry.gSingleItemParam[0] = item;
		GorillaTelemetry.PostShopEvent(playerRig, shopEvent, GorillaTelemetry.gSingleItemParam);
		GorillaTelemetry.gSingleItemParam[0] = default(CosmeticsController.CosmeticItem);
	}

	private static string[] FetchItemArgs(IList<CosmeticsController.CosmeticItem> items)
	{
		int count = items.Count;
		if (count == 0)
		{
			return Array.Empty<string>();
		}
		HashSet<string> hashSet = new HashSet<string>(count);
		int num = 0;
		for (int i = 0; i < items.Count; i++)
		{
			CosmeticsController.CosmeticItem cosmeticItem = items[i];
			if (!cosmeticItem.isNullItem)
			{
				string itemName = cosmeticItem.itemName;
				if (!string.IsNullOrWhiteSpace(itemName) && !itemName.Contains("NOTHING", StringComparison.InvariantCultureIgnoreCase) && hashSet.Add(itemName))
				{
					num++;
				}
			}
		}
		string[] array = new string[num];
		hashSet.CopyTo(array);
		return array;
	}

	public static void PostShopEvent(VRRig playerRig, GTShopEventType shopEvent, IList<CosmeticsController.CosmeticItem> items)
	{
		if (!GorillaTelemetry.IsConnected())
		{
			return;
		}
		if (!playerRig.isLocal)
		{
			return;
		}
		string text = GorillaTelemetry.PlayFabUserId();
		string name = shopEvent.GetName<GTShopEventType>();
		string[] array = GorillaTelemetry.FetchItemArgs(items);
		Dictionary<string, object> dictionary = GorillaTelemetry.gShopEventArgs;
		dictionary["User"] = text;
		dictionary["EventType"] = name;
		dictionary["Items"] = array;
		GorillaTelemetry.EnqueueTelemetryEventPlayFab(new EventContents
		{
			Name = "telemetry_shop_event",
			EventNamespace = GorillaTelemetry.EVENT_NAMESPACE,
			Payload = dictionary
		});
		GorillaTelemetry.EnqueueTelemetryEvent("telemetry_shop_event", dictionary, null);
	}

	private static void PostShopEvent_OnResult(WriteEventResponse result)
	{
	}

	private static void PostShopEvent_OnError(PlayFabError error)
	{
	}

	public static void PostBuilderKioskEvent(VRRig playerRig, GTShopEventType shopEvent, BuilderSetManager.BuilderSetStoreItem item)
	{
		GorillaTelemetry.gSingleItemBuilderParam[0] = item;
		GorillaTelemetry.PostBuilderKioskEvent(playerRig, shopEvent, GorillaTelemetry.gSingleItemBuilderParam);
		GorillaTelemetry.gSingleItemBuilderParam[0] = default(BuilderSetManager.BuilderSetStoreItem);
	}

	private static string[] BuilderItemsToStrings(IList<BuilderSetManager.BuilderSetStoreItem> items)
	{
		int count = items.Count;
		if (count == 0)
		{
			return Array.Empty<string>();
		}
		HashSet<string> hashSet = new HashSet<string>(count);
		int num = 0;
		for (int i = 0; i < items.Count; i++)
		{
			BuilderSetManager.BuilderSetStoreItem builderSetStoreItem = items[i];
			if (!builderSetStoreItem.isNullItem)
			{
				string playfabID = builderSetStoreItem.playfabID;
				if (!string.IsNullOrWhiteSpace(playfabID) && !playfabID.Contains("NOTHING", StringComparison.InvariantCultureIgnoreCase) && hashSet.Add(playfabID))
				{
					num++;
				}
			}
		}
		string[] array = new string[num];
		hashSet.CopyTo(array);
		return array;
	}

	public static void PostBuilderKioskEvent(VRRig playerRig, GTShopEventType shopEvent, IList<BuilderSetManager.BuilderSetStoreItem> items)
	{
		if (!GorillaTelemetry.IsConnected())
		{
			return;
		}
		if (!playerRig.isLocal)
		{
			return;
		}
		string text = GorillaTelemetry.PlayFabUserId();
		string name = shopEvent.GetName<GTShopEventType>();
		string[] array = GorillaTelemetry.BuilderItemsToStrings(items);
		Dictionary<string, object> dictionary = GorillaTelemetry.gShopEventArgs;
		dictionary["User"] = text;
		dictionary["EventType"] = name;
		dictionary["Items"] = array;
		GorillaTelemetry.EnqueueTelemetryEventPlayFab(new EventContents
		{
			Name = "telemetry_shop_event",
			EventNamespace = GorillaTelemetry.EVENT_NAMESPACE,
			Payload = dictionary
		});
		GorillaTelemetry.EnqueueTelemetryEvent("telemetry_shop_event", dictionary, null);
	}

	public static void PostKidEvent(bool joinGroupsEnabled, bool voiceChatEnabled, bool customUsernamesEnabled, AgeStatusType ageCategory, GTKidEventType kidEvent)
	{
		if ((double)Random.value < 0.1)
		{
			return;
		}
		if (!GorillaTelemetry.IsConnected())
		{
			return;
		}
		string text = GorillaTelemetry.PlayFabUserId();
		string name = kidEvent.GetName<GTKidEventType>();
		string text2 = ((ageCategory == AgeStatusType.LEGALADULT) ? "Not_Managed_Account" : "Managed_Account");
		string text3 = joinGroupsEnabled.ToString().ToUpper();
		string text4 = voiceChatEnabled.ToString().ToUpper();
		string text5 = customUsernamesEnabled.ToString().ToUpper();
		Dictionary<string, object> dictionary = GorillaTelemetry.gKidEventArgs;
		dictionary["User"] = text;
		dictionary["EventType"] = name;
		dictionary["AgeCategory"] = text2;
		dictionary["VoiceChatEnabled"] = text4;
		dictionary["CustomUsernameEnabled"] = text5;
		dictionary["JoinGroups"] = text3;
		GorillaTelemetry.EnqueueTelemetryEventPlayFab(new EventContents
		{
			Name = "telemetry_kid_event",
			EventNamespace = GorillaTelemetry.EVENT_NAMESPACE,
			Payload = dictionary
		});
		GorillaTelemetry.EnqueueTelemetryEvent("telemetry_kid_event", dictionary, null);
	}

	public static void WamGameStart(string playerId, string gameId, string machineId)
	{
		if (!GorillaTelemetry.IsConnected())
		{
			return;
		}
		GorillaTelemetry.gWamGameStartArgs["User"] = playerId;
		GorillaTelemetry.gWamGameStartArgs["WamGameId"] = gameId;
		GorillaTelemetry.gWamGameStartArgs["WamMachineId"] = machineId;
		GorillaTelemetry.EnqueueTelemetryEventPlayFab(new EventContents
		{
			Name = "telemetry_wam_gameStartEvent",
			EventNamespace = GorillaTelemetry.EVENT_NAMESPACE,
			Payload = GorillaTelemetry.gWamGameStartArgs
		});
		GorillaTelemetry.EnqueueTelemetryEvent("telemetry_wam_gameStartEvent", GorillaTelemetry.gWamGameStartArgs, null);
	}

	public static void WamLevelEnd(string playerId, int gameId, string machineId, int currentLevelNumber, int levelGoodMolesShown, int levelHazardMolesShown, int levelMinScore, int currentScore, int levelHazardMolesHit, string currentGameResult)
	{
		if (!GorillaTelemetry.IsConnected())
		{
			return;
		}
		GorillaTelemetry.gWamLevelEndArgs["User"] = playerId;
		GorillaTelemetry.gWamLevelEndArgs["WamGameId"] = gameId.ToString();
		GorillaTelemetry.gWamLevelEndArgs["WamMachineId"] = machineId;
		GorillaTelemetry.gWamLevelEndArgs["WamMLevelNumber"] = currentLevelNumber.ToString();
		GorillaTelemetry.gWamLevelEndArgs["WamGoodMolesShown"] = levelGoodMolesShown.ToString();
		GorillaTelemetry.gWamLevelEndArgs["WamHazardMolesShown"] = levelHazardMolesShown.ToString();
		GorillaTelemetry.gWamLevelEndArgs["WamLevelMinScore"] = levelMinScore.ToString();
		GorillaTelemetry.gWamLevelEndArgs["WamLevelScore"] = currentScore.ToString();
		GorillaTelemetry.gWamLevelEndArgs["WamHazardMolesHit"] = levelHazardMolesHit.ToString();
		GorillaTelemetry.gWamLevelEndArgs["WamGameState"] = currentGameResult;
		GorillaTelemetry.EnqueueTelemetryEventPlayFab(new EventContents
		{
			Name = "telemetry_wam_levelEndEvent",
			EventNamespace = GorillaTelemetry.EVENT_NAMESPACE,
			Payload = GorillaTelemetry.gWamLevelEndArgs
		});
		GorillaTelemetry.EnqueueTelemetryEvent("telemetry_wam_levelEndEvent", GorillaTelemetry.gWamLevelEndArgs, null);
	}

	public static void PostCustomMapPerformance(string mapName, long mapModId, int lowestFPS, int lowestDC, int lowestPC, int avgFPS, int avgDC, int avgPC, int highestFPS, int highestDC, int highestPC, int playtime)
	{
		if (!GorillaTelemetry.IsConnected())
		{
			return;
		}
		Dictionary<string, object> dictionary = GorillaTelemetry.gCustomMapPerfArgs;
		dictionary["CustomMapName"] = mapName;
		dictionary["CustomMapModId"] = mapModId.ToString();
		dictionary["LowestFPS"] = lowestFPS.ToString();
		dictionary["LowestFPSDrawCalls"] = lowestDC.ToString();
		dictionary["LowestFPSPlayerCount"] = lowestPC.ToString();
		dictionary["AverageFPS"] = avgFPS.ToString();
		dictionary["AverageDrawCalls"] = avgDC.ToString();
		dictionary["AveragePlayerCount"] = avgPC.ToString();
		dictionary["HighestFPS"] = highestFPS.ToString();
		dictionary["HighestFPSDrawCalls"] = highestDC.ToString();
		dictionary["HighestFPSPlayerCount"] = highestPC.ToString();
		dictionary["PlaytimeInSeconds"] = playtime.ToString();
		GorillaTelemetry.EnqueueTelemetryEventPlayFab(new EventContents
		{
			Name = "CustomMapPerformance",
			EventNamespace = GorillaTelemetry.EVENT_NAMESPACE,
			Payload = dictionary
		});
		GorillaTelemetry.EnqueueTelemetryEvent("CustomMapPerformance", dictionary, null);
	}

	public static void PostCustomMapTracking(string mapName, long mapModId, string mapCreatorUsername, int minPlayers, int maxPlayers, int playtime, bool privateRoom)
	{
		if (!GorillaTelemetry.IsConnected())
		{
			return;
		}
		int num = playtime % 60;
		int num2 = (playtime - num) / 60;
		int num3 = num2 % 60;
		int num4 = (num2 - num3) / 60;
		string text = string.Format("{0}.{1}.{2}", num4, num3, num);
		Dictionary<string, object> dictionary = GorillaTelemetry.gCustomMapTrackingMetrics;
		dictionary["User"] = GorillaTelemetry.PlayFabUserId();
		dictionary["CustomMapName"] = mapName;
		dictionary["CustomMapModId"] = mapModId.ToString();
		dictionary["CustomMapCreator"] = mapCreatorUsername;
		dictionary["MinPlayerCount"] = minPlayers.ToString();
		dictionary["MaxPlayerCount"] = maxPlayers.ToString();
		dictionary["PlaytimeInSeconds"] = playtime.ToString();
		dictionary["PrivateRoom"] = privateRoom.ToString();
		dictionary["PlaytimeOnMap"] = text;
		GorillaTelemetry.EnqueueTelemetryEventPlayFab(new EventContents
		{
			Name = "CustomMapTracking",
			EventNamespace = GorillaTelemetry.EVENT_NAMESPACE,
			Payload = dictionary
		});
		GorillaTelemetry.EnqueueTelemetryEvent("CustomMapTracking", dictionary, null);
	}

	public static void PostCustomMapDownloadEvent(string mapName, long mapModId, string mapCreatorUsername)
	{
	}

	public static void GhostReactorShiftStart(string gameId, int initialCores, float timeIntoShift, bool wasPlayerInAtStart, int numPlayers, int floorJoined, string playerRank)
	{
		if (!GorillaTelemetry.IsConnected())
		{
			return;
		}
		GorillaTelemetry.gGhostReactorShiftStartArgs["User"] = GorillaTelemetry.PlayFabUserId();
		GorillaTelemetry.gGhostReactorShiftStartArgs["ghost_game_id"] = gameId;
		GorillaTelemetry.gGhostReactorShiftStartArgs["event_timestamp"] = DateTime.Now.ToString();
		GorillaTelemetry.gGhostReactorShiftStartArgs["initial_cores_balance"] = initialCores.ToString();
		GorillaTelemetry.gGhostReactorShiftStartArgs["number_of_players"] = numPlayers.ToString();
		GorillaTelemetry.gGhostReactorShiftStartArgs["start_at_beginning"] = wasPlayerInAtStart.ToString();
		GorillaTelemetry.gGhostReactorShiftStartArgs["seconds_into_shift_at_join"] = timeIntoShift.ToString();
		GorillaTelemetry.gGhostReactorShiftStartArgs["floor_joined"] = floorJoined.ToString();
		GorillaTelemetry.gGhostReactorShiftStartArgs["player_rank"] = playerRank;
		GorillaTelemetry.gGhostReactorShiftStartArgs["is_private_room"] = NetworkSystem.Instance.SessionIsPrivate.ToString();
		GorillaTelemetry.EnqueueTelemetryEventPlayFab(new EventContents
		{
			Name = "ghost_game_start",
			EventNamespace = GorillaTelemetry.EVENT_NAMESPACE,
			Payload = GorillaTelemetry.gGhostReactorShiftStartArgs
		});
		GhostReactorTelemetryData ghostReactorTelemetryData = new GhostReactorTelemetryData
		{
			EventName = "ghost_game_start",
			CustomTags = new string[]
			{
				KIDTelemetry.GameVersionCustomTag,
				KIDTelemetry.GameEnvironment
			},
			BodyData = new Dictionary<string, object>
			{
				{ "ghost_game_id", gameId },
				{
					"event_timestamp",
					DateTime.Now.ToString()
				},
				{
					"initial_cores_balance",
					initialCores.ToString()
				},
				{
					"number_of_players",
					numPlayers.ToString()
				},
				{
					"start_at_beginning",
					wasPlayerInAtStart.ToString()
				},
				{
					"seconds_into_shift_at_join",
					timeIntoShift.ToString()
				},
				{
					"floor_joined",
					floorJoined.ToString()
				},
				{ "player_rank", playerRank },
				{
					"is_private_room",
					NetworkSystem.Instance.SessionIsPrivate.ToString()
				}
			}
		};
		GorillaTelemetry.EnqueueTelemetryEvent(ghostReactorTelemetryData.EventName, ghostReactorTelemetryData.BodyData, ghostReactorTelemetryData.CustomTags);
	}

	public static void GhostReactorGameEnd(string gameId, int finalCores, int totalCoresCollectedByPlayer, int totalCoresCollectedByGroup, int totalCoresSpentByPlayer, int totalCoresSpentByGroup, int gatesUnlocked, int deaths, List<string> itemsPurchased, int shiftCut, bool isShiftActuallyEnding, float timeIntoShiftAtJoin, float playDuration, bool wasPlayerInAtStart, ZoneClearReason zoneClearReason, int maxNumberOfPlayersInShift, int endNumberOfPlayers, Dictionary<string, int> itemTypesHeldThisShift, int revives, int numShiftsPlayed)
	{
		if (!GorillaTelemetry.IsConnectedToPlayfab())
		{
			return;
		}
		GorillaTelemetry.gGhostReactorShiftEndArgs["User"] = GorillaTelemetry.PlayFabUserId();
		GorillaTelemetry.gGhostReactorShiftEndArgs["ghost_game_id"] = gameId;
		GorillaTelemetry.gGhostReactorShiftEndArgs["event_timestamp"] = DateTime.Now.ToString();
		GorillaTelemetry.gGhostReactorShiftEndArgs["final_cores_balance"] = finalCores.ToString();
		GorillaTelemetry.gGhostReactorShiftEndArgs["total_cores_collected_by_player"] = totalCoresCollectedByPlayer.ToString();
		GorillaTelemetry.gGhostReactorShiftEndArgs["total_cores_collected_by_group"] = totalCoresCollectedByGroup.ToString();
		GorillaTelemetry.gGhostReactorShiftEndArgs["total_cores_spent_by_player"] = totalCoresSpentByPlayer.ToString();
		GorillaTelemetry.gGhostReactorShiftEndArgs["total_cores_spent_by_group"] = totalCoresSpentByGroup.ToString();
		GorillaTelemetry.gGhostReactorShiftEndArgs["gates_unlocked"] = gatesUnlocked.ToString();
		GorillaTelemetry.gGhostReactorShiftEndArgs["died"] = deaths.ToString();
		GorillaTelemetry.gGhostReactorShiftEndArgs["items_purchased"] = itemsPurchased.ToJson(true);
		GorillaTelemetry.gGhostReactorShiftEndArgs["shift_cut"] = shiftCut.ToJson(true);
		GorillaTelemetry.gGhostReactorShiftEndArgs["play_duration"] = playDuration.ToString();
		GorillaTelemetry.gGhostReactorShiftEndArgs["started_late"] = (!wasPlayerInAtStart).ToString();
		GorillaTelemetry.gGhostReactorShiftEndArgs["time_started"] = timeIntoShiftAtJoin.ToString();
		GorillaTelemetry.gGhostReactorShiftEndArgs["revives"] = revives.ToString();
		string text = "shift_ended";
		if (!isShiftActuallyEnding)
		{
			if (zoneClearReason == ZoneClearReason.LeaveZone)
			{
				text = "left_zone";
			}
			else
			{
				text = "disconnect";
			}
		}
		GorillaTelemetry.gGhostReactorShiftEndArgs["reason"] = text;
		GorillaTelemetry.gGhostReactorShiftEndArgs["max_number_in_game"] = maxNumberOfPlayersInShift.ToString();
		GorillaTelemetry.gGhostReactorShiftEndArgs["end_number_in_game"] = endNumberOfPlayers.ToString();
		GorillaTelemetry.gGhostReactorShiftEndArgs["items_picked_up"] = itemTypesHeldThisShift.ToJson(true);
		GorillaTelemetry.gGhostReactorShiftEndArgs["num_shifts_played"] = numShiftsPlayed.ToString();
		GorillaTelemetry.EnqueueTelemetryEventPlayFab(new EventContents
		{
			Name = "ghost_game_end",
			EventNamespace = GorillaTelemetry.EVENT_NAMESPACE,
			Payload = GorillaTelemetry.gGhostReactorShiftEndArgs
		});
		GhostReactorTelemetryData ghostReactorTelemetryData = new GhostReactorTelemetryData
		{
			EventName = "ghost_game_end",
			CustomTags = new string[]
			{
				KIDTelemetry.GameVersionCustomTag,
				KIDTelemetry.GameEnvironment
			},
			BodyData = new Dictionary<string, object>
			{
				{ "ghost_game_id", gameId },
				{
					"event_timestamp",
					DateTime.Now.ToString()
				},
				{
					"final_cores_balance",
					finalCores.ToString()
				},
				{
					"total_cores_collected_by_player",
					totalCoresCollectedByPlayer.ToString()
				},
				{
					"total_cores_collected_by_group",
					totalCoresCollectedByGroup.ToString()
				},
				{
					"total_cores_spent_by_player",
					totalCoresSpentByPlayer.ToString()
				},
				{
					"total_cores_spent_by_group",
					totalCoresSpentByGroup.ToString()
				},
				{
					"gates_unlocked",
					gatesUnlocked.ToString()
				},
				{
					"died",
					deaths.ToString()
				},
				{
					"items_purchased",
					itemsPurchased.ToJson(true)
				},
				{
					"shift_cut_data",
					shiftCut.ToJson(true)
				},
				{
					"play_duration",
					playDuration.ToString()
				},
				{
					"started_late",
					(!wasPlayerInAtStart).ToString()
				},
				{
					"time_started",
					timeIntoShiftAtJoin.ToString()
				},
				{ "reason", text },
				{
					"max_number_in_game",
					maxNumberOfPlayersInShift.ToString()
				},
				{
					"end_number_in_game",
					endNumberOfPlayers.ToString()
				},
				{
					"items_picked_up",
					itemTypesHeldThisShift.ToJson(true)
				},
				{
					"revives",
					revives.ToString()
				},
				{
					"num_shifts_played",
					numShiftsPlayed.ToString()
				}
			}
		};
		GorillaTelemetry.EnqueueTelemetryEvent(ghostReactorTelemetryData.EventName, ghostReactorTelemetryData.BodyData, ghostReactorTelemetryData.CustomTags);
	}

	public static void GhostReactorFloorStart(string gameId, int initialCores, float timeIntoShift, bool wasPlayerInAtStart, int numPlayers, string playerRank, int floor, string preset, string modifier)
	{
		if (!GorillaTelemetry.IsConnected())
		{
			return;
		}
		GorillaTelemetry.gGhostReactorFloorStartArgs["User"] = GorillaTelemetry.PlayFabUserId();
		GorillaTelemetry.gGhostReactorFloorStartArgs["ghost_game_id"] = gameId;
		GorillaTelemetry.gGhostReactorFloorStartArgs["event_timestamp"] = DateTime.Now.ToString();
		GorillaTelemetry.gGhostReactorFloorStartArgs["initial_cores_balance"] = initialCores.ToString();
		GorillaTelemetry.gGhostReactorFloorStartArgs["number_of_players"] = numPlayers.ToString();
		GorillaTelemetry.gGhostReactorFloorStartArgs["start_at_beginning"] = wasPlayerInAtStart.ToString();
		GorillaTelemetry.gGhostReactorFloorStartArgs["seconds_into_shift_at_join"] = timeIntoShift.ToString();
		GorillaTelemetry.gGhostReactorFloorStartArgs["player_rank"] = playerRank;
		GorillaTelemetry.gGhostReactorFloorStartArgs["floor"] = floor.ToString();
		GorillaTelemetry.gGhostReactorFloorStartArgs["preset"] = preset.ToString();
		GorillaTelemetry.gGhostReactorFloorStartArgs["modifier"] = modifier.ToString();
		GorillaTelemetry.gGhostReactorFloorStartArgs["is_private_room"] = NetworkSystem.Instance.SessionIsPrivate.ToString();
		GorillaTelemetry.EnqueueTelemetryEventPlayFab(new EventContents
		{
			Name = "ghost_floor_start",
			EventNamespace = GorillaTelemetry.EVENT_NAMESPACE,
			Payload = GorillaTelemetry.gGhostReactorFloorStartArgs
		});
		GhostReactorTelemetryData ghostReactorTelemetryData = new GhostReactorTelemetryData
		{
			EventName = "ghost_floor_start",
			CustomTags = new string[]
			{
				KIDTelemetry.GameVersionCustomTag,
				KIDTelemetry.GameEnvironment
			},
			BodyData = new Dictionary<string, object>
			{
				{ "ghost_game_id", gameId },
				{
					"event_timestamp",
					DateTime.Now.ToString()
				},
				{
					"initial_cores_balance",
					initialCores.ToString()
				},
				{
					"number_of_players",
					numPlayers.ToString()
				},
				{
					"start_at_beginning",
					wasPlayerInAtStart.ToString()
				},
				{
					"seconds_into_shift_at_join",
					timeIntoShift.ToString()
				},
				{ "player_rank", playerRank },
				{
					"floor",
					floor.ToString()
				},
				{
					"preset",
					preset.ToString()
				},
				{
					"modifier",
					modifier.ToString()
				},
				{
					"is_private_room",
					NetworkSystem.Instance.SessionIsPrivate.ToString()
				}
			}
		};
		GorillaTelemetry.EnqueueTelemetryEvent(ghostReactorTelemetryData.EventName, ghostReactorTelemetryData.BodyData, ghostReactorTelemetryData.CustomTags);
	}

	public static void GhostReactorFloorComplete(string gameId, int finalCores, int totalCoresCollectedByPlayer, int totalCoresCollectedByGroup, int totalCoresSpentByPlayer, int totalCoresSpentByGroup, int gatesUnlocked, int deaths, List<string> itemsPurchased, int shiftCut, bool isShiftActuallyEnding, float timeIntoShiftAtJoin, float playDuration, bool wasPlayerInAtStart, ZoneClearReason zoneClearReason, int maxNumberOfPlayersInShift, int endNumberOfPlayers, Dictionary<string, int> itemTypesHeldThisShift, int revives, int floor, string preset, string modifier, int chaosSeedsCollected, bool objectivesCompleted, string section, int xpGained)
	{
		if (!GorillaTelemetry.IsConnectedToPlayfab())
		{
			return;
		}
		GorillaTelemetry.gGhostReactorFloorEndArgs["User"] = GorillaTelemetry.PlayFabUserId();
		GorillaTelemetry.gGhostReactorFloorEndArgs["ghost_game_id"] = gameId;
		GorillaTelemetry.gGhostReactorFloorEndArgs["event_timestamp"] = DateTime.Now.ToString();
		GorillaTelemetry.gGhostReactorFloorEndArgs["final_cores_balance"] = finalCores.ToString();
		GorillaTelemetry.gGhostReactorFloorEndArgs["total_cores_collected_by_player"] = totalCoresCollectedByPlayer.ToString();
		GorillaTelemetry.gGhostReactorFloorEndArgs["total_cores_collected_by_group"] = totalCoresCollectedByGroup.ToString();
		GorillaTelemetry.gGhostReactorFloorEndArgs["total_cores_spent_by_player"] = totalCoresSpentByPlayer.ToString();
		GorillaTelemetry.gGhostReactorFloorEndArgs["total_cores_spent_by_group"] = totalCoresSpentByGroup.ToString();
		GorillaTelemetry.gGhostReactorFloorEndArgs["gates_unlocked"] = gatesUnlocked.ToString();
		GorillaTelemetry.gGhostReactorFloorEndArgs["died"] = deaths.ToString();
		GorillaTelemetry.gGhostReactorFloorEndArgs["items_purchased"] = itemsPurchased.ToJson(true);
		GorillaTelemetry.gGhostReactorFloorEndArgs["shift_cut"] = shiftCut.ToJson(true);
		GorillaTelemetry.gGhostReactorFloorEndArgs["play_duration"] = playDuration.ToString();
		GorillaTelemetry.gGhostReactorFloorEndArgs["started_late"] = (!wasPlayerInAtStart).ToString();
		GorillaTelemetry.gGhostReactorFloorEndArgs["time_started"] = timeIntoShiftAtJoin.ToString();
		GorillaTelemetry.gGhostReactorFloorEndArgs["revives"] = revives.ToString();
		string text = "shift_ended";
		if (!isShiftActuallyEnding)
		{
			if (zoneClearReason == ZoneClearReason.LeaveZone)
			{
				text = "left_zone";
			}
			else
			{
				text = "disconnect";
			}
		}
		GorillaTelemetry.gGhostReactorFloorEndArgs["reason"] = text;
		GorillaTelemetry.gGhostReactorFloorEndArgs["max_number_in_game"] = maxNumberOfPlayersInShift.ToString();
		GorillaTelemetry.gGhostReactorFloorEndArgs["end_number_in_game"] = endNumberOfPlayers.ToString();
		GorillaTelemetry.gGhostReactorFloorEndArgs["items_picked_up"] = itemTypesHeldThisShift.ToJson(true);
		GorillaTelemetry.gGhostReactorFloorEndArgs["floor"] = floor.ToString();
		GorillaTelemetry.gGhostReactorFloorEndArgs["preset"] = preset;
		GorillaTelemetry.gGhostReactorFloorEndArgs["modifier"] = modifier;
		GorillaTelemetry.gGhostReactorFloorEndArgs["section"] = section;
		GorillaTelemetry.gGhostReactorFloorEndArgs["xp_gained"] = xpGained.ToString();
		GorillaTelemetry.gGhostReactorFloorEndArgs["chaos_seeds_collected"] = chaosSeedsCollected.ToString();
		GorillaTelemetry.gGhostReactorFloorEndArgs["objectives_completed"] = objectivesCompleted.ToString();
		GorillaTelemetry.EnqueueTelemetryEventPlayFab(new EventContents
		{
			Name = "ghost_floor_end",
			EventNamespace = GorillaTelemetry.EVENT_NAMESPACE,
			Payload = GorillaTelemetry.gGhostReactorFloorEndArgs
		});
		GhostReactorTelemetryData ghostReactorTelemetryData = new GhostReactorTelemetryData
		{
			EventName = "ghost_floor_end",
			CustomTags = new string[]
			{
				KIDTelemetry.GameVersionCustomTag,
				KIDTelemetry.GameEnvironment
			},
			BodyData = new Dictionary<string, object>
			{
				{ "ghost_game_id", gameId },
				{
					"event_timestamp",
					DateTime.Now.ToString()
				},
				{
					"final_cores_balance",
					finalCores.ToString()
				},
				{
					"total_cores_collected_by_player",
					totalCoresCollectedByPlayer.ToString()
				},
				{
					"total_cores_collected_by_group",
					totalCoresCollectedByGroup.ToString()
				},
				{
					"total_cores_spent_by_player",
					totalCoresSpentByPlayer.ToString()
				},
				{
					"total_cores_spent_by_group",
					totalCoresSpentByGroup.ToString()
				},
				{
					"gates_unlocked",
					gatesUnlocked.ToString()
				},
				{
					"died",
					deaths.ToString()
				},
				{
					"items_purchased",
					itemsPurchased.ToJson(true)
				},
				{
					"shift_cut_data",
					shiftCut.ToJson(true)
				},
				{
					"play_duration",
					playDuration.ToString()
				},
				{
					"started_late",
					(!wasPlayerInAtStart).ToString()
				},
				{
					"time_started",
					timeIntoShiftAtJoin.ToString()
				},
				{ "reason", text },
				{
					"max_number_in_game",
					maxNumberOfPlayersInShift.ToString()
				},
				{
					"end_number_in_game",
					endNumberOfPlayers.ToString()
				},
				{
					"items_picked_up",
					itemTypesHeldThisShift.ToJson(true)
				},
				{
					"revives",
					revives.ToString()
				},
				{
					"floor",
					floor.ToString()
				},
				{ "preset", preset },
				{ "modifier", modifier },
				{
					"chaos_seeds_collected",
					chaosSeedsCollected.ToString()
				},
				{
					"objectives_completed",
					objectivesCompleted.ToString()
				},
				{ "section", section },
				{
					"xp_gained",
					xpGained.ToString()
				}
			}
		};
		GorillaTelemetry.EnqueueTelemetryEvent(ghostReactorTelemetryData.EventName, ghostReactorTelemetryData.BodyData, ghostReactorTelemetryData.CustomTags);
	}

	public static void GhostReactorToolPurchased(string gameId, string toolName, int toolLevel, int coresSpent, int shinyRocksSpent, int floor, string preset)
	{
		if (!GorillaTelemetry.IsConnected())
		{
			return;
		}
		GorillaTelemetry.gGhostReactorToolPurchasedArgs["User"] = GorillaTelemetry.PlayFabUserId();
		GorillaTelemetry.gGhostReactorToolPurchasedArgs["ghost_game_id"] = gameId;
		GorillaTelemetry.gGhostReactorToolPurchasedArgs["event_timestamp"] = DateTime.Now.ToString();
		GorillaTelemetry.gGhostReactorToolPurchasedArgs["tool"] = toolName;
		GorillaTelemetry.gGhostReactorToolPurchasedArgs["tool_level"] = toolLevel.ToString();
		GorillaTelemetry.gGhostReactorToolPurchasedArgs["cores_spent"] = coresSpent.ToString();
		GorillaTelemetry.gGhostReactorToolPurchasedArgs["shiny_rocks_spent"] = shinyRocksSpent.ToString();
		GorillaTelemetry.gGhostReactorToolPurchasedArgs["floor"] = floor.ToString();
		GorillaTelemetry.gGhostReactorToolPurchasedArgs["preset"] = preset;
		GorillaTelemetry.EnqueueTelemetryEventPlayFab(new EventContents
		{
			Name = "ghost_tool_purchased",
			EventNamespace = GorillaTelemetry.EVENT_NAMESPACE,
			Payload = GorillaTelemetry.gGhostReactorToolPurchasedArgs
		});
		GhostReactorTelemetryData ghostReactorTelemetryData = new GhostReactorTelemetryData
		{
			EventName = "ghost_tool_purchased",
			CustomTags = new string[]
			{
				KIDTelemetry.GameVersionCustomTag,
				KIDTelemetry.GameEnvironment
			},
			BodyData = new Dictionary<string, object>
			{
				{ "ghost_game_id", gameId },
				{
					"event_timestamp",
					DateTime.Now.ToString()
				},
				{ "tool", toolName },
				{
					"tool_level",
					toolLevel.ToString()
				},
				{
					"cores_spent",
					coresSpent.ToString()
				},
				{
					"shiny_rocks_spent",
					shinyRocksSpent.ToString()
				},
				{
					"floor",
					floor.ToString()
				},
				{ "preset", preset }
			}
		};
		GorillaTelemetry.EnqueueTelemetryEvent(ghostReactorTelemetryData.EventName, ghostReactorTelemetryData.BodyData, ghostReactorTelemetryData.CustomTags);
	}

	public static void GhostReactorRankUp(string gameId, string newRank, int floor, string preset)
	{
		if (!GorillaTelemetry.IsConnected())
		{
			return;
		}
		GorillaTelemetry.gGhostReactorRankUpArgs["User"] = GorillaTelemetry.PlayFabUserId();
		GorillaTelemetry.gGhostReactorRankUpArgs["ghost_game_id"] = gameId;
		GorillaTelemetry.gGhostReactorRankUpArgs["event_timestamp"] = DateTime.Now.ToString();
		GorillaTelemetry.gGhostReactorRankUpArgs["new_rank"] = newRank;
		GorillaTelemetry.gGhostReactorRankUpArgs["floor"] = floor.ToString();
		GorillaTelemetry.gGhostReactorRankUpArgs["preset"] = preset;
		GorillaTelemetry.EnqueueTelemetryEventPlayFab(new EventContents
		{
			Name = "ghost_game_rank_up",
			EventNamespace = GorillaTelemetry.EVENT_NAMESPACE,
			Payload = GorillaTelemetry.gGhostReactorRankUpArgs
		});
		GhostReactorTelemetryData ghostReactorTelemetryData = new GhostReactorTelemetryData
		{
			EventName = "ghost_game_rank_up",
			CustomTags = new string[]
			{
				KIDTelemetry.GameVersionCustomTag,
				KIDTelemetry.GameEnvironment
			},
			BodyData = new Dictionary<string, object>
			{
				{ "ghost_game_id", gameId },
				{
					"event_timestamp",
					DateTime.Now.ToString()
				},
				{ "new_rank", newRank },
				{
					"floor",
					floor.ToString()
				},
				{ "preset", preset }
			}
		};
		GorillaTelemetry.EnqueueTelemetryEvent(ghostReactorTelemetryData.EventName, ghostReactorTelemetryData.BodyData, ghostReactorTelemetryData.CustomTags);
	}

	public static void GhostReactorToolUnlock(string gameId, string toolName)
	{
		if (!GorillaTelemetry.IsConnected())
		{
			return;
		}
		GorillaTelemetry.gGhostReactorToolUnlockArgs["User"] = GorillaTelemetry.PlayFabUserId();
		GorillaTelemetry.gGhostReactorToolUnlockArgs["ghost_game_id"] = gameId;
		GorillaTelemetry.gGhostReactorToolUnlockArgs["event_timestamp"] = DateTime.Now.ToString();
		GorillaTelemetry.gGhostReactorToolUnlockArgs["tool"] = toolName;
		GorillaTelemetry.EnqueueTelemetryEventPlayFab(new EventContents
		{
			Name = "ghost_game_tool_unlock",
			EventNamespace = GorillaTelemetry.EVENT_NAMESPACE,
			Payload = GorillaTelemetry.gGhostReactorToolUnlockArgs
		});
		GhostReactorTelemetryData ghostReactorTelemetryData = new GhostReactorTelemetryData
		{
			EventName = "ghost_game_tool_unlock",
			CustomTags = new string[]
			{
				KIDTelemetry.GameVersionCustomTag,
				KIDTelemetry.GameEnvironment
			},
			BodyData = new Dictionary<string, object>
			{
				{ "ghost_game_id", gameId },
				{
					"event_timestamp",
					DateTime.Now.ToString()
				},
				{ "tool", toolName }
			}
		};
		GorillaTelemetry.EnqueueTelemetryEvent(ghostReactorTelemetryData.EventName, ghostReactorTelemetryData.BodyData, ghostReactorTelemetryData.CustomTags);
	}

	public static void GhostReactorPodUpgradePurchased(string gameId, string toolName, int level, int shinyRocksSpent, int juiceSpent)
	{
		if (!GorillaTelemetry.IsConnected())
		{
			return;
		}
		GorillaTelemetry.gGhostReactorPodUpgradePurchasedArgs["User"] = GorillaTelemetry.PlayFabUserId();
		GorillaTelemetry.gGhostReactorPodUpgradePurchasedArgs["ghost_game_id"] = gameId;
		GorillaTelemetry.gGhostReactorPodUpgradePurchasedArgs["event_timestamp"] = DateTime.Now.ToString();
		GorillaTelemetry.gGhostReactorPodUpgradePurchasedArgs["tool"] = toolName;
		GorillaTelemetry.gGhostReactorPodUpgradePurchasedArgs["new_level"] = level.ToString();
		GorillaTelemetry.gGhostReactorPodUpgradePurchasedArgs["shiny_rocks_spent"] = shinyRocksSpent.ToString();
		GorillaTelemetry.gGhostReactorPodUpgradePurchasedArgs["juice_spent"] = juiceSpent.ToString();
		GorillaTelemetry.EnqueueTelemetryEventPlayFab(new EventContents
		{
			Name = "ghost_pod_upgrade_purchased",
			EventNamespace = GorillaTelemetry.EVENT_NAMESPACE,
			Payload = GorillaTelemetry.gGhostReactorPodUpgradePurchasedArgs
		});
		GhostReactorTelemetryData ghostReactorTelemetryData = new GhostReactorTelemetryData
		{
			EventName = "ghost_pod_upgrade_purchased",
			CustomTags = new string[]
			{
				KIDTelemetry.GameVersionCustomTag,
				KIDTelemetry.GameEnvironment
			},
			BodyData = new Dictionary<string, object>
			{
				{ "ghost_game_id", gameId },
				{
					"event_timestamp",
					DateTime.Now.ToString()
				},
				{ "tool", toolName },
				{
					"new_level",
					level.ToString()
				},
				{
					"shiny_rocks_spent",
					shinyRocksSpent.ToString()
				},
				{
					"juice_spent",
					juiceSpent.ToString()
				}
			}
		};
		GorillaTelemetry.EnqueueTelemetryEvent(ghostReactorTelemetryData.EventName, ghostReactorTelemetryData.BodyData, ghostReactorTelemetryData.CustomTags);
	}

	public static void GhostReactorToolUpgrade(string gameId, string upgradeType, string toolName, int newLevel, int juiceSpent, int griftSpent, int coresSpent, int floor, string preset)
	{
		if (!GorillaTelemetry.IsConnected())
		{
			return;
		}
		GorillaTelemetry.gGhostReactorToolUpgradeArgs["User"] = GorillaTelemetry.PlayFabUserId();
		GorillaTelemetry.gGhostReactorToolUpgradeArgs["ghost_game_id"] = gameId;
		GorillaTelemetry.gGhostReactorToolUpgradeArgs["event_timestamp"] = DateTime.Now.ToString();
		GorillaTelemetry.gGhostReactorToolUpgradeArgs["type"] = upgradeType;
		GorillaTelemetry.gGhostReactorToolUpgradeArgs["tool"] = toolName;
		GorillaTelemetry.gGhostReactorToolUpgradeArgs["new_level"] = newLevel.ToString();
		GorillaTelemetry.gGhostReactorToolUpgradeArgs["juice_spent"] = juiceSpent.ToString();
		GorillaTelemetry.gGhostReactorToolUpgradeArgs["grift_spent"] = griftSpent.ToString();
		GorillaTelemetry.gGhostReactorToolUpgradeArgs["cores_spent"] = coresSpent.ToString();
		GorillaTelemetry.gGhostReactorToolUpgradeArgs["floor"] = floor.ToString();
		GorillaTelemetry.gGhostReactorToolUpgradeArgs["preset"] = preset;
		GorillaTelemetry.EnqueueTelemetryEventPlayFab(new EventContents
		{
			Name = "ghost_game_tool_upgrade",
			EventNamespace = GorillaTelemetry.EVENT_NAMESPACE,
			Payload = GorillaTelemetry.gGhostReactorToolUpgradeArgs
		});
		GhostReactorTelemetryData ghostReactorTelemetryData = new GhostReactorTelemetryData
		{
			EventName = "ghost_game_tool_upgrade",
			CustomTags = new string[]
			{
				KIDTelemetry.GameVersionCustomTag,
				KIDTelemetry.GameEnvironment
			},
			BodyData = new Dictionary<string, object>
			{
				{ "ghost_game_id", gameId },
				{
					"event_timestamp",
					DateTime.Now.ToString()
				},
				{ "type", upgradeType },
				{ "tool", toolName },
				{
					"new_level",
					newLevel.ToString()
				},
				{
					"juice_spent",
					juiceSpent.ToString()
				},
				{
					"grift_spent",
					griftSpent.ToString()
				},
				{
					"cores_spent",
					coresSpent.ToString()
				},
				{
					"floor",
					floor.ToString()
				},
				{ "preset", preset }
			}
		};
		GorillaTelemetry.EnqueueTelemetryEvent(ghostReactorTelemetryData.EventName, ghostReactorTelemetryData.BodyData, ghostReactorTelemetryData.CustomTags);
	}

	public static void GhostReactorChaosSeedStart(string gameId, string unlockTime, int chaosSeedsInQueue, int floor, string preset)
	{
		if (!GorillaTelemetry.IsConnected())
		{
			return;
		}
		GorillaTelemetry.gGhostReactorChaosSeedStartArgs["User"] = GorillaTelemetry.PlayFabUserId();
		GorillaTelemetry.gGhostReactorChaosSeedStartArgs["ghost_game_id"] = gameId;
		GorillaTelemetry.gGhostReactorChaosSeedStartArgs["event_timestamp"] = DateTime.Now.ToString();
		GorillaTelemetry.gGhostReactorChaosSeedStartArgs["unlock_time"] = unlockTime;
		GorillaTelemetry.gGhostReactorChaosSeedStartArgs["chaos_seeds_in_queue"] = chaosSeedsInQueue.ToString();
		GorillaTelemetry.gGhostReactorChaosSeedStartArgs["floor"] = floor.ToString();
		GorillaTelemetry.gGhostReactorChaosSeedStartArgs["preset"] = preset;
		GorillaTelemetry.EnqueueTelemetryEventPlayFab(new EventContents
		{
			Name = "ghost_chaos_seed_start",
			EventNamespace = GorillaTelemetry.EVENT_NAMESPACE,
			Payload = GorillaTelemetry.gGhostReactorChaosSeedStartArgs
		});
		GhostReactorTelemetryData ghostReactorTelemetryData = new GhostReactorTelemetryData
		{
			EventName = "ghost_chaos_seed_start",
			CustomTags = new string[]
			{
				KIDTelemetry.GameVersionCustomTag,
				KIDTelemetry.GameEnvironment
			},
			BodyData = new Dictionary<string, object>
			{
				{ "ghost_game_id", gameId },
				{
					"event_timestamp",
					DateTime.Now.ToString()
				},
				{ "unlock_time", unlockTime },
				{
					"chaos_seeds_in_queue",
					chaosSeedsInQueue.ToString()
				},
				{
					"floor",
					floor.ToString()
				},
				{ "preset", preset }
			}
		};
		GorillaTelemetry.EnqueueTelemetryEvent(ghostReactorTelemetryData.EventName, ghostReactorTelemetryData.BodyData, ghostReactorTelemetryData.CustomTags);
	}

	public static void GhostReactorChaosJuiceCollected(string gameId, int juiceCollected, int coresProcessedByOverdrive)
	{
		if (!GorillaTelemetry.IsConnected())
		{
			return;
		}
		GorillaTelemetry.gGhostReactorChaosJuiceCollectedArgs["User"] = GorillaTelemetry.PlayFabUserId();
		GorillaTelemetry.gGhostReactorChaosJuiceCollectedArgs["ghost_game_id"] = gameId;
		GorillaTelemetry.gGhostReactorChaosJuiceCollectedArgs["event_timestamp"] = DateTime.Now.ToString();
		GorillaTelemetry.gGhostReactorChaosJuiceCollectedArgs["juice_collected"] = juiceCollected.ToString();
		GorillaTelemetry.gGhostReactorChaosJuiceCollectedArgs["cores_processed_by_overdrive"] = coresProcessedByOverdrive.ToString();
		GorillaTelemetry.EnqueueTelemetryEventPlayFab(new EventContents
		{
			Name = "ghost_chaos_juice_collected",
			EventNamespace = GorillaTelemetry.EVENT_NAMESPACE,
			Payload = GorillaTelemetry.gGhostReactorChaosJuiceCollectedArgs
		});
		GhostReactorTelemetryData ghostReactorTelemetryData = new GhostReactorTelemetryData
		{
			EventName = "ghost_chaos_juice_collected",
			CustomTags = new string[]
			{
				KIDTelemetry.GameVersionCustomTag,
				KIDTelemetry.GameEnvironment
			},
			BodyData = new Dictionary<string, object>
			{
				{ "ghost_game_id", gameId },
				{
					"event_timestamp",
					DateTime.Now.ToString()
				},
				{
					"juice_collected",
					juiceCollected.ToString()
				},
				{
					"cores_processed_by_overdrive",
					coresProcessedByOverdrive.ToString()
				}
			}
		};
		GorillaTelemetry.EnqueueTelemetryEvent(ghostReactorTelemetryData.EventName, ghostReactorTelemetryData.BodyData, ghostReactorTelemetryData.CustomTags);
	}

	public static void GhostReactorOverdrivePurchased(string gameId, int shinyRocksUsed, int chaosSeedsInQueue, int floor, string preset)
	{
		if (!GorillaTelemetry.IsConnected())
		{
			return;
		}
		GorillaTelemetry.gGhostReactorOverdrivePurchasedArgs["User"] = GorillaTelemetry.PlayFabUserId();
		GorillaTelemetry.gGhostReactorOverdrivePurchasedArgs["ghost_game_id"] = gameId;
		GorillaTelemetry.gGhostReactorOverdrivePurchasedArgs["event_timestamp"] = DateTime.Now.ToString();
		GorillaTelemetry.gGhostReactorOverdrivePurchasedArgs["shiny_rocks_used"] = shinyRocksUsed.ToString();
		GorillaTelemetry.gGhostReactorOverdrivePurchasedArgs["chaos_seeds_in_queue"] = chaosSeedsInQueue.ToString();
		GorillaTelemetry.gGhostReactorOverdrivePurchasedArgs["floor"] = floor.ToString();
		GorillaTelemetry.gGhostReactorOverdrivePurchasedArgs["preset"] = preset;
		GorillaTelemetry.EnqueueTelemetryEventPlayFab(new EventContents
		{
			Name = "ghost_overdrive_purchased",
			EventNamespace = GorillaTelemetry.EVENT_NAMESPACE,
			Payload = GorillaTelemetry.gGhostReactorOverdrivePurchasedArgs
		});
		GhostReactorTelemetryData ghostReactorTelemetryData = new GhostReactorTelemetryData
		{
			EventName = "ghost_overdrive_purchased",
			CustomTags = new string[]
			{
				KIDTelemetry.GameVersionCustomTag,
				KIDTelemetry.GameEnvironment
			},
			BodyData = new Dictionary<string, object>
			{
				{ "ghost_game_id", gameId },
				{
					"event_timestamp",
					DateTime.Now.ToString()
				},
				{
					"shiny_rocks_used",
					shinyRocksUsed.ToString()
				},
				{
					"chaos_seeds_in_queue",
					chaosSeedsInQueue.ToString()
				},
				{
					"floor",
					floor.ToString()
				},
				{ "preset", preset }
			}
		};
		GorillaTelemetry.EnqueueTelemetryEvent(ghostReactorTelemetryData.EventName, ghostReactorTelemetryData.BodyData, ghostReactorTelemetryData.CustomTags);
	}

	public static void GhostReactorCreditsRefillPurchased(string gameId, int shinyRocksSpent, int finalCredits, int floor, string preset)
	{
		if (!GorillaTelemetry.IsConnected())
		{
			return;
		}
		GorillaTelemetry.gGhostReactorCreditsRefillPurchasedArgs["User"] = GorillaTelemetry.PlayFabUserId();
		GorillaTelemetry.gGhostReactorCreditsRefillPurchasedArgs["ghost_game_id"] = gameId;
		GorillaTelemetry.gGhostReactorCreditsRefillPurchasedArgs["event_timestamp"] = DateTime.Now.ToString();
		GorillaTelemetry.gGhostReactorCreditsRefillPurchasedArgs["shiny_rocks_spent"] = shinyRocksSpent.ToString();
		GorillaTelemetry.gGhostReactorCreditsRefillPurchasedArgs["final_credits"] = finalCredits.ToString();
		GorillaTelemetry.gGhostReactorCreditsRefillPurchasedArgs["floor"] = floor.ToString();
		GorillaTelemetry.gGhostReactorCreditsRefillPurchasedArgs["preset"] = preset;
		GorillaTelemetry.EnqueueTelemetryEventPlayFab(new EventContents
		{
			Name = "ghost_credits_refill_purchased",
			EventNamespace = GorillaTelemetry.EVENT_NAMESPACE,
			Payload = GorillaTelemetry.gGhostReactorCreditsRefillPurchasedArgs
		});
		GhostReactorTelemetryData ghostReactorTelemetryData = new GhostReactorTelemetryData
		{
			EventName = "ghost_credits_refill_purchased",
			CustomTags = new string[]
			{
				KIDTelemetry.GameVersionCustomTag,
				KIDTelemetry.GameEnvironment
			},
			BodyData = new Dictionary<string, object>
			{
				{ "ghost_game_id", gameId },
				{
					"event_timestamp",
					DateTime.Now.ToString()
				},
				{
					"shiny_rocks_spent",
					shinyRocksSpent.ToString()
				},
				{
					"final_credits",
					finalCredits.ToString()
				},
				{
					"floor",
					floor.ToString()
				},
				{ "preset", preset }
			}
		};
		GorillaTelemetry.EnqueueTelemetryEvent(ghostReactorTelemetryData.EventName, ghostReactorTelemetryData.BodyData, ghostReactorTelemetryData.CustomTags);
	}

	public static void SuperInfectionEvent(bool roomDisconnect, float totalPlayTime, float roomPlayTime, float sessionPlayTime, float intervalPlayTime, float terminalTotalTime, float terminalIntervalTime, Dictionary<SITechTreePageId, float> timeUsingGadgetsTotal, Dictionary<SITechTreePageId, float> timeUsingGadgetsInterval, float timeUsingOwnGadgetsTotal, float timeUsingOwnGadgetsInterval, float timeUsingOthersGadgetsTotal, float timeUsingOthersGadgetsInterval, Dictionary<SITechTreePageId, int> tagsUsingGadgetsTotal, Dictionary<SITechTreePageId, int> tagsUsingGadgetsInterval, int tagsHoldingOwnGadgetsTotal, int tagsHoldingOwnGadgetsInterval, int tagsHoldingOthersGadgetsTotal, int tagsHoldingOthersGadgetsInterval, Dictionary<SIResource.ResourceType, int> resourcesGatheredTotal, Dictionary<SIResource.ResourceType, int> resourcesGatheredInterval, int roundsPlayedTotal, int roundsPlayedInterval, bool[][] unlockedNodes, int numberOfPlayers)
	{
		if (!GorillaTelemetry.IsConnectedIgnoreRoom())
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < unlockedNodes.Length; i++)
		{
			num += unlockedNodes[i].Length;
		}
		char[] array = new char[num];
		num = 0;
		for (int j = 0; j < unlockedNodes.Length; j++)
		{
			for (int k = 0; k < unlockedNodes[j].Length; k++)
			{
				array[num] = (unlockedNodes[j][k] ? '1' : '0');
				num++;
			}
		}
		GorillaTelemetry.gSuperInfectionArgs["User"] = GorillaTelemetry.PlayFabUserId();
		GorillaTelemetry.gSuperInfectionArgs["event_timestamp"] = DateTime.Now.ToString();
		GorillaTelemetry.gSuperInfectionArgs["total_play_time"] = totalPlayTime.ToString();
		GorillaTelemetry.gSuperInfectionArgs["room_play_time"] = roomPlayTime.ToString();
		GorillaTelemetry.gSuperInfectionArgs["session_play_time"] = sessionPlayTime.ToString();
		GorillaTelemetry.gSuperInfectionArgs["interval_play_time"] = intervalPlayTime.ToString();
		GorillaTelemetry.gSuperInfectionArgs["terminal_total_time"] = terminalTotalTime.ToString();
		GorillaTelemetry.gSuperInfectionArgs["terminal_interval_time"] = terminalIntervalTime.ToString();
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
		Dictionary<string, object> dictionary3 = new Dictionary<string, object>();
		Dictionary<string, object> dictionary4 = new Dictionary<string, object>();
		for (int l = 0; l < 6; l++)
		{
			SITechTreePageId sitechTreePageId = (SITechTreePageId)l;
			float num2;
			timeUsingGadgetsTotal.TryGetValue(sitechTreePageId, out num2);
			float num3;
			timeUsingGadgetsInterval.TryGetValue(sitechTreePageId, out num3);
			int num4;
			tagsUsingGadgetsTotal.TryGetValue(sitechTreePageId, out num4);
			int num5;
			tagsUsingGadgetsInterval.TryGetValue(sitechTreePageId, out num5);
			string text = sitechTreePageId.ToString();
			dictionary[text] = num2.ToString();
			dictionary2[text] = num3.ToString();
			dictionary3[text] = num4.ToString();
			dictionary4[text] = num5.ToString();
		}
		Dictionary<string, object> dictionary5 = new Dictionary<string, object>();
		Dictionary<string, object> dictionary6 = new Dictionary<string, object>();
		for (int m = 0; m < 6; m++)
		{
			SIResource.ResourceType resourceType = (SIResource.ResourceType)m;
			int num6;
			resourcesGatheredTotal.TryGetValue(resourceType, out num6);
			int num7;
			resourcesGatheredInterval.TryGetValue(resourceType, out num7);
			string text2 = resourceType.ToString();
			dictionary5[text2] = num6.ToString();
			dictionary6[text2] = num7.ToString();
		}
		GorillaTelemetry.gSuperInfectionArgs["time_holding_gadget_type_total"] = dictionary;
		GorillaTelemetry.gSuperInfectionArgs["time_holding_gadget_type_interval"] = dictionary2;
		GorillaTelemetry.gSuperInfectionArgs["time_holding_own_gadgets_total"] = timeUsingOwnGadgetsTotal.ToString();
		GorillaTelemetry.gSuperInfectionArgs["time_holding_own_gadgets_interval"] = timeUsingOwnGadgetsInterval.ToString();
		GorillaTelemetry.gSuperInfectionArgs["time_holding_others_gadgets_total"] = timeUsingOthersGadgetsTotal.ToString();
		GorillaTelemetry.gSuperInfectionArgs["time_holding_others_gadgets_interval"] = timeUsingOthersGadgetsInterval.ToString();
		GorillaTelemetry.gSuperInfectionArgs["tags_holding_gadget_type_total"] = dictionary3;
		GorillaTelemetry.gSuperInfectionArgs["tags_holding_gadget_type_interval"] = dictionary4;
		GorillaTelemetry.gSuperInfectionArgs["tags_holding_own_gadgets_total"] = tagsHoldingOwnGadgetsTotal.ToString();
		GorillaTelemetry.gSuperInfectionArgs["tags_holding_own_gadgets_interval"] = tagsHoldingOwnGadgetsInterval.ToString();
		GorillaTelemetry.gSuperInfectionArgs["tags_holding_others_gadgets_total"] = tagsHoldingOthersGadgetsTotal.ToString();
		GorillaTelemetry.gSuperInfectionArgs["tags_holding_others_gadgets_interval"] = tagsHoldingOthersGadgetsInterval.ToString();
		GorillaTelemetry.gSuperInfectionArgs["resource_collected_total"] = dictionary5;
		GorillaTelemetry.gSuperInfectionArgs["resource_collected_interval"] = dictionary6;
		GorillaTelemetry.gSuperInfectionArgs["rounds_played_total"] = roundsPlayedTotal.ToString();
		GorillaTelemetry.gSuperInfectionArgs["rounds_played_interval"] = roundsPlayedInterval.ToString();
		GorillaTelemetry.gSuperInfectionArgs["unlocked_nodes"] = new string(array);
		GorillaTelemetry.gSuperInfectionArgs["number_of_players"] = numberOfPlayers.ToString();
		GorillaTelemetry.QueueTelemetryEventPlayFab(new EventContents
		{
			Name = (roomDisconnect ? "super_infection_room_disconnect" : "super_infection_interval"),
			EventNamespace = GorillaTelemetry.EVENT_NAMESPACE,
			Payload = GorillaTelemetry.gSuperInfectionArgs
		});
		GorillaTelemetry.SendMothershipAnalytics(new GhostReactorTelemetryData
		{
			EventName = (roomDisconnect ? "super_infection_room_left" : "super_infection_interval"),
			CustomTags = new string[]
			{
				KIDTelemetry.GameVersionCustomTag,
				KIDTelemetry.GameEnvironment
			},
			BodyData = new Dictionary<string, object>
			{
				{
					"event_timestamp",
					DateTime.Now.ToString()
				},
				{
					"total_play_time",
					totalPlayTime.ToString()
				},
				{
					"room_play_time",
					roomPlayTime.ToString()
				},
				{
					"session_play_time",
					sessionPlayTime.ToString()
				},
				{
					"interval_play_time",
					intervalPlayTime.ToString()
				},
				{
					"terminal_total_time",
					terminalTotalTime.ToString()
				},
				{
					"terminal_interval_time",
					terminalIntervalTime.ToString()
				},
				{ "time_holding_gadget_type_total", timeUsingGadgetsTotal },
				{ "time_holding_gadget_type_interval", timeUsingGadgetsInterval },
				{
					"time_holding_own_gadgets_total",
					timeUsingOwnGadgetsTotal.ToString()
				},
				{
					"time_holding_own_gadgets_interval",
					timeUsingOwnGadgetsInterval.ToString()
				},
				{
					"time_holding_others_gadgets_total",
					timeUsingOthersGadgetsTotal.ToString()
				},
				{
					"time_holding_others_gadgets_interval",
					timeUsingOthersGadgetsInterval.ToString()
				},
				{ "tags_holding_gadget_type_total", dictionary3 },
				{ "tags_holding_gadget_type_interval", dictionary4 },
				{
					"tags_holding_own_gadgets_total",
					tagsHoldingOwnGadgetsTotal.ToString()
				},
				{
					"tags_holding_own_gadgets_interval",
					tagsHoldingOwnGadgetsInterval.ToString()
				},
				{
					"tags_holding_others_gadgets_total",
					tagsHoldingOthersGadgetsTotal.ToString()
				},
				{
					"tags_holding_others_gadgets_interval",
					tagsHoldingOthersGadgetsInterval.ToString()
				},
				{ "resource_type_collected_total", dictionary5 },
				{ "resource_type_collected_interval", dictionary6 },
				{
					"rounds_played_total",
					roundsPlayedTotal.ToString()
				},
				{
					"rounds_played_interval",
					roundsPlayedInterval.ToString()
				},
				{
					"unlocked_nodes",
					new string(array)
				},
				{
					"player_count",
					numberOfPlayers.ToString()
				}
			}
		});
	}

	public static void SuperInfectionEvent(string purchaseType, int shinyRockCost, int techPointsPurchased, float totalPlayTime, float roomPlayTime, float sessionPlayTime)
	{
		if (!GorillaTelemetry.IsConnectedIgnoreRoom())
		{
			return;
		}
		GorillaTelemetry.gSuperInfectionArgs["User"] = GorillaTelemetry.PlayFabUserId();
		GorillaTelemetry.gSuperInfectionArgs["event_timestamp"] = DateTime.Now.ToString();
		GorillaTelemetry.gSuperInfectionArgs["total_play_time"] = totalPlayTime.ToString();
		GorillaTelemetry.gSuperInfectionArgs["room_play_time"] = roomPlayTime.ToString();
		GorillaTelemetry.gSuperInfectionArgs["session_play_time"] = sessionPlayTime.ToString();
		GorillaTelemetry.gSuperInfectionArgs["si_purchase_type"] = purchaseType;
		GorillaTelemetry.gSuperInfectionArgs["si_shiny_rock_cost"] = shinyRockCost;
		GorillaTelemetry.gSuperInfectionArgs["si_tech_points_purchased"] = techPointsPurchased;
		GorillaTelemetry.QueueTelemetryEventPlayFab(new EventContents
		{
			Name = "super_infection_purchase",
			EventNamespace = GorillaTelemetry.EVENT_NAMESPACE,
			Payload = GorillaTelemetry.gSuperInfectionArgs
		});
		GorillaTelemetry.SendMothershipAnalytics(new GhostReactorTelemetryData
		{
			EventName = "super_infection_purchase",
			CustomTags = new string[]
			{
				KIDTelemetry.GameVersionCustomTag,
				KIDTelemetry.GameEnvironment
			},
			BodyData = new Dictionary<string, object>
			{
				{
					"event_timestamp",
					DateTime.Now.ToString()
				},
				{
					"total_play_time",
					totalPlayTime.ToString()
				},
				{
					"room_play_time",
					roomPlayTime.ToString()
				},
				{
					"session_play_time",
					sessionPlayTime.ToString()
				},
				{
					"si_purchase_type",
					purchaseType.ToString()
				},
				{
					"si_shiny_rock_cost",
					shinyRockCost.ToString()
				},
				{
					"si_tech_points_purchased",
					techPointsPurchased.ToString()
				}
			}
		});
	}

	public static void SendMothershipAnalytics(TelemetryData data)
	{
		if (string.IsNullOrEmpty(data.EventName))
		{
			Debug.LogError("[GORILLA_TELEMETRY::MOTHERSHIP_ANALYTICS] Event Name is null or empty");
			return;
		}
		if (data.BodyData == null || data.BodyData.Count == 0)
		{
			Debug.LogError("[GORILLA_TELEMETRY::MOTHERSHIP_ANALYTICS] Body Data KVPs are null or empty - must have at least 1");
			return;
		}
		string text = string.Empty;
		if (data.CustomTags != null && data.CustomTags.Length != 0)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			for (int j = 0; j < data.CustomTags.Length; j++)
			{
				dictionary.Add(string.Format("tag{0}", j + 1), data.CustomTags[j]);
			}
			text = JsonConvert.SerializeObject(dictionary);
		}
		string text2 = JsonConvert.SerializeObject(data.BodyData);
		MothershipWriteEventsRequest mothershipWriteEventsRequest = new MothershipWriteEventsRequest
		{
			title_id = MothershipClientApiUnity.TitleId,
			deployment_id = MothershipClientApiUnity.DeploymentId,
			env_id = MothershipClientApiUnity.EnvironmentId,
			events = new AnalyticsRequestVector(new List<MothershipAnalyticsEvent>
			{
				new MothershipAnalyticsEvent
				{
					event_timestamp = DateTime.UtcNow.ToString("O"),
					event_name = data.EventName,
					custom_tags = text,
					body = text2
				}
			})
		};
		MothershipClientApiUnity.WriteEvents(MothershipClientContext.MothershipId, mothershipWriteEventsRequest, delegate(MothershipWriteEventsResponse resp)
		{
			Debug.Log("[GORILLA_TELEMETRY::MOTHERSHIP_ANALYTICS] Successfully submitted analytics for event: [" + data.EventName + "]");
		}, delegate(MothershipError err, int i)
		{
			Debug.Log("[GORILLA_TELEMETRY::MOTHERSHIP_ANALYTICS] Failed to submit analytics for event: [" + data.EventName + "], with error:\n" + err.Message);
		});
	}

	public static void SendMothershipAnalytics(GhostReactorTelemetryData data)
	{
		if (string.IsNullOrEmpty(data.EventName))
		{
			Debug.LogError("[GORILLA_TELEMETRY::MOTHERSHIP_ANALYTICS] Event Name is null or empty");
			return;
		}
		if (data.BodyData == null || data.BodyData.Count == 0)
		{
			Debug.LogError("[GORILLA_TELEMETRY::MOTHERSHIP_ANALYTICS] Body Data KVPs are null or empty - must have at least 1");
			return;
		}
		string text = string.Empty;
		if (data.CustomTags != null && data.CustomTags.Length != 0)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			for (int j = 0; j < data.CustomTags.Length; j++)
			{
				dictionary.Add(string.Format("tag{0}", j + 1), data.CustomTags[j]);
			}
			text = JsonConvert.SerializeObject(dictionary);
		}
		string text2 = JsonConvert.SerializeObject(data.BodyData);
		MothershipWriteEventsRequest mothershipWriteEventsRequest = new MothershipWriteEventsRequest
		{
			title_id = MothershipClientApiUnity.TitleId,
			deployment_id = MothershipClientApiUnity.DeploymentId,
			env_id = MothershipClientApiUnity.EnvironmentId,
			events = new AnalyticsRequestVector(new List<MothershipAnalyticsEvent>
			{
				new MothershipAnalyticsEvent
				{
					event_timestamp = DateTime.UtcNow.ToString("O"),
					event_name = data.EventName,
					custom_tags = text,
					body = text2
				}
			})
		};
		MothershipClientApiUnity.WriteEvents(MothershipClientContext.MothershipId, mothershipWriteEventsRequest, delegate(MothershipWriteEventsResponse resp)
		{
			Debug.Log("[GORILLA_TELEMETRY::MOTHERSHIP_ANALYTICS] Successfully submitted analytics for event: [" + data.EventName + "]");
		}, delegate(MothershipError err, int i)
		{
			Debug.Log("[GORILLA_TELEMETRY::MOTHERSHIP_ANALYTICS] Failed to submit analytics for event: [" + data.EventName + "], with error:\n" + err.Message);
		});
	}

	public static void PostNotificationEvent(string notificationType)
	{
		if (!GorillaTelemetry.IsConnected())
		{
			return;
		}
		string text = GorillaTelemetry.PlayFabUserId();
		Dictionary<string, object> dictionary = GorillaTelemetry.gNotifEventArgs;
		dictionary["User"] = text;
		dictionary["EventType"] = notificationType;
		GorillaTelemetry.EnqueueTelemetryEventPlayFab(new EventContents
		{
			Name = "telemetry_ggwp_event",
			EventNamespace = GorillaTelemetry.EVENT_NAMESPACE,
			Payload = dictionary
		});
		GorillaTelemetry.EnqueueTelemetryEvent("telemetry_ggwp_event", dictionary, null);
	}

	private static readonly float TELEMETRY_FLUSH_SEC = 10f;

	private static readonly ConcurrentQueue<EventContents> telemetryEventsQueuePlayFab = new ConcurrentQueue<EventContents>();

	private static readonly ConcurrentQueue<MothershipAnalyticsEvent> telemetryEventsQueueMothership = new ConcurrentQueue<MothershipAnalyticsEvent>();

	private static readonly Dictionary<int, List<EventContents>> gListPoolPlayFab = new Dictionary<int, List<EventContents>>();

	private static readonly Dictionary<int, List<MothershipAnalyticsEvent>> gListPoolMothership = new Dictionary<int, List<MothershipAnalyticsEvent>>();

	private static readonly string namespacePrefix = "custom";

	private static readonly string EVENT_NAMESPACE = GorillaTelemetry.namespacePrefix + "." + PlayFabAuthenticatorSettings.TitleId;

	private static PlayFabAuthenticator gPlayFabAuth;

	private static readonly Dictionary<string, object> gZoneEventArgs;

	private static readonly Dictionary<string, object> gNotifEventArgs;

	public static GTZone LastZone;

	public static GTSubZone LastSubZone;

	public static GTZoneEventType LastZoneEventType;

	private static readonly Dictionary<string, object> gGameModeStartEventArgs;

	private static readonly Dictionary<string, object> gShopEventArgs;

	private static CosmeticsController.CosmeticItem[] gSingleItemParam;

	private static BuilderSetManager.BuilderSetStoreItem[] gSingleItemBuilderParam;

	private static Dictionary<string, object> gKidEventArgs;

	private static readonly Dictionary<string, object> gWamGameStartArgs;

	private static readonly Dictionary<string, object> gWamLevelEndArgs;

	private static Dictionary<string, object> gCustomMapPerfArgs;

	private static Dictionary<string, object> gCustomMapTrackingMetrics;

	private static Dictionary<string, object> gCustomMapDownloadMetrics;

	private static readonly Dictionary<string, object> gGhostReactorShiftStartArgs;

	private static readonly Dictionary<string, object> gGhostReactorShiftEndArgs;

	private static readonly Dictionary<string, object> gGhostReactorFloorStartArgs;

	private static readonly Dictionary<string, object> gGhostReactorFloorEndArgs;

	private static readonly Dictionary<string, object> gGhostReactorToolPurchasedArgs;

	private static readonly Dictionary<string, object> gGhostReactorRankUpArgs;

	private static readonly Dictionary<string, object> gGhostReactorToolUnlockArgs;

	private static readonly Dictionary<string, object> gGhostReactorPodUpgradePurchasedArgs;

	private static readonly Dictionary<string, object> gGhostReactorToolUpgradeArgs;

	private static readonly Dictionary<string, object> gGhostReactorChaosSeedStartArgs;

	private static readonly Dictionary<string, object> gGhostReactorChaosJuiceCollectedArgs;

	private static readonly Dictionary<string, object> gGhostReactorOverdrivePurchasedArgs;

	private static readonly Dictionary<string, object> gGhostReactorCreditsRefillPurchasedArgs;

	private static readonly Dictionary<string, object> gSuperInfectionArgs;

	public static class k
	{
		public const string User = "User";

		public const string ZoneId = "ZoneId";

		public const string SubZoneId = "SubZoneId";

		public const string EventType = "EventType";

		public const string IsPrivateRoom = "IsPrivateRoom";

		public const string Items = "Items";

		public const string VoiceChatEnabled = "VoiceChatEnabled";

		public const string JoinGroups = "JoinGroups";

		public const string CustomUsernameEnabled = "CustomUsernameEnabled";

		public const string AgeCategory = "AgeCategory";

		public const string telemetry_zone_event = "telemetry_zone_event";

		public const string telemetry_shop_event = "telemetry_shop_event";

		public const string telemetry_kid_event = "telemetry_kid_event";

		public const string telemetry_ggwp_event = "telemetry_ggwp_event";

		public const string NOTHING = "NOTHING";

		public const string telemetry_wam_gameStartEvent = "telemetry_wam_gameStartEvent";

		public const string telemetry_wam_levelEndEvent = "telemetry_wam_levelEndEvent";

		public const string WamMachineId = "WamMachineId";

		public const string WamGameId = "WamGameId";

		public const string WamMLevelNumber = "WamMLevelNumber";

		public const string WamGoodMolesShown = "WamGoodMolesShown";

		public const string WamHazardMolesShown = "WamHazardMolesShown";

		public const string WamLevelMinScore = "WamLevelMinScore";

		public const string WamLevelScore = "WamLevelScore";

		public const string WamHazardMolesHit = "WamHazardMolesHit";

		public const string WamGameState = "WamGameState";

		public const string CustomMapName = "CustomMapName";

		public const string LowestFPS = "LowestFPS";

		public const string LowestFPSDrawCalls = "LowestFPSDrawCalls";

		public const string LowestFPSPlayerCount = "LowestFPSPlayerCount";

		public const string AverageFPS = "AverageFPS";

		public const string AverageDrawCalls = "AverageDrawCalls";

		public const string AveragePlayerCount = "AveragePlayerCount";

		public const string HighestFPS = "HighestFPS";

		public const string HighestFPSDrawCalls = "HighestFPSDrawCalls";

		public const string HighestFPSPlayerCount = "HighestFPSPlayerCount";

		public const string CustomMapCreator = "CustomMapCreator";

		public const string CustomMapModId = "CustomMapModId";

		public const string MinPlayerCount = "MinPlayerCount";

		public const string MaxPlayerCount = "MaxPlayerCount";

		public const string PlaytimeOnMap = "PlaytimeOnMap";

		public const string PlaytimeInSeconds = "PlaytimeInSeconds";

		public const string PrivateRoom = "PrivateRoom";

		public const string ghost_game_start = "ghost_game_start";

		public const string ghost_game_end = "ghost_game_end";

		public const string ghost_floor_start = "ghost_floor_start";

		public const string ghost_floor_end = "ghost_floor_end";

		public const string ghost_tool_purchased = "ghost_tool_purchased";

		public const string ghost_game_rank_up = "ghost_game_rank_up";

		public const string ghost_game_tool_unlock = "ghost_game_tool_unlock";

		public const string ghost_pod_upgrade_purchased = "ghost_pod_upgrade_purchased";

		public const string ghost_game_tool_upgrade = "ghost_game_tool_upgrade";

		public const string ghost_chaos_seed_start = "ghost_chaos_seed_start";

		public const string ghost_chaos_juice_collected = "ghost_chaos_juice_collected";

		public const string ghost_overdrive_purchased = "ghost_overdrive_purchased";

		public const string ghost_credits_refill_purchased = "ghost_credits_refill_purchased";

		public const string ghost_game_id = "ghost_game_id";

		public const string event_timestamp = "event_timestamp";

		public const string initial_cores_balance = "initial_cores_balance";

		public const string final_cores_balance = "final_cores_balance";

		public const string cores_spent_waiting_in_breakroom = "cores_spent_waiting_in_breakroom";

		public const string cores_collected = "cores_collected";

		public const string cores_collected_from_ghosts = "cores_collected_from_ghosts";

		public const string cores_collected_from_gathering = "cores_collected_from_gathering";

		public const string cores_spent_on_items = "cores_spent_on_items";

		public const string cores_spent_on_gates = "cores_spent_on_gates";

		public const string cores_spent_on_levels = "cores_spent_on_levels";

		public const string cores_given_to_others = "cores_given_to_others";

		public const string cores_received_from_others = "cores_received_from_others";

		public const string gates_unlocked = "gates_unlocked";

		public const string died = "died";

		public const string caught_in_anamole = "caught_in_anamole";

		public const string items_purchased = "items_purchased";

		public const string levels_unlocked = "levels_unlocked";

		public const string shift_cut = "shift_cut";

		public const string number_of_players = "number_of_players";

		public const string start_at_beginning = "start_at_beginning";

		public const string seconds_into_shift_at_join = "seconds_into_shift_at_join";

		public const string reason = "reason";

		public const string play_duration = "play_duration";

		public const string started_late = "started_late";

		public const string time_started = "time_started";

		public const string max_number_in_game = "max_number_in_game";

		public const string end_number_in_game = "end_number_in_game";

		public const string items_picked_up = "items_picked_up";

		public const string super_infection_room_disconnect = "super_infection_room_disconnect";

		public const string super_infection_interval = "super_infection_interval";

		public const string super_infection_purchase = "super_infection_purchase";

		public const string si_purchase_type = "si_purchase_type";

		public const string si_shiny_rock_cost = "si_shiny_rock_cost";

		public const string si_tech_points_purchased = "si_tech_points_purchased";

		public const string total_play_time = "total_play_time";

		public const string room_play_time = "room_play_time";

		public const string session_play_time = "session_play_time";

		public const string interval_play_time = "interval_play_time";

		public const string terminal_total_time = "terminal_total_time";

		public const string terminal_interval_time = "terminal_interval_time";

		public const string time_holding_gadget_type_total = "time_holding_gadget_type_total";

		public const string time_holding_gadget_type_interval = "time_holding_gadget_type_interval";

		public const string time_holding_own_gadgets_total = "time_holding_own_gadgets_total";

		public const string time_holding_own_gadgets_interval = "time_holding_own_gadgets_interval";

		public const string time_holding_others_gadgets_total = "time_holding_others_gadgets_total";

		public const string time_holding_others_gadgets_interval = "time_holding_others_gadgets_interval";

		public const string tags_holding_gadget_type_total = "tags_holding_gadget_type_total";

		public const string tags_holding_gadget_type_interval = "tags_holding_gadget_type_interval";

		public const string tags_holding_own_gadgets_total = "tags_holding_own_gadgets_total";

		public const string tags_holding_own_gadgets_interval = "tags_holding_own_gadgets_interval";

		public const string tags_holding_others_gadgets_total = "tags_holding_others_gadgets_total";

		public const string tags_holding_others_gadgets_interval = "tags_holding_others_gadgets_interval";

		public const string resource_collected_total = "resource_collected_total";

		public const string resource_collected_interval = "resource_collected_interval";

		public const string rounds_played_total = "rounds_played_total";

		public const string rounds_played_interval = "rounds_played_interval";

		public const string unlocked_nodes = "unlocked_nodes";

		public const string floor_joined = "floor_joined";

		public const string player_rank = "player_rank";

		public const string total_cores_collected_by_player = "total_cores_collected_by_player";

		public const string total_cores_collected_by_group = "total_cores_collected_by_group";

		public const string total_cores_spent_by_player = "total_cores_spent_by_player";

		public const string total_cores_spent_by_group = "total_cores_spent_by_group";

		public const string floor = "floor";

		public const string preset = "preset";

		public const string modifier = "modifier";

		public const string section = "section";

		public const string xp_gained = "xp_gained";

		public const string chaos_seeds_collected = "chaos_seeds_collected";

		public const string objectives_completed = "objectives_completed";

		public const string revives = "revives";

		public const string tool = "tool";

		public const string tool_level = "tool_level";

		public const string cores_spent = "cores_spent";

		public const string shiny_rocks_spent = "shiny_rocks_spent";

		public const string new_rank = "new_rank";

		public const string upgrade = "upgrade";

		public const string grift_price = "grift_price";

		public const string type = "type";

		public const string new_level = "new_level";

		public const string juice_spent = "juice_spent";

		public const string grift_spent = "grift_spent";

		public const string chaos_seeds_in_queue = "chaos_seeds_in_queue";

		public const string unlock_time = "unlock_time";

		public const string shiny_rocks_used = "shiny_rocks_used";

		public const string juice_collected = "juice_collected";

		public const string cores_processed_by_overdrive = "cores_processed_by_overdrive";

		public const string final_credits = "final_credits";

		public const string is_private_room = "is_private_room";

		public const string num_shifts_played = "num_shifts_played";

		public const string game_mode_played_event = "game_mode_played_event";

		public const string game_mode = "game_mode";
	}

	private class BatchRunner : MonoBehaviour
	{
		private IEnumerator Start()
		{
			for (;;)
			{
				float start = Time.time;
				while (Time.time < start + GorillaTelemetry.TELEMETRY_FLUSH_SEC)
				{
					yield return null;
				}
				GorillaTelemetry.FlushPlayFabTelemetry();
				GorillaTelemetry.FlushMothershipTelemetry();
			}
			yield break;
		}
	}
}
