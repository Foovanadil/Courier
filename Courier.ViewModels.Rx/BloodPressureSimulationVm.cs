using System;
using System.Reactive.Linq;
using Courier.Rx;
using System.Windows.Threading;

namespace Courier.ViewModels.Rx
{
	public class BloodPressureSimulationVm : BaseViewModel
	{

		public String AlertPressure { get; set; }
		public String CurrentPressure { get; set; }
		private Random rand = new Random();

		public BloodPressureSimulationVm()
		{
			Mediator.RegisterForMessage<BloodPressure>("BloodPressureMessage", OnPressureChanged);

			IObservable<BloodPressure> observable = from msg in Mediator.RegisterForMessage<BloodPressure>("BloodPressureMessage")
											 where (msg.Systolic < 90 || msg.Systolic > 119)
											 && (msg.Diastolic < 60 || msg.Diastolic > 79)
											 select msg;

			observable.Subscribe(OnAlertReceived);

			StartMessagePumpSimulation();
		}

		private void OnPressureChanged(BloodPressure obj)
		{
			CurrentPressure = obj.ToString();
			PropertyChange("CurrentPressure");
		}

		private void OnAlertReceived(object bloodPressure)
		{
			var pressure = bloodPressure as BloodPressure;
			if (pressure != null)
			{
				AlertPressure = pressure.ToString();
				PropertyChange("AlertPressure");
			}
		}


		private void StartMessagePumpSimulation()
		{
			var timer = new DispatcherTimer
			{
				Interval = TimeSpan.FromSeconds(1)
			};
			timer.Tick += BroadcastRandomBloodPressureMessage;
			timer.Start();
		}

		private void BroadcastRandomBloodPressureMessage(object sender, EventArgs e)
		{
			Int32 randomSystolic = rand.Next(75, 145);
			Int32 randomDiastolic = rand.Next(40, 100);

			var pressure = new BloodPressure(randomSystolic, randomDiastolic);

			Mediator.BroadcastMessage("BloodPressureMessage", pressure);
		}
	}
}
