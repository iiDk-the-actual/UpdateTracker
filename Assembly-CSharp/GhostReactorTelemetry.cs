using System;
using UnityEngine;

public class GhostReactorTelemetry : MonoBehaviour
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

	public const string SHIFT_START_EVENT_NAME = "ghost_game_start";

	public const string SHIFT_END_EVENT_NAME = "ghost_game_end";

	public const string FLOOR_START_EVENT_NAME = "ghost_floor_start";

	public const string FLOOR_END_EVENT_NAME = "ghost_floor_end";

	public const string TOOL_PURCHASED_EVENT_NAME = "ghost_tool_purchased";

	public const string RANK_UP_EVENT_NAME = "ghost_game_rank_up";

	public const string TOOL_UNLOCK_EVENT_NAME = "ghost_game_tool_unlock";

	public const string POD_UPGRADE_PURCHASED_EVENT_NAME = "ghost_pod_upgrade_purchased";

	public const string TOOL_UPGRADE_EVENT_NAME = "ghost_game_tool_upgrade";

	public const string CHAOS_SEED_START_EVENT_NAME = "ghost_chaos_seed_start";

	public const string CHAOS_JUICE_COLLECTED_EVENT_NAME = "ghost_chaos_juice_collected";

	public const string OVERDRIVE_PURCHASED_EVENT_NAME = "ghost_overdrive_purchased";

	public const string CREDITS_REFILL_PURCHASED_EVENT_NAME = "ghost_credits_refill_purchased";

	private const string GAME_VERSION_CUSTOM_TAG_PREFIX = "game_version_";

	private const string METRIC_ACTION_CUSTOM_TAG_PREFIX = "metric_action_";

	public const string GHOST_GAME_ID_BODY_DATA = "ghost_game_id";

	public const string EVENT_TIMESTAMP_BODY_DATA = "event_timestamp";

	public const string INITIAL_CORES_BALANCE_BODY_DATA = "initial_cores_balance";

	public const string FINAL_CORES_BALANCE_BODY_DATA = "final_cores_balance";

	public const string CORES_SPENT_WAITING_IN_BREAKROOM_BODY_DATA = "cores_spent_waiting_in_breakroom";

	public const string CORES_COLLECTED_FROM_GHOSTS_BODY_DATA = "cores_collected_from_ghosts";

	public const string CORES_COLLECTED_FROM_GATHERING_BODY_DATA = "cores_collected_from_gathering";

	public const string CORES_SPENT_ON_ITEMS_BODY_DATA = "cores_spent_on_items";

	public const string CORES_SPENT_ON_GATES_BODY_DATA = "cores_spent_on_gates";

	public const string CORES_SPENT_ON_LEVELS_BODY_DATA = "cores_spent_on_levels";

	public const string CORES_GIVEN_TO_OTHERS_BODY_DATA = "cores_given_to_others";

	public const string CORES_RECEIVED_FROM_OTHERS_BODY_DATA = "cores_received_from_others";

	public const string SHIFT_CUT_DATA = "shift_cut_data";

	public const string GATES_UNLOCKED_BODY_DATA = "gates_unlocked";

	public const string DIED_BODY_DATA = "died";

	public const string CAUGHT_IN_ANAMOLE_BODY_DATA = "caught_in_anamole";

	public const string ITEMS_PURCHASED_BODY_DATA = "items_purchased";

	public const string LEVELS_UNLOCKED_BODY_DATA = "levels_unlocked";

	public const string NUMBER_OF_PLAYERS_BODY_DATA = "number_of_players";

	public const string START_AT_BEGINNING_BODY_DATA = "start_at_beginning";

	public const string SECONDS_INTO_SHIFT_AT_JOIN_BODY_DATA = "seconds_into_shift_at_join";

	public const string REASON_BODY_DATA = "reason";

	public const string PLAY_DURATION_BODY_DATA = "play_duration";

	public const string STARTED_LATE_BODY_DATA = "started_late";

	public const string TIME_STARTED_BODY_DATA = "time_started";

	public const string CORES_COLLECTED_BODY_DATA = "cores_collected";

	public const string MAX_NUMBER_IN_GAME_BODY_DATA = "max_number_in_game";

	public const string END_NUMBER_IN_GAME_BODY_DATA = "end_number_in_game";

	public const string ITEMS_PICKED_UP_BODY_DATA = "items_picked_up";

	public const string FLOOR_JOINED_BODY_DATA = "floor_joined";

	public const string PLAYER_RANK_BODY_DATA = "player_rank";

	public const string TOTAL_CORES_COLLECTED_BY_PLAYER_BODY_DATA = "total_cores_collected_by_player";

	public const string TOTAL_CORES_COLLECTED_BY_GROUP_BODY_DATA = "total_cores_collected_by_group";

	public const string TOTAL_CORES_SPENT_BY_PLAYER_BODY_DATA = "total_cores_spent_by_player";

	public const string TOTAL_CORES_SPENT_BY_GROUP_BODY_DATA = "total_cores_spent_by_group";

	public const string FLOOR_BODY_DATA = "floor";

	public const string PRESET_BODY_DATA = "preset";

	public const string MODIFIER_BODY_DATA = "modifier";

	public const string SECTION_BODY_DATA = "section";

	public const string XP_GAINED_BODY_DATA = "xp_gained";

	public const string CHAOS_SEEDS_COLLECTED_BODY_DATA = "chaos_seeds_collected";

	public const string OBJECTIVES_COMPLETED_BODY_DATA = "objectives_completed";

	public const string REVIVES_BODY_DATA = "revives";

	public const string TOOL_BODY_DATA = "tool";

	public const string TOOL_LEVEL_BODY_DATA = "tool_level";

	public const string CORES_SPENT_BODY_DATA = "cores_spent";

	public const string SHINY_ROCKS_SPENT_BODY_DATA = "shiny_rocks_spent";

	public const string NEW_RANK_BODY_DATA = "new_rank";

	public const string UPGRADE_BODY_DATA = "upgrade";

	public const string GRIFT_PRICE_BODY_DATA = "grift_price";

	public const string TYPE_BODY_DATA = "type";

	public const string NEW_LEVEL_BODY_DATA = "new_level";

	public const string JUICE_SPENT_BODY_DATA = "juice_spent";

	public const string GRIFT_SPENT_BODY_DATA = "grift_spent";

	public const string CHAOS_SEEDS_IN_QUEUE_BODY_DATA = "chaos_seeds_in_queue";

	public const string UNLOCK_TIME_BODY_DATA = "unlock_time";

	public const string SHINY_ROCKS_USED_BODY_DATA = "shiny_rocks_used";

	public const string JUICE_COLLECTED_BODY_DATA = "juice_collected";

	public const string CORES_PROCESSED_BY_OVERDRIVE_BODY_DATA = "cores_processed_by_overdrive";

	public const string FINAL_CREDITS_BODY_DATA = "final_credits";

	public const string IS_PRIVATE_ROOM_BODY_DATA = "is_private_room";

	public const string NUM_SHIFTS_PLAYED_BODY_DATA = "num_shifts_played";
}
