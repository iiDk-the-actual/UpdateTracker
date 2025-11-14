using System;

namespace PerformanceSystems
{
	public interface ITimeSlice
	{
		void SliceUpdate();

		void SliceUpdateAlways(float deltaTime);

		void SliceUpdate(float deltaTime);
	}
}
