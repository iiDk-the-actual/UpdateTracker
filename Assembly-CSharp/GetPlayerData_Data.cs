using System;
using System.Runtime.CompilerServices;
using KID.Model;
using UnityEngine;

public class GetPlayerData_Data
{
	public GetPlayerData_Data(GetSessionResponseType type, GetPlayerDataResponse response)
	{
		this.responseType = type;
		if (response == null)
		{
			if (this.responseType == GetSessionResponseType.OK)
			{
				this.responseType = GetSessionResponseType.ERROR;
				Debug.LogError("[KID::GET_PLAYER_DATA_DATA] Incoming [GetPlayerDataResponse] is NULL");
			}
			return;
		}
		this.AgeStatus = response.AgeStatus;
		this.status = response.Status;
		if (this.status != null)
		{
			this.session = new TMPSession(response.Session, response.DefaultSession, response.Age, this.status.Value);
			this.session.SetOptInPermissions(response.Permissions);
			Debug.Log("[KID::GET_PLAYER_DATA_DATA::OptInRefactor] Setting Opt-in Permissions: " + string.Join(", ", this.session.GetOptedInPermissions()));
		}
		this.HasConfirmedSetup = response.HasConfirmedSetup;
	}

	public readonly AgeStatusType? AgeStatus;

	public readonly GetSessionResponseType responseType;

	public readonly SessionStatus? status;

	public readonly TMPSession session;

	[Nullable(new byte[] { 2, 0 })]
	public readonly string[] OptInPermissions;

	public readonly bool HasConfirmedSetup;
}
