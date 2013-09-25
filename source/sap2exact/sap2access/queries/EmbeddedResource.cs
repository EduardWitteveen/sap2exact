using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sap2access.queries
{
    public class EmbeddedResource
    {
        private EmbeddedResource()
        {
        }

        public static System.IO.StreamReader GetStream(System.Reflection.Assembly assembly, string name)
        {
            var resources = new StringBuilder();
            foreach (string resName in assembly.GetManifestResourceNames())
            {
                resources.Append("\t" + resName + "\n");
                if (resName.EndsWith(name))
                {
                    return new System.IO.StreamReader(assembly.GetManifestResourceStream(resName));
                }
            }
            throw new Exception("resource: " + name + " not found in:\n" + resources);
        }

        public static string GetString(System.Reflection.Assembly assembly, string name)
        {
            System.IO.StreamReader sr = EmbeddedResource.GetStream(assembly, name);
            string data = sr.ReadToEnd();
            sr.Close();
            return data;
        }

        public static string GetString(string name)
        {
            return EmbeddedResource.GetString(typeof(EmbeddedResource).Assembly, name);
        }
    }
}
