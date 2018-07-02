using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels.Tcp;
using System.Windows.Forms;
using JJSCommon;

/* Copyright (C) 2017 Rusty Fork @ Twisted Room Escapes
 * 
 * All Rights Reserved
 *
 * Please contact contact@twistedroomescapes.com for any
 * information regarding this source code.
 */


namespace EscapeRoomController
{
    public partial class EscapeRoomController : Form
    {

        static public MyMessage MyMessage = new MyMessage();
        private Timer timeX;
        public int SecondsLeft { get; set; }
        public int SecondsOver { get; set; }
        public int StartSeconds { get; set; }
        public int DefaultSeconds { get; set; }

        private Timer timeY;
        private Timer timeZ;
        private bool ClueUsed_1 { get; set; }
        private bool ClueUsed_2 { get; set; }
        private bool ClueUsed_3 { get; set; }
        public string GamesDirectory { get; set; }
        public string CurrentGameDirectory { get; set; }
        private TreeNode CurrentGameNode { get; set; }
        private string CurrentClueDirectory { get; set; }
        private string CurrentSoundDirectory { get; set; }
        private string CurrentGame { get; set; }
        private Dictionary<string, Dictionary<int, TimeTriggeredSounds>> TimeTriggeredSounds { get; set; }
        private bool GameStarted { get; set; }
        private bool GameEnded { get; set; }
        private bool TimerRunning { get; set; }

        public const string CLUES_DIR = "Clues";
        public const string SOUNDS_DIR = "Sounds";
        public const string VIDEO_DIR = "Video";
        public const string SCREENCAPTURE_DIR = "ScreenCapture";
        public const string GAMELOG_FILENAME = "GameLog.txt";
        public WMPLib.WindowsMediaPlayer WPlayer { get; set; }
        public System.Media.SoundPlayer player { get; set; }



        private string Title { get; set; }

        public WebBrowser WebBrowser
        {
            get
            {
                return this.webBrowser;
            }
            set
            {
                this.webBrowser = value;
            }
        }

        private frmSetup SetupForm {get;set;}

        public EscapeRoomController()
        {
            InitializeComponent();

            Initialize();

            timeX = new Timer() { Interval = 1000 };
            timeX.Tick += new EventHandler(timeX_Tick);

            timeY = new Timer() { Interval = 1000 };
            timeY.Tick += new EventHandler(timeY_Tick);

            timeZ = new Timer() { Interval = 1000 };
            timeZ.Tick += new EventHandler(timeZ_Tick);
            timeZ.Start();

            //this.MaximumSize = new Size(0, 0);
        }


        private void Initialize()
        {
            ckPlayOnStart.Visible = false;

            ToolTip toolTipHide = new System.Windows.Forms.ToolTip();
            toolTipHide.SetToolTip(btnHide, "Hide");

            ToolTip toolTipStart = new System.Windows.Forms.ToolTip();
            toolTipStart.SetToolTip(btnStart, "Start");

            ToolTip toolTipPause = new System.Windows.Forms.ToolTip();
            toolTipPause.SetToolTip(btnPause, "Pause");

            ToolTip toolTipResume = new System.Windows.Forms.ToolTip();
            toolTipResume.SetToolTip(btnResume, "Resume");

            ToolTip toolTipReset = new System.Windows.Forms.ToolTip();
            toolTipReset.SetToolTip(btnReset, "Reset");

            ToolTip toolTipEnd = new System.Windows.Forms.ToolTip();
            toolTipEnd.SetToolTip(btnEnd, "End");

            ToolTip toolTipSet = new System.Windows.Forms.ToolTip();
            toolTipSet.SetToolTip(btnSet, "Set Msg");

            ToolTip toolTipClear = new System.Windows.Forms.ToolTip();
            toolTipClear.SetToolTip(btnClear, "Clear Msg");

            ToolTip toolTipAdd = new System.Windows.Forms.ToolTip();
            toolTipAdd.SetToolTip(btnPlus, "Add Min");

            ToolTip toolTipMinus = new System.Windows.Forms.ToolTip();
            toolTipMinus.SetToolTip(btnMinus, "Subtr Min");

            RegistryHelper.SetTextBoxFromRegistry(txtGame, string.Empty);
            if (!string.IsNullOrWhiteSpace(txtGame.Text))
            {
                CurrentGame = txtGame.Text;
            }

            // Options
            RegistryHelper.SetCheckBoxFromRegistry(ckPlayOnStart, false);
            
            GameStarted = false;
            GameEnded = false;
            TimerRunning = false;
            SecondsLeft = 0;
            SecondsOver = 0;
            StartSeconds = 3601;
            DefaultSeconds = 3601;

            lblStartTime.Text = string.Empty;
            lblCurrentDate.Text = DateTime.Now.ToString("M/d/yyyy");
            lblCurrentTime.Text = DateTime.Now.ToString("hh:mm:ss tt");

            Title = this.Text;

            lblTimeOver.Visible = false;
            
            MyMessage.TimeRemaining = lblTimeRemaining.Text;

            ClueUsed_1 = false;
            ClueUsed_2 = false;
            ClueUsed_3 = false;

            WPlayer = new WMPLib.WindowsMediaPlayer();
            TimeTriggeredSounds = new Dictionary<string, Dictionary<int, TimeTriggeredSounds>>(1);

            GamesDirectory = ConfigurationManager.AppSettings["GamesDirectory"]; // Root folder

            //var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
            //var image = Path.Combine(directory, @"\images\Missing Maestro-thumb.jpg");

            //ImageList myImageList = new ImageList();
            //myImageList.Images.Add(Image.FromFile(image));

            var gameDir1 = GamesDirectory;
            var gameDir2 = GamesDirectory;
            var gameDir3 = GamesDirectory;

            if (!Directory.Exists(GamesDirectory))
            {
                GamesDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + GamesDirectory;

                if (!Directory.Exists(GamesDirectory))
                {
                    gameDir2 = GamesDirectory;

                    GamesDirectory = GamesDirectory.Replace(@"\Dropbox", @"\Documents\Dropbox");
                }
            }

            if (!Directory.Exists(GamesDirectory))
            {
                gameDir3  = GamesDirectory;
                MessageBox.Show(string.Format("Can't find game directory {0} or {1} or {2}.", gameDir1, gameDir2, gameDir3), "Message");
                return;
            }

            string[] icons = Directory.GetFiles(GamesDirectory, "*icon.png");

            ImageList li = new ImageList();
            li.ImageSize = new Size(16, 16);
            li.ColorDepth = ColorDepth.Depth32Bit;

            li.Images.Add(new Bitmap(1, 1));

            if (icons.Any())
            {
                foreach (var i in icons)
                {
                    li.Images.Add(Image.FromFile(i));
                }
            }

            treeViewGames.ImageList = li;

            // Find the game graphic in the top level directory. An image file with the suffix *Game.png and
            // set the image on the main screen. (Default for the application if a game directory one wasn't already set).

            string[] images = Directory.GetFiles(GamesDirectory, "*Game.png");

            if (images.Any())
            {
                try
                {
                    Image gameImg = ControllerHelper.ResizeImage(Image.FromFile(images[0]), gameImage.Size);

                    if (gameImage != null)
                    {
                        gameImage.Image = gameImg;
                    }
                }
                catch (Exception ee)
                {
                    int i = 0;
                }
            }

            // Look for games under the top level games directory, i.e. EscapeRoomGames. Only looking for directories.
            // Directories should contain a certain sub-directory structure, i.e. Clues, Sounds, Video, ScreenCapture
            string[] games = Directory.GetDirectories(GamesDirectory, "*");

            var gameNodes = new List<TreeNode>();

            foreach (var g in games)
            {
                ContextMenuStrip gameMenu = new ContextMenuStrip();
                ToolStripMenuItem gameMenuSetItem = new ToolStripMenuItem();
                gameMenuSetItem.Name = "gameMenuSetItem";
                gameMenuSetItem.Text = "Set";

                ToolStripMenuItem gameMenuEndItem = new ToolStripMenuItem();
                gameMenuEndItem.Name = "gameMenuEndItem";
                gameMenuEndItem.Text = "End";

                ToolStripMenuItem gameMenuPlayItem = new ToolStripMenuItem();
                gameMenuPlayItem.Name = "gameMenuPlayItem";
                gameMenuPlayItem.Text = "Play";

                ToolStripMenuItem gameMenuPauseItem = new ToolStripMenuItem();
                gameMenuPauseItem.Name = "gameMenuPauseItem";
                gameMenuPauseItem.Text = "Pause";

                ToolStripMenuItem gameMenuResumeItem = new ToolStripMenuItem();
                gameMenuResumeItem.Name = "gameMenuResumeItem";
                gameMenuResumeItem.Text = "Resume";

                ToolStripMenuItem gameMenuPlayOnStartUpItem = new ToolStripMenuItem();
                gameMenuPlayOnStartUpItem.Name = "gameMenuPlayOnStartUpItem";
                gameMenuPlayOnStartUpItem.Text = "Play on Start";

                if (ckPlayOnStart.Checked)
                {
                    gameMenuPlayOnStartUpItem.Checked = true;
                }

                gameMenu.Items.AddRange(new ToolStripMenuItem[] { gameMenuSetItem, gameMenuEndItem, gameMenuPlayItem, gameMenuPauseItem, gameMenuResumeItem, gameMenuPlayOnStartUpItem });

                //gameMenus.Add(Path.GetFileName(g), menu); // add to menu dict

                var clueNodes = new List<TreeNode>();
                var soundDirectoryNodes = new List<TreeNode>();

                TreeNode soundTreeNode = null;
                TreeNode soundDirectoryTreeNode = null;

                var clueDirectoryNodes = new List<TreeNode>();

                TreeNode clueTreeNode = null;
                TreeNode gameChildNode = null;

                // Sounds sub-directory
                string soundDirectory = Path.Combine(g, SOUNDS_DIR);
                var soundNodes2 = buildSoundNodes(g, soundDirectory);
                var soundTreeNode2 = new TreeNode("Sounds", soundNodes2.ToArray());
                clueDirectoryNodes.Add(soundTreeNode2);

                string game = Path.GetFileName(g);

                TimeTriggeredSounds.Add(game, new Dictionary<int, TimeTriggeredSounds>(1));

                // Let's load up special sound files for time triggered sounds

                string[] soundTimeDirectories = Directory.GetDirectories(soundDirectory, "*");

                foreach (var std in soundTimeDirectories)
                {
                    var soundTimeFiles = Directory.GetFiles(std, "*.wav");

                   foreach (var f in soundTimeFiles)
                   {
                        var minuteString = Path.GetFileName(std);

                       int minute;

                       if (int.TryParse(minuteString, out minute))
                       {
                           TimeTriggeredSounds tts = new TimeTriggeredSounds(f);

                           // problem when there is more than 1 file in the directory, throws exception

                           if (!TimeTriggeredSounds[game].ContainsKey(minute))
                           {
                               TimeTriggeredSounds[game].Add(minute, tts);
                           }
                       }
                   }
                }


                // Video sub-directory
                string videoDirectory = Path.Combine(g, VIDEO_DIR);
                var videoNodes2 = buildVideoNodes(g, videoDirectory);
                var videoTreeNode2 = new TreeNode("Video", videoNodes2.ToArray());
                clueDirectoryNodes.Add(videoTreeNode2);



                // Clues sub-directory

                string clueDirectory = Path.Combine(g, CLUES_DIR);

                if (Directory.Exists(clueDirectory))
                {
                    string[] clueDirectories = Directory.GetDirectories(clueDirectory, "*");

                    foreach (var cd in clueDirectories)
                    {
                        var dirName = Path.GetFileName(cd);
                 
                        var clueNodes2 = buildClueNodes(g, cd);

                        var clueTreeNode2 = new TreeNode(dirName, clueNodes2.ToArray());

                        clueDirectoryNodes.Add(clueTreeNode2);
                    }

                    gameChildNode = new TreeNode(Path.GetFileName(g), clueDirectoryNodes.ToArray());
                    gameChildNode.Collapse();
                }
  

                gameChildNode.ContextMenuStrip = gameMenu;

                gameMenuSetItem.Click += new EventHandler((sender, e) => gameMenuSetItem_Click(sender, e, gameChildNode, g));
                gameMenuEndItem.Click += new EventHandler((sender, e) => gameMenuEndItem_Click(sender, e));
                gameMenuPlayItem.Click += new EventHandler((sender, e) => gameMenuPlayItem_Click(sender, e, gameChildNode, g));
                gameMenuPauseItem.Click += new EventHandler((sender, e) => gameMenuPauseItem_Click(sender, e, gameChildNode, g));
                gameMenuResumeItem.Click += new EventHandler((sender, e) => gameMenuResumeItem_Click(sender, e, gameChildNode, g));
                gameMenuPlayOnStartUpItem.Click += new EventHandler((sender, e) => gameMenuPlayOnStartUpItem_Click(sender, e, gameChildNode, g));

                gameChildNode.ImageIndex = 1;
                gameChildNode.SelectedImageIndex = 1;


                gameNodes.Add(gameChildNode);

                if (!string.IsNullOrWhiteSpace(CurrentGame))
                {
                    if (CurrentGame.Equals(gameChildNode.Text, StringComparison.CurrentCultureIgnoreCase))
                    {
                        gameMenuSetItem_Click(this, null, gameChildNode, g);
                    }
                }
            }

            var treeNode = new TreeNode("Games", gameNodes.ToArray());
            treeNode.Expand();

            treeViewGames.Nodes.Add(treeNode);

            showGamesToolStripMenuItem.Checked = true;
            showButtonsToolStripMenuItem.Checked = true;
            showToolStripMenuItem.Checked = true;
            hideStatusToolStripMenuItem.Checked = true;
            listMessages.Visible = false;

        }

        /// <summary>
        /// Build the tree nodes of clue files. Look for .html markup files in the clues directory of the current game.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="cd"></param>
        /// <returns></returns>
        public List<TreeNode> buildClueNodes(string g, string cd)
        {
            var clueNodes = new List<TreeNode>();

            var clues = Directory.GetFiles(cd, "*.html");

            foreach (var c in clues)
            {
                ContextMenuStrip clueMenu = new ContextMenuStrip();
                ToolStripMenuItem clueMenuShowItem = new ToolStripMenuItem();
                clueMenuShowItem.Name = "clueMenuShowItem";
                clueMenuShowItem.Text = "Show";

                clueMenu.Items.AddRange(new ToolStripMenuItem[] { clueMenuShowItem, /*clueMenuPlayItem*/ });

                var fileN = Path.GetFileNameWithoutExtension(c);
                /*
                var fileN2 = fileN;
                var hyphen = fileN.IndexOf("-");

                if (hyphen >= 0)
                {
                    fileN2 = fileN.Substring(hyphen + 1);
                }
                */

                var cn = new TreeNode(fileN);

                cn.ContextMenuStrip = clueMenu;

                clueMenuShowItem.Click += new EventHandler((sender, e) => clueMenuShowItem_Click(sender, e, cn, g, c));

                clueNodes.Add(cn);
            }

            return clueNodes;
        }

        /// <summary>
        /// Build the tree nodes of sounds. Look for .wav sound files in the sounds directory of the current game.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        public List<TreeNode> buildSoundNodes(string g, string dir)
        {
            var soundNodes = new List<TreeNode>();

            var clues = Directory.GetFiles(dir, "*.wav");

            foreach (var c in clues)
            {
                ContextMenuStrip soundMenu = new ContextMenuStrip();

                ToolStripMenuItem soundMenuPlayItem = new ToolStripMenuItem();
                soundMenuPlayItem.Name = "soundMenuPlayItem";
                soundMenuPlayItem.Text = "Play";

                soundMenu.Items.AddRange(new ToolStripMenuItem[] { soundMenuPlayItem });

                var fileN = Path.GetFileNameWithoutExtension(c);

                var sn = new TreeNode(fileN);

                sn.ContextMenuStrip = soundMenu;

                soundMenuPlayItem.Click += new EventHandler((sender, e) => soundMenuPlayItem_Click(sender, e, sn, c));

                soundNodes.Add(sn);
            }

            return soundNodes;
        }


        /// <summary>
        /// Build the tree nodes of videos. Look for .mp4 video files in the video directory of the current game.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        public List<TreeNode> buildVideoNodes(string g, string dir)
        {
            var videoNodes = new List<TreeNode>();

            var clues = Directory.GetFiles(dir, "*.mp4");

            foreach (var c in clues)
            {
                ContextMenuStrip videoMenu = new ContextMenuStrip();

                ToolStripMenuItem videoMenuPlayItem = new ToolStripMenuItem();
                videoMenuPlayItem.Name = "videoMenuPlayItem";
                videoMenuPlayItem.Text = "Play";

                videoMenu.Items.AddRange(new ToolStripMenuItem[] { videoMenuPlayItem });

                var fileN = Path.GetFileNameWithoutExtension(c);

                var sn = new TreeNode(fileN);

                sn.ContextMenuStrip = videoMenu;

                videoMenuPlayItem.Click += new EventHandler((sender, e) => videoMenuPlayItem_Click(sender, e, sn, c));

                videoNodes.Add(sn);
            }

            return videoNodes;
        }

        /// <summary>
        /// Set the timer. Used to reset the timer when time needs to be reset for an issue.
        /// </summary>
        /// <param name="time"></param>
        public void SetTimer(TimeSpan time)
        {
            TimeTriggeredSoundsReset();

            StartSeconds = (int)time.TotalSeconds;
            SecondsLeft = (int) time.TotalSeconds;

            lblTimeRemaining.Text = string.Format("{0}:{1}", time.Minutes.ToString("D2"), time.Seconds.ToString("D2"));
        }



        /// <summary>
        /// X timer has triggered. Called every second.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timeX_Tick(object sender, EventArgs e)
        {
            if (SecondsLeft == 0)
            {
                GameStarted = false;
                GameEnded = true;
            }
            else if (SecondsLeft > 0)
            {
                timeX_Down();
            }

            if (SecondsLeft == 0)
            {
                timeX_Up();
            }
        }

        /// <summary>
        /// Increase the down-tick timer when time is still left and the game is in progress. Update the screen labels.
        /// Called every second while the game is in progress.
        /// </summary>
        private void timeX_Down()
        {
            TimerRunning = true;

            lblTimeOver.Visible = false;

            SecondsLeft--;

            var mins = (SecondsLeft / 60);
            var secs = SecondsLeft % 60;

            // Change the color of the time remaining when things are winding down.
            if (mins <= 15)
            {
                lblTimeRemaining.ForeColor = System.Drawing.Color.Red;
            }
            else if (mins >= 60)
            {
                lblTimeRemaining.ForeColor = System.Drawing.Color.GreenYellow;
            }
            else
            {
                lblTimeRemaining.ForeColor = System.Drawing.Color.White;
            }

            lblTimeRemaining.Text = string.Format("{0}:{1}", mins.ToString("D2"), secs.ToString("D2"));
            MyMessage.TimeRemaining = lblTimeRemaining.Text;

            var elapsedSeconds = (60*60) - SecondsLeft;
            var elapsedMins = (elapsedSeconds / 60);
            var elapsedSecs = (elapsedSeconds % 60);
            lblElaspedTime.Text = string.Format("{0}:{1}", elapsedMins.ToString("D2"), elapsedSecs.ToString("D2"));



            // Let's trigger a sound if the Minute has occured. The dictionary of sounds (keyed by minute) should already
            // be loaded. If the minute has "hit" we play the sound from the directory. The sound file in the directory
            // needs to be a .wav file.

            var playSound = true;

           if (TimeTriggeredSounds != null)
           {
               if (TimeTriggeredSounds.ContainsKey(CurrentGame))
               {
                   var tts = TimeTriggeredSounds[CurrentGame];

                   if (tts != null)
                   {
                       if (tts.ContainsKey(mins))
                       {
                           if (!tts[mins].Triggered)
                           {
                               // If this is nearing end of game, let's not play sound until 00:00
                               if (mins == 0)
                               {
                                   playSound = false;

                                   if (SecondsLeft == 0)
                                   {
                                       playSound = true;
                                   }
                               }

                               if (playSound)
                               {
                                    PlaySoundFile(tts[mins].SoundFile);
                                    tts[mins].Triggered = true;
                               }
                           }
                       }
                   }
               }
           }
        }

        /// <summary>
        /// Increase the up-tick timer when time has expired and the game is allowed to continue. Update the screen labels.
        /// Called every second while the game is in progress.
        /// </summary>
        private void timeX_Up()
       {
           TimerRunning = true;

           lblTimeOver.Visible = true;

           SecondsOver++;

           var minsOver = (SecondsOver / 60);
           var secsOver = SecondsOver % 60;

           lblTimeOver.Text = string.Format("+{0}:{1}", minsOver.ToString("D2"), secsOver.ToString("D2"));
            //MyMessage.TimeRemaining = lblTimeRemaining.Text;

            var elapsedSeconds = (60 * 60) + SecondsOver;
            var elapsedMins = (elapsedSeconds / 60);
            var elapsedSecs = (elapsedSeconds % 60);
            lblElaspedTime.Text = string.Format("{0}:{1}", elapsedMins.ToString("D2"), elapsedSecs.ToString("D2"));

        }



        /// <summary>
        /// Make sure the sounds are reset. Called when the timer is reset.
        /// </summary>
        public void TimeTriggeredSoundsReset()
        {
            if (TimeTriggeredSounds != null)
            {
                foreach (var tts in TimeTriggeredSounds)
                {
                    if (tts.Value != null)
                    {
                        foreach (var t in tts.Value)
                        {
                            t.Value.Triggered = false;
                        }
                    }
                }
            }
        }


       private void timeY_Tick(object sender, EventArgs e)
       {
           DelegateHelper.AddMessage(listMessages, false, "Info", "Timer Tic");

           try
           {
               if (RemoteListener != null)
               {
                   DelegateHelper.AddMessage(listMessages, false, "Info", "GetAllData called");
                   var myMessage = RemoteListener.GetAllData();
                   DelegateHelper.AddMessage(listMessages, false, "Info", "GetAllData returned");
                   webBrowser.DocumentText = myMessage.Document;
                   DelegateHelper.SetLabel(lblTimeRemaining, myMessage.TimeRemaining);
               }
           }
           catch (Exception ex)
           {
               timeY.Stop();
               viewRemoteToolStripMenuItem.Checked = false;
               DelegateHelper.AddMessage(listMessages, false, "Error", string.Format("An error occurred when attempting to connect to the remote server - {0}", ex));
           }
       }


       private void timeZ_Tick(object sender, EventArgs e)
       {
           lblCurrentTime.Text = DateTime.Now.ToString("hh:mm:ss tt");
       }


       private void startToolStripMenuControl_Click(object sender, EventArgs e)
       {
           if (TimerRunning)
           {
               if (!timeX.Enabled)
               {
                   resumeToolStripMenuControl_Click(this, null);
               }

               return;
           }

           if (string.IsNullOrWhiteSpace(CurrentGame))
           {
               MessageBox.Show("Please set the game context before starting timer.", "Message");
               return;
           }

           DateTime dt = DateTime.Now;

           TimeTriggeredSoundsReset();
           SecondsLeft = StartSeconds;
           SecondsOver = 0;
           timeX.Start();

           string gameFileDirectory = Path.Combine(CurrentGameDirectory, SCREENCAPTURE_DIR, GAMELOG_FILENAME);
           WriteToGameFile(gameFileDirectory, string.Empty);
           WriteToGameFile(gameFileDirectory, string.Format("[{0}] - Game Started [{1}], Game Host [{2}]", dt.ToString("MM/dd/yyyy"), dt.ToString("hh:mm:ss tt"), txtGameHost.Text));

           lblStartTime.Text = dt.ToString("hh:mm:ss tt");
           GameStarted = true;
           GameEnded = false;
           try
           {
               PlayOnStartUp();
           }
           catch (Exception ee)
           {
           }
       }

       private void pauseToolStripMenuControl_Click(object sender, EventArgs e)
       {
           timeX.Enabled = false;
           lblTimeRemaining.ForeColor = System.Drawing.Color.Yellow;
       }

       private void endToolStripMenuItem_Click(object sender, EventArgs e)
       {
           timeX.Enabled = false;
           TimerRunning = false;

           /*
           if (!lblTimeOver.Visible)
           {
               lblGameOverMessage.Text = "You Escaped!";
               lblGameOverMessage.ForeColor = Color.Green;
           }
           else
           {
               lblGameOverMessage.Text = "Doom!";
               lblGameOverMessage.ForeColor = Color.Red;
           }

           webBrowser.Visible = false;
           lblGameOverMessage.Visible = true;
            */

           gameMenuEndItem_Click(this, null);
       }

       private void resumeToolStripMenuControl_Click(object sender, EventArgs e)
       {
           timeX.Enabled = true;
       }

       private void resetToolStripMenuControl_Click(object sender, EventArgs e)
       {
           TimeTriggeredSoundsReset();
           SecondsLeft = DefaultSeconds;
           SecondsOver = 0;
           timeX_Tick(null, null);
       }

       private void freeFormToolStripMenuItem_Click(object sender, EventArgs e)
       {
           webBrowser.DocumentText = "This is a test";
       }

       private void clearToolStripMenuItem_Click(object sender, EventArgs e)
       {
           webBrowser.DocumentText = string.Empty;
       }

       private void showToolStripMenuItem_Click(object sender, EventArgs e)
       {
           if (showToolStripMenuItem.CheckState == CheckState.Checked)
           {
               webBrowser.Visible = false;
               lblMessages.Visible = false;
               //lblClue.Visible = false;
               showToolStripMenuItem.Checked = false;
           }
           else
           {
               webBrowser.Visible = true;
               lblMessages.Visible = true;
               //lblClue.Visible = true;
               showToolStripMenuItem.Checked = true;
           }
       }

       private void hideToolStripMenuItem_Click(object sender, EventArgs e)
       {
           webBrowser.Visible = false;
       }

       private void SetTitle ()
       {
           //this.Text = string.Format("{0} - {1}", Title, ModeName);
           this.Text = string.Format("{0}", Title);
        }



        /// <summary>
        /// Sets the current game when "Set" menu item from the top tree node is selected. Setting the game
        /// sets context for all of the clues, sounds, videos, etc for the game that is being played.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="node"></param>
        /// <param name="directory"></param>
       private void gameMenuSetItem_Click(object sender, EventArgs e, TreeNode node, string directory)
       {
           CurrentGameDirectory = directory;
           CurrentGameNode = node;
           CurrentClueDirectory = Path.Combine(directory, CLUES_DIR);
           CurrentSoundDirectory = Path.Combine(directory, SOUNDS_DIR);

            if (node != null)
            {
                Title = node.Text;
                CurrentGame = Title;
                txtGame.Text = Title;
                RegistryHelper.SetRegistryFromTextBox(txtGame); // Saves the setting for this computer in the registry

                SetTitle();
                //this.Text = node.Text;

                if (node.Parent != null)
                {
                    foreach (TreeNode n in node.Parent.Nodes)
                    {
                        n.ImageIndex = 1;
                        n.SelectedImageIndex = 1;
                    }
                }

                node.ImageIndex = 3;
                node.SelectedImageIndex = 3;


                // Find the game graphic in the top level directory. An image file with the suffix *Game.png and
                // set the image on the main screen.
                string[] images = Directory.GetFiles(directory, "*Game.png");

                if (images.Any())
                {
                    try
                    {
                        Image gameImg = ControllerHelper.ResizeImage(Image.FromFile(images[0]), gameImage.Size);

                        if (gameImage != null)
                        {
                            gameImage.Image = gameImg;
                        }
                    }
                    catch (Exception ee)
                    {
                        int i = 0;
                    }
                }
            }
       }


        const int ENDGAME = 99;
        /// <summary>
        /// Look for a directory named 99 in the sounds subdirectory and play it. 
        /// The sound file needs to be a .wav file. Only 1 file in the directory.
        /// </summary>
        public void PlayEndGameSound()
        {
            if (TimeTriggeredSounds != null)
            {
                if (TimeTriggeredSounds.ContainsKey(CurrentGame))
                {
                    var tts = TimeTriggeredSounds[CurrentGame];

                    if (tts != null)
                    {
                        if (tts.ContainsKey(ENDGAME))
                        {
                            if (!tts[ENDGAME].Triggered)
                            {
                                PlaySoundFile(tts[ENDGAME].SoundFile);
                                tts[ENDGAME].Triggered = true;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Will take a screen capture and write to the games directory with game name and the date time.
        /// Will log (append) an entry to the gamelog text file to log when the game was started and ended 
        /// and who the game master was.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="node"></param>
        /// <param name="directory"></param>
       private void gameMenuEndItem_Click(object sender, EventArgs e)
       {
           DateTime dt = DateTime.Now;

            try
            {

                PlayEndGameSound(); 

                PauseOnEnd();
                string gameFileDirectory = Path.Combine(CurrentGameDirectory, SCREENCAPTURE_DIR, GAMELOG_FILENAME);
                string underOverMessage = string.Empty;
                if (lblTimeOver.Visible)
                {
                    underOverMessage = string.Format("Over [{0}]", lblTimeOver.Text);
                }
                else
                {
                    underOverMessage = string.Format("Remain [{0}]", lblTimeRemaining.Text);
                }

                WriteToGameFile(gameFileDirectory, string.Format("[{0}] - Game Ended   [{1}], Game Host [{2}], {3}.", dt.ToString("MM/dd/yyyy"), dt.ToString("hh:mm:ss tt"), txtGameHost.Text, underOverMessage));
            }
            catch (Exception ee)
            {
            }

            try
            {
                Rectangle bounds = this.Bounds;
                using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
                {
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        g.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size);
                    }

                    string fileName = string.Format("{0}-{1}.jpg", CurrentGame, dt.ToString("yyyy-MM-dd-HH-mm-ss"));
                    string fullFileName = Path.Combine(CurrentGameDirectory, SCREENCAPTURE_DIR, fileName);

                    bitmap.Save(fullFileName, System.Drawing.Imaging.ImageFormat.Jpeg);

                    MessageBox.Show(string.Format("Game Ended")); //. Screen Capture Taken and saved in file location = {0}", fullFileName), "Message");

                    lblStartTime.Text = string.Empty;
                }
            }
            catch (Exception ex)
            {
            }
       }


  
       private void gameMenuPlayItem_Click(object sender, EventArgs e, TreeNode node, string directory)
       {
           var musicList = Directory.GetFiles(directory, "*.mp3", SearchOption.AllDirectories); //.Union(Directory.GetFiles(directory, "*.wav", SearchOption.AllDirectories));

           var music = musicList.ToArray();
           
           if (music.Any())
           {
               int i = 0;
               foreach (var m in music)
               {
                   WPlayer.URL = music[i];
                   i++;
               }

               //System.Media.SoundPlayer player = new System.Media.SoundPlayer();
      
               WPlayer.controls.play();
               //player.SoundLocation = music[0];
               //player.LoadAsync();
               //player.PlayLooping();
           }
           else
           {
               MessageBox.Show("No .mp3 music files found.", "Message");
           }
       }

       private void gameMenuPlayOnStartUpItem_Click(object sender, EventArgs e, TreeNode node, string directory)
       {
           ToolStripMenuItem menuItem = sender as ToolStripMenuItem;

           if (menuItem.CheckState == CheckState.Checked)
           {
               menuItem.Checked = false;
           }
           else
           {
               menuItem.Checked = true;
           }

           ckPlayOnStart.Checked = menuItem.Checked;
           RegistryHelper.SetRegistryFromCheckBox(ckPlayOnStart);

           // need to use RegistryHelper
           //RegistryHelper

           this.Focus();

       }
       
       /// <summary>
       /// See if we are suppose to play the "game" music on start of the game.
       /// </summary>
       private void PlayOnStartUp()
       {
           bool play = false;

           if (CurrentGameNode.ContextMenuStrip.Items.ContainsKey("gameMenuPlayOnStartUpItem"))
           {
               ToolStripMenuItem menuItem = CurrentGameNode.ContextMenuStrip.Items["gameMenuPlayOnStartUpItem"] as ToolStripMenuItem;

               play = menuItem.Checked;
           }

           if (play)
           {
               gameMenuPlayItem_Click(this, null, CurrentGameNode, CurrentGameDirectory);
           }
       }

       private void PauseOnEnd()
       {
            gameMenuPauseItem_Click(this, null, CurrentGameNode, CurrentGameDirectory);
       }


       private void gameMenuPauseItem_Click(object sender, EventArgs e, TreeNode node, string directory)
       {
           if (!string.IsNullOrWhiteSpace(WPlayer.URL))
           {
               WPlayer.controls.pause();
           }
       }

       private void gameMenuResumeItem_Click(object sender, EventArgs e, TreeNode node, string directory)
       {
           if (!string.IsNullOrWhiteSpace(WPlayer.URL))
           {
               WPlayer.controls.play();
           }
       }

       private void clueMenuShowItem_Click(object sender, EventArgs e, TreeNode node, string directory, string file)
       {
           axWindowsMediaPlayer.Visible = false;

           showToolStripMenuItem.Checked = false;

           showToolStripMenuItem_Click(null, null);
    
           if (node != null)
           {
               // Need to read the contents of the file and display

               string readText = File.ReadAllText(file);

               var dir = Path.GetDirectoryName(file);
               //Uri uri = new Uri(dir);

               readText = readText.Replace("{$Base}", dir);
               //webBrowser.Navigate(uri);

               
               //webBrowser.Navigate(new Uri(file));

               webBrowser.DocumentText = readText;

               MyMessage.Document = webBrowser.DocumentText;

               //lblClue.Text = Path.GetFileNameWithoutExtension(file);
               //lblClue.Visible = true;

               clueMenuPlayItem_Click(sender, e, node, Path.GetDirectoryName(file));
           }
       }

       public bool ProcessCommand(string command, out string message)
       {
            message = string.Empty;

            bool rc = false;

            if (string.IsNullOrEmpty(CurrentGameDirectory))
            {
                message = "Can not determine game being played. Please use the 'Set' menu opton for a game";
                return false;
            }

            if (!(command.StartsWith("show", StringComparison.CurrentCultureIgnoreCase) 
                || command.StartsWith("play", StringComparison.CurrentCultureIgnoreCase)
                || command.StartsWith("video", StringComparison.CurrentCultureIgnoreCase)
                )
                )
            {
                message = "command not reconginzed. Try 'show' , 'play' or 'video'";
                return false;
            }

            if (command.StartsWith("show", StringComparison.CurrentCultureIgnoreCase))
            {
                if (command.StartsWith("show ", StringComparison.CurrentCultureIgnoreCase))
                {
                    // Find the .html file
                    string[] tokens = command.Substring(command.IndexOf(' ')).Split('-');

                    if (tokens.Length > 1)
                    {
                        var startingDir = Path.Combine(CurrentGameDirectory, "Clues");
                        var dir = Path.Combine(startingDir, tokens[0].Trim());
                        var file = Path.Combine(dir, tokens[1].Trim() + ".html");

                        if (Directory.Exists(dir))
                        {

                            if (File.Exists(file))
                            {
                                // Display the contents
                                clueMenuShowItem_Click(this, null, new TreeNode(), CurrentGameDirectory, file);
                                rc = true;
                            }
                            else
                            {
                                message = string.Format("HTML file not found in the directory specified. File = {0}.", file);
                            }
                        }
                        else
                        {
                            message = string.Format("Directory not found for this game. Directory = {0}.", dir);
                        }
                    }
                    else
                    {
                        message = "'show' command needs a directory-file parameter. File should not have the .html extention. i.e. 'show 01-01'";
                    }
                }
                else
                {
                    message = "'show' command needs a directory-file parameter. File should not have the .html extention. i.e. 'show 01-01'";
                }
            }


            if (command.StartsWith("play", StringComparison.CurrentCultureIgnoreCase))
            {

                if (command.StartsWith("play ", StringComparison.CurrentCultureIgnoreCase))
                {
                    string token = command.Substring(command.IndexOf(' '));

                    if (!string.IsNullOrWhiteSpace(token))
                    {
                        var startingDir = Path.Combine(CurrentGameDirectory, SOUNDS_DIR);
                        var file = Path.Combine(startingDir, token.Trim() + ".wav");

                        if (File.Exists(file))
                        {
                            // Display the contents
                            soundMenuPlayItem_Click(this, null, new TreeNode(), file);
                            rc = true;
                        }
                        else
                        {
                            message = string.Format("Sound file (.wav) not found. File = {0} not found.", file);
                        }
                    }
                    else
                    {
                        message = "'play' command needs a file parameter without the .wav extension. i.e. 'play sound1'";
                    }
                }
                else
                {
                    message = "'play' command needs a file parameter without the .wav extension. i.e. 'play sound1'";
                }
            }

            if (command.StartsWith("video", StringComparison.CurrentCultureIgnoreCase))
            {

                if (command.StartsWith("video ", StringComparison.CurrentCultureIgnoreCase))
                {
                    string token = command.Substring(command.IndexOf(' '));

                    if (!string.IsNullOrWhiteSpace(token))
                    {
                        var startingDir = Path.Combine(CurrentGameDirectory, VIDEO_DIR);
                        var file = Path.Combine(startingDir, token.Trim() + ".mp4");

                        if (File.Exists(file))
                        {
                            // Display the contents
                            videoMenuPlayItem_Click(this, null, new TreeNode(), file);
                            rc = true;
                        }
                        else
                        {
                            message = string.Format("Sound file (.mp4) not found. File = {0} not found.", file);
                        }
                    }
                    else
                    {
                        message = "'video' command needs a file parameter without the .mp4 extension. i.e. 'video video1'";
                    }
                }
                else
                {
                    message = "'play' command needs a file parameter without the .wav extension. i.e. 'play sound1'";
                }
            }


           return rc;
       }


       private void PlaySoundFile(string file)
       {
            player = new System.Media.SoundPlayer();
            player.SoundLocation = file;
            player.LoadAsync();
            player.Play();
       }


       public void clueMenuPlayItem_Click(object sender, EventArgs e, TreeNode node, string directory)
       {
           player = new System.Media.SoundPlayer();

           var musicList = Directory.GetFiles(directory, "*.wav", SearchOption.AllDirectories);

           var music = musicList.ToArray();

           if (music.Any())
           {
               foreach (var m in music)
               {
                   player.SoundLocation = m;
                   break;
               }
           }
           else
           {
               // Try getting the parent

               var parent = Directory.GetParent(directory);

               if (parent.Exists)
               {

                   musicList = Directory.GetFiles(parent.FullName, "*.wav", SearchOption.AllDirectories);

                   music = musicList.ToArray();

                   if (music.Any())
                   {
                       foreach (var m in music)
                       {
                           player.SoundLocation = m;
                           break;
                       }
                   }
               }
           }


           if (player.SoundLocation != null)
           {
               player.LoadAsync();
               player.Play();
           }
       }


       private void soundMenuPlayItem_Click(object sender, EventArgs e, TreeNode node, string file)
       {
            player = new System.Media.SoundPlayer();
            player.SoundLocation = file;
            player.LoadAsync();
            player.Play();
            //WPlayer.controls.play();
       }

       bool buttonsShowing = false;
       bool treeShowing = false;

       private void videoMenuPlayItem_Click(object sender, EventArgs e, TreeNode node, string file)
       {
           axWindowsMediaPlayer.PlayStateChange += new AxWMPLib._WMPOCXEvents_PlayStateChangeEventHandler(player_PlayStateChange);
           axWindowsMediaPlayer.stretchToFit = true;
           //axWindowsMediaPlayer.Location = new System.Drawing.Point(191, 410);
           axWindowsMediaPlayer.Location = this.Location;
           //axWindowsMediaPlayer.Size = new System.Drawing.Size(773, 400);
           axWindowsMediaPlayer.Size = new System.Drawing.Size(this.Width, this.Height);
           axWindowsMediaPlayer.uiMode = "none";
           axWindowsMediaPlayer.Visible = true;
           axWindowsMediaPlayer.URL = file;

           treeShowing = treeViewGames.Visible;
           buttonsShowing = panelButtons.Visible;

           if (treeShowing)
           {
               treeViewGames.Visible = false;
           }

           if (buttonsShowing)
           {
                showButtons(false);
           }
           axWindowsMediaPlayer.Ctlcontrols.play();
           

       }



 
        private void player_PlayStateChange(object sender, AxWMPLib._WMPOCXEvents_PlayStateChangeEvent e)
        {
            // Test the current state of the player and display a message for each state.
            switch (e.newState)
            {
                case 0:    // Undefined
                    break;

                case 1:    // Stopped
                    break;

                case 2:    // Paused
                    break;

                case 3:    // Playing
                    break;

                case 4:    // ScanForward
                    break;

                case 5:    // ScanReverse
                    break;

                case 6:    // Buffering
                    break;

                case 7:    // Waiting
                    break;

                case 8:    // MediaEnded

                    axWindowsMediaPlayer.Visible = false;
                    if (treeShowing)
                    {
                        treeViewGames.Visible = true;
                    }

                    if (buttonsShowing)
                    {
                        showButtons(true);
                    }
 
                    break;

                case 9:    // Transitioning
                    break;

                case 10:   // Ready
                    break;

                case 11:   // Reconnecting
                    break;

                case 12:   // Last
                    break;

                default:
        
                    break;
            }
        }

       private void pictureClue1_Click(object sender, EventArgs e)
       {
           ClueUsed_1 = !ClueUsed_1;

           if (ClueUsed_1)
           {
               pictureClue1.Image = global::EscapeRoomController.Properties.Resources.iock_icon_ity_bity_x;
           }
           else
           {
               pictureClue1.Image = global::EscapeRoomController.Properties.Resources.iock_icon_ity_bity;
           }
       }

       private void pictureClue3_Click(object sender, EventArgs e)
       {
           ClueUsed_3 = !ClueUsed_3;

           if (ClueUsed_3)
           {
               pictureClue3.Image = global::EscapeRoomController.Properties.Resources.iock_icon_ity_bity_x;
           }
           else
           {
               pictureClue3.Image = global::EscapeRoomController.Properties.Resources.iock_icon_ity_bity;
           }
       }

       private void pictureClue2_Click(object sender, EventArgs e)
       {
           ClueUsed_2 = !ClueUsed_2;

           if (ClueUsed_2)
           {
               pictureClue2.Image = global::EscapeRoomController.Properties.Resources.iock_icon_ity_bity_x;
           }
           else
           {
               pictureClue2.Image = global::EscapeRoomController.Properties.Resources.iock_icon_ity_bity;
           }
       }

       private void roomViewToolStripMenuItem_Click(object sender, EventArgs e)
       {
           // This is the room view, it has several elements hidden from the participant's view

           if (roomViewToolStripMenuItem.CheckState == CheckState.Checked)
           {
               // Need to close connection
               roomViewToolStripMenuItem.Checked = false;
           }
           else
           {
               roomViewToolStripMenuItem.Checked = true;

               ModeName = "Room";
               SetTitle();

               //treeViewGames.Visible = false;

               //listMessages.Visible = false;

               ServerListener();
           }

       }

       private void gameMasterViewToolStripMenuItem_Click(object sender, EventArgs e)
       {
           // This is the game master view, it has all of the elements visible.

           if (gameMasterViewToolStripMenuItem.CheckState == CheckState.Checked)
           {
               // Need to close connection
               gameMasterViewToolStripMenuItem.Checked = false;
           }
           else
           {
               gameMasterViewToolStripMenuItem.Checked = true;

               ModeName = "GameMaster";
               SetTitle();

               treeViewGames.Visible = true;

               ServerListener();
           }
       }

       private void controllerViewToolStripMenuItem_Click(object sender, EventArgs e)
       {
           // This is the controller view,  it has all of the element visible.

           if (controllerViewToolStripMenuItem.CheckState == CheckState.Checked)
           {
               // Need to close connection
               controllerViewToolStripMenuItem.Checked = false;
           }
           else
           {
               controllerViewToolStripMenuItem.Checked = true;

               ModeName = "Controller";
               SetTitle();

               treeViewGames.Visible = true;

               ServerListener();
           }
       }

       private TcpServerChannel TcpServerChan = null;

       internal void ServerListener()
       {
           RemotingSettings settings = new RemotingSettings(ModeName);

           bool Is64Bit = (IntPtr.Size == 8);

           try
           {
               TcpServerChan = new TcpServerChannel(ModeName, settings.RemotingPort);

               try
               {
                   System.Runtime.Remoting.Channels.ChannelServices.RegisterChannel(TcpServerChan, false);

                   DelegateHelper.AddMessage(listMessages, false, "Info", string.Format("{0} - TCP channel opened", ModeName));
               }
               catch (System.Runtime.Remoting.RemotingException)
               {
                   // Ignore - this will occur if the channel is already registered
               }
           }
           catch (Exception e)
           {
               DelegateHelper.AddMessage(listMessages, false, "Info", string.Format("{0} - An error occurred when attempting to open a TCP channel", ModeName));
               //Logger.Error(string.Format("An error occurred when attempting to open a TCP channel"), e);
               throw e;
           }

           // Register the server
           try
           {
               System.Runtime.Remoting.RemotingConfiguration.RegisterWellKnownServiceType(typeof(RemoteListener), settings.RemotingURI, System.Runtime.Remoting.WellKnownObjectMode.SingleCall);

               DelegateHelper.AddMessage(listMessages, false, "Info", string.Format("{0} - Registered the remote server", ModeName));
           }
           catch (Exception e)
           {
               DelegateHelper.AddMessage(listMessages, false, "Info", string.Format("{0} - An error occurred when attempting to register the remote server", ModeName));
               //Logger.Error(string.Format("An error occurred when attempting to register the remote server"), e);
               throw e;
           }
       }


       private string ModeName { get; set; }

       private bool clientPortOpen = false;
       private IRemoteListener RemoteListenerObject {get; set;}
       //private string MachineName = "localhost";
       private string MachineFullName = "localhost";
       private string MachineNameWithProdDomainSuffix = string.Empty;
       private string MachineNameWithStagingDomainSuffix = string.Empty;
       private string MachineNameWithPreviewDomainSuffix = string.Empty;

       private IRemoteListener RemoteListener
       {
           get
           {
               if (RemoteListenerObject == null)
               {
                   RemotingSettings settings = new RemotingSettings("Room");

                   try
                   {
                       OpenClientPort();
                       DelegateHelper.AddMessage(listMessages, false, "Info", string.Format("Opened client port"));
                   }
                   catch (Exception)
                   {
                       DelegateHelper.AddMessage(listMessages, false, "Error", string.Format("Failed to open client port"));

                       throw;
                   }

                   var URL = "tcp://" + settings.MachineName + ":" + settings.RemotingPort + "/" + settings.RemotingURI;


                   DelegateHelper.AddMessage(listMessages, false, "Info", string.Format("Attempting to connect to remote server {0}", URL));
 

                   Exception FirstTryE = null;

                   for (int c = 0; c < 3; c++)
                   {
                       string TestURL = null;
                       if (c == 0)
                       {
                           // Try the plain machine name
                           TestURL = URL;
                       }
                       else if (c == 1)
                       {
                           // See if we have a distinct full name
                           if (MachineFullName == null || MachineFullName.Trim() == string.Empty || settings.MachineName.ToUpper() == MachineFullName.ToUpper())
                           {
                               continue;
                           }

                           TestURL = "tcp://" + this.MachineFullName + ":" + settings.RemotingPort + "/" + settings.RemotingURI;
                       }
                       else
                       {
                           // See if the machine might be in a different domain
                           string MachName = settings.MachineName.ToUpper();
                           if (MachName.IndexOf("XT") > -1)
                           {
                               TestURL = "tcp://" + MachineNameWithProdDomainSuffix + ":" + settings.RemotingPort + "/" + settings.RemotingURI;
                           }
                           else if (MachName.IndexOf("STG") > -1)
                           {
                               TestURL = "tcp://" + MachineNameWithStagingDomainSuffix + ":" + settings.RemotingPort + "/" + settings.RemotingURI;
                           }
                           else if (MachName.IndexOf("PPE") > -1 || MachName.IndexOf("PV") > -1)
                           {
                               TestURL = "tcp://" + MachineNameWithPreviewDomainSuffix + ":" + settings.RemotingPort + "/" + settings.RemotingURI;
                           }
                           else
                           {
                               // If we do not have a machine name that triggers a retry,
                               // throw the original error
                               throw FirstTryE;
                           }
                       }

                       try
                       {
                           RemoteListenerObject = (IRemoteListener) Activator.GetObject(typeof(IRemoteListener), TestURL);
                       }
                       catch (Exception e)
                       {
                           RemoteListenerObject = null;
                           //this.connState = ConnectionState.Failed;

                           throw new Exception("An error occurred when trying to create a proxy for the controller listener.  " + "URL: " + URL, e);
                       }

                       // See if we can open this channel
                       try
                       {
                           string status = RemoteListenerObject.GetStatus();
                           break;
                       }
                       catch (Exception e)
                       {
                           RemoteListenerObject = null;
                           if (c == 0)
                           {
                               FirstTryE = e;
                               continue;
                           }
                           throw;
                       }
                   }
               }

               return RemoteListenerObject;
           }
       }



       private void OpenClientPort()
       {
           DelegateHelper.AddMessage(listMessages, false, "Info", string.Format("Attempting to open client port"));

           if (this.clientPortOpen)
           {
               DelegateHelper.AddMessage(listMessages, false, "Info", string.Format("Client port already open"));
               return;
           }

           System.Collections.IDictionary ChanProps = new Dictionary<string, object>();
           ChanProps.Add("connectionTimeout", 10000);
           ChanProps.Add("timeout", 10000);
           try
           {
               DelegateHelper.AddMessage(listMessages, false, "Info", string.Format("Attempting to register channel"));

               System.Runtime.Remoting.Channels.ChannelServices.RegisterChannel(new TcpClientChannel(ChanProps, null), false);
           }
           catch (System.Runtime.Remoting.RemotingException)
           {
               // Ignore, the channel is already registered
           }
           catch (Exception e)
           {
               throw new Exception("An error occurred attempting to register a client port", e);
           }

           this.clientPortOpen = true;
       }

       private void viewRemoteToolStripMenuItem_Click(object sender, EventArgs e)
       {
           if (viewRemoteToolStripMenuItem.CheckState == CheckState.Checked)
           {
               viewRemoteToolStripMenuItem.Checked = false;
               timeY.Stop();
           }
           else
           {
               viewRemoteToolStripMenuItem.Checked = true;
               timeY.Start();
           }

           DelegateHelper.AddMessage(listMessages, false, "Info", "View Remote Invoked");
       }




       private void setupToolStripMenuItem_Click(object sender, EventArgs e)
       {
           if (SetupForm == null)
           {
               SetupForm = new frmSetup();
           }
           SetupForm.ShowDialog();
           this.Focus();
       }

       private void setToolStripMenuItem_Click(object sender, EventArgs e)
       {
           frmSetTimerDialog timerDlg = new frmSetTimerDialog(this);
           timerDlg.ShowDialog();
           this.Focus();
       }

       private void hideStatusToolStripMenuItem_Click(object sender, EventArgs e)
       {
           if (hideStatusToolStripMenuItem.CheckState == CheckState.Checked)
           {
               listMessages.Visible = true;
               hideStatusToolStripMenuItem.Checked = false;
           }
           else
           {
               listMessages.Visible = false;
               hideStatusToolStripMenuItem.Checked = true;
           }
           this.Focus();
       }

       private void sendToolStripMenuItem_Click(object sender, EventArgs e)
       {
           Form form = new fmrSetClueDialog(this);
           form.ShowDialog();

           // Play sound if message was sent
           //clueMenuPlayItem_Click(sender, e, null, CurrentGameDirectory);

           showToolStripMenuItem.Checked = false;

           showToolStripMenuItem_Click(null, null);

           this.Focus();
       }


       private void showGamesToolStripMenuItem_Click(object sender, EventArgs e)
       {
           if (showGamesToolStripMenuItem.CheckState == CheckState.Checked)
           {
               treeViewGames.Visible = false;
               showGamesToolStripMenuItem.Checked = false;
           }
           else
           {
               treeViewGames.Visible = true;
               showGamesToolStripMenuItem.Checked = true;
           }
           this.Focus();
       }


        /// <summary>
        /// This method intercepts keystorkes
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="keyData"></param>
        /// <returns></returns>
       protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
       {
           if (keyData == (Keys.Control | Keys.S))
           {
               startToolStripMenuControl_Click(this, null); // Start timer
               this.Focus();
               return true;
           }
           else if (keyData == (Keys.Control | Keys.P))  // Pause timer
           {
               pauseToolStripMenuControl_Click(this, null);
               this.Focus();
               return true;
           }
           else if (keyData == (Keys.Control | Keys.O))  // Pause timer
           {
               pauseToolStripMenuControl_Click(this, null);
               this.Focus();
               return true;
           }
           else if (keyData == (Keys.Control | Keys.R))  // Resume timer
           {
               resumeToolStripMenuControl_Click(this, null);
               this.Focus();
               return true;
           }
           else if (keyData == (Keys.Control | Keys.E))  // End game, stop timer
           {
               endToolStripMenuItem_Click(this, null);
               this.Focus();
               return true;
           }
           else if (keyData == (Keys.Control | Keys.T))  //Reset timer
           {
               resetToolStripMenuControl_Click(this, null);
               this.Focus();
               return true;
           }
           else if (keyData == (Keys.Control | Keys.Q))  // Command
           {
               commandToolStripMenuItem_Click(this, null);
               this.Focus();
               return true;
           }
           else if (keyData == (Keys.Control | Keys.C)) // clear clue message
           {
               clearToolStripMenuItem_Click(this, null);
               this.Focus();
               return true;
           }
           else if (keyData == (Keys.Control | Keys.D)) // clear clue message
           {
               clearToolStripMenuItem_Click(this, null);
               this.Focus();
               return true;
           }
           else if (keyData == (Keys.Control | Keys.M)) // set clue message
           {
               sendToolStripMenuItem_Click(this, null);
               this.Focus();
               return true;
           }
           else if (keyData == (Keys.Control | Keys.H)) // hide game tree
           {
               showGamesToolStripMenuItem_Click(this, null);
               this.Focus();
               return true;
           }
           else if (keyData == (Keys.Control | Keys.Oemplus)) // Add 1 minute
           {
               btnPlus_Click(this, null);
               this.Focus();
               return true;
           }
           else if (keyData == (Keys.Control | Keys.OemMinus)) // Substract 1 minnute
           {
               btnMinus_Click(this, null);
               this.Focus();
               return true;
           }
           
           return base.ProcessCmdKey(ref msg, keyData);
           this.Focus();
       }

       private void commandToolStripMenuItem_Click(object sender, EventArgs e)
       {
           frmCommandDiaglog form = new frmCommandDiaglog(this);
           form.ShowDialog();
           this.Focus();
       }


       private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
       {
           frmAboutDialog frm = new frmAboutDialog();
           frm.ShowDialog();
           this.Focus();
       }


       private void helpToolStripMenuItem_Click(object sender, EventArgs e)
       {
           frmHelp frm = new frmHelp(this);
           frm.ShowDialog();
           this.Focus();
       }

       private void btnStart_Click(object sender, EventArgs e)
       {
           startToolStripMenuControl_Click(this, null);
           this.Focus();
       }

       private void btnPause_Click(object sender, EventArgs e)
       {
           pauseToolStripMenuControl_Click(this, null);
           this.Focus();
       }

       private void btnEnd_Click(object sender, EventArgs e)
       {
           endToolStripMenuItem_Click(this, null);
           this.Focus();
       }

       private void btnResume_Click(object sender, EventArgs e)
       {
           resumeToolStripMenuControl_Click(this, null);
           this.Focus();
       }

       private void btnSet_Click(object sender, EventArgs e)
       {
           sendToolStripMenuItem_Click(this, null);
           this.Focus();
       }

       private void btnClear_Click(object sender, EventArgs e)
       {
           clearToolStripMenuItem_Click(this, null);
           this.Focus();
       }

       private void btnHide_Click(object sender, EventArgs e)
       {
           showGamesToolStripMenuItem_Click(this, null);
           this.Focus();
       }

       private void btnReset_Click(object sender, EventArgs e)
       {
           resetToolStripMenuControl_Click(this, null);
           this.Focus();
       }

       private void showButtonsToolStripMenuItem_Click(object sender, EventArgs e)
       {
           if (showButtonsToolStripMenuItem.CheckState == CheckState.Checked)
           {
               showButtons(false);
               showButtonsToolStripMenuItem.Checked = false;
           }
           else
           {
               showButtons(true);
               showButtonsToolStripMenuItem.Checked = true;
           }
           this.Focus();
       }

       private void showButtons(bool visible)
       {
           btnClear.Visible = visible;
           btnEnd.Visible = visible;
           btnHide.Visible = visible;
           btnPause.Visible = visible;
           btnReset.Visible = visible;
           btnResume.Visible = visible;
           btnSet.Visible = visible;
           btnStart.Visible = visible;
           panelButtons.Visible = visible;
       }

       private void btnPlus_Click(object sender, EventArgs e)
       {
           // Add 60 seconds to timer


           if (GameStarted && !GameEnded)
           {
               if (SecondsLeft > 0)
               {
                   pauseToolStripMenuControl_Click(this, null);

                   TimeTriggeredSoundsReset();
                   SecondsLeft += 60;

                   if (SecondsLeft > 3601)
                   {
                       SecondsLeft = 3601;
                   }

                   resumeToolStripMenuControl_Click(this, null);
               }
           }
       }

       private void btnMinus_Click(object sender, EventArgs e)
       {
           // Take 60 seconds from timer

           if (GameStarted && !GameEnded)
           {
               if (SecondsLeft > 60)
               {
                   pauseToolStripMenuControl_Click(this, null);

                   TimeTriggeredSoundsReset();
                   SecondsLeft -= 60;

                   if (SecondsLeft < 1)
                   {
                       SecondsLeft = 1;
                   }

                   resumeToolStripMenuControl_Click(this, null);
               }
           }
       }

       private void label1_Click(object sender, EventArgs e)
       {

       }

       private void txtGameHost_KeyUp(object sender, KeyEventArgs e)
       {
           if ((e.KeyCode == Keys.Enter) || (e.KeyCode == Keys.Return))
           {
               this.SelectNextControl((Control)sender, true, true, true, true);
           }
       }

       private void txtGameHost_Enter(object sender, EventArgs e)
       {
           txtGameHost.SelectAll();
           txtGameHost.Focus();
       }

       

       public void WriteToGameFile(string path, string txt)
       {
           using (StreamWriter sw = File.AppendText(path))
           {
               sw.WriteLine(txt);
           }
       }


    }
}
