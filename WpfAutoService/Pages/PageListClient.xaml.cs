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
using static WpfAutoService.MainWindow;

namespace WpfAutoService.Pages
{
    /// <summary>
    /// Логика взаимодействия для PageListClient.xaml
    /// </summary>
    public partial class PageListClient : Page
    {
        private int start = 0;
        private int fullCount = 0;
        private int order = 0;
        private string fnd = "";
        private string gnd = "";
        private Frame fr;
        private int recordsPerPage = 10;
        private int totalRecords = 0;
        private List<Client> clientVisits = new List<Client>(); // Определите список clientVisits


        public PageListClient(Frame frame)
        {
            InitializeComponent();
            fr = frame;
            List<Gender> genders = new List<Gender> { };
            genders = helper.GetContext().Gender.ToList();
            genders.Add(new Gender { Name = "Все полы" });
            Gender.ItemsSource = genders.OrderBy(Gender => Gender.Code);

            Load();
        }


        public void Load(string nameTag = "")
        {

            try
            {
                List<Client> clients = new List<Client>();
                var cl = helper.GetContext().Client.Where(Client => Client.LastName.Contains(fnd) || Client.FirstName.Contains(fnd) || Client.Phone.Contains(fnd) || Client.Email.Contains(fnd));

                if (!string.IsNullOrEmpty(nameTag))
                {
                    cl = cl.Where(client => client.IDTag.ToString() == nameTag);
                }

                if (gnd != "")
                {
                    cl = cl.Where(Client => (Client.LastName.Contains(fnd) || Client.FirstName.Contains(fnd) || Client.Phone.Contains(fnd) || Client.Email.Contains(fnd)) && (Client.GenderCode == gnd));
                }

                clients.Clear();
                clientVisits.Clear(); // Очистите список clientVisits перед заполнением

                foreach (Client client in cl)
                {
                    if (client.PhotoPath == null)
                    {
                        client.PhotoPath = "\\Клиенты\\picture.jpg";
                    }
                    //Ошибка: "изображение не найдено"
                    /*
                    if (client.PhotoPath == null || !File.Exists(client.PhotoPath))
                    {
                        client.PhotoPath = "/Клиенты/picture.jpg";
                    }
                    */

                    // Получаем дату последнего посещения и количество посещений для текущего клиента
                    var lastVisit = helper.GetContext().ClientService.Where(cs => cs.ClientID == client.ID).OrderByDescending(cs => cs.StartTime).FirstOrDefault();
                    client.LastVisitDate = lastVisit?.StartTime;
                    client.VisitCount = helper.GetContext().ClientService.Count(cs => cs.ClientID == client.ID);
                    client.Tag = helper.GetContext().Tag.FirstOrDefault(t => t.IDTag == client.IDTag);

                    clientVisits.Add(client); // Добавьте клиента в список clientVisits
                }

                fullCount = cl.Count();
                totalRecords = cl.Count();
                if (RecordsPerPage.SelectedItem != null)
                {
                    ComboBoxItem selectedItem = (ComboBoxItem)RecordsPerPage.SelectedItem;
                    recordsPerPage = selectedItem.Tag.ToString() == "-1" ? int.MaxValue : Convert.ToInt32(selectedItem.Tag.ToString());
                }


                // Сортировка и установка источника данных
                if (order == 0)
                {
                    clientsDataGrid.ItemsSource = cl.OrderBy(Client => Client.ID).Skip(start * recordsPerPage).Take(recordsPerPage).ToList();
                }
                if (order == 1)
                {
                    clientsDataGrid.ItemsSource = cl.OrderBy(Client => Client.LastName).Skip(start * recordsPerPage).Take(recordsPerPage).ToList();
                }
                if (order == 2)
                {
                    clientsDataGrid.ItemsSource = cl.OrderByDescending(Client => Client.LastName).Skip(start * recordsPerPage).Take(recordsPerPage).ToList();
                }
                if (order == 3)
                {
                    clientsDataGrid.ItemsSource = cl.OrderBy(Client => Client.RegistrationDate).Skip(start * recordsPerPage).Take(recordsPerPage).ToList();
                }
                if (order == 4)
                {
                    clientsDataGrid.ItemsSource = cl.OrderByDescending(Client => Client.RegistrationDate).Skip(start * recordsPerPage).Take(recordsPerPage).ToList();
                }
                if (order == 5)
                {
                    clientsDataGrid.ItemsSource = clientVisits
                        .OrderBy(cv => cv.LastVisitDate)
                        .Skip(start * recordsPerPage)
                        .Take(recordsPerPage)
                        .ToList();
                }
                if (order == 6)
                {
                    clientsDataGrid.ItemsSource = clientVisits
                        .OrderByDescending(cv => cv.LastVisitDate)
                        .Skip(start * recordsPerPage)
                        .Take(recordsPerPage)
                        .ToList();
                }
                // Сортировка клиентов по количеству посещений
                if (order == 7)
                {
                    clientsDataGrid.ItemsSource = clientVisits
                        .OrderBy(cv => cv.VisitCount)
                        .Skip(start * recordsPerPage)
                        .Take(recordsPerPage)
                        .ToList();
                }
                if (order == 8)
                {
                    clientsDataGrid.ItemsSource = clientVisits
                        .OrderByDescending(cv => cv.VisitCount)
                        .Skip(start * recordsPerPage)
                        .Take(recordsPerPage)
                        .ToList();
                }
                full.Text = $"{Math.Min(recordsPerPage, totalRecords)} из {totalRecords} записей";

                // Разбиение на страницы
                int ost = fullCount % recordsPerPage;
                int pag = (fullCount - ost) / recordsPerPage;
                if (ost > 0) pag++;
                pagin.Children.Clear();
                for (int i = 0; i < pag; i++)
                {
                    Button myButton = new Button
                    {
                        Height = 30,
                        Content = i + 1,
                        Width = 20,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Tag = i
                    };
                    myButton.Click += new RoutedEventHandler(paginButto_Click);
                    pagin.Children.Add(myButton);
                }
                turnButton();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
        }


        private void turnButton()
        {
            if (start == 0) { back.IsEnabled = false; }
            else { back.IsEnabled = true; };
            if ((start + 1) * recordsPerPage > fullCount) { forward.IsEnabled = false; }
            else { forward.IsEnabled = true; };
        }
        private void paginButto_Click(object sender, RoutedEventArgs e)
        {
            start = Convert.ToInt32(((Button)sender).Tag.ToString());
            Load();
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            fnd = ((TextBox)sender).Text;
            Load();
        }

        private void FIO_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            ComboBoxItem selectedItem = (ComboBoxItem)comboBox.SelectedItem;

            if (selectedItem.Tag.ToString() == "0")
            {
                order = 0;
                Load();
                return;
            }

            order = Convert.ToInt32(selectedItem.Tag.ToString());
            Load();
        }

        private void Gender_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((Gender)Gender.SelectedItem).Name == "Все полы")
            {
                gnd = "";
            }
            else
            {
                gnd = ((Gender)Gender.SelectedItem).Code;
            }
            Load();
        }

        private void RecordsPerPage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem selectedItem = (ComboBoxItem)RecordsPerPage.SelectedItem;
            if (selectedItem.Tag.ToString() == "-1")
            {
                recordsPerPage = int.MaxValue;
                full.Text = $"{totalRecords} из {totalRecords} записей";
            }
            else
            {
                recordsPerPage = Convert.ToInt32(selectedItem.Tag.ToString());
                full.Text = $"{Math.Min(recordsPerPage, totalRecords)} из {totalRecords} записей";
            }
            Load();
        }

        private void clientsDataGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (clientsDataGrid.SelectedItems.Count > 0)
            {
                Client client = clientsDataGrid.SelectedItems[0] as Client;

                if (client != null)
                {
                    fr.Content = new PageAddClient(client);
                }
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (clientsDataGrid.SelectedItems.Count > 0)
            {
                Client client = clientsDataGrid.SelectedItems[0] as Client;

                if (client != null)
                {
                    fr.Content = new PageAddClient(client);
                }
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (clientsDataGrid.SelectedItems.Count > 0)
            {
                Client client = clientsDataGrid.SelectedItems[0] as Client;

                if (client != null && !CheckClientForVisits(client))
                {
                    MessageBoxResult result = MessageBox.Show($"Удалить клиента {client.FirstName} {client.LastName}?",
                        "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        helper.GetContext().Client.Remove(client);
                        helper.GetContext().SaveChanges();
                        Load();
                        MessageBox.Show("Клиент успешно удален.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }

        private bool CheckClientForVisits(Client client)
        {
            using (var context = new Entities())
            {
                var visits = context.ClientService.Count(cs => cs.ClientID == client.ID);
                if (visits > 0)
                {
                    MessageBox.Show("Удаление клиента невозможно, так как у него есть записи посещений.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return true;
                }
            }
            return false;
        }


        private void forward_Click(object sender, RoutedEventArgs e)
        {
            start++;
            Load();
        }

        private void back_Click(object sender, RoutedEventArgs e)
        {
            start--;
            Load();
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            fr.Content = new PageAddClient(new Client());
        }

        private void updateButton_Click(object sender, RoutedEventArgs e)
        {
            fnd = "";
            gnd = "";
            order = 0;

            Search.Text = string.Empty;

            FIO.SelectedIndex = 0;

            Gender.SelectedIndex = 0;

            RecordsPerPage.SelectedIndex = 0;

            Load();
        }
    }
}
