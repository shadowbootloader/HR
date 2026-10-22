using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HR.Controls.AddEmployees
{
    /// <summary>
    /// Interaction logic for SelfRate.xaml
    /// </summary>
    public partial class SelfAssessment : UserControl
    {
        public SelfAssessment()
        {
            InitializeComponent();
        }

        private void AssessmentSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (AverageScoreText == null) return;

            double avg =
                (SliderDiscipline.Value +
                 SliderSpeed.Value +
                 SliderEffort.Value +
                 SliderNegotiation.Value +
                 SliderPatience.Value +
                 SliderCreativity.Value +
                 SliderPunctuality.Value +
                 SliderTeamwork.Value +
                 SliderAccuracy.Value +
                 SliderPlanning.Value) / 10.0;

            AverageScoreText.Text = avg.ToString("0");
        }
    }
}
