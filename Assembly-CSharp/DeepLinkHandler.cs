using System;
using System.Collections;
using GorillaNetworking;
using Oculus.Platform;
using Oculus.Platform.Models;
using UnityEngine;
using UnityEngine.Networking;

public class DeepLinkHandler : MonoBehaviour
{
	public void Awake()
	{
		if (DeepLinkHandler.instance == null)
		{
			DeepLinkHandler.instance = this;
			return;
		}
		if (DeepLinkHandler.instance != this)
		{
			Object.Destroy(this);
		}
	}

	public static void Initialize(GameObject parent)
	{
		if (DeepLinkHandler.instance == null && parent != null)
		{
			parent.AddComponent<DeepLinkHandler>();
		}
		if (DeepLinkHandler.instance == null)
		{
			return;
		}
		DeepLinkHandler.instance.RefreshLaunchDetails();
		if (DeepLinkHandler.instance.cachedLaunchDetails != null && DeepLinkHandler.instance.cachedLaunchDetails.LaunchType == LaunchType.Deeplink)
		{
			DeepLinkHandler.instance.HandleDeepLink();
			return;
		}
		Object.Destroy(DeepLinkHandler.instance);
	}

	private void RefreshLaunchDetails()
	{
		if (global::UnityEngine.Application.platform != RuntimePlatform.Android)
		{
			GTDev.Log<string>("[DeepLinkHandler::RefreshLaunchDetails] Not on Android Platform!", null);
			return;
		}
		this.cachedLaunchDetails = ApplicationLifecycle.GetLaunchDetails();
		GTDev.Log<string>(string.Concat(new string[]
		{
			"[DeepLinkHandler::RefreshLaunchDetails] LaunchType: ",
			this.cachedLaunchDetails.LaunchType.ToString(),
			"\n[DeepLinkHandler::RefreshLaunchDetails] LaunchSource: ",
			this.cachedLaunchDetails.LaunchSource,
			"\n[DeepLinkHandler::RefreshLaunchDetails] DeepLinkMessage: ",
			this.cachedLaunchDetails.DeeplinkMessage
		}), null);
	}

	private static IEnumerator ProcessWebRequest(string url, string data, string contentType, Action<UnityWebRequest> callback)
	{
		UnityWebRequest request = UnityWebRequest.Post(url, data, contentType);
		yield return request.SendWebRequest();
		callback(request);
		yield break;
	}

	private void HandleDeepLink()
	{
		GTDev.Log<string>("[DeepLinkHandler::HandleDeepLink] Handling deep link...", null);
		if (this.cachedLaunchDetails.LaunchSource.Contains("7221491444554579"))
		{
			GTDev.Log<string>("[DeepLinkHandler::HandleDeepLink] DeepLink received from Witchblood, processing...", null);
			string text = JsonUtility.ToJson(new DeepLinkHandler.CollabRequest
			{
				itemGUID = this.cachedLaunchDetails.DeeplinkMessage,
				launchSource = this.cachedLaunchDetails.LaunchSource,
				oculusUserID = PlayFabAuthenticator.instance.userID,
				playFabID = PlayFabAuthenticator.instance.GetPlayFabPlayerId(),
				playFabSessionTicket = PlayFabAuthenticator.instance.GetPlayFabSessionTicket(),
				mothershipId = MothershipClientContext.MothershipId,
				mothershipToken = MothershipClientContext.Token,
				mothershipEnvId = MothershipClientApiUnity.EnvironmentId
			});
			GTDev.Log<string>("[DeepLinkHandler::HandleDeepLink] Web Request body: \n" + text, null);
			base.StartCoroutine(DeepLinkHandler.ProcessWebRequest(PlayFabAuthenticatorSettings.HpPromoApiBaseUrl + "/api/ConsumeItem", text, "application/json", new Action<UnityWebRequest>(this.OnWitchbloodCollabResponse)));
			return;
		}
		if (this.cachedLaunchDetails.LaunchSource.Contains("1903584373052985"))
		{
			GTDev.Log<string>("[DeepLinkHandler::HandleDeepLink] DeepLink received from Racoon Lagoon, processing...", null);
			string text2 = JsonUtility.ToJson(new DeepLinkHandler.CollabRequest
			{
				itemGUID = this.cachedLaunchDetails.DeeplinkMessage,
				launchSource = this.cachedLaunchDetails.LaunchSource,
				oculusUserID = PlayFabAuthenticator.instance.userID,
				playFabID = PlayFabAuthenticator.instance.GetPlayFabPlayerId(),
				playFabSessionTicket = PlayFabAuthenticator.instance.GetPlayFabSessionTicket(),
				mothershipId = MothershipClientContext.MothershipId,
				mothershipToken = MothershipClientContext.Token,
				mothershipEnvId = MothershipClientApiUnity.EnvironmentId
			});
			GTDev.Log<string>("[DeepLinkHandler::HandleDeepLink] Web Request body: \n" + text2, null);
			base.StartCoroutine(DeepLinkHandler.ProcessWebRequest(PlayFabAuthenticatorSettings.HpPromoApiBaseUrl + "/api/ConsumeItem", text2, "application/json", new Action<UnityWebRequest>(this.OnRaccoonLagoonCollabResponse)));
			return;
		}
		GTDev.LogError<string>("[DeepLinkHandler::HandleDeepLink] App launched via DeepLink, but from an unknown app. App ID: " + this.cachedLaunchDetails.LaunchSource, null);
		Object.Destroy(this);
	}

	private void OnWitchbloodCollabResponse(UnityWebRequest completedRequest)
	{
		if (completedRequest.result != UnityWebRequest.Result.Success)
		{
			GTDev.LogError<string>("[DeepLinkHandler::OnWitchbloodCollabResponse] Web Request failed: " + completedRequest.error + "\nDetails: " + completedRequest.downloadHandler.text, null);
			Object.Destroy(this);
			return;
		}
		if (completedRequest.downloadHandler.text.Contains("AlreadyRedeemed", StringComparison.OrdinalIgnoreCase))
		{
			GTDev.Log<string>("[DeepLinkHandler::OnWitchbloodCollabResponse] Item has already been redeemed!", null);
			Object.Destroy(this);
			return;
		}
		GTDev.Log<string>("[DeepLinkHandler::OnWitchbloodCollabResponse] Item successfully granted, processing external unlock...", null);
		base.StartCoroutine(this.CheckProcessExternalUnlock(this.WitchbloodCollabCosmeticID, true, true, true));
	}

	private void OnRaccoonLagoonCollabResponse(UnityWebRequest completedRequest)
	{
		if (completedRequest.result != UnityWebRequest.Result.Success)
		{
			GTDev.LogError<string>("[DeepLinkHandler::OnRaccoonLagoonCollabResponse] Web Request failed: " + completedRequest.error + "\nDetails: " + completedRequest.downloadHandler.text, null);
			Object.Destroy(this);
			return;
		}
		if (completedRequest.downloadHandler.text.Contains("AlreadyRedeemed", StringComparison.OrdinalIgnoreCase))
		{
			GTDev.Log<string>("[DeepLinkHandler::OnRaccoonLagoonCollabResponse] Item has already been redeemed!", null);
			Object.Destroy(this);
			return;
		}
		GTDev.Log<string>("[DeepLinkHandler::OnRaccoonLagoonCollabResponse] Item successfully granted, processing external unlock...", null);
		base.StartCoroutine(this.CheckProcessExternalUnlock(this.RaccoonLagoonCosmeticIDs, true, true, true));
	}

	private IEnumerator CheckProcessExternalUnlock(string[] itemIDs, bool autoEquip, bool isLeftHand, bool destroyOnFinish)
	{
		GTDev.Log<string>("[DeepLinkHandler::CheckProcessExternalUnlock] Checking if we can process external cosmetic unlock...", null);
		while (!CosmeticsController.instance.allCosmeticsDict_isInitialized || !CosmeticsV2Spawner_Dirty.allPartsInstantiated)
		{
			yield return null;
		}
		GTDev.Log<string>("[DeepLinkHandler::CheckProcessExternalUnlock] Cosmetics initialized, proceeding to process external unlock...", null);
		foreach (string text in itemIDs)
		{
			CosmeticsController.instance.ProcessExternalUnlock(text, autoEquip, isLeftHand);
		}
		if (destroyOnFinish)
		{
			Object.Destroy(this);
		}
		yield break;
	}

	public static volatile DeepLinkHandler instance;

	private LaunchDetails cachedLaunchDetails;

	private const string WitchbloodAppID = "7221491444554579";

	private readonly string[] WitchbloodCollabCosmeticID = new string[] { "LMAKT." };

	private const string RaccoonLagoonAppID = "1903584373052985";

	private readonly string[] RaccoonLagoonCosmeticIDs = new string[] { "LMALI.", "LHAGS." };

	private const string HiddenPathCollabEndpoint = "/api/ConsumeItem";

	[Serializable]
	private class CollabRequest
	{
		public string itemGUID;

		public string launchSource;

		public string oculusUserID;

		public string playFabID;

		public string playFabSessionTicket;

		public string mothershipId;

		public string mothershipToken;

		public string mothershipEnvId;
	}
}
