using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text;
using Newtonsoft.Json;
using Microsoft.Maui.Controls;
using MonkeyFinder.Services;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace MonkeyFinder.View;

public partial class RegistrationPage : ContentPage
{
    private const string ServerUrl = "http://176.109.104.102"; // �������� �� ��� ����� �������
    private readonly MonkeyService monkeyService = new();
    public ObservableCollection<Monkey> Monkeys { get; } = new();

    public RegistrationPage()
    {

         // �������� ���������� ������
        InitializeComponent();

    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // �������������� ���� ��� ������ ����������� ��������
        CheckSavedCredentialsAsync();
    }

    private async Task CheckSavedCredentialsAsync()
    {
        try
        {
            // �������� ���������� ����� � ������
            var username = await SecureStorage.GetAsync("session_username");
            var password = await SecureStorage.GetAsync("session_password");
            var userui = await SecureStorage.GetAsync("session_userid");
            var local_monkey = await SecureStorage.GetAsync("local_monkey");

            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                // ���������� ������ �� ������
                var response = await LoginAsync(username, password);

                if (response.Item1)
                {
                    var cleanData = JsonConvert.DeserializeObject<string>(response.Item2.ReadAsStringAsync().Result);
                    await SecureStorage.SetAsync("session_userid", cleanData);
                    // ���� ����������� �������, ��������� � ������� ��������
                    var monkeys = await monkeyService.GetMonkeys(await SecureStorage.GetAsync("session_userid"));

                    
                    await DisplayAlert("�����", "�� ������� �����!", "OK");

                    string json = JsonConvert.SerializeObject(monkeys);
                    await SecureStorage.SetAsync("local_monkey", json);

                    if (RegistrationData.CompletionSource != null)
                    {
                        RegistrationData.CompletionSource.TrySetResult(monkeys);
                    }
                    await Shell.Current.GoToAsync("..", true);
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("������", $"������ �������� ���������� ������: {ex.Message}", "OK");
        }

        // ���������� �����, ���� ����������� �� �������

    }

    private async Task<(bool, HttpContent)> LoginAsync(string username, string password)
    {
        try
        {
            //var response = await HttpClient.PostAsync(ServerUrl, content);

            var client = new HttpClient();

            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            var api = "/api/User/true";
            client.BaseAddress = new Uri(ServerUrl);
            client.DefaultRequestHeaders.Accept.Add(contentType);

            var data = new Dictionary<string, string>
            {
                {"id","3fa85f64-5717-4562-b3fc-2c963f66afa6"},
                {"userName","find"},
                {"email",username},
                {"password",password},
            };

            var jsonData = JsonConvert.SerializeObject(data);
            var contentData = new StringContent(jsonData, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(api, contentData);

            return (response.IsSuccessStatusCode, response.Content); // �������� �����������, ���� ������ 200
        }
        catch
        {
            return (false, null);
        }
    }

    private async Task<(bool, HttpContent)> RegAsync(string email, string password, string username)
    {
        try
        {
            //var response = await HttpClient.PostAsync(ServerUrl, content);

            var client = new HttpClient();

            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            var api = "/api/User";
            client.BaseAddress = new Uri(ServerUrl);
            client.DefaultRequestHeaders.Accept.Add(contentType);

            var data = new Dictionary<string, string>
            {
                {"id","3fa85f64-5717-4562-b3fc-2c963f66afa6"},
                {"userName",username},
                {"email",email},
                {"password",password},
            };

            var jsonData = JsonConvert.SerializeObject(data);
            var contentData = new StringContent(jsonData, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(api, contentData);

            return (response.IsSuccessStatusCode, response.Content);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            try
            {
                var client = new HttpClient();

                var contentType = new MediaTypeWithQualityHeaderValue("application/json");
                var api = "/api/User";
                client.BaseAddress = new Uri(ServerUrl);
                client.DefaultRequestHeaders.Accept.Add(contentType);

                var data = new Dictionary<string, string>
                {
                    {"id","3fa85f64-5717-4562-b3fc-2c963f66afa6"},
                    {"userName",username},
                    {"email",email},
                    {"password",password}
                };

                var jsonData = JsonConvert.SerializeObject(data);
                var contentData = new StringContent(jsonData, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(api, contentData);

                return (response.IsSuccessStatusCode, response.Content);
            }
            catch
            {
                return (false, null);
            }
        }
    }


    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        var username = UsernameEntry.Text?.Trim();
        var password = PasswordEntry.Text?.Trim();
        var name = NameEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("������", "����� � ������ �� ����� ���� �������.", "OK");
            return;
        }

        var success = await LoginAsync(username, password);

        List<Monkey> monkeys = [];

        if (success.Item1)
        {
            await DisplayAlert("�����", "�� ������� �����!", "OK");
            await SecureStorage.SetAsync("session_username", username);
            await SecureStorage.SetAsync("session_password", password);
            var cleanData = JsonConvert.DeserializeObject<string>(success.Item2.ReadAsStringAsync().Result);
            await SecureStorage.SetAsync("session_userid", cleanData);
            monkeys = await monkeyService.GetMonkeys(await SecureStorage.GetAsync("session_userid"));

            string json = JsonConvert.SerializeObject(monkeys);
            await SecureStorage.SetAsync("local_monkey", json);
        }
        else
        {   
            var response_reg = await RegAsync(username, password, name);
            if (response_reg.Item1)
            {
                // ��������� ����� ������, ���� �� ������� ����� ������������
                await SecureStorage.SetAsync("session_username", username);
                await SecureStorage.SetAsync("session_password", password);
                var raw_id = await response_reg.Item2.ReadAsStringAsync();
                var id_user = JsonConvert.DeserializeObject<string>(raw_id);
                await SecureStorage.SetAsync("session_userid", id_user);
                await DisplayAlert("�����", "�� ������� ������������������!", "OK");
            }
            else
            {
                await DisplayAlert("������", $"������ ��� ���������� ������: {response_reg}", "OK");
            }
        }

        if (RegistrationData.CompletionSource != null)
        {
            RegistrationData.CompletionSource.TrySetResult(monkeys);
        }
        await Shell.Current.GoToAsync("..", true);

    }
}
