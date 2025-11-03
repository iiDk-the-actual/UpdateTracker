using System;
using UnityEngine;

[Serializable]
public class CallLimiterWithCooldown : CallLimiter
{
	public CallLimiterWithCooldown(float coolDownSpam, int historyLength, float coolDown)
		: base(historyLength, coolDown, 0.5f)
	{
		this.spamCoolDown = coolDownSpam;
	}

	public CallLimiterWithCooldown(float coolDownSpam, int historyLength, float coolDown, float latencyMax)
		: base(historyLength, coolDown, latencyMax)
	{
		this.spamCoolDown = coolDownSpam;
	}

	public override bool CheckCallTime(float time)
	{
		if (this.blockCall && time < this.blockStartTime + this.spamCoolDown)
		{
			this.blockStartTime = time;
			return false;
		}
		return base.CheckCallTime(time);
	}

	[SerializeField]
	private float spamCoolDown;
}
