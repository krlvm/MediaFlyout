using System.Windows.Forms;

namespace MediaFlyout.Helpers
{
    class TrayClickListener
    {
        public event MouseEventHandler Click;
        public event MouseEventHandler DoubleClick;

        private MouseButtons _lastMouseButton;
        private bool _ignoreNextMouseUp;

        public TrayClickListener(NotifyIcon notifyIcon)
        {
            notifyIcon.MouseUp += Icon_OnMouseUp;
            notifyIcon.DoubleClick += Icon_OnDoubleClick;
        }

        private void Icon_OnMouseUp(object sender, MouseEventArgs args)
        {
            // WinForms raises DoubleClick on the second button-down and suppresses
            // Click/MouseClick on the matching button-up. MouseUp still arrives.
            if (_ignoreNextMouseUp)
            {
                _ignoreNextMouseUp = false;
                return;
            }

            _lastMouseButton = args.Button;
            Click?.Invoke(this, args);
        }

        private void Icon_OnDoubleClick(object sender, System.EventArgs args)
        {
            _ignoreNextMouseUp = true;
            DoubleClick?.Invoke(this, new MouseEventArgs(_lastMouseButton, 2, 0, 0, 0));
        }
    }
}
