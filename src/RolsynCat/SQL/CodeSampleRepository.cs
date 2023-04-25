using SqlSugar;

namespace RoslynCat.SQL
{
    public class CodeSampleRepository
    {
        private readonly ISqlSugarClient _db;
        public CodeSampleRepository(ISqlSugarClient db) {
            _db = db;
            _db.DbMaintenance.CreateDatabase();
            _db.CodeFirst.InitTables(typeof(CodeSample));
        }

        public async Task<List<CodeSample>> GetAll() {
            return await _db.Queryable<CodeSample>().ToListAsync();
        }

        public async Task<CodeSample> GetById(int id) {
            return await _db.Queryable<CodeSample>().InSingleAsync(id);
        }

        public async Task Add(CodeSample codeSample) {
            await _db.Insertable(codeSample).ExecuteCommandAsync();
        }

        public async Task<int> Update(CodeSample codeSample) {
           return await _db.Updateable(codeSample).ExecuteCommandAsync();
        }

        public async Task Remove(int id) {
            await _db.Deleteable<CodeSample>().In(id).ExecuteCommandAsync();
        }

        public bool HaveTitle(string title) {
            return _db.Queryable<CodeSample>().Any(c => c.Title == title);
        }

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
