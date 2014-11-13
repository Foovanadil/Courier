using System.Windows;
using Courier.ViewModels.Rx;

namespace Courier.Example.Rx
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			NewVM.DataContext = new BloodPressureSimulationVm();
		}
	}
}
