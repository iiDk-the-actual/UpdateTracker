using System;
using GorillaNetworking;
using TMPro;
using UnityEngine;

public class CustomMapsAccessScreen : CustomMapsTerminalScreen
{
	private void LateUpdate()
	{
		if (CustomMapsTerminal.GetDriverID() == -2)
		{
			return;
		}
		if (CustomMapsTerminal.IsDriver)
		{
			return;
		}
		if (GorillaComputer.instance == null)
		{
			return;
		}
		if (this.useNametags == GorillaComputer.instance.NametagsEnabled)
		{
			return;
		}
		this.useNametags = GorillaComputer.instance.NametagsEnabled;
		this.SetDriverName();
	}

	public override void Initialize()
	{
	}

	public override void Show()
	{
		base.Show();
		if (this.displayedText == string.Empty)
		{
			this.displayedText = this.defaultText;
		}
		this.errorText.gameObject.SetActive(false);
		this.terminalControlPromptText.gameObject.SetActive(true);
		this.terminalControlPromptText.text = this.displayedText;
	}

	public override void Hide()
	{
		this.errorText.gameObject.SetActive(false);
		this.terminalControlPromptText.gameObject.SetActive(false);
		base.Hide();
	}

	public void Reset()
	{
		this.errorText.gameObject.SetActive(false);
		this.terminalControlPromptText.gameObject.SetActive(true);
		this.displayedText = this.defaultText;
	}

	public void SetDetailsScreenForDriver()
	{
		this.displayedText = this.detailsScreenText;
	}

	public void SetDriverName()
	{
		bool flag = KIDManager.HasPermissionToUseFeature(EKIDFeatures.Custom_Nametags);
		string text;
		if (NetworkSystem.Instance.InRoom)
		{
			NetPlayer netPlayerByID = NetworkSystem.Instance.GetNetPlayerByID(CustomMapsTerminal.GetDriverID());
			text = netPlayerByID.DefaultName;
			if (this.useNametags && flag)
			{
				RigContainer rigContainer;
				if (netPlayerByID.IsLocal)
				{
					text = netPlayerByID.NickName;
				}
				else if (VRRigCache.Instance.TryGetVrrig(netPlayerByID, out rigContainer))
				{
					text = rigContainer.Rig.playerNameVisible;
				}
			}
		}
		else
		{
			text = ((this.useNametags && flag) ? NetworkSystem.Instance.LocalPlayer.NickName : NetworkSystem.Instance.LocalPlayer.DefaultName);
		}
		this.displayedText = "TERMINAL CONTROLLED BY: " + text;
		if (!this.isControlScreen)
		{
			this.displayedText += this.detailsScreenText;
		}
		this.terminalControlPromptText.text = this.displayedText;
	}

	public void DisplayError(string errorMessage)
	{
		this.terminalControlPromptText.gameObject.SetActive(false);
		this.errorText.text = errorMessage;
		this.errorText.gameObject.SetActive(true);
	}

	[SerializeField]
	private TMP_Text errorText;

	[SerializeField]
	private TMP_Text terminalControlPromptText;

	[SerializeField]
	private bool isControlScreen = true;

	[SerializeField]
	private string defaultText = "PRESS THE 'TERMINAL AVAILABLE' BUTTON TO PROCEED.";

	private string detailsScreenText = "\nMAP DETAILS WILL APPEAR HERE WHEN A MAP IS SELECTED.";

	private string displayedText = string.Empty;

	private bool useNametags;
}
