using BowlFrame.Adapter;
using BowlFrame.Config;
using BowlFrame.Database;
using BowlFrame.Database.TableStruct;
using BowlFrame.Event;
using BowlFrame.Tools;
using BowlFrame.Target;
using NanoidDotNet;
using Newtonsoft.Json.Linq;
using SqlSugar;
using static BowlFrame.Tools.Logger;

namespace BowlFrame.Perm
{
    public class Permission
    {
        private readonly DatabaseClient _database;

        public Permission(string? uuid = null, string? fatherUUID = null, Permission? linkTarget = null)
            : this(new DatabaseClient() { UUID = uuid }, uuid, fatherUUID, linkTarget)
        {
        }

        public Permission(DatabaseClient database, string? uuid = null, string? fatherUUID = null, Permission? linkTarget = null)
        {
            _database = database;
            if (uuid is not null)
                UUID = uuid;
            if (fatherUUID is not null)
                FatherUUID = fatherUUID;
            if (linkTarget is not null)
                LinkTarget = linkTarget;
        }

        private string? uuid;

        public string? UUID
        { get => uuid; private set { _database.UUID = value; uuid = value; } }

        public string? FatherUUID { get; private set; }

        /// <summary>
        /// 常用于对当前场景的权限判断
        /// </summary>
        public Permission? LinkTarget { get; init; }

        public required IPlatform Platform { get; init; }

        public SourceType? SourceType { get; set; }

        /// <summary>
        /// 添加对象(初始化)
        /// </summary>
        /// <param groupsName="id"></param>
        /// <param groupsName="targetType"></param>
        /// <param groupsName="platform"></param>
        /// <param groupsName="fatherUUID"></param>
        /// <param groupsName="cancellationToken"></param>
        /// <returns></returns>
        public async Task<string?> CreateTarget(string id, TargetType targetType, IPlatform? platform = null, string? fatherUUID = null, CancellationToken cancellationToken = default)
        {
            platform ??= Platform;

            try
            {
                await _database.Client.BeginTranAsync();

                ISugarQueryable<DbPlatformID> query =
                _database.Client.Queryable<DbPlatformID>()
                .Where(a => a.ID == id && a.Platform == platform.ID);

                if (await query.AnyAsync())
                {
                    Log.Debug("尝试创建一个重复的对象被阻止 id: {0} target: {1} platformID: {2} fatherUUID: {3}", id, targetType, platform.ID, fatherUUID);
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
                    TextHelper.ReplacePlaceholders(
                        ConfigLoad.GetFileString("Target") ?? throw new NullReferenceException()
                        )
                    )["CreateTarget"] ?? throw new NullReferenceException("Target.json 缺少 CreateTarget 对象");

                JObject writer = [];

                if (rawValue["Global"] is not null)
                    writer.Merge(rawValue["Global"]![targetType.ToString()]);
                if (rawValue[platform.ID] is not null)
                    writer.Merge(rawValue[platform.ID]![targetType.ToString()]);

                await _database.WriteJsonIntoData(writer, uuid, cancellationToken: cancellationToken);
                await _database.Client.CommitTranAsync();

                UUID = uuid;
                if (fatherUUID is not null)
                    FatherUUID = fatherUUID;
                return uuid;
            }
            catch (Exception e)
            {
                await _database.Client.RollbackTranAsync();
                Log.Error(e);
                return null;
            }
        }

        /// <summary>
        /// 删除对象(删除标志)
        /// </summary>
        /// <returns></returns>
        public async Task<bool?> RemoveTarget(string? uuid = null)
        {
            if (uuid is null)
                if (UUID is not null)
                    uuid = UUID;
                else
                    return null;

            try
            {
                ISugarQueryable<DbID> query = _database.Client.Queryable<DbID>()
                .Where(a => a.UUID == uuid);

                if (!await query.AnyAsync())
                    return false;

                await _database.Client.Updateable<DbID>().UpdateColumns(a => a.Type == TargetType.Remove).ExecuteCommandAsync();
                return true;
            }
            catch (Exception e)
            {
                Log.Error(e);
                return false;
            }
        }

        /// <summary>
        /// 平台映射寻找对象(初始化)
        /// </summary>
        /// <param groupsName="id">平台的唯一ID</param>
        /// <returns></returns>
        public async Task<DbPlatformID?> FindTarget(string id, IPlatform? platform = null)
        {
            platform ??= Platform;

            try
            {
                ISugarQueryable<DbPlatformID> query = _database.Client.Queryable<DbPlatformID>()
                .Where(a => a.ID == id && a.Platform == Platform.ID);

                int count = await query.CountAsync();

                if (count > 1)
                {
                    Log.Error("尝试寻找平台映射对象发现重复数据 id: {0} platformID: {1}", id, Platform.ID);
                    return null;
                }
                else if (count < 1)
                    return DbPlatformID.Empty;

                DbPlatformID value = await query.SingleAsync();

                UUID = value.UUID;
                if (value.FatherUUID is not null)
                    FatherUUID = value.FatherUUID;

                return value;
            }
            catch (Exception e)
            {
                Log.Error(e);
                return null;
            }
        }

        /// <summary>
        /// 查询对象平台映射
        /// </summary>
        /// <param groupsName="uuid">默认为当前对象UUID</param>
        /// <returns></returns>
        public async Task<DbPlatformID?> GetPlatformID(string? uuid = null)
        {
            if (uuid is null)
                if (UUID is not null)
                    uuid = UUID;
                else
                    return null;

            try
            {
                ISugarQueryable<DbPlatformID> query = _database.Client.Queryable<DbPlatformID>()
                .Where(a => a.UUID == uuid && a.Platform == Platform.ID);

                if (!await query.AnyAsync())
                    return DbPlatformID.Empty;

                return await query.SingleAsync();
            }
            catch (Exception e)
            {
                Log.Error(e);
                return null;
            }
        }

        /// <summary>
        /// 查询平台所有对象映射
        /// </summary>
        /// <param groupsName="uuid">默认为当前对象UUID</param>
        /// <returns></returns>
        public async Task<DbPlatformID[]?> GetPlatformIDAllTarget(IPlatform? platform = null)
        {
            platform ??= Platform;

            try
            {
                ISugarQueryable<DbPlatformID> query = _database.Client.Queryable<DbPlatformID>()
                                .Where(a => a.Platform == platform.ID);

                if (!await query.AnyAsync())
                    return [];

                return await query.ToArrayAsync();
            }
            catch (Exception e)
            {
                Log.Error(e);
                return null;
            }
        }

        /// <summary>
        /// 查询对象所有平台映射
        /// </summary>
        /// <param groupsName="uuid">默认为当前对象UUID</param>
        /// <returns></returns>
        public async Task<DbPlatformID[]?> GetAllPlatformID(string? uuid = null)
        {
            if (uuid is null)
                if (UUID is not null)
                    uuid = UUID;
                else
                    return null;

            try
            {
                ISugarQueryable<DbPlatformID> query = _database.Client.Queryable<DbPlatformID>()
                                .Where(a => a.UUID == uuid);

                if (!await query.AnyAsync())
                    return [];

                return await query.ToArrayAsync();
            }
            catch (Exception e)
            {
                Log.Error(e);
                return null;
            }
        }

        /// <summary>
        /// 查询子对象
        /// </summary>
        /// <param groupsName="fatherUUID">默认为当前对象UUID</param>
        /// <returns></returns>
        public async Task<DbPlatformID[]?> FindChild(string? fatherUUID = null)
        {
            if (fatherUUID is null)
                if (UUID is not null)
                    fatherUUID = UUID;
                else
                    return null;

            try
            {
                ISugarQueryable<DbPlatformID> query = _database.Client.Queryable<DbPlatformID>()
                                .Where(a => a.FatherUUID == fatherUUID);

                if (!await query.AnyAsync())
                    return [];

                return await query.ToArrayAsync();
            }
            catch (Exception e)
            {
                Log.Error(e);
                return null;
            }
        }

        /// <summary>
        /// 添加新平台映射
        /// </summary>
        /// <param groupsName="id"></param>
        /// <param groupsName="platform"></param>
        /// <param groupsName="fatherUUID"></param>
        /// <returns></returns>
        public async Task<bool?> AddPlatformID(string id, IPlatform platform, string? fatherUUID = null)
        {
            if (UUID is null)
                return null;

            try
            {
                ISugarQueryable<DbPlatformID> query = _database.Client.Queryable<DbPlatformID>()
                                .Where(a => a.UUID == UUID && a.Platform == platform.ID);

                if (await query.AnyAsync())
                    return false;

                await _database.Client.Insertable(new DbPlatformID
                {
                    UUID = UUID,
                    ID = id,
                    FatherUUID = fatherUUID,
                    Platform = platform.ID,
                }).ExecuteCommandAsync();

                return true;
            }
            catch (Exception e)
            {
                Log.Error(e);
                return false;
            }
        }

        /// <summary>
        /// 删除平台映射
        /// </summary>
        /// <param groupsName="uuid"></param>
        /// <param groupsName="platform"></param>
        /// <returns></returns>
        public async Task<bool?> RemovePlatformID(string? uuid = null, IPlatform? platform = null)
        {
            if (uuid is null)
                if (UUID is not null)
                    uuid = UUID;
                else
                    return null;

            platform ??= Platform;

            try
            {
                ISugarQueryable<DbPlatformID> query = _database.Client.Queryable<DbPlatformID>()
                                .Where(a => a.UUID == UUID && a.Platform == platform.ID);

                if (await query.AnyAsync())
                    return false;

                await _database.Client.Deleteable<DbPlatformID>(a => a.UUID == UUID && a.Platform == platform.ID).ExecuteCommandAsync();
                return true;
            }
            catch (Exception e)
            {
                Log.Error(e);
                return false;
            }
        }

        /// <summary>
        /// 获取权限列表
        /// </summary>
        /// <param groupsName="uuid">默认为当前对象UUID</param>
        /// <returns></returns>
        public async Task<PermissionInfo[]?> GetPerms(string? uuid = null)
        {
            if (uuid is null)
                if (UUID is not null)
                    uuid = UUID;
                else
                    return null;

            try
            {
                ISugarQueryable<DbPerms> query = _database.Client.Queryable<DbPerms>()
                                .Where(a => a.UUID == uuid);

                if (!await query.AnyAsync())
                    return [];

                return (await query.ToListAsync()).Select(x => x.ToPermissionInfo()).ToArray();
            }
            catch (Exception e)
            {
                Log.Error(e);
                return null;
            }
        }

        /// <summary>
        /// 添加权限
        /// </summary>
        /// <param groupsName="perm"></param>
        /// <param groupsName="uuid"></param>
        /// <param groupsName="_a"></param>
        /// <returns></returns>
        public async Task<bool?> AddPerm(PermissionInfo perm, string? uuid = null, bool _a = true)
        {
            if (uuid is null)
                if (UUID is not null)
                    uuid = UUID;
                else
                    return null;

            perm.UUID ??= uuid!;

            try
            {
                ISugarQueryable<DbPerms> query = _database.Client.Queryable<DbPerms>()
                                .Where(a => a.UUID == uuid && a.Permission == perm.Permission && a.Area == perm.Area);

                int count = await query.CountAsync();

                if (count > 1)
                {
                    Log.Debug("尝试读取权限时发现重复数据 uuid: {0} permission: {1} area: {2}", uuid, perm.Permission, perm.Area);
                    return null;
                }

                if (count == 1)
                {
                    DbPerms value = await query.SingleAsync();

                    if ((perm.Expir is not null && value.Expir >= perm.Expir) && value.Value == perm.Value)
                    {
                        Log.Debug("尝试写入权限时发现数据一致 uuid: {0} permission: {1} area: {2} expir: {3} value: {4}", uuid, perm.Permission, perm.Area, perm.Expir, perm.Value);
                        return false;
                    }

                    _database.Client.Updateable<DbPerms>()
                        .WhereColumns(a => value)
                        .UpdateColumns(a => a.Area == perm.Area)
                        .UpdateColumns(a => a.Value == perm.Value)
                        .AddQueue();
                }
                else
                    _database.Client.Insertable(perm.ToDbPerms())
                        .AddQueue();

                if (_a)
                    await _database.Client.SaveQueuesAsync();

                return true;
            }
            catch (Exception e)
            {
                Log.Error(e);
                return null;
            }
        }

        /// <summary>
        /// 添加权限
        /// </summary>
        /// <param groupsName="perms"></param>
        /// <param groupsName="uuid"></param>
        /// <returns></returns>
        public async Task<bool?> AddPerm(ArraySegment<PermissionInfo> perms, string? uuid = null)
        {
            if (uuid is null)
                if (UUID is not null)
                    uuid = UUID;
                else
                    return null;

            try
            {
                await _database.Client.BeginTranAsync();

                bool? result;

                foreach (PermissionInfo perm in perms)
                {
                    result = await AddPerm(perm, uuid, false);
                    if (result != true)
                    {
                        await _database.Client.RollbackTranAsync();
                        return result;
                    }
                }

                await _database.Client.SaveQueuesAsync();

                return true;
            }
            catch (Exception e)
            {
                await _database.Client.RollbackTranAsync();
                Log.Error(e);
                return null;
            }
        }

        /// <summary>
        /// 更新权限
        /// </summary>
        /// <param groupsName="perm"></param>
        /// <param groupsName="uuid"></param>
        /// <param groupsName="_a"></param>
        /// <returns></returns>
        public async Task<bool?> UpdatePerm(PermissionInfo perm, string? uuid = null, bool _a = true)
        {
            if (uuid is null)
                if (UUID is not null)
                    uuid = UUID;
                else
                    return null;

            perm.UUID ??= uuid!;

            try
            {
                ISugarQueryable<DbPerms> query = _database.Client.Queryable<DbPerms>()
                                .Where(a => a.UUID == uuid && a.Permission == perm.Permission && a.Area == perm.Area);

                int count = await query.CountAsync();

                if (count > 1)
                {
                    Log.Debug("尝试读取权限时发现重复数据 uuid: {0} permission: {1} area: {2}", uuid, perm.Permission, perm.Area);
                    return null;
                }
                else if (count < -1)
                    return false;

                DbPerms value = await query.SingleAsync();

                perm.ID = value.ID;

                _database.Client.Updateable<DbPerms>(perm.ToDbPerms())
                    .WhereColumns(a => value)
                    .AddQueue();

                if (_a)
                    await _database.Client.SaveQueuesAsync();

                return true;
            }
            catch (Exception e)
            {
                Log.Error(e);
                return null;
            }
        }

        /// <summary>
        /// 更新权限
        /// </summary>
        /// <param groupsName="perms"></param>
        /// <param groupsName="uuid"></param>
        /// <returns></returns>
        public async Task<bool?> UpdatePerm(ArraySegment<PermissionInfo> perms, string? uuid = null)
        {
            if (uuid is null)
                if (UUID is not null)
                    uuid = UUID;
                else
                    return null;

            try
            {
                await _database.Client.BeginTranAsync();

                bool? result;

                foreach (PermissionInfo perm in perms)
                {
                    result = await UpdatePerm(perm, uuid, false);
                    if (result != true)
                    {
                        await _database.Client.RollbackTranAsync();
                        return result;
                    }
                }

                await _database.Client.SaveQueuesAsync();

                return true;
            }
            catch (Exception e)
            {
                await _database.Client.RollbackTranAsync();
                Log.Error(e);
                return null;
            }
        }

        /// <summary>
        /// 添加权限组
        /// </summary>
        /// <returns></returns>
        public async Task<string?> AddGroups(string groupsName)
        {
            BasePlatform platform = new();

            DbPlatformID? target = await FindTarget(groupsName, platform);

            if (target is null || target == DbPlatformID.Empty)
                return "";

            return await CreateTarget(groupsName, TargetType.Groups, platform);
        }

        /// <summary>
        /// 寻找权限组
        /// </summary>
        /// <returns></returns>
        public async Task<DbPlatformID?> FindGroups(string groupsName) => await FindTarget(groupsName, new BasePlatform());

        /// <summary>
        /// 删除权限组
        /// </summary>
        /// <param groupsName="groupsName"></param>
        /// <returns></returns>
        public async Task<bool?> RemoveGroups(string groupsName)
        {
            DbPlatformID? target = await FindGroups(groupsName);

            if (target is null)
                return null;
            else if (target == DbPlatformID.Empty)
                return false;

            return await RemoveTarget(target.UUID);
        }

        /// <summary>
        /// 取权限组列表
        /// </summary>
        /// <param groupsName="uuid"></param>
        /// <returns></returns>
        public async Task<DbID[]?> GetGroups()
        {
            try
            {
                ISugarQueryable<DbID> query = _database.Client.Queryable<DbID>()
                .Where(a => a.Type == TargetType.Groups);

                if (!await query.AnyAsync())
                    return [];

                return await query.ToArrayAsync();
            }
            catch (Exception e)
            {
                Log.Error(e);
                return null;
            }
        }

        /// <summary>
        /// 取对象继承权限组列表
        /// </summary>
        /// <param groupsName="uuid"></param>
        /// <returns></returns>
        public async Task<PermissionInfo[]?> GetTargetGroups(string? uuid = null, bool includeAll = false)
        {
            if (uuid is null)
                if (UUID is not null)
                    uuid = UUID;
                else
                    return null;

            List<PermissionInfo> groups = [];
            PermissionInfo[]? perms = await GetPerms(uuid);

            if (perms is null)
                return null;

            foreach (var perm in perms)
            {
                if (perm.Permission.StartsWith("Group.") && (includeAll || ComparePerm("Group.*", perm) == true))
                {
                    groups.Add(perm);
                    continue;
                }
            }

            return [.. groups];
        }

        /// <summary>
        /// 判断权限是否匹配
        /// </summary>
        /// <param groupsName="permission"></param>
        /// <param groupsName="perm"></param>
        /// <returns>null 未匹配, true 匹配成功值为true, false 匹配成功值为false</returns>
        private bool? ComparePerm(string permission, PermissionInfo perm)
        {
            //分割文本
            string[] compareText = permission.Split('.');
            string[] comparedText = perm.Permission.Split('.');

            //取最短长度
            int length = compareText.Length > comparedText.Length ? comparedText.Length : compareText.Length;

            //比较文本
            for (int i = 0; i < length; i++)
            {
                if (comparedText[i] == compareText[i])
                    continue;
                else
                {
                    if (compareText[i] == "*" || comparedText[i] == "*")
                        break;
                    else
                        return null;
                }
            }

            //判断场景
            if (perm.Area != "Global")
            {
                if (SourceType is null)
                    return null;
                //不为全局
                if (perm.Area != SourceType.ToString())
                {
                    //判断不相等
                    JToken json;
                    try
                    {
                        json = JToken.Parse(perm.Area);
                    }
                    catch (Exception)
                    {
                        //非Json结构
                        return null;
                    }

                    if (!json.HasValues)
                    {
                        Log.Error("Perms.Area 内容为空 id: {0}", perm.ID);
                        return null;
                    }

                    JToken[] list;

                    if (json.Type == JTokenType.Array)
                    {
                        list = [.. json];
                    }
                    else if (json.Type == JTokenType.Object)
                    {
                        if (json[Platform.ID] is not null)
                        {
                            list = [.. json[Platform.ID]];
                        }
                        else
                        {
                            if (json["Global"] is not null)
                                list = [.. json["Global"]];
                            else
                                return null;
                        }
                    }
                    else
                    {
                        Log.Error("Perms.Area 类型错误 id: {0} jTokenType", perm.ID, JTokenType.Object.ToString());
                        return null;
                    }

                    int index = 0;
                    foreach (var item in list)
                    {
                        if ((string?)item == SourceType.ToString())
                            break;
                        index++;
                    }

                    if (index == list.Length)
                        return null;
                }
            }

            if (perm.Expir > 0 && perm.Expir < new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds())
                return null;

            // 附加条件 perm.Content

            return perm.Value;
        }

        /// <summary>
        /// 判断是否拥有权限
        /// </summary>
        /// <param groupsName="permission"></param>
        /// <param groupsName="uuid"></param>
        /// <param groupsName="bypassLinkTarget"></param>
        /// <returns></returns>
        public async Task<(bool, PermissionInfo?)?> IsHavePerm(string permission, string? uuid = null, bool bypassLinkTarget = false)
        {
            if (uuid is null)
                if (UUID is not null)
                    uuid = UUID;
                else
                    return null;

            bool? result;
            PermissionInfo[]? perms = await GetPerms(uuid);
            List<PermissionInfo> groups = [];

            if (perms is not null && perms.Length > 0)
            {
                foreach (PermissionInfo perm in perms)
                {
                    if (perm.Permission.StartsWith("Group.") && ComparePerm("Group.*", perm) == true)
                    {
                        groups.Add(perm);
                        continue;
                    }

                    result = ComparePerm(permission, perm);

                    if (result is not null)
                        return ((bool)result, perm);
                }
            }
            else if (!bypassLinkTarget && LinkTarget is not null)
                return await LinkTarget.IsHavePerm(permission);

            return null;
        }
    }
}