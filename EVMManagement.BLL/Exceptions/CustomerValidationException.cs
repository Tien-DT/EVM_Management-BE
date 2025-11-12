using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace EVMManagement.BLL.Exceptions
{
    public class CustomerValidationException : ValidationException
    {
        public IReadOnlyList<string> Errors { get; }

        public CustomerValidationException(IEnumerable<string> errors)
            : base("Thông tin khách hàng không hợp lệ.")
        {
            Errors = errors?.ToList() ?? new List<string>();
        }
    }
}
