using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Forms = System.Windows.Forms;

public class Preferences {
    public string Video = "";
    public string Music = "";
    public bool Shuffle = false;
    public bool Sound = false;
    public bool VideoSound = false;
}

public static class Program {
    public static readonly string Root = AppDomain.CurrentDomain.BaseDirectory;
    public static readonly string PrefFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DancingBabyModern", "settings.json");
    public static Preferences Pref = new Preferences();
    public static string[] Videos;
    static readonly Random Random = new Random();
    static bool closing;
    static DateTime started;
    static System.Drawing.Point mouse;
    static Window[] windows;
    static MediaElement[] players;
    static DateTime clipStarted;
    static string currentVideo;
    static int failedVideos;
    [DllImport("winmm.dll", CharSet=CharSet.Unicode)]
    static extern int mciSendString(string command, StringBuilder result, int length, IntPtr hwnd);
    [DllImport("user32.dll")] static extern IntPtr SetParent(IntPtr child, IntPtr parent);
    [DllImport("user32.dll")] static extern int GetWindowLong(IntPtr hwnd, int index);
    [DllImport("user32.dll")] static extern int SetWindowLong(IntPtr hwnd, int index, int value);
    [DllImport("user32.dll")] static extern bool GetClientRect(IntPtr hwnd, out RECT rect);
    [StructLayout(LayoutKind.Sequential)] struct RECT { public int Left, Top, Right, Bottom; }

    [STAThread] public static int Main(string[] args) {
        try {
            Videos = Directory.Exists(Path.Combine(Root,"Videos")) ? Directory.GetFiles(Path.Combine(Root,"Videos"),"*.mp4").OrderBy(p=>p).ToArray() : new string[0];
            if (File.Exists(PrefFile)) { try { Pref = new JavaScriptSerializer().Deserialize<Preferences>(File.ReadAllText(PrefFile)) ?? new Preferences(); } catch {} }
            if (Videos.Length == 0) { MessageBox.Show("Keep DancingBaby.scr together with its Videos and Music folders.","Dancing Baby"); return 1; }
            string mode = args.Length == 0 ? "/c" : args[0].ToLowerInvariant();
            if (mode == "/verify") return Verify();
            var app = new Application();
            app.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            if (mode.StartsWith("/p") || mode.StartsWith("-p")) {
                string raw = mode.Contains(":") ? mode.Substring(mode.IndexOf(':')+1) : (args.Length > 1 ? args[1] : "0");
                long handle;
                if (!long.TryParse(raw, out handle) || handle == 0) return 1;
                Preview(new IntPtr(handle));
            } else if (mode == "/s" || mode == "-s") RunSaver();
            else if (mode.StartsWith("/c") || mode.StartsWith("-c") || args.Length == 0) Settings();
            else return 1;
            app.Run();
            return 0;
        } catch(Exception ex) { MessageBox.Show(ex.Message,"Dancing Baby"); return 1; }
    }

    static string PickVideo() {
        if (!Pref.Shuffle && File.Exists(Pref.Video) && Videos.Contains(Pref.Video)) return Pref.Video;
        if (!Pref.Shuffle) return Videos.FirstOrDefault(p=>p.IndexOf("BABYCHA1",StringComparison.OrdinalIgnoreCase)>=0) ?? Videos[0];
        if (Videos.Length < 2) return Videos[0];
        string next; do { next = Videos[Random.Next(Videos.Length)]; } while (next == currentVideo);
        return next;
    }
    static MediaElement MakePlayer(bool preview) {
        return new MediaElement { LoadedBehavior=MediaState.Manual, UnloadedBehavior=MediaState.Close, Stretch=Stretch.Uniform, Volume=preview || !Pref.VideoSound || Pref.Sound ? 0 : 0.6, ScrubbingEnabled=true };
    }
    static void SetClip() {
        currentVideo = PickVideo(); clipStarted = DateTime.UtcNow;
        foreach (var player in players) { player.Stop(); player.Source = new Uri(currentVideo); player.Play(); }
    }
    static void StopAll() {
        if (closing) return; closing=true;
        mciSendString("close babyMusic",null,0,IntPtr.Zero);
        if(players != null) foreach(var p in players) { p.Stop(); p.Close(); }
        Application.Current.Shutdown();
    }
    static void RunSaver() {
        started=DateTime.UtcNow; mouse=Forms.Cursor.Position;
        var screens=Forms.Screen.AllScreens;
        windows=new Window[screens.Length]; players=new MediaElement[screens.Length];
        for(int i=0;i<screens.Length;i++) {
            var b=screens[i].Bounds;
            var p=MakePlayer(false); players[i]=p;
            var w=new Window { WindowStyle=WindowStyle.None, ResizeMode=ResizeMode.NoResize, Background=Brushes.Black, Topmost=true, ShowInTaskbar=false, Content=p, Cursor=System.Windows.Input.Cursors.None };
            windows[i]=w;
            w.SourceInitialized += delegate {
                // Screen coordinates are physical pixels; SetWindowPos handles mixed-DPI desktops.
                var hwnd=new System.Windows.Interop.WindowInteropHelper(w).Handle;
                SetWindowPos(hwnd,new IntPtr(-1),b.X,b.Y,b.Width,b.Height,0x0040);
            };
            w.PreviewKeyDown += delegate { StopAll(); };
            w.PreviewMouseDown += delegate { StopAll(); };
            w.Closed += delegate { StopAll(); };
            p.MediaFailed += delegate {
                if(++failedVideos >= Videos.Length) { StopAll(); return; }
                Application.Current.Dispatcher.BeginInvoke(new Action(SetClip));
            };
            if(i==0) p.MediaEnded += delegate {
                failedVideos=0;
                if(Pref.Shuffle && (DateTime.UtcNow-clipStarted).TotalSeconds>=30) SetClip();
                else foreach(var q in players) { q.Position=TimeSpan.Zero; q.Play(); }
            };
            w.Show();
        }
        SetClip();
        if(Pref.Sound && File.Exists(Pref.Music) && Pref.Music.IndexOf('"')<0) {
            if(mciSendString("open \""+Pref.Music+"\" alias babyMusic",null,0,IntPtr.Zero)==0) mciSendString("play babyMusic from 0",null,0,IntPtr.Zero);
        }
        var timer=new DispatcherTimer { Interval=TimeSpan.FromMilliseconds(200) };
        timer.Tick += delegate {
            var pos=Forms.Cursor.Position;
            if((DateTime.UtcNow-started).TotalSeconds>1.5 && (Math.Abs(pos.X-mouse.X)>5 || Math.Abs(pos.Y-mouse.Y)>5)) { StopAll(); return; }
            var status=new StringBuilder(80);
            if(Pref.Sound && mciSendString("status babyMusic mode",status,status.Capacity,IntPtr.Zero)==0 && status.ToString()=="stopped") mciSendString("play babyMusic from 0",null,0,IntPtr.Zero);
        };
        timer.Start();
    }
    [DllImport("user32.dll")] static extern bool SetWindowPos(IntPtr h, IntPtr after, int x,int y,int cx,int cy,uint flags);
    static void Preview(IntPtr parent) {
        RECT rect; if(!GetClientRect(parent,out rect)) { Application.Current.Shutdown(); return; }
        var player=MakePlayer(true);
        var w=new Window { WindowStyle=WindowStyle.None, ResizeMode=ResizeMode.NoResize, Background=Brushes.Black, ShowInTaskbar=false, Content=player, Width=Math.Max(1,rect.Right), Height=Math.Max(1,rect.Bottom) };
        w.SourceInitialized += delegate {
            var hwnd=new System.Windows.Interop.WindowInteropHelper(w).Handle;
            SetWindowLong(hwnd,-16,(GetWindowLong(hwnd,-16) | 0x40000000) & ~unchecked((int)0x80000000));
            SetParent(hwnd,parent); SetWindowPos(hwnd,IntPtr.Zero,0,0,rect.Right,rect.Bottom,0x0040);
        };
        player.MediaEnded += delegate { player.Position=TimeSpan.Zero; player.Play(); };
        w.Closed += delegate { player.Close(); Application.Current.Shutdown(); };
        w.Show(); player.Source=new Uri(PickVideo()); player.Play();
        var timer=new DispatcherTimer { Interval=TimeSpan.FromSeconds(1) };
        timer.Tick += delegate { RECT r; if(!GetClientRect(parent,out r)) { player.Close(); Application.Current.Shutdown(); } };
        timer.Start();
    }
    static void Save() {
        Directory.CreateDirectory(Path.GetDirectoryName(PrefFile));
        string temp=PrefFile+".tmp"; File.WriteAllText(temp,new JavaScriptSerializer().Serialize(Pref));
        if(File.Exists(PrefFile)) File.Replace(temp,PrefFile,null); else File.Move(temp,PrefFile);
    }
    static void Settings() {
        var w=new Window { Title="Dancing Baby — Settings", Width=670,Height=650,MinWidth=530,MinHeight=570,WindowStartupLocation=WindowStartupLocation.CenterScreen,Background=new SolidColorBrush(Color.FromRgb(24,26,34)),Foreground=Brushes.White };
        var panel=new StackPanel { Margin=new Thickness(24) }; w.Content=new ScrollViewer { Content=panel,VerticalScrollBarVisibility=ScrollBarVisibility.Auto };
        panel.Children.Add(new TextBlock { Text="The Dancing Baby",FontSize=28,FontWeight=FontWeights.Bold });
        panel.Children.Add(new TextBlock { Text="Original 1998 animations · current Windows player",Margin=new Thickness(0,4,0,16) });
        var preview=MakePlayer(true); preview.Height=235; preview.MediaEnded += delegate { preview.Position=TimeSpan.Zero; preview.Play(); }; panel.Children.Add(preview);
        panel.Children.Add(new TextBlock { Text="Animation",Margin=new Thickness(0,12,0,5) });
        var video=new ComboBox { ItemsSource=Videos.Select(Path.GetFileNameWithoutExtension).ToArray(),MaxDropDownHeight=230 };
        video.SelectedIndex=Array.IndexOf(Videos,PickVideo()); if(video.SelectedIndex<0) video.SelectedIndex=0;
        video.SelectionChanged += delegate { preview.Stop(); preview.Source=new Uri(Videos[video.SelectedIndex]); preview.Play(); };
        panel.Children.Add(video);
        var shuffle=new CheckBox { Content="Shuffle animations (change about every 30 seconds)",IsChecked=Pref.Shuffle,Margin=new Thickness(0,12,0,6),Foreground=Brushes.White }; panel.Children.Add(shuffle);
        var audio=new CheckBox { Content="Play selected MIDI / WAV music",IsChecked=Pref.Sound,Margin=new Thickness(0,6,0,6),Foreground=Brushes.White }; panel.Children.Add(audio);
        var tracks=Directory.Exists(Path.Combine(Root,"Music")) ? Directory.GetFiles(Path.Combine(Root,"Music"),"*",SearchOption.AllDirectories).Where(p=>p.EndsWith(".mid",StringComparison.OrdinalIgnoreCase)||p.EndsWith(".wav",StringComparison.OrdinalIgnoreCase)).OrderBy(Path.GetFileName).ToArray() : new string[0];
        var music=new ComboBox { ItemsSource=tracks.Select(p=>Path.GetFileNameWithoutExtension(p)+" — "+new DirectoryInfo(Path.GetDirectoryName(p)).Name).ToArray(),MaxDropDownHeight=220 };
        music.SelectedIndex=Math.Max(0,Array.IndexOf(tracks,Pref.Music)); panel.Children.Add(music);
        var videoAudio=new CheckBox { Content="Play animation's own audio when music is off",IsChecked=Pref.VideoSound,Margin=new Thickness(0,10,0,6),Foreground=Brushes.White }; panel.Children.Add(videoAudio);
        panel.Children.Add(new TextBlock { Text="Mouse movement or any key exits. Windows controls lock-on-resume.",FontSize=12,Margin=new Thickness(0,9,0,10),TextWrapping=TextWrapping.Wrap });
        var buttons=new StackPanel { Orientation=Orientation.Horizontal,HorizontalAlignment=HorizontalAlignment.Right };
        Action capture=delegate { Pref.Video=Videos[video.SelectedIndex]; Pref.Shuffle=shuffle.IsChecked==true; Pref.Sound=audio.IsChecked==true; Pref.VideoSound=videoAudio.IsChecked==true; Pref.Music=music.SelectedIndex>=0 ? tracks[music.SelectedIndex] : ""; Save(); };
        var test=new Button { Content="Preview full screen",Padding=new Thickness(14,7,14,7),Margin=new Thickness(0,0,8,0) };
        test.Click += delegate { capture(); preview.Stop(); System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(System.Reflection.Assembly.GetExecutingAssembly().Location,"/s") { UseShellExecute=false }); };
        var save=new Button { Content="Save and close",Padding=new Thickness(14,7,14,7) }; save.Click += delegate { capture(); w.Close(); };
        buttons.Children.Add(test); buttons.Children.Add(save); panel.Children.Add(buttons);
        w.Closed += delegate { preview.Close(); Application.Current.Shutdown(); };
        w.Show(); preview.Source=new Uri(Videos[video.SelectedIndex]); preview.Play();
    }
    static int Verify() {
        int result=1; string report="";
        var dispatcher=Dispatcher.CurrentDispatcher;
        var player=new MediaPlayer();
        player.Volume=0;
        player.MediaOpened += delegate {
            report="Opened "+PickVideo()+"; "+player.NaturalVideoWidth+"x"+player.NaturalVideoHeight+"; duration "+player.NaturalDuration;
            var frameTimer=new DispatcherTimer { Interval=TimeSpan.FromSeconds(2) };
            frameTimer.Tick += delegate {
                frameTimer.Stop();
                var visual=new DrawingVisual(); using(var dc=visual.RenderOpen()) dc.DrawVideo(player,new Rect(0,0,320,240));
                var bitmap=new System.Windows.Media.Imaging.RenderTargetBitmap(320,240,96,96,PixelFormats.Pbgra32); bitmap.Render(visual);
                var pixels=new byte[320*240*4]; bitmap.CopyPixels(pixels,320*4,0);
                int lit=0; for(int i=0;i<pixels.Length;i+=4) if(pixels[i]+pixels[i+1]+pixels[i+2]>60) lit++;
                result=lit>100 ? 0 : 1; report+="; nonblack pixels "+lit+"; result "+result;
                var encoder=new System.Windows.Media.Imaging.PngBitmapEncoder(); encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmap));
                using(var file=File.Create(Path.Combine(Root,"playback-frame.png"))) encoder.Save(file);
                player.Close(); dispatcher.BeginInvokeShutdown(DispatcherPriority.Normal);
            }; frameTimer.Start();
        };
        player.MediaFailed += delegate(object sender, ExceptionEventArgs e) { report=e.ErrorException.ToString(); player.Close(); dispatcher.BeginInvokeShutdown(DispatcherPriority.Normal); };
        var timer=new DispatcherTimer { Interval=TimeSpan.FromSeconds(15) };
        timer.Tick += delegate { report="Media opening timed out"; player.Close(); dispatcher.BeginInvokeShutdown(DispatcherPriority.Normal); };
        timer.Start(); player.Open(new Uri(PickVideo())); player.Play(); Dispatcher.Run();
        File.WriteAllText(Path.Combine(Root,"playback-check.txt"),report); return result;
    }
}
