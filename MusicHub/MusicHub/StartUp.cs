namespace MusicHub
{
    using System;
    using System.Text;
    using Castle.Core.Internal;
    using Castle.DynamicProxy.Contributors;
    using Data;
    using Initializer;
    using Microsoft.EntityFrameworkCore.ValueGeneration;

    public class StartUp
    {
        public static void Main()
        {
            MusicHubDbContext context =
                new MusicHubDbContext();

            DbInitializer.ResetDatabase(context);

            //Test your solutions here
            Console.WriteLine(ExportSongsAboveDuration(context, 4));
        }

        public static string ExportAlbumsInfo(MusicHubDbContext context, int producerId)
        {
            var albums = context.Albums
                .Where(a => a.ProducerId == producerId)
                .Select(a => new
                {
                    a.Name,
                    ReleaseDate = $"{a.ReleaseDate:MM/dd/yyyy}",
                    ProducerName = a.Producer.Name,
                    Songs = a.Songs.Select(s => new
                    {
                        SongName = s.Name,
                        SongPrice = $"{s.Price:f2}",
                        SongWriterName = s.Writer.Name,
                    }).OrderByDescending(s => s.SongName).ThenBy(s => s.SongWriterName).ToList(),
                    Price = a.Price
                }).ToList().OrderByDescending(a => a.Price);
            StringBuilder sb = new StringBuilder();

            foreach (var a in albums)
            {
                sb.AppendLine($"-AlbumName: {a.Name}");
                sb.AppendLine($"-ReleaseDate: {a.ReleaseDate}");
                sb.AppendLine($"-ProducerName: {a.ProducerName}");
                sb.AppendLine($"-Songs:");
                int counter = 1;
                foreach (var song in a.Songs)
                {
                    sb.AppendLine($"---#{counter++}");
                    sb.AppendLine($"---SongName: {song.SongName}");
                    sb.AppendLine($"---Price: {song.SongPrice}");
                    sb.AppendLine($"---Writer: {song.SongWriterName}");
                }
                sb.AppendLine($"-AlbumPrice: {a.Price:f2}");
            }
            return sb.ToString().TrimEnd();
        }

        public static string ExportSongsAboveDuration(MusicHubDbContext context, int duration)
        {
            var neededSongs = context.Songs
                .AsEnumerable()
                .Where(s => s.Duration.TotalSeconds > duration)
                .Select(s => new
                {
                    s.Name,
                    PerformersFullNames = s.SongPerformers
                        .Select(sp => $"{sp.Performer.FirstName} {sp.Performer.LastName}").OrderBy(p => p).ToList(),
                    WriterName = s.Writer.Name,
                    AlbumProducer = s.Album.Producer.Name,
                    Duration = $"{s.Duration:c}"
                })
                .OrderBy(s => s.Name)
                .ThenBy(s => s.WriterName);

            StringBuilder sb = new StringBuilder();
            int counter = 1;
            foreach (var s in neededSongs)
            {
                sb.AppendLine($"-Song #{counter++}");
                sb.AppendLine($"---SongName: {s.Name}");
                sb.AppendLine($"---Writer: {s.WriterName}");
                if (s.PerformersFullNames.Any())
                {
                    foreach (var p in s.PerformersFullNames)
                    {
                        sb.AppendLine($"---Performer: {p}");
                    }
                }
                sb.AppendLine($"---AlbumProducer: {s.AlbumProducer}");
                sb.AppendLine($"---Duration: {s.Duration}");
            }
            return sb.ToString().TrimEnd();
        }
    }
}
