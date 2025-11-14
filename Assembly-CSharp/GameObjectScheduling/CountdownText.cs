using System;
using System.Collections;
using System.Globalization;
using System.Runtime.CompilerServices;
using GorillaNetworking;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.SmartFormat.PersistentVariables;

namespace GameObjectScheduling
{
	public class CountdownText : MonoBehaviour
	{
		private bool ShouldLocalize
		{
			get
			{
				return this.shouldLocalize && (this._locTextComp != null && this._countdownLocStr != null && this._timeCountdownVar != null && this._timescaleCountdownVar != null) && this._isValidVar != null;
			}
		}

		public CountdownTextDate Countdown
		{
			get
			{
				return this.CountdownTo;
			}
			set
			{
				this.CountdownTo = value;
				if (this.CountdownTo.FormatString.Length > 0)
				{
					this.displayTextFormat = this.CountdownTo.FormatString;
				}
				this.displayText.text = this.CountdownTo.DefaultString;
				if (base.gameObject.activeInHierarchy && !this.useExternalTime && this.monitor == null && this.CountdownTo != null)
				{
					this.monitor = base.StartCoroutine(this.MonitorTime());
				}
			}
		}

		private void Awake()
		{
			this.displayText = base.GetComponent<TMP_Text>();
			this.displayTextFormat = string.Empty;
			this.displayText.text = string.Empty;
			if (this.CountdownTo == null)
			{
				return;
			}
			if (this.displayTextFormat.Length == 0 && this.CountdownTo.FormatString.Length > 0)
			{
				this.displayTextFormat = this.CountdownTo.FormatString;
			}
			this.displayText.text = this.CountdownTo.DefaultString;
			if (!this.shouldLocalize)
			{
				return;
			}
			this._locTextComp = base.GetComponent<LocalizedText>();
			if (this._locTextComp == null)
			{
				Debug.LogError("[LOCALIZATION::COUNTDOWN_TEXT] There is no [LocalizedText] component on [" + base.name + "]!", this);
				return;
			}
			this._countdownLocStr = this._locTextComp.StringReference;
			if (this._locTextComp.StringReference == null || this._locTextComp.StringReference.IsEmpty)
			{
				Debug.LogError("[LOCALIZATION::COUNTDOWN_TEXT] There is no [StringReference] assigned on [" + base.name + "]!", this);
				return;
			}
			this._timeCountdownVar = this._countdownLocStr["time-value"] as IntVariable;
			this._timescaleCountdownVar = this._countdownLocStr["timescale-index"] as IntVariable;
			this._isValidVar = this._countdownLocStr["is-valid"] as BoolVariable;
		}

		private void OnEnable()
		{
			if (this.CountdownTo == null)
			{
				return;
			}
			if (this.monitor == null && !this.useExternalTime)
			{
				this.monitor = base.StartCoroutine(this.MonitorTime());
			}
		}

		private void OnDisable()
		{
			this.StopMonitorTime();
			this.StopDisplayRefresh();
		}

		private IEnumerator MonitorTime()
		{
			while (GorillaComputer.instance == null || GorillaComputer.instance.startupMillis == 0L)
			{
				yield return null;
			}
			this.monitor = null;
			this.targetTime = this.TryParseDateTime();
			if (this.updateDisplay)
			{
				this.StartDisplayRefresh();
			}
			else
			{
				this.RefreshDisplay();
			}
			yield break;
		}

		private IEnumerator MonitorExternalTime(DateTime countdown)
		{
			while (GorillaComputer.instance == null || GorillaComputer.instance.startupMillis == 0L)
			{
				yield return null;
			}
			this.monitor = null;
			this.targetTime = countdown;
			if (this.updateDisplay)
			{
				this.StartDisplayRefresh();
			}
			else
			{
				this.RefreshDisplay();
			}
			yield break;
		}

		private void StopMonitorTime()
		{
			if (this.monitor != null)
			{
				base.StopCoroutine(this.monitor);
			}
			this.monitor = null;
		}

		public void SetCountdownTime(DateTime countdown)
		{
			this.StopMonitorTime();
			this.StopDisplayRefresh();
			this.monitor = base.StartCoroutine(this.MonitorExternalTime(countdown));
		}

		public void SetFixedText(string text)
		{
			this.StopMonitorTime();
			this.StopDisplayRefresh();
			this.displayText.text = text;
		}

		private void StartDisplayRefresh()
		{
			this.StopDisplayRefresh();
			this.displayRefresh = base.StartCoroutine(this.WaitForDisplayRefresh());
		}

		private void StopDisplayRefresh()
		{
			if (this.displayRefresh != null)
			{
				base.StopCoroutine(this.displayRefresh);
			}
			this.displayRefresh = null;
		}

		private IEnumerator WaitForDisplayRefresh()
		{
			for (;;)
			{
				this.RefreshDisplay();
				TimeSpan timeSpan;
				if (this.countdownTime.Days > 0)
				{
					timeSpan = this.countdownTime - TimeSpan.FromDays((double)this.countdownTime.Days);
				}
				else if (this.countdownTime.Hours > 0)
				{
					timeSpan = this.countdownTime - TimeSpan.FromHours((double)this.countdownTime.Hours);
				}
				else if (this.countdownTime.Minutes > 0)
				{
					timeSpan = this.countdownTime - TimeSpan.FromMinutes((double)this.countdownTime.Minutes);
				}
				else
				{
					if (this.countdownTime.Seconds <= 0)
					{
						break;
					}
					timeSpan = this.countdownTime - TimeSpan.FromSeconds((double)this.countdownTime.Seconds);
				}
				yield return new WaitForSeconds((float)timeSpan.TotalSeconds);
			}
			yield break;
		}

		private void RefreshDisplay()
		{
			this.countdownTime = this.targetTime.Subtract(GorillaComputer.instance.GetServerTime());
			ValueTuple<string, int, int, bool> timeDisplay = CountdownText.GetTimeDisplay(this.countdownTime, this.displayTextFormat, this.CountdownTo.DaysThreshold, string.Empty, this.CountdownTo.DefaultString);
			string item = timeDisplay.Item1;
			int item2 = timeDisplay.Item2;
			int item3 = timeDisplay.Item3;
			bool item4 = timeDisplay.Item4;
			if (!this.ShouldLocalize)
			{
				this.displayText.text = item;
				return;
			}
			this._timescaleCountdownVar.Value = item2;
			this._timeCountdownVar.Value = item3;
			this._isValidVar.Value = item4;
		}

		public static string GetTimeDisplay(TimeSpan ts, string format)
		{
			return CountdownText.GetTimeDisplay(ts, format, int.MaxValue, string.Empty, string.Empty).Item1;
		}

		[return: TupleElementNames(new string[] { "msg", "timescaleVar", "countdownVar", "valid" })]
		public static ValueTuple<string, int, int, bool> GetTimeDisplay(TimeSpan ts, string format, int maxDaysToDisplay, string elapsedString, string overMaxString)
		{
			string text = overMaxString;
			int num = 0;
			int num2 = ts.Days;
			bool flag = false;
			if (ts.TotalSeconds < 0.0)
			{
				return new ValueTuple<string, int, int, bool>(elapsedString, num, num2, flag);
			}
			if (ts.TotalDays < (double)maxDaysToDisplay)
			{
				if (ts.Days > 0)
				{
					num = 3;
					num2 = ts.Days;
					flag = true;
					text = string.Format(format, ts.Days, CountdownText.getTimeChunkString(CountdownText.TimeChunk.DAY, ts.Days));
				}
				else if (ts.Hours > 0)
				{
					num = 2;
					num2 = ts.Hours;
					flag = true;
					text = string.Format(format, ts.Hours, CountdownText.getTimeChunkString(CountdownText.TimeChunk.HOUR, ts.Hours));
				}
				else if (ts.Minutes > 0)
				{
					num = 1;
					num2 = ts.Minutes;
					flag = true;
					text = string.Format(format, ts.Minutes, CountdownText.getTimeChunkString(CountdownText.TimeChunk.MINUTE, ts.Minutes));
				}
				else if (ts.Seconds > 0)
				{
					num = 0;
					num2 = ts.Seconds;
					flag = true;
					text = string.Format(format, ts.Seconds, CountdownText.getTimeChunkString(CountdownText.TimeChunk.SECOND, ts.Seconds));
				}
			}
			return new ValueTuple<string, int, int, bool>(text, num, num2, flag);
		}

		private static string getTimeChunkString(CountdownText.TimeChunk chunk, int n)
		{
			switch (chunk)
			{
			case CountdownText.TimeChunk.DAY:
				if (n == 1)
				{
					return "DAY";
				}
				return "DAYS";
			case CountdownText.TimeChunk.HOUR:
				if (n == 1)
				{
					return "HOUR";
				}
				return "HOURS";
			case CountdownText.TimeChunk.MINUTE:
				if (n == 1)
				{
					return "MINUTE";
				}
				return "MINUTES";
			case CountdownText.TimeChunk.SECOND:
				if (n == 1)
				{
					return "SECOND";
				}
				return "SECONDS";
			default:
				return string.Empty;
			}
		}

		private DateTime TryParseDateTime()
		{
			DateTime dateTime;
			try
			{
				dateTime = DateTime.Parse(this.CountdownTo.CountdownTo, CultureInfo.InvariantCulture);
			}
			catch
			{
				dateTime = DateTime.MinValue;
			}
			return dateTime;
		}

		[SerializeField]
		private CountdownTextDate CountdownTo;

		[SerializeField]
		private bool updateDisplay;

		[SerializeField]
		private bool useExternalTime;

		[SerializeField]
		private bool shouldLocalize = true;

		private TMP_Text displayText;

		private string displayTextFormat;

		private DateTime targetTime;

		private TimeSpan countdownTime;

		private Coroutine monitor;

		private Coroutine displayRefresh;

		private LocalizedText _locTextComp;

		private LocalizedString _countdownLocStr;

		private IntVariable _timeCountdownVar;

		private IntVariable _timescaleCountdownVar;

		private BoolVariable _isValidVar;

		private enum TimeChunk
		{
			DAY,
			HOUR,
			MINUTE,
			SECOND
		}
	}
}
