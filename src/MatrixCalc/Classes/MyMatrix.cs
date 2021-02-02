using System;
using System.Collections.Generic;
using System.Threading;

namespace MatrixCalc.Classes
{
    public class MyMatrix
    {
        public List<List<decimal>> Matrix { get; set; }

        public int Row { get; }
        public int Column { get; }
        
        #region Constructor

        public MyMatrix(int row, int column)
        {
            Row = row;
            Column = column;

            Matrix = new List<List<decimal>>(row);

            for (var i = 0; i < row; i++)
            {
                Matrix.Add(new List<decimal>());
                for (var j = 0; j < column; j++)
                    Matrix[i].Add(0);
            }
        }

        #endregion

        #region Helpers

        public void Random(int mySeed = 0)
        {
            var rnd = new Random(Environment.TickCount + mySeed);

            for (var i = 0; i < Row; i++)
            for (var j = 0; j < Column; j++)
                Matrix[i][j] = rnd.Next(10);
        }

        public void Random(int min, int max, int mySeed = 0)
        {
            var rnd = new Random(Environment.TickCount + mySeed);

            for (var i = 0; i < Row; i++)
            for (var j = 0; j < Column; j++)
                Matrix[i][j] = rnd.Next(min, max);
        }

        private static readonly Func<MyMatrix, MyMatrix, Action<object>, MyMatrix> Multithreading = (firstMatrix, secondMatrix, action) =>
        {
            var newMatrix = new MyMatrix(firstMatrix.Row, secondMatrix.Column);

            var threadsPool = new List<Thread>();
            var rangeSize = (int)Math.Ceiling(firstMatrix.Row / (double)Environment.ProcessorCount);

            for (var row = 0; row < firstMatrix.Row; row += rangeSize)
            {
                var thread = new Thread(action.Invoke);
                thread.Start(new Helper
                {
                    Begin = row,
                    End = row + rangeSize > firstMatrix.Row
                        ? firstMatrix.Row - 1
                        : row + rangeSize - 1,

                    FirstMatrix = firstMatrix,
                    SecondMatrix = secondMatrix,
                    NewMatrix = newMatrix
                });

                threadsPool.Add(thread);
            }

            threadsPool.WaitAll();

            return newMatrix;
        };

        public override string ToString()
        {
            var str = "";

            for (var i = 0; i < Row; i++)
            {
                for (var j = 0; j < Column; j++)
                    str += Matrix[i][j] + " ";
                str += "\n";
            }

            return str;
        }

        #endregion

        #region Operators

        #region Addition

        public static MyMatrix operator +(MyMatrix firstMatrix, MyMatrix secondMatrix)
        {
            return Multithreading(firstMatrix, secondMatrix, Addition);
        }
        
        private static void Addition(object value)
        {
            var range = (Helper) value;
            for (var i = range.Begin; i <= range.End; i++)
            for (var j = 0; j < range.FirstMatrix.Column; j++)
                range.NewMatrix.Matrix[i][j] = range.FirstMatrix.Matrix[i][j] + range.SecondMatrix.Matrix[i][j];
        }

        #endregion

        #region Subtraction

        public static MyMatrix operator -(MyMatrix firstMatrix, MyMatrix secondMatrix)
        {
            return Multithreading(firstMatrix, secondMatrix, Subtraction);
        }

        private static void Subtraction(object value)
        {
            var range = (Helper)value;
            for (var i = range.Begin; i <= range.End; i++)
            for (var j = 0; j < range.FirstMatrix.Column; j++)
                range.NewMatrix.Matrix[i][j] = range.FirstMatrix.Matrix[i][j] - range.SecondMatrix.Matrix[i][j];
        }

        #endregion

        #region Multiplication

        public static MyMatrix operator *(MyMatrix firstMatrix, MyMatrix secondMatrix)
        {
            return Multithreading(firstMatrix, secondMatrix, Multiplication);
        }

        public static void Multiplication(object value)
        {
            var range = (Helper)value;

            for (var i = range.Begin; i <= range.End; i++)
            {
                for (var j = 0; j < range.SecondMatrix.Column; j++)
                {
                    decimal sum = 0;

                    for (var k = 0; k < range.FirstMatrix.Column; k++)
                        sum += range.FirstMatrix.Matrix[i][k] * range.SecondMatrix.Matrix[k][j];

                    range.NewMatrix.Matrix[i][j] = sum;
                }
            }
        }

        #endregion

        #endregion
    }

    public class Helper
    {
        public int Begin;
        public int End;

        public MyMatrix FirstMatrix, SecondMatrix, NewMatrix;
    }

    public static class ThreadExtension
    {
        public static void WaitAll(this IEnumerable<Thread> threads)
        {
            if (threads == null) return;
            foreach (var thread in threads)
                thread.Join();
        }
    }
}