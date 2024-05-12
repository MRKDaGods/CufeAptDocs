using System.Windows.Controls;

namespace MRK
{
    public static class PageExtensions
    {
        public static MainWindow MainWindowInstance(this Page _)
        {
            return MainWindow.Instance!;
        }

        public static void GoTo(this Page page, string path)
        {
            page.MainWindowInstance().GoTo(path);
        }

        public static void ShowMessagePopup(this Page page, string text)
        {
            page.MainWindowInstance().ShowMessagePopup(text);
        }

        public static void HideMessagePopup(this Page page)
        {
            page.MainWindowInstance().HideMessagePopup();
        }

        public static string? GetStringFromUser(this Page page, string header, string? oldText = null)
        {
            return page.MainWindowInstance().GetStringFromUser(header, oldText);
        }
    }
}
