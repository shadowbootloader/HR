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
            Loaded += (s, e) => AddSkills();
        }

        private void AddSkills()
        {
            SkillsList.Children.Clear();

            // داده‌های واقعی (می‌تونی از ViewModel بیاری)
            var skills = new[]
            {
                ("مدیریت افراد", 90, "AccountChildOutline"),
                ("توسعه استعداد", 85, "HumanGreetingVariant"),
                ("مدیریت ذینفعان", 88, "MicrophoneMessage"),
                ("مدیریت تغییر", 80, "Strategy"),
                ("منابع انسانی داده‌محور", 75, "FileDocumentEditOutline"),
                ("تفکر استراتژیک", 82, "BullseyeArrow")
            };

            foreach (var skill in skills)
            {
                var item = new SkillItem
                {
                    Title = skill.Item1,
                    Percentage = skill.Item2,
                    Icon = skill.Item3
                };
                SkillsList.Children.Add(item);
            }
        }
    }
}