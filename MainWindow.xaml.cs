using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using static System.Windows.Forms.LinkLabel;

namespace MDirMediaPlayer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //хранит строики из файла сохранения, использовать для перезаписывания этого файла "sevedser.txt"
        String[] lines;
        Player player;
        List<serial> ser = new List<serial>();
        List<serial> filesrep = new List<serial>();
        List<serial> notfoundser = new List<serial>();
        string[] extsub = new string[0];
        int countfiles = 0;
        bool IsShowNotfound = false;
        // сюда записываются параметры текущей папки (сериала)
        string param = "";
        public MainWindow()
        {
            InitializeComponent();
            StartGrid();
        }
        public void GoHome(Object Sender, RoutedEventArgs e)
        {
            Serials.ItemsSource = null;
            X_Button.Visibility = Visibility.Visible;
            Serials.ItemsSource = ser;
        }
        public void WatchWideoClick(object Sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.ShowDialog();
            string path = dialog.FileName;
            if (path != null && path != "")
            {
                string[] curi = new string[] { path };
                string[] cps = new string[] { "M", "1", "?", "0", "0", "n" };
                StartPlayer(cps, curi);
            }
        }
        public void AddSeries_Click(Object Sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            dialog.ShowDialog();
            string path = dialog.SelectedPath;
            File.AppendAllLines("sevedser.txt", new[] { path });
            Serials.ItemsSource = null;
            ChangeInfofilesRep(path, "S&1&0&?&1&n");
            Serials.ItemsSource = ser;
        }
        public void DeleteSer(Object Sender, RoutedEventArgs e)
        {
            var data = (Sender as System.Windows.Controls.Button).DataContext as serial;
            if (data.isFolder == true) {
                Fileworks.ChangeData("sevedser.txt", data.pth , "del"); }
            StartGrid();
        }
        public void ShowNotFound(Object Sender, RoutedEventArgs e)
        {
            if (IsShowNotfound == false)
            {
                FoundNotfoundButton.Header = "показать найденные сериалы";
                IsShowNotfound = true;
                Serials.ItemsSource = null;
                Serials.ItemsSource = notfoundser;
            }
            else {
                FoundNotfoundButton.Header = "показать не найденные сериалы";
                IsShowNotfound = false;
                Serials.ItemsSource = null;
                Serials.ItemsSource = ser;
            }
        }
        public void StartGrid(string mode ="start")
        {
            Serials.ItemsSource = null;
            ser.Clear();
            X_Button.Visibility = Visibility.Visible;
            lines = File.ReadAllLines("sevedser.txt");
            int i = 0;
            foreach (string line in lines)
            {
                if (line != "")
                {
                    string[] ln = line.Split('^');
                    if (ln.Length > 1)
                    {
                        ChangeInfofilesRep(ln[0], ln[1]);
                    }
                    else
                    {
                        ChangeInfofilesRep(ln[0], "S&1&0&?&1&1");
                        lines[i] = line+ "^S&1&0&?&1&1";
                    }
                }
                i++;
            }
            
            File.WriteAllLines("sevedser.txt", lines);
            Serials.ItemsSource = ser;
        }
        public void ChangeInfofilesRep(string pth, string pm) {
            if (pth.Length>1)
            {
                var matchname = Regex.Match(pth, @"[^\\]+$");
                string name = "none";
                if (matchname.Success)
                {
                    name = matchname.Value;
                }
                if (Directory.Exists(pth))
                {
                    ser.Add(new serial { name = name, pth = pth, isFolder = true, par = pm });
                }
                else
                {
                    notfoundser.Add(new serial { name = name, pth = pth, isFolder = true, par = pm });
                }
            }
        }
        // Здеся датагрид переключаестя из режима сериалов в режим серий и наоборот
        public void Active(object sender, RoutedEventArgs e)    {
            var data = (sender as System.Windows.Controls.Button).DataContext as serial;
            if (data.isFolder)
            {
                var pth = data.pth;
                Serials.ItemsSource = null;
                bool check = GenerateVidColl(pth);
                if (check) {
                    Serials.ItemsSource = filesrep;
                }
                X_Button.Visibility = Visibility.Collapsed;
                param = data.par;
            }
            else {
                string[] paramlocal = param.Split('&');
                paramlocal[1] = (data.name as string).Split(' ')[0];
                paramlocal[2] = "0";
                paramlocal[3] = (data.pth as string).Split('.').Last();
                string[] uri = new string[countfiles];
                int i =0;
                foreach (var part in filesrep)
                {
                    if (part.isFolder == false)
                    {
                        uri[i] = part.pth;
                        i++;
                    }
                }
                StartPlayer(paramlocal , uri);
            }
        }
        // Здеся заполняется список filesrep
        public bool GenerateVidColl(string path) {
            if (!string.IsNullOrEmpty(path))
            {
                if (Directory.Exists(path))
                {
                    countfiles = 0;
                    filesrep = new List<serial>();
                    string[] ext = { ".mp4", ".mkv" };
                    string rexp = @"(?<=^|\D)(\d{1,3})(?=\D|$|\s*\(\d+\))";
                    string sername = path.Split(Convert.ToChar(@"\")).Last();

                    foreach (var file in Directory.GetFiles(path))
                    {
                        
                        string num = Fileworks.RemovePrefix(file.Split(Convert.ToChar(@"\")).Last(), sername);
                        if (ext.Contains(System.IO.Path.GetExtension(file)))
                        {
                            Match match = Regex.Match(num, rexp);
                            if (match.Success)
                            {
                                num = match.Groups[0].Value;
                            }
                            filesrep.Add(new serial { pth = file, name = num+ " Серия", isFolder = false });
                            countfiles++;
                        }
                    }
                    filesrep = filesrep.OrderBy(f =>
                    {
                        string str = f.name.ToString();
                        string numPart = str.Split(' ')[0];
                        return int.TryParse(numPart, out int num) ? num : int.MaxValue;
                    }).ToList();
                    // ищем папку с внешними субтитрами и вводим в массив с адресами субтитров проиндексированные субтитры
                    string rexpsub = @"(?i)sub";
                    extsub = new string[countfiles];
                    foreach (var folder in Directory.GetDirectories(path))
                    {
                        string num = Fileworks.RemovePrefix(folder.Split(Convert.ToChar(@"\")).Last(), sername);
                        {
                            Match match = Regex.Match(num, rexpsub);
                            
                            if (match.Success) { 
                                foreach (var file in Directory.GetFiles(folder))
                                {
                                    string subnum = Fileworks.RemovePrefix(file.Split(Convert.ToChar(@"\")).Last(), sername);
                                    Match matchsub = Regex.Match(subnum, rexp);
                                    if (matchsub.Success)
                                    {
                                        extsub[Convert.ToInt16(matchsub.Groups[0].Value)-1] = file;
                                    }
                                }
                            }
                        }
                        {
                            Match match = Regex.Match(num, rexp);

                            if (match.Success)
                            {
                                num = match.Groups[0].Value;
                                filesrep.Add(new serial { pth = folder, name = num + " сезон", isFolder = true });
                            };
                        }
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        // script which starts player
        public void OpenVideo_Click_Main(Object Sender, RoutedEventArgs e)
        {
            string[] uri = new string[countfiles];
            int i = 0;
            foreach (var part in filesrep) {
                if (part.isFolder == false) {
                    uri[i] = part.pth;
                    i++;
                }
            }
            StartPlayer(param.Split('&'), uri);
        }
        public bool StartPlayer(string[] ps, string[] uri)
        {
            try {
                // вызов окна
                player = new Player(ps, uri, extsub);
                player.Show();
                player.OnClose += destroyPlayer;
                return true;
            }
            catch { return false; };
        }
        public void destroyPlayer(object sender, EventArgs e)
        {
            StartGrid();
            Console.WriteLine("It seems that I was destroyed");
            player = null;
        }
    }
}

class serial
{
    public string pth { get; set; }
    public string name { get; set; }
    public bool isFolder { get; set; }
    public string par {get; set; }
    public string im { get; set; }
}