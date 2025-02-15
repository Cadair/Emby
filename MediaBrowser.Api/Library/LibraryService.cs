﻿using MediaBrowser.Api.Movies;
using MediaBrowser.Api.Music;
using MediaBrowser.Controller.Activity;
using MediaBrowser.Controller.Channels;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.LiveTv;
using MediaBrowser.Controller.Localization;
using MediaBrowser.Controller.Net;
using MediaBrowser.Controller.Persistence;
using MediaBrowser.Controller.TV;
using MediaBrowser.Model.Activity;
using MediaBrowser.Model.Channels;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Querying;
using ServiceStack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommonIO;
using MediaBrowser.Common.IO;

namespace MediaBrowser.Api.Library
{
    [Route("/Items/{Id}/File", "GET", Summary = "Gets the original file of an item")]
    [Authenticated]
    public class GetFile
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        [ApiMember(Name = "Id", Description = "Item Id", IsRequired = true, DataType = "string", ParameterType = "path", Verb = "GET")]
        public string Id { get; set; }
    }

    /// <summary>
    /// Class GetCriticReviews
    /// </summary>
    [Route("/Items/{Id}/CriticReviews", "GET", Summary = "Gets critic reviews for an item")]
    [Authenticated]
    public class GetCriticReviews : IReturn<QueryResult<ItemReview>>
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        [ApiMember(Name = "Id", Description = "Item Id", IsRequired = true, DataType = "string", ParameterType = "path", Verb = "GET")]
        public string Id { get; set; }

        /// <summary>
        /// Skips over a given number of items within the results. Use for paging.
        /// </summary>
        /// <value>The start index.</value>
        [ApiMember(Name = "StartIndex", Description = "Optional. The record index to start at. All items with a lower index will be dropped from the results.", IsRequired = false, DataType = "int", ParameterType = "query", Verb = "GET")]
        public int? StartIndex { get; set; }

        /// <summary>
        /// The maximum number of items to return
        /// </summary>
        /// <value>The limit.</value>
        [ApiMember(Name = "Limit", Description = "Optional. The maximum number of records to return", IsRequired = false, DataType = "int", ParameterType = "query", Verb = "GET")]
        public int? Limit { get; set; }
    }

    /// <summary>
    /// Class GetThemeSongs
    /// </summary>
    [Route("/Items/{Id}/ThemeSongs", "GET", Summary = "Gets theme songs for an item")]
    [Authenticated]
    public class GetThemeSongs : IReturn<ThemeMediaResult>
    {
        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        /// <value>The user id.</value>
        [ApiMember(Name = "UserId", Description = "Optional. Filter by user id, and attach user data", IsRequired = false, DataType = "string", ParameterType = "query", Verb = "GET")]
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        [ApiMember(Name = "Id", Description = "Item Id", IsRequired = true, DataType = "string", ParameterType = "path", Verb = "GET")]
        public string Id { get; set; }

        [ApiMember(Name = "InheritFromParent", Description = "Determines whether or not parent items should be searched for theme media.", IsRequired = false, DataType = "string", ParameterType = "query", Verb = "GET")]
        public bool InheritFromParent { get; set; }
    }

    /// <summary>
    /// Class GetThemeVideos
    /// </summary>
    [Route("/Items/{Id}/ThemeVideos", "GET", Summary = "Gets theme videos for an item")]
    [Authenticated]
    public class GetThemeVideos : IReturn<ThemeMediaResult>
    {
        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        /// <value>The user id.</value>
        [ApiMember(Name = "UserId", Description = "Optional. Filter by user id, and attach user data", IsRequired = false, DataType = "string", ParameterType = "query", Verb = "GET")]
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        [ApiMember(Name = "Id", Description = "Item Id", IsRequired = true, DataType = "string", ParameterType = "path", Verb = "GET")]
        public string Id { get; set; }

        [ApiMember(Name = "InheritFromParent", Description = "Determines whether or not parent items should be searched for theme media.", IsRequired = false, DataType = "string", ParameterType = "query", Verb = "GET")]
        public bool InheritFromParent { get; set; }
    }

    /// <summary>
    /// Class GetThemeVideos
    /// </summary>
    [Route("/Items/{Id}/ThemeMedia", "GET", Summary = "Gets theme videos and songs for an item")]
    [Authenticated]
    public class GetThemeMedia : IReturn<AllThemeMediaResult>
    {
        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        /// <value>The user id.</value>
        [ApiMember(Name = "UserId", Description = "Optional. Filter by user id, and attach user data", IsRequired = false, DataType = "string", ParameterType = "query", Verb = "GET")]
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        [ApiMember(Name = "Id", Description = "Item Id", IsRequired = true, DataType = "string", ParameterType = "path", Verb = "GET")]
        public string Id { get; set; }

        [ApiMember(Name = "InheritFromParent", Description = "Determines whether or not parent items should be searched for theme media.", IsRequired = false, DataType = "string", ParameterType = "query", Verb = "GET")]
        public bool InheritFromParent { get; set; }
    }

    [Route("/Library/Refresh", "POST", Summary = "Starts a library scan")]
    [Authenticated(Roles = "Admin")]
    public class RefreshLibrary : IReturnVoid
    {
    }

    [Route("/Items/{Id}", "DELETE", Summary = "Deletes an item from the library and file system")]
    [Authenticated]
    public class DeleteItem : IReturnVoid
    {
        [ApiMember(Name = "Id", Description = "Item Id", IsRequired = true, DataType = "string", ParameterType = "path", Verb = "DELETE")]
        public string Id { get; set; }
    }

    [Route("/Items", "DELETE", Summary = "Deletes an item from the library and file system")]
    [Authenticated]
    public class DeleteItems : IReturnVoid
    {
        [ApiMember(Name = "Ids", Description = "Ids", IsRequired = true, DataType = "string", ParameterType = "path", Verb = "DELETE")]
        public string Ids { get; set; }
    }

    [Route("/Items/Counts", "GET")]
    [Authenticated]
    public class GetItemCounts : IReturn<ItemCounts>
    {
        [ApiMember(Name = "UserId", Description = "Optional. Get counts from a specific user's library.", IsRequired = false, DataType = "string", ParameterType = "query", Verb = "GET")]
        public string UserId { get; set; }

        [ApiMember(Name = "IsFavorite", Description = "Optional. Get counts of favorite items", IsRequired = false, DataType = "bool", ParameterType = "query", Verb = "GET")]
        public bool? IsFavorite { get; set; }
    }

    [Route("/Items/{Id}/Ancestors", "GET", Summary = "Gets all parents of an item")]
    [Authenticated]
    public class GetAncestors : IReturn<BaseItemDto[]>
    {
        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        /// <value>The user id.</value>
        [ApiMember(Name = "UserId", Description = "Optional. Filter by user id, and attach user data", IsRequired = false, DataType = "string", ParameterType = "query", Verb = "GET")]
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        [ApiMember(Name = "Id", Description = "Item Id", IsRequired = true, DataType = "string", ParameterType = "path", Verb = "GET")]
        public string Id { get; set; }
    }

    [Route("/Items/YearIndex", "GET", Summary = "Gets a year index based on an item query.")]
    [Authenticated]
    public class GetYearIndex : IReturn<List<ItemIndex>>
    {
        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        /// <value>The user id.</value>
        [ApiMember(Name = "UserId", Description = "Optional. Filter by user id, and attach user data", IsRequired = false, DataType = "string", ParameterType = "query", Verb = "GET")]
        public string UserId { get; set; }

        [ApiMember(Name = "IncludeItemTypes", Description = "Optional. If specified, results will be filtered based on item type. This allows multiple, comma delimeted.", IsRequired = false, DataType = "string", ParameterType = "query", Verb = "GET", AllowMultiple = true)]
        public string IncludeItemTypes { get; set; }
    }

    /// <summary>
    /// Class GetPhyscialPaths
    /// </summary>
    [Route("/Library/PhysicalPaths", "GET", Summary = "Gets a list of physical paths from virtual folders")]
    [Authenticated(Roles = "Admin")]
    public class GetPhyscialPaths : IReturn<List<string>>
    {
    }

    [Route("/Library/MediaFolders", "GET", Summary = "Gets all user media folders.")]
    [Authenticated]
    public class GetMediaFolders : IReturn<ItemsResult>
    {
        [ApiMember(Name = "IsHidden", Description = "Optional. Filter by folders that are marked hidden, or not.", IsRequired = false, DataType = "boolean", ParameterType = "query", Verb = "GET")]
        public bool? IsHidden { get; set; }
    }

    [Route("/Library/Series/Added", "POST", Summary = "Reports that new episodes of a series have been added by an external source")]
    [Route("/Library/Series/Updated", "POST", Summary = "Reports that new episodes of a series have been added by an external source")]
    [Authenticated]
    public class PostUpdatedSeries : IReturnVoid
    {
        [ApiMember(Name = "TvdbId", Description = "Tvdb Id", IsRequired = false, DataType = "string", ParameterType = "path", Verb = "GET")]
        public string TvdbId { get; set; }
    }

    [Route("/Library/Movies/Added", "POST", Summary = "Reports that new movies have been added by an external source")]
    [Route("/Library/Movies/Updated", "POST", Summary = "Reports that new movies have been added by an external source")]
    [Authenticated]
    public class PostUpdatedMovies : IReturnVoid
    {
        [ApiMember(Name = "TmdbId", Description = "Tmdb Id", IsRequired = false, DataType = "string", ParameterType = "path", Verb = "GET")]
        public string TmdbId { get; set; }
        [ApiMember(Name = "ImdbId", Description = "Imdb Id", IsRequired = false, DataType = "string", ParameterType = "path", Verb = "GET")]
        public string ImdbId { get; set; }
    }

    [Route("/Items/{Id}/Download", "GET", Summary = "Downloads item media")]
    [Authenticated(Roles = "download")]
    public class GetDownload
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        [ApiMember(Name = "Id", Description = "Item Id", IsRequired = true, DataType = "string", ParameterType = "path", Verb = "GET")]
        public string Id { get; set; }
    }

    [Route("/Items/{Id}/Similar", "GET", Summary = "Gets similar items")]
    [Authenticated]
    public class GetSimilarItems : BaseGetSimilarItemsFromItem
    {
    }

    /// <summary>
    /// Class LibraryService
    /// </summary>
    public class LibraryService : BaseApiService
    {
        /// <summary>
        /// The _item repo
        /// </summary>
        private readonly IItemRepository _itemRepo;

        private readonly ILibraryManager _libraryManager;
        private readonly IUserManager _userManager;
        private readonly IUserDataManager _userDataManager;

        private readonly IDtoService _dtoService;
        private readonly IAuthorizationContext _authContext;
        private readonly IActivityManager _activityManager;
        private readonly ILocalizationManager _localization;
        private readonly ILiveTvManager _liveTv;
        private readonly IChannelManager _channelManager;
        private readonly ITVSeriesManager _tvManager;
        private readonly ILibraryMonitor _libraryMonitor;
        private readonly IFileSystem _fileSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="LibraryService" /> class.
        /// </summary>
        public LibraryService(IItemRepository itemRepo, ILibraryManager libraryManager, IUserManager userManager,
                              IDtoService dtoService, IUserDataManager userDataManager, IAuthorizationContext authContext, IActivityManager activityManager, ILocalizationManager localization, ILiveTvManager liveTv, IChannelManager channelManager, ITVSeriesManager tvManager, ILibraryMonitor libraryMonitor, IFileSystem fileSystem)
        {
            _itemRepo = itemRepo;
            _libraryManager = libraryManager;
            _userManager = userManager;
            _dtoService = dtoService;
            _userDataManager = userDataManager;
            _authContext = authContext;
            _activityManager = activityManager;
            _localization = localization;
            _liveTv = liveTv;
            _channelManager = channelManager;
            _tvManager = tvManager;
            _libraryMonitor = libraryMonitor;
            _fileSystem = fileSystem;
        }

        public object Get(GetSimilarItems request)
        {
            var user = !string.IsNullOrWhiteSpace(request.UserId) ? _userManager.GetUserById(request.UserId) : null;

            var item = string.IsNullOrEmpty(request.Id) ?
                (!string.IsNullOrWhiteSpace(request.UserId) ? user.RootFolder :
                _libraryManager.RootFolder) : _libraryManager.GetItemById(request.Id);

            if (item is Game)
            {
                return new GamesService(_userManager, _userDataManager, _libraryManager, _itemRepo, _dtoService)
                {
                    AuthorizationContext = AuthorizationContext,
                    Logger = Logger,
                    Request = Request,
                    SessionContext = SessionContext,
                    ResultFactory = ResultFactory

                }.Get(new GetSimilarGames
                {
                    Fields = request.Fields,
                    Id = request.Id,
                    Limit = request.Limit,
                    UserId = request.UserId
                });
            }
            if (item is MusicAlbum)
            {
                return new AlbumsService(_userManager, _userDataManager, _libraryManager, _itemRepo, _dtoService)
                {
                    AuthorizationContext = AuthorizationContext,
                    Logger = Logger,
                    Request = Request,
                    SessionContext = SessionContext,
                    ResultFactory = ResultFactory

                }.Get(new GetSimilarAlbums
                {
                    Fields = request.Fields,
                    Id = request.Id,
                    Limit = request.Limit,
                    UserId = request.UserId
                });
            }
            if (item is MusicArtist)
            {
                return new AlbumsService(_userManager, _userDataManager, _libraryManager, _itemRepo, _dtoService)
                {
                    AuthorizationContext = AuthorizationContext,
                    Logger = Logger,
                    Request = Request,
                    SessionContext = SessionContext,
                    ResultFactory = ResultFactory

                }.Get(new GetSimilarArtists
                {
                    Fields = request.Fields,
                    Id = request.Id,
                    Limit = request.Limit,
                    UserId = request.UserId
                });
            }

            var program = item as IHasProgramAttributes;
            var channelItem = item as ChannelVideoItem;

            if (item is Movie || (program != null && program.IsMovie) || (channelItem != null && channelItem.ContentType == ChannelMediaContentType.Movie) || (channelItem != null && channelItem.ContentType == ChannelMediaContentType.MovieExtra))
            {
                return new MoviesService(_userManager, _userDataManager, _libraryManager, _itemRepo, _dtoService, _channelManager)
                {
                    AuthorizationContext = AuthorizationContext,
                    Logger = Logger,
                    Request = Request,
                    SessionContext = SessionContext,
                    ResultFactory = ResultFactory

                }.Get(new GetSimilarMovies
                {
                    Fields = request.Fields,
                    Id = request.Id,
                    Limit = request.Limit,
                    UserId = request.UserId
                });
            }

            if (item is Series || (program != null && program.IsSeries) || (channelItem != null && channelItem.ContentType == ChannelMediaContentType.Episode))
            {
                return new TvShowsService(_userManager, _userDataManager, _libraryManager, _itemRepo, _dtoService, _tvManager)
                {
                    AuthorizationContext = AuthorizationContext,
                    Logger = Logger,
                    Request = Request,
                    SessionContext = SessionContext,
                    ResultFactory = ResultFactory

                }.Get(new GetSimilarShows
                {
                    Fields = request.Fields,
                    Id = request.Id,
                    Limit = request.Limit,
                    UserId = request.UserId
                });
            }

            return new ItemsResult();
        }

        public object Get(GetMediaFolders request)
        {
            var items = _libraryManager.GetUserRootFolder().Children.OrderBy(i => i.SortName).ToList();

            if (request.IsHidden.HasValue)
            {
                var val = request.IsHidden.Value;

                items = items.Where(i => i.IsHidden == val).ToList();
            }

            var dtoOptions = GetDtoOptions(request);

            var result = new ItemsResult
            {
                TotalRecordCount = items.Count,

                Items = items.Select(i => _dtoService.GetBaseItemDto(i, dtoOptions)).ToArray()
            };

            return ToOptimizedResult(result);
        }

        public void Post(PostUpdatedSeries request)
        {
            var series = _libraryManager.GetItems(new InternalItemsQuery
            {
                IncludeItemTypes = new[] { typeof(Series).Name }

            }).Items;

            series = series.Where(i => string.Equals(request.TvdbId, i.GetProviderId(MetadataProviders.Tvdb), StringComparison.OrdinalIgnoreCase)).ToArray();

            if (series.Length > 0)
            {
                foreach (var item in series)
                {
                    _libraryMonitor.ReportFileSystemChanged(item.Path);
                }
            }
            else
            {
                Task.Run(() => _libraryManager.ValidateMediaLibrary(new Progress<double>(), CancellationToken.None));
            }
        }

        public void Post(PostUpdatedMovies request)
        {
            var movies = _libraryManager.GetItems(new InternalItemsQuery
            {
                IncludeItemTypes = new[] { typeof(Movie).Name }

            }).Items;

            if (!string.IsNullOrWhiteSpace(request.ImdbId))
            {
                movies = movies.Where(i => string.Equals(request.ImdbId, i.GetProviderId(MetadataProviders.Imdb), StringComparison.OrdinalIgnoreCase)).ToArray();
            }
            else if (!string.IsNullOrWhiteSpace(request.TmdbId))
            {
                movies = movies.Where(i => string.Equals(request.TmdbId, i.GetProviderId(MetadataProviders.Tmdb), StringComparison.OrdinalIgnoreCase)).ToArray();
            }
            else
            {
                movies = new BaseItem[] { };
            }

            if (movies.Length > 0)
            {
                foreach (var item in movies)
                {
                    _libraryMonitor.ReportFileSystemChanged(item.Path);
                }
            }
            else
            {
                Task.Run(() => _libraryManager.ValidateMediaLibrary(new Progress<double>(), CancellationToken.None));
            }
        }

        public object Get(GetDownload request)
        {
            var item = _libraryManager.GetItemById(request.Id);
            var auth = _authContext.GetAuthorizationInfo(Request);

            var user = _userManager.GetUserById(auth.UserId);

            if (user != null)
            {
                if (!item.CanDownload(user))
                {
                    throw new ArgumentException("Item does not support downloading");
                }
            }
            else
            {
                if (!item.CanDownload())
                {
                    throw new ArgumentException("Item does not support downloading");
                }
            }

            var headers = new Dictionary<string, string>();

            // Quotes are valid in linux. They'll possibly cause issues here
            var filename = Path.GetFileName(item.Path).Replace("\"", string.Empty);
            headers["Content-Disposition"] = string.Format("attachment; filename=\"{0}\"", filename);

            if (user != null)
            {
                LogDownload(item, user, auth);
            }

            return ResultFactory.GetStaticFileResult(Request, new StaticFileResultOptions
            {
                Path = item.Path,
                ResponseHeaders = headers
            });
        }

        private async void LogDownload(BaseItem item, User user, AuthorizationInfo auth)
        {
            try
            {
                await _activityManager.Create(new ActivityLogEntry
                {
                    Name = string.Format(_localization.GetLocalizedString("UserDownloadingItemWithValues"), user.Name, item.Name),
                    Type = "UserDownloadingContent",
                    ShortOverview = string.Format(_localization.GetLocalizedString("AppDeviceValues"), auth.Client, auth.Device),
                    UserId = auth.UserId

                }).ConfigureAwait(false);
            }
            catch
            {
                // Logged at lower levels
            }
        }

        public object Get(GetFile request)
        {
            var item = _libraryManager.GetItemById(request.Id);
            var locationType = item.LocationType;
            if (locationType == LocationType.Remote || locationType == LocationType.Virtual)
            {
                throw new ArgumentException("This command cannot be used for remote or virtual items.");
            }
			if (_fileSystem.DirectoryExists(item.Path))
            {
                throw new ArgumentException("This command cannot be used for directories.");
            }

            return ToStaticFileResult(item.Path);
        }

        /// <summary>
        /// Gets the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>System.Object.</returns>
        public object Get(GetPhyscialPaths request)
        {
            var result = _libraryManager.RootFolder.Children
                .SelectMany(c => c.PhysicalLocations)
                .ToList();

            return ToOptimizedSerializedResultUsingCache(result);
        }

        /// <summary>
        /// Gets the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>System.Object.</returns>
        public object Get(GetAncestors request)
        {
            var result = GetAncestors(request);

            return ToOptimizedSerializedResultUsingCache(result);
        }

        /// <summary>
        /// Gets the ancestors.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>Task{BaseItemDto[]}.</returns>
        public List<BaseItemDto> GetAncestors(GetAncestors request)
        {
            var item = _libraryManager.GetItemById(request.Id);

            var baseItemDtos = new List<BaseItemDto>();

            var user = !string.IsNullOrWhiteSpace(request.UserId) ? _userManager.GetUserById(request.UserId) : null;

            var dtoOptions = GetDtoOptions(request);

            BaseItem parent = item.Parent;

            while (parent != null)
            {
                if (user != null)
                {
                    parent = TranslateParentItem(parent, user);
                }

                baseItemDtos.Add(_dtoService.GetBaseItemDto(parent, dtoOptions, user));

                parent = parent.Parent;
            }

            return baseItemDtos.ToList();
        }

        private BaseItem TranslateParentItem(BaseItem item, User user)
        {
            if (item.Parent is AggregateFolder)
            {
                return user.RootFolder.GetChildren(user, true).FirstOrDefault(i => i.PhysicalLocations.Contains(item.Path));
            }

            return item;
        }

        /// <summary>
        /// Gets the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>System.Object.</returns>
        public object Get(GetCriticReviews request)
        {
            var result = GetCriticReviews(request);

            return ToOptimizedSerializedResultUsingCache(result);
        }

        /// <summary>
        /// Gets the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>System.Object.</returns>
        public object Get(GetItemCounts request)
        {
            var filteredItems = GetAllLibraryItems(request.UserId, _userManager, _libraryManager, null, i => i.LocationType != LocationType.Virtual && FilterItem(i, request, request.UserId));

            var counts = new ItemCounts
            {
                AlbumCount = filteredItems.Count(i => i is MusicAlbum),
                EpisodeCount = filteredItems.Count(i => i is Episode),
                GameCount = filteredItems.Count(i => i is Game),
                GameSystemCount = filteredItems.Count(i => i is GameSystem),
                MovieCount = filteredItems.Count(i => i is Movie),
                SeriesCount = filteredItems.Count(i => i is Series),
                SongCount = filteredItems.Count(i => i is Audio),
                MusicVideoCount = filteredItems.Count(i => i is MusicVideo),
                BoxSetCount = filteredItems.Count(i => i is BoxSet),
                BookCount = filteredItems.Count(i => i is Book),

                UniqueTypes = filteredItems.Select(i => i.GetClientTypeName()).Distinct().ToList()
            };

            return ToOptimizedSerializedResultUsingCache(counts);
        }

        private bool FilterItem(BaseItem item, GetItemCounts request, string userId)
        {
            if (!string.IsNullOrWhiteSpace(userId))
            {
                if (request.IsFavorite.HasValue)
                {
                    var val = request.IsFavorite.Value;

                    if (_userDataManager.GetUserData(userId, item.GetUserDataKey()).IsFavorite != val)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Posts the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        public void Post(RefreshLibrary request)
        {
            try
            {
                _libraryManager.ValidateMediaLibrary(new Progress<double>(), CancellationToken.None);
            }
            catch (Exception ex)
            {
                Logger.ErrorException("Error refreshing library", ex);
            }
        }

        /// <summary>
        /// Deletes the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        public void Delete(DeleteItems request)
        {
            var ids = string.IsNullOrWhiteSpace(request.Ids)
             ? new string[] { }
             : request.Ids.Split(',');

            var tasks = ids.Select(i =>
            {
                var item = _libraryManager.GetItemById(i);
                var auth = _authContext.GetAuthorizationInfo(Request);
                var user = _userManager.GetUserById(auth.UserId);

                if (!item.CanDelete(user))
                {
                    if (ids.Length > 1)
                    {
                        throw new SecurityException("Unauthorized access");
                    }

                    return Task.FromResult(true);
                }

                if (item is ILiveTvRecording)
                {
                    return _liveTv.DeleteRecording(i);
                }

                return _libraryManager.DeleteItem(item);
            }).ToArray();

            Task.WaitAll(tasks);
        }

        /// <summary>
        /// Deletes the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        public void Delete(DeleteItem request)
        {
            Delete(new DeleteItems
            {
                Ids = request.Id
            });
        }

        /// <summary>
        /// Gets the critic reviews async.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>Task{ItemReviewsResult}.</returns>
        private QueryResult<ItemReview> GetCriticReviews(GetCriticReviews request)
        {
            var reviews = _itemRepo.GetCriticReviews(new Guid(request.Id));

            var reviewsArray = reviews.ToArray();

            var result = new QueryResult<ItemReview>
            {
                TotalRecordCount = reviewsArray.Length
            };

            if (request.StartIndex.HasValue)
            {
                reviewsArray = reviewsArray.Skip(request.StartIndex.Value).ToArray();
            }
            if (request.Limit.HasValue)
            {
                reviewsArray = reviewsArray.Take(request.Limit.Value).ToArray();
            }

            result.Items = reviewsArray;

            return result;
        }

        public object Get(GetThemeMedia request)
        {
            var themeSongs = GetThemeSongs(new GetThemeSongs
            {
                InheritFromParent = request.InheritFromParent,
                Id = request.Id,
                UserId = request.UserId

            });

            var themeVideos = GetThemeVideos(new GetThemeVideos
            {
                InheritFromParent = request.InheritFromParent,
                Id = request.Id,
                UserId = request.UserId

            });

            return ToOptimizedSerializedResultUsingCache(new AllThemeMediaResult
            {
                ThemeSongsResult = themeSongs,
                ThemeVideosResult = themeVideos,

                SoundtrackSongsResult = new ThemeMediaResult()
            });
        }

        /// <summary>
        /// Gets the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>System.Object.</returns>
        public object Get(GetThemeSongs request)
        {
            var result = GetThemeSongs(request);

            return ToOptimizedSerializedResultUsingCache(result);
        }

        private ThemeMediaResult GetThemeSongs(GetThemeSongs request)
        {
            var user = !string.IsNullOrWhiteSpace(request.UserId) ? _userManager.GetUserById(request.UserId) : null;

            var item = string.IsNullOrEmpty(request.Id)
                           ? (!string.IsNullOrWhiteSpace(request.UserId)
                                  ? user.RootFolder
                                  : (Folder)_libraryManager.RootFolder)
                           : _libraryManager.GetItemById(request.Id);

            while (GetThemeSongIds(item).Count == 0 && request.InheritFromParent && item.Parent != null)
            {
                item = item.Parent;
            }

            var dtoOptions = GetDtoOptions(request);

            var dtos = GetThemeSongIds(item).Select(_libraryManager.GetItemById)
                            .OrderBy(i => i.SortName)
                            .Select(i => _dtoService.GetBaseItemDto(i, dtoOptions, user, item));

            var items = dtos.ToArray();

            return new ThemeMediaResult
            {
                Items = items,
                TotalRecordCount = items.Length,
                OwnerId = _dtoService.GetDtoId(item)
            };
        }

        /// <summary>
        /// Gets the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>System.Object.</returns>
        public object Get(GetThemeVideos request)
        {
            var result = GetThemeVideos(request);

            return ToOptimizedSerializedResultUsingCache(result);
        }

        public ThemeMediaResult GetThemeVideos(GetThemeVideos request)
        {
            var user = !string.IsNullOrWhiteSpace(request.UserId) ? _userManager.GetUserById(request.UserId) : null;

            var item = string.IsNullOrEmpty(request.Id)
                           ? (!string.IsNullOrWhiteSpace(request.UserId)
                                  ? user.RootFolder
                                  : (Folder)_libraryManager.RootFolder)
                           : _libraryManager.GetItemById(request.Id);

            while (GetThemeVideoIds(item).Count == 0 && request.InheritFromParent && item.Parent != null)
            {
                item = item.Parent;
            }

            var dtoOptions = GetDtoOptions(request);

            var dtos = GetThemeVideoIds(item).Select(_libraryManager.GetItemById)
                            .OrderBy(i => i.SortName)
                            .Select(i => _dtoService.GetBaseItemDto(i, dtoOptions, user, item));

            var items = dtos.ToArray();

            return new ThemeMediaResult
            {
                Items = items,
                TotalRecordCount = items.Length,
                OwnerId = _dtoService.GetDtoId(item)
            };
        }

        private List<Guid> GetThemeVideoIds(BaseItem item)
        {
            var i = item as IHasThemeMedia;

            if (i != null)
            {
                return i.ThemeVideoIds;
            }

            return new List<Guid>();
        }

        private List<Guid> GetThemeSongIds(BaseItem item)
        {
            var i = item as IHasThemeMedia;

            if (i != null)
            {
                return i.ThemeSongIds;
            }

            return new List<Guid>();
        }

        private readonly CultureInfo _usCulture = new CultureInfo("en-US");

        public object Get(GetYearIndex request)
        {
            var includeTypes = string.IsNullOrWhiteSpace(request.IncludeItemTypes)
             ? new string[] { }
             : request.IncludeItemTypes.Split(',');

            Func<BaseItem, bool> filter = i =>
            {
                if (includeTypes.Length > 0)
                {
                    if (!includeTypes.Contains(i.GetType().Name, StringComparer.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }

                return true;
            };

            IEnumerable<BaseItem> items = GetAllLibraryItems(request.UserId, _userManager, _libraryManager, null, filter);

            var lookup = items
                .ToLookup(i => i.ProductionYear ?? -1)
                .OrderBy(i => i.Key)
                .Select(i => new ItemIndex
                {
                    ItemCount = i.Count(),
                    Name = i.Key == -1 ? string.Empty : i.Key.ToString(_usCulture)
                })
                .ToList();

            return ToOptimizedSerializedResultUsingCache(lookup);
        }
    }
}
