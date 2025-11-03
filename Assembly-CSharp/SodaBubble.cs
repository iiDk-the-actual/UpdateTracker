using System;
using System.Collections;
using UnityEngine;

public class SodaBubble : MonoBehaviour
{
	public void Pop()
	{
		base.StartCoroutine(this.PopCoroutine());
	}

	private IEnumerator PopCoroutine()
	{
		this.audioSource.GTPlay();
		this.bubbleMesh.gameObject.SetActive(false);
		this.bubbleCollider.gameObject.SetActive(false);
		yield return new WaitForSeconds(1f);
		this.bubbleMesh.gameObject.SetActive(true);
		this.bubbleCollider.gameObject.SetActive(true);
		ObjectPools.instance.Destroy(base.gameObject);
		yield break;
	}

	public MeshRenderer bubbleMesh;

	public Rigidbody body;

	public MeshCollider bubbleCollider;

	public AudioSource audioSource;
}
