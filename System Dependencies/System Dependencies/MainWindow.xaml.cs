//Author: Marlon Robancho
using System.Windows;

namespace System_Dependencies
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly VM vm = new VM();
        public MainWindow()
        {
            InitializeComponent();
            DataContext = vm;
        }

        private void ProcessInput_Click(object sender, RoutedEventArgs e)
        {
            vm.Init();
        }
    }
}
