using System.Collections.Generic;

namespace ZedCrestTest.Persistence.DataAccessor
{
    public class SqlModelRes<T>
    {
        public bool Success { get; set; }
        public int ResultInt { get; set; }
        public string ErrorMessage { get; set; }
        public List<T> ResultList { get; set; }
        public T Result { get; set; }

    }
}