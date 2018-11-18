/////////////////////////////////////////////////////////////////////
// MPCommService.cs - service for MessagePassingComm               //
// ver 2.1                                                         //
// Jim Fawcett, CSE681-OnLine, Summer 2017                         //
/////////////////////////////////////////////////////////////////////
/*
 * Started this project with C# Console Project wizard
 * - Added references to:
 *   - System.ServiceModel
 *   - System.Runtime.Serialization
 *   - System.Threading;
 *   - System.IO;
 *   
 * Package Operations:
 * -------------------
 * This package defines three classes:
 * - Sender which implements the public methods:
 *   -------------------------------------------
 *   - Sender           : constructs sender using address string and port
 *   - connect          : opens channel and attempts to connect to an endpoint, 
 *                        trying multiple times to send a connect message
 *   - close            : closes channel
 *   - postMessage      : posts to an internal thread-safe blocking queue, which
 *                        a sendThread then dequeues msg, inspects for destination,
 *                        and calls connect(address, port)
 *   - postFile         : attempts to upload a file in blocks
 *   - close            : closes current connection
 *   - getLastError     : returns exception messages on method failure
 *
 * - Receiver which implements the public methods:
 *   ---------------------------------------------
 *   - Receiver         : constructs Receiver instance
 *   - start            : creates instance of ServiceHost which services incoming messages
 *                        using address string and port of listener
 *   - postMessage      : Sender proxies call this message to enqueue for processing
 *   - getMessage       : called by Receiver application to retrieve incoming messages
 *   - close            : closes ServiceHost
 *   - openFileForWrite : opens a file for storing incoming file blocks
 *   - writeFileBlock   : writes an incoming file block to storage
 *   - closeFile        : closes newly uploaded file
 *   - size             : returns number of messages waiting in receive queue
 *   
 * - Comm which implements, using Sender and Receiver instances, the public methods:
 *   -------------------------------------------------------------------------------
 *   - Comm             : create Comm instance with address and port
 *   - postMessage      : send CommMessage instance to a Receiver instance
 *   - getMessage       : retrieves a CommMessage from a Sender instance
 *   - postFile         : called by a Sender instance to transfer a file
 *   - close()          : stops sender and receiver threads
 *   - restart          : attempts to restart with port - that must be different from
 *                        any port previously used while the embedding process states alive
 *   - closeConnection  : closes current connection, can reopen that or another connection
 *   - size             : returns number of messages in receive queue
 *    
 * The Package also implements the class TestPCommService with public methods:
 * ---------------------------------------------------------------------------
 * - testSndrRcvr()     : test instances of Sender and Receiver
 * - testComm()         : test Comm instance
 * - compareMsgs        : compare two CommMessage instances for near equality
 * - compareFileBytes   : compare two files byte by byte
 *
 * Required Files:
 * ---------------
 * IMPCommService.cs, MPCommService.cs
 * 
 * Maintenance History:
 * --------------------
 * ver 3.0 : 26 Oct 2017
 * - Receiver receive thread processing changed to discard connect messages
 * - added close, size, and restart functions
 * ver 2.1 : 20 Oct 2017
 * - minor changes to these comments
 * ver 2.0 : 19 Oct 2017
 * - renamed namespace and several components
 * - eliminated IPluggable.cs
 * ver 1.0 : 14 Jun 2017
 * - first release
 * 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Threading;
using System.IO;
using Navigator;

namespace MessagePassingComm
{
  ///////////////////////////////////////////////////////////////////
  // Receiver class - receives CommMessages and Files from Senders

  public class Receiver : IMessagePassingComm
  {
    public static SWTools.BlockingQueue<CommMessage> rcvQ { get; set; } = null;
    public bool restartFailed { get; set; } = false;
    ServiceHost commHost = null;
    FileStream fs = null;
    string lastError = "";

    /*----< constructor >------------------------------------------*/

    public Receiver()
    {
      if (rcvQ == null)
        rcvQ = new SWTools.BlockingQueue<CommMessage>();
    }
    /*----< create ServiceHost listening on specified endpoint >---*/
    /*
     * baseAddress is of the form: http://IPaddress or http://networkName
     */
    public bool start(string baseAddress, int port)
    {
      try
      {
        string address = baseAddress + ":" + port.ToString() + "/IMessagePassingComm";
        TestUtilities.putLine(string.Format("starting Receiver on thread {0}", Thread.CurrentThread.ManagedThreadId));
        createCommHost(address);
        restartFailed = false;
        return true;
      }
      catch(Exception ex)
      {
        restartFailed = true;
        Console.Write("\n{0}\n", ex.Message);
        Console.Write("\n  You can't restart a listener on a previously used port");
        Console.Write(" - Windows won't release it until the process shuts down");
        return false;
      }
    }
    /*----< create ServiceHost listening on specified endpoint >---*/
    /*
     * address is of the form: http://IPaddress:8080/IMessagePassingComm
     */
    public void createCommHost(string address)
    {
      WSHttpBinding binding = new WSHttpBinding();
      Uri baseAddress = new Uri(address);
      commHost = new ServiceHost(typeof(Receiver), baseAddress);
      commHost.AddServiceEndpoint(typeof(IMessagePassingComm), binding, baseAddress);
      commHost.Open();
    }
    /*----< enqueue a message for transmission to a Receiver >-----*/

    public void postMessage(CommMessage msg)
    {
      msg.threadId = Thread.CurrentThread.ManagedThreadId;
      TestUtilities.putLine(string.Format("sender enqueuing message on thread {0}", Thread.CurrentThread.ManagedThreadId));
      rcvQ.enQ(msg);
    }
    /*----< retrieve a message sent by a Sender instance >---------*/

    public CommMessage getMessage()
    {
      CommMessage msg = rcvQ.deQ();
      if (msg.type == CommMessage.MessageType.closeReceiver)
      {
        close();
      }
      if(msg.type == CommMessage.MessageType.connect)
      {
        msg = rcvQ.deQ();  // discarding the connect message
      }
      return msg;
    }
    /*----< how many messages in receive queue? >-----------------*/

    public int size()
    {
      return rcvQ.size();
    } 
    /*----< close ServiceHost >----------------------------------*/

    public void close()
    {
      Console.Write("\n  closing receiver - please wait");
      commHost.Close();
      (commHost as IDisposable).Dispose();
      
      Console.Write("\n  commHost.Close() returned");
    }
    /*---< called by Sender's proxy to open file on Receiver >-----*/

    public bool openFileForWrite(string name)
    {
      try
      {
        string writePath = Path.Combine(ServerEnvironment.root, name);
        fs = File.OpenWrite(writePath);
        return true;
      }
      catch(Exception ex)
      {
        lastError = ex.Message;
        return false;
      }
    }
    /*----< write a block received from Sender instance >----------*/

    public bool writeFileBlock(byte[] block)
    {
      try
      {
        fs.Write(block, 0, block.Length);
        return true;
      }
      catch (Exception ex)
      {
        lastError = ex.Message;
        return false;
      }
    }
    /*----< close Receiver's uploaded file >-----------------------*/

    public void closeFile()
    {
      fs.Close();
    }
  }
  ///////////////////////////////////////////////////////////////////
  // Sender class - sends messages and files to Receiver

  public class Sender
  {
    private IMessagePassingComm channel;
    private ChannelFactory<IMessagePassingComm> factory = null;
    private SWTools.BlockingQueue<CommMessage> sndQ = null;
    private int port = 0;
    private string fromAddress = "";
    private string toAddress = "";
    Thread sndThread = null;
    int tryCount = 0, maxCount = 10;
    string lastError = "";
    string lastUrl = "";

    /*----< constructor >------------------------------------------*/

    public Sender(string baseAddress, int listenPort)
    {
      port = listenPort;
      fromAddress = baseAddress + listenPort.ToString() + "/IMessagePassingComm";
      sndQ = new SWTools.BlockingQueue<CommMessage>();
      TestUtilities.putLine(string.Format("starting Sender on thread {0}", Thread.CurrentThread.ManagedThreadId));
      sndThread = new Thread(threadProc);
      sndThread.Start();
    }
    /*----< creates proxy with interface of remote instance >------*/

    public void createSendChannel(string address)
    {
      EndpointAddress baseAddress = new EndpointAddress(address);
      WSHttpBinding binding = new WSHttpBinding();
      factory = new ChannelFactory<IMessagePassingComm>(binding, address);
      channel = factory.CreateChannel();
    }
    /*----< attempts to connect to Receiver instance >-------------*/

    public bool connect(string baseAddress, int port)
    {
      toAddress = baseAddress + ":" + port.ToString() + "/IMessagePassingComm";
      return connect(toAddress);
    }
    /*----< attempts to connect to Receiver instance >-------------*/
    /*
     * - attempts a finite number of times to connect to a Receiver
     * - first attempt to send will throw exception of no listener
     *   at the specified endpoint
     * - to test that we attempt to send a connect message
     */
    public bool connect(string toAddress)
    {
      int timeToSleep = 500;
      TestUtilities.putLine("attempting to connect to \"" + toAddress + "\"");
      createSendChannel(toAddress);
      CommMessage connectMsg = new CommMessage(CommMessage.MessageType.connect);
      while (true)
      {
        try
        {
          channel.postMessage(connectMsg);
          tryCount = 0;
          return true;
        }
        catch (Exception ex)
        {
          if (++tryCount < maxCount)
          {
            TestUtilities.putLine("failed to connect - waiting to try again");
            Thread.Sleep(timeToSleep);
          }
          else
          {
            TestUtilities.putLine("failed to connect - quitting");
            lastError = ex.Message;
            return false;
          }
        }
      }
    }
    /*----< closes Sender's proxy >--------------------------------*/

    public void close()
    {
      while(sndQ.size() > 0)
      {
        CommMessage msg = sndQ.deQ();
        try
        {
          channel.postMessage(msg);
        }
        catch(Exception ex)
        {
          Console.Write(ex.Message);
        }
      }
        
      try
      {
        if (factory != null)
          factory.Close();
      }
      catch(Exception ex)
      {
        Console.Write("\n  already closed");
      }
    }
    /*----< processing for send thread >--------------------------*/
    /*
     * - send thread dequeues send message and posts to channel proxy
     * - thread inspects message and routes to appropriate specified endpoint
     */
    void threadProc()
    {
      while(true)
      {
        TestUtilities.putLine(string.Format("sender enqueuing message on thread {0}", Thread.CurrentThread.ManagedThreadId));

        CommMessage msg = sndQ.deQ();
        if (msg.type == CommMessage.MessageType.closeSender)
        {
          TestUtilities.putLine("Sender send thread quitting");
          break;
        }
        if (msg.to == lastUrl)
        {
          channel.postMessage(msg);
        }
        else
        {
          close();
          if (!connect(msg.to))
            continue;
          lastUrl = msg.to;
          channel.postMessage(msg);
        }
      }
    }
    /*----< main thread enqueues message for sending >-------------*/

    public void postMessage(CommMessage msg)
    {
      sndQ.enQ(msg);
    }
    /*----< uploads file to Receiver instance >--------------------*/

    public bool postFile(string fileName)
    {
      FileStream fs = null;
      long bytesRemaining;

      try
      {
        string path = Path.Combine(ClientEnvironment.root, fileName);
        fs = File.OpenRead(path);
        bytesRemaining = fs.Length;
        channel.openFileForWrite(fileName);
        while (true)
        {
          long bytesToRead = Math.Min(ClientEnvironment.blockSize, bytesRemaining);
          byte[] blk = new byte[bytesToRead];
          long numBytesRead = fs.Read(blk, 0, (int)bytesToRead);
          bytesRemaining -= numBytesRead;

          channel.writeFileBlock(blk);
          if (bytesRemaining <= 0)
            break;
        }
        channel.closeFile();
        fs.Close();
      }
      catch (Exception ex)
      {
        lastError = ex.Message;
        return false;
      }
      return true;
    }
  }
  ///////////////////////////////////////////////////////////////////
  // Comm class combines Receiver and Sender

  public class Comm
  {
    private Receiver rcvr = null;
    private Sender sndr = null;
    private string address = null;
    private int portNum = 0;

    /*----< constructor >------------------------------------------*/
    /*
     * - starts listener listening on specified endpoint
     */
    public Comm(string baseAddress, int port)
    {
      address = baseAddress;
      portNum = port;
      rcvr = new Receiver();
      rcvr.start(baseAddress, port);
      sndr = new Sender(baseAddress, port);
    }
    /*----< shutdown comm >----------------------------------------*/

    public void close()
    {
      Console.Write("\n  Comm closing");
      rcvr.close();
      sndr.close();
    }
    /*----< restart comm >-----------------------------------------*/

    public bool restart(int newport)
    {
      rcvr = new Receiver();
      rcvr.start(address, newport);
      if(rcvr.restartFailed)
      {
        return false;
      }
      sndr = new Sender(address, portNum);
      return true;
    }
    /*----< closes connection but keeps comm alive >---------------*/

    public void closeConnection()
    {
      sndr.close();
    }
    /*----< post message to remote Comm >--------------------------*/

    public void postMessage(CommMessage msg)
    {
      sndr.postMessage(msg);
    }
    /*----< retrieve message from remote Comm >--------------------*/

    public CommMessage getMessage()
    {
      return rcvr.getMessage();
    }
    /*----< called by remote Comm to upload file >-----------------*/

    public bool postFile(string filename)
    {
      return sndr.postFile(filename);
    }
    /*----< how many messages in receive queue? >-----------------*/

    public int size()
    {
      return rcvr.size();
    }
  }
  ///////////////////////////////////////////////////////////////////
  // TestPCommService class - tests Receiver, Sender, and Comm

  class TestPCommService
  {
    /*----< collect file names from client's FileStore >-----------*/

    public static List<string> getClientFileList()
    {
      List<string> names = new List<string>();
      string[] files = Directory.GetFiles(ClientEnvironment.root);
      foreach(string file in files)
      {
        names.Add(Path.GetFileName(file));
      }
      return names;
    }
    /*----< compare CommMessages property by property >------------*/
    /*
     * - skips threadId property
     */
    public static bool compareMsgs(CommMessage msg1, CommMessage msg2)
    {
      bool t1 = (msg1.type == msg2.type);
      bool t2 = (msg1.to == msg2.to);
      bool t3 = (msg1.from == msg2.from);
      bool t4 = (msg1.author == msg2.author);
      bool t5 = (msg1.command == msg2.command);
      //bool t6 = (msg1.threadId == msg2.threadId);
      bool t7 = (msg1.errorMsg == msg2.errorMsg);
      if (msg1.arguments.Count != msg2.arguments.Count)
        return false;
      for(int i=0; i<msg1.arguments.Count; ++i)
      {
        if (msg1.arguments[i] != msg2.arguments[i])
          return false;
      }
      return t1 && t2 && t3 && t4 && t5 && /*t6 &&*/ t7;
    }
    /*----< compare binary file's bytes >--------------------------*/

    static bool compareFileBytes(string filename)
    {
      TestUtilities.putLine(string.Format("testing byte equality for \"{0}\"", filename));

      string fileSpec1 = Path.Combine(ClientEnvironment.root, filename);
      string fileSpec2 = Path.Combine(ServerEnvironment.root, filename);
      try
      {
        byte[] bytes1 = File.ReadAllBytes(fileSpec1);
        byte[] bytes2 = File.ReadAllBytes(fileSpec2);
        if (bytes1.Length != bytes2.Length)
          return false;
        for (int i = 0; i < bytes1.Length; ++i)
        {
          if (bytes1[i] != bytes2[i])
            return false;
        }
      }
      catch (Exception ex)
      {
        TestUtilities.putLine(string.Format("\n  {0}\n", ex.Message));
        return false;
      }
      return true;
    }
    /*----< test Sender and Receiver classes >---------------------*/

    public static bool testSndrRcvr()
    {
      TestUtilities.vbtitle("testing Sender & Receiver");

      bool test = true;
      Receiver rcvr = new Receiver();
      rcvr.start("http://localhost", 8080);
      Sender sndr = new Sender("http://localhost", 8080);

      CommMessage sndMsg = new CommMessage(CommMessage.MessageType.request);
      sndMsg.command = "show";
      sndMsg.author = "Jim Fawcett";
      sndMsg.to = "http://localhost:8080/IMessagePassingComm";
      sndMsg.from = "http://localhost:8080/IMessagePassingComm";

      sndr.postMessage(sndMsg);
      CommMessage rcvMsg;
      rcvMsg = rcvr.getMessage();
      if (ClientEnvironment.verbose)
        rcvMsg.show();
      if (!compareMsgs(sndMsg, rcvMsg))
        test = false;
      TestUtilities.checkResult(test, "sndMsg equals rcvMsg");
      TestUtilities.putLine();

      sndMsg.type = CommMessage.MessageType.closeReceiver;
      sndr.postMessage(sndMsg);
      rcvMsg = rcvr.getMessage();
      if (ClientEnvironment.verbose)
        rcvMsg.show();
      if (!compareMsgs(sndMsg, rcvMsg))
        test = false;
      TestUtilities.checkResult(test, "Close Receiver");
      TestUtilities.putLine();

      sndMsg.type = CommMessage.MessageType.closeSender;
      if (ClientEnvironment.verbose)
        sndMsg.show();
      sndr.postMessage(sndMsg);
      // rcvr.getMessage() would fail because server has shut down
      // no rcvMsg so no compare

      TestUtilities.putLine("last message received\n");
      return test;
    }
    /*----< test Comm instance >-----------------------------------*/
    /*
     * - Note: change every occurance of string "Odin" to your machine name
     * 
     */
    public static bool testComm()
    {
      TestUtilities.vbtitle("testing Comm");
      bool test = true;

      Comm comm = new Comm("http://localhost", 8081);
      CommMessage csndMsg = new CommMessage(CommMessage.MessageType.request);

      csndMsg.command = "show";
      csndMsg.author = "Jim Fawcett";
      string localEndPoint = "http://localhost:8081/IMessagePassingComm";
      csndMsg.to = localEndPoint;
      csndMsg.from = localEndPoint;

      comm.postMessage(csndMsg);
      CommMessage crcvMsg = comm.getMessage();
      if (ClientEnvironment.verbose)
        crcvMsg.show();
      if (!compareMsgs(csndMsg, crcvMsg))
        test = false;
      TestUtilities.checkResult(test, "csndMsg equals crcvMsg");
      TestUtilities.putLine(comm.size().ToString() + " messages left in queue");
      TestUtilities.putLine();

      TestUtilities.vbtitle("testing connect to new EndPoint");
      csndMsg.to = "http://DESKTOP-J98AUHN:8081/IMessagePassingComm";
      comm.postMessage(csndMsg);
      crcvMsg = comm.getMessage();
      if (ClientEnvironment.verbose)
        crcvMsg.show();
      if (!compareMsgs(csndMsg, crcvMsg))
        test = false;
      TestUtilities.checkResult(test, "csndMsg equals crcvMsg");
      TestUtilities.putLine(comm.size().ToString() + " messages left in queue");
      TestUtilities.putLine();

      TestUtilities.vbtitle("testing file transfer");

      bool testFileTransfer = true;

      List<string> names = getClientFileList();
      foreach (string name in names)
      {
        TestUtilities.putLine(string.Format("transferring file \"{0}\"", name));
        bool transferSuccess = comm.postFile(name);
        TestUtilities.checkResult(transferSuccess, "transfer");
      }

      foreach (string name in names)
      {
        if (!compareFileBytes(name))
        {
          testFileTransfer = false;
          break;
        }
      }
      TestUtilities.checkResult(testFileTransfer, "file transfers");
      TestUtilities.putLine(comm.size().ToString() + " messages left in queue");
      TestUtilities.putLine();

      TestUtilities.vbtitle("test closeConnection then postMessage");
      comm.closeConnection();
      CommMessage newMsg = new CommMessage(CommMessage.MessageType.request);
      newMsg.to = localEndPoint;
      newMsg.from = localEndPoint;
      comm.postMessage(newMsg);
      CommMessage reply = comm.getMessage();
      reply.show();
      // if we get here, test passed
      TestUtilities.checkResult(true, "closeSenderConnenction then PostMessage");
      TestUtilities.putLine(comm.size().ToString() + " messages left in queue");
      TestUtilities.putLine();

      TestUtilities.vbtitle("test receiver close");
      csndMsg.type = CommMessage.MessageType.closeReceiver;
      if(ClientEnvironment.verbose)
        csndMsg.show();
      comm.postMessage(csndMsg);
      crcvMsg = comm.getMessage();
      if (ClientEnvironment.verbose)
        crcvMsg.show();
      if (!compareMsgs(csndMsg, crcvMsg))
        test = false;
      TestUtilities.checkResult(test, "closeReceiver");
      TestUtilities.putLine(comm.size().ToString() + " messages left in queue");
      TestUtilities.putLine();

      csndMsg.type = CommMessage.MessageType.closeSender;
      comm.postMessage(csndMsg);
      if(ClientEnvironment.verbose)
        csndMsg.show();
      TestUtilities.putLine(comm.size().ToString() + " messages left in queue");
      // comm.getMessage() would fail because server has shut down
      // no rcvMsg so no compare

      TestUtilities.putLine("last message received\n");

      TestUtilities.putLine("Test comm.restart on same port - expected to fail");

      if (comm.restart(8081))
      {
        CommMessage newerMsg = new CommMessage(CommMessage.MessageType.request);
        newerMsg.to = ClientEnvironment.endPoint;
        newerMsg.from = ClientEnvironment.endPoint;
        comm.postMessage(newerMsg);
        CommMessage newReply = comm.getMessage();
        newReply.show();
      }
      else
      {
        Console.Write("\n  can't restart but won't fail test");
      }

      return test && testFileTransfer;
    }
    /*----< do the tests >-----------------------------------------*/

    static void Main(string[] args)
    {
      ClientEnvironment.verbose = true;
      TestUtilities.vbtitle("testing Message-Passing Communication", '=');

      /*----< uncomment to see Sender & Receiver testing >---------*/
      //TestUtilities.checkResult(testSndrRcvr(), "Sender & Receiver");
      //TestUtilities.putLine();

      TestUtilities.checkResult(testComm(), "Comm");
      TestUtilities.putLine();

      TestUtilities.putLine("Press key to quit\n");
      if(ClientEnvironment.verbose)
        Console.ReadKey();
    }
  }
}
