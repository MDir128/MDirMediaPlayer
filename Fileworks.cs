using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDirMediaPlayer
{
    internal static class Fileworks
    {
        public static bool ChangeData(string path, string name, string newparam)
        {
            if (File.Exists(path))
            {
                string[] data = File.ReadAllLines(path);
                int i = 0;
                if (newparam != "del")
                {
                    foreach (string line in data)
                    {
                        string[] parts = line.Split('^');
                        if (parts[0] == name)
                        {
                            string newdata = parts[0] + "^" + newparam;
                            data[i] = newdata;
                            File.WriteAllLines(path, data);

                            return true;
                        }
                        i++;
                    }
                }
                else {
                    List<string> newdata = data.ToList<string>();
                    foreach (string line in data) {
                        if (line.Split('^')[0] == name) { 
                            newdata.Remove(line);
                            data = newdata.ToArray();
                            File.WriteAllLines(path, data);
                        }
                    }
                }
            }
            return false;
        }
        public static string RemovePrefix(string input, string prefix)
        {
            return input.StartsWith(prefix) ? input.Substring(prefix.Length) : input;
        }
    }
}
