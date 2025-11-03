using System;
using Fusion;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Splines;

[NetworkBehaviourWeaved(1)]
public class GTSplineAnimateFixedUpdater : NetworkComponent
{
	protected override void Awake()
	{
		base.Awake();
		this.splineAnimateRef.AddCallbackOnLoad(new Action(this.InitSplineAnimate));
		this.splineAnimateRef.AddCallbackOnUnload(new Action(this.ClearSplineAnimate));
	}

	private void InitSplineAnimate()
	{
		this.isSplineLoaded = this.splineAnimateRef.TryResolve<SplineAnimate>(out this.splineAnimate);
		if (this.isSplineLoaded && this.splineAnimate != null)
		{
			this.splineAnimate.enabled = false;
		}
	}

	private void ClearSplineAnimate()
	{
		this.splineAnimate = null;
		this.isSplineLoaded = false;
	}

	private void FixedUpdate()
	{
		if (!base.IsMine && this.progressLerpStartTime + 1f > Time.time)
		{
			if (this.isSplineLoaded)
			{
				this.progress = Mathf.Lerp(this.progressLerpStart, this.progressLerpEnd, (Time.time - this.progressLerpStartTime) / 1f) % this.Duration;
				this.splineAnimate.NormalizedTime = this.progress / this.Duration;
				return;
			}
		}
		else
		{
			this.progress = (this.progress + Time.fixedDeltaTime) % this.Duration;
			if (this.isSplineLoaded)
			{
				this.splineAnimate.NormalizedTime = this.progress / this.Duration;
			}
		}
	}

	[Networked]
	[NetworkedWeaved(0, 1)]
	public unsafe float Netdata
	{
		get
		{
			if (this.Ptr == null)
			{
				throw new InvalidOperationException("Error when accessing GTSplineAnimateFixedUpdater.Netdata. Networked properties can only be accessed when Spawned() has been called.");
			}
			return *(float*)(this.Ptr + 0);
		}
		set
		{
			if (this.Ptr == null)
			{
				throw new InvalidOperationException("Error when accessing GTSplineAnimateFixedUpdater.Netdata. Networked properties can only be accessed when Spawned() has been called.");
			}
			*(float*)(this.Ptr + 0) = value;
		}
	}

	public override void WriteDataFusion()
	{
		this.Netdata = this.progress + 1f;
	}

	public override void ReadDataFusion()
	{
		this.SharedReadData(this.Netdata);
	}

	protected override void WriteDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		if (!info.Sender.IsMasterClient)
		{
			return;
		}
		stream.SendNext(this.progress + 1f);
	}

	protected override void ReadDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		if (!info.Sender.IsMasterClient)
		{
			return;
		}
		float num = (float)stream.ReceiveNext();
		this.SharedReadData(num);
	}

	private void SharedReadData(float incomingValue)
	{
		if (float.IsNaN(incomingValue) || incomingValue > this.Duration + 1f || incomingValue < 0f)
		{
			return;
		}
		this.progressLerpEnd = incomingValue;
		if (this.progressLerpEnd < this.progress)
		{
			if (this.progress < this.Duration)
			{
				this.progressLerpEnd += this.Duration;
			}
			else
			{
				this.progress -= this.Duration;
			}
		}
		this.progressLerpStart = this.progress;
		this.progressLerpStartTime = Time.time;
	}

	[WeaverGenerated]
	public override void CopyBackingFieldsToState(bool A_1)
	{
		base.CopyBackingFieldsToState(A_1);
		this.Netdata = this._Netdata;
	}

	[WeaverGenerated]
	public override void CopyStateToBackingFields()
	{
		base.CopyStateToBackingFields();
		this._Netdata = this.Netdata;
	}

	[SerializeField]
	private XSceneRef splineAnimateRef;

	[SerializeField]
	private float Duration;

	private const float progressLerpDuration = 1f;

	private SplineAnimate splineAnimate;

	private bool isSplineLoaded;

	private float progress;

	private float progressLerpStart;

	private float progressLerpEnd;

	private float progressLerpStartTime;

	[WeaverGenerated]
	[SerializeField]
	[DefaultForProperty("Netdata", 0, 1)]
	[DrawIf("IsEditorWritable", true, CompareOperator.Equal, DrawIfMode.ReadOnly)]
	private float _Netdata;
}
