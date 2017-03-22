using System.Collections.Generic;
using System.Windows;
using System.IO;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Linq;

namespace RFC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string filenameDB = "DB.txt";
        const char splitChar = '|';
        ObservableCollection<Item> items;
        string[] dbContents;

        public double authorWidth { get; set; }
        public double requestWidth { get; set; }

        public MainWindow()
        {
            authorWidth = 100.0;
            InitializeComponent();
            requestWidth = itemList.Width - authorWidth - 10;

            RetrieveDB();
            PopulateList();
        }

        // Get current data in file
        private void RetrieveDB()
        {
            if (File.Exists(filenameDB))
                dbContents = File.ReadAllLines(filenameDB);
        }

        // Append request to database
        private void ButtonAction(object sender)
        {
            Button btn = sender as Button;

            if (btn.Content == submitButton.Content)
            {
                string line = authorTextBox.Text + splitChar + requestTextBox.Text + splitChar + priorityComboBox.SelectedIndex;
                File.AppendAllLines(filenameDB, new List<string>() { line });
                authorTextBox.Text = requestTextBox.Text = string.Empty;
                UpdateList(line);
            }
            else if (btn.Content == solvedButton.Content)
            {
                RetrieveDB();
                File.Delete(filenameDB);
                Item selectedItem = items[itemList.SelectedIndex];
                foreach (string str in dbContents)
                {
                    if (FormatString(str).Request != selectedItem.Request)
                        File.AppendAllLines(filenameDB, new List<string>() { str });
                }
                items.RemoveAt(itemList.SelectedIndex);
            }
            SortList();
        }

        // Sorts DB list by priority. Bubble sort, for now
        private void SortList()
        {
            IEnumerable<Item> query = items.OrderBy(item => item.Priority);
            foreach (Item item in query)
            {
                items.RemoveAt(0);
                items.Add(item);
            }
        }

        // Read data and format it to present on screen
        private void PopulateList()
        {
            items = new ObservableCollection<Item>();
            if (dbContents != null)
            {
                foreach (string str in dbContents)
                {
                    items.Add(FormatString(str));
                }
            }
            itemList.ItemsSource = items;
            SortList();
        }

        // Adds new item to the list
        private void UpdateList(string newItem)
        {
            items.Add(FormatString(newItem));
        }

        // Prepares string for presentation
        private Item FormatString(string str)
        {
            string[] item = str.Split(splitChar);
            return new Item(item[0], item[1], (EPriority)int.Parse(item[2]));
        }

        // Check that the current request is valid
        private bool ValidRequest(object sender)
        {
            Button btn = sender as Button;
            if (btn.Content == submitButton.Content)
                return authorTextBox.Text != string.Empty && requestTextBox.Text != string.Empty;
            else if (btn.Content == solvedButton.Content)
                return itemList.SelectedIndex >= 0;

            return false;
        }

        private void AskForValidRequest(object sender)
        {
            Button btn = sender as Button;
            if (btn.Content == submitButton.Content)
                MessageBox.Show("Please, make sure all fields are filled in.");
            else if (btn.Content == solvedButton.Content)
                MessageBox.Show("Please, select a request to mark as solved");
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (ValidRequest(sender))
                ButtonAction(sender);
            else
                AskForValidRequest(sender);
        }
    }
}
