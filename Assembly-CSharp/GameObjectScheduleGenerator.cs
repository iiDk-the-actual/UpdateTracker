using System;
using System.Globalization;
using GameObjectScheduling;
using UnityEngine;

[CreateAssetMenu(fileName = "New Game Object Schedule Generator", menuName = "Game Object Scheduling/Game Object Schedule Generator")]
public class GameObjectScheduleGenerator : ScriptableObject
{
	private void GenerateSchedule()
	{
		DateTime dateTime;
		try
		{
			dateTime = DateTime.Parse(this.scheduleStart, CultureInfo.InvariantCulture);
		}
		catch
		{
			Debug.LogError("Don't understand Start Date " + this.scheduleStart);
			return;
		}
		DateTime dateTime2;
		try
		{
			dateTime2 = DateTime.Parse(this.scheduleEnd, CultureInfo.InvariantCulture);
		}
		catch
		{
			Debug.LogError("Don't understand End Date " + this.scheduleEnd);
			return;
		}
		if (this.scheduleType == GameObjectScheduleGenerator.ScheduleType.DailyShuffle)
		{
			GameObjectSchedule.GenerateDailyShuffle(dateTime, dateTime2, this.schedules);
		}
	}

	[SerializeField]
	private GameObjectSchedule[] schedules;

	[SerializeField]
	private string scheduleStart = "1/1/0001 00:00:00";

	[SerializeField]
	private string scheduleEnd = "1/1/0001 00:00:00";

	[SerializeField]
	private GameObjectScheduleGenerator.ScheduleType scheduleType;

	private enum ScheduleType
	{
		DailyShuffle
	}
}
