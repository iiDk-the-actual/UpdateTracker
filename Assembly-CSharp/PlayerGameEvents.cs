using System;

public class PlayerGameEvents
{
	public static event Action<string> OnGameModeObjectiveTrigger;

	public static event Action<string> OnGameModeCompleteRound;

	public static event Action<string> OnGrabbedObject;

	public static event Action<string> OnDroppedObject;

	public static event Action<string> OnEatObject;

	public static event Action<string> OnTapObject;

	public static event Action<string> OnLaunchedProjectile;

	public static event Action<float, float> OnPlayerMoved;

	public static event Action<float, float> OnPlayerSwam;

	public static event Action<string> OnTriggerHandEffect;

	public static event Action<string> OnEnterLocation;

	public static event Action<string, int> OnMiscEvent;

	public static event Action<string> OnCritterEvent;

	public static void GameModeObjectiveTriggered()
	{
		string text = GorillaGameManager.instance.GameModeName();
		Action<string> onGameModeObjectiveTrigger = PlayerGameEvents.OnGameModeObjectiveTrigger;
		if (onGameModeObjectiveTrigger == null)
		{
			return;
		}
		onGameModeObjectiveTrigger(text);
	}

	public static void GameModeCompleteRound()
	{
		string text = GorillaGameManager.instance.GameModeName();
		Action<string> onGameModeCompleteRound = PlayerGameEvents.OnGameModeCompleteRound;
		if (onGameModeCompleteRound == null)
		{
			return;
		}
		onGameModeCompleteRound(text);
	}

	public static void GrabbedObject(string objectName)
	{
		Action<string> onGrabbedObject = PlayerGameEvents.OnGrabbedObject;
		if (onGrabbedObject == null)
		{
			return;
		}
		onGrabbedObject(objectName);
	}

	public static void DroppedObject(string objectName)
	{
		Action<string> onDroppedObject = PlayerGameEvents.OnDroppedObject;
		if (onDroppedObject == null)
		{
			return;
		}
		onDroppedObject(objectName);
	}

	public static void EatObject(string objectName)
	{
		Action<string> onEatObject = PlayerGameEvents.OnEatObject;
		if (onEatObject == null)
		{
			return;
		}
		onEatObject(objectName);
	}

	public static void TapObject(string objectName)
	{
		Action<string> onTapObject = PlayerGameEvents.OnTapObject;
		if (onTapObject == null)
		{
			return;
		}
		onTapObject(objectName);
	}

	public static void LaunchedProjectile(string objectName)
	{
		Action<string> onLaunchedProjectile = PlayerGameEvents.OnLaunchedProjectile;
		if (onLaunchedProjectile == null)
		{
			return;
		}
		onLaunchedProjectile(objectName);
	}

	public static void PlayerMoved(float distance, float speed)
	{
		Action<float, float> onPlayerMoved = PlayerGameEvents.OnPlayerMoved;
		if (onPlayerMoved == null)
		{
			return;
		}
		onPlayerMoved(distance, speed);
	}

	public static void PlayerSwam(float distance, float speed)
	{
		Action<float, float> onPlayerSwam = PlayerGameEvents.OnPlayerSwam;
		if (onPlayerSwam == null)
		{
			return;
		}
		onPlayerSwam(distance, speed);
	}

	public static void TriggerHandEffect(string effectName)
	{
		Action<string> onTriggerHandEffect = PlayerGameEvents.OnTriggerHandEffect;
		if (onTriggerHandEffect == null)
		{
			return;
		}
		onTriggerHandEffect(effectName);
	}

	public static void TriggerEnterLocation(string locationName)
	{
		Action<string> onEnterLocation = PlayerGameEvents.OnEnterLocation;
		if (onEnterLocation == null)
		{
			return;
		}
		onEnterLocation(locationName);
	}

	public static void MiscEvent(string eventName, int count = 1)
	{
		Action<string, int> onMiscEvent = PlayerGameEvents.OnMiscEvent;
		if (onMiscEvent == null)
		{
			return;
		}
		onMiscEvent(eventName, count);
	}

	public static void CritterEvent(string eventName)
	{
		Action<string> onCritterEvent = PlayerGameEvents.OnCritterEvent;
		if (onCritterEvent == null)
		{
			return;
		}
		onCritterEvent(eventName);
	}

	public enum EventType
	{
		NONE,
		GameModeObjective,
		GameModeCompleteRound,
		GrabbedObject,
		DroppedObject,
		EatObject,
		TapObject,
		LaunchedProjectile,
		PlayerMoved,
		PlayerSwam,
		TriggerHandEfffect,
		EnterLocation,
		MiscEvent
	}
}
