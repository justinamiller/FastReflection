using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoApp
{
    public sealed class SimpleObject
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }    
        public string State { get; set; }
        public string Zip { get; set; }

        private bool _isPrivate = false;
        private bool IsPrivate
        {
            get { return _isPrivate; }
            set { _isPrivate = value; }
        }

        public static SimpleObject Get()
        {
            var obj = new SimpleObject();
            obj.Name = "John Doe";
            obj.Address = "123 Main St";
            obj.City = "San Diego";
            obj.State = "CA";
            obj.Zip = "92101";
            return obj;
        }
    }
}
