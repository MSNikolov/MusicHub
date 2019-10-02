using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MusicHub.DataProcessor.ExportDtos
{
    public class JsonExportAlbum
    {
        public string AlbumName { get; set; }

        public string ReleaseDate { get; set; }

        public string ProducerName { get; set; }

        public List<JsonExportSong> Songs { get; set; } = new List<JsonExportSong>();

        public string AlbumPrice { get; set; }
    }
}
