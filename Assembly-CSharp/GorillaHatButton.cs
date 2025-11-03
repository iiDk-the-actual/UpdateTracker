using System;
using UnityEngine;
using UnityEngine.UI;

[Obsolete("This class is obsolete and will be removed in a future version. (MattO 2024-02-26) It doesn't appear to be used anywhere.")]
public class GorillaHatButton : MonoBehaviour
{
	public void Update()
	{
		if (this.testPress)
		{
			this.testPress = false;
			if (this.touchTime + this.debounceTime < Time.time)
			{
				this.touchTime = Time.time;
				this.isOn = !this.isOn;
				this.buttonParent.PressButton(this.isOn, this.buttonType, this.cosmeticName);
			}
		}
	}

	private void OnTriggerEnter(Collider collider)
	{
		if (this.touchTime + this.debounceTime < Time.time && collider.GetComponentInParent<GorillaTriggerColliderHandIndicator>() != null)
		{
			this.touchTime = Time.time;
			GorillaTriggerColliderHandIndicator component = collider.GetComponent<GorillaTriggerColliderHandIndicator>();
			this.isOn = !this.isOn;
			this.buttonParent.PressButton(this.isOn, this.buttonType, this.cosmeticName);
			if (component != null)
			{
				GorillaTagger.Instance.StartVibration(component.isLeftHand, GorillaTagger.Instance.tapHapticStrength / 2f, GorillaTagger.Instance.tapHapticDuration);
			}
		}
	}

	public void UpdateColor()
	{
		if (this.isOn)
		{
			base.GetComponent<MeshRenderer>().material = this.onMaterial;
			this.myText.text = this.onText;
			return;
		}
		base.GetComponent<MeshRenderer>().material = this.offMaterial;
		this.myText.text = this.offText;
	}

	public GorillaHatButtonParent buttonParent;

	public GorillaHatButton.HatButtonType buttonType;

	public bool isOn;

	public Material offMaterial;

	public Material onMaterial;

	public string offText;

	public string onText;

	public Text myText;

	public float debounceTime = 0.25f;

	public float touchTime;

	public string cosmeticName;

	public bool testPress;

	public enum HatButtonType
	{
		Hat,
		Face,
		Badge
	}
}
