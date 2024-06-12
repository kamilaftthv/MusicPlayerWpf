using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using TagLib;

namespace MusicPlayerWpf
{
    public partial class MainWindow : Window
    {
        private MediaPlayer _player;
        private bool _isPlaying = false;
        public List<TrackInfo> FilesInFolders { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            _player = new MediaPlayer();
            FilesInFolders = new List<TrackInfo>();
            DataContext = this;
        }

        private void OpenFileMI_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "MP3 files (*.mp3)|*.mp3";
            if (openFileDialog.ShowDialog() == true)
            {
                var filePath = openFileDialog.FileName;
                var track = CreateTrackInfo(filePath);
                FilesInFolders.Add(track);
                FilesDG.Items.Refresh();
            }
        }

        private void OpenFolderMI_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var files = Directory.GetFiles(dialog.SelectedPath, "*.mp3");
                    foreach (var file in files)
                    {
                        var track = CreateTrackInfo(file);
                        FilesInFolders.Add(track);
                    }
                    FilesDG.Items.Refresh();
                }
            }
        }

        private TrackInfo CreateTrackInfo(string filePath)
        {
            var file = TagLib.File.Create(filePath);
            return new TrackInfo
            {
                FilePath = filePath,
                Name = file.Tag.Title,
                Artist = file.Tag.FirstPerformer,
                Duration = file.Properties.Duration.ToString(@"mm\:ss")
            };
        }

        private void PlayFile(string filePath)
        {
            _player.Open(new Uri(filePath));
            _player.Play();
            _isPlaying = true;
            PlayPauseIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause;

            var file = TagLib.File.Create(filePath);
            FileNameLb.Content = $"Playing: {file.Tag.Title} by {file.Tag.FirstPerformer}";
            TrackInfo.Content = $"{file.Tag.FirstPerformer} - {file.Tag.Title}";
        }

        private void PlayPause_Click(object sender, RoutedEventArgs e)
        {
            if (_isPlaying)
            {
                _player.Pause();
                _isPlaying = false;
                PlayPauseIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Play;
            }
            else
            {
                _player.Play();
                _isPlaying = true;
                PlayPauseIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause;
            }
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            _player.Stop();
            _isPlaying = false;
            PlayPauseIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Play;
        }

        private void Previous_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
                WindowState = WindowState.Normal;
            else
                WindowState = WindowState.Maximized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CloseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        private void CloseCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void FilesDG_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedTrack = FilesDG.SelectedItem as TrackInfo;
            if (selectedTrack != null)
            {
                PlayFile(selectedTrack.FilePath);
            }
        }
    }

    public class TrackInfo
    {
        public string FilePath { get; set; }
        public string Name { get; set; }
        public string Artist { get; set; }
        public string Duration { get; set; }
    }
}
