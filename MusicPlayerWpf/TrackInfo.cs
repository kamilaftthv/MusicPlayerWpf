using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MusicPlayerWpf
{
    public class TrackInfo
    {
        public string FilePath { get; set; }
        public string Name { get; set; }
        public string Artist { get; set; }
        public string Duration { get; set; }
        public BitmapImage AlbumCover { get; set; }
        public TrackInfo()
        {
            FilePath = string.Empty;
            Name = string.Empty;
            Artist = string.Empty;
            Duration = string.Empty;
            AlbumCover = null;
        }
    }
}
