////////////////////////////////////////////////////////////////////////////
// NavigatorServer.cs - File Server for WPF NavigatorClient Application   //
// ver 2.0                                                                //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2017        //
////////////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * This package defines a single NavigatorServer class that returns file
 * and directory information about its rootDirectory subtree.  It uses
 * a message dispatcher that handles processing of all incoming and outgoing
 * messages.
 * 
 * Maintanence History:
 * --------------------
 * ver 2.0 - 24 Oct 2017
 * - added message dispatcher which works very well - see below
 * - added these comments
 * ver 1.0 - 22 Oct 2017
 * - first release
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessagePassingComm;
using FileMgr;
using System.Diagnostics;

namespace DepAlyzServer
{
    public class DependencyAlyzServer
    {
        IFileMgr localFileMgr { get; set; } = null;
        Comm comm { get; set; } = null;

        Dictionary<string, Func<CommMessage, CommMessage>> messageDispatcher =
          new Dictionary<string, Func<CommMessage, CommMessage>>();

        /*----< initialize server processing >-------------------------*/

        public DependencyAlyzServer()
        {
            initializeEnvironment();
            Console.Title = "Navigator Server";
            localFileMgr = FileMgrFactory.create(FileMgrType.Local);
        }
        /*----< set Environment properties needed by server >----------*/

        void initializeEnvironment()
        {
            MessagePassingComm.Environment.root = ServerEnvironment.root;
            MessagePassingComm.Environment.address = ServerEnvironment.address;
            MessagePassingComm.Environment.port = ServerEnvironment.port;
            MessagePassingComm.Environment.endPoint = ServerEnvironment.endPoint;
        }
        /*----< define how each message will be processed >------------*/

        void initializeDispatcher()
        {
            //Func<CommMessage, CommMessage> getTopFiles = (CommMessage msg) =>
            //{
            //    localFileMgr.currentPath = "";
            //    CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
            //    reply.to = msg.from;
            //    reply.from = msg.to;
            //    reply.command = "getTopFiles";
            //    reply.arguments = localFileMgr.getFiles().ToList<string>();
            //    return reply;
            //};
            //messageDispatcher["getTopFiles"] = getTopFiles;

            //Func<CommMessage, CommMessage> getTopDirs = (CommMessage msg) =>
            //{
            //    localFileMgr.currentPath = "";
            //    CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
            //    reply.to = msg.from;
            //    reply.from = msg.to;
            //    reply.command = "getTopDirs";
            //    reply.arguments = localFileMgr.getDirs().ToList<string>();
            //    return reply;
            //};
            //messageDispatcher["getTopDirs"] = getTopDirs;

            Func<CommMessage, CommMessage> moveIntoFolderFiles = (CommMessage msg) =>
            {
                if (msg.arguments.Count() == 1)
                    localFileMgr.currentPath = msg.arguments[0];
                CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                reply.to = msg.from;
                reply.from = msg.to;
                reply.command = "moveIntoFolderFiles";

                reply.arguments = localFileMgr.getFiles().ToList<string>();
                return reply;
            };
            messageDispatcher["moveIntoFolderFiles"] = moveIntoFolderFiles;

            Func<CommMessage, CommMessage> moveIntoFolderDirs = (CommMessage msg) =>
            {
                if (msg.arguments.Count() == 1)
                    localFileMgr.currentPath = msg.arguments[0];
                CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                reply.to = msg.from;
                reply.from = msg.to;
                reply.command = "moveIntoFolderDirs";
                reply.arguments = localFileMgr.getDirs().ToList<string>();
                return reply;
            };
            messageDispatcher["moveIntoFolderDirs"] = moveIntoFolderDirs;

            Func<CommMessage, CommMessage> depAnalysis = (CommMessage msg) =>
            {
                string absDepAnalyzerPath = System.IO.Path.GetFullPath(MessagePassingComm.ServerEnvironment.DepAnalyzerPath);
                string anlyzDir;
                if (msg.arguments[0].Length >= 2)
                    anlyzDir = MessagePassingComm.ServerEnvironment.root + msg.arguments[0].Substring(2);
                else
                    anlyzDir = MessagePassingComm.ServerEnvironment.root;
                string optionDA = msg.arguments[1];
                string optionSC = msg.arguments[2];
                string optionFileRecursion = msg.arguments[3];
                string commandline = anlyzDir + ' ' + optionDA + ' ' + optionSC + ' ' + optionFileRecursion;

                CommMessage reply = createProcess(absDepAnalyzerPath, commandline, msg);
                return reply;
            };
            messageDispatcher["depAnalysis"] = depAnalysis;

        }

        CommMessage createProcess(string fileName, string commandline, CommMessage msg)
        {
            Process proc = new Process();

            proc.StartInfo.FileName = fileName;
            proc.StartInfo.Arguments = commandline;
            proc.EnableRaisingEvents = true;
            //proc.Exited += (sender, e) => childExited(sender, e, msg);

            Console.Write("\n  attempting to start {0}", fileName);
            try
            {
                proc.Start();
                proc.WaitForExit();

                List<string> results = readResultFile();

                CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                reply.to = msg.from;
                reply.from = msg.to;
                reply.command = "depAnalysis";
                reply.arguments = results;
                return reply;
            }
            catch (Exception ex)
            {
                Console.Write("\n  {0}", ex.Message);
                return null;
            }
        }

        List<string> readResultFile()
        {
            string[] lines = System.IO.File.ReadAllLines(MessagePassingComm.ServerEnvironment.resultFilePath);
            return lines.ToList<string>();
        }

        //void childExited(object sender, System.EventArgs e, CommMessage msg)
        //{
        //    Console.Write("\n  child process exited");

        //    //CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
        //    //reply.to = msg.from;
        //    //reply.from = msg.to;
        //    //reply.command = "depAnalysis";
        //    //reply.arguments = new List<string>{ "result"};

        //    //reply.show();
        //    //server.comm.postMessage(reply);
        //}


        /*----< Server processing >------------------------------------*/
        /*
         * - all server processing is implemented with the simple loop, below,
         *   and the message dispatcher lambdas defined above.
         */
        static void Main(string[] args)
        {
            TestUtilities.title("Starting Navigation Server", '=');
            try
            {
                DependencyAlyzServer server = new DependencyAlyzServer();
                server.initializeDispatcher();
                server.comm = new MessagePassingComm.Comm(ServerEnvironment.address, ServerEnvironment.port);

                while (true)
                {
                    CommMessage msg = server.comm.getMessage();
                    if (msg.type == CommMessage.MessageType.closeReceiver)
                        break;
                    msg.show();
                    if (msg.command == null)
                        continue;
                    CommMessage reply = server.messageDispatcher[msg.command](msg);
                    reply.show();
                    server.comm.postMessage(reply);
                }
            }
            catch (Exception ex)
            {
                Console.Write("\n  exception thrown:\n{0}\n\n", ex.Message);
            }
        }
    }
}
