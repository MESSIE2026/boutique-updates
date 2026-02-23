using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BoutiqueRebuildFixed
{
    public static class UiHelpers
    {
        public static void EnableDoubleBuffering(Control c)
        {
            if (c == null) return;

            // 1) DoubleBuffered (protected)
            typeof(Control).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(c, true, null);

            // 2) SetStyle (protected)
            typeof(Control).InvokeMember("SetStyle",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod,
                null, c,
                new object[] { ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true });

            // 3) UpdateStyles (protected)
            typeof(Control).InvokeMember("UpdateStyles",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod,
                null, c, null);
        }
    }
}