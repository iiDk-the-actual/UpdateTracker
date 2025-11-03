using System;
using UnityEngine;
using UnityEngine.Localization.SmartFormat.PersistentVariables;

public class KIDAgeGateConfirmation : MonoBehaviour
{
	private IntVariable UserAgeVar
	{
		get
		{
			if (this._userAgeVar == null)
			{
				this._userAgeVar = this._localizedTextBody.StringReference["user-age"] as IntVariable;
				if (this._userAgeVar == null)
				{
					Debug.LogError("[Localization::KID_AGE_GATE_CONFIRMATION] Failed to get [user-age] smart variable as IntVariable");
				}
			}
			return this._userAgeVar;
		}
	}

	public KidAgeConfirmationResult Result { get; private set; }

	private void Start()
	{
		this.Result = KidAgeConfirmationResult.None;
	}

	public void OnConfirm()
	{
		this.Result = KidAgeConfirmationResult.Confirm;
	}

	public void OnBack()
	{
		this.Result = KidAgeConfirmationResult.Back;
	}

	public void Reset(int userAge)
	{
		this.Result = KidAgeConfirmationResult.None;
		if (this.UserAgeVar == null)
		{
			Debug.LogError("[LOCALIZATION::KID_AGE_GATE_CONFIRMATION] Unable to update [UserAgeVar] value, as it is null");
			return;
		}
		this.UserAgeVar.Value = userAge;
	}

	[Header("Localization")]
	[SerializeField]
	private LocalizedText _localizedTextBody;

	private IntVariable _userAgeVar;
}
