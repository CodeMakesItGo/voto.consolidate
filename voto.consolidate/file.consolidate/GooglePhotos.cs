using CasCap.Models;
using CasCap.Services;
using file.consolidate;
using File.Consolidate;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace google.consolidate
{
    public class GooglePhotos : FileBase
    {
        public class PhotoAlbum
        {
            public string Title { get; set; }
            public string Id { get; set; }
            public string Url { get; set; }
            public bool IsSelected { get; set; }
            public int MediaCount { get; set; }
        }

        public class MediaInfo
        {
            public string Url { get; set; }
            public string Id { get; set; }
            public string Width { get; set; }
            public string Height { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public DateTime TimeStamp { get; set; }
        }

        public List<PhotoAlbum> PhotoAlbums = new();

        //https://console.cloud.google.com/apis/dashboard

        static readonly string _user = "";
        static readonly string _clientId = "";
        static readonly string _clientSecret = "";

        static GooglePhotosService _googlePhotosSvc = null;

        static async Task Login()
        {
            ProgressReport.Instance.Report = "Logging into Google: " + _user;

            if (new[] { _user, _clientId, _clientSecret }.Any(p => string.IsNullOrWhiteSpace(p)))
            {
                Console.WriteLine("Please populate authentication details to continue...");
                Debugger.Break();
                return;
            }

            //1) new-up some basic logging (if using appsettings.json you could load logging configuration from there)
            //var configuration = new ConfigurationBuilder().Build();
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                //builder.AddConfiguration(configuration.GetSection("Logging")).AddDebug().AddConsole();
            });
            var logger = loggerFactory.CreateLogger<GooglePhotosService>();

            //2) create a configuration object
            var options = new GooglePhotosOptions
            {
                User = _user,
                ClientId = _clientId,
                ClientSecret = _clientSecret,
                Scopes = new[] { GooglePhotosScope.ReadOnly },
            };

            //3) (Optional) display local OAuth 2.0 JSON file(s);
            var path = options.FileDataStoreFullPathOverride is null ? options.FileDataStoreFullPathDefault : options.FileDataStoreFullPathOverride;
            Console.WriteLine($"{nameof(options.FileDataStoreFullPathOverride)}:\t{path}");
            if (Directory.Exists(path))
            {
                var files = Directory.GetFiles(path);
                if (files.Length == 0)
                    Console.WriteLine($"\t- n/a this is probably the first time we have authenticated...");
                else
                {
                    Console.WriteLine($"Files;");
                    foreach (var file in files)
                        Console.WriteLine($"\t- {Path.GetFileName(file)}");
                }
            }
            //4) create a single HttpClient, this will be efficiently re-used by GooglePhotosService
            var handler = new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate };
            var client = new HttpClient(handler) { BaseAddress = new Uri(options.BaseAddress) };

            //5) new-up the GooglePhotosService passing in the previous references (in lieu of dependency injection)
            _googlePhotosSvc = new GooglePhotosService(logger, Options.Create(options), client);

            //6) log-in
            if (!await _googlePhotosSvc.LoginAsync()) throw new Exception($"login failed!");

            ProgressReport.Instance.Report = "Logged in as " + _user;
        }

        public static async Task<List<MediaInfo>> GetAllGooglePhotos()
        {
            if (_googlePhotosSvc == null) await Login();

            List<MediaInfo> media = new();

            var photos = await _googlePhotosSvc.GetMediaItemsAsync();

            foreach (var mediaItem in photos)
            {
                MediaInfo pi = new()
                {
                    Description = mediaItem.description,
                    Height = mediaItem.mediaMetadata.height,
                    Id = mediaItem.id,
                    TimeStamp = mediaItem.mediaMetadata.creationTime,
                    Title = mediaItem.filename,
                    Width = mediaItem.mediaMetadata.width,
                    Url = mediaItem.productUrl
                };
                media.Add(pi);
            }

            return media;
        }

        public static async Task<List<PhotoAlbum>> GetPhotoAlbums()
        {
            if (_googlePhotosSvc == null) await Login();

            var albums = await _googlePhotosSvc.GetAlbumsAsync();
            List<PhotoAlbum> rtn = new();

            foreach (var album in albums)
            {
                rtn.Add(new PhotoAlbum() { Url = album.coverPhotoBaseUrl, Title = album.title, Id = album.id, MediaCount = album.mediaItemsCount });
            }

            return rtn;
        }

        public static async Task<List<MediaInfo>> GetGooglePhotoIds(string albumId)
        {
            if (_googlePhotosSvc == null) await Login();

            List<MediaInfo> media = new();

            var album = await _googlePhotosSvc.GetMediaItemsByAlbumAsync(albumId);

            
            foreach (var mediaItem in album)
            {
                MediaInfo pi = new()
                {
                    Description = mediaItem.description,
                    Height = mediaItem.mediaMetadata.height,
                    Id = mediaItem.id,
                    TimeStamp = mediaItem.mediaMetadata.creationTime,
                    Title = mediaItem.filename,
                    Width = mediaItem.mediaMetadata.width,
                    Url = mediaItem.productUrl
                };

                media.Add(pi);
            }

            return media;
        }    

        public static async Task<MediaInfo> GetGooglePhotoInfo(string photoId)
        {
            if (_googlePhotosSvc == null) await Login();

            var mediaItem = await _googlePhotosSvc.GetMediaItemByIdAsync(photoId);

            if (mediaItem != null)
            {
                MediaInfo pi = new()
                {
                    Description = mediaItem.description,
                    Height = mediaItem.mediaMetadata.height,
                    Id = mediaItem.id,
                    TimeStamp = mediaItem.mediaMetadata.creationTime,
                    Title = mediaItem.filename,
                    Width = mediaItem.mediaMetadata.width,
                    Url = mediaItem.productUrl
                };

                return pi;
            }

            return null;
        }

        public static async Task<byte[]> GetGooglePhoto(string photoId)
        {
            if (_googlePhotosSvc == null) await Login();

            var mediaItem = await _googlePhotosSvc.GetMediaItemByIdAsync(photoId);

            if (mediaItem != null)
            {
                var bytes = await _googlePhotosSvc.DownloadBytes(mediaItem, null, null, false, true);

                return bytes;
            }

            return null;
        }



    } 
}
