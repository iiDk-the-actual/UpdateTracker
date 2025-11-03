using System;
using UnityEngine;

public class SizeChangerTrigger : MonoBehaviour, IBuilderPieceComponent
{
	public event SizeChangerTrigger.SizeChangerTriggerEvent OnEnter;

	public event SizeChangerTrigger.SizeChangerTriggerEvent OnExit;

	private void Awake()
	{
		this.myCollider = base.GetComponent<Collider>();
	}

	public void OnTriggerEnter(Collider other)
	{
		if (this.OnEnter != null)
		{
			this.OnEnter(other);
		}
	}

	public void OnTriggerExit(Collider other)
	{
		if (this.OnExit != null)
		{
			this.OnExit(other);
		}
	}

	public Vector3 ClosestPoint(Vector3 position)
	{
		return this.myCollider.ClosestPoint(position);
	}

	public void OnPieceCreate(int pieceType, int pieceId)
	{
	}

	public void OnPieceDestroy()
	{
	}

	public void OnPiecePlacementDeserialized()
	{
	}

	public void OnPieceActivate()
	{
		Debug.LogError("Size Trigger Pieces no longer work, need reimplementation");
	}

	public void OnPieceDeactivate()
	{
		Debug.LogError("Size Trigger Pieces no longer work, need reimplementation");
	}

	private Collider myCollider;

	public bool builderEnterTrigger;

	public bool builderExitOnEnterTrigger;

	public delegate void SizeChangerTriggerEvent(Collider other);
}
