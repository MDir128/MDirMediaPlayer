using Mpv.NET.API;
using Mpv.NET.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Text.RegularExpressions;

namespace MDirMediaPlayer
{
    /// <summary>
    /// Логика взаимодействия для Player.xaml
    /// </summary>
    public partial class Player : Window
    {
        private MpvPlayer player;
        string[] param;
        string[] goturi;
        string[] extfolders;
        string[] gotextsub;

        List<Tracks> correntSubs;
        List<Tracks> correntAudio;
        bool isFullScreen = false;
        bool isPlaying = true;
        public double duration = 0;
        public double currentpos = 0;

        public event EventHandler OnClose;
        bool dodisturb = false;
        ~ Player() {
            Console.WriteLine("I've destroyed");
        }
        public Player(string[] pr, string[] uri, string[] folders)
        {
            InitializeComponent();
            this.KeyDown += MainKeyDown;
            this.Width = SystemParameters.PrimaryScreenWidth;
            this.Height = SystemParameters.PrimaryScreenHeight;
            this.Left = 0;
            this.Top = 0;
            goturi = uri;
            param = pr;
            extfolders = folders;
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            player = new MpvPlayer(PlayerHost.Handle)
            {
                Volume = 50,
            };
            player.API.SetPropertyString("hwdec", "auto"); 
            player.API.SetPropertyString("vo", "gpu");
            player.Load(goturi[Convert.ToInt16(param[1]) - 1]);
            player.Resume();
            PlayerHost.MouseUp += Click;
            player.API.FileLoaded += (Lsender, Largs) => {
                duration = player.API.GetPropertyDouble("duration");
                dodisturb = true;
                if (param[3] == "mkv")
                {
                    changeAudio(param[4]);
                    changeSub(param[5]);
                }
                UpdateTracks();
            };
            Console.WriteLine(player.API.GetPropertyString("track-list"));
            Console.WriteLine(extfolders);
            player.API.EndFile += (Esender, Eargs) =>
            {
                if (Convert.ToInt32(param[1]) < goturi.Length) { changevid(1); }
                else { player.Stop(); }
            };
            gotextsub = new string[goturi.Length];
        }
        
        private void JUSTCLOSE()
        {
            if (param[0] == "S")
            {
                string[] namepath = goturi[0].Split(Convert.ToChar(@"\"));
                string name = namepath[0];
                for (int i = 1; i < namepath.Length - 1; i++)
                {
                    name += @"\" + namepath[i];
                }
                string newparams = "";
                int j = 0;
                foreach (string pt in param)
                {
                    if (j == 0)
                    {
                        newparams = newparams + pt;
                    }
                    else
                    {
                        newparams = newparams + "&" + pt;
                    }
                    j++;
                }
                bool check = Fileworks.ChangeData("sevedser.txt", name, newparams);
                Console.WriteLine($"Сохраняем: {newparams}");
            }
        }
        private void WindowOnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            JUSTCLOSE();
            player.API.Command("quit");

            OnClose?.Invoke(sender, e);
        }
        public void changevid(int move)
        {
            if (dodisturb == true)
            {
                dodisturb = false;
                int newindex = Convert.ToInt32(param[1]) + move;
                if ((newindex >= 1) && (newindex <= goturi.Length))
                {
                    param[1] = newindex.ToString();
                    player.Load(goturi[Convert.ToInt32(param[1]) - 1]);
                    player.Resume();
                }
            }
        }
        public void changeAudio(string set)
        {
            player.API.SetPropertyString("aid", set);
            param[4] = set;
            UpdateTracks();
        }
        public void changeSub(string set)
        {
            if (set=="n")
            {
                if (param[5] != "n")
                {
                    if (Convert.ToInt16(param[5]) < 0)
                    {
                        try
                        {
                            player.API.Command("sub-remove");
                            param[5] = set;
                        }
                        catch { }
                    }

                    player.API.SetPropertyString("sid", "no");
                }
            }
            else if (Convert.ToInt16(set) > 0)
            {
                player.API.SetPropertyString("sid", set);
                param[5] = set;
            }
            else if (Convert.ToInt16(set)*(-1) <= extfolders.Length)
            {

                FoundLinkSubOrSound(extfolders[Convert.ToInt16(set) * (-1) - 1], gotextsub);

                if (gotextsub[Convert.ToInt16(param[1]) - 1] != null)
                {
                    try
                    {
                        player.API.Command("sub-remove");

                    }
                    catch { }

                    player.API.Command("sub-add", gotextsub[Convert.ToInt16(param[1]) - 1], "select");
                    param[5] = set;
                    Console.WriteLine(player.API.GetPropertyString("track-list"));
                }
            }
            UpdateTracks();
        }
        
        public void NextVid(Object Sender, RoutedEventArgs e)
        {
            changevid(1);
        }
        public void PrevVid(Object Sender, RoutedEventArgs e)
        {
            changevid(-1);
        }
        private void MenuOpened(object sender, RoutedEventArgs args)
        {
            var menu = sender as ContextMenu;
            var SubMenu = menu.Items.OfType<MenuItem>().FirstOrDefault(i => i.Header?.ToString() == "Субтитры");
            var AudioMenu = menu.Items.OfType<MenuItem>().FirstOrDefault(i => i.Header?.ToString() == "Аудио");


            if (SubMenu != null) SubMenu.Items.Clear();
            else SubMenu = new MenuItem() { Header = "Субтитры", Name = "SubMenu" };
            if (AudioMenu != null) AudioMenu.Items.Clear();
            else AudioMenu = new MenuItem() { Header = "Аудио", Name = "AudioMenu" };

            //перечисляет субтитры в составе контейнера
            foreach (var track in correntSubs)
            {
                var menuItem = new MenuItem
                {
                    Header = $"{track.title} ({track.lang})",
                    Tag = track.id,
                    IsChecked = track.selected
                };
                menuItem.Click += (s, e) => {
                    changeSub(menuItem.Tag.ToString());
                };
                SubMenu.Items.Add(menuItem);
            }
            // перечисляет внешние субтитры
            if (extfolders.Length>0) {
                var menuItem = new MenuItem
                {
                    Header = $"Внешние субтитры"
                };
                int i = 1;
                foreach (string subfol in extfolders) {
                    var subitem = new MenuItem { Header = subfol.Split(Convert.ToChar(@"\")).Last(), Tag = i*(-1)};
                    subitem.Click += (s, e) => {
                        changeSub(subitem.Tag.ToString());
                    };
                    menuItem.Items.Add(subitem);
                    i++;
                }
                SubMenu.Items.Add(menuItem);
            }
            if (param[5] != "n") {
                var menuItem = new MenuItem
                {
                    Header=$"Выключить субтитры"
                };
                menuItem.Click += (s, e) => {
                    changeSub("n");
                    param[5] = "n";
                };
                SubMenu.Items.Add(menuItem);
            }
            // перечисляет аудиотреки в составе контейнера
            foreach (var track in correntAudio)
            {
                var menuItem = new MenuItem
                {
                    Header = $"{track.title} ({track.lang})",
                    Tag = track.id,
                    IsChecked = track.selected
                };
                menuItem.Click += (s, e) => {
                    var cItem = (MenuItem)s;
                    changeAudio(menuItem.Tag.ToString());
                };
                AudioMenu.Items.Add(menuItem);
            }
        }
        public void UpdateTracks()
        {
            List<Tracks> tracks = new List<Tracks>();
            try
            {
                string strJsonTracks = player.API.GetPropertyString("track-list");
                var json = JsonConvert.DeserializeObject<List<Tracks>>(strJsonTracks);
                correntAudio = json.Where(t => t.type == "audio").ToList();
                correntSubs = json.Where(t => t.type == "sub").ToList();
            }
            catch { MessageBox.Show("трек-лист не загрузился!"); }
        }
        public void FoundLinkSubOrSound(string dirpath, string[] links)
        {
            string rexp = @"(?<!-)\b\d{1,3}\b";
            foreach (string sub in Directory.GetFiles(dirpath))
            {
                Match match = Regex.Match(sub, rexp);
                if (match.Success)
                {
                    Console.WriteLine(match.Groups[0].Value);
                    links[Convert.ToInt16(match.Groups[0].Value) - 1] = sub;
                }
            }
        }

        public void Click(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                var menu = this.Resources["AuSubContextMenu"] as ContextMenu;
                menu.Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint;
                menu.IsOpen = true;
            }
        }
        public void  MainKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F)
            {
                if (isFullScreen == false)
                {
                    player.API.SetPropertyString("fullscreen", "yes");
                    this.WindowStyle = WindowStyle.None;
                    this.WindowState = WindowState.Maximized;
                    isFullScreen = true;
                    Console.WriteLine(player.API.GetPropertyString("fullscreen"));
                }
                else
                {
                    player.API.SetPropertyString("fullscreen", "no");
                    this.WindowStyle = WindowStyle.SingleBorderWindow;
                    this.WindowState = WindowState.Normal;
                    isFullScreen = false;
                }
            }
            else if (Keyboard.IsKeyDown(Key.LeftCtrl) && (e.Key == Key.Right)) { changevid(1); }
            else if (Keyboard.IsKeyDown(Key.LeftCtrl) && (e.Key == Key.Left)) { changevid(-1);}
            else if (e.Key == Key.Left && player.IsMediaLoaded)
            {
                if (player.API.GetPropertyDouble("time-pos") - 10 > 0)
                { player.API.Command("seek", "-10"); }
                else player.API.SetPropertyDouble("time-pos", 0);
            }
            else if (e.Key == Key.Right && player.IsMediaLoaded)
            {
                if (player.API.GetPropertyDouble("time-pos") + 10 < duration)
                { player.API.Command("seek", "10"); }
                else player.API.SetPropertyDouble("time-pos", duration);
            }
            else if (e.Key == Key.Space)
            {
                if (isPlaying == true)
                {
                    player.API.SetPropertyString("pause", "yes");
                    isPlaying = false;
                }
                else
                {
                    player.API.SetPropertyString("pause", "no");
                    isPlaying = true;
                }
            }

        }
    }
    public class Tracks
    {
        public string type;
        public long id;
        public string lang;
        public string title;
        public bool selected;
    }
}
