using BowlFrame.Config;
using BowlFrame.Database.TableStruct;
using BowlFrame.Exceptions.Database.DatabaseEx;
using Newtonsoft.Json.Linq;
using SqlSugar;
using System.Threading;
using System;
using static BowlFrame.Database.TableStruct.DataTypeToJTokenType;
using static BowlFrame.Tools.Logger;
using SqlSugar.Extensions;

namespace BowlFrame.Database
{
    public class DatabaseClient
    {
        protected SqlSugarClient _client;

        public SqlSugarClient Client
        { get { return _client; } }

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

        public async Task<JObject> ReadJsonFromData(string uuid, string key, string? subKey = null, CancellationToken cancellationToken = default)
        {
            ISugarQueryable<DbData> table = _client.Queryable<DbData>();

            ISugarQueryable<DbData> query = from a in table
                                            where a.UUID == uuid && a.Key == key && (subKey == null || a.SubKey == subKey)
                                            select a;

            #region 构建Json

            JTokenWriter writer = new();
            writer.WriteStartObject();
            writer.WritePropertyName(key);

            if (await query.CountAsync(cancellationToken) > 0)
            {
                writer.WriteStartObject();

                foreach (DbData data in await query.ToArrayAsync())
                {
                    if (cancellationToken.IsCancellationRequested)
                        cancellationToken.ThrowIfCancellationRequested();

                    writer.WritePropertyName(data.SubKey);
                    switch (data.DataType)
                    {
                        case DbDataType.Null:
                            writer.WriteNull();
                            break;

                        case DbDataType.Integer:
                            writer.WriteValue(Convert.ToInt64(data.Value));
                            break;

                        case DbDataType.Float:
                            writer.WriteValue(Convert.ToDouble(data.Value));
                            break;

                        case DbDataType.Boolean:
                            writer.WriteValue(data.Value.Equals("true", StringComparison.CurrentCultureIgnoreCase));
                            break;

                        case DbDataType.String:
                            writer.WriteValue(data.Value);
                            break;

                        case DbDataType.Array:
                            JArray.Parse(data.Value).WriteTo(writer);
                            break;

                        case DbDataType.Object:
                            JObject.Parse(data.Value).WriteTo(writer);
                            break;

                        case DbDataType.Date:
                            writer.WriteValue(Convert.ToDateTime(data.Value));
                            break;

                        case DbDataType.Bytes:
                            writer.WriteValue(Convert.FromBase64String(data.Value));
                            break;

                        default:
                            throw new ReadDataError(key, subKey);
                    }
                }
                writer.WriteEndObject();
            }
            else
                writer.WriteNull();

            writer.WriteEndObject();

            #endregion 构建Json

            return writer.Token as JObject ?? throw new ReadDataError(key, subKey);
        }

        public async Task<JObject> ReadJsonFromData(string uuid, ArraySegment<string> keys, string? subKey = null)
        {
            JObject value = [];
            foreach (string key in keys)
                value.Merge(await ReadJsonFromData(uuid, key, subKey));
            return value;
        }

        public async Task WriteJsonIntoData(string uuid, JObject values, CancellationToken cancellationToken = default)
        {
            await _client.BeginTranAsync(System.Data.IsolationLevel.ReadUncommitted);

            List<DbData> list = [];

            foreach (JProperty property in values.Properties())
                //锁行
                _ = _client.Queryable<DbData>().TranLock(DbLockType.Wait).Where(a => a.UUID == uuid && a.Key == property.Name).ToList();

            try
            {
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
                            throw new WriteDataError(property.Name, property_2.Name);
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

        public async Task SafeChangeDataNumber(ChangeInfo change, CancellationToken cancellationToken = default)
        {
            try
            {
                ISugarQueryable<DbData>? query;

                if (cancellationToken.IsCancellationRequested)
                    cancellationToken.ThrowIfCancellationRequested();

                if (change.Value is null || change.IsDouble is null)
                    return;

                bool isDouble = (bool)change.IsDouble;

                var value = Convert.ChangeType(change.Value, isDouble ? typeof(decimal) : typeof(long));

                //创建查询对象
                query = _client.Queryable<DbData>()
                        .Where(a => a.UUID == change.UUID && a.Key == change.Key && a.SubKey == change.SubKey);

                if (cancellationToken.IsCancellationRequested)
                    cancellationToken.ThrowIfCancellationRequested();

                DbData[] data = await query.ToArrayAsync();

                //超过一条数据
                if (data.Length > 1)
                    throw new WriteDataError(change.Key, change.SubKey);
                //插入数据
                else if (data.Length == 0)
                    await _client.Insertable(new DbData()
                    {
                        UUID = change.UUID,
                        Key = change.Key,
                        SubKey = change.SubKey,
                        DataType = isDouble ? DbDataType.Float : DbDataType.Integer,
                        Value = value.ToString()!,
                    }).ExecuteCommandAsync(cancellationToken);
                //更新数据
                else if (data[0].DataType == DbDataType.Integer || data[0].DataType == DbDataType.Float)
                    await _client.Updateable<DbData>()
                        .SetColumns(a => a.Value == a.Value + value)
                        .SetColumns(a => a.DataType == (a.DataType == DbDataType.Float ? DbDataType.Float : (isDouble ? DbDataType.Float : DbDataType.Integer)))
                        .Where(a => a.UUID == change.UUID && a.Key == change.Key && a.SubKey == change.SubKey)
                        .ExecuteCommandAsync(cancellationToken);
                else
                    throw new WriteDataError(change.Key, change.SubKey);
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
                    await SafeChangeDataNumber(change, cancellationToken);
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