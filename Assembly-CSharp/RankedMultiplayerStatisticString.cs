using System;
using UnityEngine;

[Serializable]
public class RankedMultiplayerStatisticString : RankedMultiplayerStatistic
{
	public RankedMultiplayerStatisticString(string n, string val, RankedMultiplayerStatistic.SerializationType s = RankedMultiplayerStatistic.SerializationType.None)
		: base(n, s)
	{
		this.stringValue = val;
	}

	public static implicit operator string(RankedMultiplayerStatisticString stat)
	{
		if (stat.IsValid)
		{
			return stat.stringValue;
		}
		Debug.LogError("Attempting to retrieve value for user data that does not yet have a valid key: " + stat.name);
		return string.Empty;
	}

	public void Set(string val)
	{
		this.stringValue = val;
		this.Save();
	}

	public string Get()
	{
		return this.stringValue;
	}

	public override bool TrySetValue(string valAsString)
	{
		this.stringValue = valAsString;
		return true;
	}

	protected override void Save()
	{
		RankedMultiplayerStatistic.SerializationType serializationType = this.serializationType;
		if (serializationType != RankedMultiplayerStatistic.SerializationType.Mothership && serializationType == RankedMultiplayerStatistic.SerializationType.PlayerPrefs)
		{
			PlayerPrefs.SetString(this.name, this.stringValue);
			PlayerPrefs.Save();
		}
	}

	public override void Load()
	{
		RankedMultiplayerStatistic.SerializationType serializationType = this.serializationType;
		if (serializationType != RankedMultiplayerStatistic.SerializationType.Mothership)
		{
			if (serializationType == RankedMultiplayerStatistic.SerializationType.PlayerPrefs)
			{
				base.IsValid = true;
				this.stringValue = PlayerPrefs.GetString(this.name, this.stringValue);
				return;
			}
		}
		else
		{
			base.IsValid = false;
		}
	}

	public override string ToString()
	{
		return this.stringValue;
	}

	private string stringValue;
}
