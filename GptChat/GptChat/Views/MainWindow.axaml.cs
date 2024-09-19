using Avalonia.Controls;
using Avalonia.ReactiveUI;
using GptChat.ViewModels;

namespace GptChat.Views;

public partial class MainWindow : ReactiveWindow<MainViewModel>
{
    public MainWindow()
    {
        InitializeComponent();
    }
}