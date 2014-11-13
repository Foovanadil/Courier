using System;

namespace Courier.ViewModels.Rx
{
	public class BloodPressure
	{
		public Int32 Systolic { get; set; }
		public Int32 Diastolic { get; set; }

		public BloodPressure(Int32 systolic, Int32 diastolic)
		{
			Systolic = systolic;
			Diastolic = diastolic;
		}

		public override string ToString()
		{
			return String.Format("{0} / {1}", Systolic, Diastolic);
		}
	}
}