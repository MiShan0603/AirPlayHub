using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AirPlayHub.UI
{
    /// <summary>
    /// SettingWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingWindow : Window
    {
        public SettingWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtName.Text = Config.Instance().Name;
            txtPin.Text = Config.Instance().PinCode;
            checkBoxShowWindow.IsChecked = Config.Instance().StartWithWindow;
            TxtStartWithWindow();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (txtName.Text.Length == 0)
            {
                MessageBox.Show("服务名称不能为空，请重新设置后再次确定。");
                return;
            }

            this.DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }


        private void txtPin_TextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsDigit(e.Text);
        }

        private void checkBoxShowWindow_Click(object sender, RoutedEventArgs e)
        {
            TxtStartWithWindow();
        }

        private void TxtStartWithWindow()
        {
            if ((bool)checkBoxShowWindow.IsChecked)
            {
                txtStartWithWindow.Text = "程序启动时，显示渲染窗口";
            }
            else
            {
                txtStartWithWindow.Text = "程序启动时，不显示渲染窗口，当投屏时，程序默认全屏显示。";
            }
        }


        public string GetName()
        {
            return txtName.Text;
        }

        public string GetPinCode()
        {
            return txtPin.Text;
        }

        public bool IsStartWithShowWindow()
        {
            return (bool)checkBoxShowWindow.IsChecked;
        }


        private bool IsDigit(string text)
        {
            foreach (char c in text)
            {
                if (!char.IsDigit(c))
                    return false;
            }

            return true;
        }


    }
}
