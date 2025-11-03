using System;
using System.Collections.Generic;
using KID.Model;

[Serializable]
public class KIDSession
{
	public SessionStatus SessionStatus { get; set; }

	public GTAgeStatusType AgeStatus { get; set; }

	public Guid SessionId { get; set; }

	public string KUID { get; set; }

	public string etag { get; set; }

	public List<Permission> Permissions { get; set; }

	public DateTime DateOfBirth { get; set; }

	public string Jurisdiction { get; set; }
}
