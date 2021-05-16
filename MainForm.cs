using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using Microsoft.VisualBasic.Devices;
using System.Threading;
using System.Security.AccessControl;

namespace MyFileManager
{
    public partial class MainForm : Form
    {
        // Текущий путь
        private string curFilePath = "";

        // Текущий выбранный узел дерева (узел каталога)
        private TreeNode curSelectedNode = null;

        // Следует ли переместить файл
        private bool isMove = false;

        // Исходный путь к файлу \ папке, которую нужно скопировать и вставить
        private string[] copyFilesSourcePaths = new string[200];

        // Первый узел пути исторического пути доступа пользователя
        private DoublyLinkedListNode firstPathNode = new DoublyLinkedListNode();

        //Текущий узел пути
        private DoublyLinkedListNode curPathNode = null;

        //Имя файла для поиска
        private string fileName;

        //Следует ли инициализировать в первый раз tvwDirectory
        private bool isInitializeTvwDirectory = true;

        public MainForm()
        {
            InitializeComponent();
        }



        private void MainForm_Load(object sender, EventArgs e)
        {
            //Инициализация связанных параметров представления
            InitViewChecks();

            //Инициализация отображения интерфейса диспетчера
            InitDisplay();
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            //При изменении размера формы изменяется длина адресной строки
            tscboAddress.Width = this.Width - 290;
        }

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            //При изменении размера формы изменяется длина адресной строки
            tscboAddress.Width = this.Width - 290;
        }


        private void tsmiNewFolder_Click(object sender, EventArgs e)
        {
            //Создайте новую папку
            CreateFolder();
        }

        private void tsmiNewFile_Click(object sender, EventArgs e)
        {
            //Создайте новый файл
            CreateFile();
        }

        private void tsmiPrivilege_Click(object sender, EventArgs e)
        {
            //Отображение окна Управления правами
            ShowPrivilegeForm();
        }

        private void tsmiProperties_Click(object sender, EventArgs e)
        {
            // Показать окно свойств
            ShowAttributeForm();
        }





        private void tsmiCopy_Click(object sender, EventArgs e)
        {
            
            CopyFiles();
        }

        private void tsmiPaste_Click(object sender, EventArgs e)
        {
            
            PasteFiles();
        }

        private void tsmiCut_Click(object sender, EventArgs e)
        {
            
            CutFiles();
        }

        private void tsmiDelete_Click(object sender, EventArgs e)
        {
            DeleteFiles();
        }

       


        private void tsmiToolbar_Click(object sender, EventArgs e)
        {
            // Установите, видна ли панель инструментов
            tsMain.Visible = !tsMain.Visible;

            tsmiToolbar.Checked = !tsmiToolbar.Checked;
        }

        private void tsmiStatusbar_Click(object sender, EventArgs e)
        {
            // Установите, видна ли строка состояния
            ssFooter.Visible = !ssFooter.Visible;

            tsmiStatusbar.Checked = !tsmiStatusbar.Checked;
        }

        private void tsmiBigIcon_Click(object sender, EventArgs e)
        {
            ResetViewChecks();
            tsmiBigIcon.Checked = true;
            tsmiBigIcon1.Checked = true;
            lvwFiles.View = View.LargeIcon;
        }

        private void tsmiSmallIcon_Click(object sender, EventArgs e)
        {
            ResetViewChecks();
            tsmiSmallIcon.Checked = true;
            tsmiSmallIcon1.Checked = true;
            lvwFiles.View = View.SmallIcon;
        }

        private void tsmiList_Click(object sender, EventArgs e)
        {
            ResetViewChecks();
            tsmiList.Checked = true;
            tsmiList1.Checked = true;
            lvwFiles.View = View.List;
        }

        private void tsmiDetailedInfo_Click(object sender, EventArgs e)
        {
            ResetViewChecks();
            tsmiDetailedInfo.Checked = true;
            tsmiDetailedInfo1.Checked = true;
            lvwFiles.View = View.Details;
        }

        private void tsmiRefresh_Click(object sender, EventArgs e)
        {
            ShowFilesList(curFilePath, false);
        }




       

        




        



        // Примечание: В обратной и прямой логике не создаются новые узлы пути, но на основе существующих узлов пути (ссылки）

        //отходить
        private void tsbtnBack_Click(object sender, EventArgs e)
        {
            if (curPathNode != firstPathNode)
            {
                curPathNode = curPathNode.PreNode;
                string prePath = curPathNode.Path;

                ShowFilesList(prePath, false);

                //Доступна кнопка вперед
                tsbtnAdvance.Enabled = true;
            }
            else
            {
                // Кнопка "Назад" недоступна
                tsbtnBack.Enabled = false;
            }
        }

        //вперед
        private void tsbtnAdvance_Click(object sender, EventArgs e)
        {
            if (curPathNode.NextNode != null)
            {
                curPathNode = curPathNode.NextNode;
                string nextPath = curPathNode.Path;

                ShowFilesList(nextPath, false);

                //Кнопка Назад доступна
                tsbtnBack.Enabled = true;
            }
            else
            {
                //Кнопка вперед недоступна
                tsbtnAdvance.Enabled = false;
            }
        }

        //Каталог на один уровень вверх
        private void tsbtnUpArrow_Click(object sender, EventArgs e)
        {
            if (curFilePath == "")
            {
                return;
            }

            DirectoryInfo directoryInfo = new DirectoryInfo(curFilePath);

            // Корневые каталоги, такие как: C: \, D: \, E: \ и т. Д.
            // Если вы не достигли корневого каталога
            if (directoryInfo.Parent != null)
            {
                ShowFilesList(directoryInfo.Parent.FullName, true);
            }
            // Достигли корневого каталога, остановимся
            else
            {
                return;
            }
        }




        private void tscboAddress_KeyDown(object sender, KeyEventArgs e)
        {
            // Введите новый адрес
            if (e.KeyCode == Keys.Enter)
            {
                string newPath = tscboAddress.Text;

                if (newPath == "")
                {
                    return;
                }
                else if (!Directory.Exists(newPath))
                {
                    return;
                }

                ShowFilesList(newPath, true);
            }
        }




        

       

        private void tscboSearch_KeyDown(object sender, KeyEventArgs e)
        {
            //Введите имя файла
            if (e.KeyCode == Keys.Enter)
            {
                

                if (string.IsNullOrEmpty(fileName))
                {
                    return;
                }

                //Используйте несколько потоков для поиска файлов / папок
                SearchWithMultiThread(curFilePath, fileName);
            }
        }





        //Когда контекстное меню открыто
        private void cmsMain_Opening(object sender, CancelEventArgs e)
        {
            //Преобразование полученных текущих координат мыши (преобразование экранных координат в координаты рабочей области)
            Point curPoint = lvwFiles.PointToClient(Cursor.Position);

            // Получить координаты ListViewItem
            ListViewItem item = lvwFiles.GetItemAt(curPoint.X, curPoint.Y);

            //Текущая позиция имеет ListViewItem
            if (item != null)
            {
                tsmiOpen.Visible = true;
                tsmiView1.Visible = false;
                tsmiRefresh1.Visible = false;
                tsmiCopy1.Visible = true;
                tsmiPaste1.Visible = false;
                tsmiCut1.Visible = true;
                tsmiDelete1.Visible = true;
                tsmiRename.Visible = true;
                tsmiNewFolder1.Visible = false;
                tsmiNewFile1.Visible = false;
                tssLine4.Visible = false;
            }
            //Нет текущего местоположения ListViewItem
            else
            {
                tsmiOpen.Visible = false;
                tsmiView1.Visible = true;
                tsmiRefresh1.Visible = true;
                tsmiCopy1.Visible = false;
                tsmiPaste1.Visible = true;
                tsmiCut1.Visible = false;
                tsmiDelete1.Visible = false;
                tsmiRename.Visible = false;
                tsmiNewFolder1.Visible = true;
                tsmiNewFile1.Visible = true;
                tssLine4.Visible = true;
            }
        }

        private void tsmiOpen_Click(object sender, EventArgs e)
        {
            Open();
        }

        private void tsmiRename_Click(object sender, EventArgs e)
        {
            // Переименование файла
            RenameFile();
        }

        private void lvwFiles_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            string newName = e.Label;
            // Выбранный элемент
            ListViewItem selectedItem = lvwFiles.SelectedItems[0];

            //Если имя пустое
            if (string.IsNullOrEmpty(newName))
            {
                MessageBox.Show("Имя файла не может быть пустым! ", "ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);

                //Когда отображается, восстановить исходную метку
                e.CancelEdit = true;
            }
            // Тег не изменился
            else if (newName == null)
            {
                return;
            }
            // Метка была изменена, но в итоге она осталась прежней
            else if (newName == selectedItem.Text)
            {
                return;
            }
            // Имя файла недействительно
            else if (!IsValidFileName(newName))
            {
                MessageBox.Show("Имя файла не может содержать следующие символы::\r\n" + "\t\\/:*?\"<>|", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // При отображении восстанавливаем исходную метку
                e.CancelEdit = true;
            }
            else
            {
                Computer myComputer = new Computer();

                // Если это файл
                if (File.Exists(selectedItem.Tag.ToString()))
                {
                    //Если в текущем пути есть файл с таким же именем
                    if (File.Exists(Path.Combine(curFilePath, newName)))
                    {
                        MessageBox.Show("В текущем пути есть файл с таким же именем！", "ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        
                        // При отображении восстанавливаем исходную метку
                        e.CancelEdit = true;
                    }
                    else
                    {
                        myComputer.FileSystem.RenameFile(selectedItem.Tag.ToString(), newName);

                        FileInfo fileInfo = new FileInfo(selectedItem.Tag.ToString());
                        string parentPath = Path.GetDirectoryName(fileInfo.FullName);
                        string newPath = Path.Combine(parentPath, newName);

                        // Обновляем выбранный элементTag
                        selectedItem.Tag = newPath;

                        //Обновите дерево каталогов слева
                        LoadChildNodes(curSelectedNode);
                    }
                }
                //Если это папка
                else if (Directory.Exists(selectedItem.Tag.ToString()))
                {
                    //Если в текущем пути есть папка с таким же именем
                    if (Directory.Exists(Path.Combine(curFilePath, newName)))
                    {
                        MessageBox.Show("в текущем пути есть папка с таким же именем", "ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        // При отображении восстанавливаем исходную метку
                        e.CancelEdit = true;
                    }
                    else
                    {
                        myComputer.FileSystem.RenameDirectory(selectedItem.Tag.ToString(), newName);

                        DirectoryInfo directoryInfo = new DirectoryInfo(selectedItem.Tag.ToString());
                        string parentPath = directoryInfo.Parent.FullName;
                        string newPath = Path.Combine(parentPath, newName);

                        // Обновляем тег выбранного элемента
                        selectedItem.Tag = newPath;

                        // Обновить дерево каталогов слева
                        LoadChildNodes(curSelectedNode);
                    }
                }
            }
        }




        // Активируем событие (действие активации по умолчанию - "двойной щелчок")
        private void lvwFiles_ItemActivate(object sender, EventArgs e)
        {
            Open();
        }


        //TreeView По умолчанию идет процесс получения фокуса, по умолчанию выбирается верхний узел, то есть индекс равен 0, что является «недавно посещенным» узлом. Позвоню
        // Эта функция приводит к тому, что представление списка файлов справа становится представлением списка файлов "недавно открывавшихся".
        private void tvwDirectory_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // Первая инициализация tvwDirectory
            if (isInitializeTvwDirectory)
            {
                curFilePath = @"Недавние Визиты";
                tscboAddress.Text = curFilePath;

                // Сохраните первый путь к истории пользователя
                firstPathNode.Path = curFilePath;
                curPathNode = firstPathNode;

                curSelectedNode = e.Node;

                //В правой форме отображается список”Недавно просмотренных " файлов
                ShowFilesList(curFilePath, true);

                isInitializeTvwDirectory = false;
            }
            else
            {
                curSelectedNode = e.Node;
                ShowFilesList(e.Node.Tag.ToString(), true);
            }
           
        }

        private void tvwDirectory_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            //Загрузите дочерние узлы выбранного узла до того, как выбранный узел развернется
            LoadChildNodes(e.Node);
        }

        private void tvwDirectory_AfterExpand(object sender, TreeViewEventArgs e)
        {
            //После того, как выбранный узел будет развернут, разверните узел
            e.Node.Expand();
        }











        //Значок области каталога слева от формы хранения находится в ImageList（в ilstDirectoryIcons）В индексе
        private class IconsIndexes
        {
            public const int FixedDrive = 0; // Фиксированный диск
            public const int CDRom = 1; // оптический привод
            public const int RemovableDisk = 2; //съемный диск
            public const int Folder = 3; // значок папки
            public const int RecentFiles = 4; // последние посещения
        }


        // Класс узла двусвязного списка (используется для хранения исторического пути доступа пользователя)
        class DoublyLinkedListNode
        {
            //Сохраненные пути
            public string Path { set; get; }
            public DoublyLinkedListNode PreNode { set; get; }
            public DoublyLinkedListNode NextNode { set; get; }

        }



        // Отображение интерфейса диспетчера инициализации(представление дерева дисков для левой формы инициализации и представление списка файлов для правой формы）
        private void InitDisplay()
        {
            tvwDirectory.Nodes.Clear();

            TreeNode recentFilesNode = tvwDirectory.Nodes.Add("Недавно посещенный");
            recentFilesNode.Tag = "Недавно посещенный";
            recentFilesNode.ImageIndex = IconsIndexes.RecentFiles;
            recentFilesNode.SelectedImageIndex = IconsIndexes.RecentFiles;


            DriveInfo[] driveInfos = DriveInfo.GetDrives();

            foreach (DriveInfo info in driveInfos)
            {
                TreeNode driveNode = null;

                switch (info.DriveType)
                {

                    //Фиксированный диск
                    case DriveType.Fixed:

                        //Отображаемое имя
                        driveNode = tvwDirectory.Nodes.Add("Локальный диск(" + info.Name.Split('\\')[0] + ")");

                        // Реальный путь
                        driveNode.Tag = info.Name;

                        driveNode.ImageIndex = IconsIndexes.FixedDrive;
                        driveNode.SelectedImageIndex = IconsIndexes.FixedDrive;

                        break;
                    // CD-привод
                    case DriveType.CDRom:

                        // Отображаемое имя
                        driveNode = tvwDirectory.Nodes.Add("Оптический привод(" + info.Name.Split('\\')[0] + ")");

                        // Реальный путь
                        driveNode.Tag = info.Name;

                        driveNode.ImageIndex = IconsIndexes.CDRom;
                        driveNode.SelectedImageIndex = IconsIndexes.CDRom;

                        break;

                    // Съемный диск
                    case DriveType.Removable:

                        //Отображаемое имя
                        driveNode = tvwDirectory.Nodes.Add("Съемные диски(" + info.Name.Split('\\')[0] + ")");

                        //Реальный путь
                        driveNode.Tag = info.Name;

                        driveNode.ImageIndex = IconsIndexes.RemovableDisk;
                        driveNode.SelectedImageIndex = IconsIndexes.RemovableDisk;

                        break;
                }
            }

            //Загрузите подкаталоги под каждым диском
            foreach (TreeNode node in tvwDirectory.Nodes)
            {
                LoadChildNodes(node);
            }


            // Где отображение представления списка файлов правильной формы фактически вызывается по умолчанию при инициализации каталога TVW
            // tvwDirectory_AfterSelect функция,не нужно писать дополнительный код здесь
        }



        // Загрузить дочерние узлы (загрузить подкаталоги в текущем каталоге）
        private void LoadChildNodes(TreeNode node)
        {
            try
            {
                // Очистите пустые узлы перед загрузкой дочерних узлов
                node.Nodes.Clear();

                if (node.Tag.ToString() == "Недавние Визиты")
                {
                    return;
                }
                else
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(node.Tag.ToString());
                    DirectoryInfo[] directoryInfos = directoryInfo.GetDirectories();

                    foreach (DirectoryInfo info in directoryInfos)
                    {
                        // Имя дисплея
                        TreeNode childNode = node.Nodes.Add(info.Name);

                        //Реальный путь
                        childNode.Tag = info.FullName;

                        childNode.ImageIndex = IconsIndexes.Folder;
                        childNode.SelectedImageIndex = IconsIndexes.Folder;

                        //Загрузите пустой узел, чтобы реализовать знак " +" 
                        childNode.Nodes.Add("");
                    }
                }

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        //Отображает все файлы/папки по указанному пути в правильной форме
        public void ShowFilesList(string path, bool isRecord)
        {
            //Кнопка Назад доступна
            tsbtnBack.Enabled = true;

            //Если вам нужно сохранить запись, вам нужно создать новый узел пути
            if (isRecord)
            {
                //Сохраните исторический путь доступа пользователя
                DoublyLinkedListNode newNode = new DoublyLinkedListNode();
                newNode.Path = path;
                curPathNode.NextNode = newNode;
                newNode.PreNode = curPathNode;

                curPathNode = newNode;
            }


            // Начать обновление данных
            lvwFiles.BeginUpdate();

            //очищать lvwFiles
            lvwFiles.Items.Clear();

            if (path == "Недавние Визиты")
            {
                //Возвращает коллекцию перечислений путей к последним используемым файлам
                var recentFiles = RecentFilesUtil.GetRecentFiles();

                foreach (string file in recentFiles)
                {
                    if (File.Exists(file))
                    {
                        FileInfo fileInfo = new FileInfo(file);

                        ListViewItem item = lvwFiles.Items.Add(fileInfo.Name);

                        //для exe-файлов или без расширения
                        if (fileInfo.Extension == ".exe" || fileInfo.Extension == "")
                        {
                            //Получить соответствующий значок файла через текущую систему
                            Icon fileIcon = GetSystemIcon.GetIconByFileName(fileInfo.FullName);

                            //Поскольку разные exe-файлы,как правило, имеют разные значки, поэтому вы не можете получить доступ к значку по расширению, вы должны получить доступ к значку по имени файла
                            ilstIcons.Images.Add(fileInfo.Name, fileIcon);

                            item.ImageKey = fileInfo.Name;
                        }
                        //Другие документы
                        else
                        {
                            if (!ilstIcons.Images.ContainsKey(fileInfo.Extension))
                            {
                                Icon fileIcon = GetSystemIcon.GetIconByFileName(fileInfo.FullName);

                                //Из-за типа（кроме exe）Тот же файл, тот же значок,так что вы можете получить доступ к значку по расширению
                                ilstIcons.Images.Add(fileInfo.Extension, fileIcon);
                            }

                            item.ImageKey = fileInfo.Extension;
                        }

                        item.Tag = fileInfo.FullName;
                        item.SubItems.Add(fileInfo.LastWriteTime.ToString());
                        item.SubItems.Add(fileInfo.Extension + "документ");
                        
                    }
                    else if (Directory.Exists(file))
                    {
                        DirectoryInfo dirInfo = new DirectoryInfo(file);

                        ListViewItem item = lvwFiles.Items.Add(dirInfo.Name, IconsIndexes.Folder);
                        item.Tag = dirInfo.FullName;
                        item.SubItems.Add(dirInfo.LastWriteTime.ToString());
                        item.SubItems.Add("папка");
                        item.SubItems.Add("");
                    }
                }
            }
            else
            {
                try
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(path);
                    DirectoryInfo[] directoryInfos = directoryInfo.GetDirectories();
                    FileInfo[] fileInfos = directoryInfo.GetFiles();

                    //удалить ilstIcons(ImageList) в exe Значок файла, освобожденного ilstIcons Пространство
                    foreach (ListViewItem item in lvwFiles.Items)
                    {
                        if (item.Text.EndsWith(".exe"))
                        {
                            ilstIcons.Images.RemoveByKey(item.Text);
                        }
                    }



                    //Список всех папок
                    foreach (DirectoryInfo dirInfo in directoryInfos)
                    {
                        ListViewItem item = lvwFiles.Items.Add(dirInfo.Name, IconsIndexes.Folder);
                        item.Tag = dirInfo.FullName;
                        item.SubItems.Add(dirInfo.LastWriteTime.ToString());
                        item.SubItems.Add("папка");
                        item.SubItems.Add("");
                    }

                    //Список всех файлов
                    foreach (FileInfo fileInfo in fileInfos)
                    {
                        ListViewItem item = lvwFiles.Items.Add(fileInfo.Name);

                        //для exe-файлов или без расширения
                        if (fileInfo.Extension == ".exe" || fileInfo.Extension == "")
                        {
                            //Получить соответствующий значок файла через текущую систему
                            Icon fileIcon = GetSystemIcon.GetIconByFileName(fileInfo.FullName);

                            //Поскольку разные exe-файлы,как правило, имеют разные значки, поэтому вы не можете получить доступ к значку по расширению, вы должны получить доступ к значку по имени файла
                            ilstIcons.Images.Add(fileInfo.Name, fileIcon);

                            item.ImageKey = fileInfo.Name;
                        }
                        //Другие документы
                        else
                        {
                            if (!ilstIcons.Images.ContainsKey(fileInfo.Extension))
                            {
                                Icon fileIcon = GetSystemIcon.GetIconByFileName(fileInfo.FullName);

                                //Поскольку файл того же типа (кроме exe), значок тот же, поэтому вы можете получить доступ к значку по расширению
                                ilstIcons.Images.Add(fileInfo.Extension, fileIcon);
                            }

                            item.ImageKey = fileInfo.Extension;
                        }

                        item.Tag = fileInfo.FullName;
                        item.SubItems.Add(fileInfo.LastWriteTime.ToString());
                        item.SubItems.Add(fileInfo.Extension + "документ");
                       }

                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }

            // Обновить текущий путь
            curFilePath = path;

            // Обновить адресную строку
            tscboAddress.Text = curFilePath;

            // Строка состояния обновления
            tsslblFilesNum.Text = lvwFiles.Items.Count + " Проекты";

            //Конечное обновление данных
            lvwFiles.EndUpdate();
        }



        //Проверьте,является ли имя файла законным, имя файла не может содержать символ\/:*?"<>|
        private bool IsValidFileName(string fileName)
        {
            bool isValid = true;

            // Незаконные символы
            string errChar = "\\/:*?\"<>|";

            for (int i = 0; i < errChar.Length; i++)
            {
                if (fileName.Contains(errChar[i].ToString()))
                {
                    isValid = false;
                    break;
                }
            }

            return isValid;
        }



        // Показать окно свойств
        private void ShowAttributeForm()
        {
            //Никакие файлы/папки не выбраны в правильной форме
            if (lvwFiles.SelectedItems.Count == 0)
            {

                if (curFilePath == "Недавние Визиты")
                {
                    MessageBox.Show("Невозможно просмотреть свойства текущего пути！", "ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

               
            }
            // В форме справа выбраны файлы / папки
            else
            {
                
            }
        }



        // Отобразить окно управления полномочиями
        private void ShowPrivilegeForm()
        {
            // Файл / папка не выбрана в правильной форме
            if (lvwFiles.SelectedItems.Count == 0)
            {
                if (curFilePath == "Недавно посещенный")
                {
                    MessageBox.Show("Невозможно просмотреть управление полномочиями текущего пути！", "ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                PrivilegeForm privilegeForm = new PrivilegeForm(curFilePath);

                //Отображение интерфейса управления разрешениями для текущей папки
                privilegeForm.Show();
            }
            //В правильной форме выбраны файлы / папки.
            else
            {
                // Отображение интерфейса управления разрешениями для первого выбранного файла / папки
                PrivilegeForm privilegeForm = new PrivilegeForm(lvwFiles.SelectedItems[0].Tag.ToString());

                privilegeForm.Show();
            }

        }



        
        private void CreateFolder()
        {
            if (curFilePath == "Недавно посещенный")
            {
                MessageBox.Show("Невозможно создать новую папку по текущему пути！", "ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                int num = 1;
                string path = Path.Combine(curFilePath, "новая папка");
                string newFolderPath = path;

                while (Directory.Exists(newFolderPath))
                {
                    newFolderPath = path + "(" + num + ")";
                    num++;
                }

                Directory.CreateDirectory(newFolderPath);

                ListViewItem item = lvwFiles.Items.Add("новая папка" + (num == 1 ? "" : "(" + (num - 1) + ")"), IconsIndexes.Folder);

                //Реальный путь
                item.Tag = newFolderPath;

                //Обновите дерево каталогов слева
                LoadChildNodes(curSelectedNode);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }


       
        private void CreateFile()
        {
            if (curFilePath == "Недавно посещенный")
            {
                MessageBox.Show("Невозможно создать новый файл по текущему пути！", "ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            NewFileForm newFileForm = new NewFileForm(curFilePath, this);
            newFileForm.Show();
        }

       

        
        private void Open()
        {
            if (lvwFiles.SelectedItems.Count > 0)
            {
                string path = lvwFiles.SelectedItems[0].Tag.ToString();

                try
                {// Если выбранная папка
                    if (Directory.Exists(path))
                    {
                        //Открыть папку

                        ShowFilesList(path, true);
                    }
                    //Если выбранный файл
                    else
                    {
                        // открыть файл
                        Process.Start(path);
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        
        private void CopyFiles()
        {
            SetCopyFilesSourcePaths();
        }


        private void PasteFiles()
        {
            
            if (copyFilesSourcePaths[0] == null)
            {
                return;
            }

            if (!Directory.Exists(curFilePath))
            {
                return;
            }

            if (curFilePath == "Недавно посещенный")
            {
                MessageBox.Show("Невозможно вставить текущий путь！", "ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            for (int i = 0; copyFilesSourcePaths[i] != null; i++)
            {
               
                if (File.Exists(copyFilesSourcePaths[i]))
                {
                   
                    MoveToOrCopyToFileBySourcePath(copyFilesSourcePaths[i]);
                }
                
                else if (Directory.Exists(copyFilesSourcePaths[i]))
                {
                    MoveToOrCopyToDirectoryBySourcePath(copyFilesSourcePaths[i]);
                }

            }

            ShowFilesList(curFilePath, false);

           
            LoadChildNodes(curSelectedNode);

            copyFilesSourcePaths = new string[200];
        }



        private void CutFiles()
        {
           
            SetCopyFilesSourcePaths();

            isMove = true;
        }



        
        private void DeleteFiles()
        {
            if (lvwFiles.SelectedItems.Count > 0)
            {
                DialogResult dialogResult = MessageBox.Show("Вы уверены, что хотите его удалить？", "подтвердить удаление", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);

                if (dialogResult == DialogResult.No)
                {
                    return;
                }
                else
                {
                    try
                    {
                        foreach (ListViewItem item in lvwFiles.SelectedItems)
                        {
                            string path = item.Tag.ToString();

                            //Если это файл
                            if (File.Exists(path))
                            {
                                File.Delete(path);
                            }
                            //к
                            else if (Directory.Exists(path))
                            {
                                Directory.Delete(path, true);
                            }

                            lvwFiles.Items.Remove(item);
                        }

                        // Обновляем дерево каталогов слева
                        LoadChildNodes(curSelectedNode);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message, "ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }


        // Получаем исходный путь к копируемому файлу
        private void SetCopyFilesSourcePaths()
        {
            if (lvwFiles.SelectedItems.Count > 0)
            {
                int i = 0;

                foreach (ListViewItem item in lvwFiles.SelectedItems)
                {
                    copyFilesSourcePaths[i++] = item.Tag.ToString();
                }

                isMove = false;
            }
        }






        // Выполнить файл "переместить в" или "скопировать в"
        private void MoveToOrCopyToFileBySourcePath(string sourcePath)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(sourcePath);

                // Получить путь к месту назначения
                string destPath = Path.Combine(curFilePath, fileInfo.Name);

                // Если целевой путь совпадает с исходным, никакая операция не выполняется
                if (destPath == sourcePath)
                {
                    return;
                }

                // Переместите файл в целевой путь (в настоящее время выполняется операция «вырезать + вставить»)
                if (isMove)
                {
                    fileInfo.MoveTo(destPath);
                }
                //Вставьте файл в целевой путь (в настоящее время выполняется операция «копировать + вставить»)
                else
                {
                    fileInfo.CopyTo(destPath);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }



        // Рекурсивно копируем и вставляем папку (включая все файлы в папке)
        // Нет метода DirectoryInfo.CopyTo (строковый путь), вам нужно реализовать его самостоятельно
        private void CopyAndPasteDirectory(DirectoryInfo sourceDirInfo, DirectoryInfo destDirInfo)
        {
            //Определите, является ли целевая папка подкаталогом исходной папки, если да, будет выдано сообщение об ошибке и никакие операции выполняться не будут.
            for (DirectoryInfo dirInfo = destDirInfo.Parent; dirInfo != null; dirInfo = dirInfo.Parent)
            {
                if (dirInfo.FullName == sourceDirInfo.FullName)
                {
                    MessageBox.Show("Не могу скопировать! Папка назначения - это подкаталог исходной папки.！", "ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            //Создать целевую папку
            if (!Directory.Exists(destDirInfo.FullName))
            {
                Directory.CreateDirectory(destDirInfo.FullName);
            }

            //Скопируйте файл и вставьте его в папку назначения
            foreach (FileInfo fileInfo in sourceDirInfo.GetFiles())
            {
                fileInfo.CopyTo(Path.Combine(destDirInfo.FullName, fileInfo.Name));
            }

            //Рекурсивно скопируйте и вставьте подпапки в целевую папку
            foreach (DirectoryInfo sourceSubDirInfo in sourceDirInfo.GetDirectories())
            {
                DirectoryInfo destSubDirInfo = destDirInfo.CreateSubdirectory(sourceSubDirInfo.Name);
                CopyAndPasteDirectory(sourceSubDirInfo, destSubDirInfo);
            }

        }



        //Выполните папку «переместить в» или «скопировать в»”
        private void MoveToOrCopyToDirectoryBySourcePath(string sourcePath)
        {
            try
            {
                DirectoryInfo sourceDirectoryInfo = new DirectoryInfo(sourcePath);
                // Получаем целевой путь
                string destPath = Path.Combine(curFilePath, sourceDirectoryInfo.Name);

                // Если путь назначения и исходный путь совпадают, ничего не делать
                if (destPath == sourcePath)
                {
                    return;
                }

                //Переместите папку в целевой путь (в настоящее время выполняется операция «вырезать + вставить»)
                if (isMove)
                {
                    CopyAndPasteDirectory(sourceDirectoryInfo, new DirectoryInfo(destPath));

                    Directory.Delete(sourcePath, true);

                }
                //Вставьте папку в целевой путь (в настоящее время выполняется операция «копировать + вставить»)
                else
                {// Рекурсивно копируем и вставляем папку (включая все файлы в папке)
                    CopyAndPasteDirectory(sourceDirectoryInfo, new DirectoryInfo(destPath));
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }




        //Переименуйте файл
        private void RenameFile()
        {
            if (lvwFiles.SelectedItems.Count > 0)
            {
                //По сути, имитируйте редактирование метки, чтобы вызвать событие LabelEdit через код.
                lvwFiles.SelectedItems[0].BeginEdit();
            }
        }



        // Инициализировать связанные параметры "просмотра"
        private void InitViewChecks()
        {
            // По умолчанию в форме справа отображается подробное представление
            tsmiDetailedInfo.Checked = true;
            tsmiDetailedInfo1.Checked = true;
        }



        //Сбросить соответствующие параметры просмотра
        private void ResetViewChecks()
        {
            tsmiBigIcon.Checked = false;
            tsmiSmallIcon.Checked = false;
            tsmiList.Checked = false;
            tsmiDetailedInfo.Checked = false;

            tsmiBigIcon1.Checked = false;
            tsmiSmallIcon1.Checked = false;
            tsmiList1.Checked = false;
            tsmiDetailedInfo1.Checked = false;
        }




        //Используйте несколько потоков для поиска файлов / папок
        private void SearchWithMultiThread(string path, string fileName)
        {
           
            lvwFiles.Items.Clear();

           
            tsslblFilesNum.Text = 0 + " Предметы";

            this.fileName = fileName;

            ThreadPool.SetMaxThreads(1000, 1000);

            ThreadPool.QueueUserWorkItem(new WaitCallback(Search), path);

        }



        public void Search(Object obj)
        {
            string path = obj.ToString();

            DirectorySecurity directorySecurity = new DirectorySecurity(path, AccessControlSections.Access);

            if(!directorySecurity.AreAccessRulesProtected)
            {

                
                DirectoryInfo directoryInfo = new DirectoryInfo(path);

                FileInfo[] fileInfos = directoryInfo.GetFiles();

            
                if (fileInfos.Length > 0)
                {
                    foreach (FileInfo fileInfo in fileInfos)
                    {
                        try
                        {
                            if (fileInfo.Name.Split('.')[0].Contains(fileName))
                            {
                                AddSearchResultItemIntoList(fileInfo.FullName, true);

                                tsslblFilesNum.Text = lvwFiles.Items.Count + " Предметы";
                            }
                        }
                        catch (Exception e)
                        {
                            continue;
                        }
                    }

                }

              
                DirectoryInfo[] directoryInfos = directoryInfo.GetDirectories();

               
                if (directoryInfos.Length > 0)
                {
                    foreach (DirectoryInfo dirInfo in directoryInfos)
                    {
                        try
                        {
                            if (dirInfo.Name.Contains(fileName))
                            {
                                AddSearchResultItemIntoList(dirInfo.FullName, false);

                                
                                tsslblFilesNum.Text = lvwFiles.Items.Count + " Предметы";
                            }
                            else
                            {
                                // Многопоточная стратегия 1. Начиная с папки для поиска, поток будет открываться для рекурсивного поиска каждый раз, когда папка обнаруживается в рекурсивном процессе. Общее количество потоков велико, но
                                // Используйте пул потоков, он будет автоматически управляться, так что поток может использоваться повторно, после завершения поисковой задачи потока его можно продолжать использовать
                                // Выполняем другую задачу поиска в очереди задач.
                                // Преимущества: он может адаптироваться к обычным ситуациям, и скорость поиска в целом очень высокая!
                                ThreadPool.QueueUserWorkItem(new WaitCallback(Search), dirInfo.FullName);

                                // Многопоточная стратегия 2: открыть поток для каждой папки в папке, в которой будет выполняться поиск для рекурсивного поиска, и затем никаких потоков, общее количество потоков равно количеству подпапок в папке, в которой будет выполняться поиск.
                                // Недостатки: когда количество подпапок в папке для поиска меньше, эффект хуже и скорость меньше.
                                //ThreadPool.QueueUserWorkItem( новый WaitCallback (SearchWithOneThread), dirInfo.FullName);
                            }
                        }
                        catch (Exception e)
                        {

                        }

                    }
                }

            }
        }




        // Использование единственного потока для поиска единственной подпапки
        public void SearchWithOneThread(object obj)
        {
            string path = obj.ToString();

            DirectorySecurity directorySecurity = new DirectorySecurity(path, AccessControlSections.Access);

            // Каталог доступен
            if (!directorySecurity.AreAccessRulesProtected)
            {

                // подпапка
                DirectoryInfo directoryInfo = new DirectoryInfo(path);

                // Файлы в подпапках
                FileInfo[] fileInfos = directoryInfo.GetFiles();

                // Папки в подпапках
                DirectoryInfo[] directoryInfos = directoryInfo.GetDirectories();


                //Искать в файле
                if (fileInfos.Length > 0)
                {
                    foreach (FileInfo fileInfo in fileInfos)
                    {
                        try
                        {
                            if (fileInfo.Name.Split('.')[0].Contains(fileName))
                            {
                                AddSearchResultItemIntoList(fileInfo.FullName, true);

                                //Обновить строку состояния
                                tsslblFilesNum.Text = lvwFiles.Items.Count + " Предметы";
                            }
                        }
                        catch (Exception e)
                        {
                            continue;
                        }
                    }

                }

                // Поиск в папке
                if (directoryInfos.Length > 0)
                {
                    foreach (DirectoryInfo dirInfo in directoryInfos)
                    {
                        try
                        {
                            if (dirInfo.Name.Contains(fileName))
                            {
                                AddSearchResultItemIntoList(dirInfo.FullName, false);

                               
                                tsslblFilesNum.Text = lvwFiles.Items.Count + " предметы";
                            }
                            else
                            {
                                SearchWithOneThread(dirInfo.FullName);
                            }
                        }
                        catch (Exception e)
                        {
                            continue;
                        }
                    }
                }
            }
        }


        // Отображаем результаты поиска в списке файлов
        private void AddSearchResultItemIntoList(string fullPath, bool isFile)
        {
            //Это файл
            if (isFile)
            {
                FileInfo fileInfo = new FileInfo(fullPath);

                ListViewItem item = lvwFiles.Items.Add(fileInfo.Name);

                // Это exe-файл или без расширения
                if (fileInfo.Extension == ".exe" || fileInfo.Extension == "")
                {
                    Icon fileIcon = GetSystemIcon.GetIconByFileName(fileInfo.FullName);

                    ilstIcons.Images.Add(fileInfo.Name, fileIcon);

                    item.ImageKey = fileInfo.Name;
                }
               
                else
                {
                    if (!ilstIcons.Images.ContainsKey(fileInfo.Extension))
                    {
                        Icon fileIcon = GetSystemIcon.GetIconByFileName(fileInfo.FullName);

                        ilstIcons.Images.Add(fileInfo.Extension, fileIcon);
                    }

                    item.ImageKey = fileInfo.Extension;
                }

                item.Tag = fileInfo.FullName;

                item.SubItems.Add(fileInfo.LastWriteTimeUtc.ToString());
                item.SubItems.Add(fileInfo.Extension + "文件");
            }
           
            else
            {
                DirectoryInfo dirInfo = new DirectoryInfo(fullPath);

                ListViewItem item = lvwFiles.Items.Add(dirInfo.Name, IconsIndexes.Folder);
                item.Tag = dirInfo.FullName;
                item.SubItems.Add(dirInfo.LastWriteTimeUtc.ToString());
                item.SubItems.Add("папка");
                item.SubItems.Add("");
            }
        }


        
        
        //о форме
        private void tsmiAbout_Click(object sender, EventArgs e)
        {
            AboutForm aboutForm = new AboutForm();
            aboutForm.Show();
        }
    }

}
