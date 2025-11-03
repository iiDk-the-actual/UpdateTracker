using System;
using GorillaNetworking;
using KID.Model;
using UnityEngine;

internal class UGCPermissionManager : MonoBehaviour
{
	public static void UsePlayFabSafety()
	{
		UGCPermissionManager.permissions = new UGCPermissionManager.PlayFabPermissions(new Action<bool>(UGCPermissionManager.SetUGCEnabled));
		UGCPermissionManager.permissions.Initialize();
	}

	public static void UseKID()
	{
		UGCPermissionManager.permissions = new UGCPermissionManager.KIDPermissions(new Action<bool>(UGCPermissionManager.SetUGCEnabled));
		UGCPermissionManager.permissions.Initialize();
	}

	public static bool IsUGCDisabled
	{
		get
		{
			return !UGCPermissionManager.isUGCEnabled.GetValueOrDefault();
		}
	}

	public static void CheckPermissions()
	{
		UGCPermissionManager.IUGCPermissions iugcpermissions = UGCPermissionManager.permissions;
		if (iugcpermissions == null)
		{
			return;
		}
		iugcpermissions.CheckPermissions();
	}

	public static void SubscribeToUGCEnabled(Action callback)
	{
		UGCPermissionManager.onUGCEnabled = (Action)Delegate.Combine(UGCPermissionManager.onUGCEnabled, callback);
	}

	public static void UnsubscribeFromUGCEnabled(Action callback)
	{
		UGCPermissionManager.onUGCEnabled = (Action)Delegate.Remove(UGCPermissionManager.onUGCEnabled, callback);
	}

	public static void SubscribeToUGCDisabled(Action callback)
	{
		UGCPermissionManager.onUGCDisabled = (Action)Delegate.Combine(UGCPermissionManager.onUGCDisabled, callback);
	}

	public static void UnsubscribeFromUGCDisabled(Action callback)
	{
		UGCPermissionManager.onUGCDisabled = (Action)Delegate.Remove(UGCPermissionManager.onUGCDisabled, callback);
	}

	private static void SetUGCEnabled(bool enabled)
	{
		bool? flag = UGCPermissionManager.isUGCEnabled;
		if (!((enabled == flag.GetValueOrDefault()) & (flag != null)))
		{
			Debug.LogFormat("[UGCPermissionManager][KID] UGC state changed: [{0}]", new object[] { enabled ? "ENABLED" : "DISABLED" });
			UGCPermissionManager.isUGCEnabled = new bool?(enabled);
			if (enabled)
			{
				Debug.Log("[UGCPermissionManager][KID] Invoking onUGCEnabled");
				Action action = UGCPermissionManager.onUGCEnabled;
				if (action == null)
				{
					return;
				}
				action();
				return;
			}
			else
			{
				Debug.Log("[UGCPermissionManager][KID] Invoking onUGCDisabled");
				Action action2 = UGCPermissionManager.onUGCDisabled;
				if (action2 == null)
				{
					return;
				}
				action2();
			}
		}
	}

	[OnEnterPlay_SetNull]
	private static UGCPermissionManager.IUGCPermissions permissions;

	[OnEnterPlay_SetNull]
	private static Action onUGCEnabled;

	[OnEnterPlay_SetNull]
	private static Action onUGCDisabled;

	private static bool? isUGCEnabled;

	private interface IUGCPermissions
	{
		void Initialize();

		void CheckPermissions();
	}

	private class PlayFabPermissions : UGCPermissionManager.IUGCPermissions
	{
		public PlayFabPermissions(Action<bool> setUGCEnabled)
		{
			this.setUGCEnabled = setUGCEnabled;
		}

		public void Initialize()
		{
			bool safety = PlayFabAuthenticator.instance.GetSafety();
			Debug.LogFormat("[UGCPermissionManager][KID] UGC initialized from Playfab: [{0}]", new object[] { safety ? "DISABLED" : "ENABLED" });
			Action<bool> action = this.setUGCEnabled;
			if (action == null)
			{
				return;
			}
			action(!safety);
		}

		public void CheckPermissions()
		{
		}

		private Action<bool> setUGCEnabled;
	}

	private class KIDPermissions : UGCPermissionManager.IUGCPermissions
	{
		public KIDPermissions(Action<bool> setUGCEnabled)
		{
			this.setUGCEnabled = setUGCEnabled;
		}

		private void SetUGCEnabled(bool enabled)
		{
			Action<bool> action = this.setUGCEnabled;
			if (action == null)
			{
				return;
			}
			action(enabled);
		}

		public void Initialize()
		{
			Debug.Log("[UGCPermissionManager][KID] Initializing with KID");
			this.CheckPermissions();
			KIDManager.RegisterSessionUpdatedCallback_UGC(new Action<bool, Permission.ManagedByEnum>(this.OnKIDSessionUpdate));
		}

		public void CheckPermissions()
		{
			Permission permissionDataByFeature = KIDManager.GetPermissionDataByFeature(EKIDFeatures.Mods);
			bool item = KIDManager.CheckFeatureOptIn(EKIDFeatures.Mods, null).Item2;
			this.ProcessPermissionKID(item, permissionDataByFeature.Enabled, permissionDataByFeature.ManagedBy);
		}

		private void OnKIDSessionUpdate(bool isEnabled, Permission.ManagedByEnum managedBy)
		{
			Debug.Log("[UGCPermissionManager][KID] KID session update.");
			bool item = KIDManager.CheckFeatureOptIn(EKIDFeatures.Mods, null).Item2;
			this.ProcessPermissionKID(item, isEnabled, managedBy);
		}

		private void ProcessPermissionKID(bool hasOptedIn, bool isEnabled, Permission.ManagedByEnum managedBy)
		{
			Debug.LogFormat("[UGCPermissionManager][KID] Process KID permissions - opted in: [{0}], enabled: [{1}], managedBy: [{2}].", new object[] { hasOptedIn, isEnabled, managedBy });
			if (managedBy == Permission.ManagedByEnum.PROHIBITED)
			{
				Debug.Log("[UGCPermissionManager][KID] KID UGC prohibited.");
				this.SetUGCEnabled(false);
				return;
			}
			if (managedBy != Permission.ManagedByEnum.PLAYER)
			{
				if (managedBy == Permission.ManagedByEnum.GUARDIAN)
				{
					Debug.LogFormat("[UGCPermissionManager][KID] KID UGC managed by guardian. (opted in: [{0}], enabled: [{1}])", new object[] { hasOptedIn, isEnabled });
					this.SetUGCEnabled(isEnabled);
				}
				return;
			}
			if (isEnabled)
			{
				Debug.Log("[UGCPermissionManager][KID] KID UGC managed by player and enabled - opting in and enabling UGC.");
				if (!hasOptedIn)
				{
					KIDManager.SetFeatureOptIn(EKIDFeatures.Mods, true);
				}
				this.SetUGCEnabled(true);
				return;
			}
			Debug.LogFormat("[UGCPermissionManager][KID] KID UGC managed by player and disabled by default - using opt in status. (opted in: [{0}])", new object[] { hasOptedIn });
			this.SetUGCEnabled(hasOptedIn);
		}

		private Action<bool> setUGCEnabled;
	}
}
