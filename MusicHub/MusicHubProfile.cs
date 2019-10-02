namespace MusicHub
{
    using AutoMapper;
    using MusicHub.Data.Models;
    using MusicHub.DataProcessor.ExportDtos;
    using MusicHub.DataProcessor.ImportDtos;

    public class MusicHubProfile : Profile
    {
        // Configure your AutoMapper here if you wish to use it. If not, DO NOT DELETE THIS CLASS
        public MusicHubProfile()
        {
            this.CreateMap<JsonImportWriter, Writer>();

            this.CreateMap<JsonImportAlbum, Album>();

            this.CreateMap<JsonImportProducer, Producer>();

            this.CreateMap<XmlImportSong, Song>();

            this.CreateMap<XmlImportPerformer, Performer>();

            this.CreateMap<Song, JsonExportSong>();

            this.CreateMap<Album, JsonExportAlbum>();

            this.CreateMap<Song, XmlExportSong>();
        }
    }
}
