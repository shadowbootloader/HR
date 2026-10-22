using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HR.Models;

namespace HR.Pages
{
    public partial class AssessmentTestShellPage : Page
    {
        private readonly AssessmentDefinition _def;
        private readonly AssessmentSession _session;
        private readonly List<QuestionDefinition> _questions;
        private readonly List<QuestionSlot> _slots = new();

        private int PageSize => _def.QuestionsPerPage <= 0 ? 1 : _def.QuestionsPerPage;
        private int PageCount => Math.Max(1, (int)Math.Ceiling(_questions.Count / (double)PageSize));

        public event EventHandler<AssessmentSession> Submitted;

        public AssessmentTestShellPage(AssessmentDefinition definition, AssessmentSession session)
        {
            InitializeComponent();
            _def = definition ?? throw new ArgumentNullException(nameof(definition));
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _questions = (_def.Questions ?? new List<QuestionDefinition>()).OrderBy(q => q.Order).ToList();
            TitleText.Text = _def.Title;

            if (_session.Status == AssessmentStatus.NotStarted)
            {
                _session.Status = AssessmentStatus.InProgress;
                _session.StartedAt = DateTime.Now;
            }

            Loaded += (_, __) => ShowPage(_session.CurrentIndex);
        }

        private void ShowPage(int pageIndex)
        {
            if (pageIndex < 0) pageIndex = 0;
            if (pageIndex >= PageCount) pageIndex = PageCount - 1;
            _session.CurrentIndex = pageIndex;

            SlideHost.Children.Clear();
            _slots.Clear();

            int start = pageIndex * PageSize;
            var slice = _questions.Skip(start).Take(PageSize).ToList();
            for (int i = 0; i < slice.Count; i++)
            {
                var q = slice[i];
                _session.Answers.TryGetValue(q.Id, out var saved);
                var slot = BuildSlot(q, start + i + 1, saved);
                _slots.Add(slot);
                SlideHost.Children.Add(slot.Root);
            }

            bool last = pageIndex >= PageCount - 1;
            BtnPrev.IsEnabled = pageIndex > 0;
            BtnNext.Content = last ? "ثبت پاسخ‌ها" : "بعدی";

            int through = Math.Min(start + slice.Count, _questions.Count);
            int left = Math.Max(0, _questions.Count - through);
            RemainText.Text = _questions.Count == 0
                ? "سوالی نیست"
                : $"سوال {through} از {_questions.Count}  ·  {left} مانده";
            UpdateProgress(through);
        }

        private void UpdateProgress(int doneCount)
        {
            ProgressFill.Width = 0;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var track = ProgressFill.Parent as FrameworkElement;
                if (track == null || track.ActualWidth <= 0 || _questions.Count <= 0) return;
                ProgressFill.Width = track.ActualWidth * Math.Min(1.0, doneCount / (double)_questions.Count);
            }));
        }

        private bool SaveCurrentPage()
        {
            var ok = true;
            foreach (var slot in _slots)
            {
                var answer = slot.Read();
                if (slot.Question.Required && !slot.HasValue(answer))
                {
                    slot.ShowError("این سوال اجباری است");
                    ok = false;
                }
                else slot.ShowError(null);

                if (!string.IsNullOrEmpty(answer.QuestionId))
                    _session.Answers[answer.QuestionId] = answer;
            }
            return ok;
        }

        private void BtnPrev_Click(object sender, RoutedEventArgs e)
        {
            SaveCurrentPage();
            ShowPage(_session.CurrentIndex - 1);
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            if (!SaveCurrentPage()) return;
            if (_session.CurrentIndex >= PageCount - 1)
            {
                _session.Status = AssessmentStatus.Completed;
                _session.CompletedAt = DateTime.Now;

                // موتور امتیاز — قوانین داخل خود تعریف آزمون
                var result = AssessmentScoringEngine.Compute(_def, _session);

                Submitted?.Invoke(this, _session);

                MessageBox.Show(
                    result.Summary + Environment.NewLine + Environment.NewLine +
                    "امتیاز کل: " + result.TotalScore.ToString("0.#") + Environment.NewLine +
                    "سطح: " + result.LevelLabel,
                    "نتیجه آزمون",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                if (NavigationService?.CanGoBack == true) NavigationService.GoBack();
                return;
            }
            ShowPage(_session.CurrentIndex + 1);
        }

        private QuestionSlot BuildSlot(QuestionDefinition q, int number, AnswerValue saved)
        {
            var root = new Border
            {
                CornerRadius = new CornerRadius(16),
                Background = Brush("#0C1830"),
                BorderBrush = Brush("#1A3A5C"),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(16),
                Margin = new Thickness(0, 0, 0, 12)
            };
            var stack = new StackPanel();
            root.Child = stack;

            var head = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
            head.Children.Add(new Border
            {
                Width = 26,
                Height = 26,
                CornerRadius = new CornerRadius(13),
                Background = Brush("#0E3A4A"),
                Margin = new Thickness(0, 0, 8, 0),
                Child = new TextBlock
                {
                    Text = number.ToString(),
                    Foreground = Brush("#22D3EE"),
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            });
            head.Children.Add(new TextBlock
            {
                Text = q.Text,
                Foreground = Brush("#E8F4FF"),
                FontSize = 15,
                FontWeight = FontWeights.SemiBold,
                TextWrapping = TextWrapping.Wrap,
                VerticalAlignment = VerticalAlignment.Center
            });
            stack.Children.Add(head);

            if (!string.IsNullOrWhiteSpace(q.HelpText))
                stack.Children.Add(new TextBlock
                {
                    Text = q.HelpText,
                    Foreground = Brush("#5D7694"),
                    FontSize = 12,
                    Margin = new Thickness(34, 0, 0, 8),
                    TextWrapping = TextWrapping.Wrap
                });

            var error = new TextBlock
            {
                Foreground = Brush("#FB7185"),
                FontSize = 12,
                Margin = new Thickness(34, 8, 0, 0),
                Visibility = Visibility.Collapsed
            };

            Func<AnswerValue> read;
            Func<AnswerValue, bool> has;

            switch (q.Type)
            {
                case QuestionType.SingleChoice:
                case QuestionType.MultiChoice:
                    bool multi = q.Type == QuestionType.MultiChoice;
                    var selected = new HashSet<string>(saved?.SelectedOptionIds ?? new List<string>());
                    var boxes = new List<CheckBox>();
                    foreach (var opt in q.Options ?? new List<OptionDefinition>())
                    {
                        var cb = new CheckBox
                        {
                            Content = opt.Text,
                            Tag = opt.Id,
                            Foreground = Brush("#E8F4FF"),
                            FontSize = 14,
                            Margin = new Thickness(8, 4, 8, 4),
                            IsChecked = selected.Contains(opt.Id)
                        };
                        if (!multi)
                        {
                            cb.Checked += (_, __) =>
                            {
                                foreach (var other in boxes)
                                    if (!ReferenceEquals(other, cb)) other.IsChecked = false;
                            };
                        }
                        boxes.Add(cb);
                        stack.Children.Add(cb);
                    }
                    read = () => new AnswerValue
                    {
                        QuestionId = q.Id,
                        SelectedOptionIds = boxes.Where(b => b.IsChecked == true).Select(b => (string)b.Tag).ToList()
                    };
                    has = a => a.SelectedOptionIds != null && a.SelectedOptionIds.Count > 0;
                    break;

                case QuestionType.Likert:
                    int min = q.Min ?? 1;
                    int max = q.Max ?? 5;
                    int? picked = saved?.Numeric.HasValue == true ? (int)saved.Numeric.Value : (int?)null;
                    var row = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };
                    var likertButtons = new List<RadioButton>();
                    for (int v = min; v <= max; v++)
                    {
                        var rb = new RadioButton
                        {
                            Content = v.ToString(),
                            GroupName = "likert-" + q.Id,
                            Tag = v,
                            Foreground = Brush("#E8F4FF"),
                            Margin = new Thickness(8, 0, 8, 0),
                            IsChecked = picked == v
                        };
                        likertButtons.Add(rb);
                        row.Children.Add(rb);
                    }
                    stack.Children.Add(row);
                    read = () =>
                    {
                        var on = likertButtons.FirstOrDefault(b => b.IsChecked == true);
                        return new AnswerValue { QuestionId = q.Id, Numeric = on == null ? (double?)null : Convert.ToDouble(on.Tag) };
                    };
                    has = a => a.Numeric.HasValue;
                    break;

                case QuestionType.Scale:
                    var slider = new Slider
                    {
                        Minimum = q.Min ?? 0,
                        Maximum = q.Max ?? 10,
                        TickFrequency = 1,
                        IsSnapToTickEnabled = true,
                        Margin = new Thickness(8, 8, 8, 0),
                        Value = saved?.Numeric ?? (q.Min ?? 0)
                    };
                    bool touched = saved?.Numeric != null;
                    var valueText = new TextBlock
                    {
                        Text = ((int)slider.Value).ToString(),
                        Foreground = Brush("#22D3EE"),
                        FontSize = 20,
                        FontWeight = FontWeights.Bold,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };
                    slider.ValueChanged += (_, ev) =>
                    {
                        touched = true;
                        valueText.Text = ((int)ev.NewValue).ToString();
                    };
                    stack.Children.Add(valueText);
                    stack.Children.Add(slider);
                    read = () => new AnswerValue { QuestionId = q.Id, Numeric = touched ? slider.Value : (double?)null };
                    has = a => a.Numeric.HasValue;
                    break;

                default:
                    bool longText = q.Type == QuestionType.TextLong;
                    var box = new TextBox
                    {
                        Text = saved?.Text ?? "",
                        AcceptsReturn = longText,
                        TextWrapping = TextWrapping.Wrap,
                        MinHeight = longText ? 110 : 36,
                        Padding = new Thickness(8),
                        Background = Brush("#081420"),
                        Foreground = Brush("#E8F4FF"),
                        BorderBrush = Brush("#16304C"),
                        CaretBrush = Brush("#22D3EE")
                    };
                    stack.Children.Add(box);
                    read = () => new AnswerValue { QuestionId = q.Id, Text = box.Text?.Trim() };
                    has = a => !string.IsNullOrWhiteSpace(a.Text);
                    break;
            }

            stack.Children.Add(error);
            return new QuestionSlot(q, root, error, read, has);
        }

        private static SolidColorBrush Brush(string hex)
        {
            var b = (SolidColorBrush)new BrushConverter().ConvertFrom(hex);
            b.Freeze();
            return b;
        }

        private sealed class QuestionSlot
        {
            public QuestionDefinition Question { get; }
            public Border Root { get; }
            private readonly TextBlock _error;
            private readonly Func<AnswerValue> _read;
            private readonly Func<AnswerValue, bool> _has;

            public QuestionSlot(QuestionDefinition question, Border root, TextBlock error, Func<AnswerValue> read, Func<AnswerValue, bool> has)
            {
                Question = question;
                Root = root;
                _error = error;
                _read = read;
                _has = has;
            }

            public AnswerValue Read() => _read();
            public bool HasValue(AnswerValue answer) => _has(answer);
            public void ShowError(string message)
            {
                _error.Text = message;
                _error.Visibility = string.IsNullOrEmpty(message) ? Visibility.Collapsed : Visibility.Visible;
            }
        }
    }
}