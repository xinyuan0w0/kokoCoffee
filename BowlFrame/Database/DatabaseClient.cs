using BowlFrame.Config;
using BowlFrame.Database.TableStruct;
using BowlFrame.Exceptions.Database.DatabaseEx;
using Newtonsoft.Json.Linq;
using SqlSugar;
using static BowlFrame.Database.TableStruct.DataTypeToJTokenType;
using static BowlFrame.Tools.Logger;

namespace BowlFrame.Database
{
    public class DatabaseClient
    {
        protected SqlSugarClient _client;

        public SqlSugarClient Client => _client;

        public string? UUID { get; set; }

        public DatabaseClient()
        {
            //默认配置
            ConnectionConfig config = new()
            {
                DbType = DatabaseConfig.Driver,
                ConnectionString = DatabaseConfig.ConnectionString,
                IsAutoCloseConnection = true
            };
            CreateClient(config);

            if (_client is null)
                throw new NullReferenceException();
        }

        public DatabaseClient(ConnectionConfig config)
        {
            CreateClient(config);

            if (_client is null)
                throw new NullReferenceException();
        }

        private void CreateClient(ConnectionConfig config)
        {
            //创建客户端
            try
            {
                _client = new SqlSugarClient(config, db => { db.Aop.OnLogExecuting = (sql, pars) => Log.Trace(UtilMethods.GetNativeSql(sql, pars)); });
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        public async Task<JObject> ReadJsonFromData(string key, string? subKey = null, string? uuid = null, CancellationToken cancellationToken = default)
        {
            if (uuid is null)
                if (UUID is not null)
                    uuid = UUID;
                else
                    throw new NullReferenceException();

            ISugarQueryable<DbData> query = _client.Queryable<DbData>()
                .Where(a => a.UUID == uuid && a.Key == key && (subKey == null || a.SubKey == subKey));

            #region 构建Json

            using JTokenWriter writer = new();
            writer.WriteStartObject();
            writer.WritePropertyName(key);

            if (await query.AnyAsync())
            {
                writer.WriteStartObject();
                foreach (DbData data in await query.ToArrayAsync())
                {
                    if (cancellationToken.IsCancellationRequested)
                        cancellationToken.ThrowIfCancellationRequested();
                    writer.WritePropertyName(data.SubKey);
                    if (!WriteJsonValue(writer, data.DataType, data.Value))
                        throw new ReadDataError(key, subKey);
                }
                writer.WriteEndObject();
            }
            else
                writer.WriteNull();

            writer.WriteEndObject();

            #endregion 构建Json

            return writer.Token as JObject ?? throw new ReadDataError(key, subKey);
        }

        private static bool WriteJsonValue(JTokenWriter writer, DbDataType dataType, string value)
        {
            switch (dataType)
            {
                case DbDataType.Null:
                    writer.WriteNull();
                    break;

                case DbDataType.Integer:
                    writer.WriteValue(Convert.ToInt64(value));
                    break;

                case DbDataType.Float:
                    writer.WriteValue(Convert.ToDouble(value));
                    break;

                case DbDataType.Boolean:
                    writer.WriteValue(value.Equals("true", StringComparison.CurrentCultureIgnoreCase));
                    break;

                case DbDataType.String:
                    writer.WriteValue(value);
                    break;

                case DbDataType.Array:
                    JArray.Parse(value).WriteTo(writer);
                    break;

                case DbDataType.Object:
                    JObject.Parse(value).WriteTo(writer);
                    break;

                case DbDataType.Date:
                    writer.WriteValue(Convert.ToDateTime(value));
                    break;

                case DbDataType.Bytes:
                    writer.WriteValue(Convert.FromBase64String(value));
                    break;

                default:
                    return false;
            }
            return true;
        }

        public async Task<JObject> ReadJsonFromData(ArraySegment<string> keys, string? subKey = null, string? uuid = null)
        {
            if (uuid is null)
                if (UUID is not null)
                    uuid = UUID;
                else
                    throw new NullReferenceException();

            JObject value = [];
            foreach (string key in keys)
                value.Merge(await ReadJsonFromData(uuid, key, subKey));
            return value;
        }

        public async Task WriteJsonIntoData(JObject values, string? uuid = null, CancellationToken cancellationToken = default)
        {
            if (uuid is null)
                if (UUID is not null)
                    uuid = UUID;
                else
                    throw new NullReferenceException();

            try
            {
                await _client.BeginTranAsync(System.Data.IsolationLevel.ReadUncommitted);

                List<DbData> list = [];

                foreach (JProperty property in values.Properties())
                    //锁行
                    _ = _client.Queryable<DbData>().TranLock(DbLockType.Wait).Where(a => a.UUID == uuid && a.Key == property.Name).ToList();
                //第一层Json循环
                foreach (JProperty property in values.Properties())
                {
                    if (cancellationToken.IsCancellationRequested)
                        cancellationToken.ThrowIfCancellationRequested();

                    ISugarQueryable<DbData>? query;

                    //第二次Json循环
                    foreach (JProperty property_2 in property.Value.Cast<JProperty>())
                    {
                        if (property_2.Value is null || property_2.Value.Type == JTokenType.Comment)
                            continue;

                        //创建查询对象
                        query = _client.Queryable<DbData>()
                                .Where(a => a.UUID == uuid && a.Key == property.Name && a.SubKey == property_2.Name);

                        if (cancellationToken.IsCancellationRequested)
                            cancellationToken.ThrowIfCancellationRequested();

                        DbData[] data = await query.ToArrayAsync();

                        //超过一条数据
                        if (data.Length > 1)
                            throw new ExtraData(property.Name, property_2.Name);
                        //插入数据
                        else if (data.Length == 0)
                            list.Add(
                                new DbData
                                {
                                    UUID = uuid,
                                    Key = property.Name,
                                    SubKey = property_2.Name,
                                    DataType = GetDataType(property_2.Value.Type) ?? throw new NullReferenceException(),
                                    Value = property_2.Value.ToString()
                                });
                        //更新数据
                        //判断数据是否更新
                        else if (data[0].DataType != (GetDataType(property_2.Value.Type) ?? throw new NullReferenceException())
                            || data[0].Value != (property_2.Value.Type == JTokenType.Bytes ? Convert.ToBase64String((byte[]?)property_2.Value ?? []) : property_2.Value.ToString()))
                            list.Add(
                                new DbData
                                {
                                    UUID = uuid,
                                    Key = property.Name,
                                    SubKey = property_2.Name,
                                    DataType = GetDataType(property_2.Value!.Type) ?? throw new NullReferenceException(),
                                    Value = property_2.Value!.ToString()
                                });
                    }
                }

                if (list.Count > 0)
                {
                    int lineCount = await _client.Storageable(list).WhereColumns(a => new { a.UUID, a.Key, a.SubKey }).ExecuteCommandAsync(cancellationToken);
                    Log.Debug("增加/修改了 {0} 行", lineCount);
                }
            }
            catch (OperationCanceledException e)
            {
                await _client.RollbackTranAsync();
                Log.Error(e);
                throw;
            }
            catch (Exception e)
            {
                await _client.RollbackTranAsync();
                Log.Error(e);
                throw new WriteDataError();
            }

            await _client.CommitTranAsync();
        }

        public async Task AddData(string key, string subKey, DbDataType dbDataType, string value, string? uuid = null, CancellationToken cancellationToken = default)
        {
            if (uuid is null)
                if (UUID is not null)
                    uuid = UUID;
                else
                    throw new NullReferenceException();

            try
            {
                if (cancellationToken.IsCancellationRequested)
                    cancellationToken.ThrowIfCancellationRequested();

                ISugarQueryable<DbData> query = _client.Queryable<DbData>()
                    .Where(a => a.UUID == uuid && a.Key == key && a.SubKey == subKey);

                if (cancellationToken.IsCancellationRequested)
                    cancellationToken.ThrowIfCancellationRequested();

                DbData[] data = await query.ToArrayAsync();

                if (data.Length > 1)
                    throw new ExtraData(key, subKey);
                else if (data.Length == 0)
                    await _client.Insertable(new DbData
                    {
                        UUID = uuid,
                        Key = key,
                        SubKey = subKey,
                        DataType = dbDataType,
                        Value = value
                    }).ExecuteCommandAsync(cancellationToken);
                else if (data[0].DataType != dbDataType || data[0].Value != value)
                    await _client.Updateable<DbData>()
                        .SetColumns(a => a.DataType == dbDataType)
                        .SetColumns(a => a.Value == value)
                        .Where(a => a.UUID == uuid && a.Key == key && a.SubKey == subKey)
                        .ExecuteCommandAsync(cancellationToken);
            }
            catch (OperationCanceledException e)
            {
                Log.Error(e);
                throw;
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw new WriteDataError();
            }
        }

        public async Task RemoveData(string key, string subKey, string? uuid = null, CancellationToken cancellationToken = default)
        {
            if (uuid is null)
                if (UUID is not null)
                    uuid = UUID;
                else
                    throw new NullReferenceException();

            try
            {
                if (cancellationToken.IsCancellationRequested)
                    cancellationToken.ThrowIfCancellationRequested();

                ISugarQueryable<DbData> query = _client.Queryable<DbData>()
                    .Where(a => a.UUID == uuid && a.Key == key && a.SubKey == subKey);

                if (cancellationToken.IsCancellationRequested)
                    cancellationToken.ThrowIfCancellationRequested();

                DbData[] data = await query.ToArrayAsync();

                if (data.Length > 1)
                    throw new ExtraData(key, subKey);
                else
                    await _client.Deleteable<DbData>()
                        .Where(a => a.UUID == uuid && a.Key == key && a.SubKey == subKey)
                        .ExecuteCommandAsync(cancellationToken);
            }
            catch (OperationCanceledException e)
            {
                Log.Error(e);
                throw;
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw new RemoveDataError();
            }
        }

        public async Task<string> ReadData(string key, string subKey, string? uuid = null, CancellationToken cancellationToken = default)
        {
            if (uuid is null)
                if (UUID is not null)
                    uuid = UUID;
                else
                    throw new NullReferenceException();

            try
            {
                if (cancellationToken.IsCancellationRequested)
                    cancellationToken.ThrowIfCancellationRequested();

                ISugarQueryable<DbData> query = _client.Queryable<DbData>()
                    .Where(a => a.UUID == uuid && a.Key == key && a.SubKey == subKey);

                if (cancellationToken.IsCancellationRequested)
                    cancellationToken.ThrowIfCancellationRequested();

                DbData[] data = await query.ToArrayAsync();

                if (data.Length > 1)
                    throw new ExtraData(key, subKey);

                return data[0].Value;
            }
            catch (OperationCanceledException e)
            {
                Log.Error(e);
                throw;
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw new ReadDataError();
            }
        }

        public async Task SafeChangeDataNumber(ChangeInfo change, bool _a = true, CancellationToken cancellationToken = default)
        {
            if (change.UUID is null)
                if (UUID is not null)
                    change.UUID = UUID;
                else
                    throw new NullReferenceException();

            try
            {
                if (cancellationToken.IsCancellationRequested)
                    cancellationToken.ThrowIfCancellationRequested();

                if (change.Value is null || change.IsDouble is null)
                    return;

                bool isDouble = (bool)change.IsDouble;

                var value = Convert.ChangeType(change.Value, isDouble ? typeof(decimal) : typeof(long));

                //创建查询对象
                ISugarQueryable<DbData> query = _client.Queryable<DbData>()
                        .Where(a => a.UUID == change.UUID && a.Key == change.Key && a.SubKey == change.SubKey);

                if (cancellationToken.IsCancellationRequested)
                    cancellationToken.ThrowIfCancellationRequested();

                DbData[] data = await query.ToArrayAsync();

                //超过一条数据
                if (data.Length > 1)
                    throw new ExtraData(change.Key, change.SubKey);
                //插入数据
                else if (data.Length == 0)
                    _client.Insertable(new DbData()
                    {
                        UUID = change.UUID,
                        Key = change.Key,
                        SubKey = change.SubKey,
                        DataType = isDouble ? DbDataType.Float : DbDataType.Integer,
                        Value = value.ToString()!,
                    }).AddQueue();
                //更新数据
                else if (data[0].DataType == DbDataType.Integer || data[0].DataType == DbDataType.Float)
                    _client.Updateable<DbData>()
                        .SetColumns(a => a.Value == a.Value + value)
                        .SetColumns(a => a.DataType == (a.DataType == DbDataType.Float ? DbDataType.Float : (isDouble ? DbDataType.Float : DbDataType.Integer)))
                        .Where(a => a.UUID == change.UUID && a.Key == change.Key && a.SubKey == change.SubKey)
                        .AddQueue();
                else
                    throw new WriteDataError(change.Key, change.SubKey);

                if (cancellationToken.IsCancellationRequested)
                    cancellationToken.ThrowIfCancellationRequested();

                //直接提交
                if (_a)
                    await _client.SaveQueuesAsync();
            }
            catch (OperationCanceledException e)
            {
                Log.Error(e);
                throw;
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw new WriteDataError();
            }
        }

        public async Task SafeChangeDataNumber(ArraySegment<ChangeInfo> changes, CancellationToken cancellationToken = default)
        {
            await _client.BeginTranAsync(System.Data.IsolationLevel.ReadUncommitted);

            //foreach (ChangeInfo change in changes)
            //    //锁行
            //    _ = _client.Queryable<DbData>().TranLock(DbLockType.Wait).Where(a => a.UUID == change.UUID && a.Key == change.Key && a.SubKey == change.SubKey).ToList();

            try
            {
                foreach (ChangeInfo change in changes)
                    await SafeChangeDataNumber(change, false, cancellationToken);
                await _client.SaveQueuesAsync();
            }
            catch (OperationCanceledException e)
            {
                await _client.RollbackTranAsync();
                Log.Error(e);
                throw;
            }
            catch (Exception e)
            {
                await _client.RollbackTranAsync();
                Log.Error(e);
                throw;
            }

            await _client.CommitTranAsync();
        }
    }
}