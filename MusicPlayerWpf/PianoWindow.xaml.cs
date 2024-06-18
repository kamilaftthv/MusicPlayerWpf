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

namespace MusicPlayerWpf
{
    /// <summary>
    /// Логика взаимодействия для PianoWindow.xaml
    /// </summary>
    public partial class PianoWindow : Window
    {
        public PianoWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Console.Beep(261, 500);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Console.Beep(293, 500);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Console.Beep(329, 500);
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            Console.Beep(349, 500);
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            Console.Beep(392, 500);
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            Console.Beep(440, 500);
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            Console.Beep(493, 500);
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.D:
                    Button_Click(ButtonC, null);
                    break;
                case Key.F:
                    Button_Click_1(ButtonD, null);
                    break;
                case Key.G:
                    Button_Click_2(ButtonE, null);
                    break;
                case Key.H:
                    Button_Click_3(ButtonF, null);
                    break;
                case Key.J:
                    Button_Click_4(ButtonG, null);
                    break;
                case Key.K:
                    Button_Click_5(ButtonA, null);
                    break;
                case Key.L:
                    Button_Click_6(ButtonB, null);
                    break;
            }
        }

    }
}