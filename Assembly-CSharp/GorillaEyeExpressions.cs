using System;
using UnityEngine;

public class GorillaEyeExpressions : MonoBehaviour, IGorillaSliceableSimple
{
	private void Awake()
	{
		this.loudness = base.GetComponent<GorillaSpeakerLoudness>();
	}

	public void OnEnable()
	{
		GorillaSlicerSimpleManager.RegisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.LateUpdate);
		this.timeLastUpdated = Time.time;
		this.deltaTime = Time.deltaTime;
	}

	public void OnDisable()
	{
		GorillaSlicerSimpleManager.UnregisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.LateUpdate);
	}

	public void SliceUpdate()
	{
		this.deltaTime = Time.time - this.timeLastUpdated;
		this.timeLastUpdated = Time.time;
		this.CheckEyeEffects();
		this.UpdateEyeExpression();
	}

	private void CheckEyeEffects()
	{
		if (this.loudness == null)
		{
			this.loudness = base.GetComponent<GorillaSpeakerLoudness>();
		}
		if (this.loudness.IsSpeaking && this.loudness.Loudness > this.screamVolume)
		{
			this.overrideDuration = this.screamDuration;
			this.overrideUV = this.ScreamUV;
			return;
		}
		if (this.overrideDuration > 0f)
		{
			this.overrideDuration -= this.deltaTime;
			if (this.overrideDuration <= 0f)
			{
				this.overrideUV = this.BaseUV;
			}
		}
	}

	private void UpdateEyeExpression()
	{
		this.targetFace.GetComponent<Renderer>().material.SetVector(this._BaseMap_ST, new Vector4(0.5f, 1f, this.overrideUV.x, this.overrideUV.y));
	}

	public GameObject targetFace;

	[Space]
	[SerializeField]
	private float screamVolume = 0.2f;

	[SerializeField]
	private float screamDuration = 0.5f;

	[SerializeField]
	private Vector2 ScreamUV = new Vector2(0.8f, 0f);

	private Vector2 BaseUV = Vector3.zero;

	private GorillaSpeakerLoudness loudness;

	private float overrideDuration;

	private Vector2 overrideUV;

	private float timeLastUpdated;

	private float deltaTime;

	private ShaderHashId _BaseMap_ST = "_BaseMap_ST";
}
