using System;
using System.Globalization;
using UnityEngine;

[Serializable]
public struct GTDateTimeSerializable : ISerializationCallbackReceiver
{
	public DateTime dateTime
	{
		get
		{
			return this._dateTime;
		}
		set
		{
			this._dateTime = value;
			this._dateTimeString = GTDateTimeSerializable.FormatDateTime(this._dateTime);
		}
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
		this._dateTimeString = GTDateTimeSerializable.FormatDateTime(this._dateTime);
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		DateTime dateTime;
		if (GTDateTimeSerializable.TryParseDateTime(this._dateTimeString, out dateTime))
		{
			this._dateTime = dateTime;
		}
	}

	public GTDateTimeSerializable(int dummyValue)
	{
		DateTime now = DateTime.Now;
		this._dateTime = new DateTime(now.Year, now.Month, now.Day, 11, 0, 0);
		this._dateTimeString = GTDateTimeSerializable.FormatDateTime(this._dateTime);
	}

	private static string FormatDateTime(DateTime dateTime)
	{
		return dateTime.ToString("yyyy-MM-dd HH:mm");
	}

	private static bool TryParseDateTime(string value, out DateTime result)
	{
		if (DateTime.TryParseExact(value, new string[] { "yyyy-MM-dd HH:mm", "yyyy-MM-dd", "yyyy-MM" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
		{
			DateTime dateTime = result;
			if (dateTime.Hour == 0 && dateTime.Minute == 0)
			{
				result = result.AddHours(11.0);
			}
			return true;
		}
		return false;
	}

	[HideInInspector]
	[SerializeField]
	private string _dateTimeString;

	private DateTime _dateTime;
}
