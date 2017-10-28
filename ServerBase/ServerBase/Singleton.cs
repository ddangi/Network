using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerBase
{
    public class Singleton<T>
    {
        protected static readonly Lazy<T> lazy = new Lazy<T>();

        public static T Instance => lazy.Value;
    }
}
