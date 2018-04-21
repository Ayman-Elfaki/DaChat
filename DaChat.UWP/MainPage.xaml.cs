using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace DaChat.UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        private DaStream.DaStream daStream;
        private ObservableCollection<string> messages;
        public MainPage()
        {
            this.InitializeComponent();

            messages = new ObservableCollection<string>();
            daStream = new DaStream.DaStream();

        }

        private void DaStream_DataRecived(object sender, byte[] e)
        {
            var data = Encoding.UTF8.GetString(e);
            var message = !string.IsNullOrWhiteSpace(data) ? data : "Bye.";
            messages.Add("Server: " + message);
            scrollViewer.UpdateLayout();
            scrollViewer.ScrollToVerticalOffset(double.MaxValue);
        }

        private void DaStream_Disconnected(object sender, EventArgs e)
        {
            stautsLabel.Text = "Offline";
            stautsLabel.Foreground = new SolidColorBrush(Windows.UI.Colors.Black);
        }

        private void DaStream_Connected(object sender, EventArgs e)
        {
            stautsLabel.Text = "Online";
            stautsLabel.Foreground = new SolidColorBrush(Windows.UI.Colors.Green);
        }

        private async void connectButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(serverUrlTextbox.Text) || string.IsNullOrEmpty(serverUrlTextbox.Text))
            {
                await new MessageDialog("Enter the server URL").ShowAsync();
            }
            else
            {
                stautsLabel.Text = "Connecting...";
                stautsLabel.Foreground = new SolidColorBrush(Windows.UI.Colors.YellowGreen);
                connectButton.IsEnabled = serverUrlTextbox.IsEnabled = false;
                disconnectButton.IsEnabled = sendButton.IsEnabled = messageTextbox.IsEnabled = true;
                daStream.Connected += DaStream_Connected;
                daStream.Disconnected += DaStream_Disconnected;
                daStream.DataRecived += DaStream_DataRecived;
                await daStream.ConnectAsync(serverUrlTextbox.Text.ToLower().Trim());
            }
        }

        private async void disconnectButton_Click(object sender, RoutedEventArgs e)
        {
            stautsLabel.Text = "Offline";
            stautsLabel.Foreground = new SolidColorBrush(Windows.UI.Colors.Black);
            connectButton.IsEnabled = serverUrlTextbox.IsEnabled = true;
            disconnectButton.IsEnabled = sendButton.IsEnabled = messageTextbox.IsEnabled = false;
            daStream.DataRecived -= DaStream_DataRecived;
            await daStream.DisconnectAsync();
            daStream.Dispose();
        }

        private async void sendButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(messageTextbox.Text) && !string.IsNullOrEmpty(messageTextbox.Text))
            {
                var data = Encoding.UTF8.GetBytes(messageTextbox.Text);
                await daStream.SendAsync(data);
                messageTextbox.Text = "";
            }
        }

        private async void messageTextbox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                if (!string.IsNullOrWhiteSpace(messageTextbox.Text) && !string.IsNullOrEmpty(messageTextbox.Text))
                {
                    var data = Encoding.UTF8.GetBytes(messageTextbox.Text);
                    await daStream.SendAsync(data);
                    messageTextbox.Text = "";
                }
            }
        }

    
    }
}
