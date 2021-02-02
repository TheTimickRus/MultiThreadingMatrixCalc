using System;
using MatrixCalc.Classes;

namespace MatrixCalc
{
    public class MainClass
    {
        public static void Main()
        {
            var a = new MyMatrix(2, 2);
            var b = new MyMatrix(2, 2);

            a.Random(1, 9, 1);
            b.Random(1, 9, 2);

            var c = a + b;

            Console.WriteLine(a);
            Console.WriteLine(b);
            Console.WriteLine(c);

            Console.ReadKey();
        }
    }
}
