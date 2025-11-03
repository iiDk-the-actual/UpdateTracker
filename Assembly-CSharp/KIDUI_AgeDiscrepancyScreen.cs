using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.SmartFormat.PersistentVariables;

public class KIDUI_AgeDiscrepancyScreen : MonoBehaviour
{
	private void Awake()
	{
		this.CheckLocalizationReferences();
	}

	public async Task ShowAgeDiscrepancyScreenWithAwait(string description)
	{
		base.gameObject.SetActive(true);
		this.CheckLocalizationReferences();
		this._descriptionText.text = description;
		await this.WaitForCompletion();
	}

	public async Task ShowAgeDiscrepancyScreenWithAwait(int userAge, int accAge, int lowestAge)
	{
		base.gameObject.SetActive(true);
		this.CheckLocalizationReferences();
		this._userAgeVar.Value = userAge;
		this._accountAgeVar.Value = accAge;
		this._lowestAgeVar.Value = lowestAge;
		await this.WaitForCompletion();
	}

	private async Task WaitForCompletion()
	{
		do
		{
			await Task.Yield();
		}
		while (!this._hasCompleted);
	}

	public void OnHoldComplete()
	{
		this._hasCompleted = true;
	}

	public void OnQuitPressed()
	{
		Application.Quit();
	}

	private void CheckLocalizationReferences()
	{
		if (this._bodyLocStr != null && this._userAgeVar != null && this._accountAgeVar != null && this._lowestAgeVar != null)
		{
			return;
		}
		if (this._bodyTextLoc == null)
		{
			Debug.LogError("[LOCALIZATION::KIDUI_AGE_DISCREPANCY_SCREEN] [_bodyTextLoc] is not set, unable to localize smart string");
			return;
		}
		this._bodyLocStr = this._bodyTextLoc.StringReference;
		this._userAgeVar = this._bodyLocStr["user-age"] as IntVariable;
		this._accountAgeVar = this._bodyLocStr["account-age"] as IntVariable;
		this._lowestAgeVar = this._bodyLocStr["lowest-age"] as IntVariable;
	}

	[SerializeField]
	private TMP_Text _descriptionText;

	[Header("Localization")]
	[SerializeField]
	private LocalizedText _bodyTextLoc;

	private bool _hasCompleted;

	private LocalizedString _bodyLocStr;

	private IntVariable _userAgeVar;

	private IntVariable _accountAgeVar;

	private IntVariable _lowestAgeVar;
}
