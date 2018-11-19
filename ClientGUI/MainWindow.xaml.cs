﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MessagePassingComm;

namespace ClientGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static string initPath = MessagePassingComm.ServerEnvironment.root;
        ViewModel viewModel = new ViewModel(initPath);
        Comm comm { get; set; } = null;
        Dictionary<string, Action<CommMessage>> messageDispatcher = new Dictionary<string, Action<CommMessage>>();
        Thread rcvThread = null;

        public MainWindow()
        {
            InitializeComponent();
            folderTree.DataContext = viewModel;

            comm = new Comm(ClientEnvironment.address, ClientEnvironment.port);
            initializeMessageDispatcher();
            rcvThread = new Thread(rcvThreadProc);
            rcvThread.Start();

            LoadNavigator(initPath);
            tbkPath.Text = ".\\";

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
                //int startIndex = path.LastIndexOf('/') + 1;
                //int length = path.LastIndexOf('\\') - path.LastIndexOf('/');
                //path = path.Substring(startIndex, length);
                string toPath = System.IO.Directory.GetParent(path).FullName + '\\';
                string fromPath = System.IO.Path.GetFullPath(MessagePassingComm.ServerEnvironment.root);
                path = TestUtilities.MakeRelativePath(fromPath, toPath);
            }
            return path;
        }

        private void FolderTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            IFileType fileType = (IFileType)folderTree.SelectedItem;
            string fullPath = fileType.FullPath;
            if (fileType.FileType == FileTypeCal.Folder)
            {
                //tbkPath.Text = fullPath.Substring(fullPath.LastIndexOf('/') + 1);
                string fromPath = System.IO.Path.GetFullPath(MessagePassingComm.ServerEnvironment.root);
                string toPath = System.IO.Path.GetFullPath(fullPath);
                tbkPath.Text = ".\\" + TestUtilities.MakeRelativePath(fromPath, toPath);
            }
            else
            {
                //When select a file, display the ancestor path.
                string parentPath = getAncestor(1, fullPath);
                tbkPath.Text = ".\\" + parentPath;
            }
        }

        private void FolderTree_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (folderTree.SelectedItem == null)
                return;
            IFileType fileType = (IFileType)folderTree.SelectedItem;
            if (fileType.FileType == FileTypeCal.Folder)
            {
                CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
                msg1.from = ClientEnvironment.endPoint;
                msg1.to = ServerEnvironment.endPoint;
                msg1.command = "moveIntoFolderFiles";
                msg1.arguments.Add(tbkPath.Text);//fileType.FullPath);
                comm.postMessage(msg1);
                CommMessage msg2 = msg1.clone();
                msg2.command = "moveIntoFolderDirs";
                comm.postMessage(msg2);

                //LoadNavigator(fileType.FullPath);
            }
        }

        void initializeMessageDispatcher()
        {
            // load remoteFiles listbox with files from root

            //messageDispatcher["getTopFiles"] = (CommMessage msg) =>
            //{
            //    remoteFiles.Items.Clear();
            //    foreach (string file in msg.arguments)
            //    {
            //        remoteFiles.Items.Add(file);
            //    }
            //};
            //// load remoteDirs listbox with dirs from root

            //messageDispatcher["getTopDirs"] = (CommMessage msg) =>
            //{
            //    remoteDirs.Items.Clear();
            //    foreach (string dir in msg.arguments)
            //    {
            //        remoteDirs.Items.Add(dir);
            //    }
            //};
            //// load remoteFiles listbox with files from folder

            //messageDispatcher["moveIntoFolderFiles"] = (CommMessage msg) =>
            //{
            //    remoteFiles.Items.Clear();
            //    foreach (string file in msg.arguments)
            //    {
            //        remoteFiles.Items.Add(file);
            //    }
            //};
            //// load remoteDirs listbox with dirs from folder

            //messageDispatcher["moveIntoFolderDirs"] = (CommMessage msg) =>
            //{
            //    remoteDirs.Items.Clear();
            //    foreach (string dir in msg.arguments)
            //    {
            //        remoteDirs.Items.Add(dir);
            //    }
            //};
        }

        //----< define processing for GUI's receive thread >-------------

        void rcvThreadProc()
        {
            Console.Write("\n  starting client's receive thread");
            while (true)
            {
                CommMessage msg = comm.getMessage();
                msg.show();
                if (msg.command == null)
                    continue;

                // pass the Dispatcher's action value to the main thread for execution

                Dispatcher.Invoke(messageDispatcher[msg.command], new object[] { msg });
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            comm.close();
        }
    }
}
