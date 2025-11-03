using System;
using TMPro;
using UnityEngine;

public class KIDUI_AgeAppealEmailError : MonoBehaviour
{
	public void ShowAgeAppealEmailErrorScreen(bool hasChallenge, int newAge, string email)
	{
		this.hasChallenge = hasChallenge;
		this.newAge = newAge;
		this._emailText.text = email;
		base.gameObject.SetActive(true);
	}

	public void onBackPressed()
	{
		base.gameObject.SetActive(false);
		this._ageAppealEmailScreen.ShowAgeAppealEmailScreen(this.hasChallenge, this.newAge);
	}

	[SerializeField]
	private KIDUI_AgeAppealEmailScreen _ageAppealEmailScreen;

	[SerializeField]
	private TMP_Text _emailText;

	private bool hasChallenge;

	private int newAge;
}
