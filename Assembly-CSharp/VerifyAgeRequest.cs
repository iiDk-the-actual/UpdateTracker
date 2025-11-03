using System;

[Serializable]
public class VerifyAgeRequest : KIDRequestData
{
	public int? Age;

	public PlayerPlatform? Platform;
}
