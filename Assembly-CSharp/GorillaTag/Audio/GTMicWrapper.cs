using System;
using Photon.Voice;
using Photon.Voice.Unity;
using UnityEngine;

namespace GorillaTag.Audio
{
	public class GTMicWrapper : MicWrapper
	{
		public GTMicWrapper(string device, int suggestedFrequency, bool allowPitchAdjustment, float pitchAdjustment, bool allowVolumeAdjustment, float volumeAdjustment, Photon.Voice.ILogger logger)
			: base(device, suggestedFrequency, logger)
		{
			this.UpdatePitchAdjustment(allowPitchAdjustment, pitchAdjustment);
			this.UpdateVolumeAdjustment(allowVolumeAdjustment, volumeAdjustment);
		}

		public void UpdateWrapper(bool allowPitchAdjustment, float pitchAdjustment, bool allowVolumeAdjustment, float volumeAdjustment)
		{
			this.UpdatePitchAdjustment(allowPitchAdjustment, pitchAdjustment);
			this.UpdateVolumeAdjustment(allowVolumeAdjustment, volumeAdjustment);
		}

		public void UpdatePitchAdjustment(bool allow, float pitchAdjustment)
		{
			this._allowPitchAdjustment = allow;
			this._pitchAdjustment = pitchAdjustment;
		}

		public void UpdateVolumeAdjustment(bool allow, float volumeAdjustment)
		{
			this._allowVolumeAdjustment = allow;
			this._volumeAdjustment = volumeAdjustment;
		}

		public override bool Read(float[] buffer)
		{
			if (base.Error != null)
			{
				return false;
			}
			int position = UnityMicrophone.GetPosition(this.device);
			if (position < this.micPrevPos)
			{
				this.micLoopCnt++;
			}
			this.micPrevPos = position;
			int num = this.micLoopCnt * this.mic.samples + position;
			if (this.mic.channels == 0)
			{
				base.Error = "Number of channels is 0 in Read()";
				this.logger.LogError("[PV] MicWrapper: " + base.Error, Array.Empty<object>());
				return false;
			}
			int num2 = buffer.Length / this.mic.channels;
			int num3 = this.readAbsPos + num2;
			if (num3 < num)
			{
				this.mic.GetData(buffer, this.readAbsPos % this.mic.samples);
				this.readAbsPos = num3;
				float num4 = Mathf.Clamp(this._pitchAdjustment, 0.5f, 2f);
				if (this._allowPitchAdjustment && !Mathf.Approximately(num4, 1f))
				{
					this.PitchShift(num4, (long)num2, (float)base.SamplingRate, buffer);
				}
				if (this._allowVolumeAdjustment && !Mathf.Approximately(this._volumeAdjustment, 1f))
				{
					for (int i = 0; i < buffer.Length; i++)
					{
						buffer[i] = Mathf.Clamp(buffer[i] * this._volumeAdjustment, float.MinValue, float.MaxValue);
					}
				}
				return true;
			}
			return false;
		}

		private void PitchShift(float pitchShift, long numSampsToProcess, float sampleRate, float[] indata)
		{
			this.PitchShift(pitchShift, numSampsToProcess, 2048L, 10L, sampleRate, indata);
		}

		public void PitchShift(float pitchShift, long numSampsToProcess, long fftFrameSize, long osamp, float sampleRate, float[] indata)
		{
			long num = fftFrameSize / 2L;
			long num2 = fftFrameSize / osamp;
			double num3 = (double)sampleRate / (double)fftFrameSize;
			double num4 = 6.283185307179586 * (double)num2 / (double)fftFrameSize;
			long num5 = fftFrameSize - num2;
			if (this._gRover == 0L)
			{
				this._gRover = num5;
			}
			for (long num6 = 0L; num6 < numSampsToProcess; num6 += 1L)
			{
				checked
				{
					this.InFifo[(int)((IntPtr)this._gRover)] = indata[(int)((IntPtr)num6)];
					indata[(int)((IntPtr)num6)] = this.OutFifo[(int)((IntPtr)(unchecked(this._gRover - num5)))];
				}
				this._gRover += 1L;
				if (this._gRover >= fftFrameSize)
				{
					this._gRover = num5;
					for (long num7 = 0L; num7 < fftFrameSize; num7 += 1L)
					{
						double num8 = -0.5 * Math.Cos(6.283185307179586 * (double)num7 / (double)fftFrameSize) + 0.5;
						this.FfTworksp[(int)(checked((IntPtr)(unchecked(2L * num7))))] = (float)((double)this.InFifo[(int)(checked((IntPtr)num7))] * num8);
						checked
						{
							this.FfTworksp[(int)((IntPtr)(unchecked(2L * num7 + 1L)))] = 0f;
						}
					}
					this.ShortTimeFourierTransform(this.FfTworksp, fftFrameSize, -1L);
					for (long num7 = 0L; num7 <= num; num7 += 1L)
					{
						double num9;
						double num10;
						checked
						{
							num9 = (double)this.FfTworksp[(int)((IntPtr)(unchecked(2L * num7)))];
							num10 = (double)this.FfTworksp[(int)((IntPtr)(unchecked(2L * num7 + 1L)))];
						}
						double num11 = 2.0 * Math.Sqrt(num9 * num9 + num10 * num10);
						double num12 = Math.Atan2(num10, num9);
						double num13 = num12 - (double)this.LastPhase[(int)(checked((IntPtr)num7))];
						this.LastPhase[(int)(checked((IntPtr)num7))] = (float)num12;
						num13 -= (double)num7 * num4;
						long num14 = (long)(num13 / 3.141592653589793);
						if (num14 >= 0L)
						{
							num14 += num14 & 1L;
						}
						else
						{
							num14 -= num14 & 1L;
						}
						num13 -= 3.141592653589793 * (double)num14;
						num13 = (double)osamp * num13 / 6.283185307179586;
						num13 = (double)num7 * num3 + num13 * num3;
						checked
						{
							this.AnaMagn[(int)((IntPtr)num7)] = (float)num11;
							this.AnaFreq[(int)((IntPtr)num7)] = (float)num13;
						}
					}
					int num15 = 0;
					while ((long)num15 < fftFrameSize)
					{
						this.SynMagn[num15] = 0f;
						this.SynFreq[num15] = 0f;
						num15++;
					}
					for (long num7 = 0L; num7 <= num; num7 += 1L)
					{
						long num16 = (long)((float)num7 * pitchShift);
						if (num16 <= num)
						{
							this.SynMagn[(int)(checked((IntPtr)num16))] += this.AnaMagn[(int)(checked((IntPtr)num7))];
							this.SynFreq[(int)(checked((IntPtr)num16))] = this.AnaFreq[(int)(checked((IntPtr)num7))] * pitchShift;
						}
					}
					for (long num7 = 0L; num7 <= num; num7 += 1L)
					{
						double num11;
						double num13;
						checked
						{
							num11 = (double)this.SynMagn[(int)((IntPtr)num7)];
							num13 = (double)this.SynFreq[(int)((IntPtr)num7)];
						}
						num13 -= (double)num7 * num3;
						num13 /= num3;
						num13 = 6.283185307179586 * num13 / (double)osamp;
						num13 += (double)num7 * num4;
						this.SumPhase[(int)(checked((IntPtr)num7))] += (float)num13;
						checked
						{
							double num12 = (double)this.SumPhase[(int)((IntPtr)num7)];
							this.FfTworksp[(int)((IntPtr)(unchecked(2L * num7)))] = (float)(unchecked(num11 * Math.Cos(num12)));
							this.FfTworksp[(int)((IntPtr)(unchecked(2L * num7 + 1L)))] = (float)(unchecked(num11 * Math.Sin(num12)));
						}
					}
					for (long num7 = fftFrameSize + 2L; num7 < 2L * fftFrameSize; num7 += 1L)
					{
						this.FfTworksp[(int)(checked((IntPtr)num7))] = 0f;
					}
					this.ShortTimeFourierTransform(this.FfTworksp, fftFrameSize, 1L);
					for (long num7 = 0L; num7 < fftFrameSize; num7 += 1L)
					{
						double num8 = -0.5 * Math.Cos(6.283185307179586 * (double)num7 / (double)fftFrameSize) + 0.5;
						this.OutputAccum[(int)(checked((IntPtr)num7))] += (float)(2.0 * num8 * (double)this.FfTworksp[(int)(checked((IntPtr)(unchecked(2L * num7))))] / (double)(num * osamp));
					}
					for (long num7 = 0L; num7 < num2; num7 += 1L)
					{
						checked
						{
							this.OutFifo[(int)((IntPtr)num7)] = this.OutputAccum[(int)((IntPtr)num7)];
						}
					}
					for (long num7 = 0L; num7 < fftFrameSize; num7 += 1L)
					{
						checked
						{
							this.OutputAccum[(int)((IntPtr)num7)] = this.OutputAccum[(int)((IntPtr)(unchecked(num7 + num2)))];
						}
					}
					for (long num7 = 0L; num7 < num5; num7 += 1L)
					{
						checked
						{
							this.InFifo[(int)((IntPtr)num7)] = this.InFifo[(int)((IntPtr)(unchecked(num7 + num2)))];
						}
					}
				}
			}
		}

		public void ShortTimeFourierTransform(float[] fftBuffer, long fftFrameSize, long sign)
		{
			for (long num = 2L; num < 2L * fftFrameSize - 2L; num += 2L)
			{
				long num2 = 2L;
				long num3 = 0L;
				while (num2 < 2L * fftFrameSize)
				{
					if ((num & num2) != 0L)
					{
						num3 += 1L;
					}
					num3 <<= 1;
					num2 <<= 1;
				}
				checked
				{
					if (num < num3)
					{
						float num4 = fftBuffer[(int)((IntPtr)num)];
						fftBuffer[(int)((IntPtr)num)] = fftBuffer[(int)((IntPtr)num3)];
						fftBuffer[(int)((IntPtr)num3)] = num4;
						num4 = fftBuffer[(int)((IntPtr)(unchecked(num + 1L)))];
						fftBuffer[(int)((IntPtr)(unchecked(num + 1L)))] = fftBuffer[(int)((IntPtr)(unchecked(num3 + 1L)))];
						fftBuffer[(int)((IntPtr)(unchecked(num3 + 1L)))] = num4;
					}
				}
			}
			long num5 = (long)(Math.Log((double)fftFrameSize) / Math.Log(2.0) + 0.5);
			long num6 = 0L;
			long num7 = 2L;
			while (num6 < num5)
			{
				num7 <<= 1;
				long num8 = num7 >> 1;
				float num9 = 1f;
				float num10 = 0f;
				float num11 = 3.1415927f / (float)(num8 >> 1);
				float num12 = (float)Math.Cos((double)num11);
				float num13 = (float)((double)sign * Math.Sin((double)num11));
				for (long num3 = 0L; num3 < num8; num3 += 2L)
				{
					float num14;
					for (long num = num3; num < 2L * fftFrameSize; num += num7)
					{
						num14 = fftBuffer[(int)(checked((IntPtr)(unchecked(num + num8))))] * num9 - fftBuffer[(int)(checked((IntPtr)(unchecked(num + num8 + 1L))))] * num10;
						float num15 = fftBuffer[(int)(checked((IntPtr)(unchecked(num + num8))))] * num10 + fftBuffer[(int)(checked((IntPtr)(unchecked(num + num8 + 1L))))] * num9;
						fftBuffer[(int)(checked((IntPtr)(unchecked(num + num8))))] = fftBuffer[(int)(checked((IntPtr)num))] - num14;
						fftBuffer[(int)(checked((IntPtr)(unchecked(num + num8 + 1L))))] = fftBuffer[(int)(checked((IntPtr)(unchecked(num + 1L))))] - num15;
						fftBuffer[(int)(checked((IntPtr)num))] += num14;
						fftBuffer[(int)(checked((IntPtr)(unchecked(num + 1L))))] += num15;
					}
					num14 = num9 * num12 - num10 * num13;
					num10 = num9 * num13 + num10 * num12;
					num9 = num14;
				}
				num6 += 1L;
			}
		}

		private bool _allowPitchAdjustment;

		private float _pitchAdjustment = 1f;

		private bool _allowVolumeAdjustment;

		private float _volumeAdjustment = 1f;

		private static readonly int MaxFrameLength = 16000;

		private readonly float[] InFifo = new float[GTMicWrapper.MaxFrameLength];

		private readonly float[] OutFifo = new float[GTMicWrapper.MaxFrameLength];

		private readonly float[] FfTworksp = new float[2 * GTMicWrapper.MaxFrameLength];

		private readonly float[] LastPhase = new float[GTMicWrapper.MaxFrameLength / 2 + 1];

		private readonly float[] SumPhase = new float[GTMicWrapper.MaxFrameLength / 2 + 1];

		private readonly float[] OutputAccum = new float[2 * GTMicWrapper.MaxFrameLength];

		private readonly float[] AnaFreq = new float[GTMicWrapper.MaxFrameLength];

		private readonly float[] AnaMagn = new float[GTMicWrapper.MaxFrameLength];

		private readonly float[] SynFreq = new float[GTMicWrapper.MaxFrameLength];

		private readonly float[] SynMagn = new float[GTMicWrapper.MaxFrameLength];

		private long _gRover;
	}
}
