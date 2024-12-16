using MonkeyFinder.Services;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;

namespace MonkeyFinder.ViewModel;

public partial class MonkeysViewModel : BaseViewModel
{
    public ObservableCollection<Monkey> MonkeysRaw = new();
    public ObservableCollection<Monkey> Monkeys { get; set; } = new();
    MonkeyService monkeyService;
    IConnectivity connectivity;
    public MonkeysViewModel(MonkeyService monkeyService, IConnectivity connectivity)
    {
        Title = "Monkey Finder";
        this.monkeyService = monkeyService;
        this.connectivity = connectivity;
    }

    [ObservableProperty]
    private string searchText;

    [RelayCommand]
    private void Search()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            Monkeys.Clear();

            foreach (var monkey in MonkeysRaw)
            {
                Monkeys.Add(monkey);
            }
        }
        else
        {
            var filtered = MonkeysRaw
                .Where(m => m.name.Contains(SearchText, System.StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (filtered.Count != 0)
            {
                Monkeys.Clear();
            }

            foreach (var monkey in filtered)
            {
                Monkeys.Add(monkey);
            }
        }
    }

    [RelayCommand]
    private void Delete(Monkey monkey)
    {
        if (Monkeys.Contains(monkey))
        {
            Monkeys.Remove(monkey);
            MonkeysRaw.Remove(monkey);
        }
    }

    [RelayCommand]
    async Task GoToDetails(Monkey monkey)
    {
        if (monkey == null)
        return;

        await Shell.Current.GoToAsync(nameof(DetailsPage), true, new Dictionary<string, object>
        {
            {"Monkey", monkey }
        });
    }

    [ObservableProperty]
    bool isRefreshing;

    [RelayCommand]
    async Task GetMonkeysAsync()
    {
        if (IsBusy)
            return;

        try
        {
            if (connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                await Shell.Current.DisplayAlert("No connectivity!",
                    $"Please check internet and try again.", "OK");
                return;
            }
            IsBusy = true;

            var completionSource = new TaskCompletionSource<List<Monkey>>();

            RegistrationData.CompletionSource = completionSource;


            await Shell.Current.GoToAsync(nameof(View.RegistrationPage), true);
            //await Shell.Current.GoToAsync(nameof(DetailsPage), true, new Dictionary<string, object>
            //{
            //    {"Monkey", empty }
            //});

            var result = await completionSource.Task;

            if (Monkeys.Count != 0)
            {
                Monkeys.Clear();
                MonkeysRaw.Clear();
            }

            foreach (var monkey in result)
            {
                Monkeys.Add(monkey);
                MonkeysRaw.Add(monkey);
            }
        

        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unable to get monkeys: {ex.Message}");
            await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }

    }

    [RelayCommand]
    async Task CreateRecordAsync()
    {
        await Shell.Current.GoToAsync(nameof(View.CreateRecordPage), true);
    }
}
