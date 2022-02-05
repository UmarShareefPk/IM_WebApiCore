using System;

namespace IM_Core.Common
{
    public class DbError
    {
        public string Message { get; set; }
        public Exception InnerException { get; set; }
    }
}
