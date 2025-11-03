using System;
using System.Collections;
using UnityEngine;

public class DeployedChild : MonoBehaviour
{
	public void Deploy(DeployableObject parent, Vector3 launchPos, Quaternion launchRot, Vector3 releaseVel, bool isRemote = false)
	{
		this._parent = parent;
		this._parent.DeployChild();
		Transform transform = base.transform;
		transform.position = launchPos;
		transform.rotation = launchRot;
		transform.localScale = this._parent.transform.lossyScale;
		this._rigidbody.linearVelocity = releaseVel;
		this._isRemote = isRemote;
	}

	public void ReturnToParent(float delay)
	{
		if (delay > 0f)
		{
			base.StartCoroutine(this.ReturnToParentDelayed(delay));
			return;
		}
		if (this._parent != null)
		{
			this._parent.ReturnChild();
		}
	}

	private IEnumerator ReturnToParentDelayed(float delay)
	{
		float start = Time.time;
		while (Time.time < start + delay)
		{
			yield return null;
		}
		if (this._parent != null)
		{
			this._parent.ReturnChild();
		}
		yield break;
	}

	[SerializeField]
	private Rigidbody _rigidbody;

	[SerializeReference]
	private DeployableObject _parent;

	private bool _isRemote;
}
