using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MediaFlyout.Helpers
{
    class TrayClickListener
    {
        private bool isSingleClick = false;

        public event EventHandler Click;
        public event EventHandler DoubleClick;

        public TrayClickListener(NotifyIcon notifyIcon)
        {
            notifyIcon.Click += Icon_OnClick;
            notifyIcon.DoubleClick += Icon_OnDoubleClick;
        }

        private async void Icon_OnClick(object sender, EventArgs args)
        {
            isSingleClick = true;
            await Task.Delay(TimeSpan.FromMilliseconds(SystemInformation.DoubleClickTime / 4));

            if (isSingleClick)
            {
                Click?.Invoke(this, null);
            }
            else
            {
                DoubleClick?.Invoke(this, null);
            }
        }

        private void Icon_OnDoubleClick(object sender, EventArgs args)
        {
            isSingleClick = false;
        }
    }
}