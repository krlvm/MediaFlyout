using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MediaFlyout.Helpers
{
    class TrayClickListener
    {
        public event MouseEventHandler Click;
        public event MouseEventHandler DoubleClick;

        private CancellationTokenSource _pendingClick;
        private MouseEventArgs _pendingClickArgs;
        private bool _ignoreNextClick;

        public TrayClickListener(NotifyIcon notifyIcon)
        {
            // NotifyIcon.Click is the reliable left-button event. MouseClick is
            // retained for right and middle clicks, which carry button information.
            notifyIcon.Click += Icon_OnClick;
            notifyIcon.MouseClick += Icon_OnMouseClick;
            notifyIcon.DoubleClick += Icon_OnDoubleClick;
        }

        private async void Icon_OnClick(object sender, EventArgs args)
        {
            if (_ignoreNextClick)
            {
                _ignoreNextClick = false;
                return;
            }

            _pendingClick?.Cancel();
            var pendingClick = new CancellationTokenSource();
            _pendingClick = pendingClick;
            _pendingClickArgs = new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0);

            try
            {
                // Allow Windows to report a double-click before treating this as
                // a single-click request to open or dismiss the flyout.
                await Task.Delay(SystemInformation.DoubleClickTime, pendingClick.Token);
                if (ReferenceEquals(_pendingClick, pendingClick))
                {
                    _pendingClick = null;
                    Click?.Invoke(this, _pendingClickArgs);
                    _pendingClickArgs = null;
                }
            }
            catch (TaskCanceledException)
            {
                // The double-click handler consumed this click.
            }
            finally
            {
                pendingClick.Dispose();
            }
        }

        private void Icon_OnMouseClick(object sender, MouseEventArgs args)
        {
            if (args.Button != MouseButtons.Left)
            {
                Click?.Invoke(this, args);
            }
        }

        private void Icon_OnDoubleClick(object sender, EventArgs args)
        {
            _ignoreNextClick = true;
            _pendingClick?.Cancel();
            _pendingClick = null;
            DoubleClick?.Invoke(this, _pendingClickArgs ?? new MouseEventArgs(MouseButtons.Left, 2, 0, 0, 0));
            _pendingClickArgs = null;
        }
    }
}
