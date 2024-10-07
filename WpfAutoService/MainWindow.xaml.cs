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
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfAutoService.Pages;

namespace WpfAutoService
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            frame.Content = new PageListClient(frame);
        }

        private void frame_LoadCompleted(object sender, NavigationEventArgs e)
        {

            try
            {
                PageListClient pg = (PageListClient)e.Content;
                pg.Load();
            }
            catch { };
        }
        public class helper
        {
            public static bool flag = false;
            public static int prioritet = 0;
            public static Entities ent;
            public static Entities GetContext()
            {
                if (ent == null)
                {
                    ent = new Entities();
                }
                return ent;
            }
        }
    }
}
