using MediaFlyout.Interop;
using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace MediaFlyout.Helpers
{
    class AnimationHelper
    {
        private const double ANIMATION_TIME_ENTRACE = 0.3;
        private const double ANIMATION_TIME_EXIT = 0.08;

        private static FlyoutAnimationScheme CalculateFlyoutAnimationScheme<T>(T window, bool topmost = false) where T : Window, IFlyout
        {
            DependencyProperty property;
            double from, to;

            var taskbar = WindowsTaskbar.Current;

            if (taskbar.IsHorizontal)
            {
                property = Window.TopProperty;
                window.Left = taskbar.Right - window.Width + window.BorderThickness.Right;
            }
            else
            {
                property = Window.LeftProperty;
                window.Top = taskbar.Bottom - window.Height + window.BorderThickness.Bottom;
            }

            switch (taskbar.Side)
            {
                case WindowsTaskbar.Position.Bottom:
                    from = taskbar.Bottom;
                    to = taskbar.Bottom - window.Height + window.BorderThickness.Bottom;
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

            if (IS_WINDOWS11)
            {
                to -= 12;
                window.Left = taskbar.Right - window.Width + window.BorderThickness.Right - 12;
            }

            return new FlyoutAnimationScheme
            {
                Property = property,
                From = from,
                To = to
            };
        }

        public static void ShowFlyout<T>(T window, bool topmost = false) where T : Window, IFlyout
        {
            var scheme = CalculateFlyoutAnimationScheme(window);

            window.IsRaising = true;
            BringTaskbarToFront();
            if (scheme.Property == Window.TopProperty) window.Top = 999999; else window.Left = 999999;
            window.Visibility = Visibility.Visible;
            System.Threading.Thread.Sleep(1);
            window.Activate();
            BringTaskbarToFront();
            InteropHelper.CloakWindow(window, false);

            if (!SystemParameters.MenuAnimation)
            {
                if(scheme.Property == Window.TopProperty)
                {
                    window.Top = scheme.To;
                }
                else
                {
                    window.Left = scheme.To;
                }
                return;
            }

            var easingMode = scheme.To < scheme.From ? EasingMode.EaseOut : EasingMode.EaseIn;
            easingMode = EasingMode.EaseOut;

            var entraceAnimation = new DoubleAnimation
            {
                Duration = TimeSpan.FromSeconds(ANIMATION_TIME_ENTRACE),
                From = scheme.From,
                To = scheme.To,
                EasingFunction = new CubicEase
                {
                    EasingMode = easingMode
                }
            };
            Storyboard.SetTarget(entraceAnimation, window);
            Storyboard.SetTargetProperty(entraceAnimation, new PropertyPath(scheme.Property));

            var fadeAnimation = new DoubleAnimation
            {
                Duration = TimeSpan.FromSeconds(ANIMATION_TIME_ENTRACE),
                From = 0.8,
                To = 1,
                EasingFunction = new CubicEase
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
                //BringTaskbarToFront();
                window.Focus();
                window.Activate();
                window.IsRaising = false;
            };

            sb.Begin(window);
        }

        public static void HideFlyout<T>(T window) where T : Window, IFlyout
        {
            if (!SystemParameters.MenuAnimation || !IS_WINDOWS11)
            {
                HideFlyoutInternal(window);
                return;
            }

            var scheme = CalculateFlyoutAnimationScheme(window);

            InteropHelper.CloakWindow(window, false);

            var easingMode = scheme.To < scheme.From ? EasingMode.EaseOut : EasingMode.EaseIn;
            easingMode = EasingMode.EaseIn;

            var fadeAnimation = new DoubleAnimation
            {
                Duration = TimeSpan.FromSeconds(ANIMATION_TIME_EXIT),
                From = 1,
                To = 0.8,
                EasingFunction = new CubicEase
                {
                    EasingMode = easingMode
                }
            };
            Storyboard.SetTarget(fadeAnimation, window);
            Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath(Window.OpacityProperty));

            var exitAnimation = new DoubleAnimation
            {
                Duration = TimeSpan.FromSeconds(ANIMATION_TIME_EXIT),
                From = scheme.To,
                To = scheme.From,
                EasingFunction = new CubicEase
                {
                    EasingMode = easingMode
                }
            };
            Storyboard.SetTarget(exitAnimation, window);
            Storyboard.SetTargetProperty(exitAnimation, new PropertyPath(scheme.Property));

            var sb = new Storyboard();
            sb.FillBehavior = FillBehavior.Stop;
            sb.Children.Add(fadeAnimation);
            sb.Children.Add(exitAnimation);
            sb.Completed += (object sender, EventArgs e) =>
            {
                HideFlyoutInternal(window);
            };

            BringTaskbarToFront();
            sb.Begin(window);
        }

        private static void HideFlyoutInternal<T>(T window) where T : Window, IFlyout
        {
            window.Left = 999999;
            window.Tray.isClosing = true;
            System.Threading.Thread.Sleep(System.Windows.Forms.SystemInformation.DoubleClickTime / 2);
            window.Visibility = Visibility.Hidden;
            window.Tray.isClosing = false;
        }

        public static void BringTaskbarToFront()
        {
            NativeMethods.SetForegroundWindow(NativeMethods.FindWindow("Shell_TrayWnd", null));
        }

        public static bool IS_WINDOWS11 { get => _isWindows11; }
        private static bool _isWindows11 = IsWindows11();
        private static bool IsWindows11()
        {
            var reg = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            var currentBuild = int.Parse((string)reg.GetValue("CurrentBuild"));
            return currentBuild >= 22000;
        }
    }

    interface IFlyout
    {
        TrayManager Tray { get; }
        bool IsRaising { get; set; }
    }

    struct FlyoutAnimationScheme
    {
        public DependencyProperty Property;
        public double From;
        public double To;
    }
}
