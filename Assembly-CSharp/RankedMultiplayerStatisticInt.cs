using System;
using UnityEngine;

[Serializable]
public class RankedMultiplayerStatisticInt : RankedMultiplayerStatistic
{
	public RankedMultiplayerStatisticInt(string n, int val, int min = 0, int max = 2147483647, RankedMultiplayerStatistic.SerializationType s = RankedMultiplayerStatistic.SerializationType.None)
		: base(n, s)
	{
		this.intValue = val;
		this.minValue = min;
		this.maxValue = max;
	}

	public static implicit operator int(RankedMultiplayerStatisticInt stat)
	{
		if (stat.IsValid)
		{
			return stat.intValue;
		}
		Debug.LogError("Attempting to retrieve value for user data that does not yet have a valid key: " + stat.name);
		return 0;
	}

	public void Set(int val)
	{
		this.intValue = Mathf.Clamp(val, this.minValue, this.maxValue);
		this.Save();
	}

	public int Get()
	{
		return this.intValue;
	}

	public override bool TrySetValue(string valAsString)
	{
		int num;
		bool flag = int.TryParse(valAsString, out num);
		if (flag)
		{
			this.intValue = Mathf.Clamp(num, this.minValue, this.maxValue);
		}
		return flag;
	}

	public void Increment()
	{
		this.AddTo(1);
	}

	public void AddTo(int amount)
	{
		this.intValue += amount;
		this.intValue = Mathf.Clamp(this.intValue, this.minValue, this.maxValue);
		this.Save();
	}

	protected override void Save()
	{
		RankedMultiplayerStatistic.SerializationType serializationType = this.serializationType;
		if (serializationType != RankedMultiplayerStatistic.SerializationType.Mothership && serializationType == RankedMultiplayerStatistic.SerializationType.PlayerPrefs)
		{
			PlayerPrefs.SetInt(this.name, this.intValue);
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
				this.intValue = PlayerPrefs.GetInt(this.name, this.intValue);
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
		return this.intValue.ToString();
	}

	private int intValue;

	private int minValue;

	private int maxValue;
}
