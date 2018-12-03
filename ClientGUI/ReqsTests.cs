using MessagePassingComm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ClientGUI
{
    class ReqDisplay
    {
        public static void title(string tle)
        {
            Console.Write("\n  {0}", tle);
            Console.Write("\n {0}", new string('-', tle.Length + 2));
        }
        public static void message(string msg)
        {
            Console.Write("\n  {0}\n", msg);
        }
        public static void showSet(HashSet<string> set, string msg = "")
        {
            if (msg.Length > 0)
                Console.Write("\n  {0}\n  ", msg);
            else
                Console.Write("\n  Set:\n  ");
            foreach (var tok in set)
            {
                Console.Write("\"{0}\" ", tok);
            }
            Console.Write("\n");
        }

        public static void showList(List<string> lst, string msg = "")
        {
            if (msg.Length > 0)
                Console.Write("\n  {0}\n  ", msg);
            else
                Console.Write("\n  List:\n  ");
            int count = 0;
            foreach (var tok in lst)
            {
                Console.Write("\"{0}\" ", tok);
                if (++count == 10)
                {
                    count = 0;
                    Console.Write("\n  ");
                }
            }
            Console.Write("\n");
        }
    }
    public partial class MainWindow : Window
    {
        class ReqTest3 : ITest
        {
            public string name { get; set; } = "Req3";
            public string path { get; set; } = "../../";
            public bool result { get; set; } = false;
            void onFile(string filename)
            {
                Console.Write("\n    {0}", filename);
                result = true;
            }
            void onDir(string dirname)
            {
                Console.Write("\n  {0}", dirname);
            }
            public bool doTest()
            {
                ReqDisplay.title("Req #3 - C# packages requirements");
                ReqDisplay.message("- Shall have C# packages: " +
                                   "\n    Comm, Server, Client, Most of the packages used for Project #3." +
                                   "\n  - This program including the following packages:");
                FileUtilities.Navigate nav = new FileUtilities.Navigate();
                nav.Add("*.cs");
                //nav.newDir += new FileUtilities.Navigate.newDirHandler(onDir);
                nav.newFile += new FileUtilities.Navigate.newFileHandler(onFile);
                List<string> listDirectory = new List<string>{"ClientGUI", "DepAlyzServer", "IMessagePassingCommService",
                "MessagePassingCommService", "Environment", "LocalFileMgr", "TestUtilities", "Toker", "SemiExp",
            "TypeTable", "TypeAnalysis", "DepAnalysis","StrongComponentAlys", "Display",
            "Repository", "DemoExecutive", "Graph", "FileMgr", "TestHarness" };
                foreach (string directory in listDirectory)
                {
                    path = "../../../" + directory;
                    nav.go(path, false);
                }

                Console.WriteLine();
                return true;
            }
        }

        class ReqTest456 : ITest
        {
            public string name { get; set; } = "Req456";
            public string path { get; set; } = "../../";
            public bool result { get; set; } = false;
            void onFile(string filename)
            {
                Console.Write("\n    {0}", filename);
                result = true;
            }
            void onDir(string dirname)
            {
                Console.Write("\n  {0}", dirname);
            }
            public bool doTest()
            {
                ReqDisplay.title("Req #4,#5,#6 - Server shall evaluate all the dependencies and find all strong components.");
                ReqDisplay.message("- The Server packages shall evaluate all the dependencies between files in a specified file set, based on received request messages. "
                    + "\n  - The Server packages shall find all strong components, if any, in the file collection."
                    + "\n  - The Client packages shall display requested results in a well formated GUI display."
                    + "\n  - Demonstrate these requirements by sending a request to the server and display results in the GUI. ");

                string path = ".\\SpecialTestCases";
                string optionDA = "true";
                string optionSC = "true";
                string optionFileRecursion = "true";

                MainWindow.sendMSgforReqTest456(new List<string> { path, optionDA, optionSC, optionFileRecursion });

                Console.WriteLine();
                return true;
            }
        }

        class ReqTest7 : ITest
        {
            public string name { get; set; } = "Req7";
            public string path { get; set; } = "../../";
            public bool result { get; set; } = false;
            void onFile(string filename)
            {
                Console.Write("\n    {0}", filename);
                result = true;
            }
            void onDir(string dirname)
            {
                Console.Write("\n  {0}", dirname);
            }
            public bool doTest()
            {
                ReqDisplay.title("Req #7- Shall include an automated unit test suite.");
                ReqDisplay.message("- This is the automated unit test suite that demonstrates all of the functional requirements.");

                return true;
            }
        }
    }
}

