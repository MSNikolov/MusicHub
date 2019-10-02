namespace MusicHub.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using AutoMapper;
    using Data;
    using MusicHub.Data.Models;
    using MusicHub.DataProcessor.ImportDtos;
    using Newtonsoft.Json;
    using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data";

        private const string SuccessfullyImportedWriter 
            = "Imported {0}";
        private const string SuccessfullyImportedProducerWithPhone 
            = "Imported {0} with phone: {1} produces {2} albums";
        private const string SuccessfullyImportedProducerWithNoPhone
            = "Imported {0} with no phone number produces {1} albums";
        private const string SuccessfullyImportedSong 
            = "Imported {0} ({1} genre) with duration {2}";
        private const string SuccessfullyImportedPerformer
            = "Imported {0} ({1} songs)";

        public static string ImportWriters(MusicHubDbContext context, string jsonString)
        {
            var writersDto = JsonConvert.DeserializeObject<List<JsonImportWriter>>(jsonString);

            var writers = new List<Writer>();

            var result = new StringBuilder();

            foreach (var writerDto in writersDto)
            {
                if (!IsValid(writerDto))
                {
                    result.AppendLine(ErrorMessage);

                    continue;
                }

                var writer = Mapper.Map<Writer>(writerDto);

                writers.Add(writer);

                result.AppendLine(string.Format(SuccessfullyImportedWriter, writer.Name));
            }

            context.Writers.AddRange(writers);

            context.SaveChanges();

            return result.ToString().Trim();
        }

        public static string ImportProducersAlbums(MusicHubDbContext context, string jsonString)
        {
            var producersDto = JsonConvert.DeserializeObject<List<JsonImportProducer>>(jsonString);

            var producers = new List<Producer>();

            var result = new StringBuilder();

            foreach (var producerDto in producersDto)
            {
                if (!IsValid(producerDto))
                {
                    result.AppendLine(ErrorMessage);

                    continue;
                }

                var invalidAlbum = false;

                foreach (var albumDto in producerDto.Albums)
                {
                    if (!IsValid(albumDto))
                    {
                        invalidAlbum = true;
                    }
                }

                if (invalidAlbum)
                {
                    result.AppendLine(ErrorMessage);

                    continue;
                }

                var producer = Mapper.Map<Producer>(producerDto);

                foreach (var album in producer.Albums)
                {
                    album.ReleaseDate = DateTime.ParseExact(producerDto.Albums.FirstOrDefault(a => a.Name == album.Name).DateOfRelease, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                }

                if (producer.PhoneNumber != null)
                {
                    result.AppendLine(string.Format(SuccessfullyImportedProducerWithPhone, producer.Name, producer.PhoneNumber, producer.Albums.Count));
                }

                else
                {
                    result.AppendLine(string.Format(SuccessfullyImportedProducerWithNoPhone, producer.Name, producer.Albums.Count));
                }

                producers.Add(producer);
            }

            context.Producers.AddRange(producers);

            context.SaveChanges();

            return result.ToString().Trim();
        }

        public static string ImportSongs(MusicHubDbContext context, string xmlString)
        {
            var ser = new XmlSerializer(typeof(List<XmlImportSong>), new XmlRootAttribute("Songs"));

            var songsTdo = (List<XmlImportSong>)ser.Deserialize(new StringReader(xmlString));

            var songs = new List<Song>();

            var result = new StringBuilder();

            foreach (var songDto in songsTdo)
            {
                var validGenre = Enum.TryParse(typeof(Genre), (songDto.Genre), out object genre);

                var validAlbum = context.Albums.Any(a => a.Id == songDto.AlbumId) || songDto.AlbumId == null;

                var validWriter = context.Writers.Any(w => w.Id == songDto.WriterId);

                if (!IsValid(songDto) || !validGenre || !validAlbum || !validWriter)
                {
                    result.AppendLine(ErrorMessage);

                    continue;
                }

                var song = Mapper.Map<Song>(songDto);

                song.Duration = TimeSpan.ParseExact(songDto.Time, "c", CultureInfo.InvariantCulture);

                song.CreatedOn = DateTime.ParseExact(songDto.DateOfCreation, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                songs.Add(song);

                result.AppendLine(string.Format(SuccessfullyImportedSong, song.Name, song.Genre, song.Duration));
            }

            context.Songs.AddRange(songs);

            context.SaveChanges();

            return result.ToString().Trim();
        }

        public static string ImportSongPerformers(MusicHubDbContext context, string xmlString)
        {
            var ser = new XmlSerializer(typeof(List<XmlImportPerformer>), new XmlRootAttribute("Performers"));

            var res = new StringBuilder();

            var performers = new List<Performer>();

            var validPerfsDto = new List<XmlImportPerformer>();

            var performersDto = (List<XmlImportPerformer>)ser.Deserialize(new StringReader(xmlString));

            foreach (var perfDto in performersDto)
            {
                var invalidId = false;

                foreach (var songId in perfDto.SongIds)
                {
                    if (!context.Songs.Any(s => s.Id == songId.Id))
                    {
                        invalidId = true;
                    }
                }

                if (invalidId || !IsValid(perfDto))
                {
                    res.AppendLine(ErrorMessage);

                    continue;
                }

                var performer = Mapper.Map<Performer>(perfDto);

                performers.Add(performer);

                validPerfsDto.Add(perfDto);

                res.AppendLine(string.Format(SuccessfullyImportedPerformer, performer.FirstName, perfDto.SongIds.Count));
            }

            context.Performers.AddRange(performers);

            var songsPerfs = new List<SongPerformer>();

            for (int i = 0; i < performers.Count; i++)
            {
                var perfId = performers[i].Id;

                var perfDto = validPerfsDto[i];

                foreach (var songId in perfDto.SongIds)
                {
                    var song = new SongPerformer
                    {
                        SongId = songId.Id,
                        PerformerId = perfId
                    };

                    songsPerfs.Add(song);
                }
            }

            context.SongsPerformers.AddRange(songsPerfs);

            context.SaveChanges();

            return res.ToString().Trim();
        }

        private static bool IsValid(object dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResult = new List<ValidationResult>();

            return Validator.TryValidateObject(dto, validationContext, validationResult, true);
        }
    }
}