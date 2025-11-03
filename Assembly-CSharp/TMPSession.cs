using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KID.Model;
using UnityEngine;

public class TMPSession
{
	public bool IsValidSession
	{
		get
		{
			return (this.IsDefault && this.Permissions != null && this.Permissions.Count > 0) || (!this.IsDefault && this.SessionId != Guid.Empty);
		}
	}

	public TMPSession(Session session, KIDDefaultSession defaultSession, int? age, SessionStatus status)
	{
		this.Permissions = new Dictionary<EKIDFeatures, Permission>();
		this.OptedInPermissions = new HashSet<EKIDFeatures>();
		this.Age = age.GetValueOrDefault();
		this.SessionStatus = status;
		if (session == null && defaultSession == null)
		{
			return;
		}
		if (session == null)
		{
			this.IsDefault = true;
			this.AgeStatus = defaultSession.AgeStatus;
			this.InitialiseDefaultPermissionSet(defaultSession);
			return;
		}
		this.SessionId = session.SessionId;
		this.Etag = session.Etag;
		this.AgeStatus = session.AgeStatus;
		this.KidStatus = session.Status;
		this.DateOfBirth = session.DateOfBirth;
		this.KUID = session.Kuid;
		this.Jurisdiction = session.Jurisdiction;
		this.ManagedBy = session.ManagedBy;
		this.Age = this.GetAgeFromDateOfBirth();
		for (int i = 0; i < session.Permissions.Count; i++)
		{
			EKIDFeatures? ekidfeatures = KIDFeaturesExtensions.FromString(session.Permissions[i].Name);
			if (ekidfeatures != null && !this.Permissions.TryAdd(ekidfeatures.Value, session.Permissions[i]))
			{
				Debug.LogError("[KID::SESSION] Tried creating new session, but permission for [" + ekidfeatures.Value.ToStandardisedString() + "] already exists");
			}
		}
	}

	public void SetOptInPermissions(string[] optedInPermissions)
	{
		if (optedInPermissions == null || optedInPermissions.Length == 0)
		{
			Debug.LogWarning("[KID::SESSION] OptedInPermissions is null or empty. Returning without setting.");
			return;
		}
		int num = 0;
		for (;;)
		{
			int num2 = num;
			int? num3 = ((optedInPermissions != null) ? new int?(optedInPermissions.Length) : null);
			if (!((num2 < num3.GetValueOrDefault()) & (num3 != null)))
			{
				break;
			}
			EKIDFeatures? ekidfeatures = KIDFeaturesExtensions.FromString(optedInPermissions[num]);
			if (ekidfeatures != null)
			{
				this.OptInToPermission(ekidfeatures.Value, true);
			}
			num++;
		}
		Debug.Log(string.Format("[KID::SESSION::OptInRefactor] Constructor OptedInPermissions: {0}", this.GetOptedInPermissions()));
	}

	public bool TryGetPermission(EKIDFeatures feature, out Permission permission)
	{
		if (!this.Permissions.ContainsKey(feature))
		{
			Debug.LogError("[KID::SESSION] Tried retreiving permission for [" + feature.ToStandardisedString() + "], but does not exist");
			permission = null;
			return false;
		}
		permission = this.Permissions[feature];
		return true;
	}

	public List<Permission> GetAllPermissions()
	{
		return this.Permissions.Values.ToList<Permission>();
	}

	public bool HasPermissionForFeature(EKIDFeatures feature)
	{
		Permission permission;
		if (!this.TryGetPermission(feature, out permission))
		{
			Debug.LogError("[KID::SESSION] Tried checking for permission but couldn't find [" + feature.ToStandardisedString() + "]. Assuming disabled");
			return false;
		}
		return permission.Enabled;
	}

	public void OptInToPermission(EKIDFeatures feature, bool optIn)
	{
		Debug.Log(string.Format("[KID::SESSION::OptInRefactor] Opting in to permission for [{0}] with optIn: {1}", feature.ToStandardisedString(), optIn));
		if (optIn && !this.OptedInPermissions.Contains(feature))
		{
			this.OptedInPermissions.Add(feature);
			return;
		}
		if (!optIn && this.OptedInPermissions.Contains(feature))
		{
			this.OptedInPermissions.Remove(feature);
			return;
		}
	}

	public bool HasOptedInToPermission(EKIDFeatures feature)
	{
		return this.OptedInPermissions.Contains(feature);
	}

	public string[] GetOptedInPermissions()
	{
		if (this.OptedInPermissions == null || this.OptedInPermissions.Count == 0)
		{
			Debug.LogWarning("[KID::SESSION] OptedInPermissions is null or empty. Returning empty array.");
			return Array.Empty<string>();
		}
		return this.OptedInPermissions.Select((EKIDFeatures f) => f.ToStandardisedString()).ToArray<string>();
	}

	public void UpdatePermission(EKIDFeatures feature, Permission newData)
	{
		if (!this.Permissions.ContainsKey(feature))
		{
			Debug.Log("[KID::SESSION] Trying to update permission, but could not find [" + feature.ToStandardisedString() + "] in dictionary. Will add new one");
			this.Permissions.Add(feature, null);
		}
		this.Permissions[feature] = newData;
	}

	private void InitialiseDefaultPermissionSet(KIDDefaultSession defaultSession)
	{
		for (int i = 0; i < defaultSession.Permissions.Count; i++)
		{
			EKIDFeatures? ekidfeatures = KIDFeaturesExtensions.FromString(defaultSession.Permissions[i].Name);
			if (ekidfeatures != null && !this.Permissions.TryAdd(ekidfeatures.Value, defaultSession.Permissions[i]))
			{
				Debug.LogError("[KID::SESSION] Tried creating new session, but permission for [" + ekidfeatures.Value.ToStandardisedString() + "] already exists");
			}
		}
	}

	private int GetAgeFromDateOfBirth()
	{
		DateTime today = DateTime.Today;
		int num = today.Year - this.DateOfBirth.Year;
		int num2 = today.Month - this.DateOfBirth.Month;
		if (num2 < 0)
		{
			num--;
		}
		else if (num2 == 0 && today.Day - this.DateOfBirth.Day < 0)
		{
			num--;
		}
		return num;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("New TMPSession]:");
		stringBuilder.AppendLine(string.Format("    - Is Default    :   {0}", this.IsDefault));
		stringBuilder.AppendLine(string.Format("    - Is Valid      :   {0}", this.IsValidSession));
		stringBuilder.AppendLine(string.Format("    - SessionID     :   {0}", this.SessionId));
		stringBuilder.AppendLine(string.Format("    - Age           :   {0}", this.Age));
		stringBuilder.AppendLine(string.Format("    - AgeStatus     :   {0}", this.AgeStatus));
		stringBuilder.AppendLine(string.Format("    - SessionStatus :   {0}", this.KidStatus));
		stringBuilder.AppendLine("    - DoB           :   " + this.DateOfBirth.ToString());
		stringBuilder.AppendLine("    - KUID          :   " + this.KUID);
		stringBuilder.AppendLine("    - Jurisdiction  :   " + this.Jurisdiction);
		stringBuilder.AppendLine("    - PERMISSIONS   :");
		if (this.Permissions != null)
		{
			foreach (Permission permission in this.Permissions.Values)
			{
				stringBuilder.AppendLine(string.Format("        - {0} - Enabled: {1} - ManagedBy: {2}", permission.Name, permission.Enabled, permission.ManagedBy));
			}
		}
		return stringBuilder.ToString();
	}

	public readonly Guid SessionId;

	public readonly string Etag;

	public readonly AgeStatusType AgeStatus;

	public readonly Session.StatusEnum KidStatus;

	public readonly Session.ManagedByEnum ManagedBy;

	public readonly DateTime DateOfBirth;

	public readonly string Jurisdiction;

	public readonly string KUID;

	public readonly int Age;

	public readonly bool IsDefault;

	public readonly SessionStatus SessionStatus;

	private Dictionary<EKIDFeatures, Permission> Permissions;

	private HashSet<EKIDFeatures> OptedInPermissions;
}
