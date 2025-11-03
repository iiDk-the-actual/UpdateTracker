using System;
using System.Collections.Generic;
using KID.Model;

[Serializable]
public class KIDDefaultSession
{
	public List<Permission> Permissions { get; set; }

	public AgeStatusType AgeStatus { get; set; }
}
