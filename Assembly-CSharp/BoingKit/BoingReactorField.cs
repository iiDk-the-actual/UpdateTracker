using System;
using System.Collections.Generic;
using UnityEngine;

namespace BoingKit
{
	public class BoingReactorField : BoingBase
	{
		public static BoingReactorField.ShaderPropertyIdSet ShaderPropertyId
		{
			get
			{
				if (BoingReactorField.s_shaderPropertyId == null)
				{
					BoingReactorField.s_shaderPropertyId = new BoingReactorField.ShaderPropertyIdSet();
				}
				return BoingReactorField.s_shaderPropertyId;
			}
		}

		public bool UpdateShaderConstants(MaterialPropertyBlock props, float positionSampleMultiplier = 1f, float rotationSampleMultiplier = 1f)
		{
			if (this.HardwareMode != BoingReactorField.HardwareModeEnum.GPU)
			{
				return false;
			}
			if (this.m_fieldParamsBuffer == null || this.m_cellsBuffer == null)
			{
				return false;
			}
			props.SetFloat(BoingReactorField.ShaderPropertyId.PositionSampleMultiplier, positionSampleMultiplier);
			props.SetFloat(BoingReactorField.ShaderPropertyId.RotationSampleMultiplier, rotationSampleMultiplier);
			props.SetBuffer(BoingReactorField.ShaderPropertyId.RenderFieldParams, this.m_fieldParamsBuffer);
			props.SetBuffer(BoingReactorField.ShaderPropertyId.RenderCells, this.m_cellsBuffer);
			return true;
		}

		public bool UpdateShaderConstants(Material material, float positionSampleMultiplier = 1f, float rotationSampleMultiplier = 1f)
		{
			if (this.HardwareMode != BoingReactorField.HardwareModeEnum.GPU)
			{
				return false;
			}
			if (this.m_fieldParamsBuffer == null || this.m_cellsBuffer == null)
			{
				return false;
			}
			material.SetFloat(BoingReactorField.ShaderPropertyId.PositionSampleMultiplier, positionSampleMultiplier);
			material.SetFloat(BoingReactorField.ShaderPropertyId.RotationSampleMultiplier, rotationSampleMultiplier);
			material.SetBuffer(BoingReactorField.ShaderPropertyId.RenderFieldParams, this.m_fieldParamsBuffer);
			material.SetBuffer(BoingReactorField.ShaderPropertyId.RenderCells, this.m_cellsBuffer);
			return true;
		}

		public int GpuResourceSetId
		{
			get
			{
				return this.m_gpuResourceSetId;
			}
		}

		public BoingReactorField()
		{
			this.Params.Init();
			this.m_bounds = Aabb.Empty;
			this.m_init = false;
		}

		public void Reboot()
		{
			this.m_gridCenter = base.transform.position;
			Vector3 vector = this.QuantizeNorm(this.m_gridCenter);
			this.m_qPrevGridCenterNorm = vector;
			BoingReactorField.CellMoveModeEnum cellMoveMode = this.CellMoveMode;
			if (cellMoveMode == BoingReactorField.CellMoveModeEnum.Follow)
			{
				this.m_gridCenter = base.transform.position;
				this.m_iCellBaseX = 0;
				this.m_iCellBaseY = 0;
				this.m_iCellBaseZ = 0;
				this.m_iCellBaseZ = 0;
				this.m_iCellBaseZ = 0;
				return;
			}
			if (cellMoveMode != BoingReactorField.CellMoveModeEnum.WrapAround)
			{
				return;
			}
			this.m_gridCenter = vector * this.CellSize;
			this.m_iCellBaseX = MathUtil.Modulo((int)this.m_qPrevGridCenterNorm.x, this.CellsX);
			this.m_iCellBaseY = MathUtil.Modulo((int)this.m_qPrevGridCenterNorm.y, this.CellsY);
			this.m_iCellBaseZ = MathUtil.Modulo((int)this.m_qPrevGridCenterNorm.z, this.CellsZ);
		}

		public void OnEnable()
		{
			this.Reboot();
			BoingManager.Register(this);
		}

		public void Start()
		{
			this.Reboot();
			this.m_cellMoveMode = this.CellMoveMode;
		}

		public void OnDisable()
		{
			BoingManager.Unregister(this);
			this.DisposeCpuResources();
			this.DisposeGpuResources();
		}

		public void DisposeCpuResources()
		{
			this.m_aCpuCell = null;
		}

		public void DisposeGpuResources()
		{
			if (this.m_effectorIndexBuffer != null)
			{
				this.m_effectorIndexBuffer.Dispose();
				this.m_effectorIndexBuffer = null;
			}
			if (this.m_reactorParamsBuffer != null)
			{
				this.m_reactorParamsBuffer.Dispose();
				this.m_reactorParamsBuffer = null;
			}
			if (this.m_fieldParamsBuffer != null)
			{
				this.m_fieldParamsBuffer.Dispose();
				this.m_fieldParamsBuffer = null;
			}
			if (this.m_cellsBuffer != null)
			{
				this.m_cellsBuffer.Dispose();
				this.m_cellsBuffer = null;
			}
			if (this.m_cellsBuffer != null)
			{
				this.m_cellsBuffer.Dispose();
				this.m_cellsBuffer = null;
			}
		}

		public bool SampleCpuGrid(Vector3 p, out Vector3 positionOffset, out Vector4 rotationOffset)
		{
			bool flag = false;
			switch (this.FalloffDimensions)
			{
			case BoingReactorField.FalloffDimensionsEnum.XYZ:
				flag = this.m_bounds.Contains(p);
				break;
			case BoingReactorField.FalloffDimensionsEnum.XY:
				flag = this.m_bounds.ContainsX(p) && this.m_bounds.ContainsY(p);
				break;
			case BoingReactorField.FalloffDimensionsEnum.XZ:
				flag = this.m_bounds.ContainsX(p) && this.m_bounds.ContainsZ(p);
				break;
			case BoingReactorField.FalloffDimensionsEnum.YZ:
				flag = this.m_bounds.ContainsY(p) && this.m_bounds.ContainsZ(p);
				break;
			}
			if (!flag)
			{
				positionOffset = Vector3.zero;
				rotationOffset = QuaternionUtil.ToVector4(Quaternion.identity);
				return false;
			}
			float num = 0.5f * this.CellSize;
			Vector3 vector = p - (this.m_gridCenter + this.GetCellCenterOffset(0, 0, 0));
			Vector3 vector2 = this.QuantizeNorm(vector + new Vector3(-num, -num, -num));
			Vector3 vector3 = vector2 * this.CellSize;
			int num2 = Mathf.Clamp((int)vector2.x, 0, this.CellsX - 1);
			int num3 = Mathf.Clamp((int)vector2.y, 0, this.CellsY - 1);
			int num4 = Mathf.Clamp((int)vector2.z, 0, this.CellsZ - 1);
			int num5 = Mathf.Min(num2 + 1, this.CellsX - 1);
			int num6 = Mathf.Min(num3 + 1, this.CellsY - 1);
			int num7 = Mathf.Min(num4 + 1, this.CellsZ - 1);
			int num8;
			int num9;
			int num10;
			this.ResolveCellIndex(num2, num3, num4, 1, out num8, out num9, out num10);
			int num11;
			int num12;
			int num13;
			this.ResolveCellIndex(num5, num6, num7, 1, out num11, out num12, out num13);
			bool flag2 = num8 != num11;
			bool flag3 = num9 != num12;
			bool flag4 = num10 != num13;
			Vector3 vector4 = (vector - vector3) / this.CellSize;
			Vector3 vector5 = p - base.transform.position;
			switch (this.FalloffDimensions)
			{
			case BoingReactorField.FalloffDimensionsEnum.XY:
				vector5.z = 0f;
				break;
			case BoingReactorField.FalloffDimensionsEnum.XZ:
				vector5.y = 0f;
				break;
			case BoingReactorField.FalloffDimensionsEnum.YZ:
				vector5.x = 0f;
				break;
			}
			int num14 = Mathf.Max(this.CellsX, Mathf.Max(this.CellsY, this.CellsZ));
			float num15 = 1f;
			BoingReactorField.FalloffModeEnum falloffMode = this.FalloffMode;
			if (falloffMode != BoingReactorField.FalloffModeEnum.Circle)
			{
				if (falloffMode == BoingReactorField.FalloffModeEnum.Square)
				{
					Vector3 vector6 = num * new Vector3((float)this.CellsX, (float)this.CellsY, (float)this.CellsZ);
					Vector3 vector7 = this.FalloffRatio * vector6 - num * Vector3.one;
					vector7.x = Mathf.Max(0f, vector7.x);
					vector7.y = Mathf.Max(0f, vector7.y);
					vector7.z = Mathf.Max(0f, vector7.z);
					Vector3 vector8 = (1f - this.FalloffRatio) * vector6 - num * Vector3.one;
					vector8.x = Mathf.Max(MathUtil.Epsilon, vector8.x);
					vector8.y = Mathf.Max(MathUtil.Epsilon, vector8.y);
					vector8.z = Mathf.Max(MathUtil.Epsilon, vector8.z);
					Vector3 vector9 = new Vector3(1f - Mathf.Clamp01((Mathf.Abs(vector5.x) - vector7.x) / vector8.x), 1f - Mathf.Clamp01((Mathf.Abs(vector5.y) - vector7.y) / vector8.y), 1f - Mathf.Clamp01((Mathf.Abs(vector5.z) - vector7.z) / vector8.z));
					switch (this.FalloffDimensions)
					{
					case BoingReactorField.FalloffDimensionsEnum.XY:
						vector9.x = 1f;
						break;
					case BoingReactorField.FalloffDimensionsEnum.XZ:
						vector9.y = 1f;
						break;
					case BoingReactorField.FalloffDimensionsEnum.YZ:
						vector9.z = 1f;
						break;
					}
					num15 = Mathf.Min(vector9.x, Mathf.Min(vector9.y, vector9.z));
				}
			}
			else
			{
				float num16 = num * (float)num14;
				Vector3 vector10 = new Vector3((float)num14 / (float)this.CellsX, (float)num14 / (float)this.CellsY, (float)num14 / (float)this.CellsZ);
				vector5.x *= vector10.x;
				vector5.y *= vector10.y;
				vector5.z *= vector10.z;
				float magnitude = vector5.magnitude;
				float num17 = Mathf.Max(0f, this.FalloffRatio * num16 - num);
				float num18 = Mathf.Max(MathUtil.Epsilon, (1f - this.FalloffRatio) * num16 - num);
				num15 = 1f - Mathf.Clamp01((magnitude - num17) / num18);
			}
			BoingReactorField.s_aCellOffset[0] = this.m_aCpuCell[num10, num9, num8].PositionSpring.Value - this.m_gridCenter - this.GetCellCenterOffset(num2, num3, num4);
			BoingReactorField.s_aCellOffset[1] = this.m_aCpuCell[num10, num9, num11].PositionSpring.Value - this.m_gridCenter - this.GetCellCenterOffset(num5, num3, num4);
			BoingReactorField.s_aCellOffset[2] = this.m_aCpuCell[num10, num12, num8].PositionSpring.Value - this.m_gridCenter - this.GetCellCenterOffset(num2, num6, num4);
			BoingReactorField.s_aCellOffset[3] = this.m_aCpuCell[num10, num12, num11].PositionSpring.Value - this.m_gridCenter - this.GetCellCenterOffset(num5, num6, num4);
			BoingReactorField.s_aCellOffset[4] = this.m_aCpuCell[num13, num9, num8].PositionSpring.Value - this.m_gridCenter - this.GetCellCenterOffset(num2, num3, num7);
			BoingReactorField.s_aCellOffset[5] = this.m_aCpuCell[num13, num9, num11].PositionSpring.Value - this.m_gridCenter - this.GetCellCenterOffset(num5, num3, num7);
			BoingReactorField.s_aCellOffset[6] = this.m_aCpuCell[num13, num12, num8].PositionSpring.Value - this.m_gridCenter - this.GetCellCenterOffset(num2, num6, num7);
			BoingReactorField.s_aCellOffset[7] = this.m_aCpuCell[num13, num12, num11].PositionSpring.Value - this.m_gridCenter - this.GetCellCenterOffset(num5, num6, num7);
			positionOffset = VectorUtil.TriLerp(ref BoingReactorField.s_aCellOffset[0], ref BoingReactorField.s_aCellOffset[1], ref BoingReactorField.s_aCellOffset[2], ref BoingReactorField.s_aCellOffset[3], ref BoingReactorField.s_aCellOffset[4], ref BoingReactorField.s_aCellOffset[5], ref BoingReactorField.s_aCellOffset[6], ref BoingReactorField.s_aCellOffset[7], flag2, flag3, flag4, vector4.x, vector4.y, vector4.z);
			rotationOffset = VectorUtil.TriLerp(ref this.m_aCpuCell[num10, num9, num8].RotationSpring.ValueVec, ref this.m_aCpuCell[num10, num9, num11].RotationSpring.ValueVec, ref this.m_aCpuCell[num10, num12, num8].RotationSpring.ValueVec, ref this.m_aCpuCell[num10, num12, num11].RotationSpring.ValueVec, ref this.m_aCpuCell[num13, num9, num8].RotationSpring.ValueVec, ref this.m_aCpuCell[num13, num9, num11].RotationSpring.ValueVec, ref this.m_aCpuCell[num13, num12, num8].RotationSpring.ValueVec, ref this.m_aCpuCell[num13, num12, num11].RotationSpring.ValueVec, flag2, flag3, flag4, vector4.x, vector4.y, vector4.z);
			positionOffset *= num15;
			rotationOffset = QuaternionUtil.ToVector4(QuaternionUtil.Pow(QuaternionUtil.FromVector4(rotationOffset, true), num15));
			return true;
		}

		private void UpdateFieldParamsGpu()
		{
			this.m_fieldParams.CellsX = this.CellsX;
			this.m_fieldParams.CellsY = this.CellsY;
			this.m_fieldParams.CellsZ = this.CellsZ;
			this.m_fieldParams.NumEffectors = 0;
			if (this.Effectors != null)
			{
				foreach (BoingEffector boingEffector in this.Effectors)
				{
					if (!(boingEffector == null))
					{
						BoingEffector component = boingEffector.GetComponent<BoingEffector>();
						if (!(component == null) && component.isActiveAndEnabled)
						{
							this.m_fieldParams.NumEffectors = this.m_fieldParams.NumEffectors + 1;
						}
					}
				}
			}
			this.m_fieldParams.iCellBaseX = this.m_iCellBaseX;
			this.m_fieldParams.iCellBaseY = this.m_iCellBaseY;
			this.m_fieldParams.iCellBaseZ = this.m_iCellBaseZ;
			this.m_fieldParams.FalloffMode = (int)this.FalloffMode;
			this.m_fieldParams.FalloffDimensions = (int)this.FalloffDimensions;
			this.m_fieldParams.PropagationDepth = this.PropagationDepth;
			this.m_fieldParams.GridCenter = this.m_gridCenter;
			this.m_fieldParams.UpWs = (this.Params.Bits.IsBitSet(6) ? this.Params.RotationReactionUp : (base.transform.rotation * VectorUtil.NormalizeSafe(this.Params.RotationReactionUp, Vector3.up)));
			this.m_fieldParams.FieldPosition = base.transform.position;
			this.m_fieldParams.FalloffRatio = this.FalloffRatio;
			this.m_fieldParams.CellSize = this.CellSize;
			this.m_fieldParams.DeltaTime = Time.deltaTime;
			if (this.m_fieldParamsBuffer != null)
			{
				this.m_fieldParamsBuffer.SetData(new BoingReactorField.FieldParams[] { this.m_fieldParams });
			}
		}

		private void UpdateFlags()
		{
			this.Params.Bits.SetBit(0, this.TwoDDistanceCheck);
			this.Params.Bits.SetBit(1, this.TwoDPositionInfluence);
			this.Params.Bits.SetBit(2, this.TwoDRotationInfluence);
			this.Params.Bits.SetBit(3, this.EnablePositionEffect);
			this.Params.Bits.SetBit(4, this.EnableRotationEffect);
			this.Params.Bits.SetBit(6, this.GlobalReactionUpVector);
			this.Params.Bits.SetBit(7, this.EnablePropagation);
			this.Params.Bits.SetBit(8, this.AnchorPropagationAtBorder);
		}

		public void UpdateBounds()
		{
			this.m_bounds = new Aabb(this.m_gridCenter + this.GetCellCenterOffset(0, 0, 0), this.m_gridCenter + this.GetCellCenterOffset(this.CellsX - 1, this.CellsY - 1, this.CellsZ - 1));
			this.m_bounds.Expand(this.CellSize);
		}

		public void PrepareExecute()
		{
			this.Init();
			if (this.SharedParams != null)
			{
				BoingWork.Params.Copy(ref this.SharedParams.Params, ref this.Params);
			}
			this.UpdateFlags();
			this.UpdateBounds();
			BoingReactorField.HardwareModeEnum hardwareModeEnum;
			if (this.m_hardwareMode != this.HardwareMode)
			{
				hardwareModeEnum = this.m_hardwareMode;
				if (hardwareModeEnum != BoingReactorField.HardwareModeEnum.CPU)
				{
					if (hardwareModeEnum == BoingReactorField.HardwareModeEnum.GPU)
					{
						this.DisposeGpuResources();
					}
				}
				else
				{
					this.DisposeCpuResources();
				}
				this.m_hardwareMode = this.HardwareMode;
			}
			hardwareModeEnum = this.m_hardwareMode;
			if (hardwareModeEnum != BoingReactorField.HardwareModeEnum.CPU)
			{
				if (hardwareModeEnum == BoingReactorField.HardwareModeEnum.GPU)
				{
					this.ValidateGpuResources();
				}
			}
			else
			{
				this.ValidateCpuResources();
			}
			this.HandleCellMove();
			hardwareModeEnum = this.m_hardwareMode;
			if (hardwareModeEnum == BoingReactorField.HardwareModeEnum.CPU)
			{
				this.FinishPrepareExecuteCpu();
				return;
			}
			if (hardwareModeEnum != BoingReactorField.HardwareModeEnum.GPU)
			{
				return;
			}
			this.FinishPrepareExecuteGpu();
		}

		private void ValidateCpuResources()
		{
			this.CellsX = Mathf.Max(1, this.CellsX);
			this.CellsY = Mathf.Max(1, this.CellsY);
			this.CellsZ = Mathf.Max(1, this.CellsZ);
			if (this.m_aCpuCell == null || this.m_cellsX != this.CellsX || this.m_cellsY != this.CellsY || this.m_cellsZ != this.CellsZ)
			{
				this.m_aCpuCell = new BoingWork.Params.InstanceData[this.CellsZ, this.CellsY, this.CellsX];
				for (int i = 0; i < this.CellsZ; i++)
				{
					for (int j = 0; j < this.CellsY; j++)
					{
						for (int k = 0; k < this.CellsX; k++)
						{
							int num;
							int num2;
							int num3;
							this.ResolveCellIndex(k, j, i, -1, out num, out num2, out num3);
							this.m_aCpuCell[i, j, k].Reset(this.m_gridCenter + this.GetCellCenterOffset(num, num2, num3), false);
						}
					}
				}
				this.m_cellsX = this.CellsX;
				this.m_cellsY = this.CellsY;
				this.m_cellsZ = this.CellsZ;
			}
		}

		private void ValidateGpuResources()
		{
			bool flag = false;
			bool flag2 = this.m_shader == null || BoingReactorField.s_computeKernelId == null;
			if (flag2)
			{
				this.m_shader = Resources.Load<ComputeShader>("Boing Kit/BoingReactorFieldCompute");
				flag = true;
				if (BoingReactorField.s_computeKernelId == null)
				{
					BoingReactorField.s_computeKernelId = new BoingReactorField.ComputeKernelId();
					BoingReactorField.s_computeKernelId.InitKernel = this.m_shader.FindKernel("Init");
					BoingReactorField.s_computeKernelId.MoveKernel = this.m_shader.FindKernel("Move");
					BoingReactorField.s_computeKernelId.WrapXKernel = this.m_shader.FindKernel("WrapX");
					BoingReactorField.s_computeKernelId.WrapYKernel = this.m_shader.FindKernel("WrapY");
					BoingReactorField.s_computeKernelId.WrapZKernel = this.m_shader.FindKernel("WrapZ");
					BoingReactorField.s_computeKernelId.ExecuteKernel = this.m_shader.FindKernel("Execute");
				}
			}
			bool flag3 = this.m_effectorIndexBuffer == null || (this.Effectors != null && this.m_numEffectors != this.Effectors.Length);
			if (flag3 && this.Effectors != null)
			{
				if (this.m_effectorIndexBuffer != null)
				{
					this.m_effectorIndexBuffer.Dispose();
				}
				this.m_effectorIndexBuffer = new ComputeBuffer(this.Effectors.Length, 4);
				flag = true;
				this.m_numEffectors = this.Effectors.Length;
			}
			if (flag2 || flag3)
			{
				this.m_shader.SetBuffer(BoingReactorField.s_computeKernelId.ExecuteKernel, BoingReactorField.ShaderPropertyId.EffectorIndices, this.m_effectorIndexBuffer);
			}
			bool flag4 = this.m_reactorParamsBuffer == null;
			if (flag4)
			{
				this.m_reactorParamsBuffer = new ComputeBuffer(1, BoingWork.Params.Stride);
				flag = true;
			}
			if (flag2 || flag4)
			{
				this.m_shader.SetBuffer(BoingReactorField.s_computeKernelId.ExecuteKernel, BoingReactorField.ShaderPropertyId.ReactorParams, this.m_reactorParamsBuffer);
			}
			bool flag5 = this.m_fieldParamsBuffer == null;
			if (flag5)
			{
				this.m_fieldParamsBuffer = new ComputeBuffer(1, BoingReactorField.FieldParams.Stride);
				flag = true;
			}
			if (flag2 || flag5)
			{
				this.m_shader.SetBuffer(BoingReactorField.s_computeKernelId.InitKernel, BoingReactorField.ShaderPropertyId.ComputeFieldParams, this.m_fieldParamsBuffer);
				this.m_shader.SetBuffer(BoingReactorField.s_computeKernelId.MoveKernel, BoingReactorField.ShaderPropertyId.ComputeFieldParams, this.m_fieldParamsBuffer);
				this.m_shader.SetBuffer(BoingReactorField.s_computeKernelId.WrapXKernel, BoingReactorField.ShaderPropertyId.ComputeFieldParams, this.m_fieldParamsBuffer);
				this.m_shader.SetBuffer(BoingReactorField.s_computeKernelId.WrapYKernel, BoingReactorField.ShaderPropertyId.ComputeFieldParams, this.m_fieldParamsBuffer);
				this.m_shader.SetBuffer(BoingReactorField.s_computeKernelId.WrapZKernel, BoingReactorField.ShaderPropertyId.ComputeFieldParams, this.m_fieldParamsBuffer);
				this.m_shader.SetBuffer(BoingReactorField.s_computeKernelId.ExecuteKernel, BoingReactorField.ShaderPropertyId.ComputeFieldParams, this.m_fieldParamsBuffer);
			}
			this.m_cellBufferNeedsReset = this.m_cellsBuffer == null || this.m_cellsX != this.CellsX || this.m_cellsY != this.CellsY || this.m_cellsZ != this.CellsZ;
			if (this.m_cellBufferNeedsReset)
			{
				if (this.m_cellsBuffer != null)
				{
					this.m_cellsBuffer.Dispose();
				}
				int num = this.CellsX * this.CellsY * this.CellsZ;
				this.m_cellsBuffer = new ComputeBuffer(num, BoingWork.Params.InstanceData.Stride);
				BoingWork.Params.InstanceData[] array = new BoingWork.Params.InstanceData[num];
				for (int i = 0; i < num; i++)
				{
					array[i].PositionSpring.Reset();
					array[i].RotationSpring.Reset();
				}
				this.m_cellsBuffer.SetData(array);
				flag = true;
				this.m_cellsX = this.CellsX;
				this.m_cellsY = this.CellsY;
				this.m_cellsZ = this.CellsZ;
			}
			if (flag2 || this.m_cellBufferNeedsReset)
			{
				this.m_shader.SetBuffer(BoingReactorField.s_computeKernelId.InitKernel, BoingReactorField.ShaderPropertyId.ComputeCells, this.m_cellsBuffer);
				this.m_shader.SetBuffer(BoingReactorField.s_computeKernelId.MoveKernel, BoingReactorField.ShaderPropertyId.ComputeCells, this.m_cellsBuffer);
				this.m_shader.SetBuffer(BoingReactorField.s_computeKernelId.WrapXKernel, BoingReactorField.ShaderPropertyId.ComputeCells, this.m_cellsBuffer);
				this.m_shader.SetBuffer(BoingReactorField.s_computeKernelId.WrapYKernel, BoingReactorField.ShaderPropertyId.ComputeCells, this.m_cellsBuffer);
				this.m_shader.SetBuffer(BoingReactorField.s_computeKernelId.WrapZKernel, BoingReactorField.ShaderPropertyId.ComputeCells, this.m_cellsBuffer);
				this.m_shader.SetBuffer(BoingReactorField.s_computeKernelId.ExecuteKernel, BoingReactorField.ShaderPropertyId.ComputeCells, this.m_cellsBuffer);
			}
			if (flag)
			{
				this.m_gpuResourceSetId++;
				if (this.m_gpuResourceSetId < 0)
				{
					this.m_gpuResourceSetId = -1;
				}
			}
		}

		private void FinishPrepareExecuteCpu()
		{
			Quaternion rotation = base.transform.rotation;
			for (int i = 0; i < this.CellsZ; i++)
			{
				for (int j = 0; j < this.CellsY; j++)
				{
					for (int k = 0; k < this.CellsX; k++)
					{
						int num;
						int num2;
						int num3;
						this.ResolveCellIndex(k, j, i, -1, out num, out num2, out num3);
						this.m_aCpuCell[i, j, k].PrepareExecute(ref this.Params, this.m_gridCenter, rotation, this.GetCellCenterOffset(num, num2, num3));
					}
				}
			}
		}

		private void FinishPrepareExecuteGpu()
		{
			if (this.m_cellBufferNeedsReset)
			{
				this.UpdateFieldParamsGpu();
				this.m_shader.Dispatch(BoingReactorField.s_computeKernelId.InitKernel, this.CellsX, this.CellsY, this.CellsZ);
			}
		}

		public void Init()
		{
			if (this.m_init)
			{
				return;
			}
			this.m_hardwareMode = this.HardwareMode;
			this.m_init = true;
		}

		public void Sanitize()
		{
			if (this.PropagationDepth < 0)
			{
				Debug.LogWarning("Propagation iterations must be a positive number.");
			}
			else if (this.PropagationDepth > 3)
			{
				Debug.LogWarning("For performance reasons, propagation is limited to 3 iterations.");
			}
			this.PropagationDepth = Mathf.Clamp(this.PropagationDepth, 1, 3);
		}

		public void HandleCellMove()
		{
			if (this.m_cellMoveMode != this.CellMoveMode)
			{
				this.Reboot();
				this.m_cellMoveMode = this.CellMoveMode;
			}
			BoingReactorField.CellMoveModeEnum cellMoveMode = this.CellMoveMode;
			BoingReactorField.HardwareModeEnum hardwareModeEnum;
			if (cellMoveMode == BoingReactorField.CellMoveModeEnum.Follow)
			{
				Vector3 vector = base.transform.position - this.m_gridCenter;
				hardwareModeEnum = this.HardwareMode;
				if (hardwareModeEnum != BoingReactorField.HardwareModeEnum.CPU)
				{
					if (hardwareModeEnum == BoingReactorField.HardwareModeEnum.GPU)
					{
						this.UpdateFieldParamsGpu();
						this.m_shader.SetVector(BoingReactorField.ShaderPropertyId.MoveParams, vector);
						this.m_shader.Dispatch(BoingReactorField.s_computeKernelId.MoveKernel, this.CellsX, this.CellsY, this.CellsZ);
					}
				}
				else
				{
					for (int i = 0; i < this.CellsZ; i++)
					{
						for (int j = 0; j < this.CellsY; j++)
						{
							for (int k = 0; k < this.CellsX; k++)
							{
								ref BoingWork.Params.InstanceData ptr = ref this.m_aCpuCell[i, j, k];
								ptr.PositionSpring.Value = ptr.PositionSpring.Value + vector;
							}
						}
					}
				}
				this.m_gridCenter = base.transform.position;
				this.m_qPrevGridCenterNorm = this.QuantizeNorm(this.m_gridCenter);
				return;
			}
			if (cellMoveMode != BoingReactorField.CellMoveModeEnum.WrapAround)
			{
				return;
			}
			this.m_gridCenter = base.transform.position;
			Vector3 vector2 = this.QuantizeNorm(this.m_gridCenter);
			this.m_gridCenter = vector2 * this.CellSize;
			int num = (int)(vector2.x - this.m_qPrevGridCenterNorm.x);
			int num2 = (int)(vector2.y - this.m_qPrevGridCenterNorm.y);
			int num3 = (int)(vector2.z - this.m_qPrevGridCenterNorm.z);
			this.m_qPrevGridCenterNorm = vector2;
			if (num == 0 && num2 == 0 && num3 == 0)
			{
				return;
			}
			hardwareModeEnum = this.m_hardwareMode;
			if (hardwareModeEnum != BoingReactorField.HardwareModeEnum.CPU)
			{
				if (hardwareModeEnum == BoingReactorField.HardwareModeEnum.GPU)
				{
					this.WrapGpu(num, num2, num3);
				}
			}
			else
			{
				this.WrapCpu(num, num2, num3);
			}
			this.m_iCellBaseX = MathUtil.Modulo(this.m_iCellBaseX + num, this.CellsX);
			this.m_iCellBaseY = MathUtil.Modulo(this.m_iCellBaseY + num2, this.CellsY);
			this.m_iCellBaseZ = MathUtil.Modulo(this.m_iCellBaseZ + num3, this.CellsZ);
		}

		private void InitPropagationCpu(ref BoingWork.Params.InstanceData data)
		{
			data.PositionPropagationWorkData = Vector3.zero;
			data.RotationPropagationWorkData = Vector3.zero;
		}

		private void PropagateSpringCpu(ref BoingWork.Params.InstanceData data, float dt)
		{
			data.PositionSpring.Velocity = data.PositionSpring.Velocity + BoingReactorField.kPropagationFactor * this.PositionPropagation * data.PositionPropagationWorkData * dt;
			data.RotationSpring.VelocityVec = data.RotationSpring.VelocityVec + BoingReactorField.kPropagationFactor * this.RotationPropagation * data.RotationPropagationWorkData * dt;
		}

		private void ExtendPropagationBorder(ref BoingWork.Params.InstanceData data, float weight, int adjDeltaX, int adjDeltaY, int adjDeltaZ)
		{
			data.PositionPropagationWorkData += weight * (data.PositionOrigin + new Vector3((float)adjDeltaX, (float)adjDeltaY, (float)adjDeltaZ) * this.CellSize);
			data.RotationPropagationWorkData += weight * data.RotationOrigin;
		}

		private void AccumulatePropagationWeightedNeighbor(ref BoingWork.Params.InstanceData data, ref BoingWork.Params.InstanceData neighbor, float weight)
		{
			data.PositionPropagationWorkData += weight * (neighbor.PositionSpring.Value - neighbor.PositionOrigin);
			data.RotationPropagationWorkData += weight * (neighbor.RotationSpring.ValueVec - neighbor.RotationOrigin);
		}

		private void GatherPropagation(ref BoingWork.Params.InstanceData data, float weightSum)
		{
			data.PositionPropagationWorkData = data.PositionPropagationWorkData / weightSum - (data.PositionSpring.Value - data.PositionOrigin);
			data.RotationPropagationWorkData = data.RotationPropagationWorkData / weightSum - (data.RotationSpring.ValueVec - data.RotationOrigin);
		}

		private void AnchorPropagationBorder(ref BoingWork.Params.InstanceData data)
		{
			data.PositionPropagationWorkData = Vector3.zero;
			data.RotationPropagationWorkData = Vector3.zero;
		}

		private void PropagateCpu(float dt)
		{
			int[] array = new int[this.PropagationDepth * 2 + 1];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = i - this.PropagationDepth;
			}
			for (int j = 0; j < this.CellsZ; j++)
			{
				for (int k = 0; k < this.CellsY; k++)
				{
					for (int l = 0; l < this.CellsX; l++)
					{
						this.InitPropagationCpu(ref this.m_aCpuCell[j, k, l]);
					}
				}
			}
			for (int m = 0; m < this.CellsZ; m++)
			{
				for (int n = 0; n < this.CellsY; n++)
				{
					for (int num = 0; num < this.CellsX; num++)
					{
						int num2;
						int num3;
						int num4;
						this.ResolveCellIndex(num, n, m, -1, out num2, out num3, out num4);
						float num5 = 0f;
						foreach (int num7 in array)
						{
							foreach (int num9 in array)
							{
								foreach (int num11 in array)
								{
									if (num11 != 0 || num9 != 0 || num7 != 0)
									{
										int num12 = num11 * num11 + num9 * num9 + num7 * num7;
										float num13 = BoingReactorField.s_aSqrtInv[num12];
										num5 += num13;
										if ((this.CellsX <= 2 || ((num2 != 0 || num11 >= 0) && (num2 != this.CellsX - 1 || num11 <= 0))) && (this.CellsY <= 2 || ((num3 != 0 || num9 >= 0) && (num3 != this.CellsY - 1 || num9 <= 0))) && (this.CellsZ <= 2 || ((num4 != 0 || num7 >= 0) && (num4 != this.CellsZ - 1 || num7 <= 0))))
										{
											int num14 = MathUtil.Modulo(num + num11, this.CellsX);
											int num15 = MathUtil.Modulo(n + num9, this.CellsY);
											int num16 = MathUtil.Modulo(m + num7, this.CellsZ);
											this.AccumulatePropagationWeightedNeighbor(ref this.m_aCpuCell[m, n, num], ref this.m_aCpuCell[num16, num15, num14], num13);
										}
									}
								}
							}
						}
						if (num5 > 0f)
						{
							this.GatherPropagation(ref this.m_aCpuCell[m, n, num], num5);
						}
					}
				}
			}
			if (this.AnchorPropagationAtBorder)
			{
				for (int num17 = 0; num17 < this.CellsZ; num17++)
				{
					for (int num18 = 0; num18 < this.CellsY; num18++)
					{
						for (int num19 = 0; num19 < this.CellsX; num19++)
						{
							int num20;
							int num21;
							int num22;
							this.ResolveCellIndex(num19, num18, num17, -1, out num20, out num21, out num22);
							if (((num20 == 0 || num20 == this.CellsX - 1) && this.CellsX > 2) || ((num21 == 0 || num21 == this.CellsY - 1) && this.CellsY > 2) || ((num22 == 0 || num22 == this.CellsZ - 1) && this.CellsZ > 2))
							{
								this.AnchorPropagationBorder(ref this.m_aCpuCell[num17, num18, num19]);
							}
						}
					}
				}
			}
			for (int num23 = 0; num23 < this.CellsZ; num23++)
			{
				for (int num24 = 0; num24 < this.CellsY; num24++)
				{
					for (int num25 = 0; num25 < this.CellsX; num25++)
					{
						this.PropagateSpringCpu(ref this.m_aCpuCell[num23, num24, num25], dt);
					}
				}
			}
		}

		private void WrapCpu(int deltaX, int deltaY, int deltaZ)
		{
			if (deltaX != 0)
			{
				int num = ((deltaX > 0) ? (-1) : 1);
				for (int i = 0; i < this.CellsZ; i++)
				{
					for (int j = 0; j < this.CellsY; j++)
					{
						int num2 = ((deltaX > 0) ? (deltaX - 1) : (this.CellsX + deltaX));
						while (num2 >= 0 && num2 < this.CellsX)
						{
							int num3;
							int num4;
							int num5;
							this.ResolveCellIndex(num2, j, i, 1, out num3, out num4, out num5);
							int num6;
							int num7;
							int num8;
							this.ResolveCellIndex(num3 - deltaX, num4 - deltaY, num5 - deltaZ, -1, out num6, out num7, out num8);
							this.m_aCpuCell[num5, num4, num3].Reset(this.m_gridCenter + this.GetCellCenterOffset(num6, num7, num8), true);
							num2 += num;
						}
					}
				}
			}
			if (deltaY != 0)
			{
				int num9 = ((deltaY > 0) ? (-1) : 1);
				for (int k = 0; k < this.CellsZ; k++)
				{
					int num10 = ((deltaY > 0) ? (deltaY - 1) : (this.CellsY + deltaY));
					while (num10 >= 0 && num10 < this.CellsY)
					{
						for (int l = 0; l < this.CellsX; l++)
						{
							int num11;
							int num12;
							int num13;
							this.ResolveCellIndex(l, num10, k, 1, out num11, out num12, out num13);
							int num14;
							int num15;
							int num16;
							this.ResolveCellIndex(num11 - deltaX, num12 - deltaY, num13 - deltaZ, -1, out num14, out num15, out num16);
							this.m_aCpuCell[num13, num12, num11].Reset(this.m_gridCenter + this.GetCellCenterOffset(num14, num15, num16), true);
						}
						num10 += num9;
					}
				}
			}
			if (deltaZ != 0)
			{
				int num17 = ((deltaZ > 0) ? (-1) : 1);
				int num18 = ((deltaZ > 0) ? (deltaZ - 1) : (this.CellsZ + deltaZ));
				while (num18 >= 0 && num18 < this.CellsZ)
				{
					for (int m = 0; m < this.CellsY; m++)
					{
						for (int n = 0; n < this.CellsX; n++)
						{
							int num19;
							int num20;
							int num21;
							this.ResolveCellIndex(n, m, num18, 1, out num19, out num20, out num21);
							int num22;
							int num23;
							int num24;
							this.ResolveCellIndex(num19 - deltaX, num20 - deltaY, num21 - deltaZ, -1, out num22, out num23, out num24);
							this.m_aCpuCell[num21, num20, num19].Reset(this.m_gridCenter + this.GetCellCenterOffset(num22, num23, num24), true);
						}
					}
					num18 += num17;
				}
			}
		}

		private void WrapGpu(int deltaX, int deltaY, int deltaZ)
		{
			this.UpdateFieldParamsGpu();
			this.m_shader.SetInts(BoingReactorField.ShaderPropertyId.WrapParams, new int[] { deltaX, deltaY, deltaZ });
			if (deltaX != 0)
			{
				this.m_shader.Dispatch(BoingReactorField.s_computeKernelId.WrapXKernel, 1, this.CellsY, this.CellsZ);
			}
			if (deltaY != 0)
			{
				this.m_shader.Dispatch(BoingReactorField.s_computeKernelId.WrapYKernel, this.CellsX, 1, this.CellsZ);
			}
			if (deltaZ != 0)
			{
				this.m_shader.Dispatch(BoingReactorField.s_computeKernelId.WrapZKernel, this.CellsX, this.CellsY, 1);
			}
		}

		public void ExecuteCpu(float dt)
		{
			this.PrepareExecute();
			if (this.Effectors == null || this.Effectors.Length == 0)
			{
				return;
			}
			if (this.EnablePropagation)
			{
				this.PropagateCpu(dt);
			}
			foreach (BoingEffector boingEffector in this.Effectors)
			{
				if (!(boingEffector == null))
				{
					BoingEffector.Params @params = default(BoingEffector.Params);
					@params.Fill(boingEffector);
					if (this.m_bounds.Intersects(ref @params))
					{
						for (int j = 0; j < this.CellsZ; j++)
						{
							for (int k = 0; k < this.CellsY; k++)
							{
								for (int l = 0; l < this.CellsX; l++)
								{
									this.m_aCpuCell[j, k, l].AccumulateTarget(ref this.Params, ref @params, dt);
								}
							}
						}
					}
				}
			}
			for (int m = 0; m < this.CellsZ; m++)
			{
				for (int n = 0; n < this.CellsY; n++)
				{
					for (int num = 0; num < this.CellsX; num++)
					{
						this.m_aCpuCell[m, n, num].EndAccumulateTargets(ref this.Params);
						this.m_aCpuCell[m, n, num].Execute(ref this.Params, dt);
					}
				}
			}
		}

		public void ExecuteGpu(float dt, ComputeBuffer effectorParamsBuffer, Dictionary<int, int> effectorParamsIndexMap)
		{
			this.PrepareExecute();
			this.UpdateFieldParamsGpu();
			this.m_shader.SetBuffer(BoingReactorField.s_computeKernelId.ExecuteKernel, BoingReactorField.ShaderPropertyId.Effectors, effectorParamsBuffer);
			if (this.m_fieldParams.NumEffectors > 0)
			{
				int[] array = new int[this.m_fieldParams.NumEffectors];
				int num = 0;
				foreach (BoingEffector boingEffector in this.Effectors)
				{
					if (!(boingEffector == null))
					{
						BoingEffector component = boingEffector.GetComponent<BoingEffector>();
						int num2;
						if (!(component == null) && component.isActiveAndEnabled && effectorParamsIndexMap.TryGetValue(component.GetInstanceID(), out num2))
						{
							array[num++] = num2;
						}
					}
				}
				this.m_effectorIndexBuffer.SetData(array);
			}
			this.s_aReactorParams[0] = this.Params;
			this.m_reactorParamsBuffer.SetData(this.s_aReactorParams);
			this.m_shader.SetVector(BoingReactorField.ShaderPropertyId.PropagationParams, new Vector4(this.PositionPropagation, this.RotationPropagation, BoingReactorField.kPropagationFactor, 0f));
			this.m_shader.Dispatch(BoingReactorField.s_computeKernelId.ExecuteKernel, this.CellsX, this.CellsY, this.CellsZ);
		}

		public void OnDrawGizmosSelected()
		{
			if (!base.isActiveAndEnabled)
			{
				return;
			}
			this.DrawGizmos(true);
		}

		private void DrawGizmos(bool drawEffectors)
		{
			Vector3 vector = this.GetGridCenter();
			BoingReactorField.CellMoveModeEnum cellMoveMode = this.CellMoveMode;
			if (cellMoveMode != BoingReactorField.CellMoveModeEnum.Follow)
			{
				if (cellMoveMode == BoingReactorField.CellMoveModeEnum.WrapAround)
				{
					vector = new Vector3(Mathf.Round(base.transform.position.x / this.CellSize), Mathf.Round(base.transform.position.y / this.CellSize), Mathf.Round(base.transform.position.z / this.CellSize)) * this.CellSize;
				}
			}
			else
			{
				vector = base.transform.position;
			}
			BoingWork.Params.InstanceData[,,] array = null;
			BoingReactorField.HardwareModeEnum hardwareMode = this.HardwareMode;
			if (hardwareMode != BoingReactorField.HardwareModeEnum.CPU)
			{
				if (hardwareMode == BoingReactorField.HardwareModeEnum.GPU)
				{
					if (this.m_cellsBuffer != null)
					{
						array = new BoingWork.Params.InstanceData[this.CellsZ, this.CellsY, this.CellsX];
						this.m_cellsBuffer.GetData(array);
					}
				}
			}
			else
			{
				array = this.m_aCpuCell;
			}
			int num = 1;
			if (this.CellsX * this.CellsY * this.CellsZ > 1024)
			{
				num = 2;
			}
			if (this.CellsX * this.CellsY * this.CellsZ > 4096)
			{
				num = 3;
			}
			if (this.CellsX * this.CellsY * this.CellsZ > 8192)
			{
				num = 4;
			}
			for (int i = 0; i < this.CellsZ; i++)
			{
				for (int j = 0; j < this.CellsY; j++)
				{
					for (int k = 0; k < this.CellsX; k++)
					{
						int num2;
						int num3;
						int num4;
						this.ResolveCellIndex(k, j, i, -1, out num2, out num3, out num4);
						Vector3 vector2 = vector + this.GetCellCenterOffset(num2, num3, num4);
						if (array != null && k % num == 0 && j % num == 0 && i % num == 0)
						{
							BoingWork.Params.InstanceData instanceData = array[i, j, k];
							Gizmos.color = new Color(1f, 1f, 1f, 1f);
							Gizmos.matrix = Matrix4x4.TRS(instanceData.PositionSpring.Value, instanceData.RotationSpring.ValueQuat, Vector3.one);
							Gizmos.DrawCube(Vector3.zero, Mathf.Min(0.1f, 0.5f * this.CellSize) * Vector3.one);
							Gizmos.matrix = Matrix4x4.identity;
						}
						Gizmos.color = new Color(1f, 0.5f, 0.2f, 1f);
						Gizmos.DrawWireCube(vector2, this.CellSize * Vector3.one);
					}
				}
			}
			BoingReactorField.FalloffModeEnum falloffMode = this.FalloffMode;
			if (falloffMode != BoingReactorField.FalloffModeEnum.Circle)
			{
				if (falloffMode == BoingReactorField.FalloffModeEnum.Square)
				{
					Vector3 vector3 = this.CellSize * this.FalloffRatio * new Vector3((float)this.CellsX, (float)this.CellsY, (float)this.CellsZ);
					Gizmos.color = new Color(1f, 1f, 0.2f, 0.5f);
					Gizmos.DrawWireCube(vector, vector3);
				}
			}
			else
			{
				float num5 = (float)Mathf.Max(this.CellsX, Mathf.Max(this.CellsY, this.CellsZ));
				Gizmos.color = new Color(1f, 1f, 0.2f, 0.5f);
				Gizmos.matrix = Matrix4x4.Translate(vector) * Matrix4x4.Scale(new Vector3((float)this.CellsX, (float)this.CellsY, (float)this.CellsZ) / num5);
				Gizmos.DrawWireSphere(Vector3.zero, 0.5f * this.CellSize * num5 * this.FalloffRatio);
				Gizmos.matrix = Matrix4x4.identity;
			}
			if (drawEffectors && this.Effectors != null)
			{
				foreach (BoingEffector boingEffector in this.Effectors)
				{
					if (!(boingEffector == null))
					{
						boingEffector.OnDrawGizmosSelected();
					}
				}
			}
		}

		private Vector3 GetGridCenter()
		{
			BoingReactorField.CellMoveModeEnum cellMoveMode = this.CellMoveMode;
			if (cellMoveMode == BoingReactorField.CellMoveModeEnum.Follow)
			{
				return base.transform.position;
			}
			if (cellMoveMode != BoingReactorField.CellMoveModeEnum.WrapAround)
			{
				return base.transform.position;
			}
			return this.QuantizeNorm(base.transform.position) * this.CellSize;
		}

		private Vector3 QuantizeNorm(Vector3 p)
		{
			return new Vector3(Mathf.Round(p.x / this.CellSize), Mathf.Round(p.y / this.CellSize), Mathf.Round(p.z / this.CellSize));
		}

		private Vector3 GetCellCenterOffset(int x, int y, int z)
		{
			return this.CellSize * (-0.5f * (new Vector3((float)this.CellsX, (float)this.CellsY, (float)this.CellsZ) - Vector3.one) + new Vector3((float)x, (float)y, (float)z));
		}

		private void ResolveCellIndex(int x, int y, int z, int baseMult, out int resX, out int resY, out int resZ)
		{
			resX = MathUtil.Modulo(x + baseMult * this.m_iCellBaseX, this.CellsX);
			resY = MathUtil.Modulo(y + baseMult * this.m_iCellBaseY, this.CellsY);
			resZ = MathUtil.Modulo(z + baseMult * this.m_iCellBaseZ, this.CellsZ);
		}

		private static BoingReactorField.ShaderPropertyIdSet s_shaderPropertyId;

		private BoingReactorField.FieldParams m_fieldParams;

		public BoingReactorField.HardwareModeEnum HardwareMode = BoingReactorField.HardwareModeEnum.GPU;

		private BoingReactorField.HardwareModeEnum m_hardwareMode;

		public BoingReactorField.CellMoveModeEnum CellMoveMode = BoingReactorField.CellMoveModeEnum.WrapAround;

		private BoingReactorField.CellMoveModeEnum m_cellMoveMode;

		[Range(0.1f, 10f)]
		public float CellSize = 1f;

		public int CellsX = 8;

		public int CellsY = 1;

		public int CellsZ = 8;

		private int m_cellsX = -1;

		private int m_cellsY = -1;

		private int m_cellsZ = -1;

		private int m_iCellBaseX;

		private int m_iCellBaseY;

		private int m_iCellBaseZ;

		public BoingReactorField.FalloffModeEnum FalloffMode = BoingReactorField.FalloffModeEnum.Square;

		[Range(0f, 1f)]
		public float FalloffRatio = 0.7f;

		public BoingReactorField.FalloffDimensionsEnum FalloffDimensions = BoingReactorField.FalloffDimensionsEnum.XZ;

		public BoingEffector[] Effectors = new BoingEffector[1];

		private int m_numEffectors = -1;

		private Aabb m_bounds;

		public bool TwoDDistanceCheck;

		public bool TwoDPositionInfluence;

		public bool TwoDRotationInfluence;

		public bool EnablePositionEffect = true;

		public bool EnableRotationEffect = true;

		public bool GlobalReactionUpVector;

		public BoingWork.Params Params;

		public SharedBoingParams SharedParams;

		public bool EnablePropagation;

		[Range(0f, 1f)]
		public float PositionPropagation = 1f;

		[Range(0f, 1f)]
		public float RotationPropagation = 1f;

		[Range(1f, 3f)]
		public int PropagationDepth = 1;

		public bool AnchorPropagationAtBorder;

		private static readonly float kPropagationFactor = 600f;

		private BoingWork.Params.InstanceData[,,] m_aCpuCell;

		private ComputeShader m_shader;

		private ComputeBuffer m_effectorIndexBuffer;

		private ComputeBuffer m_reactorParamsBuffer;

		private ComputeBuffer m_fieldParamsBuffer;

		private ComputeBuffer m_cellsBuffer;

		private int m_gpuResourceSetId = -1;

		private static BoingReactorField.ComputeKernelId s_computeKernelId;

		private bool m_init;

		private Vector3 m_gridCenter;

		private Vector3 m_qPrevGridCenterNorm;

		private static Vector3[] s_aCellOffset = new Vector3[8];

		private bool m_cellBufferNeedsReset;

		private static float[] s_aSqrtInv = new float[]
		{
			0f, 1f, 0.70711f, 0.57735f, 0.5f, 0.44721f, 0.40825f, 0.37796f, 0.35355f, 0.33333f,
			0.31623f, 0.30151f, 0.28868f, 0.27735f, 0.26726f, 0.2582f, 0.25f, 0.24254f, 0.2357f, 0.22942f,
			0.22361f, 0.21822f, 0.2132f, 0.20851f, 0.20412f, 0.2f, 0.19612f, 0.19245f
		};

		private BoingWork.Params[] s_aReactorParams = new BoingWork.Params[1];

		public enum HardwareModeEnum
		{
			CPU,
			GPU
		}

		public enum CellMoveModeEnum
		{
			Follow,
			WrapAround
		}

		public enum FalloffModeEnum
		{
			None,
			Circle,
			Square
		}

		public enum FalloffDimensionsEnum
		{
			XYZ,
			XY,
			XZ,
			YZ
		}

		public class ShaderPropertyIdSet
		{
			public ShaderPropertyIdSet()
			{
				this.MoveParams = Shader.PropertyToID("moveParams");
				this.WrapParams = Shader.PropertyToID("wrapParams");
				this.Effectors = Shader.PropertyToID("aEffector");
				this.EffectorIndices = Shader.PropertyToID("aEffectorIndex");
				this.ReactorParams = Shader.PropertyToID("reactorParams");
				this.ComputeFieldParams = Shader.PropertyToID("fieldParams");
				this.ComputeCells = Shader.PropertyToID("aCell");
				this.RenderFieldParams = Shader.PropertyToID("aBoingFieldParams");
				this.RenderCells = Shader.PropertyToID("aBoingFieldCell");
				this.PositionSampleMultiplier = Shader.PropertyToID("positionSampleMultiplier");
				this.RotationSampleMultiplier = Shader.PropertyToID("rotationSampleMultiplier");
				this.PropagationParams = Shader.PropertyToID("propagationParams");
			}

			public int MoveParams;

			public int WrapParams;

			public int Effectors;

			public int EffectorIndices;

			public int ReactorParams;

			public int ComputeFieldParams;

			public int ComputeCells;

			public int RenderFieldParams;

			public int RenderCells;

			public int PositionSampleMultiplier;

			public int RotationSampleMultiplier;

			public int PropagationParams;
		}

		private struct FieldParams
		{
			private void SuppressWarnings()
			{
				this.m_padding0 = 0;
				this.m_padding1 = 0;
				this.m_padding2 = 0f;
				this.m_padding4 = 0f;
				this.m_padding5 = 0f;
				this.m_padding0 = this.m_padding1;
				this.m_padding1 = (int)this.m_padding2;
				this.m_padding2 = this.m_padding3;
				this.m_padding3 = this.m_padding4;
				this.m_padding4 = this.m_padding5;
			}

			public static readonly int Stride = 112;

			public int CellsX;

			public int CellsY;

			public int CellsZ;

			public int NumEffectors;

			public int iCellBaseX;

			public int iCellBaseY;

			public int iCellBaseZ;

			public int m_padding0;

			public int FalloffMode;

			public int FalloffDimensions;

			public int PropagationDepth;

			public int m_padding1;

			public Vector3 GridCenter;

			private float m_padding3;

			public Vector3 UpWs;

			private float m_padding2;

			public Vector3 FieldPosition;

			public float m_padding4;

			public float FalloffRatio;

			public float CellSize;

			public float DeltaTime;

			private float m_padding5;
		}

		private class ComputeKernelId
		{
			public int InitKernel;

			public int MoveKernel;

			public int WrapXKernel;

			public int WrapYKernel;

			public int WrapZKernel;

			public int ExecuteKernel;
		}
	}
}
