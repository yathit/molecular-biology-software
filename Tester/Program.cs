using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioCSharp.Seqs;
using NUnit.Framework;
using NUnit.Core;
using NUnit.Core.Tests;
using iMoBio.Primer;

namespace Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            PlasmidCanvasTest test = new PlasmidCanvasTest();
            test.SpaceAdjustTest();
            Console.ReadLine();

        }
    }


   
}
