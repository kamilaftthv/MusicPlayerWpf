﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using TagLib;

namespace MusicPlayerWpf
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public static RoutedCommand PreviousTrackCommand = new RoutedCommand();
        public static RoutedCommand StopTrackCommand = new RoutedCommand();
        public static RoutedCommand NextTrackCommand = new RoutedCommand();
        public static RoutedCommand PlayPauseTrackCommand = new RoutedCommand();

        private MediaPlayer _player;
        private bool _isPlaying = false;
        private double _volume = 50;
        private int _currentTrackIndex = -1;
        private Cursor _normalCursor;
        private Cursor _clickedCursor;

        public List<TrackInfo> FilesInFolders { get; set; }

        private Random _random = new Random();
        private bool _isShuffle = false;

        public double Volume
        {
            get { return _volume; }
            set
            {
                _volume = value;
                OnPropertyChanged(nameof(Volume));
                if (_player != null)
                    _player.Volume = Volume / 100.0;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            _player = new MediaPlayer();
            FilesInFolders = new List<TrackInfo>();
            DataContext = this;
            LoadCustomCursors();
            this.Cursor = _normalCursor;

            CommandBindings.Add(new CommandBinding(PreviousTrackCommand, Previous_Click));
            CommandBindings.Add(new CommandBinding(StopTrackCommand, Stop_Click));
            CommandBindings.Add(new CommandBinding(NextTrackCommand, Next_Click));
            CommandBindings.Add(new CommandBinding(PlayPauseTrackCommand, PlayPause_Click));
        }

        private void LoadCustomCursors()
        {
            _normalCursor = LoadCursor("Resources/NormalCursor.cur");
            _clickedCursor = LoadCursor("Resources/ClickedCursor.cur");
        }

        private Cursor LoadCursor(string filePath)
        {
            try
            {
                var cursorStream = Application.GetResourceStream(new Uri(filePath, UriKind.Relative)).Stream;
                return new Cursor(cursorStream);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading cursor: {ex.Message}");
                return Cursors.Arrow;
            }
        }

        private void ChangeCursor(Cursor cursor)
        {
            this.Cursor = cursor;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OpenFileMI_Click(object sender, RoutedEventArgs e)
        {
            StopCurrentTrack();
            var openFileDialog = new OpenFileDialog();
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
            StopCurrentTrack();
            ClearTrackList();
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
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

        private void ClearTrackList()
        {
            FilesInFolders.Clear();
            FilesDG.Items.Refresh();
        }

        private void StopCurrentTrack()
        {
            _player.Stop();
            _isPlaying = false;
            PlayPauseIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Play;

            FileNameLb.Content = "No song selected";
            TrackTitle.Content = "Title";
            TrackArtist.Content = "Artist";
            AlbumCoverImage.Source = null;
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
            TrackTitle.Content = file.Tag.Title;
            TrackArtist.Content = file.Tag.FirstPerformer;

            ExtractAlbumCover(filePath);

            _player.MediaOpened += Player_MediaOpened;
            _player.MediaEnded += Player_MediaEnded;

            var track = FilesInFolders.Find(t => t.FilePath == filePath);
            if (track != null)
            {
                AlbumCoverImage.Source = track.AlbumCover;
            }
        }

        private void ExtractAlbumCover(string filePath)
        {
            try
            {
                TagLib.File file = TagLib.File.Create(filePath);
                var tag = file.Tag;

                if (tag.Pictures.Length >= 1)
                {
                    var bin = tag.Pictures[0].Data.Data;
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = new MemoryStream(bin);
                    bitmap.EndInit();

                    foreach (var item in FilesInFolders)
                    {
                        if (item.FilePath == filePath)
                        {
                            item.AlbumCover = bitmap;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex) { }
        }

        private void Player_MediaOpened(object sender, EventArgs e)
        {
            TrackSlider.Maximum = _player.NaturalDuration.TimeSpan.TotalSeconds;
            CompositionTarget.Rendering += UpdateSliderPosition;

            if (_currentTrackIndex >= 0 && _currentTrackIndex < FilesInFolders.Count)
            {
                var filePath = FilesInFolders[_currentTrackIndex].FilePath;
                ExtractAlbumCover(filePath);
            }
        }

        private void Player_MediaEnded(object sender, EventArgs e)
        {
            _isPlaying = false;
            PlayPauseIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Play;
            TrackSlider.Value = 0;
            CompositionTarget.Rendering -= UpdateSliderPosition;
            PlayNextTrack();
        }

        private void UpdateSliderPosition(object sender, EventArgs e)
        {
            TrackSlider.Value = _player.Position.TotalSeconds;
        }

        private void TrackSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _player.Position = TimeSpan.FromSeconds(TrackSlider.Value);
        }

        private void TrackSlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            _player.Pause();
        }

        private void TrackSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            _player.Play();
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _player.Volume = Volume / 100.0;
        }

        private void VolumeSlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
        }

        private void VolumeSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
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

            FileNameLb.Content = "No song selected";
            TrackTitle.Content = "Title";
            TrackArtist.Content = "Artist";
            AlbumCoverImage.Source = null;
        }

        private void Previous_Click(object sender, RoutedEventArgs e)
        {
            if (FilesInFolders.Count > 0)
            {
                PlayPreviousTrack();
            }
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            if (FilesInFolders.Count > 0)
            {
                PlayNextTrack();
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
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
                _currentTrackIndex = FilesInFolders.IndexOf(selectedTrack);
                PlayFile(selectedTrack.FilePath);
            }
        }

        private void DeleteTrack_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                DataGridRow row = DataGridRow.GetRowContainingElement(button);
                if (row != null)
                {
                    TrackInfo track = row.Item as TrackInfo;
                    if (track != null)
                    {
                        FilesInFolders.Remove(track);
                        FilesDG.Items.Refresh();
                        if (_currentTrackIndex >= 0 && _currentTrackIndex < FilesInFolders.Count && FilesInFolders[_currentTrackIndex] == track)
                        {
                            StopCurrentTrack();
                            _currentTrackIndex = -1;
                        }
                    }
                }
            }
        }

        public static class DataGridRowHelper
        {
            public static DataGridRow GetRowContainingElement(FrameworkElement element)
            {
                while (element != null && !(element is DataGridRow))
                {
                    element = VisualTreeHelper.GetParent(element) as FrameworkElement;
                }
                return element as DataGridRow;
            }
        }

        private void AnyElement_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ChangeCursor(_clickedCursor);
        }

        private void AnyElement_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ChangeCursor(_normalCursor);
        }
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            new PianoWindow().Show();
        }

        private void Shuffle_Click(object sender, RoutedEventArgs e)
        {
            _isShuffle = true;
            MessageBox.Show("Shuffle mode activated");
        }

        private void Order_Click(object sender, RoutedEventArgs e)
        {
            _isShuffle = false;
            MessageBox.Show("Play in order mode activated");
        }

        private void PlayNextTrack()
        {
            if (_isShuffle)
            {
                _currentTrackIndex = _random.Next(FilesInFolders.Count);
            }
            else
            {
                _currentTrackIndex++;
                if (_currentTrackIndex >= FilesInFolders.Count)
                {
                    _currentTrackIndex = 0;
                }
            }

            PlayFile(FilesInFolders[_currentTrackIndex].FilePath);
        }

        private void PlayPreviousTrack()
        {
            if (_isShuffle)
            {
                _currentTrackIndex = _random.Next(FilesInFolders.Count);
            }
            else
            {
                _currentTrackIndex--;
                if (_currentTrackIndex < 0)
                {
                    _currentTrackIndex = FilesInFolders.Count - 1;
                }
            }

            PlayFile(FilesInFolders[_currentTrackIndex].FilePath);
        }
    }
}
