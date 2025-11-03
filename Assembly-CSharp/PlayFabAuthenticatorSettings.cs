using System;
using UnityEngine;

public class PlayFabAuthenticatorSettings
{
	static PlayFabAuthenticatorSettings()
	{
		PlayFabAuthenticatorSettings.Load("PlayFabAuthenticatorSettings");
	}

	public static void Load(string path)
	{
		PlayFabAuthenticatorSettingsScriptableObject playFabAuthenticatorSettingsScriptableObject = Resources.Load<PlayFabAuthenticatorSettingsScriptableObject>(path);
		PlayFabAuthenticatorSettings.TitleId = playFabAuthenticatorSettingsScriptableObject.TitleId;
		PlayFabAuthenticatorSettings.AuthApiBaseUrl = playFabAuthenticatorSettingsScriptableObject.AuthApiBaseUrl;
		PlayFabAuthenticatorSettings.DailyQuestsApiBaseUrl = playFabAuthenticatorSettingsScriptableObject.DailyQuestsApiBaseUrl;
		PlayFabAuthenticatorSettings.FriendApiBaseUrl = playFabAuthenticatorSettingsScriptableObject.FriendApiBaseUrl;
		PlayFabAuthenticatorSettings.HpPromoApiBaseUrl = playFabAuthenticatorSettingsScriptableObject.HpPromoApiBaseUrl;
		PlayFabAuthenticatorSettings.IapApiBaseUrl = playFabAuthenticatorSettingsScriptableObject.IapApiBaseUrl;
		PlayFabAuthenticatorSettings.KidApiBaseUrl = playFabAuthenticatorSettingsScriptableObject.KidApiBaseUrl;
		PlayFabAuthenticatorSettings.MmrApiBaseUrl = playFabAuthenticatorSettingsScriptableObject.MmrApiBaseUrl;
		PlayFabAuthenticatorSettings.ProgressionApiBaseUrl = playFabAuthenticatorSettingsScriptableObject.ProgressionApiBaseUrl;
		PlayFabAuthenticatorSettings.TitleDataApiBaseUrl = playFabAuthenticatorSettingsScriptableObject.TitleDataApiBaseUrl;
		PlayFabAuthenticatorSettings.VotingApiBaseUrl = playFabAuthenticatorSettingsScriptableObject.VotingApiBaseUrl;
	}

	public static string TitleId;

	public static string AuthApiBaseUrl;

	public static string DailyQuestsApiBaseUrl;

	public static string FriendApiBaseUrl;

	public static string HpPromoApiBaseUrl;

	public static string IapApiBaseUrl;

	public static string KidApiBaseUrl;

	public static string MmrApiBaseUrl;

	public static string ProgressionApiBaseUrl;

	public static string TitleDataApiBaseUrl;

	public static string VotingApiBaseUrl;
}
