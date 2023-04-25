using RoslynCat.SQL;
using SqlSugar;
using System.ComponentModel.DataAnnotations;

namespace RoslynCat.Abandon
{
    public class UniqueTitleAttribute : ValidationAttribute
    {
        CodeSampleRepository _resository;
        public UniqueTitleAttribute(CodeSampleRepository sampleRepository) {
            _resository = sampleRepository;
        }
        protected override ValidationResult IsValid(object value, ValidationContext validationContext) {
            if (_resository.HaveTitle(value.ToSqlValue())) {
                return new ValidationResult("Title 已经存在。"); // 返回验证失败的结果
            }
            return ValidationResult.Success; // 返回验证成功的结果
        }
    }
}
