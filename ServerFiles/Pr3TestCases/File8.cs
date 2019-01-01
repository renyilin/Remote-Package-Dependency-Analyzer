using File6;

namespace namespace8
{
    public class File8
    {
      public File8()
        {
            A a = new A();  // dependency on File6.cs through struct
            // NO DEPENDENCY ON File7.cs 
            a.hello();
        }
    }
}
