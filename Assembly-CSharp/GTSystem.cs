using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

[DisallowMultipleComponent]
public abstract class GTSystem<T> : MonoBehaviour, IReadOnlyList<T>, IEnumerable<T>, IEnumerable, IReadOnlyCollection<T> where T : MonoBehaviour
{
	public PhotonView photonView
	{
		get
		{
			return this._photonView;
		}
	}

	protected virtual void Awake()
	{
		GTSystem<T>.SetSingleton(this);
	}

	protected virtual void Tick()
	{
		float deltaTime = Time.deltaTime;
		for (int i = 0; i < this._instances.Count; i++)
		{
			T t = this._instances[i];
			if (t)
			{
				this.OnTick(deltaTime, t);
			}
		}
	}

	protected virtual void OnApplicationQuit()
	{
		GTSystem<T>.gAppQuitting = true;
	}

	protected virtual void OnTick(float dt, T instance)
	{
	}

	private bool RegisterInstance(T instance)
	{
		if (instance == null)
		{
			GTDev.LogError<string>("[" + base.GetType().Name + "::Register] Instance is null.", null);
			return false;
		}
		if (this._instances.Contains(instance))
		{
			return false;
		}
		this._instances.Add(instance);
		this.OnRegister(instance);
		return true;
	}

	protected virtual void OnRegister(T instance)
	{
	}

	private bool UnregisterInstance(T instance)
	{
		if (instance == null)
		{
			GTDev.LogError<string>("[" + base.GetType().Name + "::Unregister] Instance is null.", null);
			return false;
		}
		if (!this._instances.Contains(instance))
		{
			return false;
		}
		this._instances.Remove(instance);
		this.OnUnregister(instance);
		return true;
	}

	protected virtual void OnUnregister(T instance)
	{
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return ((IEnumerable<T>)this._instances).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable<T>)this._instances).GetEnumerator();
	}

	int IReadOnlyCollection<T>.Count
	{
		get
		{
			return this._instances.Count;
		}
	}

	T IReadOnlyList<T>.this[int index]
	{
		get
		{
			return this._instances[index];
		}
	}

	public static PhotonView PhotonView
	{
		get
		{
			return GTSystem<T>.gSingleton._photonView;
		}
	}

	protected static void SetSingleton(GTSystem<T> system)
	{
		if (GTSystem<T>.gAppQuitting)
		{
			return;
		}
		if (GTSystem<T>.gSingleton != null && GTSystem<T>.gSingleton != system)
		{
			Object.Destroy(system);
			GTDev.LogWarning<string>("Singleton of type " + GTSystem<T>.gSingleton.GetType().Name + " already exists.", null);
			return;
		}
		GTSystem<T>.gSingleton = system;
		if (!GTSystem<T>.gInitializing)
		{
			return;
		}
		GTSystem<T>.gSingleton._instances.Clear();
		T[] array = GTSystem<T>.gQueueRegister.Where((T x) => x != null).ToArray<T>();
		GTSystem<T>.gSingleton._instances.AddRange(array);
		GTSystem<T>.gQueueRegister.Clear();
		PhotonView component = GTSystem<T>.gSingleton.GetComponent<PhotonView>();
		if (component != null)
		{
			GTSystem<T>.gSingleton._photonView = component;
			GTSystem<T>.gSingleton._networked = true;
		}
		GTSystem<T>.gInitializing = false;
	}

	public static void Register(T instance)
	{
		if (GTSystem<T>.gAppQuitting)
		{
			return;
		}
		if (instance == null)
		{
			return;
		}
		if (GTSystem<T>.gInitializing)
		{
			GTSystem<T>.gQueueRegister.Add(instance);
			return;
		}
		if (GTSystem<T>.gSingleton == null && !GTSystem<T>.gInitializing)
		{
			GTSystem<T>.gInitializing = true;
			GTSystem<T>.gQueueRegister.Add(instance);
			return;
		}
		GTSystem<T>.gSingleton.RegisterInstance(instance);
	}

	public static void Unregister(T instance)
	{
		if (GTSystem<T>.gAppQuitting)
		{
			return;
		}
		if (instance == null)
		{
			return;
		}
		if (GTSystem<T>.gInitializing)
		{
			GTSystem<T>.gQueueRegister.Remove(instance);
			return;
		}
		if (GTSystem<T>.gSingleton == null && !GTSystem<T>.gInitializing)
		{
			GTSystem<T>.gInitializing = true;
			GTSystem<T>.gQueueRegister.Remove(instance);
			return;
		}
		GTSystem<T>.gSingleton.UnregisterInstance(instance);
	}

	[SerializeField]
	protected List<T> _instances = new List<T>();

	[SerializeField]
	private bool _networked;

	[SerializeField]
	private PhotonView _photonView;

	private static GTSystem<T> gSingleton;

	private static bool gInitializing = false;

	private static bool gAppQuitting = false;

	private static HashSet<T> gQueueRegister = new HashSet<T>();
}
