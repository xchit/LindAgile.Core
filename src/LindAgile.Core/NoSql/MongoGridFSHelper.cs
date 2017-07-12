using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Linq;
using MongoDB.Driver.GridFS;
using System.IO;

namespace LindAgile.Core.NoSql
{
    /// <summary>
    /// mongodb文件存储
    /// </summary>
    public class MongoGridFSHelper
    {
        #region Constructs & Fields
        private readonly IMongoClient client;
        private readonly IMongoDatabase database;
        private readonly IMongoCollection<BsonDocument> collection;
        private readonly GridFSBucket bucket;
        private GridFSFileInfo fileInfo;
        private ObjectId oid;
        public static MongoGridFSHelper Instance;
        private static object lockObj = new object();
        private MongoGridFSHelper()
         : this(
             GlobalConfig.ConfigManager.Config.MongoDB.Host, GlobalConfig.ConfigManager.Config.MongoDB.DbName,
             "FileSystem")
        {
        }
        public MongoGridFSHelper(string url, string db, string collectionName)
        {
            if (url == null)
            {
                throw new ArgumentNullException("url");
            }
            else
            {
                var svrSettings = MongoUrl.Create("mongodb://" + url);
                client = new MongoClient(svrSettings);
            }

            if (db == null)
            {
                throw new ArgumentNullException("db");
            }
            else
            {
                database = client.GetDatabase(db);
            }

            if (collectionName == null)
            {
                throw new ArgumentNullException("collectionName");
            }
            else
            {
                collection = database.GetCollection<BsonDocument>(collectionName);
            }

            GridFSBucketOptions gfbOptions = new GridFSBucketOptions()
            {
                BucketName = "bird",
                ChunkSizeBytes = 1 * 1024 * 1024,
                ReadConcern = null,
                ReadPreference = null,
                WriteConcern = null
            };
            var bucket = new GridFSBucket(database, new GridFSBucketOptions
            {
                BucketName = "videos",
                ChunkSizeBytes = 1048576, // 1MB  
                WriteConcern = WriteConcern.WMajority,
                ReadPreference = ReadPreference.Secondary
            });
            this.bucket = new GridFSBucket(database, null);
        }
        public MongoGridFSHelper(IMongoCollection<BsonDocument> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            this.collection = collection;
            this.bucket = new GridFSBucket(collection.Database);
        }
        static MongoGridFSHelper()
        {
            if (Instance == null)
            {
                lock (lockObj)
                {
                    if (Instance == null)
                    {
                        Instance = new MongoGridFSHelper();
                    }
                }
            }
        }

        #endregion

        #region Methods
        /// <summary>
        /// 上传到GridFS
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public ObjectId UploadGridFSFromBytes(string filename, Byte[] source)
        {
            oid = bucket.UploadFromBytes(filename, source);
            return oid;
        }
        /// <summary>
        /// 上传到GridFS
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public ObjectId UploadGridFSFromStream(string filename, Stream source)
        {
            using (source)
            {
                oid = bucket.UploadFromStream(filename, source);
                return oid;
            }
        }
        /// <summary>
        /// 下载成byte数组
        /// </summary>
        /// <param name="id">mongodb主键</param>
        /// <returns></returns>
        public Byte[] DownloadAsByteArray(ObjectId id)
        {
            Byte[] bytes = bucket.DownloadAsBytes(id);
            return bytes;
        }
        /// <summary>
        /// 下载成流
        /// </summary>
        /// <param name="id">mongodb主键</param>
        /// <returns></returns>
        public Stream DownloadToStream(ObjectId id)
        {
            Stream destination = new MemoryStream();
            bucket.DownloadToStream(id, destination);
            return destination;
        }
        /// <summary>
        /// 下载成byte数组
        /// </summary>
        /// <param name="filename">文件名</param>
        /// <returns></returns>
        public Byte[] DownloadAsBytesByName(string filename)
        {
            Byte[] bytes = bucket.DownloadAsBytesByName(filename);
            return bytes;
        }
        /// <summary>
        /// 下载成流
        /// </summary>
        /// <param name="filename">文件名</param>
        /// <returns></returns>
        public Stream DownloadToStreamByName(string filename)
        {
            Stream destination = new MemoryStream();
            bucket.DownloadToStreamByName(filename, destination);
            return destination;
        }
        /// <summary>
        /// 查询文件
        /// </summary>
        /// <param name="filename">文件名</param>
        /// <returns></returns>
        public GridFSFileInfo FindFiles(string filename)
        {
            var filter = Builders<GridFSFileInfo>.Filter.And(
            Builders<GridFSFileInfo>.Filter.Eq(x => x.Filename, "man"),
            Builders<GridFSFileInfo>.Filter.Gte(x => x.UploadDateTime, new DateTime(2015, 1, 1, 0, 0, 0, DateTimeKind.Utc)),
            Builders<GridFSFileInfo>.Filter.Lt(x => x.UploadDateTime, new DateTime(2017, 2, 1, 0, 0, 0, DateTimeKind.Utc)));
            var sort = Builders<GridFSFileInfo>.Sort.Descending(x => x.UploadDateTime);
            var options = new GridFSFindOptions
            {
                Limit = 1,
                Sort = sort
            };
            using (var cursor = bucket.Find(filter, options))
            {
                fileInfo = cursor.ToList().FirstOrDefault();
            }
            return fileInfo;
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="id"></param>
        public void DeleteAndRename(ObjectId id)
        {
            bucket.Delete(id);
        }

        /// <summary>
        /// 删除所有
        /// </summary>
        public void DroppGridFSBucket()
        {
            bucket.Drop();
        }

        /// <summary>
        /// 文件重名名
        /// </summary>
        /// <param name="id"></param>
        /// <param name="newFilename"></param>
        public void RenameAsingleFile(ObjectId id, string newFilename)
        {
            bucket.Rename(id, newFilename);
        }
        /// <summary>
        /// 递归文件重名名
        /// </summary>
        /// <param name="oldFilename"></param>
        /// <param name="newFilename"></param>
        public void RenameAllRevisionsOfAfile(string oldFilename, string newFilename)
        {
            var filter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Filename, oldFilename);
            var filesCursor = bucket.Find(filter);
            var files = filesCursor.ToList();
            foreach (var file in files)
            {
                bucket.Rename(file.Id, newFilename);
            }
        }
        #endregion


    }


}
