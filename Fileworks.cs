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
        public static bool IsArrValid(string[] array) {
            if (array == null || array.Length == 0) return false;
            else if (array[0] == null) return false;
            else return true;
        }
        public static bool IsArrValid(string[] array, int corrent)
        {
            if (array == null || array.Length == 0) return false;
            else if (array[corrent] == null) return false;
            else return true;
        }
        public static string RemovePrefix(string input, string nor)
        {
            //return input.StartsWith(prefix) ? input.Substring(prefix.Length) : input;
            string[] splited_input = input.Split(' ');
            string[] splited_nor = nor.Split(' ');
            string output = "";
            int j = 0;
            for (int i = 0; i < splited_nor.Length; i++) {
                if (splited_nor[i] != splited_input[j])
                {
                    output += splited_input[j];
                    i--;
                }
                j++;
                if (j == splited_input.Length) break;
            }
            return output;
        }
        public static string FindFileWithNum(string dirpath, int num, string ext)
        {
            string filepath = "";
            
            return filepath;
        }
        public static bool IscontainFileWithExtention(string dir, string ext)
        {
            if (Directory.Exists(dir))
            {
                var files = Directory.GetFiles(dir);
                if (files.Length > 0)
                {
                    return files[0].Split('.').Last() == ext;
                }
                return false;
            }
            else return false;
        }
        public static int GetDifInteger(int[] nums, int[] nums2)
        {
            int got = 0;
            for (int i = 0; i <= nums.Length; i++) {
                if (nums[i] != nums2[i])
                {
                    got = i; break;
                }
            }
            return got;
        }
    }
}
