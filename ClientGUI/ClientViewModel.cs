/////////////////////////////////////////////////////////////////////
// ClientViewModel.cs - View Model for Client GUI                  //
//                                                                 //
// ver 1.0                                                         //
// Yilin Ren, CSE681, Fall 2018                                    //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * ----------------------
 * This package is a view model for the client GUI. It provides a FFcollection
 * which is a collection of files and folds. it is binded with the TreeView in 
 * Client GUI which is used for remote files navigation.
 * 
 * Required Files:
 * ---------------
 * ClientViewModel.cs
 * 
 * Maintenance History:
 * --------------------
 * ver 1.0 : 3 Dec 2018
 * - first release
 */


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientGUI
{
    ///////////////////////////////////////////////////////////////////
    // Enum of File Type
    public enum FileTypeCal
    {
        File,
        Folder
    }

    ///////////////////////////////////////////////////////////////////
    // interface of File Type
    public interface IFileType
    {
        FileTypeCal FileType { get; }
        string FullPath { get; }
        string FLabel { get; }
        ObservableCollection<IFileType> Children { get; }

    }

    ///////////////////////////////////////////////////////////////////
    // Folder
    // - Children :  Files and Folders in this Folder
    class Folder : IFileType
    {
        public FileTypeCal FileType { get; } = FileTypeCal.Folder;

        public string FullPath { get; set; } = "";

        public string FLabel { get; set; } = "";

        public ObservableCollection<IFileType> Children { get; set; } = new ObservableCollection<IFileType> { };

        public Folder(string FLabel, string FullPath)
        {
            this.FullPath = FullPath;
            this.FLabel = FLabel;
        }

    }

    ///////////////////////////////////////////////////////////////////
    // File
    class File : IFileType
    {
        public FileTypeCal FileType { get; } = FileTypeCal.File;

        public string FullPath { get; set; } = "";

        public string FLabel { get; set; } = "";

        public ObservableCollection<IFileType> Children { get; set; } = new ObservableCollection<IFileType> { };

        public File(string FLabel, string FullPath)
        {
            this.FullPath = FullPath;
            this.FLabel = FLabel;
        }
    }



    ///////////////////////////////////////////////////////////////////
    // ViewModel for the client GUI
    // - FFcollection is a collection of files and folds
    // - it is binded with the TreeView in Client GUI which is used for remote files navigation.
    // - INotifyPropertyChanged notifies clients that a property value has changed.
    class ViewModel : INotifyPropertyChanged
    {
        Dictionary<string, ObservableCollection<IFileType>> _folderDict;
        public Dictionary<string, ObservableCollection<IFileType>> FolderDict
        {
            get { return _folderDict; }
            set { _folderDict = value; }
        }

        public ViewModel(string rootPath)
        {
          
            m_folders = new ObservableCollection<IFileType>();
            _folderDict = new Dictionary<string, ObservableCollection<IFileType>>();
            FolderDict[rootPath] = FFcollection;
        }

        public string TEST { get; set; }


        private ObservableCollection<IFileType> m_folders;
        public ObservableCollection<IFileType> FFcollection
        {
            get { return m_folders; }
            set
            {
                m_folders = value;
                NotifiyPropertyChanged("FFcollection");
            }
        }

        // - NotifyPropertyChanged notifies clients that a property value has changed.
        void NotifiyPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        public event PropertyChangedEventHandler PropertyChanged;

#if (TEST_VIEWMODEL)
        static void Main()
        {
            ViewModel viewModel = new ViewModel(".");
            ObservableCollection<IFileType>  rootFolder = viewModel.FolderDict["."];
            Folder Folder1 = new Folder { FLabel = "Folder1", FullPath = ".\\1" };
            viewModel.FolderDict[Folder1.FullPath] = Folder1.Children;
            rootFolder.Add(Folder1);
            viewModel.FolderDict[Folder1.FullPath].Add(new File { FLabel = "File1", FullPath = ".\\1\\file1" });

        }
#endif
    }
}
