using System;
using GTMathUtil;
using Photon.Pun;
using UnityEngine;

public class MovingPlatform : BasePlatform
{
	public float InitTimeOffset()
	{
		return this.startPercentage * this.cycleLength;
	}

	private long InitTimeOffsetMs()
	{
		return (long)(this.InitTimeOffset() * 1000f);
	}

	private long NetworkTimeMs()
	{
		if (PhotonNetwork.InRoom)
		{
			return (long)((ulong)(PhotonNetwork.ServerTimestamp + int.MinValue) + (ulong)this.InitTimeOffsetMs());
		}
		return (long)(Time.time * 1000f);
	}

	private long CycleLengthMs()
	{
		return (long)(this.cycleLength * 1000f);
	}

	public double PlatformTime()
	{
		long num = this.NetworkTimeMs();
		long num2 = this.CycleLengthMs();
		return (double)(num - num / num2 * num2) / 1000.0;
	}

	public int CycleCount()
	{
		return (int)(this.NetworkTimeMs() / this.CycleLengthMs());
	}

	public float CycleCompletionPercent()
	{
		float num = (float)(this.PlatformTime() / (double)this.cycleLength);
		num = Mathf.Clamp(num, 0f, 1f);
		if (this.startDelay > 0f)
		{
			float num2 = this.startDelay / this.cycleLength;
			if (num <= num2)
			{
				num = 0f;
			}
			else
			{
				num = (num - num2) / (1f - num2);
			}
		}
		return num;
	}

	public bool CycleForward()
	{
		return (this.CycleCount() + (this.startNextCycle ? 1 : 0)) % 2 == 0;
	}

	private void Awake()
	{
		if (this.platformType == MovingPlatform.PlatformType.Child)
		{
			return;
		}
		this.rb = base.GetComponent<Rigidbody>();
		this.initLocalRotation = base.transform.localRotation;
		if (this.pivot != null)
		{
			this.initOffset = this.pivot.transform.position - this.startXf.transform.position;
		}
		this.startPos = this.startXf.position;
		this.endPos = this.endXf.position;
		this.startRot = this.startXf.rotation;
		this.endRot = this.endXf.rotation;
		this.platformInitLocalPos = base.transform.localPosition;
		this.currT = this.startPercentage;
	}

	private void OnEnable()
	{
		if (this.platformType == MovingPlatform.PlatformType.Child)
		{
			return;
		}
		base.transform.localRotation = this.initLocalRotation;
		this.startPos = this.startXf.position;
		this.endPos = this.endXf.position;
		this.startRot = this.startXf.rotation;
		this.endRot = this.endXf.rotation;
		this.platformInitLocalPos = base.transform.localPosition;
		this.currT = this.startPercentage;
	}

	private Vector3 UpdatePointToPoint()
	{
		return Vector3.Lerp(this.startPos, this.endPos, this.smoothedPercent);
	}

	private Vector3 UpdateArc()
	{
		float num = Mathf.Lerp(this.rotateStartAmt, this.rotateStartAmt + this.rotateAmt, this.smoothedPercent);
		Quaternion quaternion = this.initLocalRotation;
		Vector3 vector = Quaternion.AngleAxis(num, Vector3.forward) * this.initOffset;
		return this.pivot.transform.position + vector;
	}

	private Quaternion UpdateRotation()
	{
		return Quaternion.Slerp(this.startRot, this.endRot, this.smoothedPercent);
	}

	private Quaternion UpdateContinuousRotation()
	{
		return Quaternion.AngleAxis(this.smoothedPercent * 360f, Vector3.up) * base.transform.parent.rotation;
	}

	private void SetupContext()
	{
		double time = PhotonNetwork.Time;
		if (this.lastServerTime == time)
		{
			this.dtSinceServerUpdate += Time.fixedDeltaTime;
		}
		else
		{
			this.dtSinceServerUpdate = 0f;
			this.lastServerTime = time;
		}
		float num = this.currT;
		this.currT = this.CycleCompletionPercent();
		this.currForward = this.CycleForward();
		this.percent = this.currT;
		if (this.reverseDirOnCycle)
		{
			this.percent = (this.currForward ? this.currT : (1f - this.currT));
		}
		if (this.reverseDir)
		{
			this.percent = 1f - this.percent;
		}
		this.smoothedPercent = this.percent;
		this.lastNT = time;
		this.lastT = Time.time;
	}

	private void Update()
	{
		if (this.platformType == MovingPlatform.PlatformType.Child)
		{
			return;
		}
		this.SetupContext();
		Vector3 vector = base.transform.position;
		Quaternion quaternion = base.transform.rotation;
		bool flag = false;
		switch (this.platformType)
		{
		case MovingPlatform.PlatformType.PointToPoint:
			vector = this.UpdatePointToPoint();
			break;
		case MovingPlatform.PlatformType.Arc:
			vector = this.UpdateArc();
			flag = true;
			break;
		case MovingPlatform.PlatformType.Rotation:
			quaternion = this.UpdateRotation();
			flag = true;
			break;
		case MovingPlatform.PlatformType.ContinuousRotation:
			quaternion = this.UpdateContinuousRotation();
			flag = true;
			break;
		}
		if (!this.debugMovement)
		{
			this.lastPos = this.rb.position;
			this.lastRot = this.rb.rotation;
			if (this.platformType != MovingPlatform.PlatformType.Rotation)
			{
				this.rb.MovePosition(vector);
			}
			if (flag)
			{
				this.rb.MoveRotation(quaternion);
			}
		}
		else
		{
			this.lastPos = base.transform.position;
			this.lastRot = base.transform.rotation;
			base.transform.position = vector;
			if (flag)
			{
				base.transform.rotation = quaternion;
			}
		}
		this.deltaPosition = vector - this.lastPos;
	}

	public Vector3 ThisFrameMovement()
	{
		return this.deltaPosition;
	}

	public MovingPlatform.PlatformType platformType;

	public float cycleLength;

	public float smoothingHalflife = 0.1f;

	public float rotateStartAmt;

	public float rotateAmt;

	public bool reverseDirOnCycle = true;

	public bool reverseDir;

	private CriticalSpringDamper springCD = new CriticalSpringDamper();

	private Rigidbody rb;

	public Transform startXf;

	public Transform endXf;

	public Vector3 platformInitLocalPos;

	private Vector3 startPos;

	private Vector3 endPos;

	private Quaternion startRot;

	private Quaternion endRot;

	public float startPercentage;

	public float startDelay;

	public bool startNextCycle;

	public Transform pivot;

	private Quaternion initLocalRotation;

	private Vector3 initOffset;

	private float currT;

	private float percent;

	private float smoothedPercent = -1f;

	private bool currForward;

	private float dtSinceServerUpdate;

	private double lastServerTime;

	public Vector3 currentVelocity;

	public Vector3 rotationalAxis;

	public float angularVelocity;

	public Vector3 rotationPivot;

	public Vector3 lastPos;

	public Quaternion lastRot;

	public Vector3 deltaPosition;

	public bool debugMovement;

	private double lastNT;

	private float lastT;

	public enum PlatformType
	{
		PointToPoint,
		Arc,
		Rotation,
		Child,
		ContinuousRotation
	}
}
