using JournalApps.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JournalApps.Services
{
    public class SeedDataService
    {
        private readonly JournalService _journalService;
        private readonly UserService _userService;

        public SeedDataService(JournalService journalService, UserService userService)
        {
            _journalService = journalService;
            _userService = userService;
        }

        public async Task GenerateSeedDataAsync()
        {
            if (!_userService.IsAuthenticated) return;

            var random = new Random();
            var moods = new[] { "Happy", "Sad", "Neutral", "Excited", "Anxious", "Calm" };
            var tags = new[] { "Work", "Personal", "Health", "Idea", "Reflection", "Travel" };

            // Generate entries for the last 30 days
            for (int i = 0; i < 20; i++) // Create 20 entries
            {
                var daysAgo = random.Next(0, 30);
                var entryDate = DateTime.Now.AddDays(-daysAgo);

                var entry = new JournalEntry
                {
                    Title = $"Journal Entry - {entryDate:MMM dd}",
                    Content = $"This is a sample journal entry generated for testing purposes. Today was a {moods[random.Next(moods.Length)]} day. I worked on some coding and felt productive. " +
                              "I also took some time to relax and reflect on my goals.",
                    EntryDate = entryDate,
                    PrimaryMood = moods[random.Next(moods.Length)],
                    UserId = _userService.CurrentUser!.Id
                };

                // Create tags
                var entryTags = new List<Tag>();
                int numTags = random.Next(1, 4);
                for (int t = 0; t < numTags; t++)
                {
                    entryTags.Add(new Tag { TagName = tags[random.Next(tags.Length)], UserId = _userService.CurrentUser.Id });
                }

                // Create secondary moods
                var entryMoods = new List<SecondaryMood>();
                int numMoods = random.Next(1, 3);
                for (int m = 0; m < numMoods; m++)
                {
                    var moodName = moods[random.Next(moods.Length)];
                    var category = moodName == "Happy" || moodName == "Excited" ? "Positive" :
                                   moodName == "Sad" || moodName == "Anxious" ? "Negative" : "Neutral";
                    
                    entryMoods.Add(new SecondaryMood 
                    { 
                        MoodName = moodName, 
                        Category = category,
                        UserId = _userService.CurrentUser.Id
                    });
                }

                // Create entry BYPASSING limits
                await _journalService.CreateEntryAsync(entry, entryTags, entryMoods, bypassLimits: true);
            }
        }
    }
}
