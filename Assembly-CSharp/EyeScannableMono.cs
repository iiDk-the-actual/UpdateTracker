using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class EyeScannableMono : MonoBehaviour, IEyeScannable
{
	public event Action OnDataChange;

	int IEyeScannable.scannableId
	{
		get
		{
			return base.GetInstanceID();
		}
	}

	Vector3 IEyeScannable.Position
	{
		get
		{
			return base.transform.position - this._initialPosition + this._bounds.center;
		}
	}

	Bounds IEyeScannable.Bounds
	{
		get
		{
			return this._bounds;
		}
	}

	IList<KeyValueStringPair> IEyeScannable.Entries
	{
		get
		{
			return this.data.Entries;
		}
	}

	private void Awake()
	{
		this.RecalculateBounds();
	}

	public void OnEnable()
	{
		this.RecalculateBoundsLater();
		EyeScannerMono.Register(this);
	}

	public void OnDisable()
	{
		EyeScannerMono.Unregister(this);
	}

	private async void RecalculateBoundsLater()
	{
		await Task.Delay(100);
		this.RecalculateBounds();
	}

	private void RecalculateBounds()
	{
		this._initialPosition = base.transform.position;
		Collider[] componentsInChildren = base.GetComponentsInChildren<Collider>();
		this._bounds = default(Bounds);
		if (componentsInChildren.Length == 0)
		{
			this._bounds.center = base.transform.position;
			this._bounds.Expand(1f);
			return;
		}
		this._bounds = componentsInChildren[0].bounds;
		for (int i = 1; i < componentsInChildren.Length; i++)
		{
			this._bounds.Encapsulate(componentsInChildren[i].bounds);
		}
	}

	[SerializeField]
	private KeyValuePairSet data;

	private Bounds _bounds;

	private Vector3 _initialPosition;
}
