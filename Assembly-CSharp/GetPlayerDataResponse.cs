using System;
using System.Runtime.CompilerServices;
using KID.Model;

[Serializable]
public class GetPlayerDataResponse
{
	public SessionStatus? Status;

	public Session Session;

	public int? Age;

	public AgeStatusType? AgeStatus;

	public KIDDefaultSession DefaultSession;

	[Nullable(new byte[] { 2, 0 })]
	public string[] Permissions;

	public bool HasConfirmedSetup;
}
