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
        string[] gotextsub;
        List<Tracks> correntSubs;
        List<Tracks> correntAudio;
        bool isFullScreen = false;
        bool isPlaying = true;
        double duration = 0;
        public event EventHandler OnClose;
        bool dodisturb = false;
        ~ Player() {
            Console.WriteLine("I've destroyed");
        }
        public Player(string[] pr, string[] uri, string[] extsubs)
        {
            InitializeComponent();
            this.KeyDown += MainKeyDown;
            this.Width = SystemParameters.PrimaryScreenWidth;
            this.Height = SystemParameters.PrimaryScreenHeight;
            this.Left = 0;
            this.Top = 0;
            goturi = uri;
            param = pr;
            gotextsub = extsubs;
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
            
            player.API.EndFile += (Esender, Eargs) =>
            {
                if (Convert.ToInt32(param[1]) < goturi.Length) { changevid(1); }
                else { player.Stop(); }
            };
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
            
            if (set != "-1")
            {
                player.API.SetPropertyString("sid", set);
                param[5] = set;
            }
            else if (gotextsub[Convert.ToInt16(param[1])-1] != null)
            {
                //Снять комментирование при дебаге. Иначе что-то там крашится
                
                try
                {
                    player.API.Command("sub-remove");
                    
                }
                catch { }
                

                player.API.Command("sub-add", gotextsub[Convert.ToInt16(param[1]) - 1], "select");
                Console.WriteLine(player.API.GetPropertyString("track-list"));
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

            foreach (var track in correntSubs)
            {
                var menuItem = new MenuItem
                {
                    Header = $"{track.title} ({track.lang})",
                    Tag = track.id,
                    IsChecked = track.selected
                };
                menuItem.Click += (s, e) => {
                    var cItem = (MenuItem)s;
                    changeSub(menuItem.Tag.ToString());
                };
                SubMenu.Items.Add(menuItem);
            }
            if (Fileworks.IsArrValid(gotextsub, Convert.ToInt32(param[1])-1) && param[5] != "-1") {
                var menuItem = new MenuItem
                {
                    Header = $"Внешние субтитры"
                };
                menuItem.Click += (s, e) => {
                    changeSub("-1");
                    param[5] = "-1";
                };
                SubMenu.Items.Add(menuItem) ;
            }
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
