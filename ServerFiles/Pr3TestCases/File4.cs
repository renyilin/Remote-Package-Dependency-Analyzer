using namespace2;
using namespace3;

namespace samenamespace
{
    public interface IFile4
    {
        void hello();
        void bye();
    }

    class File4
    {
        MyDelegate mydelegate; // dependency on File3.cs through delegate
        MyEnum en; // dependency on File2.cs through enum

        File4()
        {
            mydelegate = funct;
            en = new MyEnum();
            en = MyEnum.el1;
            funct2(en);
        }

        void funct(int a, int b)
        {
            funct2(en);
        }

        void funct2(MyEnum e)
        {

        }
    }
}
