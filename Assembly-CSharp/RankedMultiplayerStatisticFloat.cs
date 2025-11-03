using System;
using UnityEngine;

[Serializable]
public class RankedMultiplayerStatisticFloat : RankedMultiplayerStatistic
{
	public RankedMultiplayerStatisticFloat(string n, float val, float min = 0f, float max = 3.4028235E+38f, RankedMultiplayerStatistic.SerializationType s = RankedMultiplayerStatistic.SerializationType.None)
		: base(n, s)
	{
		this.floatValue = val;
		this.minValue = min;
		this.maxValue = max;
	}

	public static implicit operator float(RankedMultiplayerStatisticFloat stat)
	{
		if (stat.IsValid)
		{
			return stat.floatValue;
		}
		Debug.LogError("Attempting to retrieve value for user data that does not yet have a valid key: " + stat.name);
		return 0f;
	}

	public void Set(float val)
	{
		this.floatValue = Mathf.Clamp(val, this.minValue, this.maxValue);
		this.Save();
	}

	public float Get()
	{
		return this.floatValue;
	}

	public override bool TrySetValue(string valAsString)
	{
		float num;
		bool flag = float.TryParse(valAsString, out num);
		if (flag)
		{
			this.floatValue = Mathf.Clamp(num, this.minValue, this.maxValue);
		}
		return flag;
	}

	public void Increment()
	{
		this.AddTo(1f);
	}

	public void AddTo(float amount)
	{
		this.floatValue += amount;
		this.floatValue = Mathf.Clamp(this.floatValue, this.minValue, this.maxValue);
		this.Save();
	}

	protected override void Save()
	{
		RankedMultiplayerStatistic.SerializationType serializationType = this.serializationType;
		if (serializationType != RankedMultiplayerStatistic.SerializationType.Mothership && serializationType == RankedMultiplayerStatistic.SerializationType.PlayerPrefs)
		{
			PlayerPrefs.SetFloat(this.name, this.floatValue);
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
				this.floatValue = PlayerPrefs.GetFloat(this.name, this.floatValue);
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
		return this.floatValue.ToString();
	}

	private float floatValue;

	private float minValue;

	private float maxValue;
}
