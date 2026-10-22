using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using HR.Models;

namespace HR.Controls.AddEmployees
{
    public partial class SelfAssessment : UserControl, IEmployeeSection
    {
        private readonly SelfAssessmentRecord _record = new();
        private List<Slider>? _sliders;
        private bool _loading;

        public string SectionTitle => "خودارزیابی";

        public SelfAssessment()
        {
            InitializeComponent();

            _sliders = new List<Slider>
            {
                SliderDiscipline, SliderSpeed, SliderEffort, SliderNegotiation,
                SliderPatience, SliderCreativity, SliderPunctuality,
                SliderTeamwork, SliderAccuracy, SliderPlanning
            };

            UpdateAverage();
        }

        private void AssessmentSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_loading) return;
            UpdateAverage();
        }

        private void UpdateAverage()
        {
            if (_sliders == null) return;

            double sum = 0;
            foreach (var slider in _sliders)
                sum += slider.Value;

            var average = sum / _sliders.Count;

            if (AverageScoreText != null)
                AverageScoreText.Text = average.ToString("0");

            AverageScoreText.Foreground = average switch
            {
                >= 80 => new System.Windows.Media.SolidColorBrush(
                            (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#34D399")),
                >= 50 => new System.Windows.Media.SolidColorBrush(
                            (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FBBF24")),
                _ => new System.Windows.Media.SolidColorBrush(
                            (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FB7185"))
            };
        }

        // ==================== IEmployeeSection ====================

        public void LoadFrom(EmployeeProfile profile)
        {
            if (_sliders == null) return;

            _loading = true;
            _record.Discipline = profile.SelfAssessment.Discipline;
            _record.Speed = profile.SelfAssessment.Speed;
            _record.Effort = profile.SelfAssessment.Effort;
            _record.Negotiation = profile.SelfAssessment.Negotiation;
            _record.Patience = profile.SelfAssessment.Patience;
            _record.Creativity = profile.SelfAssessment.Creativity;
            _record.Punctuality = profile.SelfAssessment.Punctuality;
            _record.Teamwork = profile.SelfAssessment.Teamwork;
            _record.Accuracy = profile.SelfAssessment.Accuracy;
            _record.Planning = profile.SelfAssessment.Planning;

            SliderDiscipline.Value = _record.Discipline;
            SliderSpeed.Value = _record.Speed;
            SliderEffort.Value = _record.Effort;
            SliderNegotiation.Value = _record.Negotiation;
            SliderPatience.Value = _record.Patience;
            SliderCreativity.Value = _record.Creativity;
            SliderPunctuality.Value = _record.Punctuality;
            SliderTeamwork.Value = _record.Teamwork;
            SliderAccuracy.Value = _record.Accuracy;
            SliderPlanning.Value = _record.Planning;
            _loading = false;

            UpdateAverage();
        }

        public void SaveTo(EmployeeProfile profile)
        {
            if (_sliders == null) return;

            var target = profile.SelfAssessment;

            target.Discipline = (int)SliderDiscipline.Value;
            target.Speed = (int)SliderSpeed.Value;
            target.Effort = (int)SliderEffort.Value;
            target.Negotiation = (int)SliderNegotiation.Value;
            target.Patience = (int)SliderPatience.Value;
            target.Creativity = (int)SliderCreativity.Value;
            target.Punctuality = (int)SliderPunctuality.Value;
            target.Teamwork = (int)SliderTeamwork.Value;
            target.Accuracy = (int)SliderAccuracy.Value;
            target.Planning = (int)SliderPlanning.Value;
        }

        public bool Validate(out string message)
        {
            message = string.Empty;
            return true;
        }
    }
}