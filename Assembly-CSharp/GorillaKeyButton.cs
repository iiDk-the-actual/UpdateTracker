using System;
using System.Collections;
using System.Runtime.CompilerServices;
using GorillaExtensions;
using GorillaTag;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

public abstract class GorillaKeyButton<TBinding> : MonoBehaviour where TBinding : Enum
{
	private void Awake()
	{
		if (this.ButtonRenderer == null)
		{
			this.ButtonRenderer = base.GetComponent<Renderer>();
		}
		this.propBlock = new MaterialPropertyBlock();
		this.pressTime = 0f;
	}

	private void OnEnable()
	{
		for (int i = 0; i < this.linkedObjects.Length; i++)
		{
			if (this.linkedObjects[i].IsNotNull())
			{
				this.linkedObjects[i].SetActive(true);
			}
		}
		this.OnEnableEvents();
	}

	private void OnDisable()
	{
		for (int i = 0; i < this.linkedObjects.Length; i++)
		{
			if (this.linkedObjects[i].IsNotNull())
			{
				this.linkedObjects[i].SetActive(false);
			}
		}
		this.OnDisableEvents();
	}

	private void OnTriggerEnter(Collider collider)
	{
		GorillaTriggerColliderHandIndicator componentInParent = collider.GetComponentInParent<GorillaTriggerColliderHandIndicator>();
		if (componentInParent)
		{
			this.PressButton(componentInParent.isLeftHand);
		}
	}

	private void PressButton(bool isLeftHand)
	{
		this.OnButtonPressedEvent();
		UnityEvent<TBinding> onKeyButtonPressed = this.OnKeyButtonPressed;
		if (onKeyButtonPressed != null)
		{
			onKeyButtonPressed.Invoke(this.Binding);
		}
		this.PressButtonColourUpdate();
		GorillaTagger.Instance.StartVibration(isLeftHand, GorillaTagger.Instance.tapHapticStrength / 2f, GorillaTagger.Instance.tapHapticDuration);
		GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(66, isLeftHand, 0.1f);
		if (NetworkSystem.Instance.InRoom && GorillaTagger.Instance.myVRRig != null)
		{
			GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlayHandTap", RpcTarget.Others, new object[] { 66, isLeftHand, 0.1f });
		}
	}

	protected virtual void OnEnableEvents()
	{
	}

	protected virtual void OnDisableEvents()
	{
	}

	public void Click(bool leftHand = false)
	{
		this.PressButton(leftHand);
	}

	public virtual void PressButtonColourUpdate()
	{
		if (!base.gameObject.activeInHierarchy)
		{
			return;
		}
		this.propBlock.SetColor(ShaderProps._BaseColor, this.ButtonColorSettings.PressedColor);
		this.propBlock.SetColor(ShaderProps._Color, this.ButtonColorSettings.PressedColor);
		this.ButtonRenderer.SetPropertyBlock(this.propBlock);
		this.pressTime = Time.time;
		base.StartCoroutine(this.<PressButtonColourUpdate>g__ButtonColorUpdate_Local|21_0());
	}

	protected abstract void OnButtonPressedEvent();

	[CompilerGenerated]
	private IEnumerator <PressButtonColourUpdate>g__ButtonColorUpdate_Local|21_0()
	{
		yield return new WaitForSeconds(this.ButtonColorSettings.PressedTime);
		if (this.pressTime != 0f && Time.time > this.ButtonColorSettings.PressedTime + this.pressTime)
		{
			this.propBlock.SetColor(ShaderProps._BaseColor, this.ButtonColorSettings.UnpressedColor);
			this.propBlock.SetColor(ShaderProps._Color, this.ButtonColorSettings.UnpressedColor);
			this.ButtonRenderer.SetPropertyBlock(this.propBlock);
			this.pressTime = 0f;
		}
		yield break;
	}

	public string characterString;

	public TBinding Binding;

	public bool functionKey;

	public Renderer ButtonRenderer;

	public ButtonColorSettings ButtonColorSettings;

	[Tooltip("These GameObjects will be Activated/Deactivated when this button is Activated/Deactivated")]
	public GameObject[] linkedObjects;

	[Tooltip("Intended for use with GorillaKeyWrapper")]
	public UnityEvent<TBinding> OnKeyButtonPressed = new UnityEvent<TBinding>();

	public bool testClick;

	public bool repeatTestClick;

	public float repeatCooldown = 2f;

	private float pressTime;

	private float lastTestClick;

	protected MaterialPropertyBlock propBlock;
}
