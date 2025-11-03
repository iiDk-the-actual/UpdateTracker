using System;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/FXSystemSettings", order = 2)]
public class FXSystemSettings : ScriptableObject
{
	public void Awake()
	{
		int num = ((this.callLimits != null) ? this.callLimits.Length : 0);
		int num2 = ((this.CallLimitsCooldown != null) ? this.CallLimitsCooldown.Length : 0);
		for (int i = 0; i < num; i++)
		{
			FXType fxtype = this.callLimits[i].Key;
			int num3 = (int)fxtype;
			if (num3 < 0 || num3 >= 24)
			{
				string text = "NO_PATH_AT_RUNTIME";
				Debug.LogError("FXSystemSettings: (this should never happen) `callLimits.Key` is out of bounds of `callSettings`! Path=\"" + text + "\"", this);
			}
			if (this.callSettings[num3] != null)
			{
				Debug.Log("FXSystemSettings: call setting for " + fxtype.ToString() + " already exists, skipping.");
			}
			else
			{
				this.callSettings[num3] = this.callLimits[i];
			}
		}
		for (int i = 0; i < num2; i++)
		{
			FXType fxtype = this.CallLimitsCooldown[i].Key;
			int num3 = (int)fxtype;
			if (this.callSettings[num3] != null)
			{
				Debug.Log("FXSystemSettings: call setting for " + fxtype.ToString() + " already exists, skipping");
			}
			else
			{
				this.callSettings[num3] = this.CallLimitsCooldown[i];
			}
		}
		for (int i = 0; i < this.callSettings.Length; i++)
		{
			if (this.callSettings[i] == null)
			{
				this.callSettings[i] = new LimiterType
				{
					CallLimitSettings = new CallLimiter(0, 0f, 0f),
					Key = (FXType)i
				};
			}
		}
	}

	private const string preLog = "FXSystemSettings: ";

	private const string preErr = "ERROR!!!  FXSystemSettings: ";

	[SerializeField]
	private LimiterType[] callLimits;

	[SerializeField]
	private CooldownType[] CallLimitsCooldown;

	[NonSerialized]
	public bool forLocalRig;

	[NonSerialized]
	public CallLimitType<CallLimiter>[] callSettings = new CallLimitType<CallLimiter>[24];
}
