
using namespace3;

namespace namespace2
{
    using alias = File3; // dependency on File3.cs

    public enum MyEnum
    {
        el1,
        el2,
        el3
    }

    class File2
    {
        public File2()
        {
            alias f = new alias();


        }
    }
}
