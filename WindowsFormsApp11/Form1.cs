using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace WindowsFormsApp11
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady)
                {
                    comboBox1.Items.Add(drive.Name);
                }
            }

            // Настроим ListView для отображения найденных файлов и папок
            listView1.View = View.Details;
            listView1.Columns.Add("Name", -2, HorizontalAlignment.Left);
            listView1.Columns.Add("Type", -2, HorizontalAlignment.Left);
            listView1.Columns.Add("Size", -2, HorizontalAlignment.Left);
            listView1.Columns.Add("Modified", -2, HorizontalAlignment.Left);

            // Настроим BackgroundWorker для выполнения поиска файлов и папок в отдельном потоке
            worker = new BackgroundWorker();
            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Проверим, что выбран диск и введена маска поиска
            if (string.IsNullOrEmpty(comboBox1.Text) || string.IsNullOrEmpty(maskedTextBox1.Text))
            {
                MessageBox.Show("Please select a drive and enter a search mask.");
                return;
            }

            // Очистим список результатов поиска
            listView1.Items.Clear();

            // Запустим BackgroundWorker для выполнения поиска
            worker.RunWorkerAsync(new SearchParams(comboBox1.Text, maskedTextBox1.Text));
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            // Получим параметры поиска из аргументов BackgroundWorker
            var searchParams = (SearchParams)e.Argument;

            // Выполним поиск файлов и папок
            var foundItems = SearchFilesAndFolders(searchParams.Drive, searchParams.Mask);

            // Сохраним результаты поиска в аргументах BackgroundWorker
            e.Result = foundItems;
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // Получим результаты поиска из аргументов BackgroundWorker
            var foundItems = (SearchResult)e.Result;

            // Отобразим результаты поиска в ListView
            foreach (var item in foundItems)
            {
                var listViewItem = new ListViewItem(item.Name);
                listViewItem.SubItems.Add(item.IsFolder ? "Folder" : "File");
                listViewItem.SubItems.Add(item.Size.ToString());
                listViewItem.SubItems.Add(item.Modified.ToString());
                listView1.Items.Add(listViewItem);
            }
        }

        private SearchResult SearchFilesAndFolders(string driveName, string mask)
        {
            var foundItems = new SearchResult();

                // Используем DirectoryInfo для получения списка папок и файлов
                var rootDirectory = new DirectoryInfo(driveName);
                var directories = rootDirectory.GetDirectories("*", SearchOption.AllDirectories);
                var files = rootDirectory.GetFiles("*", SearchOption.AllDirectories);
        }
        private void btnSearch_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            string mask = maskedTextBox1.Text;
            string path = (string)cbDrives.SelectedItem + "\\";
            if (!string.IsNullOrWhiteSpace(mask) && Directory.Exists(path))
            {
                var thread = new Thread(() => Search(mask, path));
                thread.Start();
            }
        }
        private void MainForm_Load(object sender, EventArgs e)
        {
            comboBox1.Items.AddRange(Directory.GetLogicalDrives());
            comboBox1.SelectedIndex = 0;
        }
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (var thread in threads)
            {
                if (thread.IsAlive)
                {
                    thread.Abort();
                }
            }
        }

    }
}
