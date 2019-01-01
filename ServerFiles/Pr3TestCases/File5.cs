using System;


namespace samenamespace
{
    public class File5: IFile4  // dependency on File4.cs through interface
    {
        //----------------test function displaying a message on console------------------------------
        public void hello()
        {
            Console.Write("\nHello");
        }
        public void bye()
        {
            Console.Write("\nBye");
        }
    }
}
