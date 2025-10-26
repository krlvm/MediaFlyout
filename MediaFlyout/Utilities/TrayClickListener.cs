﻿using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MediaFlyout.Helpers
{
    class TrayClickListener
    {
        public event MouseEventHandler Click;
        public event MouseEventHandler DoubleClick;

        private bool _isDoubleClick = false;

        public TrayClickListener(NotifyIcon notifyIcon)
        {
            notifyIcon.MouseClick += Icon_OnClick;
            notifyIcon.DoubleClick += Icon_OnDoubleClick;
        }

        private async void Icon_OnClick(object sender, MouseEventArgs args)
        {
            if (args.Button != MouseButtons.Left)
            {
                Click?.Invoke(this, args);
                return;
            }

            _isDoubleClick = false;
            await Task.Delay(TimeSpan.FromMilliseconds(SystemInformation.DoubleClickTime / 4));

            if (_isDoubleClick)
            {
                DoubleClick?.Invoke(this, args);
            }
            else
            {
                Click?.Invoke(this, args);
            }
        }

        private void Icon_OnDoubleClick(object sender, EventArgs args)
        {
            _isDoubleClick = true;
        }
    }
}