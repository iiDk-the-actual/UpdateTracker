using System;
using UnityEngine;

public class MetroBlimp : MonoBehaviour
{
	private void Awake()
	{
		this._startLocalHeight = base.transform.localPosition.y;
	}

	public void Tick()
	{
		bool flag = Mathf.Sin(Time.time * 2f) * 0.5f + 0.5f < 0.0001f;
		int num = Mathf.CeilToInt(this._numHandsOnBlimp / 2f);
		if (this._numHandsOnBlimp == 0f)
		{
			this._topStayTime = 0f;
			if (flag)
			{
				this.blimpRenderer.material.DisableKeyword("_INNER_GLOW");
			}
		}
		else
		{
			this._topStayTime += Time.deltaTime;
			if (flag)
			{
				this.blimpRenderer.material.EnableKeyword("_INNER_GLOW");
			}
		}
		Vector3 localPosition = base.transform.localPosition;
		Vector3 vector = localPosition;
		float y = vector.y;
		float num2 = this._startLocalHeight + this.descendOffset;
		float deltaTime = Time.deltaTime;
		if (num > 0)
		{
			if (y > num2)
			{
				vector += Vector3.down * (this.descendSpeed * (float)num * deltaTime);
			}
		}
		else if (y < this._startLocalHeight)
		{
			vector += Vector3.up * (this.ascendSpeed * deltaTime);
		}
		base.transform.localPosition = Vector3.Slerp(localPosition, vector, 0.5f);
	}

	private static bool IsPlayerHand(Collider c)
	{
		return c.gameObject.IsOnLayer(UnityLayer.GorillaHand);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (MetroBlimp.IsPlayerHand(other))
		{
			this._numHandsOnBlimp += 1f;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (MetroBlimp.IsPlayerHand(other))
		{
			this._numHandsOnBlimp -= 1f;
		}
	}

	public MetroSpotlight spotLightLeft;

	public MetroSpotlight spotLightRight;

	[Space]
	public BoxCollider topCollider;

	public Material blimpMaterial;

	public Renderer blimpRenderer;

	[Space]
	public float ascendSpeed = 1f;

	public float descendSpeed = 0.5f;

	public float descendOffset = -24.1f;

	public float descendReactionTime = 3f;

	[Space]
	[NonSerialized]
	private float _startLocalHeight;

	[NonSerialized]
	private float _topStayTime;

	[NonSerialized]
	private float _numHandsOnBlimp;

	[NonSerialized]
	private bool _lowering;

	private const string _INNER_GLOW = "_INNER_GLOW";
}
