using System;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleSystemEventShortcut : MonoBehaviour
{
	private void InitIfNeeded()
	{
		if (!this.initialized)
		{
			this.initialized = true;
			this.ps = base.GetComponent<ParticleSystem>();
			this.shape = this.ps.shape;
			this.poolExists = ObjectPools.instance.DoesPoolExist(base.gameObject);
		}
	}

	public void StopAndClear()
	{
		this.InitIfNeeded();
		this.ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
	}

	public void ClearAndPlay()
	{
		this.InitIfNeeded();
		this.ps.Clear();
		this.ps.Play();
	}

	public void PlayFromMesh(MeshRenderer mesh)
	{
		this.InitIfNeeded();
		this.shape.shapeType = ParticleSystemShapeType.MeshRenderer;
		this.shape.meshRenderer = mesh;
		this.ps.Play();
	}

	public void PlayFromSkin(SkinnedMeshRenderer skin)
	{
		this.InitIfNeeded();
		this.shape.shapeType = ParticleSystemShapeType.SkinnedMeshRenderer;
		this.shape.skinnedMeshRenderer = skin;
		this.ps.Play();
	}

	public void ReturnToPool()
	{
		this.InitIfNeeded();
		if (this.poolExists)
		{
			ObjectPools.instance.Destroy(base.gameObject);
		}
	}

	private void OnParticleSystemStopped()
	{
		this.ReturnToPool();
	}

	private bool initialized;

	private ParticleSystem ps;

	private ParticleSystem.ShapeModule shape;

	private bool poolExists;
}
