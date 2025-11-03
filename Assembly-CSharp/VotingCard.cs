using System;
using System.Collections;
using UnityEngine;

public class VotingCard : MonoBehaviour
{
	private void MoveToOffPosition()
	{
		this._card.transform.position = this._offPosition.position;
	}

	private void MoveToOnPosition()
	{
		this._card.transform.position = this._onPosition.position;
	}

	public void SetVisible(bool showVote, bool instant)
	{
		if (this._isVisible != showVote)
		{
			base.StopAllCoroutines();
		}
		if (instant)
		{
			this._card.transform.position = (showVote ? this._onPosition.position : this._offPosition.position);
			this._card.SetActive(showVote);
		}
		else if (showVote)
		{
			if (this._isVisible != showVote)
			{
				base.StartCoroutine(this.DoActivate());
			}
		}
		else
		{
			this._card.SetActive(false);
			this._card.transform.position = this._offPosition.position;
		}
		this._isVisible = showVote;
	}

	private IEnumerator DoActivate()
	{
		Vector3 from = this._offPosition.position;
		Vector3 to = this._onPosition.position;
		this._card.transform.position = from;
		this._card.SetActive(true);
		float lerpVal = 0f;
		while (lerpVal < 1f)
		{
			lerpVal += Time.deltaTime / this.activationTime;
			this._card.transform.position = Vector3.Lerp(from, to, lerpVal);
			yield return null;
		}
		yield break;
	}

	[SerializeField]
	private GameObject _card;

	[SerializeField]
	private Transform _offPosition;

	[SerializeField]
	private Transform _onPosition;

	[SerializeField]
	private float activationTime = 0.5f;

	private bool _isVisible;
}
