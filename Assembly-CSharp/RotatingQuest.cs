using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

[Serializable]
public class RotatingQuest
{
	[JsonIgnore]
	public bool IsMovementQuest
	{
		get
		{
			return this.questType == QuestType.moveDistance || this.questType == QuestType.swimDistance;
		}
	}

	[JsonIgnore]
	public GTZone RequiredZone { get; private set; } = GTZone.none;

	public void SetRequiredZone()
	{
		this.RequiredZone = ((this.requiredZones.Count > 0) ? this.requiredZones[Random.Range(0, this.requiredZones.Count)] : GTZone.none);
	}

	public void AddEventListener()
	{
		if (this.isQuestComplete)
		{
			return;
		}
		switch (this.questType)
		{
		case QuestType.gameModeObjective:
			PlayerGameEvents.OnGameModeObjectiveTrigger += this.OnGameEventOccurence;
			return;
		case QuestType.gameModeRound:
			PlayerGameEvents.OnGameModeCompleteRound += this.OnGameEventOccurence;
			return;
		case QuestType.grabObject:
			PlayerGameEvents.OnGrabbedObject += this.OnGameEventOccurence;
			return;
		case QuestType.dropObject:
			PlayerGameEvents.OnDroppedObject += this.OnGameEventOccurence;
			return;
		case QuestType.eatObject:
			PlayerGameEvents.OnEatObject += this.OnGameEventOccurence;
			return;
		case QuestType.tapObject:
			PlayerGameEvents.OnTapObject += this.OnGameEventOccurence;
			return;
		case QuestType.launchedProjectile:
			PlayerGameEvents.OnLaunchedProjectile += this.OnGameEventOccurence;
			return;
		case QuestType.moveDistance:
			PlayerGameEvents.OnPlayerMoved += this.OnGameMoveEvent;
			return;
		case QuestType.swimDistance:
			PlayerGameEvents.OnPlayerSwam += this.OnGameMoveEvent;
			return;
		case QuestType.triggerHandEffect:
			PlayerGameEvents.OnTriggerHandEffect += this.OnGameEventOccurence;
			return;
		case QuestType.enterLocation:
			PlayerGameEvents.OnEnterLocation += this.OnGameEventOccurence;
			return;
		case QuestType.misc:
			PlayerGameEvents.OnMiscEvent += this.OnGameEventOccurence;
			return;
		case QuestType.critter:
			PlayerGameEvents.OnCritterEvent += this.OnGameEventOccurence;
			return;
		default:
			return;
		}
	}

	public void RemoveEventListener()
	{
		switch (this.questType)
		{
		case QuestType.gameModeObjective:
			PlayerGameEvents.OnGameModeObjectiveTrigger -= this.OnGameEventOccurence;
			return;
		case QuestType.gameModeRound:
			PlayerGameEvents.OnGameModeCompleteRound -= this.OnGameEventOccurence;
			return;
		case QuestType.grabObject:
			PlayerGameEvents.OnGrabbedObject -= this.OnGameEventOccurence;
			return;
		case QuestType.dropObject:
			PlayerGameEvents.OnDroppedObject -= this.OnGameEventOccurence;
			return;
		case QuestType.eatObject:
			PlayerGameEvents.OnEatObject -= this.OnGameEventOccurence;
			return;
		case QuestType.tapObject:
			PlayerGameEvents.OnTapObject -= this.OnGameEventOccurence;
			return;
		case QuestType.launchedProjectile:
			PlayerGameEvents.OnLaunchedProjectile -= this.OnGameEventOccurence;
			return;
		case QuestType.moveDistance:
			PlayerGameEvents.OnPlayerMoved -= this.OnGameMoveEvent;
			return;
		case QuestType.swimDistance:
			PlayerGameEvents.OnPlayerSwam -= this.OnGameMoveEvent;
			return;
		case QuestType.triggerHandEffect:
			PlayerGameEvents.OnTriggerHandEffect -= this.OnGameEventOccurence;
			return;
		case QuestType.enterLocation:
			PlayerGameEvents.OnEnterLocation -= this.OnGameEventOccurence;
			return;
		case QuestType.misc:
			PlayerGameEvents.OnMiscEvent -= this.OnGameEventOccurence;
			return;
		case QuestType.critter:
			PlayerGameEvents.OnCritterEvent -= this.OnGameEventOccurence;
			return;
		default:
			return;
		}
	}

	public void ApplySavedProgress(int progress)
	{
		if (this.questType == QuestType.moveDistance || this.questType == QuestType.swimDistance)
		{
			this.moveDistance = (float)progress;
			this.occurenceCount = Mathf.FloorToInt(this.moveDistance);
			this.isQuestComplete = this.occurenceCount >= this.requiredOccurenceCount;
			return;
		}
		this.occurenceCount = progress;
		this.isQuestComplete = this.occurenceCount >= this.requiredOccurenceCount;
	}

	public int GetProgress()
	{
		if (this.questType == QuestType.moveDistance || this.questType == QuestType.swimDistance)
		{
			return Mathf.FloorToInt(this.moveDistance);
		}
		return this.occurenceCount;
	}

	private void OnGameEventOccurence(string eventName)
	{
		this.OnGameEventOccurence(eventName, 1);
	}

	private void OnGameEventOccurence(string eventName, int count)
	{
		if (this.RequiredZone != GTZone.none && !ZoneManagement.IsInZone(this.RequiredZone))
		{
			return;
		}
		string.IsNullOrEmpty(this.questOccurenceFilter);
		if (eventName.StartsWith(this.questOccurenceFilter))
		{
			this.SetProgress(this.occurenceCount + count);
		}
	}

	private void OnGameMoveEvent(float distance, float speed)
	{
		if (this.RequiredZone != GTZone.none && !ZoneManagement.IsInZone(this.RequiredZone))
		{
			return;
		}
		if (!(this.questOccurenceFilter == "maxSpeed"))
		{
			this.moveDistance += distance;
			this.SetProgress(Mathf.FloorToInt(this.moveDistance));
			return;
		}
		if (speed <= this.moveDistance)
		{
			return;
		}
		this.moveDistance = speed;
		this.SetProgress(Mathf.FloorToInt(this.moveDistance));
	}

	private void SetProgress(int progress)
	{
		if (this.isQuestComplete)
		{
			return;
		}
		if (this.occurenceCount == progress)
		{
			return;
		}
		this.lastChange = Time.frameCount;
		this.occurenceCount = progress;
		if (this.questType == QuestType.moveDistance || this.questType == QuestType.swimDistance)
		{
			this.moveDistance = (float)progress;
		}
		if (this.occurenceCount >= this.requiredOccurenceCount)
		{
			this.Complete();
		}
		this.questManager.HandleQuestProgressChanged(false);
	}

	private void Complete()
	{
		if (this.isQuestComplete)
		{
			return;
		}
		this.isQuestComplete = true;
		this.RemoveEventListener();
		this.questManager.HandleQuestCompleted(this.questID);
	}

	public string GetTextDescription()
	{
		return this.<GetTextDescription>g__GetActionName|32_0().ToUpper() + this.<GetTextDescription>g__GetLocationText|32_1().ToUpper();
	}

	public string GetProgressText()
	{
		if (!this.isQuestComplete)
		{
			return string.Format("{0}/{1}", this.occurenceCount, this.requiredOccurenceCount);
		}
		return "[DONE]";
	}

	[CompilerGenerated]
	private string <GetTextDescription>g__GetActionName|32_0()
	{
		switch (this.questType)
		{
		case QuestType.none:
			return "[UNDEFINED]";
		case QuestType.gameModeObjective:
			return this.questName;
		case QuestType.gameModeRound:
			return this.questName;
		case QuestType.grabObject:
			return this.questName;
		case QuestType.dropObject:
			return this.questName;
		case QuestType.eatObject:
			return this.questName;
		case QuestType.launchedProjectile:
			return this.questName;
		case QuestType.moveDistance:
			return this.questName;
		case QuestType.swimDistance:
			return this.questName;
		case QuestType.triggerHandEffect:
			return this.questName;
		case QuestType.enterLocation:
			return this.questName;
		case QuestType.misc:
			return this.questName;
		}
		return this.questName;
	}

	[CompilerGenerated]
	private string <GetTextDescription>g__GetLocationText|32_1()
	{
		if (this.RequiredZone == GTZone.none)
		{
			return "";
		}
		return string.Format(" IN {0}", this.RequiredZone);
	}

	public bool disable;

	public int questID;

	public float weight = 1f;

	public QuestCategory category;

	public string questName = "UNNAMED QUEST";

	public QuestType questType;

	public string questOccurenceFilter;

	public int requiredOccurenceCount = 1;

	[JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
	public List<GTZone> requiredZones;

	[Space]
	[NonSerialized]
	public bool isQuestActive;

	[NonSerialized]
	public bool isQuestComplete;

	[NonSerialized]
	public bool isDailyQuest;

	[NonSerialized]
	public int lastChange;

	[NonSerialized]
	public int occurenceCount;

	private float moveDistance;

	[NonSerialized]
	public GorillaQuestManager questManager;
}
