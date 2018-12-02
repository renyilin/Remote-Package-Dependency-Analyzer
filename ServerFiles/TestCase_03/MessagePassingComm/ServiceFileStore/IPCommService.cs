/////////////////////////////////////////////////////////////////////
// IPCommService.cs - service interface for PluggableComm          //
// ver 1.0                                                         //
// Jim Fawcett, CSE681-OnLine, Summer 2017                         //
/////////////////////////////////////////////////////////////////////
/*
 * Added references to:
 * - System.ServiceModel
 * - System.Runtime.Serialization
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.Threading;

namespace PluggableRepository
{
  using IP = String;
  using Port = Int32;

  using Command = String;             // Command is key for message dispatching, e.g., Dictionary<Command, Func<bool>>
  //using EndPoint = Tuple<IP, Port>;   // string is ip address or machine name, int is port number
  using Argument = String;      
  using ErrorMessage = String;

  public struct ServiceClientEnvironment
  {
    public const string fileStorage = "../../../ClientFileStore";
    public const long blockSize = 1024;
    public static string baseAddress { get; set; }
  }

  public struct ServiceEnvironment
  {
    public const string fileStorage = "../../../ServiceFileStore";
    public static string baseAddress { get; set; }
  }

  [ServiceContract(Namespace = "IPluggableRepository")]
  public interface IPluggableComm
  {
    /*----< support for message passing >--------------------------*/

    [OperationContract]
    void postMessage(CommMessage msg);

    // private to receiver so not an OperationContract
    CommMessage getMessage();

    /*----< support for sending file in blocks >-------------------*/

    [OperationContract]
    bool openFileForWrite(string name);

    [OperationContract]
    bool writeFileBlock(byte[] block);

    [OperationContract]
    void closeFile();
  }

  [DataContract]
  public class CommMessage
  {
    public enum MessageType
    {
      [EnumMember]
      connect,
      [EnumMember]
      request,
      [EnumMember]
      reply,
      [EnumMember]
      closeSender,
      [EnumMember]
      closeReceiver
    }

    public CommMessage(MessageType mt)
    {
      type = mt;
    }

    [DataMember]
    public MessageType type { get; set; } = MessageType.connect;

    [DataMember]
    public string to { get; set; }

    [DataMember]
    public string from { get; set; }

    [DataMember]
    public string author { get; set; }

    [DataMember]
    public Command command { get; set; }

    [DataMember]
    public List<Argument> arguments { get; set; } = new List<Argument>();

    [DataMember]
    public int threadId { get; set; } = Thread.CurrentThread.ManagedThreadId;

    [DataMember]
    public ErrorMessage errorMsg { get; set; } = "no error";

    public void show()
    {
      Console.Write("\n  CommMessage:");
      Console.Write("\n    MessageType : {0}", type.ToString());
      Console.Write("\n    to          : {0}", to);
      Console.Write("\n    from        : {0}", from);
      Console.Write("\n    author      : {0}", author);
      Console.Write("\n    command     : {0}", command);
      Console.Write("\n    arguments   :");
      if (arguments.Count > 0)
        Console.Write("\n      ");
      foreach (string arg in arguments)
        Console.Write("{0} ", arg);
      Console.Write("\n    ThreadId    : {0}", threadId);
      Console.Write("\n    errorMsg    : {0}\n", errorMsg);
    }
  }
}
