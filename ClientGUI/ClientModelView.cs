using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientGUI
{
    public enum FileTypeCal
    {
        File,
        Folder
    }

    public interface IFileType
    {
        FileTypeCal FileType { get; }
        string FullPath { get; }
        string FLabel { get; }
        ObservableCollection<IFileType> Children { get; }

    }

    class Folder : IFileType
    {
        public FileTypeCal FileType { get; } = FileTypeCal.Folder;

        public string FullPath { get; set; } = "";

        public string FLabel { get; set; } = "";

        public ObservableCollection<IFileType> Children { get; set; } = new ObservableCollection<IFileType> { };

    }

    class File : IFileType
    {
        public FileTypeCal FileType { get; } = FileTypeCal.File;

        public string FullPath { get; set; } = "";

        public string FLabel { get; set; } = "";

        public ObservableCollection<IFileType> Children { get; set; } = new ObservableCollection<IFileType> { };
    }




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
            //TEST = "jubba";

            m_folders = new ObservableCollection<IFileType>();
            _folderDict = new Dictionary<string, ObservableCollection<IFileType>>();
            FolderDict[rootPath] = FFcollection;

            //add Root items
            //Folders.Add(new Folder { FLabel = "Dummy1", FullPath = @"C:\dummy1" });
            //FFcollection.Add(new Folder { FLabel = "Dummy1", FullPath = @"C:\dummy1" });

            //Folders.Add(new Folder { FLabel = "Dummy2", FullPath = @"C:\dummy2" });
            //Folders.Add(new Folder { FLabel = "Dummy3", FullPath = @"C:\dummy3" });
            //Folders.Add(new Folder { FLabel = "Dummy4", FullPath = @"C:\dummy4" });

            ////add sub items
            //Folders[0].Children.Add(new Folder { FLabel = "Dummy11", FullPath = @"C:\dummy11" });
            //Folders[0].Children.Add(new Folder { FLabel = "Dummy12", FullPath = @"C:\dummy12" });
            //Folders[0].Children.Add(new Folder { FLabel = "Dummy13", FullPath = @"C:\dummy13" });
            //Folders[0].Children.Add(new Folder { FLabel = "Dummy14", FullPath = @"C:\dummy14" });

            //Folders[0].Children[0].Children.Add(new Folder { FLabel = "Dummy111", FullPath = @"C:\dummy111" });

            //Folders[1].Children.Add(new File { FLabel = "file0", FullPath = @"C:\dummy111\file0" });
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

        void NotifiyPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
