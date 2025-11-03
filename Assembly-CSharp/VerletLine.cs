using System;
using System.Collections;
using GorillaExtensions;
using UnityEngine;
using UnityEngine.Serialization;

[DisallowMultipleComponent]
public class VerletLine : MonoBehaviour
{
	private void Awake()
	{
		this._nodes = new VerletLine.LineNode[this.segmentNumber];
		this._positions = new Vector3[this.segmentNumber];
		for (int i = 0; i < this.segmentNumber; i++)
		{
			float num = (float)i / (float)(this.segmentNumber - 1);
			Vector3 vector = Vector3.Lerp(this.lineStart.position, this.lineEnd.position, num);
			this._nodes[i] = new VerletLine.LineNode
			{
				position = vector,
				lastPosition = vector,
				acceleration = this.gravity
			};
		}
		this.line.positionCount = this._nodes.Length;
		this.endRigidbody = this.lineEnd.GetComponent<Rigidbody>();
		if (this.endRigidbody)
		{
			this.endRigidbody.maxLinearVelocity = this.endMaxSpeed;
			this.endRigidbodyParent = this.endRigidbody.transform.parent;
			this.rigidBodyStartingLocalPosition = this.endRigidbody.transform.localPosition;
			this.endRigidbody.transform.parent = null;
			this.endRigidbody.gameObject.SetActive(false);
		}
		this.totalLineLength = this.segmentLength * (float)this.segmentNumber;
	}

	private void OnEnable()
	{
		if (this.endRigidbody)
		{
			this.endRigidbody.gameObject.SetActive(true);
			this.endRigidbody.transform.localPosition = this.endRigidbodyParent.TransformPoint(this.rigidBodyStartingLocalPosition);
		}
	}

	private void OnDisable()
	{
		if (this.endRigidbody)
		{
			this.endRigidbody.gameObject.SetActive(false);
		}
	}

	public void SetLength(float total, float delay = 0f)
	{
		this.segmentTargetLength = total / (float)this.segmentNumber;
		if (this.segmentTargetLength < this.segmentMinLength)
		{
			this.segmentTargetLength = this.segmentMinLength;
		}
		if (this.segmentTargetLength > this.segmentMaxLength)
		{
			this.segmentTargetLength = this.segmentMaxLength;
		}
		if (delay >= 0.01f)
		{
			base.StartCoroutine(this.ResizeAfterDelay(delay));
		}
	}

	public void AddSegmentLength(float amount, float delay = 0f)
	{
		this.segmentTargetLength = this.segmentLength + amount;
		if (this.segmentTargetLength <= 0f)
		{
			return;
		}
		if (this.segmentTargetLength > this.segmentMaxLength)
		{
			this.segmentTargetLength = this.segmentMaxLength;
		}
		if (delay >= 0.01f)
		{
			base.StartCoroutine(this.ResizeAfterDelay(delay));
		}
	}

	public void RemoveSegmentLength(float amount, float delay = 0f)
	{
		this.segmentTargetLength = this.segmentLength - amount;
		if (this.segmentTargetLength <= this.segmentMinLength)
		{
			this.segmentTargetLength = (this.segmentLength = this.segmentMinLength);
			return;
		}
		if (delay >= 0.01f)
		{
			base.StartCoroutine(this.ResizeAfterDelay(delay));
		}
	}

	private IEnumerator ResizeAfterDelay(float delay)
	{
		yield return new WaitForSeconds(delay);
		yield break;
	}

	private void Update()
	{
		if (this.segmentLength.Approx(this.segmentTargetLength, 0.1f))
		{
			this.segmentLength = this.segmentTargetLength;
			return;
		}
		this.segmentLength = Mathf.Lerp(this.segmentLength, this.segmentTargetLength, this.resizeSpeed * this.resizeScale * Time.deltaTime);
		if (this.scaleLineWidth)
		{
			this.line.widthMultiplier = base.transform.lossyScale.x;
		}
	}

	public void ForceTotalLength(float totalLength)
	{
		float num = totalLength / (float)((this.segmentNumber < 1) ? 1 : this.segmentNumber);
		this.segmentLength = (this.segmentTargetLength = num);
		this.totalLineLength = this.segmentLength * (float)this.segmentNumber;
	}

	private void FixedUpdate()
	{
		for (int i = 0; i < this._nodes.Length; i++)
		{
			VerletLine.Simulate(ref this._nodes[i], Time.fixedDeltaTime);
		}
		for (int j = 0; j < this.simIterations; j++)
		{
			for (int k = 0; k < this._nodes.Length - 1; k++)
			{
				VerletLine.LimitDistance(ref this._nodes[k], ref this._nodes[k + 1], this.segmentLength);
			}
		}
		this._nodes[0].position = this.lineStart.position;
		if (this.endRigidbody)
		{
			if (this.onlyPullAtEdges)
			{
				if ((this.endRigidbody.transform.position - this.lineStart.position).IsLongerThan(this.totalLineLength))
				{
					Vector3 vector = this.lineStart.position + (this.endRigidbody.transform.position - this.lineStart.position).normalized * this.totalLineLength;
					this.endRigidbody.linearVelocity += (vector - this.endRigidbody.transform.position) / Time.fixedDeltaTime;
					if (this.endRigidbody.linearVelocity.IsLongerThan(this.endMaxSpeed))
					{
						this.endRigidbody.linearVelocity = this.endRigidbody.linearVelocity.normalized * this.endMaxSpeed;
					}
				}
			}
			else
			{
				VerletLine.LineNode[] nodes = this._nodes;
				Vector3 vector2 = (nodes[nodes.Length - 1].position - this.lineEnd.position) * (this.tension * this.tensionScale);
				Quaternion rotation = this.endRigidbody.rotation;
				VerletLine.LineNode[] nodes2 = this._nodes;
				Vector3 position = nodes2[nodes2.Length - 1].position;
				VerletLine.LineNode[] nodes3 = this._nodes;
				Quaternion.LookRotation(position - nodes3[nodes3.Length - 2].position);
				if (!this.endRigidbody.isKinematic)
				{
					this.endRigidbody.AddForceAtPosition(vector2, this.endRigidbody.transform.TransformPoint(this.endLineAnchorLocalPosition));
				}
			}
		}
		VerletLine.LineNode[] nodes4 = this._nodes;
		nodes4[nodes4.Length - 1].position = this.lineEnd.position;
		for (int l = 0; l < this._nodes.Length; l++)
		{
			this._positions[l] = this._nodes[l].position;
		}
		this.line.SetPositions(this._positions);
	}

	private static void Simulate(ref VerletLine.LineNode p, float dt)
	{
		Vector3 position = p.position;
		p.position += p.position - p.lastPosition + p.acceleration * (dt * dt);
		p.lastPosition = position;
	}

	private static void LimitDistance(ref VerletLine.LineNode p1, ref VerletLine.LineNode p2, float restLength)
	{
		Vector3 vector = p2.position - p1.position;
		float num = vector.magnitude + 1E-05f;
		float num2 = (num - restLength) / num;
		p1.position += vector * (num2 * 0.5f);
		p2.position -= vector * (num2 * 0.5f);
	}

	public Transform lineStart;

	public Transform lineEnd;

	[Space]
	public LineRenderer line;

	public Rigidbody endRigidbody;

	public Transform endRigidbodyParent;

	public Vector3 endLineAnchorLocalPosition;

	private Vector3 rigidBodyStartingLocalPosition;

	[Space]
	public int segmentNumber = 10;

	public float segmentLength = 0.03f;

	public float segmentTargetLength = 0.03f;

	public float segmentMaxLength = 0.03f;

	public float segmentMinLength = 0.03f;

	[Space]
	public Vector3 gravity = new Vector3(0f, -9.81f, 0f);

	public int simIterations = 6;

	public float tension = 10f;

	public float tensionScale = 1f;

	public float endMaxSpeed = 48f;

	[FormerlySerializedAs("lerpSpeed")]
	[Space]
	public float resizeSpeed = 1f;

	public float resizeScale = 1f;

	[NonSerialized]
	private VerletLine.LineNode[] _nodes = new VerletLine.LineNode[0];

	[NonSerialized]
	private Vector3[] _positions = new Vector3[0];

	private float totalLineLength;

	[SerializeField]
	private bool onlyPullAtEdges;

	[SerializeField]
	private bool scaleLineWidth = true;

	[Serializable]
	public struct LineNode
	{
		public Vector3 position;

		public Vector3 lastPosition;

		public Vector3 acceleration;
	}
}
