using System;

[Serializable]
public class SetOptInPermissionsRequest : KIDRequestData
{
	public string[] OptInPermissions;
}
