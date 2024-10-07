using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Validation;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using Microsoft.Win32;

namespace WpfAutoService.Pages
{
    /// <summary>
    /// Логика взаимодействия для PageAddClient.xaml
    /// </summary>
    public partial class PageAddClient : Page
    {
        private Client client;
        private int curSelPr = 0;
        private int curTypAg = 0;

        public PageAddClient(Client Сlient)
        {
            InitializeComponent();
            LoadGenders();
            LoadServices();
            LoadTags();

            if (Сlient != null)
            {
                client = Сlient;
                Gender.SelectedItem = Сlient.Gender;
                this.FirstName.Text = Сlient.FirstName;
                this.LastName.Text = Сlient.LastName;
                this.Patronymic.Text = Сlient.Patronymic;
                this.Birthday.SelectedDate = Сlient.Birthday;
                this.RegistrationDate.SelectedDate = Сlient.RegistrationDate;
                this.Email.Text = Сlient.Email;
                this.Phone.Text = Сlient.Phone;
                client.PhotoPath = Сlient.PhotoPath; // Добавлено: загрузка пути к фото
                historyGrid.ItemsSource = helper.GetContext().ClientService.Where(ClientService => ClientService.ClientID == Сlient.ID).ToList();

                if (!string.IsNullOrEmpty(client.PhotoPath))
                {
                    string fullPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "autoded_VP-main", client.PhotoPath.TrimStart('\\'));

                    // Проверяем наличие файла по указанному пути
                    if (System.IO.File.Exists(fullPath))
                    {
                        ClientPhoto.Source = new BitmapImage(new Uri(fullPath));
                    }
                    else
                    {
                        // Если файл не найден, используем путь из базы данных
                        fullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, client.PhotoPath.TrimStart('\\'));
                        if (System.IO.File.Exists(fullPath))
                        {
                            ClientPhoto.Source = new BitmapImage(new Uri(fullPath));
                        }
                        else
                        {
                            // Вы можете добавить здесь код, чтобы отобразить заглушку или уведомление о том, что изображение не найдено
                            MessageBox.Show("Изображение не найдено.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }

                TagComboBox.SelectedItem = client.Tag; // Выбор тега для редактирования
            }
            else
            {
                client = new Client();
                btnDelClient.IsEnabled = false;
                btnWriteClient.IsEnabled = false;
            }
            this.DataContext = client;
        }

        private void LoadServices()
        {
            try
            {
                ServiceComboBox.ItemsSource = helper.GetContext().Service.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки услуг: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadGenders()
        {
            try
            {
                Gender.ItemsSource = helper.GetContext().Gender.ToList();
            }
            catch { };
        }

        private void LoadTags()
        {
            try
            {
                TagComboBox.ItemsSource = helper.GetContext().Tag.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки тегов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void Gender_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            client.GenderCode = ((Gender)Gender.SelectedItem).Code;
        }

        private void TagComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            client.SelectedTagID = (int?)TagComboBox.SelectedValue;
            OnPropertyChanged("SelectedTagID"); // Уведомление об изменении свойства
        }



        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private void btnWriteClient_Click(object sender, RoutedEventArgs e)
        {
            if (helper.GetContext() == null)
            {
                MessageBox.Show("Нет подключения к базе данных. Пожалуйста, убедитесь, что вы подключены к рабочей базе данных.");
                return;
            }

            if (string.IsNullOrWhiteSpace(this.FirstName.Text))
            {
                MessageBox.Show("Введите имя клиента.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(this.LastName.Text))
            {
                MessageBox.Show("Введите фамилию клиента.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(this.Patronymic.Text))
            {
                MessageBox.Show("Введите отчество клиента.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!Regex.IsMatch(this.Phone.Text, @"^\+?\d{0,2}\-?\d{3}\-?\d{3}\-?\d{4}"))
            {
                MessageBox.Show("Введите корректный номер телефона.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!string.IsNullOrWhiteSpace(this.Email.Text) && !Regex.IsMatch(this.Email.Text, @"(\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*)"))
            {
                MessageBox.Show("Введите корректный адрес электронной почты.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            client.FirstName = this.FirstName.Text;
            client.LastName = this.LastName.Text;
            client.Patronymic = this.Patronymic.Text;
            client.Birthday = this.Birthday.SelectedDate;
            client.RegistrationDate = this.RegistrationDate.SelectedDate.Value;
            client.Email = this.Email.Text;
            client.Phone = this.Phone.Text;
            client.GenderCode = ((Gender)Gender.SelectedItem).Code;
          
            // Путь к фото уже сохранен в client.PhotoPath при выборе фото
            if (this.TagComboBox.SelectedItem != null) // Предполагается, что TagComboBox - это ComboBox для тегов
            {
                var selectedTag = (Tag)this.TagComboBox.SelectedItem; // Приводим выбранный элемент к типу Tag
                client.IDTag = selectedTag.IDTag; // Предполагается, что у Tag есть свойство ID
            }
            else
            {
                client.IDTag = null; // Если тег не выбран, устанавливаем IDTag в null
            }

            try
            {
                if (client.ID > 0)
                {
                    helper.GetContext().Entry(client).State = EntityState.Modified;
                    helper.GetContext().SaveChanges();
                    MessageBox.Show("Информация о клиенте успешно обновлена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    helper.GetContext().Client.Add(client);
                    helper.GetContext().SaveChanges();
                    MessageBox.Show("Информация о клиенте успешно добавлена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (DbEntityValidationException ex)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var error in ex.EntityValidationErrors)
                {
                    foreach (var errorMessage in error.ValidationErrors)
                    {
                        sb.AppendLine($"- {errorMessage.ErrorMessage}");
                    }
                }
                MessageBox.Show($"Произошла ошибка валидации сущности:\n{sb.ToString()}", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            btnDelClient.IsEnabled = true;
            btnWriteClient.IsEnabled = true;
        }

        private void btnChoosePhotoClient_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                client.PhotoPath = openFileDialog.FileName;
                ClientPhoto.Source = new BitmapImage(new Uri(client.PhotoPath));
            }
        }

        private void historyGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void AddVisiting_Click(object sender, RoutedEventArgs e)
        {
            if (VisitDate.SelectedDate.HasValue && !string.IsNullOrWhiteSpace(Comment.Text) && ServiceComboBox.SelectedItem != null)
            {
                var newVisit = new ClientService
                {
                    ClientID = client.ID,
                    StartTime = VisitDate.SelectedDate.Value,
                    Comment = Comment.Text,
                    ServiceID = (int)ServiceComboBox.SelectedValue
                };

                try
                {
                    helper.GetContext().ClientService.Add(newVisit);
                    helper.GetContext().SaveChanges();
                    historyGrid.ItemsSource = helper.GetContext().ClientService.Where(cs => cs.ClientID == client.ID).ToList();
                    MessageBox.Show("Посещение успешно добавлено.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при добавлении посещения:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, заполните все поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DeleteVisiting_Click(object sender, RoutedEventArgs e)
        {
            if (historyGrid.SelectedItem != null)
            {
                var selectedVisit = (ClientService)historyGrid.SelectedItem;

                MessageBoxResult result = MessageBox.Show($"Вы действительно хотите удалить посещение от {selectedVisit.StartTime.ToShortDateString()}?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        helper.GetContext().ClientService.Remove(selectedVisit);
                        helper.GetContext().SaveChanges();
                        historyGrid.ItemsSource = helper.GetContext().ClientService.Where(cs => cs.ClientID == client.ID).ToList();
                        MessageBox.Show("Посещение успешно удалено.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении посещения:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите посещение для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnDelClient_Click(object sender, RoutedEventArgs e)
        {
            if (client.ClientService.Count > 0)
            {
                MessageBox.Show("Удаление невозможно! У клиента есть связанные услуги.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            helper.GetContext().Client.Remove(client);
            helper.GetContext().SaveChanges();
            MessageBox.Show("Удаление информации о клиенте завершено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            this.NavigationService.GoBack();
        }
    }
}
