using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaApplication.ViewModels;

namespace AvaloniaApplication.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new ViewModelBase();
        Title = "Avalonia Loudness Meter";
    }
}