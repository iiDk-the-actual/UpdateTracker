using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class CallLimitersList<Titem, Tenum> where Titem : CallLimiter, new() where Tenum : Enum
{
	public bool IsSpamming(Tenum index)
	{
		return this.IsSpamming((int)((object)index));
	}

	public bool IsSpamming(int index)
	{
		return !this.m_callLimiters[index].CheckCallTime(Time.unscaledTime);
	}

	public bool IsSpamming(Tenum index, double serverTime)
	{
		return this.IsSpamming((int)((object)index), serverTime);
	}

	public bool IsSpamming(int index, double serverTime)
	{
		return !this.m_callLimiters[index].CheckCallServerTime(serverTime);
	}

	public void Reset()
	{
		Titem[] callLimiters = this.m_callLimiters;
		for (int i = 0; i < callLimiters.Length; i++)
		{
			callLimiters[i].Reset();
		}
	}

	[RequiredListLength("GetMaxLength")]
	[SerializeField]
	private Titem[] m_callLimiters;
}
