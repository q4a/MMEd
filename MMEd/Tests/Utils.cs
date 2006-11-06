using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using NUnit.Framework;
using MMEd.Util;

namespace MMEd.Tests
{
  [TestFixture]
  public class UtilTests
  {
    [Test]
    public void TestEscape()
    {
      Assert.AreEqual("asdf\\0as\\u62378df", Utils.EscapeString("asdf\0as\u62378df"));
    }

    [Test]
    public void TestPS16bitColors()
    {
      Random rnd = new Random();
      for (int i = 0; i < 10; i++)
      {
        int rndRGB = (rnd.Next() & 0xf8f8f8) | unchecked((int)0xff000000);
        short there = Utils.ARGBColorToPS16bit(rndRGB);
        int back = Utils.PS16bitColorToARGB(there);
        Assert.AreEqual(string.Format("{0:x}", rndRGB), string.Format("{0:x}", back));
        Assert.AreEqual(rndRGB, back);
      }
      short[] specificTests = { unchecked((short)0x8000) };
      for (int i = 0; i < 10 + specificTests.Length; i++)
      {
        short rndPScol = i < specificTests.Length
            ? specificTests[i]
            : (short)(rnd.Next() & 0x7fff);
        int there = Utils.PS16bitColorToARGB(rndPScol);
        short back = Utils.ARGBColorToPS16bit(there);
        Assert.AreEqual(rndPScol, back);
      }
    }

    [Test]
    public void TestPS32bitColors()
    {
      Random rnd = new Random();
      for (int i = 0; i < 10; i++)
      {
        int rndRGB = rnd.Next(int.MinValue, int.MaxValue);
        Color there = Utils.PSRGBColorToColor(rndRGB);
        int back = Utils.ColorToPSRGBColor(there);
        Assert.AreEqual(string.Format("{0:x}", rndRGB), string.Format("{0:x}", back));
        Assert.AreEqual(rndRGB, back);
      }
      Color[] specificTests = { Color.FromArgb(unchecked((int)0xffffffff)) };
      for (int i = 0; i < 10 + specificTests.Length; i++)
      {
        Color rndcol = i < specificTests.Length
            ? specificTests[i]
            : Color.FromArgb(rnd.Next());
        int there = Utils.ColorToPSRGBColor(rndcol);
        Color back = Utils.PSRGBColorToColor(there);
        Assert.AreEqual(rndcol, back);
      }
    }
  }
}
