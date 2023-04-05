using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorEcommerce.Shared
{
    public class ServiceResponse<T>
    {
        public T? Data { get; set; }
        public bool Scecess { get; set; } = true;
        public string Message { get; set; }=string.Empty;

        public static implicit operator ServiceResponse<T>(ServiceResponse<List<Product>> v)
        {
            throw new NotImplementedException();
        }
    }
}
