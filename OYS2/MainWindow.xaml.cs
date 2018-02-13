using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Text.RegularExpressions;

namespace OYS2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            CurrentFrame = Content_01;
            Frames = new List<Grid>()
            {
                Content_01,
                Content_02
            };
        }

        private bool Locked = false;
        private Grid CurrentFrame = null;
        private int FrameIndex = 0;
        private List<Grid> Frames = null;

        private void MoveFrameRight(object sender, MouseButtonEventArgs e)
        {
            if (Locked) return;
            Lock();
            //if (CurrentFrame == null) return; //no frame at the moment, return.
            if (FrameIndex < 0) return; //frames disabled!
            if (Frames.Count < 2) return; //not enough frames to enable changing!
            if (FrameIndex == Frames.Count - 1) return; //at the last frame, can't move right!
            if (FrameIndex == Frames.Count - 2) //at the second last frame, fade the right arrow out!
            {
                FadeFrame(GridMoveRight, 0);
            }
            if (FrameIndex == 0) //at the first frame, fade the left arrow in!
            {
                FadeFrame(GridMoveLeft, 1);
            }
            ChangeFrame(Frames[FrameIndex+1]);
            FrameIndex++;
            new Thread(() => Unlock()).Start();
        }

        private void MoveFrameLeft(object sender, MouseButtonEventArgs e)
        {
            if (Locked) return;
            Lock();
            //if (CurrentFrame == null) return; //no frame at the moment, return.
            if (FrameIndex < 0) return; //frames disabled!
            if (Frames.Count < 2) return; //not enough frames to enable changing!
            if (FrameIndex == 0) return; //at the first frame, can't move left!
            if (FrameIndex == 1) //at the second frame, fade the left arrow out!
            {
                FadeFrame(GridMoveLeft, 0);
            }
            if (FrameIndex == Frames.Count - 1) //at the last frame, fade the left arrow in!
            {
                FadeFrame(GridMoveRight, 1);
            }
            ChangeFrame(Frames[FrameIndex - 1]);
            FrameIndex--;
            new Thread(() => Unlock()).Start();
        }

        private void GridDisable(Grid target)
        {
            target.IsEnabled = false;
            target.Visibility = System.Windows.Visibility.Hidden;
        }

        private void GridEnable(Grid target)
        {
            target.IsEnabled = true;
            target.Visibility = System.Windows.Visibility.Visible;
        }

        private void Lock()
        {
            Locked = true;
        }

        private void Unlock()
        {
            Thread.Sleep(500);
            Locked = false;
        }

        private void ChangeFrame(Grid NewFrame)
        {
            foreach (Grid ThisGrid in FindVisualChildren<Grid>(OpenYS_Window))
            {
                if (ThisGrid == NewFrame)
                {
                    FadeFrame(ThisGrid, 1);
                    CurrentFrame = ThisGrid;
                    continue;
                }
                if (ThisGrid.Name != null)
                {
                    if (ThisGrid.Name.ToUpperInvariant().StartsWith("CONTENT")) FadeFrame(ThisGrid, 0);
                }
            }
        }

        private DoubleAnimation FadeFrame(Grid ThisFrame, double newvalue)
        {
            Storyboard storyboard = new Storyboard();
            TimeSpan duration = new TimeSpan(0, 0, 0, 0, (int)500);

            DoubleAnimation animation = new DoubleAnimation { From = ThisFrame.Opacity, To = newvalue, Duration = new Duration(duration) };
            if (newvalue <= 0) animation.Completed += (sender_new, eArgs) => GridDisable(ThisFrame);
            else GridEnable(ThisFrame);

            Storyboard.SetTargetName(animation, ThisFrame.Name);
            Storyboard.SetTargetProperty(animation, new PropertyPath("Opacity", 0));
            storyboard.Children.Add(animation);

            storyboard.Begin(ThisFrame);

            return animation; //We need to check if it has completed!
        }

        private DoubleAnimation FadeTextBox(TextBox ThisTextBox, double newvalue)
        {
            if (ThisTextBox == null) return new DoubleAnimation();
            Storyboard storyboard = new Storyboard();
            TimeSpan duration = new TimeSpan(0, 0, 0, 0, (int)500);

            DoubleAnimation animation = new DoubleAnimation { From = ThisTextBox.Opacity, To = newvalue, Duration = new Duration(duration) };

            Storyboard.SetTargetName(animation, ThisTextBox.Name);
            Storyboard.SetTargetProperty(animation, new PropertyPath("Opacity", 0));
            storyboard.Children.Add(animation);

            storyboard.Begin(ThisTextBox);

            return animation; //We need to check if it has completed!
        }

        public IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
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

        private void Directory_Browse_Folder(object sender, MouseButtonEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.SelectedPath = Directory_Text.Text;
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (dialog.SelectedPath != "") Directory_Text.Text = dialog.SelectedPath;
        }

        private void Directory_Text_Textchanged(object sender, TextChangedEventArgs e)
        {
            bool failed = false;
            foreach (char thischar in Directory_Text.Text)
                {
                    if (System.IO.Path.GetInvalidPathChars().Contains(thischar)) failed = true;
                }
            if (!failed)
            {
                Directory_Text.BorderBrush = (Brush)new BrushConverter().ConvertFrom("#00FF00");
                FadeTextBox(Install_Type_Directory_Warning, 0.0);
            }
            else
            {
                Directory_Text.BorderBrush = (Brush)new BrushConverter().ConvertFrom("#FF0000");
                FadeTextBox(Install_Type_Directory_Warning, 1.0);
            }
        }

        private void Installation_Type_Full_Copy_Checked(object sender, RoutedEventArgs e)
        {
            FadeTextBox(Install_Type_Label_Warning, 1.0);
        }

        private void Installation_Type_Full_Copy_Unchecked(object sender, RoutedEventArgs e)
        {
            FadeTextBox(Install_Type_Label_Warning, 0.0);
        }

        private void Install_OpenYS(object sender, MouseButtonEventArgs e)
        {
            ChangeFrame(Content_03);
            FadeFrame(GridMoveLeft, 0);
            FadeFrame(GridMoveRight, 0);
            FrameIndex = -1;
            new Thread(() => _Install()).Start();
        }

        private void _Install()
        {
            //Cause a delay so the UI douesn't mess up!
            Thread.Sleep(1000);

            //Dispatcher so we don't get cock-blocked by the WPF thread security.
            Dispatcher.Invoke
            (
                System.Windows.Threading.DispatcherPriority.Normal,
                new Action
                (
                    delegate()
                    {
                        //Begin
                        //Create directory...
                        string _Directory = Directory_Text.Text;
                        try
                        {
                            if (!Directory.Exists(_Directory))
                            {
                                //We need to create the directory!
                                Directory.CreateDirectory(_Directory);

                                //Successfully created the directory!
                            }
                            else
                            {
                                //Directory already exists!
                            }
                        }
                        catch
                        {
                            Content_05_Error_Message.Text = "Output path is not a valid directory name!";
                            ChangeFrame(Content_05);
                            return;
                        }


                        //End
                        ChangeFrame(Content_04);
                    }
                )
            );
        }

        private void Directory_Text_Restore_Border(object sender, RoutedEventArgs e)
        {
            Directory_Text.BorderThickness = new Thickness(1, 1, 1, 1);
            Directory_Text.Margin = new Thickness(
                Directory_Text.Margin.Left-1,
                Directory_Text.Margin.Top-1,
                Directory_Text.Margin.Right+2,
                Directory_Text.Margin.Bottom+2);
        }

        private void Directory_Text_Remove_Border(object sender, RoutedEventArgs e)
        {
            Directory_Text.BorderThickness = new Thickness(0, 0, 0, 0);
            Directory_Text.Margin = new Thickness(
                Directory_Text.Margin.Left + 1,
                Directory_Text.Margin.Top + 1,
                Directory_Text.Margin.Right - 2,
                Directory_Text.Margin.Bottom - 2);
        }
    }
}
