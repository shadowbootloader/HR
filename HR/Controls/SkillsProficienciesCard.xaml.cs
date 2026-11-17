using System.Windows;
using System.Windows.Controls;

namespace HR.Controls
{
    public partial class SkillsProficienciesCard : UserControl
    {
        public static readonly DependencyProperty SkillsProperty =
            DependencyProperty.Register(nameof(Skills), typeof(object), typeof(SkillsProficienciesCard));

        public object Skills
        {
            get => GetValue(SkillsProperty);
            set => SetValue(SkillsProperty, value);
        }

        public SkillsProficienciesCard()
        {
            InitializeComponent();
        }

        public void ShowSkills(IReadOnlyList<(string Title, int Percent)> skills)
        {
            SkillsList.Children.Clear();
            if (skills.Count == 0)
            {
                SkillsList.Children.Add(new TextBlock
                {
                    Text = "مهارتی با سطح استاندارد ثبت نشده است.",
                    Foreground = System.Windows.Media.Brushes.White,
                    Margin = new Thickness(8)
                });
                return;
            }

            foreach (var skill in skills)
            {
                SkillsList.Children.Add(new SkillItem
                {
                    Title = skill.Title,
                    Percentage = skill.Percent,
                    Icon = "Star"
                });
            }
        }
    }
}