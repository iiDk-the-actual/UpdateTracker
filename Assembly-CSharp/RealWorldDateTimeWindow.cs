using System;
using UniLabs.Time;
using UnityEngine;

public class RealWorldDateTimeWindow : ScriptableObject
{
	public bool MatchesDate(DateTime utcDate)
	{
		return this.startTime <= utcDate && this.endTime >= utcDate;
	}

	[SerializeField]
	private UDateTime startTime;

	[SerializeField]
	private UDateTime endTime;
}
