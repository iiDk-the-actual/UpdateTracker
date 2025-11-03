using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.UI;
using Valve.VR;

public class KIDUI_InputFieldController : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	private XRUIInputModule InputModule
	{
		get
		{
			return EventSystem.current.currentInputModule as XRUIInputModule;
		}
	}

	protected void OnEnable()
	{
		if (ControllerBehaviour.Instance)
		{
			ControllerBehaviour.Instance.OnAction += this.PostUpdate;
		}
		SteamVR_Events.System(EVREventType.VREvent_KeyboardClosed).Listen(new UnityAction<VREvent_t>(this.OnKeyboardClosed));
		SteamVR_Events.System(EVREventType.VREvent_KeyboardCharInput).Listen(new UnityAction<VREvent_t>(this.OnChar));
	}

	protected void OnDisable()
	{
		if (ControllerBehaviour.Instance)
		{
			ControllerBehaviour.Instance.OnAction -= this.PostUpdate;
		}
		SteamVR_Events.System(EVREventType.VREvent_KeyboardClosed).Remove(new UnityAction<VREvent_t>(this.OnKeyboardClosed));
		SteamVR_Events.System(EVREventType.VREvent_KeyboardCharInput).Remove(new UnityAction<VREvent_t>(this.OnChar));
	}

	private void Update()
	{
		if (!this.keyboardShowing)
		{
			return;
		}
		SteamVR.instance.overlay.GetKeyboardText(this._inputStringBuilder, 1024U);
		Debug.Log("[KID::INPUTFIELD_CONTROLLER] String BUilder Says: [" + this._inputStringBuilder.ToString() + "]");
		this._inputField.text = this._inputBuffer;
		this._inputField.stringPosition = this._inputBuffer.Length;
	}

	private void PostUpdate()
	{
		if (!this._inputField.interactable || !this.inside)
		{
			return;
		}
		if (ControllerBehaviour.Instance && ControllerBehaviour.Instance.TriggerDown)
		{
			string text = string.Concat(new string[]
			{
				"[",
				base.transform.parent.parent.parent.name,
				".",
				base.transform.parent.parent.name,
				".",
				base.transform.parent.name,
				".",
				base.transform.name,
				"]"
			});
			Debug.Log(string.Concat(new string[]
			{
				"[KID::UIBUTTON::DEBUG] ",
				text,
				" - STEAM - OnClick is pressed. Time: [",
				Time.time.ToString(),
				"]"
			}), this);
			this.OnClickedInputField("");
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		this.inside = true;
		if (!this._inputField.IsInteractable() || !this._inputField.IsActive())
		{
			return;
		}
		XRRayInteractor xrrayInteractor = this.InputModule.GetInteractor(eventData.pointerId) as XRRayInteractor;
		if (!xrrayInteractor)
		{
			return;
		}
		xrrayInteractor.xrController.SendHapticImpulse(this._highlightedVibrationStrength, this._highlightedVibrationDuration);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		this.inside = false;
	}

	private void OnClickedInputField(string _ = "")
	{
		if (this.keyboardShowing)
		{
			return;
		}
		Debug.Log("[KID::INPUT_FIELD_CONTROLLER] Selecting and Activating Input Field");
		EVROverlayError evroverlayError = OpenVR.Overlay.ShowKeyboard(0, 0, 1U, "Enter Email", 1024U, this._inputField.text ?? "", 0UL);
		if (evroverlayError != EVROverlayError.None)
		{
			Debug.LogError("[KID::INPUT_FIELD_CONTROLLER] Failed to open keyboard. Resulted with error: [" + evroverlayError.ToString() + "]");
			return;
		}
		this._inputBuffer = this._inputField.text ?? "";
		this.keyboardShowing = true;
		HandRayController.Instance.DisableHandRays();
	}

	private void OnChar(VREvent_t ev)
	{
		if (!this.keyboardShowing)
		{
			return;
		}
		char c = ev.data.keyboard.cNewInput[0];
		if (c == '\b')
		{
			this._inputBuffer = this._inputBuffer.Remove(this._inputBuffer.Length - 1, 1);
			return;
		}
		if (this.IsIllegalChar(c))
		{
			return;
		}
		this._inputBuffer += c.ToString();
	}

	private void OnKeyboardClosed(VREvent_t ev)
	{
		Debug.Log("[KID::INPUTFIELD_CONTROLLER] Trying to close Keyboard");
		if (!this.keyboardShowing)
		{
			return;
		}
		Debug.Log("[KID::INPUTFIELD_CONTROLLER] Closing Keyboard");
		OpenVR.Overlay.HideKeyboard();
		this._inputField.text = this._inputBuffer;
		this._inputField.DeactivateInputField(false);
		HandRayController.Instance.EnableHandRays();
		this.keyboardShowing = false;
	}

	private bool IsIllegalChar(char c)
	{
		return c == '\t' || c == '\n';
	}

	[Header("Haptics")]
	[SerializeField]
	private float _highlightedVibrationStrength = 0.1f;

	[SerializeField]
	private float _highlightedVibrationDuration = 0.1f;

	[Header("Steam Settings")]
	[SerializeField]
	private TMP_InputField _inputField;

	[SerializeField]
	private UXSettings _cbUXSettings;

	public bool testMinimal;

	public bool minimalMode;

	private bool inside;

	private bool keyboardShowing;

	private bool _canTrigger = true;

	private string _testStr = string.Empty;

	private string previousStr = string.Empty;

	private StringBuilder _inputStringBuilder = new StringBuilder(1024);

	private string _inputBuffer = "";
}
