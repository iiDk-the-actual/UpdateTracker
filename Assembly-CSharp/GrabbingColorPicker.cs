using System;
using GorillaLocomotion;
using GorillaNetworking;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class GrabbingColorPicker : MonoBehaviour, IGorillaSliceableSimple
{
	private void Start()
	{
		if (!this.setPlayerColor)
		{
			return;
		}
		float @float = PlayerPrefs.GetFloat("redValue", 0f);
		float float2 = PlayerPrefs.GetFloat("greenValue", 0f);
		float float3 = PlayerPrefs.GetFloat("blueValue", 0f);
		this.LoadPlayerColor(@float, float2, float3);
	}

	private void LoadPlayerColor(float r, float g, float b)
	{
		this.Segment1 = Mathf.RoundToInt(Mathf.Lerp(0f, 9f, r));
		this.Segment2 = Mathf.RoundToInt(Mathf.Lerp(0f, 9f, g));
		this.Segment3 = Mathf.RoundToInt(Mathf.Lerp(0f, 9f, b));
		this.R_PushSlider.SetProgress(r);
		this.G_PushSlider.SetProgress(g);
		this.B_PushSlider.SetProgress(b);
		this.UpdateDisplay();
	}

	public void OnEnable()
	{
		GorillaSlicerSimpleManager.RegisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
		if (!this.setPlayerColor)
		{
			return;
		}
		CosmeticsController.OnPlayerColorSet = (Action<float, float, float>)Delegate.Combine(CosmeticsController.OnPlayerColorSet, new Action<float, float, float>(this.LoadPlayerColor));
		if (GorillaTagger.Instance && GorillaTagger.Instance.offlineVRRig)
		{
			GorillaTagger.Instance.offlineVRRig.OnColorChanged += this.HandleLocalColorChanged;
		}
	}

	public void OnDisable()
	{
		GorillaSlicerSimpleManager.UnregisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
		if (!this.setPlayerColor)
		{
			return;
		}
		CosmeticsController.OnPlayerColorSet = (Action<float, float, float>)Delegate.Remove(CosmeticsController.OnPlayerColorSet, new Action<float, float, float>(this.LoadPlayerColor));
		if (GorillaTagger.Instance && GorillaTagger.Instance.offlineVRRig)
		{
			GorillaTagger.Instance.offlineVRRig.OnColorChanged -= this.HandleLocalColorChanged;
		}
	}

	public void SliceUpdate()
	{
		float num = Vector3.Distance(base.transform.position, GTPlayer.Instance.transform.position);
		this.hasUpdated = false;
		if (num < 5f)
		{
			int segment = this.Segment1;
			int segment2 = this.Segment2;
			int segment3 = this.Segment3;
			this.Segment1 = Mathf.RoundToInt(Mathf.Lerp(0f, 9f, this.R_PushSlider.GetProgress()));
			this.Segment2 = Mathf.RoundToInt(Mathf.Lerp(0f, 9f, this.G_PushSlider.GetProgress()));
			this.Segment3 = Mathf.RoundToInt(Mathf.Lerp(0f, 9f, this.B_PushSlider.GetProgress()));
			if (segment != this.Segment1 || segment2 != this.Segment2 || segment3 != this.Segment3)
			{
				this.hasUpdated = true;
				if (this.setPlayerColor)
				{
					this.SetPlayerColor();
				}
				this.UpdateDisplay();
				this.UpdateColor.Invoke(new Vector3((float)this.Segment1 / 9f, (float)this.Segment2 / 9f, (float)this.Segment3 / 9f));
				if (segment != this.Segment1)
				{
					this.R_SliderAudio.transform.position = this.R_PushSlider.transform.position;
					this.R_SliderAudio.GTPlay();
				}
				if (segment2 != this.Segment2)
				{
					this.G_SliderAudio.transform.position = this.G_PushSlider.transform.position;
					this.G_SliderAudio.GTPlay();
				}
				if (segment3 != this.Segment3)
				{
					this.B_SliderAudio.transform.position = this.B_PushSlider.transform.position;
					this.B_SliderAudio.GTPlay();
				}
			}
		}
	}

	private void SetPlayerColor()
	{
		PlayerPrefs.SetFloat("redValue", (float)this.Segment1 / 9f);
		PlayerPrefs.SetFloat("greenValue", (float)this.Segment2 / 9f);
		PlayerPrefs.SetFloat("blueValue", (float)this.Segment3 / 9f);
		GorillaTagger.Instance.UpdateColor((float)this.Segment1 / 9f, (float)this.Segment2 / 9f, (float)this.Segment3 / 9f);
		GorillaComputer.instance.UpdateColor((float)this.Segment1 / 9f, (float)this.Segment2 / 9f, (float)this.Segment3 / 9f);
		PlayerPrefs.Save();
		if (NetworkSystem.Instance.InRoom)
		{
			GorillaTagger.Instance.myVRRig.SendRPC("RPC_InitializeNoobMaterial", RpcTarget.All, new object[]
			{
				(float)this.Segment1 / 9f,
				(float)this.Segment2 / 9f,
				(float)this.Segment3 / 9f
			});
		}
	}

	private void SetSliderColors(float r, float g, float b)
	{
		if (!this.hasUpdated)
		{
			this.Segment1 = Mathf.RoundToInt(Mathf.Lerp(0f, 9f, r));
			this.Segment2 = Mathf.RoundToInt(Mathf.Lerp(0f, 9f, g));
			this.Segment3 = Mathf.RoundToInt(Mathf.Lerp(0f, 9f, b));
			this.R_PushSlider.SetProgress(r);
			this.G_PushSlider.SetProgress(g);
			this.B_PushSlider.SetProgress(b);
			this.UpdateDisplay();
		}
	}

	private void HandleLocalColorChanged(Color newColor)
	{
		this.SetSliderColors(newColor.r, newColor.g, newColor.b);
	}

	private void UpdateDisplay()
	{
		this.textR.text = this.Segment1.ToString();
		this.textG.text = this.Segment2.ToString();
		this.textB.text = this.Segment3.ToString();
		Color color = new Color((float)this.Segment1 / 9f, (float)this.Segment2 / 9f, (float)this.Segment3 / 9f);
		Renderer[] componentsInChildren = this.ColorSwatch.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Material[] materials = componentsInChildren[i].materials;
			for (int j = 0; j < materials.Length; j++)
			{
				materials[j].color = color;
			}
		}
	}

	public void ResetSliders(Vector3 v)
	{
		this.SetSliderColors(v.x, v.y, v.z);
	}

	[SerializeField]
	private bool setPlayerColor = true;

	[SerializeField]
	private PushableSlider R_PushSlider;

	[SerializeField]
	private PushableSlider G_PushSlider;

	[SerializeField]
	private PushableSlider B_PushSlider;

	[SerializeField]
	private AudioSource R_SliderAudio;

	[SerializeField]
	private AudioSource G_SliderAudio;

	[SerializeField]
	private AudioSource B_SliderAudio;

	[SerializeField]
	private TextMeshPro textR;

	[SerializeField]
	private TextMeshPro textG;

	[SerializeField]
	private TextMeshPro textB;

	[SerializeField]
	private GameObject ColorSwatch;

	[SerializeField]
	private UnityEvent<Vector3> UpdateColor;

	private int Segment1;

	private int Segment2;

	private int Segment3;

	private bool hasUpdated;
}
