using System;
using System.Collections.Generic;
using GorillaNetworking;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class GRUIEmployeeTerminal : MonoBehaviour
{
	public void Setup()
	{
		this.signupButton.onPressButton.AddListener(new UnityAction(this.OnSignup));
		global::PlayFab.ClientModels.GetUserDataRequest getUserDataRequest = new global::PlayFab.ClientModels.GetUserDataRequest();
		getUserDataRequest.PlayFabId = PlayFabAuthenticator.instance.GetPlayFabPlayerId();
		getUserDataRequest.Keys = new List<string> { "GRData" };
		this.isSigningUp = true;
		PlayFabClientAPI.GetUserData(getUserDataRequest, new Action<GetUserDataResult>(this.OnGetUserDataInitialState), new Action<PlayFabError>(this.OnGetUserDataInitialStateFail), null, null);
		this.Refresh();
	}

	public void OnSignup()
	{
		if (this.isSigningUp || this.isEmployee)
		{
			return;
		}
		UpdateUserDataRequest updateUserDataRequest = new UpdateUserDataRequest
		{
			Data = new Dictionary<string, string> { { "GRData", "Now we have data" } }
		};
		if (!PlayFabClientAPI.IsClientLoggedIn())
		{
			if (PlayFabAuthenticator.instance != null)
			{
				PlayFabAuthenticator.instance.AuthenticateWithPlayFab();
			}
			return;
		}
		this.isSigningUp = true;
		PlayFabClientAPI.UpdateUserData(updateUserDataRequest, new Action<UpdateUserDataResult>(this.OnSaveTableSuccess), new Action<PlayFabError>(this.OnSaveTableFailure), null, null);
	}

	public Transform GetSpawnMarker()
	{
		return this.spawnMarker;
	}

	public void Refresh()
	{
		if (this.isSigningUp)
		{
			this.signupButtonText.text = "APPLYING";
			return;
		}
		if (this.isEmployee)
		{
			this.signupButtonText.text = "HIRED";
			return;
		}
		this.signupButtonText.text = "APPLY";
	}

	private void OnGetUserDataInitialState(GetUserDataResult result)
	{
		UserDataRecord userDataRecord;
		if (result.Data.TryGetValue("GRData", out userDataRecord))
		{
			string value = userDataRecord.Value;
			this.isEmployee = true;
		}
		else
		{
			this.isEmployee = false;
		}
		this.isSigningUp = false;
		this.Refresh();
	}

	private void OnGetUserDataInitialStateFail(PlayFabError error)
	{
		this.isEmployee = false;
		this.isSigningUp = false;
		this.Refresh();
	}

	private void OnSaveTableSuccess(UpdateUserDataResult result)
	{
		this.isEmployee = true;
		this.isSigningUp = false;
		this.Refresh();
	}

	private void OnSaveTableFailure(PlayFabError error)
	{
		this.isEmployee = false;
		this.isSigningUp = false;
		this.Refresh();
	}

	[SerializeField]
	private GorillaPressableButton signupButton;

	[SerializeField]
	private TMP_Text signupButtonText;

	[SerializeField]
	private Transform spawnMarker;

	[SerializeField]
	private GRUIStationEmployeeBadges badgeStation;

	private int entityTypeId;

	private bool isEmployee;

	private bool isSigningUp;

	private const string GR_DATA_KEY = "GRData";
}
