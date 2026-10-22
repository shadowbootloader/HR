using System;
using System.Windows;
using System.Windows.Controls;

namespace HR.Controls
{
    public partial class SkillItem : UserControl
    {
        #region Dependency Properties

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                nameof(Title),
                typeof(string),
                typeof(SkillItem),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty PercentageProperty =
            DependencyProperty.Register(
                nameof(Percentage),
                typeof(double),
                typeof(SkillItem),
                new PropertyMetadata(0.0, OnPercentageChanged));

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(
                nameof(Icon),
                typeof(string),
                typeof(SkillItem),
                new PropertyMetadata(string.Empty));

        #endregion


        #region Properties

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public double Percentage
        {
            get => (double)GetValue(PercentageProperty);
            set => SetValue(PercentageProperty, value);
        }

        public string Icon
        {
            get => (string)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        #endregion


        public SkillItem()
        {
            InitializeComponent();

            Loaded += SkillItem_Loaded;
        }


        private void SkillItem_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            UpdatePercentage();
        }


        private static void OnPercentageChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            if (d is SkillItem item)
            {
                item.UpdatePercentage();
            }
        }


        private void UpdatePercentage()
        {
            if (PercentageText == null ||
                SkillProgressBar == null)
            {
                return;
            }

            double value = Percentage;

            // محدود کردن مقدار بین 0 و 100
            value = Math.Max(0, Math.Min(100, value));

            // متن درصد
            PercentageText.Text = $"{value:0}%";

            // مقدار ProgressBar
            SkillProgressBar.Value = value;
        }
    }
}