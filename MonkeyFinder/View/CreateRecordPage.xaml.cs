namespace MonkeyFinder.View;

public partial class CreateRecordPage : ContentPage
{
    public CreateRecordPage(CreateRecordViewModel viewModel)
    {
        InitializeComponent(); // �������������� �������� ����������
        BindingContext = viewModel; // ������������� ViewModel
    }
}
