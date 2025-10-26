using System;
using System.Windows;
using System.Windows.Media.Animation;
using MediaFlyout.Interop;
using MediaFlyout.Extensions;
using MediaFlyout.Flyout;
using System.Threading.Tasks;

namespace MediaFlyout.Helpers
{
    sealed class AnimationHelper
    {
        private static double ANIMATION_TIME_ENTRACE = Environment.OSVersion.IsAtLeast(OSVersions.VER_11_21H2) ? 0.25 : 0.5;
        private const double ANIMATION_TIME_EXIT = 0.1;

        struct FlyoutAnimationScheme
        {
            public DependencyProperty Property;
            public double From;
            public double To;
        }

        private static FlyoutAnimationScheme CalculateFlyoutAnimationScheme(FlyoutWindow window, bool topmost = false)
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

            if (Environment.OSVersion.IsWindows11())
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

        public static void ShowFlyout(FlyoutWindow window, bool topmost = false)
        {
            var scheme = CalculateFlyoutAnimationScheme(window);

            var onCompleted = new EventHandler((s, e) =>
            {
                window.Cloak(false);
                window.Topmost = topmost;
                //BringTaskbarToFront();
                window.IsRaising = false;
                window.Focus();
                window.Activate();

#if !DEBUG
                Task.Run(async delegate
                {
                    await Task.Delay(1500);
                    if (!window.IsFocused && window.IsVisible && !window.Tray.isClosing)
                    {
                        HideFlyout(window);
                    }
                });
#endif
            });

            window.IsRaising = true;
            BringTaskbarToFront();
            if (scheme.Property == Window.TopProperty)
            {
                window.Top = 999999;
            }
            else
            {
                window.Left = 999999;
            }
            window.Visibility = Visibility.Visible;
            System.Threading.Thread.Sleep(1);
            window.Activate();
            BringTaskbarToFront();
            window.Cloak(false);

            if (!SystemParameters.MenuAnimation)
            {
                if (scheme.Property == Window.TopProperty)
                {
                    window.Top = scheme.To;
                }
                else
                {
                    window.Left = scheme.To;
                }
                onCompleted(null, null);
                return;
            }

            var easingMode = scheme.To < scheme.From ? EasingMode.EaseOut : EasingMode.EaseIn;
            easingMode = EasingMode.EaseOut;

            var entraceAnimation = new DoubleAnimation
            {
                Duration = TimeSpan.FromSeconds(ANIMATION_TIME_ENTRACE),
                From = scheme.From,
                To = scheme.To,
                EasingFunction = new QuinticEase
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
                EasingFunction = new QuinticEase
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
            sb.Completed += onCompleted;

            window.Cloak(false);

            sb.Begin(window);
        }

        public static void HideFlyout(FlyoutWindow window)
        {
            window.Tray.IsClosing = true;
            window.Topmost = false;
            var onCompleted = new EventHandler(async (s, e) =>
            {
                window.Cloak();
                window.Left = 999999;
                await Task.Delay(System.Windows.Forms.SystemInformation.DoubleClickTime);
                window.Visibility = Visibility.Hidden;
                window.Tray.IsClosing = false;
            });

            if (!SystemParameters.MenuAnimation || Environment.OSVersion.IsLessThan(OSVersions.VER_11_21H2))
            {
                onCompleted(null, null);
                return;
            }

            var scheme = CalculateFlyoutAnimationScheme(window);

            window.Cloak(false);

            var easingMode = scheme.To < scheme.From ? EasingMode.EaseOut : EasingMode.EaseIn;
            easingMode = EasingMode.EaseIn;

            var fadeAnimation = new DoubleAnimation
            {
                Duration = TimeSpan.FromSeconds(ANIMATION_TIME_EXIT),
                From = 1,
                To = 0.8,
                EasingFunction = new QuinticEase
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
                EasingFunction = new QuinticEase
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
            sb.Completed += onCompleted;

            BringTaskbarToFront();
            sb.Begin(window);
        }

        public static void BringTaskbarToFront()
        {
            User32.SetForegroundWindow(WindowsTaskbar.Handle);
        }
    }
}
