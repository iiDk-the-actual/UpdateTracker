using System;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayFabAuthenticatorSettings", menuName = "ScriptableObjects/PlayFabAuthenticatorSettings")]
public class PlayFabAuthenticatorSettingsScriptableObject : ScriptableObject
{
	public string TitleId;

	public string AuthApiBaseUrl;

	public string DailyQuestsApiBaseUrl;

	public string FriendApiBaseUrl;

	public string HpPromoApiBaseUrl;

	public string IapApiBaseUrl;

	public string KidApiBaseUrl;

	public string MmrApiBaseUrl;

	public string ProgressionApiBaseUrl;

	public string TitleDataApiBaseUrl;

	public string VotingApiBaseUrl;
}
