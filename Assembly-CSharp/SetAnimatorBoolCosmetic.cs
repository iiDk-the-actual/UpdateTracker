using System;
using UnityEngine;

public class SetAnimatorBoolCosmetic : MonoBehaviour
{
	private void OnAnimatorValueChanged()
	{
	}

	public void SetAnimatorBool(bool value)
	{
		if (this.bool1Hash == 0)
		{
			this.bool1Hash = Animator.StringToHash(this.boolParameterName);
		}
		this.animator.SetBool(this.bool1Hash, value);
	}

	public void SetAnimatorBool2(bool value)
	{
		if (this.bool2Hash == 0)
		{
			this.bool2Hash = Animator.StringToHash(this.bool2ParameterName);
		}
		this.animator.SetBool(this.bool2Hash, value);
	}

	public void SetAnimatorBool3(bool value)
	{
		if (this.bool3Hash == 0)
		{
			this.bool3Hash = Animator.StringToHash(this.bool3ParameterName);
		}
		this.animator.SetBool(this.bool3Hash, value);
	}

	public void SetAnimatorBool4(bool value)
	{
		if (this.bool4Hash == 0)
		{
			this.bool4Hash = Animator.StringToHash(this.bool4ParameterName);
		}
		this.animator.SetBool(this.bool4Hash, value);
	}

	public void SetAnimatorBool5(bool value)
	{
		if (this.bool5Hash == 0)
		{
			this.bool5Hash = Animator.StringToHash(this.bool5ParameterName);
		}
		this.animator.SetBool(this.bool5Hash, value);
	}

	public void SetAnimatorInteger1(int value)
	{
		if (this.int1Hash == 0)
		{
			this.int1Hash = Animator.StringToHash(this.int1ParameterName);
		}
		this.animator.SetInteger(this.int1Hash, value);
	}

	public void SetAnimatorInteger2(int value)
	{
		if (this.int2Hash == 0)
		{
			this.int2Hash = Animator.StringToHash(this.int2ParameterName);
		}
		this.animator.SetInteger(this.int2Hash, value);
	}

	public void SetAnimatorInteger3(int value)
	{
		if (this.int3Hash == 0)
		{
			this.int3Hash = Animator.StringToHash(this.int3ParameterName);
		}
		this.animator.SetInteger(this.int3Hash, value);
	}

	public void SetAnimatorInteger4(int value)
	{
		if (this.int4Hash == 0)
		{
			this.int4Hash = Animator.StringToHash(this.int4ParameterName);
		}
		this.animator.SetInteger(this.int4Hash, value);
	}

	public void SetAnimatorFloat1(float value)
	{
		if (this.float1Hash == 0)
		{
			this.float1Hash = Animator.StringToHash(this.float1ParameterName);
		}
		this.animator.SetFloat(this.float1Hash, value);
	}

	public void SetAnimatorFloat2(float value)
	{
		if (this.float2Hash == 0)
		{
			this.float2Hash = Animator.StringToHash(this.float2ParameterName);
		}
		this.animator.SetFloat(this.float2Hash, value);
	}

	public void SetAnimatorFloat3(float value)
	{
		if (this.float3Hash == 0)
		{
			this.float3Hash = Animator.StringToHash(this.float3ParameterName);
		}
		this.animator.SetFloat(this.float3Hash, value);
	}

	public void SetAnimatorFloat4(float value)
	{
		if (this.float4Hash == 0)
		{
			this.float4Hash = Animator.StringToHash(this.float4ParameterName);
		}
		this.animator.SetFloat(this.float4Hash, value);
	}

	public void SetAnimatorTrigger(string triggerName)
	{
		this.animator.SetTrigger(triggerName);
	}

	private void Reset()
	{
		this.animator = base.GetComponent<Animator>();
	}

	[SerializeField]
	private Animator animator;

	[SerializeField]
	private string boolParameterName;

	[SerializeField]
	private string bool2ParameterName;

	[SerializeField]
	private string bool3ParameterName;

	[SerializeField]
	private string bool4ParameterName;

	[SerializeField]
	private string bool5ParameterName;

	[SerializeField]
	private string int1ParameterName;

	[SerializeField]
	private string int2ParameterName;

	[SerializeField]
	private string int3ParameterName;

	[SerializeField]
	private string int4ParameterName;

	[SerializeField]
	private string float1ParameterName;

	[SerializeField]
	private string float2ParameterName;

	[SerializeField]
	private string float3ParameterName;

	[SerializeField]
	private string float4ParameterName;

	private int bool1Hash;

	private int bool2Hash;

	private int bool3Hash;

	private int bool4Hash;

	private int bool5Hash;

	private const int MAX_BOOLS = 5;

	private int int1Hash;

	private int int2Hash;

	private int int3Hash;

	private int int4Hash;

	private const int MAX_INTS = 4;

	private int float1Hash;

	private int float2Hash;

	private int float3Hash;

	private int float4Hash;

	private const int MAX_FLOATS = 4;
}
