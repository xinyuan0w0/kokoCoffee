using BowlFrame.Adapter;
using BowlFrame.Config;
using BowlFrame.Database;
using BowlFrame.Database.TableStruct;
using BowlFrame.Event;
using BowlFrame.Tools;
using NanoidDotNet;
using Newtonsoft.Json.Linq;
using SqlSugar;
using static BowlFrame.Tools.Logger;

namespace BowlFrame.Perm
{
    public class Permission
    {
        private readonly DatabaseClient _database;

        public Permission(IPlatform platform, string? uuid = null, SourceType? sourceType = null) : this(new DatabaseClient(), platform, uuid, sourceType)
        {
        }

        public Permission(DatabaseClient database, IPlatform platform, string? uuid = null, SourceType? sourceType = null)
        {
            _database = database;
            Platform = platform;
            if (uuid is not null)
                UUID = uuid;
            if (sourceType is not null)
                SourceType = sourceType;
        }

        public string? UUID { get; private set; }

        public IPlatform Platform { get; init; }

        public SourceType? SourceType { get; set; }

        public async Task<string> CreateTarget(string id, TargetType targetType, IPlatform platform, string? fatherUUID = null, CancellationToken cancellationToken = default)
        {
            try
            {
                await _database.Client.BeginTranAsync();

                ISugarQueryable<DbPlatformID> query =
                _database.Client.Queryable<DbPlatformID>()
                .Where(a => a.ID == id && a.Platform == platform.ID);

                if (await query.AnyAsync())
                {
                    Log.Debug("尝试创建一个重复的对象被阻止 id: {0} Target: {1} platformID: {2} fatherUUID: {3}", id, targetType, platform.ID, fatherUUID);
                    return "";
                }

                string uuid = Nanoid.Generate();
                _database.Client.Insertable(new DbID
                {
                    UUID = uuid,
                    Type = targetType,
                }).AddQueue();

                _database.Client.Insertable(new DbPlatformID
                {
                    UUID = uuid,
                    ID = id,
                    FatherUUID = fatherUUID,
                    Platform = platform.ID,
                }).AddQueue();

                if (cancellationToken.IsCancellationRequested)
                    cancellationToken.ThrowIfCancellationRequested();

                await _database.Client.SaveQueuesAsync();

                JToken rawValue = JObject.Parse(
                    TextHelper.ReplacePlaceholder(
                        ConfigLoad.GetFileString("Target") ?? throw new NullReferenceException()
                        )
                    )["CreateTarget"] ?? throw new NullReferenceException("Target.json 缺少 CreateTarget 对象");

                JObject writer = [];

                if (rawValue["Global"] is not null)
                    writer.Merge(rawValue["Global"]![targetType.ToString()]);
                if (rawValue[platform.ID] is not null)
                    writer.Merge(rawValue[platform.ID]![targetType.ToString()]);

                await _database.WriteJsonIntoData(uuid, writer, cancellationToken);
                await _database.Client.CommitTranAsync();

                UUID = uuid;
                return uuid;
            }
            catch (Exception e)
            {
                await _database.Client.RollbackTranAsync();
                Log.Error(e);
                return "";
            }
        }
    }
}