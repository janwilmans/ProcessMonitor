using System;
using System.Drawing;
using System.Windows.Forms;

namespace ProcessMonitor
{
    class MyToolTip : System.Windows.Forms.ToolTip
    {
        public MyToolTip()
        {
            OwnerDraw = true;
            Draw += OnDrawEvent;
            Popup += OnPopupEvent;
        }

        void OnDrawEvent(object sender, DrawToolTipEventArgs e)
        {
            // Draw the background and border.
            //e.DrawBackground();
            e.DrawBorder();

            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Near;
            sf.LineAlignment = StringAlignment.Center;
            int Margin = 5;
            int image_wid = 2 * Margin + 100;
            Rectangle rect = new Rectangle(image_wid, 0, e.Bounds.Width - image_wid, e.Bounds.Height);
            e.Graphics.DrawString(e.ToolTipText, e.Font, Brushes.Green, rect, sf);
        }

        void OnPopupEvent(object sender, PopupEventArgs e)
        {
            e.ToolTipSize = new Size(100, 100);
        }

    }
}
