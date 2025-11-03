using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class GorillaPhysicalButton : MonoBehaviour
{
	public event Action<GorillaPhysicalButton, bool> onPressedOn;

	public event Action<GorillaPhysicalButton, bool> onToggledOff;

	public virtual void Start()
	{
		if (this.moveableChildren != null)
		{
			this.moveableChildrenStartPositions = new List<Vector3>(this.moveableChildren.Count);
			for (int i = 0; i < this.moveableChildren.Count; i++)
			{
				this.moveableChildrenStartPositions.Add(this.moveableChildren[i].position);
			}
		}
		this.startButtonPosition = base.transform.position;
		base.enabled = true;
	}

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
	}

	private float GetSurfaceDistanceFromKeyToCollider(Collider collider)
	{
		if (collider == null)
		{
			return 1f;
		}
		SphereCollider sphereCollider = collider as SphereCollider;
		float num = (sphereCollider ? sphereCollider.radius : 0f);
		float num2 = base.transform.localScale.z * 0.5f;
		if (Vector3.Distance(collider.transform.position, base.transform.position) > (base.transform.localScale.magnitude * 0.5f + num) * 1.5f)
		{
			return 1f;
		}
		return Vector3.Dot(base.transform.position - collider.transform.position, -base.transform.forward) - num - num2;
	}

	protected void OnTriggerEnter(Collider other)
	{
		if (!base.enabled)
		{
			return;
		}
		if (other.GetComponentInParent<GorillaTriggerColliderHandIndicator>() == null)
		{
			return;
		}
		this.recentFingerCollider = other;
		if (this.buttonTestCoroutine == null)
		{
			this.buttonTestCoroutine = base.StartCoroutine(this.ButtonUpdate());
		}
	}

	protected IEnumerator ButtonUpdate()
	{
		for (;;)
		{
			this.UpdateButtonFromCollider();
			if (!base.enabled || this.recentFingerCollider == null)
			{
				break;
			}
			yield return null;
		}
		this.buttonTestCoroutine = null;
		yield break;
	}

	protected void UpdateButtonFromCollider()
	{
		if (this.recentFingerCollider != null)
		{
			float surfaceDistanceFromKeyToCollider = this.GetSurfaceDistanceFromKeyToCollider(this.recentFingerCollider);
			this.currentButtonDepthFromPressing -= surfaceDistanceFromKeyToCollider;
			this.currentButtonDepthFromPressing = Mathf.Clamp(this.currentButtonDepthFromPressing, 0f, this.buttonPushDepth);
		}
		else
		{
			this.currentButtonDepthFromPressing = 0f;
		}
		if (this.currentButtonDepthFromPressing == 0f)
		{
			if (!this.canToggleOn && !this.canToggleOff)
			{
				this.isOn = false;
			}
			this.recentFingerCollider = null;
			this.waitingForReleaseAfterStateChange = false;
		}
		this.TestForButtonStateChange();
		this.UpdateButtonVisuals();
	}

	protected void TestForButtonStateChange()
	{
		if (this.waitingForReleaseAfterStateChange)
		{
			return;
		}
		if (this.currentButtonDepthFromPressing > this.buttonDepthForTrigger && !this.isOn && this.recentFingerCollider != null)
		{
			this.isOn = true;
			this.waitingForReleaseAfterStateChange = true;
			GorillaTriggerColliderHandIndicator component = this.recentFingerCollider.GetComponent<GorillaTriggerColliderHandIndicator>();
			if (component == null)
			{
				return;
			}
			UnityEvent unityEvent = this.onPressButtonOn;
			if (unityEvent != null)
			{
				unityEvent.Invoke();
			}
			Action<GorillaPhysicalButton, bool> action = this.onPressedOn;
			if (action != null)
			{
				action(this, component.isLeftHand);
			}
			this.ButtonPressedOn();
			this.ButtonPressedOnWithHand(component.isLeftHand);
			GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(this.pressButtonSoundIndex, component.isLeftHand, 0.05f);
			GorillaTagger.Instance.StartVibration(component.isLeftHand, GorillaTagger.Instance.tapHapticStrength / 2f, GorillaTagger.Instance.tapHapticDuration);
			if (NetworkSystem.Instance.InRoom && GorillaTagger.Instance.myVRRig != null)
			{
				GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlayHandTap", RpcTarget.Others, new object[] { 67, component.isLeftHand, 0.05f });
				return;
			}
		}
		else if (this.currentButtonDepthFromPressing > this.buttonDepthForTrigger && this.canToggleOff && this.isOn && this.recentFingerCollider != null)
		{
			this.isOn = false;
			this.waitingForReleaseAfterStateChange = true;
			GorillaTriggerColliderHandIndicator component2 = this.recentFingerCollider.GetComponent<GorillaTriggerColliderHandIndicator>();
			if (component2 == null)
			{
				return;
			}
			UnityEvent unityEvent2 = this.onPressButtonToggleOff;
			if (unityEvent2 != null)
			{
				unityEvent2.Invoke();
			}
			Action<GorillaPhysicalButton, bool> action2 = this.onToggledOff;
			if (action2 != null)
			{
				action2(this, component2.isLeftHand);
			}
			this.ButtonToggledOff();
			this.ButtonToggledOffWithHand(component2.isLeftHand);
			GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(this.pressButtonSoundIndex, component2.isLeftHand, 0.05f);
			GorillaTagger.Instance.StartVibration(component2.isLeftHand, GorillaTagger.Instance.tapHapticStrength / 2f, GorillaTagger.Instance.tapHapticDuration);
			if (NetworkSystem.Instance.InRoom && GorillaTagger.Instance.myVRRig != null)
			{
				GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlayHandTap", RpcTarget.Others, new object[] { 67, component2.isLeftHand, 0.05f });
			}
		}
	}

	protected void UpdateButtonVisuals()
	{
		float num = this.currentButtonDepthFromPressing;
		if ((this.canToggleOff || this.canToggleOn) && this.isOn)
		{
			num = Mathf.Max(this.buttonDepthForTrigger, num);
		}
		base.transform.position = this.startButtonPosition - base.transform.forward * num;
		if (this.moveableChildren != null)
		{
			for (int i = 0; i < this.moveableChildren.Count; i++)
			{
				this.moveableChildren[i].position = this.moveableChildrenStartPositions[i] - base.transform.forward * num;
			}
		}
		this.UpdateColorWithState(this.isOn);
	}

	protected void UpdateColorWithState(bool state)
	{
		if (state)
		{
			this.buttonRenderer.material = this.pressedMaterial;
			if ((!string.IsNullOrEmpty(this.onText) || !string.IsNullOrEmpty(this.offText)) && this.textField != null)
			{
				this.textField.text = this.onText;
				return;
			}
		}
		else
		{
			this.buttonRenderer.material = this.unpressedMaterial;
			if ((!string.IsNullOrEmpty(this.offText) || !string.IsNullOrEmpty(this.onText)) && this.textField != null)
			{
				this.textField.text = this.offText;
			}
		}
	}

	public virtual void ButtonPressedOn()
	{
	}

	public virtual void ButtonPressedOnWithHand(bool isLeftHand)
	{
	}

	public virtual void ButtonToggledOff()
	{
	}

	public virtual void ButtonToggledOffWithHand(bool isLeftHand)
	{
	}

	public virtual void ResetState()
	{
		this.isOn = false;
		this.currentButtonDepthFromPressing = 0f;
		this.waitingForReleaseAfterStateChange = false;
		this.UpdateButtonVisuals();
	}

	public void SetText(string newText)
	{
		if (this.textField != null)
		{
			this.textField.text = this.offText;
		}
	}

	public virtual void SetButtonState(bool setToOn)
	{
		if (this.canToggleOn || this.canToggleOff)
		{
			if (this.isOn != setToOn)
			{
				this.isOn = setToOn;
				if (this.isOn)
				{
					UnityEvent unityEvent = this.onPressButtonOn;
					if (unityEvent != null)
					{
						unityEvent.Invoke();
					}
					this.ButtonPressedOn();
				}
				else
				{
					UnityEvent unityEvent2 = this.onPressButtonToggleOff;
					if (unityEvent2 != null)
					{
						unityEvent2.Invoke();
					}
					this.ButtonToggledOff();
				}
			}
			this.UpdateButtonVisuals();
		}
	}

	public Material pressedMaterial;

	public Material unpressedMaterial;

	public MeshRenderer buttonRenderer;

	public int pressButtonSoundIndex = 67;

	[SerializeField]
	public bool canToggleOn;

	public bool canToggleOff;

	private bool waitingForReleaseAfterStateChange;

	public bool isOn;

	public bool testPress;

	public bool testHandLeft;

	[SerializeField]
	protected float buttonPushDepth = 0.0125f;

	[SerializeField]
	protected float buttonDepthForTrigger = 0.01f;

	[SerializeField]
	public List<Transform> moveableChildren;

	[NonSerialized]
	public List<Vector3> moveableChildrenStartPositions;

	private Vector3 startButtonPosition;

	[TextArea]
	public string offText = "OFF";

	[TextArea]
	public string onText = "ON";

	[SerializeField]
	public TMP_Text textField;

	[Space]
	public UnityEvent onPressButtonOn;

	public UnityEvent onPressButtonToggleOff;

	private Collider recentFingerCollider;

	protected float currentButtonDepthFromPressing;

	private Coroutine buttonTestCoroutine;
}
