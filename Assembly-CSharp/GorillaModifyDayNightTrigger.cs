using System;

public class GorillaModifyDayNightTrigger : GorillaTriggerBox
{
	public override void OnBoxTriggered()
	{
		base.OnBoxTriggered();
		if (this.clearModifiedTime)
		{
			BetterDayNightManager.instance.currentSetting = TimeSettings.Normal;
		}
		else
		{
			int num = this.timeOfDayIndex % BetterDayNightManager.instance.timeOfDayRange.Length;
			BetterDayNightManager.instance.SetTimeOfDay(this.timeOfDayIndex);
			BetterDayNightManager.instance.SetOverrideIndex(this.timeOfDayIndex);
		}
		if (this.setFixedWeather)
		{
			BetterDayNightManager.instance.SetFixedWeather(this.fixedWeather);
			return;
		}
		BetterDayNightManager.instance.ClearFixedWeather();
	}

	public bool clearModifiedTime;

	public int timeOfDayIndex;

	public bool setFixedWeather;

	public BetterDayNightManager.WeatherType fixedWeather;
}
