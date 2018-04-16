// ******************************************************************
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE CODE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH
// THE CODE OR THE USE OR OTHER DEALINGS IN THE CODE.
// ******************************************************************

using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Toolkit.Win32.UI.Controls.Interop.WinRT;
using Microsoft.Toolkit.Win32.UI.Controls.WinForms;

namespace Microsoft.Toolkit.Win32.Samples.WPF.WebView
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _isFullScreen;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void BrowseBack_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = WebView1 != null && WebView1.CanGoBack;
        }

        private void BrowseBack_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            WebView1?.GoBack();
        }

        private void BrowseForward_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = WebView1 != null && WebView1.CanGoForward;
        }
        private void GoToPage_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void GoToPage_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var result = (Uri)new WebBrowserUriTypeConverter().ConvertFromString(Url.Text);
            WebView1.Source = result;
        }

        private void BrowseForward_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            WebView1?.GoForward();
        }

        private void Url_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && WebView1 != null)
            {
                var result =
                    (Uri)new WebBrowserUriTypeConverter().ConvertFromString(
                        Url.Text);
                WebView1.Source = result;

            }
        }

        private void WebView1_OnNavigationCompleted(object sender, WebViewNavigationCompletedEventArgs e)
        {
            Url.Text = e.Uri.ToString();
            Title = WebView1.DocumentTitle;
            if (!e.IsSuccess)
            {
                MessageBox.Show($"Could not navigate to {e.Uri}", $"Error: {e.WebErrorStatus}", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void WebView1_OnNavigationStarting(object sender, WebViewControlNavigationStartingEventArgs e)
        {
            Title = "Navigating " + e.Uri.ToString();
            Url.Text = e.Uri.ToString();
        }

        private void WebView1_OnPermissionRequested(object sender, WebViewControlPermissionRequestedEventArgs e)
        {
            if (e.PermissionRequest.State == WebViewControlPermissionState.Allow) return;

            var msg = $"Allow {e.PermissionRequest.Uri.Host} to access {e.PermissionRequest.PermissionType}?";

            var response = MessageBox.Show(msg, "Permission Request", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);

            if (response == MessageBoxResult.Yes)
            {
                if (e.PermissionRequest.State == WebViewControlPermissionState.Defer)
                {
                    WebView1.GetDeferredPermissionRequestById(e.PermissionRequest.Id)?.Allow();
                }
                else
                {
                    e.PermissionRequest.Allow();
                }
            }
            else
            {
                if (e.PermissionRequest.State == WebViewControlPermissionState.Defer)
                {
                    WebView1.GetDeferredPermissionRequestById(e.PermissionRequest.Id)?.Deny();
                }
                else
                {
                    e.PermissionRequest.Deny();
                }
            }
        }

        private void WebView1_OnScriptNotify(object sender, WebViewControlScriptNotifyEventArgs e)
        {
            MessageBox.Show(e.Value, e.Uri?.ToString() ?? string.Empty);
        }

        private void WebView1_OnContainsFullScreenElementChanged(object sender, object e)
        {
            void EnterFullScreen()
            {

                WindowState =  WindowState.Normal;
                ResizeMode = ResizeMode.NoResize;
                WindowState = WindowState.Maximized;
            }

            void LeaveFullScreen()
            {
                ResizeMode = ResizeMode.CanResize;
                WindowState = WindowState.Normal;
            }

            // Toggle
            _isFullScreen = !_isFullScreen;

            if (_isFullScreen)
            {
                EnterFullScreen();
            }
            else
            {
                LeaveFullScreen();
            }
        }
    }
}
