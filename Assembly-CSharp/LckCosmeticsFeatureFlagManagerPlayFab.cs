using System;
using System.Threading.Tasks;
using GorillaNetworking;
using Liv.Lck;
using PlayFab;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class LckCosmeticsFeatureFlagManagerPlayFab : ILckCosmeticsFeatureFlagManager
{
	[Preserve]
	public LckCosmeticsFeatureFlagManagerPlayFab()
	{
	}

	public Task<bool> IsEnabledAsync()
	{
		if (this._initializationTask != null)
		{
			return this._initializationTask;
		}
		object @lock = this._lock;
		Task<bool> task2;
		lock (@lock)
		{
			Task<bool> task;
			if ((task = this._initializationTask) == null)
			{
				task2 = (this._initializationTask = this.GetEnabledStateWithRetryAsync());
				task = task2;
			}
			task2 = task;
		}
		return task2;
	}

	private async Task<bool> GetEnabledStateWithRetryAsync()
	{
		for (int i = 0; i < 2; i++)
		{
			if (!(PlayFabTitleDataCache.Instance == null))
			{
				TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
				PlayFabTitleDataCache.Instance.GetTitleData("EnableLckCosmetics", delegate(string data)
				{
					bool flag;
					if (bool.TryParse(data, out flag))
					{
						Debug.Log(string.Format("LCK: Feature flag '{0}' is set to '{1}'.", "EnableLckCosmetics", flag));
						tcs.TrySetResult(flag);
						return;
					}
					Debug.LogError("LCK: Failed to parse feature flag 'EnableLckCosmetics' from value '" + data + "'. Defaulting to 'true'.");
					tcs.TrySetResult(true);
				}, delegate(PlayFabError error)
				{
					Debug.LogError("LCK: Error fetching feature flag 'EnableLckCosmetics': " + error.ErrorMessage + ". Defaulting to 'true'.");
					tcs.TrySetResult(true);
				}, false);
				return await tcs.Task;
			}
			Debug.LogWarning("LCK: PlayFabTitleDataCache instance is not available. " + string.Format("Retrying feature flag check in {0} seconds... (Attempt {1}/{2})", 5, i + 1, 2));
			await Task.Delay(5000);
		}
		Debug.LogError(string.Format("LCK: {0} instance was not available after {1} attempts. ", "PlayFabTitleDataCache", 2) + "Cosmetics feature will be enabled by default as a fallback measure.");
		return true;
	}

	private const string TitleDataKey = "EnableLckCosmetics";

	private const int MaxRetries = 2;

	private const int RetryDelayMilliseconds = 5000;

	private Task<bool> _initializationTask;

	private readonly object _lock = new object();
}
