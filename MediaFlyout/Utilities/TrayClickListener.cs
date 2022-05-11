using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MediaFlyout.Helpers
{
    class TrayClickListener
    {
        private bool isSingleClick = false;

        public event MouseEventHandler Click;
        public event MouseEventHandler DoubleClick;

        public TrayClickListener(NotifyIcon notifyIcon)
        {
            notifyIcon.MouseClick += Icon_OnClick;
            notifyIcon.DoubleClick += Icon_OnDoubleClick;
        }

        private async void Icon_OnClick(object sender, MouseEventArgs args)
        {
            isSingleClick = true;
            await Task.Delay(TimeSpan.FromMilliseconds(SystemInformation.DoubleClickTime / 4));

            if (isSingleClick)
            {
                Click?.Invoke(this, args);
            }
            else
            {
                DoubleClick?.Invoke(this, args);
            }
        }

        private void Icon_OnDoubleClick(object sender, EventArgs args)
        {
            isSingleClick = false;
        }
    }
}