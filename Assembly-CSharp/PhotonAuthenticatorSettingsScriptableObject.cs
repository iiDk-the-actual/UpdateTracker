using System;
using UnityEngine;

[CreateAssetMenu(fileName = "PhotonAuthenticatorSettings", menuName = "ScriptableObjects/PhotonAuthenticatorSettings")]
public class PhotonAuthenticatorSettingsScriptableObject : ScriptableObject
{
	public string PunAppId;

	public string FusionAppId;

	public string VoiceAppId;
}
