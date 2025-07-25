using Returnly.Models;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Controls;

namespace Returnly.Dialogs
{
    /// <summary>
    /// Dialog to display ITR form recommendation and details
    /// </summary>
    public class ITRRecommendationWindow : FluentWindow
    {
        public ITRRecommendationWindow(ITRSelectionResult selectionResult, ITRSelectionCriteria criteria)
        {
            InitializeWindow();
            DisplayITRRecommendation(selectionResult, criteria);
        }

        private void InitializeWindow()
        {
            this.Width = 600;
            this.Height = 700;
            this.MinWidth = 500;
            this.MinHeight = 400;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            this.ResizeMode = ResizeMode.CanResize;
        }

        private void DisplayITRRecommendation(ITRSelectionResult selectionResult, ITRSelectionCriteria criteria)
        {
            // Set window title
            this.Title = $"ITR Form Recommendation - {selectionResult.RecommendedITR}";

            // Create main content with proper structure
            var mainGrid = new Grid
            {
                Margin = new Thickness(20)
            };
            
            // Define rows
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Header
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Content
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Buttons

            // Header
            var header = new System.Windows.Controls.TextBlock
            {
                Text = "🎯 ITR Form Recommendation",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 20),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Grid.SetRow(header, 0);
            mainGrid.Children.Add(header);

            // Scrollable content area
            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Margin = new Thickness(0, 0, 0, 20),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Stretch
            };

            var contentStack = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            // Recommendation section
            var recommendationSection = CreateSection("📋 Recommended Form", 
                $"✅ {selectionResult.RecommendedITR}\n" +
                $"📝 Reason: {selectionResult.PrimaryReason}\n" +
                $"💡 {selectionResult.Explanation}");
            contentStack.Children.Add(recommendationSection);

            // Income breakdown section
            var incomeSection = CreateSection("💰 Income Summary",
                $"Total Income: ₹{criteria.TotalIncome:N0}\n\n" +
                $"Breakdown:\n" +
                $"• Salary: ₹{criteria.SalaryIncome:N0}\n" +
                $"• Interest: ₹{criteria.InterestIncome:N0}\n" +
                $"• Dividend: ₹{criteria.DividendIncome:N0}\n" +
                $"• House Property: ₹{criteria.HousePropertyIncome:N0}\n" +
                $"• Capital Gains: ₹{criteria.CapitalGains:N0}\n" +
                $"• Other Income: ₹{criteria.OtherIncome:N0}");
            contentStack.Children.Add(incomeSection);

            // Eligibility factors section
            var eligibilitySection = CreateSection("✨ Key Factors",
                GetEligibilityFactors(selectionResult, criteria));
            contentStack.Children.Add(eligibilitySection);

            scrollViewer.Content = contentStack;
            Grid.SetRow(scrollViewer, 1);
            mainGrid.Children.Add(scrollViewer);

            // Button panel at bottom
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var proceedButton = new Wpf.Ui.Controls.Button
            {
                Content = "✅ Proceed with This Form",
                Padding = new Thickness(20, 12, 20, 12),
                Margin = new Thickness(0, 0, 15, 0),
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                MinWidth = 200,
                Appearance = ControlAppearance.Primary
            };
            proceedButton.Click += (s, e) => { DialogResult = true; Close(); };

            var cancelButton = new Wpf.Ui.Controls.Button
            {
                Content = "❌ Cancel",
                Padding = new Thickness(20, 12, 20, 12),
                FontSize = 14,
                MinWidth = 100,
                Appearance = ControlAppearance.Secondary
            };
            cancelButton.Click += (s, e) => { DialogResult = false; Close(); };

            buttonPanel.Children.Add(proceedButton);
            buttonPanel.Children.Add(cancelButton);

            Grid.SetRow(buttonPanel, 2);
            mainGrid.Children.Add(buttonPanel);

            // Set the main grid as content
            this.Content = mainGrid;
        }

        private CardControl CreateSection(string title, string content)
        {
            var section = new CardControl
            {
                Margin = new Thickness(0, 0, 0, 20),
                Padding = new Thickness(20),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            var stackPanel = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            var titleBlock = new System.Windows.Controls.TextBlock
            {
                Text = title,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10),
                HorizontalAlignment = HorizontalAlignment.Left
            };

            var contentBlock = new System.Windows.Controls.TextBlock
            {
                Text = content,
                FontSize = 14,
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 20,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            stackPanel.Children.Add(titleBlock);
            stackPanel.Children.Add(contentBlock);
            section.Content = stackPanel;

            return section;
        }

        private string GetEligibilityFactors(ITRSelectionResult selectionResult, ITRSelectionCriteria criteria)
        {
            var factors = new System.Text.StringBuilder();

            factors.AppendLine($"• Taxpayer Category: {criteria.TaxpayerCategory}");
            factors.AppendLine($"• Residency Status: {criteria.ResidencyStatus}");
            
            if (criteria.HasAnyCapitalGains)
                factors.AppendLine("• Has Capital Gains ⚠️");
            
            if (criteria.HasAnyBusinessIncome)
                factors.AppendLine("• Has Business Income ⚠️");
            
            if (criteria.HasMultipleHouseProperties)
                factors.AppendLine("• Multiple House Properties ⚠️");
            
            if (criteria.HasForeignIncome)
                factors.AppendLine("• Has Foreign Income ⚠️");
            
            if (criteria.HasForeignAssets)
                factors.AppendLine("• Has Foreign Assets ⚠️");
            
            if (criteria.IsDirectorOfCompany)
                factors.AppendLine("• Director of Company ⚠️");

            // Add income limit check for ITR-1
            if (selectionResult.RecommendedITR == ITRType.ITR1_Sahaj)
            {
                factors.AppendLine($"• Income within ITR-1 limit (₹50L) ✅");
            }
            else if (criteria.TotalIncome > 5000000)
            {
                factors.AppendLine($"• Income exceeds ITR-1 limit ⚠️");
            }

            if (factors.Length == 0)
            {
                factors.AppendLine("• Simple income structure ✅");
                factors.AppendLine("• No complex investments ✅");
            }

            return factors.ToString();
        }
    }
}
