using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FitnessAnalytisHub.UI.WPF.ViewModels;

namespace FitnessAnalytisHub.UI.WPF.Views
{
    /// <summary>
    /// Interaction logic for AthleteProfileView.xaml
    /// </summary>
    public partial class AthleteProfileView : UserControl
    {
        public AthleteProfileView()
        {
            InitializeComponent();
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is AthleteProfileViewModel viewModel && e.AddedItems.Count > 0)
            {
                viewModel.SelectAthleteCommand.Execute(e.AddedItems[0]);
            }
        }
    }
}
