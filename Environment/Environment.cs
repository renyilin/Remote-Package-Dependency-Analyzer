///////////////////////////////////////////////////////////////////////////
// Environment.cs - defines environment properties for Client and Server //
// ver 1.0                                                               //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2017       //
///////////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * ----------------------
 * This package defines environment properties for Client and Server.
 * 
 * Required Files:
 * ---------------
 * Environment.cs
 * 
 * Maintenance History:
 * --------------------
 * ver 1.0 : 3 Dec 2018
 * - first release
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MessagePassingComm
{
    ///////////////////////////////////////////////////////////////////
    // Environment properties
    public struct Environment
  {
    public static string root { get; set; }
    public const long blockSize = 1024;
    public static string endPoint { get; set; }
    public static string address { get; set; }
    public static int port { get; set; }
    public static bool verbose { get; set; }
  }

    ///////////////////////////////////////////////////////////////////
    // Client Environment properties
    public struct ClientEnvironment
  {
    public static string root { get; set; } = "..\\..\\..\\ClientFiles\\";
    public const long blockSize = 1024;
    public static string endPoint { get; set; } = "http://localhost:8090/IMessagePassingComm";
    public static string address { get; set; } = "http://localhost";
    public static int port { get; set; } = 8090;
    public static bool verbose { get; set; } = false;
  }

    ///////////////////////////////////////////////////////////////////
    // Server Environment properties
    public struct ServerEnvironment
  {
    public static string root { get; set; } = "..\\..\\..\\ServerFiles\\";
    public const long blockSize = 1024;
    public static string endPoint { get; set; } = "http://localhost:8080/IMessagePassingComm";
    public static string address { get; set; } = "http://localhost";
    public static int port { get; set; } = 8080;
    public static bool verbose { get; set; } = false;
    public static string DepAnalyzerPath { get; set; } = "..\\..\\..\\Executive\\bin\\Debug\\Executive.exe";
    public static string resultFilePath { get; set; } = ".\\result.txt";
   }
}
