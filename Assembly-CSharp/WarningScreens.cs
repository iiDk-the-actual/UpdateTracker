using System;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class WarningScreens : MonoBehaviour
{
	private void Awake()
	{
		if (WarningScreens._activeReference == null)
		{
			WarningScreens._activeReference = this;
			return;
		}
		Debug.LogError("[WARNINGS] WarningScreens already exists. Destroying this instance.");
		Object.Destroy(this);
	}

	private async Task<WarningButtonResult> StartWarningScreenInternal(CancellationToken cancellationToken)
	{
		WarningScreens._closedMessageBox = false;
		WarningScreens._result = WarningButtonResult.CloseWarning;
		PlayerAgeGateWarningStatus? playerAgeGateWarningStatus = await WarningsServer.Instance.FetchPlayerData(cancellationToken);
		WarningButtonResult warningButtonResult;
		if (cancellationToken.IsCancellationRequested || playerAgeGateWarningStatus == null)
		{
			warningButtonResult = WarningButtonResult.None;
		}
		else
		{
			PlayerAgeGateWarningStatus value = playerAgeGateWarningStatus.Value;
			if (value.header.IsNullOrEmpty() || value.body.IsNullOrEmpty())
			{
				Debug.Log("[WARNINGS] Not showing warning screen.");
				warningButtonResult = WarningButtonResult.None;
			}
			else
			{
				this._messageBox.Header = value.header;
				this._messageBox.Body = value.body;
				this._messageBox.LeftButton = value.leftButtonText;
				this._messageBox.RightButton = value.rightButtonText;
				WarningScreens._leftButtonResult = value.leftButtonResult;
				WarningScreens._rightButtonResult = value.rightButtonResult;
				this._onLeftButtonPressedAction = value.onLeftButtonPressedAction;
				this._onRightButtonPressedAction = value.onRightButtonPressedAction;
				if (this._imageContainerAfter && this._withImageTextBefore && this._imageContainerBefore && this._withImageTextAfter && this._noImageText)
				{
					this._imageContainerAfter.SetActive(value.showImage == EImageVisibility.AfterBody);
					this._imageContainerBefore.SetActive(value.showImage == EImageVisibility.BeforeBody);
					this._withImageTextBefore.text = value.body;
					this._withImageTextBefore.gameObject.SetActive(value.showImage == EImageVisibility.AfterBody);
					this._withImageTextAfter.text = value.body;
					this._withImageTextAfter.gameObject.SetActive(value.showImage == EImageVisibility.BeforeBody);
					this._noImageText.gameObject.SetActive(value.showImage == EImageVisibility.None);
				}
				this._messageBox.gameObject.SetActive(true);
				GameObject canvas = this._messageBox.GetCanvas();
				PrivateUIRoom.AddUI(canvas.transform);
				HandRayController.Instance.EnableHandRays();
				await WarningScreens.WaitForResponse(cancellationToken);
				HandRayController.Instance.DisableHandRays();
				PrivateUIRoom.RemoveUI(canvas.transform);
				this._messageBox.gameObject.SetActive(false);
				warningButtonResult = WarningScreens._result;
			}
		}
		return warningButtonResult;
	}

	private async Task<WarningButtonResult> StartOptInFollowUpScreenInternal(CancellationToken cancellationToken)
	{
		WarningScreens._closedMessageBox = false;
		WarningScreens._result = WarningButtonResult.CloseWarning;
		PlayerAgeGateWarningStatus? playerAgeGateWarningStatus = await WarningsServer.Instance.GetOptInFollowUpMessage(cancellationToken);
		WarningButtonResult warningButtonResult;
		if (cancellationToken.IsCancellationRequested || playerAgeGateWarningStatus == null)
		{
			warningButtonResult = WarningButtonResult.None;
		}
		else
		{
			Debug.Log("[KID::WARNING_SCREEN] Body: " + playerAgeGateWarningStatus.Value.body);
			this._messageBox.Header = playerAgeGateWarningStatus.Value.header;
			this._messageBox.Body = playerAgeGateWarningStatus.Value.body;
			this._messageBox.LeftButton = playerAgeGateWarningStatus.Value.leftButtonText;
			this._messageBox.RightButton = playerAgeGateWarningStatus.Value.rightButtonText;
			WarningScreens._leftButtonResult = playerAgeGateWarningStatus.Value.leftButtonResult;
			WarningScreens._rightButtonResult = playerAgeGateWarningStatus.Value.rightButtonResult;
			this._onLeftButtonPressedAction = playerAgeGateWarningStatus.Value.onLeftButtonPressedAction;
			this._onRightButtonPressedAction = playerAgeGateWarningStatus.Value.onRightButtonPressedAction;
			if (this._imageContainerAfter && this._withImageTextBefore && this._imageContainerBefore && this._withImageTextAfter && this._noImageText)
			{
				this._imageContainerAfter.SetActive(playerAgeGateWarningStatus.Value.showImage == EImageVisibility.AfterBody);
				this._imageContainerBefore.SetActive(playerAgeGateWarningStatus.Value.showImage == EImageVisibility.BeforeBody);
				this._withImageTextBefore.text = playerAgeGateWarningStatus.Value.body;
				this._withImageTextBefore.gameObject.SetActive(playerAgeGateWarningStatus.Value.showImage == EImageVisibility.AfterBody);
				this._withImageTextAfter.text = playerAgeGateWarningStatus.Value.body;
				this._withImageTextAfter.gameObject.SetActive(playerAgeGateWarningStatus.Value.showImage == EImageVisibility.BeforeBody);
				this._noImageText.gameObject.SetActive(playerAgeGateWarningStatus.Value.showImage == EImageVisibility.None);
			}
			this._messageBox.gameObject.SetActive(true);
			GameObject canvas = this._messageBox.GetCanvas();
			PrivateUIRoom.AddUI(canvas.transform);
			HandRayController.Instance.EnableHandRays();
			await WarningScreens.WaitForResponse(cancellationToken);
			HandRayController.Instance.DisableHandRays();
			PrivateUIRoom.RemoveUI(canvas.transform);
			this._messageBox.gameObject.SetActive(false);
			warningButtonResult = WarningScreens._result;
		}
		return warningButtonResult;
	}

	public static async Task<WarningButtonResult> StartWarningScreen(CancellationToken cancellationToken)
	{
		return await WarningScreens._activeReference.StartWarningScreenInternal(cancellationToken);
	}

	public static async Task<WarningButtonResult> StartOptInFollowUpScreen(CancellationToken cancellationToken)
	{
		return await WarningScreens._activeReference.StartOptInFollowUpScreenInternal(cancellationToken);
	}

	private static async Task WaitForResponse(CancellationToken cancellationToken)
	{
		while (!WarningScreens._closedMessageBox)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return;
			}
			await Task.Yield();
		}
	}

	public void OnDisable()
	{
		KIDAudioManager instance = KIDAudioManager.Instance;
		if (instance == null)
		{
			return;
		}
		instance.PlaySoundWithDelay(KIDAudioManager.KIDSoundType.PageTransition);
	}

	public static void OnLeftButtonClicked()
	{
		WarningScreens._result = WarningScreens._leftButtonResult;
		WarningScreens._closedMessageBox = true;
		WarningScreens activeReference = WarningScreens._activeReference;
		if (activeReference == null)
		{
			return;
		}
		Action onLeftButtonPressedAction = activeReference._onLeftButtonPressedAction;
		if (onLeftButtonPressedAction == null)
		{
			return;
		}
		onLeftButtonPressedAction();
	}

	public static void OnRightButtonClicked()
	{
		WarningScreens._result = WarningScreens._rightButtonResult;
		WarningScreens._closedMessageBox = true;
		WarningScreens activeReference = WarningScreens._activeReference;
		if (activeReference == null)
		{
			return;
		}
		Action onRightButtonPressedAction = activeReference._onRightButtonPressedAction;
		if (onRightButtonPressedAction == null)
		{
			return;
		}
		onRightButtonPressedAction();
	}

	private static WarningScreens _activeReference;

	[SerializeField]
	private MessageBox _messageBox;

	[SerializeField]
	private GameObject _imageContainerAfter;

	[SerializeField]
	private GameObject _imageContainerBefore;

	[SerializeField]
	private TMP_Text _withImageTextBefore;

	[SerializeField]
	private TMP_Text _withImageTextAfter;

	[SerializeField]
	private TMP_Text _noImageText;

	private Action _onLeftButtonPressedAction;

	private Action _onRightButtonPressedAction;

	private static WarningButtonResult _result;

	private static WarningButtonResult _leftButtonResult;

	private static WarningButtonResult _rightButtonResult;

	private static bool _closedMessageBox;
}
