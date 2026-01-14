using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

public partial class EditEntry
{
    [Parameter] public int EntryId { get; set; }

    private bool isLoading = true;

    private string title = "";
    private int? selectedPrimaryMoodId;
    private List<int> selectedSecondaryMoodIds = new();
    private List<int> selectedTagIds = new();
    private List<string> customTags = new();
    private string customTagInput = "";
    private string content = "";
    private string contentError = "";

    private List<Mood> primaryMoods = new();
    private List<SecondaryMood> secondaryMoods = new();
    private List<Tag> availableTags = new();

    protected override async Task OnInitializedAsync()
    {
        primaryMoods = await JournalService.GetPrimaryMoods();
        secondaryMoods = await JournalService.GetSecondaryMoods();
        availableTags = await JournalService.GetTags();

        var entry = await JournalService.GetEntryById(EntryId);

        title = entry.Title;
        selectedPrimaryMoodId = entry.PrimaryMoodId;
        selectedSecondaryMoodIds = entry.SecondaryMoods.Select(m => m.Id).ToList();
        selectedTagIds = entry.Tags.Where(t => t.Id != 0).Select(t => t.Id).ToList();
        customTags = entry.Tags.Where(t => t.Id == 0).Select(t => t.Name).ToList();
        content = entry.Content;

        isLoading = false;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JS.InvokeVoidAsync("initQuill", "editor");
            await JS.InvokeVoidAsync("setQuillContent", content);
        }
    }

    private void ToggleSecondaryMood(int id, ChangeEventArgs e)
    {
        if ((bool)e.Value!)
        {
            if (selectedSecondaryMoodIds.Count >= 2)
                return;

            selectedSecondaryMoodIds.Add(id);
        }
        else
        {
            selectedSecondaryMoodIds.Remove(id);
        }
    }

    private void ToggleTag(int id, ChangeEventArgs e)
    {
        if ((bool)e.Value!)
            selectedTagIds.Add(id);
        else
            selectedTagIds.Remove(id);
    }

    private void AddCustomTag()
    {
        if (!string.IsNullOrWhiteSpace(customTagInput))
        {
            customTags.Add(customTagInput.Trim());
            customTagInput = "";
        }
    }

    private void RemoveCustomTag(string tag)
    {
        customTags.Remove(tag);
    }

    private async Task UpdateEntry()
    {
        content = await JS.InvokeAsync<string>("getQuillContent");

        if (string.IsNullOrWhiteSpace(content))
        {
            contentError = "Content is required";
            return;
        }

        contentError = "";

        await JournalService.UpdateEntry(new JournalEntryUpdateDto
        {
            EntryId = EntryId,
            Title = title,
            Content = content,
            PrimaryMoodId = selectedPrimaryMoodId!.Value,
            SecondaryMoodIds = selectedSecondaryMoodIds,
            TagIds = selectedTagIds,
            CustomTags = customTags
        });

        Nav.NavigateTo("/dashboard");
    }
}
