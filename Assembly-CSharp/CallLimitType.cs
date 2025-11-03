using System;

[Serializable]
public class CallLimitType<T> where T : CallLimiter
{
	public static implicit operator CallLimitType<CallLimiter>(CallLimitType<T> clt)
	{
		return new CallLimitType<CallLimiter>
		{
			Key = clt.Key,
			UseNetWorkTime = clt.UseNetWorkTime,
			CallLimitSettings = clt.CallLimitSettings
		};
	}

	public FXType Key;

	public bool UseNetWorkTime;

	public T CallLimitSettings;
}
