using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace MonkeyFinder.ViewModel;

public partial class CreateRecordViewModel : BaseViewModel
{
    private readonly HttpClient _httpClient;

    public Monkey monkey = new();
    


    public CreateRecordViewModel()
    {
        _httpClient = new HttpClient();
        SelectPhotoCommand = new AsyncRelayCommand(SelectPhotoAsync);
        SubmitCommand = new AsyncRelayCommand(SubmitAsync);
        NavigateToSummaryCommand = new AsyncRelayCommand(NavigateToSummaryAsync);
    }

    [ObservableProperty]
    private string selectedPhotoPath;

    [ObservableProperty]
    private string photoBase64;

    [ObservableProperty]
    private string name;

    [ObservableProperty]
    private string phoneNumber;

    [ObservableProperty]
    private string email;

    [ObservableProperty]
    private string group;

    [ObservableProperty]
    private string description;

    public IAsyncRelayCommand SelectPhotoCommand { get; }
    public IAsyncRelayCommand SubmitCommand { get; }
    public IAsyncRelayCommand NavigateToSummaryCommand { get; }

    private async Task SelectPhotoAsync()
    {
        var result = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Выберите фото",
            FileTypes = FilePickerFileType.Images
        });

        if (result != null)
        {
            SelectedPhotoPath = result.FullPath;
            using var stream = await result.OpenReadAsync();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            var imageBytes = memoryStream.ToArray();
            PhotoBase64 = Convert.ToBase64String(imageBytes);
        }
    }

    private async Task SubmitAsync()
    {

        if (string.IsNullOrEmpty(SelectedPhotoPath))
        {
            await Shell.Current.DisplayAlert("Error", "Please select a photo.", "OK");
            return;
        }

        try
        {
            // Step 1: Upload photo and get file name
            string photoFileName = await UploadPhotoAsync(SelectedPhotoPath);

            var recordData = new Monkey
            {
                userId = await SecureStorage.GetAsync("session_userid"),
                photo = "http://176.109.104.102/api/Photos/" + photoFileName,
                name = Name,
                phone = PhoneNumber,
                email = Email,
                group = Group,
                description = Description
            };

            var response = await _httpClient.PostAsJsonAsync("http://176.109.104.102/api/PhoneBook", recordData);

            if (response.IsSuccessStatusCode)
            {
                await Shell.Current.DisplayAlert("Успех", "Данные отправлены успешно!", "ОК");
            }
            else
            {
                await Shell.Current.DisplayAlert("Ошибка", "Не удалось отправить данные", "ОК");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Ошибка", $"Произошла ошибка: {ex.Message}", "ОК");
        }
    }

    private async Task<string> UploadPhotoAsync(string filePath)
    {
        // создаем MultipartFormDataContent
        using var multipartFormContent = new MultipartFormDataContent();
        // считываем данные файла в массив байтов
        byte[] fileToBytes = await File.ReadAllBytesAsync(filePath);
        // формируем отправляемое содержимое
        var content = new ByteArrayContent(fileToBytes);
        // Устанавливаем заголовок Content-Type
        content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        // Добавляем загруженный файл в MultipartFormDataContent
        multipartFormContent.Add(content, name: "photo", fileName: Path.GetFileName(filePath));

        var response = await _httpClient.PostAsync("http://176.109.104.102/api/Photos/upload", multipartFormContent);

        if (!response.IsSuccessStatusCode)
            throw new Exception("Failed to upload photo.");

        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<UploadResponse>(responseContent);
        return result?.path ?? throw new Exception("Invalid server response.");
    }

    private async Task NavigateToSummaryAsync()
    {
        var recordData = new Monkey
        {
            photo = PhotoBase64,
            name = Name,
            phone = PhoneNumber,
            email = Email,
            group = Group,
            description = Description
        };

        var navigationParameters = new Dictionary<string, object>
        {
            { "Monkey", recordData }
        };

        await Shell.Current.GoToAsync(nameof(DetailsPage), navigationParameters);
    }

    private class UploadResponse
    {
        public string path { get; set; }
    }

    private class UploadPhoto
    {
        public byte[] photo { get; set; }
    }
}