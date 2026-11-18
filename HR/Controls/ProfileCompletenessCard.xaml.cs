using System.Windows.Controls;
using HR.Services.Data;

namespace HR.Controls
{
    public partial class ProfileCompletenessCard : UserControl
    {
        public ProfileCompletenessCard()
        {
            InitializeComponent();
        }

        public void Show(ProfileReport? report)
        {
            if (report == null)
            {
                Ring.Value = 0;
                PercentText.Text = "—";
                ScoreText.Text = "امتیاز —";
                RemainingText.Text = "باقی‌مانده —";
                MissingText.Text = "پروفایلی انتخاب نشده است.";
                return;
            }

            Ring.Value = report.Percent;
            PercentText.Text = report.Percent + "٪";
            ScoreText.Text = "امتیاز " + report.Score + " از " + report.MaxScore;
            RemainingText.Text = "باقی‌مانده " + report.RemainingPercent + "٪";
            MissingText.Text = report.Missing.Count == 0
                ? "بخش ناقصی نمانده است."
                : "ناقص: " + string.Join("، ", report.Missing);
        }
    }
}
