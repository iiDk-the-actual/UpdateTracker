using System;
using UnityEngine;

public class PhotonAuthenticatorSettings
{
	static PhotonAuthenticatorSettings()
	{
		PhotonAuthenticatorSettings.Load("PhotonAuthenticatorSettings");
	}

	public static void Load(string path)
	{
		PhotonAuthenticatorSettingsScriptableObject photonAuthenticatorSettingsScriptableObject = Resources.Load<PhotonAuthenticatorSettingsScriptableObject>(path);
		PhotonAuthenticatorSettings.PunAppId = photonAuthenticatorSettingsScriptableObject.PunAppId;
		PhotonAuthenticatorSettings.FusionAppId = photonAuthenticatorSettingsScriptableObject.FusionAppId;
		PhotonAuthenticatorSettings.VoiceAppId = photonAuthenticatorSettingsScriptableObject.VoiceAppId;
	}

	public static string PunAppId;

	public static string FusionAppId;

	public static string VoiceAppId;
}
