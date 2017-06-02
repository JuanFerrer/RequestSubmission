using System.Collections.Generic;
using System.Windows;
using System.IO;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;

namespace RFC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool saveOnClose = true;
        bool promptSureCancel = false;
        string FILENAME_DB = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\RFC\rfc.db";
        const string TEMP_DB = "temp.db";
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
            // Make sure we only allow admin to solve requests
            if (!AdminChecker.IsAdmin())
            {
                solvedButton.Height = 0;
            }
            requestWidth = itemList.Width - authorWidth - 10;

            SetupDBFolder();
            RetrieveDB();
            PopulateList();
            Closing += new System.ComponentModel.CancelEventHandler(OnWindowClosing);
        }

        /// <summary>
        /// 
        /// </summary>
        private void SetupDBFolder()
        {
            if (!Directory.Exists(Path.GetDirectoryName(FILENAME_DB)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FILENAME_DB));
            }
        }

        /// <summary>
        /// Get current data in file
        /// </summary>
        private void RetrieveDB()
        {
            if (File.Exists(FILENAME_DB))
            {
                DecryptDB(FILENAME_DB, TEMP_DB);
                HideFile(TEMP_DB);
                dbContents = File.ReadAllLines(TEMP_DB);
            }
        }

        /// <summary>
        /// Run cmd with passed arguments
        /// </summary>
        /// <param name="encryptedDB"></param>
        /// <param name="tempDB"></param>
        private void EncryptDB(string tempDB, string encryptedDB)
        {
            CallEncrypter("/C encrypt.exe -e " + tempDB + " " + encryptedDB + " " + KEY);
        }

        /// <summary>
        /// Run cmd with passed arguments
        /// </summary>
        /// <param name="encryptedDB"></param>
        /// <param name="tempDB"></param>
        private void DecryptDB(string encryptedDB, string tempDB)
        {
            CallEncrypter("/C encrypt.exe -d " + encryptedDB + " " + tempDB + " " + KEY);
        }

        /// <summary>
        /// Set up process, call and wait for exit
        /// </summary>
        /// <param name="parameters"></param>
        private void CallEncrypter(string parameters)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = @"C:\Windows\System32\cmd.exe";
            startInfo.Arguments = parameters;
            process.StartInfo = startInfo;
            process.Start();
            // Wait until process is finished
            // Can't run asynchronously, because DB might not be ready to be read or written
            process.WaitForExit();
        }

        /// <summary>
        /// Add the hidden attribute to the given file
        /// </summary>
        /// <param name="path"></param>
        void HideFile(string path)
        {
            File.SetAttributes(path, File.GetAttributes(path) | FileAttributes.Hidden);
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
                promptSureCancel = true;
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
                promptSureCancel = true;
            }
            else if (btn.Content == cancelButton.Content)
            {
                MessageBoxResult result;
                if (promptSureCancel)
                {
                    result = MessageBox.Show("Are you sure you want to close? Your changes won't be saved.", "", MessageBoxButton.YesNo);                    
                }
                else
                {


                }
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
            else if (btn.Content == cancelButton.Content)
                return true;

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

        /// <summary>
        /// React to closing window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Closing correctly");
            if (saveOnClose)
            {
                EncryptDB(TEMP_DB, FILENAME_DB);
            }
            File.Delete(TEMP_DB);
        }
    }
}
