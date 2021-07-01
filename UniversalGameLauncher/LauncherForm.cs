using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Windows.Forms;
using System.Windows.Threading;
using System.Xml;


namespace UniversalGameLauncher {
    public partial class Application : Form {
        private string _filename;
        private bool downloaded = false;
        private DownloadProgressTracker _downloadProgressTracker;
        private WebClient _webClient;
        private List<CSV.HashFiles> localHashFiles = new List<CSV.HashFiles>();
        public Version LocalVersion { get { return new Version(Properties.Settings.Default.VersionText); } }

        private List<PatchNoteBlock> patchNoteBlocks = new List<PatchNoteBlock>();

        private bool _isReady;
        public bool IsReady {
            get {
                return _isReady;
            }
            set {
                _isReady = value;
                TogglePlayButton(value);
                InitializeFooter();
            }
        }

        public Application() {
            InitializeComponent();
            int style = NativeWinAPI.GetWindowLong(this.Handle, NativeWinAPI.GWL_EXSTYLE);
            style |= NativeWinAPI.WS_EX_COMPOSITED;
            NativeWinAPI.SetWindowLong(this.Handle, NativeWinAPI.GWL_EXSTYLE, style);
        }

        private void OnLoadApplication(object sender, EventArgs e) {
            InitializeConstantsSettings();
            //InitializeFiles();
            InitializeImages();
            FetchPatchNotes();

            IsReady = false;

            _downloadProgressTracker = new DownloadProgressTracker(50, TimeSpan.FromMilliseconds(500));           
        }

        private void InitializeConstantsSettings() {
            Name = Constants.GAME_TITLE;
            Text = Constants.LAUNCHER_NAME;
            if (Properties.Settings.Default.DestinationPath == "" || Properties.Settings.Default.DestinationPath is null || !Directory.Exists(Properties.Settings.Default.DestinationPath))
            {
                Properties.Settings.Default.DestinationPath = Constants.DESTINATION_PATH;
                Properties.Settings.Default.Save();
            }
            UOVnV_Location_tb.Text = Properties.Settings.Default.DestinationPath;
            SetUpButtonEvents();

            currentVersionLabel.Visible = Constants.SHOW_VERSION_TEXT;

        }

        //private void InitializeFiles() {
        //    if (!Directory.Exists(Constants.DESTINATION_PATH)) {
        //        Directory.CreateDirectory(Constants.DESTINATION_PATH);
        //    } 
        //}
        
        private void InitializeImages() {
            LoadApplicationIcon();
            navbarPanel.BackColor = Color.FromArgb(25, 100, 100, 100); // // Make panel background semi transparent
            logoPictureBox.SizeMode = PictureBoxSizeMode.CenterImage;
            closePictureBox.SizeMode = PictureBoxSizeMode.CenterImage; // Center the X icon
            minimizePictureBox.SizeMode = PictureBoxSizeMode.CenterImage; // Center the - icon
            try {
                //logoPictureBox.Load(Constants.LOGO_URL);
                using(WebClient webClient = new WebClient()) {
                    using (Stream stream = webClient.OpenRead(Constants.BACKGROUND_URL)) {
                        BackgroundImage = Image.FromStream(stream);
                    }
                }
            } catch (Exception e) {
                MessageBox.Show("The launcher was unable to retrieve some game images from the server! " + e, "Error");
            }
        }

        private void LoadApplicationIcon() {
            WebRequest request = (HttpWebRequest)WebRequest.Create(Constants.APPLICATION_ICON_URL);

            Bitmap bm = new Bitmap(32,32); 
            MemoryStream memStream;

            using (Stream response = request.GetResponse().GetResponseStream()) {
                memStream = new MemoryStream();
                byte[] buffer = new byte[1024];
                int byteCount;

                do {
                    byteCount = response.Read(buffer, 0, buffer.Length);
                    memStream.Write(buffer, 0, byteCount);
                } while (byteCount > 0);
            }

            bm = new Bitmap(Image.FromStream(memStream));                 

            if (bm != null) {
                Icon = Icon.FromHandle(bm.GetHicon());
            }

        }

      

        private void InitializeFooter() {
            if (IsReady) {
                updateProgressBar.Visible = false;
                clientReadyLabel.Visible = true;
            } else {
                updateProgressBar.Visible = true;
                clientReadyLabel.Visible= false;
            }
        }
       
        
        private void OnClickPlay(object sender, EventArgs e) {
            if (playButton.Text != "Play")
            { 
                playButton.Enabled = false;
                UOVnV_Location_btn.Enabled = false;               
                DownloadCSV();
                CSV.LoadCSV();
                LocalHash();
                DownloadEachFile();
                playButton.Enabled = true;
                IsReady = true;
                TogglePlayButton(true);
            }
            else LaunchGame();
        }
        private void LocalHash()
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)delegate
                {
                    updateLabelText.Text = "Checking Client Versions";
                });
            }
            else updateLabelText.Text = "Checking Client Versions";
            DirectoryInfo d = new DirectoryInfo($@"{Properties.Settings.Default.DestinationPath}");

            FileInfo[] Files = d.GetFiles("*",SearchOption.AllDirectories);
            foreach (FileInfo file in Files)
            {
                CSV.HashFiles hash;

                hash.filename = $@"{file.FullName.Replace(Properties.Settings.Default.DestinationPath, "")}";
                hash.sha256 = Hashing.GetSHA256(file.FullName);
                localHashFiles.Add(hash);
                if (InvokeRequired)
                {
                    Invoke((MethodInvoker)delegate
                    {
                        updateLabelText.Text = $"Checking Client Versions: {hash.filename}";
                    });
                }
                else updateLabelText.Text = $"Checking Client Versions: {hash.filename}";
                DoEvents();
            }
        }
        public void DoEvents()
        {
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
                new DispatcherOperationCallback(ExitFrame), frame);
            Dispatcher.PushFrame(frame);
        }

        public object ExitFrame(object f)
        {
            ((DispatcherFrame)f).Continue = false;

            return null;
        }
        private void DownloadEachFile()
        {
            for (int i = 0; i < CSV.hashFiles.Count; i++)
            {
                CSV.HashFiles hash = CSV.hashFiles[i];
                if (!localHashFiles.Contains(hash))
                {
                    downloaded = false;
                    _filename = hash.filename;
                    var path = new Uri($"https://uovnv.com/serverfiles{_filename}");
                    var destination = $"{Properties.Settings.Default.DestinationPath}{hash.filename}";
                    
                    DownloadFile(path.ToString(), destination);
                    while (!downloaded)
                    {
                        DoEvents();
                    }
                }

            }
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)delegate
                {
                    updateLabelText.Text = "Download finished - Launching Client";
                });
            }
            else updateLabelText.Text = "Download finished - Launching Client";
        }
        private void DownloadFile(string filename, string filelocation) {
            using (_webClient = new WebClient()) { 
                _webClient.DownloadProgressChanged += OnDownloadProgressChanged;
                _webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(OnDownloadCompleted);
                if (!Directory.Exists(Path.GetDirectoryName(filelocation)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(filelocation));
                }
                _webClient.DownloadFileAsync(new Uri(filename), filelocation);
            }
            
        }
        private void DownloadCSV()
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)delegate
                {
                    updateLabelText.Text = "Retrieving Manifest Files";
                });
            }
            else updateLabelText.Text = "Retrieving Manifest Files";
            using (_webClient = new WebClient())
            {
                //_webClient.DownloadFileAsync(new Uri(Constants.DOWNLOAD_CSV_PATH),Constants.GAME_CSV_PATH);
                _webClient.DownloadFile(new Uri(Constants.DOWNLOAD_CSV_PATH), Constants.GAME_CSV_PATH);
            }
        }

        private void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e) {
            _downloadProgressTracker.SetProgress(e.BytesReceived, e.TotalBytesToReceive);

            if (InvokeRequired)
            {
                Invoke((MethodInvoker)delegate
                {
                    updateProgressBar.Value = e.ProgressPercentage;
                    try
                    {
                        updateLabelText.Text = string.Format("Downloading {3}: {0} of {1} @ {2}", StringUtility.FormatBytes(e.BytesReceived),
                    StringUtility.FormatBytes(e.TotalBytesToReceive), _downloadProgressTracker.GetBytesPerSecondString(),_filename);
                    }
                    catch { }
                });
            }
            else
            {
                updateProgressBar.Value = e.ProgressPercentage;
                try
                {
                    updateLabelText.Text = string.Format("Downloading {3}: {0} of {1} @ {2}", StringUtility.FormatBytes(e.BytesReceived),
                    StringUtility.FormatBytes(e.TotalBytesToReceive), _downloadProgressTracker.GetBytesPerSecondString(),_filename);
                }
                catch { }
            }
        }

        private void OnDownloadCompleted(object sender, AsyncCompletedEventArgs e) {
            _downloadProgressTracker.Reset();
            downloaded = true;
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)delegate
                {
                    updateLabelText.Text = $"{_filename} Download finished";
                });
            }
            else updateLabelText.Text = $"{_filename} Download finished";
            //Extract extract = new Extract(this);
            //extract.Run();
        }

        public void SetLauncherReady() {
            updateLabelText.Text = "";
            if (!File.Exists(Constants.GAME_EXECUTABLE_PATH)) {
                MessageBox.Show("Couldn't make a connection to the game server. Please try again later or inform the developer if the issue persists.", "Fatal error");
                return;
            }

            IsReady = true;

            if (Constants.AUTOMATICALLY_LAUNCH_GAME_AFTER_UPDATING) 
                LaunchGame();           
        }

        private void FetchPatchNotes() {
            try {
                XmlDocument doc = new XmlDocument();
                doc.Load(Constants.PATCH_NOTES_URL);

               foreach(XmlNode node in doc.DocumentElement) {
                    PatchNoteBlock block = new PatchNoteBlock();
                    for(int i = 0; i < node.ChildNodes.Count; i++) {
                        switch(i) {
                            case 0:
                                block.Title = node.ChildNodes[i].InnerText;
                                break;
                            case 1:
                                block.Text = node.ChildNodes[i].InnerText;
                                break;
                            case 2:
                                block.Link = node.ChildNodes[i].InnerText;
                                break;
                        }
                    }
                    patchNoteBlocks.Add(block);
                }
            } catch {
                patchContainerPanel.Visible = false;
                if (Constants.SHOW_ERROR_BOX_IF_PATCH_NOTES_DOWNLOAD_FAILS)
                    MessageBox.Show("The launcher was unable to retrieve patch notes from the server!");
            }

            Label[] patchTitleObjects = { patchTitle1, patchTitle2, patchTitle3 };
            Label[] patchTextObjects = { patchText1, patchText2, patchText3 };

            for(int i = 0; i < patchNoteBlocks.Count; i++) {
                patchTitleObjects[i].Text = patchNoteBlocks[i].Title;
                patchTextObjects[i].Text = patchNoteBlocks[i].Text;
            }
        }

        private void LaunchGame() {
            try {
                Process.Start(Constants.GAME_EXECUTABLE_PATH);
                Environment.Exit(0);
            } catch {
                IsReady = false;
                MessageBox.Show("Couldn't locate the game executable!", "Fatal Error");
            }
        }

        private void TogglePlayButton(bool toggle) {
            switch(toggle) {
                case true:
                    playButton.BackColor = Color.Green;
                    playButton.Text = "Play";
                    break;
                case false:
                    playButton.BackColor = Color.DeepSkyBlue;
                    playButton.Text = "Update";
                    break;
            }
        }
        
        // Move the form with LMB
        private void Application_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                NativeWinAPI.ReleaseCapture();
                NativeWinAPI.SendMessage(Handle, NativeWinAPI.WM_NCLBUTTONDOWN, NativeWinAPI.HT_CAPTION, 0);
            }
        }
        
        private void SetUpButtonEvents() {
            Button[] buttons = { navbarButton1, navbarButton2, navbarButton3};

            for(int i = 0; i < buttons.Length; i++) {
                buttons[i].Click += new EventHandler(OnClickButton);
                buttons[i].Text = Constants.NAVBAR_BUTTON_TEXT_ARRAY[i];
            }
        }

        public void OnClickButton(object sender, EventArgs e) {
            Button button = (Button) sender;
            switch(button.Name) {
                case nameof(navbarButton1):
                    System.Diagnostics.Process.Start(Constants.NAVBAR_BUTTON_1_URL);
                    break;
                case nameof(navbarButton2):
                    System.Diagnostics.Process.Start(Constants.NAVBAR_BUTTON_2_URL);
                    break;
                case nameof(navbarButton3):
                    System.Diagnostics.Process.Start(Constants.NAVBAR_BUTTON_3_URL);
                    break;
                case nameof(navbarButton4):
                    System.Diagnostics.Process.Start(Constants.NAVBAR_BUTTON_4_URL);
                    break;
                case nameof(navbarButton5):
                    System.Diagnostics.Process.Start(Constants.NAVBAR_BUTTON_5_URL);
                    break;

                case nameof(patchButton1):
                    if (patchNoteBlocks.Count > 0 && !(patchNoteBlocks[0].Link is null))
                            Process.Start(patchNoteBlocks[0].Link);                    
                    break;
                case nameof(patchButton2):
                    if (patchNoteBlocks.Count > 1 && !(patchNoteBlocks[1].Link is null))
                        Process.Start(patchNoteBlocks[1].Link);                    
                    break;
                case nameof(patchButton3):
                    if (patchNoteBlocks.Count > 2 && !(patchNoteBlocks[2].Link is null))
                            Process.Start(patchNoteBlocks[2].Link);                    
                    break;
            }
        }

        private void OnMouseEnterIcon(object sender, EventArgs e) {
            var pictureBox = (PictureBox) sender;
            pictureBox.BackColor = Color.FromArgb(50, 255, 255, 255);
        }

        private void OnMouseLeaveIcon(object sender, EventArgs e) {
            var pictureBox = (PictureBox) sender;
            pictureBox.BackColor = Color.FromArgb(0, 255, 255, 255);
        }

        private void minimizePictureBox_Click(object sender, EventArgs e) {
            WindowState = FormWindowState.Minimized;
        }

        private void closePictureBox_Click(object sender, EventArgs e) {
            Environment.Exit(0);
        }

        private void UOVnV_Location_btn_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folder = new FolderBrowserDialog();
            DialogResult r = folder.ShowDialog();
            if (r == DialogResult.OK)
            {
                UOVnV_Location_tb.Text = folder.SelectedPath;
                Properties.Settings.Default.DestinationPath = UOVnV_Location_tb.Text;
                Properties.Settings.Default.Save();
            }
        }
    }
}
