using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseEnrollment.Interfaces
{
    public interface ISessionService
    {
        public Task Set(string key, string value);
        public Task<string> Get(string key);
        public void Clear(string key);
    }
}
