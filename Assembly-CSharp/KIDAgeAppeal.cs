using System;
using System.Collections.Generic;
using KID.Model;
using TMPro;
using UnityEngine;

public class KIDAgeAppeal : MonoBehaviour
{
	public void ShowAgeAppealScreen()
	{
		this._ageSlider = base.GetComponentInChildren<AgeSliderWithProgressBar>(true);
		this._ageSlider.ControllerActive = true;
		base.gameObject.SetActive(true);
		this._inputsContainer.SetActive(true);
		this._monkeLoader.SetActive(false);
	}

	public async void OnNewAgeConfirmed()
	{
		this._inputsContainer.SetActive(false);
		this._monkeLoader.SetActive(true);
		AgeStatusType ageStatusType;
		if (KIDManager.TryGetAgeStatusTypeFromAge(this._ageSlider.CurrentAge, out ageStatusType))
		{
			TelemetryData telemetryData = new TelemetryData
			{
				EventName = "kid_age_appeal_age_gate",
				CustomTags = new string[]
				{
					"kid_age_appeal",
					KIDTelemetry.GameVersionCustomTag,
					KIDTelemetry.GameEnvironment
				},
				BodyData = new Dictionary<string, string> { 
				{
					"correct_age",
					ageStatusType.ToString()
				} }
			};
			GorillaTelemetry.EnqueueTelemetryEvent(telemetryData.EventName, telemetryData.BodyData, telemetryData.CustomTags);
		}
		AttemptAgeUpdateData attemptAgeUpdateData = await KIDManager.TryAttemptAgeUpdate(this._ageSlider.CurrentAge);
		if (attemptAgeUpdateData.status == SessionStatus.PROHIBITED)
		{
			Debug.LogError("[KID::AGE-APPEAL] Age Appeal Status: PROHIBITED");
			base.gameObject.SetActive(false);
			KIDUI_AgeAppealController.Instance.StartTooYoungToPlayScreen();
		}
		else
		{
			this._ageAppealEmailScreen.ShowAgeAppealEmailScreen(attemptAgeUpdateData.status == SessionStatus.CHALLENGE, this._ageSlider.CurrentAge);
			this._ageSlider.ControllerActive = false;
			base.gameObject.SetActive(false);
		}
	}

	[SerializeField]
	private TMP_Text _ageText;

	[SerializeField]
	private KIDUI_AgeAppealEmailScreen _ageAppealEmailScreen;

	[SerializeField]
	private GameObject _inputsContainer;

	[SerializeField]
	private GameObject _monkeLoader;

	private AgeSliderWithProgressBar _ageSlider;
}
