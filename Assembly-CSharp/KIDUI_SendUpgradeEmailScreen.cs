using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KID.Model;
using UnityEngine;

public class KIDUI_SendUpgradeEmailScreen : MonoBehaviour
{
	public async Task SendUpgradeEmail(List<string> requestedPermissions)
	{
		if (requestedPermissions.Count == 0)
		{
			Debug.Log("[KID] Tried requesting 0 permissions. Skipping upgrade email flow.");
			this._mainScreen.ShowMainScreen(EMainScreenStatus.Pending);
		}
		else
		{
			base.gameObject.SetActive(true);
			this._animatedEllipsis.StartAnimation();
			UpgradeSessionData upgradeSessionData = await KIDManager.TryUpgradeSession(requestedPermissions);
			if (upgradeSessionData == null)
			{
				this.OnFailure("We couldn't get to your information. Please contact Customer Support");
				Debug.LogError("[KID] UpgradeSessionData response was null. Maybe banned.");
			}
			else if (upgradeSessionData.status == SessionStatus.PASS)
			{
				this.OnSuccess();
			}
			else if (upgradeSessionData.status == SessionStatus.CHALLENGE_SESSION_UPGRADE)
			{
				if (KIDManager.CurrentSession.ManagedBy == Session.ManagedByEnum.PLAYER)
				{
					base.gameObject.SetActive(false);
				}
				else
				{
					ValueTuple<bool, string> valueTuple = await KIDManager.TrySendUpgradeSessionChallengeEmail();
					bool item = valueTuple.Item1;
					string item2 = valueTuple.Item2;
					if (item)
					{
						this.OnSuccess();
					}
					else
					{
						this.OnFailure(item2);
					}
				}
			}
			else
			{
				Debug.LogError("[KID] Unexpected session status when upgrading session: " + upgradeSessionData.status.ToString());
				this.OnFailure(null);
			}
		}
	}

	public void OnCancel()
	{
		base.gameObject.SetActive(false);
		this._mainScreen.ShowMainScreen(EMainScreenStatus.None);
	}

	private void OnSuccess()
	{
		base.gameObject.SetActive(false);
		this._successScreen.Show(null);
	}

	private void OnFailure(string errorMessage)
	{
		base.gameObject.SetActive(false);
		this._errorScreen.Show(errorMessage);
	}

	[SerializeField]
	private KIDUI_AnimatedEllipsis _animatedEllipsis;

	[SerializeField]
	private KIDUI_MessageScreen _successScreen;

	[SerializeField]
	private KIDUI_MessageScreen _errorScreen;

	[SerializeField]
	private KIDUI_MainScreen _mainScreen;
}
