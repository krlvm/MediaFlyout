using MediaFlyout.Interop;
using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace MediaFlyout.Helpers
{
    class AnimationHelper
    {
        private const double ANIMATION_TIME = 0.25;

        public static void ShowFlyout<T>(T window, bool topmost = false) where T : Window, IFlyout
        {
            DependencyProperty property;
            double from, to;

            var taskbar = WindowsTaskbar.Current;

            if (taskbar.Side == WindowsTaskbar.Position.Bottom || taskbar.Side == WindowsTaskbar.Position.Top)
            {
                property = Window.TopProperty;
                window.Left = taskbar.Right - window.Width + window.BorderThickness.Right;
            } 
            else
            {
                property = Window.LeftProperty;
                window.Top = taskbar.Bottom - window.Height + window.BorderThickness.Bottom + 1;
            }

            switch (taskbar.Side)
            {
                case WindowsTaskbar.Position.Bottom:
                    from = taskbar.Bottom;
                    to = taskbar.Bottom - window.Height + window.BorderThickness.Bottom + 1;
                    break;
                case WindowsTaskbar.Position.Top:
                    from = 0;
                    to = taskbar.Bottom - window.BorderThickness.Top;
                    break;
                case WindowsTaskbar.Position.Left:
                    from = 0;
                    to = taskbar.Right - window.BorderThickness.Left;
                    break;
                case WindowsTaskbar.Position.Right:
                    from = taskbar.Right;
                    to = taskbar.Right - window.Width + window.BorderThickness.Right;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            //window.IsRaising = true;
            BringTaskbarToFront();
            window.Visibility = Visibility.Visible;
            window.WindowStyle = WindowStyle.SingleBorderWindow;
            window.Activate();
            InteropHelper.CloakWindow(window, false);

            if (!SystemParameters.MenuAnimation)
            {
                if(property == Window.TopProperty)
                {
                    window.Top = to;
                } else
                {
                    window.Left = to;
                }
                return;
            }

            var easingMode = to - from > 0 ? EasingMode.EaseOut : EasingMode.EaseIn;
            easingMode = EasingMode.EaseOut;

            var entraceAnimation = new DoubleAnimation
            {
                Duration = TimeSpan.FromSeconds(ANIMATION_TIME),
                From = from,
                To = to,
                EasingFunction = new ExponentialEase
                {
                    EasingMode = easingMode
                }
            };
            Storyboard.SetTarget(entraceAnimation, window);
            Storyboard.SetTargetProperty(entraceAnimation, new PropertyPath(property));

            var fadeAnimation = new DoubleAnimation
            {
                Duration = TimeSpan.FromSeconds(ANIMATION_TIME),
                From = 0.5,
                To = 1,
                EasingFunction = new ExponentialEase
                {
                    EasingMode = easingMode
                }
            };
            Storyboard.SetTarget(fadeAnimation, window);
            Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath(Window.OpacityProperty));

            var sb = new Storyboard();
            sb.FillBehavior = FillBehavior.Stop;
            sb.Children.Add(entraceAnimation);
            sb.Children.Add(fadeAnimation);
            sb.Completed += (object sender, EventArgs e) =>
            {
                window.Topmost = topmost;
                window.Focus();
            };

            sb.Begin(window);
        }

        public static void BringTaskbarToFront()
        {
            NativeMethods.SetForegroundWindow(NativeMethods.FindWindow("Shell_TrayWnd", null));
        }
    }

    interface IFlyout
    {
        bool IsRaising { get; set; }
    }
}
