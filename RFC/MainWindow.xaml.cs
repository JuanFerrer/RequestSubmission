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
        const string FILENAME_DB = "DB.txt";
        const string TEMP_DB = "DB.temp.txt";
        const char SPLIT_CHAR = '|';
        const string KEY = "123";
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

        /// <summary>
        /// Get current data in file
        /// </summary>
        private void RetrieveDB()
        {
            if (File.Exists(FILENAME_DB))
            {
                DecryptDB(FILENAME_DB, TEMP_DB);
                dbContents = File.ReadAllLines(TEMP_DB);
                if (dbContents.Length == 0)
                {
                    bool stop = true;
                }
            }
        }

        /// <summary>
        /// Run cmd with passed arguments
        /// </summary>
        /// <param name="encryptedDB"></param>
        /// <param name="tempDB"></param>
        private void DecryptDB(string encryptedDB, string tempDB)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = @"C:\Windows\System32\cmd.exe";
            startInfo.Arguments = "/C encrypt.exe -d "+ encryptedDB + " " + tempDB + " " + KEY;
            process.StartInfo = startInfo;
            process.Start();
        }

        /// <summary>
        /// Run cmd with passed arguments
        /// </summary>
        /// <param name="encryptedDB"></param>
        /// <param name="tempDB"></param>
        private void EncryptDB(string tempDB, string encryptedDB)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C encrypt.exe -e " + tempDB + " " + encryptedDB + " " + KEY;
            process.StartInfo = startInfo;
            process.Start();
        }

        /// <summary>
        /// Append request to database
        /// </summary>
        /// <param name="sender"></param>
        private void ButtonAction(object sender)
        {
            Button btn = sender as Button;

            if (btn.Content == submitButton.Content)
            {
                string line = authorTextBox.Text + SPLIT_CHAR + requestTextBox.Text + SPLIT_CHAR + priorityComboBox.SelectedIndex;
                File.AppendAllLines(TEMP_DB, new List<string>() { line });
                authorTextBox.Text = requestTextBox.Text = string.Empty;
                UpdateList(line);
            }
            else if (btn.Content == solvedButton.Content)
            {
                RetrieveDB();
                File.Delete(TEMP_DB);
                Item selectedItem = items[itemList.SelectedIndex];
                foreach (string str in dbContents)
                {
                    if (FormatString(str).Request != selectedItem.Request)
                        File.AppendAllLines(TEMP_DB, new List<string>() { str });
                }
                items.RemoveAt(itemList.SelectedIndex);
            }
            SortList();
        }

        /// <summary>
        /// Sorts DB list by priority. Bubble sort, for now
        /// </summary>
        private void SortList()
        {
            IEnumerable<Item> query = items.OrderBy(item => item.Priority);
            foreach (Item item in query)
            {
                items.RemoveAt(0);
                items.Add(item);
            }
        }

        /// <summary>
        /// Read data and format it to present on screen
        /// </summary>
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

        /// <summary>
        /// Adds new item to the list
        /// </summary>
        /// <param name="newItem"></param>
        private void UpdateList(string newItem)
        {
            items.Add(FormatString(newItem));
        }

        /// <summary>
        /// Prepares string for presentation
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private Item FormatString(string str)
        {
            string[] item = str.Split(SPLIT_CHAR);
            return new Item(item[0], item[1], (EPriority)int.Parse(item[2]));
        }

        /// <summary>
        /// Check that the current request is valid
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        private bool ValidRequest(object sender)
        {
            Button btn = sender as Button;
            if (btn.Content == submitButton.Content)
                return authorTextBox.Text != string.Empty && requestTextBox.Text != string.Empty;
            else if (btn.Content == solvedButton.Content)
                return itemList.SelectedIndex >= 0;

            return false;
        }

        /// <summary>
        ///  Make sure the request is valid
        /// </summary>
        /// <param name="sender"></param>
        private void AskForValidRequest(object sender)
        {
            Button btn = sender as Button;
            if (btn.Content == submitButton.Content)
                MessageBox.Show("Please, make sure all fields are filled in.");
            else if (btn.Content == solvedButton.Content)
                MessageBox.Show("Please, select a request to mark as solved");
        }

        /// <summary>
        ///  Event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (ValidRequest(sender))
                ButtonAction(sender);
            else
                AskForValidRequest(sender);
        }

        void DataWindow_Closing(object sender)
        {
            EncryptDB(TEMP_DB, FILENAME_DB);
            File.Delete(TEMP_DB);
        }
    }
}
