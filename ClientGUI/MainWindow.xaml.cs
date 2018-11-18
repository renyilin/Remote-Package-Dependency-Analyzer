using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ClientGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static string initPath = MessagePassingComm.ServerEnvironment.root;
        ViewModel viewModel = new ViewModel(initPath);

        public MainWindow()
        {
            InitializeComponent();
            folderTree.DataContext = viewModel;

            //comm = new Comm(ClientEnvironment.address, ClientEnvironment.port);
            //initializeMessageDispatcher();
            //rcvThread = new Thread(rcvThreadProc);
            //rcvThread.Start();

            LoadNavigator(initPath);
            tbkPath.Text = initPath.Substring(initPath.LastIndexOf('/') + 1);

        }

        void LoadNavigator(string path)
        {
            ObservableCollection<IFileType> currentFolder;
            if (viewModel.FolderDict.ContainsKey(path))
            {
                currentFolder = viewModel.FolderDict[path];
                currentFolder.Clear();
            }
            else
                return;

            string[] dirs = System.IO.Directory.GetDirectories(path);
            foreach (string dir in dirs)
            {
                System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(dir);
                string fullPath = System.IO.Path.Combine(path, di.Name);
                Folder newFolder = new Folder { FLabel = di.Name, FullPath = fullPath };
                currentFolder.Add(newFolder);
                viewModel.FolderDict[newFolder.FullPath] = newFolder.Children;
            }

            string[] files = System.IO.Directory.GetFiles(path);
            foreach (string file in files)
            {
                string name = System.IO.Path.GetFileName(file);
                string fullPath = System.IO.Path.Combine(path, name);
                currentFolder.Add(new File { FLabel = name, FullPath = fullPath });
            }

        }

        string getAncestor(int n, string path)
        {
            for (int i = 0; i < n; ++i)
            {
                int startIndex = path.LastIndexOf('/') + 1;
                int length = path.LastIndexOf('\\') - path.LastIndexOf('/');
                path = path.Substring(startIndex, length);
            }
            return path;
        }

        private void FolderTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            IFileType fileType = (IFileType)folderTree.SelectedItem;
            string fullPath = fileType.FullPath;
            if (fileType.FileType == FileTypeCal.Folder)
                tbkPath.Text = fullPath.Substring(fullPath.LastIndexOf('/') + 1);
            else
            {
                //When select a file, display the ancestor path.

                string parentPath = getAncestor(1, fullPath);
                tbkPath.Text = parentPath;
            }
        }

        private void FolderTree_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (folderTree.SelectedItem == null)
                return;
            IFileType fileType = (IFileType)folderTree.SelectedItem;
            if (fileType.FileType == FileTypeCal.Folder)
            {
                LoadNavigator(fileType.FullPath);
            }
        }
    }
}
