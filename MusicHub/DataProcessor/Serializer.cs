namespace MusicHub.DataProcessor
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using Data;
    using MusicHub.DataProcessor.ExportDtos;
    using System.Globalization;
    using Newtonsoft.Json;
    using System.Xml.Serialization;
    using System.Xml;
    using System.Text;
    using System.IO;

    public class Serializer
    {
        public static string ExportAlbumsInfo(MusicHubDbContext context, int producerId)
        {
            var albums = context.Albums
                .Where(a => a.ProducerId == producerId)
                .Select(a => new JsonExportAlbum
                {
                    AlbumName = a.Name,
                    ReleaseDate = a.ReleaseDate.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture),
                    ProducerName = a.Producer.Name,
                    Songs = a.Songs.Select(s => new JsonExportSong
                    {
                        SongName = s.Name,
                        Price = s.Price.ToString("F2"),
                        Writer = s.Writer.Name
                    })
                    .OrderByDescending(s => s.SongName)
                    .ThenBy(s => s.Writer)
                    .ToList(),
                    AlbumPrice = a.Songs.Sum(s => s.Price).ToString("F2")
                })
                .OrderByDescending(x => x.AlbumPrice)
                .ToList();

            return JsonConvert.SerializeObject(albums, Newtonsoft.Json.Formatting.Indented);
        }

        public static string ExportSongsAboveDuration(MusicHubDbContext context, int duration)
        {
            var songs = context.Songs
                .Where(s => s.Duration.TotalSeconds > duration)
                .Select(s => new XmlExportSong
                {
                    Name = s.Name,
                    Writer = s.Writer.Name,
                    Performer = s.SongPerformers.Select(p => p.Performer.FirstName + " " + p.Performer.LastName).FirstOrDefault(),
                    AlbumProducer = s.Album.Producer.Name,
                    Duration = s.Duration.ToString("c")
                })
                .OrderBy(s => s.Name)
                .ThenBy(s => s.Writer)
                .ToList();

            var ser = new XmlSerializer(typeof(List<XmlExportSong>), new XmlRootAttribute("Songs"));

            var namespaces = new XmlSerializerNamespaces(new[]
            {
                new XmlQualifiedName("","")
            });

            var sb = new StringBuilder();

            ser.Serialize(new StringWriter(sb), songs, namespaces);

            return sb.ToString().Trim();
        }
    }
}