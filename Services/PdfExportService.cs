using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;
using Microsoft.Maui.Storage;
using JournalApps.Models;
using System.Linq;
using Colors = QuestPDF.Helpers.Colors;

namespace JournalApps.Services
{
    public class PdfExportService
    {
        public async Task<string> ExportEntriesToPdf(List<JournalEntry> entries, string title = "Journal Export")
        {
            if (!entries.Any())
                throw new InvalidOperationException("No entries to export");

            // Create PDF document
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Text(title)
                        .SemiBold().FontSize(24).FontColor(Colors.Blue.Medium);

                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(column =>
                    {
                        foreach (var entry in entries.OrderByDescending(e => e.EntryDate))
                        {
                            column.Item().PaddingBottom(20).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Column(entryColumn =>
                            {
                                // Entry header
                                entryColumn.Item().Row(row =>
                                {
                                    row.RelativeItem().Text(entry.Title)
                                        .SemiBold().FontSize(16);

                                    row.AutoItem().Text(entry.EntryDate.ToString("MMMM dd, yyyy"))
                                        .FontColor(Colors.Grey.Medium);
                                });

                                // Mood and tags
                                if (!string.IsNullOrEmpty(entry.PrimaryMood) || entry.SecondaryMoods.Any() || entry.Tags.Any())
                                {
                                    entryColumn.Item().PaddingTop(5).Row(moodRow =>
                                    {
                                        if (!string.IsNullOrEmpty(entry.PrimaryMood))
                                        {
                                            moodRow.AutoItem().Text($"Primary Mood: {entry.PrimaryMood}")
                                                .FontColor(GetMoodColor(entry.PrimaryMood));
                                        }

                                        if (entry.SecondaryMoods.Any())
                                        {
                                            var secondaryMoods = string.Join(", ", entry.SecondaryMoods.Select(m => m.MoodName));
                                            moodRow.AutoItem().PaddingLeft(10).Text($"Secondary: {secondaryMoods}")
                                                .FontColor(Colors.Grey.Medium);
                                        }
                                    });

                                    if (entry.Tags.Any())
                                    {
                                        var tags = string.Join(", ", entry.Tags.Select(t => t.TagName));
                                        entryColumn.Item().PaddingTop(2).Text($"Tags: {tags}")
                                            .FontColor(Colors.Grey.Medium).FontSize(10);
                                    }
                                }

                                // Content (strip HTML tags)
                                var plainContent = System.Text.RegularExpressions.Regex.Replace(
                                    entry.Content ?? "",
                                    "<.*?>",
                                    string.Empty
                                );

                                entryColumn.Item().PaddingTop(10).Text(plainContent)
                                    .FontSize(11).LineHeight(1.5f); // Fixed: added 'f' for float

                                // Timestamps
                                entryColumn.Item().PaddingTop(10).Row(timestampRow =>
                                {
                                    timestampRow.RelativeItem().Text($"Created: {entry.CreatedAt:MMM dd, yyyy hh:mm tt}")
                                        .FontColor(Colors.Grey.Medium).FontSize(9);

                                    timestampRow.AutoItem().Text($"Updated: {entry.UpdatedAt:MMM dd, yyyy hh:mm tt}")
                                        .FontColor(Colors.Grey.Medium).FontSize(9);
                                });
                            });
                        }
                    });

                    page.Footer().AlignCenter().Text($"Page {{$pageNumber}} of {{$pagesCount}} | Exported on {DateTime.Now:MMMM dd, yyyy hh:mm tt}")
                    .FontSize(9)
                    .FontColor(Colors.Grey.Medium);
                });
            });

            // Generate PDF bytes
            var pdfBytes = document.GeneratePdf();

            // Save to file
            var fileName = $"Journal_Export_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            var filePath = Path.Combine(FileSystem.AppDataDirectory, "Exports", fileName);

            // Ensure directory exists
            var directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            await File.WriteAllBytesAsync(filePath, pdfBytes);

            return filePath;
        }

        private string GetMoodColor(string mood)
        {
            return mood.ToLower() switch
            {
                "happy" => Colors.Green.Medium,
                "sad" => Colors.Purple.Medium,
                "angry" => Colors.Red.Medium,
                "relaxed" => Colors.Blue.Medium,
                "anxious" => Colors.Orange.Medium,
                _ => Colors.Grey.Medium
            };
        }

        public async Task<string> ExportSingleEntryToPdf(JournalEntry entry)
        {
            var entries = new List<JournalEntry> { entry };
            return await ExportEntriesToPdf(entries, entry.Title);
        }

        public async Task<string> ExportDateRangeToPdf(List<JournalEntry> allEntries, DateTime startDate, DateTime endDate)
        {
            var filteredEntries = allEntries
                .Where(e => e.EntryDate.Date >= startDate.Date && e.EntryDate.Date <= endDate.Date)
                .ToList();

            var title = $"Journal Entries from {startDate:MMM dd, yyyy} to {endDate:MMM dd, yyyy}";
            return await ExportEntriesToPdf(filteredEntries, title);
        }
    }
}