namespace MonkeyFinder.View;

public partial class CreateRecordPage : ContentPage
{
    public CreateRecordPage(CreateRecordViewModel viewModel)
    {
        InitializeComponent(); // Инициализирует элементы интерфейса
        BindingContext = viewModel; // Устанавливает ViewModel
    }
}
