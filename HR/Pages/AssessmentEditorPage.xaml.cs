using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using HR.Services.Assessments;

namespace HR.Pages
{
    public partial class AssessmentEditorPage : Page
    {
        private readonly ObservableCollection<QuestionRecord> _questions = new();
        private readonly AssessmentService _assessments = new();

        public AssessmentEditorPage()
        {
            InitializeComponent();
            QuestionList.ItemsSource = _questions;
            QuestionList.DisplayMemberPath = nameof(QuestionRecord.Text);
            Unloaded += (_, _) => _assessments.Dispose();
        }

        private void AddQuestion_Click(object sender, RoutedEventArgs e)
        {
            var text = QuestionBox.Text.Trim();
            if (text.Length == 0)
            {
                StatusText.Text = "متن سؤال را بنویسید.";
                return;
            }

            var type = (TypeBox.SelectedItem as ComboBoxItem)?.Tag as string ?? "Likert";
            var question = new QuestionRecord
            {
                Code = "q" + (_questions.Count + 1),
                SortOrder = _questions.Count + 1,
                QuestionType = type,
                Text = text,
                IsRequired = true,
                CountsTowardTotal = type != "TextLong",
                Weight = 1
            };

            if (type == "Likert")
            {
                question.MinValue = 1;
                question.MaxValue = 5;
                question.MinLabel = "کاملاً مخالفم";
                question.MaxLabel = "کاملاً موافقم";
            }
            else if (type == "Scale")
            {
                question.MinValue = 0;
                question.MaxValue = 10;
                question.MinLabel = "کم";
                question.MaxLabel = "زیاد";
            }
            else if (type == "SingleChoice")
            {
                var order = 1;
                foreach (var line in OptionsBox.Text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    var parts = line.Split('|');
                    if (!double.TryParse(parts.ElementAtOrDefault(1), out var score))
                    {
                        StatusText.Text = "هر گزینه باید به شکل «متن|نمره» باشد.";
                        return;
                    }

                    question.Options.Add(new OptionRecord
                    {
                        Code = ((char)('a' + order - 1)).ToString(),
                        Text = parts[0].Trim(),
                        Score = score,
                        SortOrder = order
                    });
                    order++;
                }

                if (question.Options.Count == 0)
                {
                    StatusText.Text = "برای سؤال چندگزینه‌ای حداقل یک گزینه لازم است.";
                    return;
                }
            }

            _questions.Add(question);
            QuestionBox.Clear();
            OptionsBox.Clear();
            StatusText.Text = "سؤال اضافه شد.";
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saved = await _assessments.CreateAsync(new AssessmentRecord
                {
                    Title = TitleBox.Text,
                    Category = CategoryBox.Text,
                    Description = DescriptionBox.Text,
                    ScoringMode = "weightedSum",
                    NormalizeTo100 = true,
                    EstimatedMinutes = 10,
                    QuestionsPerPage = 2,
                    Questions = _questions.ToList()
                });
                StatusText.Text = "آزمون «" + saved.Title + "» ذخیره شد.";
                if (NavigationService?.CanGoBack == true)
                    NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                StatusText.Text = ex.Message;
            }
        }
    }
}
