using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuestsTask.Application.BaseModel
{
    public class BaseModel<T> where T : class
    {
        public bool? success { get; set; }
        public string? message { get; set; }
        public ModelStateDictionary? ModelState { get; set; }
        public Exception? Exception { get; set; }
        public string? Token { get; set; }
        public T? Data { get; set; }
        public IEnumerable<T>? DataList { get; set; }
        public int? PageSize { get; set; }
        public int? PageNumber { get; set; }
    }
}
