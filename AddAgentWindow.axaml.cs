using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Voroncov1.Models;

namespace Voroncov1;

public partial class AddAgentWindow : Window
{
    public string PathToImage = string.Empty;
    public Agent agentPresenter;
    public string agentType { get; set; } = null;
    public int agentTypeId { get; set; } = 0;
    public AddAgentWindow()
    {
        using var ctx = new DatabaseContext();
        InitializeComponent();
        agentPresenter = new Agent();
    }
    
    private async Task<Bitmap?> SelectAndSaveImage()
    {
        var showDialog = StorageProvider.OpenFilePickerAsync(
            options: new Avalonia.Platform.Storage.FilePickerOpenOptions()
            {
                Title = "Select an image",
                FileTypeFilter = new[] { FilePickerFileTypes.ImageAll }
            });
        var storageFile = await showDialog;
        try
        {
            var bmp = new Bitmap(storageFile.First().TryGetLocalPath());
            var guid = Guid.NewGuid();
            string path = $"/Users/rinchi/RiderProjects/Voroncov2103/Voroncov2103/bin/Debug/net8.0/agents/{guid}.jpg";
            bmp.Save(path);
            PathToImage = $"agents/{guid}.jpg";
            return bmp;
        }
        catch
        {
            return null;
        }
    }
    
    private async void AddClick(object? sender, RoutedEventArgs e)
    {
        using var ctx = new DatabaseContext();
        if (string.IsNullOrEmpty(TitleTextBox.Text)) return;
        agentPresenter.Title = TitleTextBox.Text;
        agentPresenter.AgentTypeId = agentTypeId;
        if (!int.TryParse(PriorityTextBox.Text, out int priority)) return;
        agentPresenter.Priority = priority;
        if (string.IsNullOrEmpty(AddressTextBox.Text)) return;
        agentPresenter.Address = AddressTextBox.Text;
        if (string.IsNullOrEmpty(INNTextBox.Text)) return;
        agentPresenter.Inn = INNTextBox.Text;
        if (string.IsNullOrEmpty(KPPTextBox.Text)) return;
        agentPresenter.Kpp = KPPTextBox.Text;
        if (string.IsNullOrEmpty(DirectorNameTextBox.Text)) return;
        agentPresenter.DirectorName = DirectorNameTextBox.Text;
        if (string.IsNullOrEmpty(PhoneTextBox.Text)) return;
        agentPresenter.Phone = PhoneTextBox.Text;
        if (string.IsNullOrEmpty(EmailTextBox.Text)) return;
        agentPresenter.Email = EmailTextBox.Text;
        
        if (String.IsNullOrEmpty(PathToImage)) return;
        agentPresenter.Logo = PathToImage;
        Close(agentPresenter);
    }
    
    private async void SelectImage(object? sender, RoutedEventArgs e)
    {
        LogoImage.Source = await SelectAndSaveImage();
    }

    private void TypeAgentFilterCombobox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (TypeAgentFilterCombobox.SelectedItem is ComboBoxItem selectedItem)
        {
            agentType = selectedItem.Content.ToString();
            switch (agentType)
            {
                case "ООО": agentTypeId = 2; break;
                case "МФО": agentTypeId = 1; break;
                case "ЗАО": agentTypeId = 3; break;
                case "МКК": agentTypeId = 5; break;
                case "ПАО": agentTypeId = 10; break;
                case "ОАО": agentTypeId = 6; break;
            }
        }
    }
}