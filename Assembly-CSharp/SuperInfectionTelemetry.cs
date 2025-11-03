using System;
using UnityEngine;

public class SuperInfectionTelemetry : MonoBehaviour
{
	public static string GameVersionCustomTag
	{
		get
		{
			return "game_version_" + Application.version;
		}
	}

	public static string GameEnvironment
	{
		get
		{
			return "game_environment_live";
		}
	}

	public const string ROOM_LEFT_EVENT_NAME = "super_infection_room_left";

	public const string INTERVAL_EVENT_NAME = "super_infection_interval";

	public const string SI_PURCHASE_EVENT_NAME = "super_infection_purchase";

	private const string GAME_VERSION_CUSTOM_TAG_PREFIX = "game_version_";

	private const string METRIC_ACTION_CUSTOM_TAG_PREFIX = "metric_action_";

	public const string SUPER_INFECTION_PREPEND = "super_infection_";

	public const string SUPER_INFECTION_GAME_ID_BODY_DATA = "super_infection_round_id";

	public const string EVENT_TIMESTAMP_BODY_DATA = "event_timestamp";

	public const string TOTAL_PLAY_TIME_BODY_DATA = "total_play_time";

	public const string ROOM_PLAY_TIME_BODY_DATA = "room_play_time";

	public const string SESSION_PLAY_TIME_BODY_DATA = "session_play_time";

	public const string INTERVAL_PLAY_TIME_BODY_DATA = "interval_play_time";

	public const string TERMINAL_TOTAL_TIME_BODY_DATA = "terminal_total_time";

	public const string TERMINAL_INTERVAL_TIME_BODY_DATA = "terminal_interval_time";

	public const string TIME_USING_GADGET_TYPE_TOTAL_BODY_DATA = "time_holding_gadget_type_total";

	public const string TIME_USING_GADGET_TYPE_INTERVAL_BODY_DATA = "time_holding_gadget_type_interval";

	public const string TIME_HOLDING_OWN_GADGETS_TOTAL_BODY_DATA = "time_holding_own_gadgets_total";

	public const string TIME_HOLDING_OWN_GADGETS_INTERVAL_BODY_DATA = "time_holding_own_gadgets_interval";

	public const string TIME_HOLDING_OTHERS_GADGETS_TOTAL_BODY_DATA = "time_holding_others_gadgets_total";

	public const string TIME_HOLDING_OTHERS_GADGETS_INTERVAL_BODY_DATA = "time_holding_others_gadgets_interval";

	public const string TAGS_HOLDING_GADGET_TYPE_TOTAL_BODY_DATA = "tags_holding_gadget_type_total";

	public const string TAGS_HOLDING_GADGET_TYPE_INTERVAL_BODY_DATA = "tags_holding_gadget_type_interval";

	public const string TAGS_HOLDING_OWN_GADGETS_TOTAL_BODY_DATA = "tags_holding_own_gadgets_total";

	public const string TAGS_HOLDING_OWN_GADGETS_INTERVAL_BODY_DATA = "tags_holding_own_gadgets_interval";

	public const string TAGS_HOLDING_OTHERS_GADGETS_TOTAL_BODY_DATA = "tags_holding_others_gadgets_total";

	public const string TAGS_HOLDING_OTHERS_GADGETS_INTERVAL_BODY_DATA = "tags_holding_others_gadgets_interval";

	public const string RESOURCE_TYPE_COLLECTED_TOTAL_BODY_DATA = "resource_type_collected_total";

	public const string RESOURCE_TYPE_COLLECTED_INTERVAL_BODY_DATA = "resource_type_collected_interval";

	public const string ROUNDS_PLAYED_TOTAL_BODY_DATA = "rounds_played_total";

	public const string ROUNDS_PLAYED_INTERVAL_BODY_DATA = "rounds_played_interval";

	public const string UNLOCKED_NODES_BODY_DATA = "unlocked_nodes";

	public const string PLAYER_COUNT_BODY_DATA = "player_count";

	public const string SI_SHINY_ROCK_COST = "si_shiny_rock_cost";

	public const string SI_PURCHASE_TYPE = "si_purchase_type";

	public const string SI_TECH_POINTS_PURCHASED = "si_tech_points_purchased";
}
