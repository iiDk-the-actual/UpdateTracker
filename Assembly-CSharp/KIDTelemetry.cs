using System;
using UnityEngine;

public static class KIDTelemetry
{
	public static string GameVersionCustomTag
	{
		get
		{
			return "game_version_" + Application.version;
		}
	}

	public static string Open_MetricActionCustomTag
	{
		get
		{
			return "metric_action_Open";
		}
	}

	public static string Updated_MetricActionCustomTag
	{
		get
		{
			return "metric_action_Updated";
		}
	}

	public static string Closed_MetricActionCustomTag
	{
		get
		{
			return "metric_action_Closed";
		}
	}

	public static string GameEnvironment
	{
		get
		{
			return "game_environment_live";
		}
	}

	public static string GetPermissionManagedByBodyData(string permission)
	{
		return "permission_managedby_" + permission.Replace('-', '_');
	}

	public static string GetPermissionEnabledBodyData(string permission)
	{
		return "permission_eneabled_" + permission.Replace('-', '_');
	}

	public const string SCREEN_SHOWN_EVENT_NAME = "kid_screen_shown";

	public const string PHASE_TWO_IN_COHORT_EVENT_NAME = "kid_phase2_incohort";

	public const string PHASE_THREE_OPTIONAL_EVENT_NAME = "kid_phase3_optional";

	public const string AGE_GATE_EVENT_NAME = "kid_age_gate";

	public const string AGE_GATE_CONFIRM_EVENT_NAME = "kid_age_gate_confirm";

	public const string AGE_DISCREPENCY_EVENT_NAME = "kid_age_gate_discrepency";

	public const string GAME_SETTINGS_EVENT_NAME = "kid_game_settings";

	public const string EMAIL_CONFIRM_EVENT_NAME = "kid_email_confirm";

	public const string AGE_APPEAL_EVENT_NAME = "kid_age_appeal";

	public const string APPEAL_AGE_GATE_EVENT_NAME = "kid_age_appeal_age_gate";

	public const string APPEAL_ENTER_EMAIL_EVENT_NAME = "kid_age_appeal_enter_email";

	public const string APPEAL_CONFIRM_EMAIL_EVENT_NAME = "kid_age_appeal_confirm_email";

	private const string GAME_VERSION_CUSTOM_TAG_PREFIX = "game_version_";

	private const string METRIC_ACTION_CUSTOM_TAG_PREFIX = "metric_action_";

	public const string WARNING_SCREEN_CUSTOM_TAG = "kid_warning_screen";

	public const string PHASE_TWO = "kid_phase_2";

	public const string PHASE_THREE = "kid_phase_3";

	public const string PHASE_FOUR = "kid_phase_4";

	public const string AGE_GATE_CUSTOM_TAG = "kid_age_gate";

	public const string SETTINGS_CUSTOM_TAG = "kid_settings";

	public const string SETUP_CUSTOM_TAG = "kid_setup";

	public const string APPEAL_CUSTOM_TAG = "kid_age_appeal";

	public const string SCREEN_TYPE_BODY_DATA = "screen";

	public const string OPT_IN_CHOICE_BODY_DATA = "opt_in_choice";

	public const string BUTTON_PRESSED_BODY_DATA = "button_pressed";

	public const string MISMATCH_EXPECTED_BODY_DATA = "mismatch_expected";

	public const string MISMATCH_ACTUAL_BODY_DATA = "mismatch_actual";

	public const string AGE_DECLARED_BODY_DATA = "age_declared";

	public const string LEARN_MORE_URL_PRESSED_BODY_DATA = "learn_more_url_pressed";

	public const string SCREEN_SHOWN_REASON_BODY_DATA = "screen_shown_reason";

	public const string SUBMITTED_AGE_BODY_DATA = "submitted_age";

	public const string CORRECT_AGE_BODY_DATA = "correct_age";

	public const string APPEAL_EMAIL_TYPE_BODY_DATA = "email_type";

	public const string SHOWN_SETTINGS_SCREEN = "saw_game_settings";

	public const string KID_STATUS_BODY_DATA = "kid_status";

	private const string PERMISSION_MANAGED_BY_BODY_DATA = "permission_managedby_";

	private const string PERMISSION_ENABLED_BODY_DATA = "permission_eneabled_";
}
