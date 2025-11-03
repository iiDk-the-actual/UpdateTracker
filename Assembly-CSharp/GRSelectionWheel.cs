using System;
using System.Collections.Generic;
using GorillaExtensions;
using TMPro;
using UnityEngine;

public class GRSelectionWheel : MonoBehaviour, ITickSystemTick
{
	public bool TickRunning { get; set; }

	public void Start()
	{
		this.targetPage = 0;
	}

	private void OnEnable()
	{
		TickSystem<object>.AddTickCallback(this);
	}

	private void OnDisable()
	{
		TickSystem<object>.RemoveTickCallback(this);
	}

	public void ShowText(bool showText)
	{
		foreach (TMP_Text tmp_Text in this.shelfNames)
		{
			tmp_Text.enabled = showText;
		}
	}

	public void InitFromNameList(List<string> shelves)
	{
		this.shelfNames.Clear();
		for (int i = 0; i < shelves.Count; i++)
		{
			TMP_Text tmp_Text = Object.Instantiate<TMP_Text>(this.templateText);
			tmp_Text.text = shelves[i];
			this.shelfNames.Add(tmp_Text);
			tmp_Text.transform.SetParent(base.transform, false);
		}
		this.UpdateVisuals();
	}

	public void Tick()
	{
		if (!this.isBeingDrivenRemotely)
		{
			float num = this.deltaAngle * (float)this.shelfNames.Count;
			float num2 = this.currentAngle / this.deltaAngle;
			int num3 = (int)(num2 + 0.5f);
			if (this.rotSpeedMult == 0f)
			{
				float num4 = ((float)num3 - num2) * this.deltaAngle;
				this.currentAngle += num4 * (1f - Mathf.Exp(-20f * Time.deltaTime));
				this.targetPage = num3;
			}
			else
			{
				this.currentAngle += this.rotSpeedMult * Time.deltaTime * this.rotSpeed;
				this.currentAngle = Mathf.Clamp(this.currentAngle, -this.deltaAngle * 0.4f, num - this.deltaAngle + this.deltaAngle * 0.4f);
			}
		}
		int num5 = (int)(this.currentAngle / this.deltaAngle + 0.5f);
		if (this.lastPlayedAudioTickPage != num5)
		{
			this.lastPlayedAudioTickPage = num5;
			this.audioSource.GTPlay();
		}
		float num6 = 0.005f;
		if (Math.Abs(this.lastAngle - this.currentAngle) > num6)
		{
			this.UpdateVisuals();
		}
		this.lastAngle = this.currentAngle;
	}

	public void SetRotationSpeed(float speed)
	{
		this.rotSpeedMult = Mathf.Sign(speed) * Mathf.Pow(Mathf.Abs(speed), 2f);
	}

	public void SetTargetShelf(int shelf)
	{
		this.currentAngle += (float)(shelf - this.targetPage) * this.deltaAngle;
		this.targetPage = shelf;
	}

	public void SetTargetAngle(float angle)
	{
		this.currentAngle = angle;
	}

	public void UpdateVisuals()
	{
		this.rotationWheel.localRotation = Quaternion.Euler(-this.currentAngle + 7.5f, 0f, 0f);
		float num = this.deltaAngle;
		int count = this.shelfNames.Count;
		float num2 = this.currentAngle / this.deltaAngle;
		for (int i = 0; i < this.shelfNames.Count; i++)
		{
			float num3 = ((float)i - num2) * this.deltaAngle + this.pointerOffsetAngle;
			float num4 = num3 * 3.1415927f / 180f;
			float num5 = Mathf.Cos(num4);
			float num6 = Mathf.Sin(num4);
			Quaternion quaternion = Quaternion.Euler(90f - num3, 180f, 0f);
			Vector3 vector = new Vector3(this.textHorizOffset, num5 * this.wheelTextRadius, num6 * this.wheelTextRadius);
			this.shelfNames[i].transform.rotation = base.transform.TransformRotation(quaternion);
			this.shelfNames[i].transform.position = base.transform.TransformPoint(vector);
			this.shelfNames[i].color = ((Math.Abs(num2 - (float)i) < 0.5f) ? Color.green : Color.white);
		}
	}

	private List<TMP_Text> shelfNames = new List<TMP_Text>();

	public TMP_Text templateText;

	public float deltaAngle;

	public float pointerOffsetAngle;

	public float wheelTextRadius;

	public float textHorizOffset = -0.0375f;

	public float rotSpeed = 60f;

	public bool isBeingDrivenRemotely;

	public AudioSource audioSource;

	public int lastPlayedAudioTickPage = -1;

	public float wheelTextPairOffset = 0.0025f;

	public Transform rotationWheel;

	public float lastAngle = -1000f;

	[NonSerialized]
	public int targetPage;

	[NonSerialized]
	public float currentAngle;

	private float rotSpeedMult;
}
