using System;
using UnityEngine;

public class GRScannable : MonoBehaviour
{
	public virtual void Start()
	{
		if (this.gameEntity == null)
		{
			this.gameEntity = base.GetComponent<GameEntity>();
		}
	}

	public virtual string GetTitleText(GhostReactor reactor)
	{
		return this.titleText;
	}

	public virtual string GetBodyText(GhostReactor reactor)
	{
		return this.bodyText;
	}

	public virtual string GetAnnotationText(GhostReactor reactor)
	{
		return this.annotationText;
	}

	public GameEntity gameEntity;

	[SerializeField]
	protected string titleText;

	[SerializeField]
	protected string bodyText;

	[SerializeField]
	protected string annotationText;
}
