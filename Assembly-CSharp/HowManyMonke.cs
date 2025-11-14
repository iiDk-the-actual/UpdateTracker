using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using GorillaNetworking;
using Newtonsoft.Json;
using PlayFab;
using UnityEngine;
using UnityEngine.Networking;

public class HowManyMonke : MonoBehaviour
{
	public static float RecheckDelay
	{
		get
		{
			return Mathf.Max((float)HowManyMonke.recheckDelay / 1000f, 1f);
		}
	}

	public async void Start()
	{
		this.state = HowManyMonke.State.READY;
		await Task.Delay(1000);
		Debug.Log(string.Format("Checking NetworkSystem.Instance: {0}", NetworkSystem.Instance));
		while (NetworkSystem.Instance == null)
		{
			await Task.Delay(1000);
			Debug.Log(string.Format("Re-Checking NetworkSystem.Instance: {0}", NetworkSystem.Instance));
		}
		TaskAwaiter<int> taskAwaiter = this.FetchThisMany().GetAwaiter();
		TaskAwaiter<int> taskAwaiter2;
		if (!taskAwaiter.IsCompleted)
		{
			await taskAwaiter;
			taskAwaiter = taskAwaiter2;
			taskAwaiter2 = default(TaskAwaiter<int>);
		}
		HowManyMonke.ThisMany = taskAwaiter.GetResult();
		if (HowManyMonke.OnCheck != null)
		{
			HowManyMonke.OnCheck(HowManyMonke.ThisMany);
		}
		Debug.Log(string.Format("Fetch Complete: {0}", HowManyMonke.ThisMany));
		await this.FetchRecheckDelay();
		while (Application.isPlaying && HowManyMonke.recheckDelay > 0)
		{
			await Task.Delay(HowManyMonke.recheckDelay);
			if (HowManyMonke.OnCheck != null)
			{
				taskAwaiter = this.FetchThisMany().GetAwaiter();
				if (!taskAwaiter.IsCompleted)
				{
					await taskAwaiter;
					taskAwaiter = taskAwaiter2;
					taskAwaiter2 = default(TaskAwaiter<int>);
				}
				HowManyMonke.ThisMany = taskAwaiter.GetResult();
				HowManyMonke.OnCheck(HowManyMonke.ThisMany);
				await this.FetchRecheckDelay();
			}
		}
	}

	private async Task FetchRecheckDelay()
	{
		this.state = HowManyMonke.State.TD_LOOKUP;
		PlayFabTitleDataCache.Instance.GetTitleData(this.titleDataKey, new Action<string>(this.onTD), new Action<PlayFabError>(this.onTDError), false);
		while (this.state != HowManyMonke.State.READY)
		{
			await Task.Yield();
		}
	}

	private void onTDError(PlayFabError error)
	{
		this.state = HowManyMonke.State.READY;
		HowManyMonke.recheckDelay = 0;
	}

	private void onTD(string obj)
	{
		this.state = HowManyMonke.State.READY;
		if (int.TryParse(obj, out HowManyMonke.recheckDelay))
		{
			HowManyMonke.recheckDelay *= 1000;
			return;
		}
		HowManyMonke.recheckDelay = 0;
	}

	private async Task<int> FetchThisMany()
	{
		int num;
		if (HowManyMonke.recheckDelay < 0)
		{
			num = NetworkSystem.Instance.GlobalPlayerCount();
		}
		else
		{
			UnityWebRequest request = new UnityWebRequest(PlayFabAuthenticatorSettings.ModerationApiBaseUrl + this.CCUEndpoint, "POST");
			request.downloadHandler = new DownloadHandlerBuffer();
			await request.SendWebRequest();
			if (request.result == UnityWebRequest.Result.Success)
			{
				num = JsonConvert.DeserializeObject<HowManyMonke.CCUResponse>(request.downloadHandler.text).CCUTotal;
			}
			else
			{
				num = NetworkSystem.Instance.GlobalPlayerCount();
			}
		}
		return num;
	}

	public static int ThisMany = 12549;

	public static Action<int> OnCheck;

	[SerializeField]
	private string titleDataKey;

	private HowManyMonke.State state;

	private static int recheckDelay;

	[SerializeField]
	private string CCUEndpoint;

	private enum State
	{
		READY,
		TD_LOOKUP,
		HMM_LOOKUP
	}

	private class CCUResponse
	{
		public int CCUTotal;

		public string ErrorMessage;
	}
}
