using System.Net.Http.Headers;
using System.Net.Http.Json;
using MonkeyFinder.Services;

namespace MonkeyFinder.ViewModel;

public partial class CreateRecordViewModel : BaseViewModel
{

    public Monkey monkey = new();

    private readonly MonkeyService monkeyService = new();



    public CreateRecordViewModel()
    {
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
            PickerTitle = "�������� ����",
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

            var content = new
            {
                recordData.user,
                recordData.photo,
                recordData.name,
                recordData.phone,
                recordData.email,
                recordData.group,
                recordData.description,
                recordData.userId
            };

            //var response = await _httpClient.PostAsJsonAsync("http://176.109.104.102/api/PhoneBook", content);

            var response = await monkeyService.SendContactAsync(content);

            if (response.IsSuccessStatusCode)
            {
                await Shell.Current.DisplayAlert("�����", "������ ���������� �������!", "��");
            }
            else
            {
                await Shell.Current.DisplayAlert("������", "�� ������� ��������� ������", "��");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("������", $"��������� ������: {ex.Message}", "��");
        }
    }

    private async Task<string> UploadPhotoAsync(string filePath)
    {
        // ������� MultipartFormDataContent
        using var multipartFormContent = new MultipartFormDataContent();
        // ��������� ������ ����� � ������ ������
        byte[] fileToBytes = await File.ReadAllBytesAsync(filePath);
        // ��������� ������������ ����������
        var content = new ByteArrayContent(fileToBytes);
        // ������������� ��������� Content-Type
        content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        // ��������� ����������� ���� � MultipartFormDataContent
        multipartFormContent.Add(content, name: "photo", fileName: Path.GetFileName(filePath));

        //var response = await _httpClient.PostAsync("http://176.109.104.102/api/Photos/upload", multipartFormContent);
        var response = await monkeyService.SendPhotoAsync(multipartFormContent);

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