using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TranscribeReadFiles
{
    public class RecordFunctionMap
    {
        public string Name { get; set; }
        public string FunctionName { get; set; }
        public string[] ImageFiles { get; set; }

        public RecordFunctionMap(string name)
        {
            this.Name = name;
            this.FunctionName = name;
            this.ImageFiles = new string[] { name + ".png" };
        }

        public RecordFunctionMap()
        {
        }

        public dynamic Run(string[] paths)
        {
            var dndm = typeof(Transcribe.Transcribe).GetMethod(this.FunctionName,new Type[] { typeof(string[])});
            try
            {
                return (dynamic)dndm.Invoke(null, new object[] { paths });
            }
            catch(TargetInvocationException tie)
            {

            }
            return null;
        }

    }
}
