using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace MyFileManager
{
    public partial class AttributeForm : Form
    {
        public AttributeForm(string filePath)
        {
            InitializeComponent();

            // Инициализация интерфейса
            InitDisplay(filePath);
        }

        // Закройте диалоговое окно
        private void btnConfirm_Click(object sender, EventArgs e)
        {
            this.Close();
        }








        // Инициализация интерфейса
        private void InitDisplay(string filePath)
        {
            // Если путь к файлу-это путь к файлу
            if (File.Exists(filePath))
            {
                FileInfo fileInfo = new FileInfo(filePath);

                txtFileName.Text = fileInfo.Name;
                txtFileType.Text = fileInfo.Extension;
                txtFileLocation.Text = (fileInfo.DirectoryName != null) ? fileInfo.DirectoryName : null;
                txtFileSize.Text = ShowFileSize(fileInfo.Length);
                txtFileCreateTime.Text = fileInfo.CreationTime.ToString();
                txtFileModifyTime.Text = fileInfo.LastWriteTime.ToString();
                txtFileAccessTime.Text = fileInfo.LastAccessTime.ToString();
            }
            // Если путь к файлу-это путь к папке
            else if (Directory.Exists(filePath))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(filePath);

                txtFileName.Text = directoryInfo.Name;
                txtFileType.Text = "папка";
                txtFileLocation.Text = (directoryInfo.Parent != null) ? directoryInfo.Parent.FullName : null;
                txtFileSize.Text = ShowFileSize(GetDirectoryLength(filePath));
                txtFileCreateTime.Text = directoryInfo.CreationTime.ToString();
                txtFileModifyTime.Text = directoryInfo.LastWriteTime.ToString();
                txtFileAccessTime.Text = directoryInfo.LastAccessTime.ToString();
            }
        }


        // Получить размер каталога
        private long GetDirectoryLength(string dirPath)
        {
            long length = 0;
            DirectoryInfo directoryInfo = new DirectoryInfo(dirPath);


            // Получить размер всех файлов в каталоге
            FileInfo[] fileInfos = directoryInfo.GetFiles();
            if (fileInfos.Length > 0)
            {
                foreach (FileInfo fileInfo in fileInfos)
                {
                    length += fileInfo.Length;
                }
            }


            // Рекурсивно получить размер всех папок в каталоге
            DirectoryInfo[] directoryInfos = directoryInfo.GetDirectories();
            if(directoryInfos.Length > 0)
            {
                foreach(DirectoryInfo dirInfo in directoryInfos)
                {
                    length += GetDirectoryLength(dirInfo.FullName);
                }
            }

            return length;
        }


        public static string ShowFileSize(long fileSize)
        {
            string fileSizeStr = "";

            if (fileSize < 1024)
            {
                fileSizeStr = fileSize + "байт";
            }
            else if (fileSize >= 1024 && fileSize < 1024 * 1024)
            {
                fileSizeStr = Math.Round(fileSize * 1.0 / 1024, 2, MidpointRounding.AwayFromZero) + " KB(" + fileSize + "байт)";
            }
            else if (fileSize >= 1024 * 1024 && fileSize < 1024 * 1024 * 1024)
            {
                fileSizeStr = Math.Round(fileSize * 1.0 / (1024 * 1024), 2, MidpointRounding.AwayFromZero) + " MB(" + fileSize + "байт)";
            }
            else if (fileSize >= 1024 * 1024 * 1024)
            {
                fileSizeStr = Math.Round(fileSize * 1.0 / (1024 * 1024 * 1024), 2, MidpointRounding.AwayFromZero) + " GB(" + fileSize + "байт)";
            }

            return fileSizeStr;
        }

        private void AttributeForm_Load(object sender, EventArgs e)
        {

        }
    }
}
