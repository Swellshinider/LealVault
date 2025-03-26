using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia.Models;

namespace LealVault.Utilities;

public static class Controls
{
    public static async Task<string> DisplayMessageBox(this Window? window, string title, string message, Icon icon, List<ButtonDefinition> buttons)
    {
        var messageParams = new MessageBoxCustomParams()
        {
            Icon = icon,
            ContentTitle = title,
            ContentMessage = message,
            ButtonDefinitions = buttons,
            CanResize = false,
            ShowInCenter = true,
            Topmost = true,
            SizeToContent = SizeToContent.WidthAndHeight,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
        };

        var result = MessageBoxManager.GetMessageBoxCustom(messageParams);

        return window == null
            ? await result.ShowAsync()
            : await result.ShowWindowDialogAsync(window!);
    }
}