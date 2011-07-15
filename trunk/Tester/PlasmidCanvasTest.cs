﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using iMoBio.PlasmidCanvas;
using MathLib;

namespace Tester
{
    [TestFixture]
    public class PlasmidCanvasTest
    {
        public void SpaceAdjustTest()
        {
            double[] u = new double[] { 0.000656886358659952, 0.00813809211006496, 0.0825122253850084, 0.153382964747099, 0.161229107364426, 0.172688124954383, 0.176082037807459, 0.287497262973506, 0.403766148456317, 0.479673016568134, 0.500036493686592, 0.509597839573754, 0.570031384570469, 0.570031384570469, 0.630939347492884, 0.641631997664404, 0.690497044011386, 0.690497044011386, 0.759981023282972, 0.801255382818772, 0.815049996350631, 0.853842785198161, 0.912561126925042, 0.983687322093278 };
            double v = 0.0570815450643777;
            double[] r_1 = new double[] { -0.162468349949708, -0.10538680488533, -0.0483052598209524, 0.0087762852434253, 0.065857830307803, 0.122939375372181, 0.180020920436558, 0.237102465500936, 0.294184010565314, 0.351265555629691, 0.408347100694069, 0.465428645758447, 0.522510190822824, 0.579591735887202, 0.63667328095158, 0.693754826015958, 0.750836371080335, 0.807917916144713, 0.864999461209091, 0.922081006273468, 0.979162551337846, 1.03624409640222, 1.0933256414666, 1.15040718653098 };
            double[] r = PlasmidCanvas.SpaceAdjust(u, v);
            double[] rem = r.Minus(r_1);
            Console.Write("Total error: {0}", rem.Sum());
            Assert.IsTrue(rem.All(x => Math.Abs(x) < 1E-10));
     
        }
    }
}