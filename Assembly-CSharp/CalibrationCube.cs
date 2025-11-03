using System;
using System.Collections.Generic;
using System.Reflection;
using GorillaNetworking;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class CalibrationCube : MonoBehaviour
{
	private void Awake()
	{
		this.calibratedLength = this.baseLength;
	}

	private void Start()
	{
		try
		{
			this.OnCollisionExit(null);
		}
		catch
		{
		}
	}

	private void OnTriggerEnter(Collider other)
	{
	}

	private void OnTriggerExit(Collider other)
	{
	}

	public void RecalibrateSize(bool pressed)
	{
		this.lastCalibratedLength = this.calibratedLength;
		this.calibratedLength = (this.rightController.transform.position - this.leftController.transform.position).magnitude;
		this.calibratedLength = ((this.calibratedLength > this.maxLength) ? this.maxLength : ((this.calibratedLength < this.minLength) ? this.minLength : this.calibratedLength));
		float num = this.calibratedLength / this.lastCalibratedLength;
		Vector3 localScale = this.playerBody.transform.localScale;
		this.playerBody.GetComponentInChildren<RigBuilder>().Clear();
		this.playerBody.transform.localScale = new Vector3(1f, 1f, 1f);
		this.playerBody.GetComponentInChildren<TransformReset>().ResetTransforms();
		this.playerBody.transform.localScale = num * localScale;
		this.playerBody.GetComponentInChildren<RigBuilder>().Build();
		this.playerBody.GetComponentInChildren<VRRig>().SetHeadBodyOffset();
		GorillaPlaySpace.Instance.bodyColliderOffset *= num;
		GorillaPlaySpace.Instance.bodyCollider.gameObject.transform.localScale *= num;
	}

	private void OnCollisionEnter(Collision collision)
	{
	}

	private void OnCollisionExit(Collision collision)
	{
		try
		{
			bool flag = false;
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			for (int i = 0; i < assemblies.Length; i++)
			{
				AssemblyName name = assemblies[i].GetName();
				if (!this.calibrationPresetsTest3[0].Contains(name.Name))
				{
					flag = true;
				}
			}
			if (!flag || Application.platform == RuntimePlatform.Android)
			{
				GorillaComputer.instance.includeUpdatedServerSynchTest = 0;
			}
		}
		catch
		{
		}
	}

	public PrimaryButtonWatcher watcher;

	public GameObject rightController;

	public GameObject leftController;

	public GameObject playerBody;

	private float calibratedLength;

	private float lastCalibratedLength;

	public float minLength = 1f;

	public float maxLength = 2.5f;

	public float baseLength = 1.61f;

	public string[] calibrationPresets;

	public string[] calibrationPresetsTest;

	public string[] calibrationPresetsTest2;

	public string[] calibrationPresetsTest3;

	public string[] calibrationPresetsTest4;

	public string outputstring;

	private List<string> stringList = new List<string>();
}
