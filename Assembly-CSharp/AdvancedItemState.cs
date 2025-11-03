using System;
using System.Runtime.CompilerServices;
using UnityEngine;

[Serializable]
public class AdvancedItemState
{
	public void Encode()
	{
		this._encodedValue = this.EncodeData();
	}

	public void Decode()
	{
		AdvancedItemState advancedItemState = this.DecodeData(this._encodedValue);
		this.index = advancedItemState.index;
		this.preData = advancedItemState.preData;
		this.limitAxis = advancedItemState.limitAxis;
		this.reverseGrip = advancedItemState.reverseGrip;
		this.angle = advancedItemState.angle;
	}

	public Quaternion GetQuaternion()
	{
		Vector3 one = Vector3.one;
		if (this.reverseGrip)
		{
			switch (this.limitAxis)
			{
			case LimitAxis.NoMovement:
				return Quaternion.identity;
			case LimitAxis.YAxis:
				return Quaternion.identity;
			case LimitAxis.XAxis:
			case LimitAxis.ZAxis:
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
		return Quaternion.identity;
	}

	[return: TupleElementNames(new string[] { "grabPointIndex", "YRotation", "XRotation", "ZRotation" })]
	public ValueTuple<int, float, float, float> DecodeAdvancedItemState(int encodedValue)
	{
		int num = (encodedValue >> 21) & 255;
		float num2 = (float)((encodedValue >> 14) & 127) / 128f * 360f;
		float num3 = (float)((encodedValue >> 7) & 127) / 128f * 360f;
		float num4 = (float)(encodedValue & 127) / 128f * 360f;
		return new ValueTuple<int, float, float, float>(num, num2, num3, num4);
	}

	private float EncodedDeltaRotation
	{
		get
		{
			return this.GetEncodedDeltaRotation();
		}
	}

	public float GetEncodedDeltaRotation()
	{
		return Mathf.Abs(Mathf.Atan2(this.angleVectorWhereUpIsStandard.x, this.angleVectorWhereUpIsStandard.y)) / 3.1415927f;
	}

	public void DecodeDeltaRotation(float encodedDelta, bool isFlipped)
	{
		float num = encodedDelta * 3.1415927f;
		if (isFlipped)
		{
			this.angleVectorWhereUpIsStandard = new Vector2(-Mathf.Sin(num), Mathf.Cos(num));
		}
		else
		{
			this.angleVectorWhereUpIsStandard = new Vector2(Mathf.Sin(num), Mathf.Cos(num));
		}
		switch (this.limitAxis)
		{
		case LimitAxis.NoMovement:
		case LimitAxis.XAxis:
		case LimitAxis.ZAxis:
			return;
		case LimitAxis.YAxis:
		{
			Vector3 vector = new Vector3(this.angleVectorWhereUpIsStandard.x, 0f, this.angleVectorWhereUpIsStandard.y);
			Vector3 vector2 = (this.reverseGrip ? Vector3.down : Vector3.up);
			this.deltaRotation = Quaternion.LookRotation(vector, vector2);
			return;
		}
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public int EncodeData()
	{
		int num = 0;
		if ((this.index >= 32) | (this.index < 0))
		{
			throw new ArgumentOutOfRangeException(string.Format("Index is invalid {0}", this.index));
		}
		num |= this.index << 25;
		AdvancedItemState.PointType pointType = this.preData.pointType;
		num |= (int)((int)(pointType & (AdvancedItemState.PointType)7) << 22);
		num |= (int)((int)this.limitAxis << 19);
		num |= (this.reverseGrip ? 1 : 0) << 18;
		bool flag = this.angleVectorWhereUpIsStandard.x < 0f;
		if (pointType != AdvancedItemState.PointType.Standard)
		{
			if (pointType != AdvancedItemState.PointType.DistanceBased)
			{
				throw new ArgumentOutOfRangeException();
			}
			int num2 = (int)(this.GetEncodedDeltaRotation() * 512f) & 511;
			num |= (flag ? 1 : 0) << 17;
			num |= num2 << 9;
			int num3 = (int)(this.preData.distAlongLine * 256f) & 255;
			num |= num3;
		}
		else
		{
			int num4 = (int)(this.GetEncodedDeltaRotation() * 65536f) & 65535;
			num |= (flag ? 1 : 0) << 17;
			num |= num4 << 1;
		}
		return num;
	}

	public AdvancedItemState DecodeData(int encoded)
	{
		AdvancedItemState advancedItemState = new AdvancedItemState();
		advancedItemState.index = (encoded >> 25) & 31;
		advancedItemState.limitAxis = (LimitAxis)((encoded >> 19) & 7);
		advancedItemState.reverseGrip = ((encoded >> 18) & 1) == 1;
		AdvancedItemState.PointType pointType = (AdvancedItemState.PointType)((encoded >> 22) & 7);
		if (pointType != AdvancedItemState.PointType.Standard)
		{
			if (pointType != AdvancedItemState.PointType.DistanceBased)
			{
				throw new ArgumentOutOfRangeException();
			}
			advancedItemState.preData = new AdvancedItemState.PreData
			{
				pointType = pointType,
				distAlongLine = (float)(encoded & 255) / 256f
			};
			this.DecodeDeltaRotation((float)((encoded >> 9) & 511) / 512f, ((encoded >> 17) & 1) > 0);
		}
		else
		{
			advancedItemState.preData = new AdvancedItemState.PreData
			{
				pointType = pointType
			};
			this.DecodeDeltaRotation((float)((encoded >> 1) & 65535) / 65536f, ((encoded >> 17) & 1) > 0);
		}
		return advancedItemState;
	}

	private int _encodedValue;

	public Vector2 angleVectorWhereUpIsStandard;

	public Quaternion deltaRotation;

	public int index;

	public AdvancedItemState.PreData preData;

	public LimitAxis limitAxis;

	public bool reverseGrip;

	public float angle;

	[Serializable]
	public class PreData
	{
		public float distAlongLine;

		public AdvancedItemState.PointType pointType;
	}

	public enum PointType
	{
		Standard,
		DistanceBased
	}
}
