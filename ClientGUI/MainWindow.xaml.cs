/////////////////////////////////////////////////////////////////////
// MainWindow.xaml.cs - GUI for the client of the remote           //
//                      type-based dependency analysis             //
//                                                                 //
// ver 1.0                                                         //
// Yilin Ren, CSE681, Fall 2018                                    //
/////////////////////////////////////////////////////////////////////
/*
 * This package provides:
 * ----------------------
 * This package is GUI for the client of the remote type-based dependency
 * analysis. It has three main tabs including Execution, Options, and Results.
 * Execution tab is used for selecting a directory for analysis and clicking 
 * Analyze button. Options tab is responsible for providing some operation
 * options. Results tab is for displaying results.
 * 
 * Required Files:f
 * ---------------
 * ClientViewModel.cs
 * ReqTests.cs
 * Environment.cs
 * IMessagePassingCommService.cs
 * TestHarness.cs
 * MessagePassingCommService.cs
 * 
 * Maintenance History:
 * --------------------
 * ver 1.0 : 3 Dec 2018
 * - first release
 */


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MessagePassingComm;
using System.Windows.Forms;

namespace ClientGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ClientMainWindow : Window
    {
        ViewModel viewModel = new ViewModel(".");
        static Comm comm { get; set; } = null;
        Dictionary<string, Action<CommMessage>> messageDispatcher = new Dictionary<string, Action<CommMessage>>();
        Thread rcvThread = null;

        //-------< Constructor >----------------------------------------
        public ClientMainWindow()
        {
            InitializeComponent();
            folderTree.DataContext = viewModel;

            Console.Title = "Client";
            Console.ForegroundColor = ConsoleColor.Yellow;

            comm = new Comm(ClientEnvironment.address, ClientEnvironment.port);
            initializeMessageDispatcher();
            rcvThread = new Thread(rcvThreadProc);
            rcvThread.Start();

            getTopFiles();         
        }

        //-------< send requests to get the files and folders in the root >----------------
        private void getTopFiles()
        {
            tb_StatusBar.Text = "Connecting...";
            tbkPath.Text = ".";
            ObservableCollection<IFileType> currentFolder = viewModel.FolderDict["."];
            currentFolder.Clear();

            CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
            msg1.from = ClientEnvironment.endPoint;
            msg1.to = ServerEnvironment.endPoint;
            msg1.command = "moveIntoFolderDirs";
            msg1.arguments.Add(tbkPath.Text);
            comm.postMessage(msg1);
            CommMessage msg2 = msg1.clone();
            msg2.command = "moveIntoFolderFiles";
            comm.postMessage(msg2);
        }

        //-----------< start an automated unit test >--------------------------------------------
        private void Tester()
        {
            Console.Write("\n\n  Demonstrating Remote Type-Based Package Dependency Analysis");
            Console.Write("\n =============================================================\n");

            ReqTest3 reqTest3 = new ReqTest3();
            ReqTest456 reqTest456 = new ReqTest456();
            ReqTest7 reqTest7 = new ReqTest7();

            TestHarness.Tester tester = new TestHarness.Tester();
            tester.add(reqTest3);
            tester.add(reqTest456);
            tester.add(reqTest7);

            tester.execute();
        }

        //-----------< get the ancestor of the path >--------------------------------------------
        string getAncestor(int n, string path)
        {
            for (int i = 0; i < n; ++i)
            {
                int length = path.LastIndexOf('\\');
                path = path.Substring(0, length);
            }
            return path;
        }

        //-----< change analysis path displayed in text block when select items changed in tree view>-----------
        private void FolderTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            IFileType fileType = (IFileType)folderTree.SelectedItem;
            if (fileType != null)
            {
                string fullPath = fileType.FullPath;
                if (fileType.FileType == FileTypeCal.Folder)
                {
                    tbkPath.Text = fullPath;
                }
                else
                {
                    //When select a file, display the ancestor path.
                    string parentPath = getAncestor(1, fullPath);
                    tbkPath.Text = parentPath;
                }
            }
        }

        //------< double click on folders to load child folders >------------------------------
        private void FolderTree_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (folderTree.SelectedItem == null)
                return;
            IFileType fileType = (IFileType)folderTree.SelectedItem;


            if (fileType.FileType == FileTypeCal.Folder)
            {
                ObservableCollection<IFileType> currentFolder = viewModel.FolderDict[tbkPath.Text];
                currentFolder.Clear();

                CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
                msg1.from = ClientEnvironment.endPoint;
                msg1.to = ServerEnvironment.endPoint;
                msg1.command = "moveIntoFolderDirs";
                msg1.arguments.Add(tbkPath.Text);
                comm.postMessage(msg1);
                CommMessage msg2 = msg1.clone();
                msg2.command = "moveIntoFolderFiles";
                comm.postMessage(msg2);
            }
        
        }



        //------< initialize Message Dispatcher >-----------------------------------------------
        void initializeMessageDispatcher()
        {
            messageDispatcher["moveIntoFolderFiles"] = (CommMessage msg) =>
            {
                if (msg.arguments.Count != 0)
                {
                    string path = getAncestor(1, msg.arguments[0]);
                    ObservableCollection<IFileType> currentFolder = viewModel.FolderDict[path];
                    //Remove all files in the current directory. 
                    currentFolder.Where(l => l.FileType == FileTypeCal.File).ToList().All(i => currentFolder.Remove(i));
                    foreach (string file in msg.arguments)
                    {
                        string name = System.IO.Path.GetFileName(file);
                        currentFolder.Add(new File(name, file));
                    }
                }
            };
            //// load remoteDirs listbox with dirs from folder
            messageDispatcher["moveIntoFolderDirs"] = (CommMessage msg) =>
            {
                if (msg.arguments.Count != 0)
                {
                    string path = getAncestor(1, msg.arguments[0]);
                    ObservableCollection<IFileType> currentFolder = viewModel.FolderDict[path];
                    //Remove all Dirs in the current directory. 
                    currentFolder.Where(l => l.FileType == FileTypeCal.Folder).ToList().All(i => currentFolder.Remove(i));
                    foreach (string dir in msg.arguments)
                    {
                        System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(dir);
                        Folder newFolder = new Folder(di.Name, dir);
                        currentFolder.Add(newFolder);
                        viewModel.FolderDict[newFolder.FullPath] = newFolder.Children;
                    }
                }
            };
            //// Execute dependency analyzsis
            messageDispatcher["depAnalysis"] = (CommMessage msg) =>
            {
                StringBuilder stringBuilder = new StringBuilder();
                tb_results.Clear();
                foreach (string str in msg.arguments)
                {
                    tb_results.AppendText(str);
                    tb_results.AppendText("\n");   
                }
                btnExecute.IsEnabled = true;
                TabControl.SelectedItem = TabResults;
            };
        }

        //----< define processing for GUI's receive thread >-------------------
        void rcvThreadProc()
        {
            Console.Write("\n  starting client's receive thread");
            while (true)
            {
                CommMessage msg = comm.getMessage();
                msg.show();
                if (msg.command == null)
                    continue;

                Dispatcher.Invoke(()=> tb_StatusBar.Text = "Double click on folders to open.");
                // pass the Dispatcher's action value to the main thread for execution

                Dispatcher.Invoke(messageDispatcher[msg.command], new object[] { msg });
            }
        }


        //----< close Comm when window closed >--------------------------------
        private void Window_Closed(object sender, EventArgs e)
        {
            comm.close();

            System.Diagnostics.Process.GetCurrentProcess().Kill();
           
        }

        //----< click refresh button >------------------------------------------
        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            getTopFiles();
        }

        //----< click executive button >----------------------------------------
        //- send request to the server for running the dependency analyzer 
        private void BtnExecute_Click(object sender, RoutedEventArgs e)
        {
            btnExecute.IsEnabled = false;
            CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
            msg1.from = ClientEnvironment.endPoint;
            msg1.to = ServerEnvironment.endPoint;
            msg1.command = "depAnalysis";
            string optionDA = cb_DA.IsChecked.ToString();
            string optionSC = cb_SC.IsChecked.ToString();
            string optionFileRecursion = RB_sub.IsChecked.ToString();
            string path = tbkPath.Text;
            msg1.arguments = new List<string> { path, optionDA, optionSC, optionFileRecursion };
            comm.postMessage(msg1);
        }

        //----< send request message for the automated unit test >------------------------------
        public static void sendMSgforReqTest456(List<string> arguments)
        {
            CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
            msg1.from = ClientEnvironment.endPoint;
            msg1.to = ServerEnvironment.endPoint;
            msg1.command = "depAnalysis";
            msg1.arguments = arguments;
            comm.postMessage(msg1);

            msg1.show();
        }

        //-----< start the automated unit test >-------------------------------------------------
        private void BtnAutoTest_Click(object sender, RoutedEventArgs e)
        {
            Tester();
            System.Windows.Forms.MessageBox.Show("The demonstration of requirements displays in the Client console." +
                "\nThe results display in the Results tab in the GUI.", "Automated Unit Test",0,MessageBoxIcon.Information);
        }

        private void TabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (TabExecution.IsSelected)
            {
                tb_StatusBar.Text = "Double click on folders to open.";
            }
            if (TabOptions.IsSelected)
            {
                tb_StatusBar.Text = "";
            }
            if (TabResults.IsSelected)
            {
                tb_StatusBar.Text = "";
            }
        }
    }
}
