using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace OYS2
{
    public static class Extensions
    {
        internal enum TransitionSpeed
        {
            Instant = 0,
            Fast = 100,
            Normal = 200,
            Slow = 500
        }

        /// <summary>
        /// Toggles the opacity of a control.
        /// </summary>
        /// <param name="control">The control.</param>
        internal static void ToggleControlFade(this Control control)
        {
            control.ToggleControlFade(TransitionSpeed.Normal);
        }

        /// <summary>
        /// Toggles the opacity of a control.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="speed">The speed.</param>
        internal static void ToggleControlFade(this Control control, TransitionSpeed speed)
        {
            Storyboard storyboard = new Storyboard();
            TimeSpan duration = new TimeSpan(0, 0, 0, 0, (int)speed); //

            DoubleAnimation animation = new DoubleAnimation { From = 1.0, To = 0.0, Duration = new Duration(duration) };
            if (control.Opacity == 0.0)
            {
                animation = new DoubleAnimation { From = 0.0, To = 1.0, Duration = new Duration(duration) };
            }

            Storyboard.SetTargetName(animation, control.Name);
            Storyboard.SetTargetProperty(animation, new PropertyPath("Opacity", 0));
            storyboard.Children.Add(animation);

            storyboard.Begin(control);
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
    }
}
