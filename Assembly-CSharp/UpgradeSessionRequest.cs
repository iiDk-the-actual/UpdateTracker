using System;
using System.Collections.Generic;
using KID.Model;

[Serializable]
public class UpgradeSessionRequest : KIDRequestData
{
	public List<RequestedPermission> Permissions;
}
