using System.Windows;
using System.Windows.Controls;
using HR.Models;
using HR.Services.Files;
using Microsoft.Win32;

namespace HR.Controls.AddEmployees
{
    public partial class InterviewInfo : UserControl, IEmployeeSection
    {
        private readonly List<InterviewDraft> _drafts = new();
        private string? _text;
        private string? _audio;

        public InterviewInfo() => InitializeComponent();

        public string SectionTitle => "مصاحبه";

        public void LoadFrom(EmployeeProfile profile) { }

        public void SaveTo(EmployeeProfile profile) { }

        public bool Validate(out string message)
        {
            message = string.Empty;
            return true;
        }

        public async System.Threading.Tasks.Task PersistAsync(Guid employeeId)
        {
            if (_drafts.Count == 0) return;
            var existing = await new InterviewService().ListAsync(employeeId);
            var all = existing.Select(item => new InterviewDraft
            {
                InterviewId = item.InterviewId,
                HeldOn = item.HeldOn,
                Interviewer = item.Interviewer,
                Notes = item.Notes,
                TranscriptPath = item.TranscriptPath,
                AudioPath = item.AudioPath
            }).Concat(_drafts).ToList();
            await new InterviewService().ReplaceAsync(employeeId, all);
            _drafts.Clear();
        }

        private void PickText_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog { Filter = "متن|*.txt;*.docx;*.pdf" };
            if (dialog.ShowDialog() == true)
                _text = FileStorageService.Load().Stage(dialog.FileName, FileKind.Document).RelativePath;
        }

        private void PickAudio_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog { Filter = "صدا|*.mp3;*.wav;*.m4a" };
            if (dialog.ShowDialog() == true)
                _audio = FileStorageService.Load().Stage(dialog.FileName, FileKind.Audio).RelativePath;
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Interviewer.Text))
            {
                ListText.Text = "نام مصاحبه‌کننده را وارد کنید.";
                return;
            }
            _drafts.Add(new InterviewDraft
            {
                HeldOn = HeldOn.SelectedDate ?? DateTime.Today,
                Interviewer = Interviewer.Text.Trim(),
                Notes = Notes.Text,
                TranscriptPath = _text,
                AudioPath = _audio
            });
            _text = null;
            _audio = null;
            ListText.Text = string.Join("\n", _drafts.Select(item => item.HeldOn.ToString("yyyy/MM/dd") + " — " + item.Interviewer));
        }
    }
}
