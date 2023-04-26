using SqlSugar;

namespace RoslynCat.SQL
{
    /// <summary>
    /// 表示用于管理 CodeSample 实体的库。
    /// </summary>
    /// <remarks>
    /// 该库使用 ISqlSugarClient 的实例提供 CodeSample 实体的 CRUD 操作。
    /// </remarks>
    public class CodeSampleRepository
    {
        private readonly ISqlSugarClient _db;
        /// <summary>
        /// 使用指定的 ISqlSugarClient 实例初始化 CodeSampleRepository 类的新实例。
        /// </summary>
        /// <param name="db">用于数据库访问的 ISqlSugarClient 实例。</param>
        public CodeSampleRepository(ISqlSugarClient db) {
            _db = db;
            _db.DbMaintenance.CreateDatabase();
            _db.CodeFirst.InitTables(typeof(CodeSample));
        }

        /// <summary>
        /// 从数据库中获取所有 CodeSample 实体。
        /// </summary>
        /// <returns>CodeSample 实体的列表。</returns>
        public async Task<List<CodeSample>> GetAll() {
            return await _db.Queryable<CodeSample>().ToListAsync();
        }

        /// <summary>
        /// 根据 ID 获取单个 CodeSample 实体。
        /// </summary>
        /// <param name="id">要检索的 CodeSample 实体的 ID。</param>
        /// <returns>具有指定 ID 的 CodeSample 实体，如果未找到则返回 null。</returns>
        public async Task<CodeSample> GetById(int id) {
            return await _db.Queryable<CodeSample>().InSingleAsync(id);
        }

        /// <summary>
        /// 向数据库添加新的 CodeSample 实体。
        /// </summary>
        /// <param name="codeSample">要添加的 CodeSample 实体。</param>
        public async Task Add(CodeSample codeSample) {
            await _db.Insertable(codeSample).ExecuteCommandAsync();
        }

        /// <summary>
        /// 在数据库中更新现有的 CodeSample 实体。
        /// </summary>
        /// <param name="codeSample">要更新的 CodeSample 实体。</param>
        /// <returns>更新操作影响的行数。</returns>
        public async Task<int> Update(CodeSample codeSample) {
            return await _db.Updateable(codeSample).ExecuteCommandAsync();
        }

        /// <summary>
        /// 根据 ID 从数据库中删除 CodeSample 实体。
        /// </summary>
        /// <param name="id">要删除的 CodeSample 实体的 ID。</param>
        public async Task Remove(int id) {
            await _db.Deleteable<CodeSample>().In(id).ExecuteCommandAsync();
        }

        /// <summary>
        /// 如果数据库中存在具有指定标题的 CodeSample 实体，则返回 true。
        /// </summary>
        /// <param name="title">要搜索的标题。</param>
        /// <returns>如果找到匹配的 CodeSample 实体，则为 true，否则为 false。</returns>
        public bool HaveTitle(string title) {
            return _db.Queryable<CodeSample>().Any(c => c.Title == title);
        }

        /// <summary>
        /// 从数据库中获取分页的 CodeSample 实体列表。
        /// </summary>
        /// <param name="pageNumber">要检索的页码。</param>
        /// <param name="pageSize">每页的项数。</param>
        /// <returns>指定页的 CodeSample 实体列表。</returns>
        public async Task<List<CodeSample>> GetPaged(int pageNumber,int pageSize) {
            var totalCount = _db.Queryable<CodeSample>().Count();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            var codeSamples = _db.Queryable<CodeSample>()
                         .OrderBy(c => c.Id)
                         .Skip((pageNumber - 1) * pageSize)
                         .Take(pageSize)
                         .ToList();
            return codeSamples;
        }
    }
}
