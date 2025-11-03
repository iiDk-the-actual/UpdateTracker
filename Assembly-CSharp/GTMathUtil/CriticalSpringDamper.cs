using System;

namespace GTMathUtil
{
	internal class CriticalSpringDamper
	{
		private static float halflife_to_damping(float halflife, float eps = 1E-05f)
		{
			return 2.7725887f / (halflife + eps);
		}

		private static float fast_negexp(float x)
		{
			return 1f / (1f + x + 0.48f * x * x + 0.235f * x * x * x);
		}

		public float Update(float dt)
		{
			float num = CriticalSpringDamper.halflife_to_damping(this.halfLife, 1E-05f) / 2f;
			float num2 = this.x - this.xGoal;
			float num3 = this.curVel + num2 * num;
			float num4 = CriticalSpringDamper.fast_negexp(num * dt);
			this.x = num4 * (num2 + num3 * dt) + this.xGoal;
			this.curVel = num4 * (this.curVel - num3 * num * dt);
			return this.x;
		}

		public float x;

		public float xGoal;

		public float halfLife = 0.1f;

		private float curVel;
	}
}
