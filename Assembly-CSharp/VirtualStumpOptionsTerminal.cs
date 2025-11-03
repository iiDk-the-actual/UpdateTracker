using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GorillaExtensions;
using GorillaTagScripts.VirtualStumpCustomMaps.UI;
using Modio;
using Modio.Customizations;
using Modio.Users;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class VirtualStumpOptionsTerminal : MonoBehaviour, IWssAuthPrompter
{
	public void Start()
	{
		this.optionList.gameObject.SetActive(true);
		this.mainScreenText.gameObject.SetActive(true);
		this.RefreshButtonState();
		this.UpdateOptionListForCurrentState();
		this.UpdateScreen();
		CustomMapsKeyboard customMapsKeyboard = this.keyboard;
		if (customMapsKeyboard != null)
		{
			customMapsKeyboard.OnKeyPressed.AddListener(new UnityAction<CustomMapKeyboardBinding>(this.OnKeyPressed));
		}
		ModIOManager.OnModIOLoggedIn.AddListener(new UnityAction(this.OnModIOLoggedIn));
		ModIOManager.OnModIOLoginStarted.AddListener(new UnityAction(this.OnModIOLoginStarted));
		ModIOManager.OnModIOLoginFailed.AddListener(new UnityAction<string>(this.OnModIOLoginFailed));
		ModIOManager.OnModIOUserChanged.AddListener(new UnityAction<User>(this.OnModIOUserChanged));
	}

	public void OnDestroy()
	{
		CustomMapsKeyboard customMapsKeyboard = this.keyboard;
		if (customMapsKeyboard != null)
		{
			customMapsKeyboard.OnKeyPressed.RemoveListener(new UnityAction<CustomMapKeyboardBinding>(this.OnKeyPressed));
		}
		ModIOManager.OnModIOLoggedIn.RemoveListener(new UnityAction(this.OnModIOLoggedIn));
		ModIOManager.OnModIOLoggedOut.RemoveListener(new UnityAction(this.OnModIOLoggedOut));
		ModIOManager.OnModIOLoginStarted.RemoveListener(new UnityAction(this.OnModIOLoginStarted));
		ModIOManager.OnModIOLoginFailed.RemoveListener(new UnityAction<string>(this.OnModIOLoginFailed));
		ModIOManager.OnModIOUserChanged.RemoveListener(new UnityAction<User>(this.OnModIOUserChanged));
	}

	public void OnEnable()
	{
		this.RefreshButtonState();
		this.UpdateOptionListForCurrentState();
		this.UpdateScreen();
	}

	private void OnKeyPressed(CustomMapKeyboardBinding pressedButton)
	{
		if (!this.cachedError.IsNullOrEmpty())
		{
			this.cachedError = null;
			this.RefreshButtonState();
			this.UpdateScreen();
			return;
		}
		if (pressedButton == CustomMapKeyboardBinding.up)
		{
			int num = this.currentState - VirtualStumpOptionsTerminal.ETerminalState.ROOM_SIZE;
			if (num < 0)
			{
				num = 1;
			}
			this.ChangeState((VirtualStumpOptionsTerminal.ETerminalState)num);
			this.UpdateOptionListForCurrentState();
			this.UpdateScreen();
			return;
		}
		if (pressedButton == CustomMapKeyboardBinding.down)
		{
			int num2 = (int)(this.currentState + 1);
			if (num2 >= 2)
			{
				num2 = 0;
			}
			this.ChangeState((VirtualStumpOptionsTerminal.ETerminalState)num2);
			this.UpdateOptionListForCurrentState();
			this.UpdateScreen();
			return;
		}
		VirtualStumpOptionsTerminal.ETerminalState eterminalState = this.currentState;
		if (eterminalState == VirtualStumpOptionsTerminal.ETerminalState.MODIO_ACCOUNT)
		{
			this.OnKeyPressed_ModIOAccount(pressedButton);
			return;
		}
		if (eterminalState != VirtualStumpOptionsTerminal.ETerminalState.ROOM_SIZE)
		{
			return;
		}
		this.OnKeyPressed_RoomSize(pressedButton);
	}

	private void ChangeState(VirtualStumpOptionsTerminal.ETerminalState newState)
	{
		if (newState == this.currentState)
		{
			return;
		}
		this.currentState = newState;
		this.RefreshButtonState();
	}

	private void RefreshButtonState()
	{
		for (int i = 0; i < this.contextualButtons.Count; i++)
		{
			if (this.contextualButtons[i].IsNotNull())
			{
				this.contextualButtons[i].SetActive(false);
			}
		}
		if (!this.cachedError.IsNullOrEmpty())
		{
			this.OKButton.SetActive(true);
			return;
		}
		VirtualStumpOptionsTerminal.ETerminalState eterminalState = this.currentState;
		if (eterminalState == VirtualStumpOptionsTerminal.ETerminalState.MODIO_ACCOUNT)
		{
			for (int j = 0; j < this.buttonsToShow_MODIO.Count; j++)
			{
				if (this.buttonsToShow_MODIO[j].IsNotNull())
				{
					this.buttonsToShow_MODIO[j].SetActive(true);
				}
			}
			return;
		}
		if (eterminalState != VirtualStumpOptionsTerminal.ETerminalState.ROOM_SIZE)
		{
			return;
		}
		for (int k = 0; k < this.buttonsToShow_ROOMSIZE.Count; k++)
		{
			if (this.buttonsToShow_ROOMSIZE[k].IsNotNull())
			{
				this.buttonsToShow_ROOMSIZE[k].SetActive(true);
			}
		}
	}

	private void UpdateOptionListForCurrentState()
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < 2; i++)
		{
			stringBuilder.Append(this.optionStrings[i]);
			if (i == (int)this.currentState)
			{
				stringBuilder.Append(" <-");
			}
			stringBuilder.Append("\n");
		}
		this.optionList.text = stringBuilder.ToString();
	}

	private void UpdateScreen()
	{
		Debug.Log("[VirtualStumpOptionsTerminal::UpdateScreen] State: " + this.currentState.ToString() + " | CachedError: " + (!this.cachedError.IsNullOrEmpty()).ToString());
		this.mainScreenText.text = "";
		if (!this.cachedError.IsNullOrEmpty())
		{
			this.RefreshButtonState();
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(this.cachedError);
			TMP_Text tmp_Text = this.mainScreenText;
			string text = "<color=\"red\">";
			StringBuilder stringBuilder2 = stringBuilder;
			tmp_Text.text = text + ((stringBuilder2 != null) ? stringBuilder2.ToString() : null);
			return;
		}
		VirtualStumpOptionsTerminal.ETerminalState eterminalState = this.currentState;
		if (eterminalState == VirtualStumpOptionsTerminal.ETerminalState.MODIO_ACCOUNT)
		{
			this.mainScreenText.text = this.UpdateScreen_ModIOAccount();
			return;
		}
		if (eterminalState != VirtualStumpOptionsTerminal.ETerminalState.ROOM_SIZE)
		{
			return;
		}
		this.mainScreenText.text = this.UpdateScreen_RoomSize();
	}

	private void OnModIOLoginStarted()
	{
		Debug.Log("[VirtualStumpOptionsTerminal::OnModIOLoginStarted]...");
		this.UpdateScreen();
	}

	private void OnModIOLoggedIn()
	{
		Debug.Log("[VirtualStumpOptionsTerminal::OnModIOLoggedIn]...");
		ModIOManager.OnModIOLoggedOut.RemoveListener(new UnityAction(this.OnModIOLoggedOut));
		ModIOManager.OnModIOLoggedOut.AddListener(new UnityAction(this.OnModIOLoggedOut));
		this.processingAccountLink = false;
		this.UpdateScreen();
		AssociateMotherhsipAndModIOAccountsRequest associateMotherhsipAndModIOAccountsRequest = new AssociateMotherhsipAndModIOAccountsRequest();
		associateMotherhsipAndModIOAccountsRequest.ModIOId = ModIOManager.GetCurrentUserId();
		associateMotherhsipAndModIOAccountsRequest.ModIOToken = ModIOManager.GetCurrentAuthToken();
		associateMotherhsipAndModIOAccountsRequest.MothershipEnvId = MothershipClientApiUnity.EnvironmentId;
		associateMotherhsipAndModIOAccountsRequest.MothershipPlayerId = MothershipClientContext.MothershipId;
		associateMotherhsipAndModIOAccountsRequest.MothershipToken = MothershipClientContext.Token;
		base.StartCoroutine(ModIOManager.AssociateMothershipAndModIOAccounts(associateMotherhsipAndModIOAccountsRequest, delegate(AssociateMotherhsipAndModIOAccountsResponse response)
		{
			Debug.Log("[VirtualStumpOptionsTerminal::OnModIOLoggedIn]... Mothership Account Association Created/Updated");
		}));
	}

	private void OnModIOLoggedOut()
	{
		Debug.Log("[VirtualStumpOptionsTerminal::OnModIOLoggedOut]...");
		ModIOManager.OnModIOLoggedOut.RemoveListener(new UnityAction(this.OnModIOLoggedOut));
		this.processingAccountLink = false;
		this.UpdateScreen();
	}

	private void OnModIOLoginFailed(string error)
	{
		Debug.Log("[VirtualStumpOptionsTerminal::OnModIOLoginFailed] Error: " + error);
		this.processingAccountLink = false;
		this.cachedError = error;
		this.UpdateScreen();
	}

	private void OnModIOUserChanged(User user)
	{
		Debug.Log("[VirtualStumpOptionsTerminal::OnModIOUserChanged] Username: " + ModIOManager.GetCurrentUsername());
		this.UpdateScreen();
	}

	private void OnKeyPressed_ModIOAccount(CustomMapKeyboardBinding pressedButton)
	{
		if (pressedButton == CustomMapKeyboardBinding.option1)
		{
			this.StartAccountLinkingProcess();
		}
		if (pressedButton == CustomMapKeyboardBinding.option2)
		{
			GTDev.Log<string>(string.Format("[VirtualStumpOptionsTerminal::OnKeyPressed_ModIOAccount] logout {0}", ModIOManager.IsLoggedIn()), null);
			if (ModIOManager.IsLoggedIn())
			{
				ModIOManager.LogoutFromModIO();
			}
		}
		if (pressedButton == CustomMapKeyboardBinding.option3)
		{
			GTDev.Log<string>(string.Format("[VirtualStumpOptionsTerminal::OnKeyPressed_ModIOAccount] login {0}", ModIOManager.IsLoggedIn()), null);
			if (!ModIOManager.IsLoggedIn())
			{
				ModIOManager.CancelExternalAuthentication();
				try
				{
					ModIOManager.RequestPlatformLogin();
				}
				catch (Exception ex)
				{
					GTDev.Log<string>(string.Format("VirtualStumpOptionsTerminal::OnKeyPressed_ModIOAccount platform login error: {0}", ex), null);
					throw;
				}
			}
		}
	}

	private async Task StartAccountLinkingProcess()
	{
		if (!this.processingAccountLink)
		{
			this.processingAccountLink = true;
			if (ModIOManager.IsAuthenticated(false))
			{
				if (ModIOManager.GetLastAuthMethod() == ModIOManager.ModIOAuthMethod.LinkedAccount)
				{
					ModIOManager.OnModIOLoggedIn.RemoveListener(new UnityAction(this.OnModIOLoggedIn));
					ModIOManager.OnModIOLoggedIn.AddListener(new UnityAction(this.OnModIOLoggedIn));
					ModIOManager.OnModIOUserChanged.RemoveListener(new UnityAction<User>(this.OnModIOUserChanged));
					ModIOManager.OnModIOUserChanged.AddListener(new UnityAction<User>(this.OnModIOUserChanged));
					this.processingAccountLink = false;
					return;
				}
				ModIOManager.OnModIOLoggedOut.RemoveListener(new UnityAction(this.OnModIOLoggedOut));
				ModIOManager.LogoutFromModIO();
				ModIOManager.OnModIOLoggedIn.RemoveListener(new UnityAction(this.OnModIOLoggedIn));
				ModIOManager.OnModIOLoggedIn.AddListener(new UnityAction(this.OnModIOLoggedIn));
				ModIOManager.OnModIOUserChanged.RemoveListener(new UnityAction<User>(this.OnModIOUserChanged));
				ModIOManager.OnModIOUserChanged.AddListener(new UnityAction<User>(this.OnModIOUserChanged));
			}
			ModIOManager.SetAccountLinkPrompter(this);
			Error error = await ModIOManager.RequestAccountLinkCode();
			if (error)
			{
				Debug.LogError("[ModIOAccountLinkingTerminal::StartAccountLinkingProcess] Failed to log in to mod.io: " + error.GetMessage());
				this.cachedError = error.GetMessage() + "\n\nPRESS THE 'LINK MOD.IO ACCOUNT' BUTTON TO RETRY.";
				this.processingAccountLink = false;
				this.UpdateScreen();
			}
		}
		else
		{
			this.UpdateScreen();
		}
	}

	public void ShowPrompt(string url, string code)
	{
		this.cachedLinkURL = url;
		this.cachedLinkCode = code;
		this.UpdateScreen();
	}

	private string UpdateScreen_ModIOAccount()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (ModIOManager.IsLoggedIn())
		{
			stringBuilder.Append(this.loggedInAsString + "\n");
			stringBuilder.Append("   " + ModIOManager.GetCurrentUsername() + "\n\n");
			if (ModIOManager.GetLastAuthMethod() != ModIOManager.ModIOAuthMethod.LinkedAccount)
			{
				stringBuilder.Append(this.linkAccountPromptString + "\n");
			}
			else
			{
				stringBuilder.Append(this.alreadyLinkedAccountString + "\n");
			}
		}
		else if (ModIOManager.IsLoggingIn() && !this.processingAccountLink)
		{
			stringBuilder.Append(this.loggingInString);
		}
		else if (ModIOManager.IsLoggingOut())
		{
			stringBuilder.Append(this.loggingOutString);
		}
		else if (this.processingAccountLink)
		{
			stringBuilder.Append(this.linkAccountPromptString + "\n\n");
			stringBuilder.Append(this.urlLabelString + this.cachedLinkURL + "\n");
			stringBuilder.Append(this.linkCodeLabelString + this.cachedLinkCode + "\n");
		}
		else
		{
			stringBuilder.Append(this.notLoggedInString + "\n\n");
			stringBuilder.Append(this.loginPromptString);
		}
		return stringBuilder.ToString();
	}

	private void OnKeyPressed_RoomSize(CustomMapKeyboardBinding pressedButton)
	{
		if (pressedButton == CustomMapKeyboardBinding.left)
		{
			this.DecrementRoomSize();
		}
		if (pressedButton == CustomMapKeyboardBinding.right)
		{
			this.IncrementRoomSize();
		}
	}

	private void DecrementRoomSize()
	{
		RoomSystem.OverrideRoomSize(RoomSystem.GetOverridenRoomSize() - 1);
		this.UpdateScreen();
	}

	private void IncrementRoomSize()
	{
		RoomSystem.OverrideRoomSize(RoomSystem.GetOverridenRoomSize() + 1);
		this.UpdateScreen();
	}

	private string UpdateScreen_RoomSize()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(this.roomSizeDescriptionString + "\n\n");
		stringBuilder.Append(this.roomSizeLabelString + RoomSystem.GetOverridenRoomSize().ToString());
		return stringBuilder.ToString();
	}

	[SerializeField]
	private TMP_Text optionList;

	[SerializeField]
	private TMP_Text mainScreenText;

	[SerializeField]
	private CustomMapsKeyboard keyboard;

	[SerializeField]
	private List<string> optionStrings = new List<string> { "MOD.IO", "ROOM SIZE" };

	[SerializeField]
	private string loggedInAsString = "LOGGED INTO MOD.IO AS: ";

	[SerializeField]
	private string notLoggedInString = "LOGGED OUT OF MOD.IO";

	[SerializeField]
	private string loginPromptString = "PRESS THE 'PLATFORM LOGIN' OR 'LINK MOD.IO ACCOUNT' BUTTON TO LOGIN";

	[SerializeField]
	private string loggingInString = "LOGGING IN TO MOD.IO...";

	[SerializeField]
	private string loggingOutString = "LOGGING OUT OF MOD.IO...";

	[SerializeField]
	private string linkAccountPromptString = "IF YOU HAVE AN EXISTING MOD.IO ACCOUNT, YOU CAN LINK IT BY PRESSING THE 'LINK MOD.IO ACCOUNT' BUTTON.";

	[SerializeField]
	private string alreadyLinkedAccountString = "YOU'VE ALREADY LINKED YOUR MOD.IO ACCOUNT.";

	[SerializeField]
	private string accountLinkingPromptString = "PLEASE GO TO THIS URL IN YOUR BROWSER AND LOG IN TO YOUR MOD.IO ACCOUNT. ONCE LOGGED IN, ENTER THE FOLLOWING CODE TO PROCEED: ";

	[SerializeField]
	private string urlLabelString = "URL: ";

	[SerializeField]
	private string linkCodeLabelString = "CODE: ";

	[SerializeField]
	private string roomSizeDescriptionString = "THIS SETTING WILL CHANGE THE MAXIMUM AMOUNT OF PLAYERS ALLOWED IN PRIVATE ROOMS YOU CREATE. WHEN JOINING A PUBLIC ROOM, THE MAP YOU'VE LOADED WILL CONTROL THE ROOM SIZE.";

	[SerializeField]
	private string roomSizeLabelString = "MAX PLAYERS: ";

	[SerializeField]
	private GameObject OKButton;

	[SerializeField]
	private List<GameObject> contextualButtons = new List<GameObject>();

	[SerializeField]
	private List<GameObject> buttonsToShow_MODIO = new List<GameObject>();

	[SerializeField]
	private List<GameObject> buttonsToShow_ROOMSIZE = new List<GameObject>();

	private bool processingAccountLink;

	private string cachedLinkURL = "";

	private string cachedLinkCode = "";

	private string cachedError;

	private VirtualStumpOptionsTerminal.ETerminalState currentState;

	private enum ETerminalState
	{
		MODIO_ACCOUNT,
		ROOM_SIZE,
		NUM_STATES
	}
}
