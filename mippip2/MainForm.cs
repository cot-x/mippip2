using System;
using System.Drawing;
using System.Drawing.Text;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Reflection;
using Codeplex.Data;
using Twitter;

namespace mippip2
{
    public class MainForm : Form
    {
        public const string AppName = "mippip2";
        private const string cache = "cache";
        private const string ConsumerKey = "";
        private const string ConsumerSecret = "";
        private const string home_timeline = "http://api.twitter.com/1.1/statuses/home_timeline.json";
        private const string user_timeline = "http://api.twitter.com/1.1/statuses/user_timeline.json";
        private const string mentions = "http://api.twitter.com/1.1/statuses/mentions_timeline.json";
        private const string update = "http://api.twitter.com/1.1/statuses/update.json";
        private const string destroy = "http://api.twitter.com/1.1/statuses/destroy/";
        private const string direct_message_new = "http://api.twitter.com/1.1/direct_messages/new.json";
        private const string direct_messages = "http://api.twitter.com/1.1/direct_messages.json";
        private const string retweet = "http://api.twitter.com/1.1/statuses/retweet/";
        private const string friends = "http://api.twitter.com/1.1/friends/ids.json";
        private const string followers = "http://api.twitter.com/1.1/followers/ids.json";
        private const string lookup = "http://api.twitter.com/1.1/users/lookup.json";
        private const string friend_create = "http://api.twitter.com/1.1/friendships/create.json";
        private const string frined_destroy = "http://api.twitter.com/1.1/friendships/destroy.json";
        private const string blocks_create = "http://api.twitter.com/1.1/blocks/create.json";
        private const string blocks_destroy = "http://api.twitter.com/1.1/blocks/destroy.json";
        private const string search = "https://twitter.com/#!/search/";
        private static ListBox tweetList;
        private Dictionary<string, Person> PersonList = new Dictionary<string, Person>();
        private List<Tweet> newTweet;
        private Dictionary<string, Person> newPerson;
        private Auth auth;
        private Dictionary<string, string> count = new Dictionary<string, string>();
        private Dictionary<string, string> cursor = new Dictionary<string, string>();
        private Timer startTimer;
        private bool isRefleshing = false, isFirst = true, isOK, notException;
        private string userID, screenName, token, tokenSecret, last_time, getScreenName;
        private Timer timer, checkMessageTimer;
        private PrivateFontCollection fontCollection = new PrivateFontCollection();
        private MenuItem[] timelineMenu, messageMenu, friendsMenu, atMenu, userMenu;
        public static Font font { get; private set; }
        
        public MainForm()
        {
            this.Text = AppName;
            this.Icon = new Icon(Assembly.GetExecutingAssembly().GetManifestResourceStream("mippip2.icon.ico"));
            this.Size = new Size(480, 648);
            this.FormBorderStyle = FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.BackgroundImage = Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("mippip2.background.jpg"));
            this.BackgroundImageLayout = ImageLayout.Stretch;

            try
            {
                if (Directory.Exists(cache))
                {
                    string[] old = Directory.GetFiles(cache);
                    foreach (string file in old)
                    {
                        File.Delete(file);
                    }
                    Directory.Delete(cache);
                }
                Directory.CreateDirectory(cache);
            }
            catch
            {
                MessageBox.Show("キャッシュの初期化に失敗しました。", AppName);
                return;
            }

            timelineMenu = new MenuItem[] {
                new MenuItem("リプライ(&R)", new EventHandler(Reply)),
                new MenuItem("コメント付きRT", new EventHandler(ReTweetWithComment)),
                new MenuItem("コメント付きQT", new EventHandler(QTWithComment)),
                new MenuItem("-"),
                new MenuItem("公式リツイート", new EventHandler(ReTweet)),
                new MenuItem("-"),
                new MenuItem("ダイレクトメッセージの送信", new EventHandler(SendDirectMessage)),
                new MenuItem("-"),
                new MenuItem("削除(&D)", new EventHandler(Delete)),
                new MenuItem("-"),
                new MenuItem("URLを開く(&O)", new EventHandler(OpenURL)),
                new MenuItem("Hashタグを検索(&H)", new EventHandler(SearchHash)),
                new MenuItem("@を見る(&A)", new EventHandler(ShowAtUserTimeLine)),
                new MenuItem("-"),
                new MenuItem("リプライ元を探す(&S)", new EventHandler(SearchInReplyTo)),
                new MenuItem("-"),
                new MenuItem("プロフィールを見る(&P)", new EventHandler(ShowUserInformation)),
                new MenuItem("指定してタイムラインを見る", new EventHandler(ShowUserTimeLine)),
                new MenuItem("コピー(&C)", new EventHandler(Copy))
            };

            atMenu = new MenuItem[] {
                new MenuItem("リプライ(&R)", new EventHandler(Reply)),
                new MenuItem("コメント付きRT", new EventHandler(ReTweetWithComment)),
                new MenuItem("コメント付きQT", new EventHandler(QTWithComment)),
                new MenuItem("-"),
                new MenuItem("公式リツイート", new EventHandler(ReTweet)),
                new MenuItem("-"),
                new MenuItem("ダイレクトメッセージの送信", new EventHandler(SendDirectMessage)),
                new MenuItem("-"),
                new MenuItem("URLを開く(&O)", new EventHandler(OpenURL)),
                new MenuItem("Hashタグを検索(&H)", new EventHandler(SearchHash)),
                new MenuItem("@を見る(&A)", new EventHandler(ShowAtUserTimeLine)),
                new MenuItem("-"),
                new MenuItem("プロフィールを見る(&P)", new EventHandler(ShowUserInformation)),
                new MenuItem("指定してタイムラインを見る", new EventHandler(ShowUserTimeLine)),
                new MenuItem("コピー(&C)", new EventHandler(Copy))
            };

            messageMenu = new MenuItem[] {
                new MenuItem("ダイレクトメッセージの送信", new EventHandler(SendDirectMessage)),
                new MenuItem("-"),
                new MenuItem("URLを開く(&O)", new EventHandler(OpenURL)),
                new MenuItem("Hashタグを検索(&H)", new EventHandler(SearchHash)),
                new MenuItem("@を見る(&A)", new EventHandler(ShowAtUserTimeLine)),
                new MenuItem("-"),
                new MenuItem("プロフィールを見る(&P)", new EventHandler(ShowUserInformation)),
                new MenuItem("コピー(&C)", new EventHandler(Copy))
            };

            friendsMenu = new MenuItem[] {
                new MenuItem("ダイレクトメッセージの送信", new EventHandler(SendDirectMessage)),
                new MenuItem("-"),
                new MenuItem("フォローする", new EventHandler(CreateFriend)),
                new MenuItem("指定してフォローする", new EventHandler(NewCreateFriend)),
                new MenuItem("フォローを解除する", new EventHandler(DestroyFriend)),
                new MenuItem("-"),
                new MenuItem("ブロックする", new EventHandler(BlockFriend)),
                new MenuItem("指定してブロックを解除する", new EventHandler(UnBlockFriend)),
                new MenuItem("-"),
                new MenuItem("プロフィールを見る(&P)", new EventHandler(ShowUserInformation)),
                new MenuItem("コピー(&C)", new EventHandler(Copy))
            };

            userMenu = new MenuItem[] {
                new MenuItem("リプライ(&R)", new EventHandler(Reply)),
                new MenuItem("コメント付きRT", new EventHandler(ReTweetWithComment)),
                new MenuItem("コメント付きQT", new EventHandler(QTWithComment)),
                new MenuItem("-"),
                new MenuItem("公式リツイート", new EventHandler(ReTweet)),
                new MenuItem("-"),
                new MenuItem("ダイレクトメッセージの送信", new EventHandler(SendDirectMessage)),
                new MenuItem("-"),
                new MenuItem("URLを開く(&O)", new EventHandler(OpenURL)),
                new MenuItem("Hashタグを検索(&H)", new EventHandler(SearchHash)),
                new MenuItem("@を見る(&A)", new EventHandler(ShowAtUserTimeLine)),
                new MenuItem("-"),
                new MenuItem("プロフィールを見る(&P)", new EventHandler(ShowUserInformation)),
                new MenuItem("指定してタイムラインを見る", new EventHandler(ShowUserTimeLine)),
                new MenuItem("コピー(&C)", new EventHandler(Copy))
            };

            tweetList = new ListBox();
            tweetList.Size = new Size(this.ClientRectangle.Width, this.ClientRectangle.Height);
            tweetList.Location = new Point(3, 3);
            tweetList.ItemHeight = 112;
            tweetList.ScrollAlwaysVisible = true;
            tweetList.DrawMode = DrawMode.OwnerDrawFixed;
            tweetList.BackColor = Color.Green;
            tweetList.DrawItem += new DrawItemEventHandler(tweetList_DrawItem);
            tweetList.MouseMove += new MouseEventHandler(tweetList_MouseMove);  
            tweetList.DoubleClick += new EventHandler(tweetList_DoubleClick);
            tweetList.ContextMenu = new ContextMenu(timelineMenu);
            this.Controls.Add(tweetList);

            Image homeImage = Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("mippip2.home.png"));
            PictureBox pictureHome = new PictureBox();
            pictureHome.Size = homeImage.Size;
            pictureHome.Location = new Point(this.ClientSize.Width / 9 * 1, this.ClientSize.Height - 40);
            pictureHome.BackgroundImage = Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("mippip2.home.jpg"));
            pictureHome.Image = homeImage;
            pictureHome.Click += new EventHandler(pictureHome_Click);
            pictureHome.Cursor = Cursors.Hand;
            this.Controls.Add(pictureHome);

            Image atImage = Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("mippip2.at.png"));
            PictureBox pictureAt = new PictureBox();
            pictureAt.Size = atImage.Size;
            pictureAt.Location = new Point(this.ClientSize.Width / 9 * 2, this.ClientSize.Height - 40);
            pictureAt.BackgroundImage = Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("mippip2.at.jpg"));
            pictureAt.Image = atImage;
            pictureAt.Click += new EventHandler(pictureAt_Click);
            pictureAt.Cursor = Cursors.Hand;
            this.Controls.Add(pictureAt);

            Image tweetImage = Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("mippip2.tweet.png"));
            PictureBox pictureTweet = new PictureBox();
            pictureTweet.Size = tweetImage.Size;
            pictureTweet.Location = new Point(this.ClientSize.Width / 9 * 3, this.ClientSize.Height - 40);
            pictureTweet.BackgroundImage = Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("mippip2.tweet.jpg"));
            pictureTweet.Image = tweetImage;
            pictureTweet.Click += new EventHandler(pictureTweet_Click);
            pictureTweet.Cursor = Cursors.Hand;
            this.Controls.Add(pictureTweet);

            Image refleshImage = Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("mippip2.reflesh.png"));
            PictureBox pictureReflesh = new PictureBox();
            pictureReflesh.Size = refleshImage.Size;
            pictureReflesh.Location = new Point(this.ClientSize.Width / 9 * 4, this.ClientSize.Height - 40);
            pictureReflesh.BackgroundImage = Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("mippip2.reflesh.jpg"));
            pictureReflesh.Image = refleshImage;
            pictureReflesh.Click += new EventHandler(pictureReflesh_Click);
            pictureReflesh.Cursor = Cursors.Hand;
            this.Controls.Add(pictureReflesh);

            Image messageImage = Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("mippip2.message.png"));
            PictureBox pictureMessage = new PictureBox();
            pictureMessage.Size = messageImage.Size;
            pictureMessage.Location = new Point(this.ClientSize.Width / 9 * 5, this.ClientSize.Height - 40);
            pictureMessage.BackgroundImage = Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("mippip2.message.jpg"));
            pictureMessage.Image = messageImage;
            pictureMessage.Click += new EventHandler(pictureMessage_Click);
            pictureMessage.Cursor = Cursors.Hand;
            this.Controls.Add(pictureMessage);

            Image friendsImage = Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("mippip2.friends.png"));
            PictureBox friendsMessage = new PictureBox();
            friendsMessage.Size = friendsImage.Size;
            friendsMessage.Location = new Point(this.ClientSize.Width / 9 * 6, this.ClientSize.Height - 40);
            friendsMessage.BackgroundImage = Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("mippip2.friends.jpg"));
            friendsMessage.Image = friendsImage;
            friendsMessage.Click += new EventHandler(friendsMessage_Click);
            friendsMessage.Cursor = Cursors.Hand;
            this.Controls.Add(friendsMessage);

            Image settingImage = Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("mippip2.setting.png"));
            PictureBox pictureSetting = new PictureBox();
            pictureSetting.Size = settingImage.Size;
            pictureSetting.Location = new Point(this.ClientSize.Width / 9 * 8, this.ClientSize.Height - 40);
            pictureSetting.BackgroundImage = Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("mippip2.setting.jpg"));
            pictureSetting.Image = settingImage;
            pictureSetting.Click += new EventHandler(pictureSetting_Click);
            pictureSetting.Cursor = Cursors.Hand;
            this.Controls.Add(pictureSetting);

            //Stream fontStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("mippip2.APJapanesefont.ttf");
            //List<byte> bytesList = new List<byte>();
            //int temp;
            //while ((temp = fontStream.ReadByte()) != -1)
            //{
            //    bytesList.Add(Convert.ToByte(temp));
            //}
            //byte[] fontBytes = bytesList.ToArray();
            //fontStream.Close();
            //IntPtr fontPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(byte)) * fontBytes.Length);
            //Marshal.Copy(fontBytes, 0, fontPtr, fontBytes.Length);
            //fontCollection.AddMemoryFont(fontPtr, fontBytes.Length);
            //Marshal.FreeHGlobal(fontPtr);
            try
            {
                fontCollection.AddFontFile("APJapanesefont.ttf");
                font = new Font(fontCollection.Families[0], 10.5f);
            }
            catch
            {
                font = new Font(FontFamily.GenericSerif, 10);
            }
            
            
            FileStream stream;
            try
            {
                stream = new FileStream("setting.dat", FileMode.OpenOrCreate);
            }
            catch
            {
                return;
            }
            StreamReader reader = new StreamReader(stream);
            try
            {
                if ((userID = reader.ReadLine()) == null || (screenName = reader.ReadLine()) == null || (token = reader.ReadLine()) == null || (tokenSecret = reader.ReadLine()) == null)
                {
                    if (!Auth(stream))
                    {
                        return;
                    }
                }
                else
                {
                    auth = new Auth(ConsumerKey, ConsumerSecret, token, tokenSecret, userID, screenName);
                }
            }
            finally
            {
                reader.Close();
                stream.Close();
            }

            count.Add("count", "200");
            cursor.Add("cursor", "-1");
            startTimer = new Timer();
            startTimer.Interval = 1;
            startTimer.Tick += new EventHandler(startTimer_Tick);
            startTimer.Start();
        }

        void pictureHome_Click(object sender, EventArgs e)
        {
            if (auth.UserId == null)
            {
                return;
            }

            RefleshTimeline();
            tweetList.TopIndex = 0;
            if (!timer.Enabled)
            {
                timer.Start();
            }
        }

        void pictureAt_Click(object sender, EventArgs e)
        {
            if (auth.UserId == null)
            {
                return;
            }

            ShowAt();
            tweetList.TopIndex = 0;
        }

        private void pictureTweet_Click(object sender, EventArgs e)
        {
            if (auth.UserId == null)
            {
                return;
            }

            DoTweet();
            tweetList.TopIndex = 0;
        }

        private void pictureReflesh_Click(object sender, EventArgs e)
        {
            if (auth.UserId == null)
            {
                return;
            }

            RefleshTimeline();
            tweetList.TopIndex = 0;
        }

        private void pictureSetting_Click(object sender, EventArgs e)
        {
            if (timer == null)
            {
                return;
            }
            FileStream stream;
            try
            {
                stream = new FileStream("setting.dat", FileMode.OpenOrCreate);
            }
            catch
            {
                return;
            }
            if (timer.Enabled)
            {
                timer.Stop();
            }
            if (!Auth(stream))
            {
                stream.Close();
                return;
            }
            stream.Close();
            timer.Start();
            isFirst = true;
            RefleshTimeline();
            tweetList.TopIndex = 0;
        }

        private void pictureMessage_Click(object sender, EventArgs e)
        {
            if (auth.UserId == null)
            {
                return;
            }

            ShowDirectMessages();
            tweetList.TopIndex = 0;
        }

        void friendsMessage_Click(object sender, EventArgs e)
        {
            if (auth.UserId == null)
            {
                return;
            }

            ShowFriends();
            tweetList.TopIndex = 0;
        }

        private void tweetList_DoubleClick(object sender, EventArgs e)
        {
            if (auth.UserId == null)
            {
                return;
            }

            ShowUserTimeLine(((Tweet)tweetList.Items[tweetList.SelectedIndex]).screen_name);
            tweetList.TopIndex = 0;
        }

        private void tweetList_MouseMove(object sender, MouseEventArgs e)
        {
            tweetList.SelectedIndex = tweetList.IndexFromPoint(e.Location);
        }

        private void startTimer_Tick(object sender, EventArgs e)
        {
            startTimer.Stop();

            checkMessageTimer = new Timer();
            checkMessageTimer.Interval = 60000;
            checkMessageTimer.Tick += delegate
            {
                if (DirectMessagesCheck())
                {
                    checkMessageTimer.Stop();
                    MessageBox.Show("新しいDirectMessageがあります。", AppName);
                    checkMessageTimer.Start();
                }
            };
            checkMessageTimer.Start();

            if (!File.Exists(userID))
            {
                ShowDirectMessages(true);
            }
            else
            {
                RefleshTimeline();
            }

            timer = new Timer();
            timer.Interval = 60000;
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            RefleshTimeline();
        }

        private void tweetList_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index == -1)
            {
                return;
            }

            e.DrawBackground();

            Person Person;
            try
            {
                Person = PersonList[((Tweet)tweetList.Items[e.Index]).screen_name];
            }
            catch
            {
                this.Text = AppName + " (読み込みエラー)";
                return;
            }

            e.Graphics.DrawImage(Person.image, e.Bounds.X + 8, e.Bounds.Y + 8, 48, 48);
            if (((Tweet)tweetList.Items[e.Index]).retweeted_screen_name != null)
            {
                try
                {
                    Person retweeted_Person = PersonList[((Tweet)tweetList.Items[e.Index]).retweeted_screen_name];
                    e.Graphics.DrawImage(retweeted_Person.image, e.Bounds.X + 8 + 24, e.Bounds.Y + 8 + 48, 24, 24);
                }
                catch
                {
                    this.Text = AppName + " (読み込みエラー)";
                    return;
                }
            }
            if (((Tweet)tweetList.Items[e.Index]).retweet_count != "0")
            {
                e.Graphics.DrawString(((Tweet)tweetList.Items[e.Index]).retweet_count + "RT", font, Brushes.LightGreen,
                    new RectangleF(e.Bounds.X + 8, e.Bounds.Y + 88, 48, font.Height));
            }
            e.Graphics.DrawString(Person.name + "(" + ((Tweet)tweetList.Items[e.Index]).screen_name + ")",
                font, Brushes.Aqua, e.Bounds.X + 64, e.Bounds.Y + 8);
            e.Graphics.DrawString(((Tweet)tweetList.Items[e.Index]).text, font, Brushes.White,
                new RectangleF(e.Bounds.X + 64, e.Bounds.Y + 24, e.Bounds.Width - 64, e.Bounds.Height - 24));
            string in_reply_to = "";
            if (((Tweet)tweetList.Items[e.Index]).in_reply_to != "")
            {
                string toScreenName = null;
                for (int i = 0; i < tweetList.Items.Count; i++)
                {
                    if (((Tweet)tweetList.Items[i]).id == ((Tweet)tweetList.Items[e.Index]).in_reply_to)
                    {
                        toScreenName = ((Tweet)tweetList.Items[i]).screen_name;
                        break;
                    }
                }
                if (toScreenName != null)
                {
                    in_reply_to = "  to " + toScreenName;
                }
            }
            e.Graphics.DrawString(((Tweet)tweetList.Items[e.Index]).time + "(" + ((Tweet)tweetList.Items[e.Index]).source + ")" + in_reply_to,
                font, Brushes.LightGreen, e.Bounds.X + 64, e.Bounds.Y + e.Bounds.Height - 16);

            e.DrawFocusRectangle();
        }

        private bool Auth(FileStream stream)
        {
            auth = new Auth(ConsumerKey, ConsumerSecret);
            auth.GetRequestToken();
            string url = auth.GetAuthorizeUrl();
            Process.Start(url);
            InputSecretDialog inputSecret = new InputSecretDialog();
            inputSecret.StartPosition = FormStartPosition.CenterScreen;
            if (inputSecret.ShowDialog() != DialogResult.OK)
            {
                return false;
            }
            try
            {
                auth.GetAccessToken(inputSecret.AuthToken);
            }
            catch
            {
                MessageBox.Show("認証に失敗しました。", AppName);
                return false;
            }
            StreamWriter writer = new StreamWriter(stream);
            writer.WriteLine(auth.UserId);
            writer.WriteLine(auth.ScreenName);
            writer.WriteLine(auth.AccessToken);
            writer.WriteLine(auth.AccessTokenSecret);
            writer.Flush();
            writer.Close();
            return true;
        }

        private void RefleshTimeline()
        {
            if (isRefleshing)
            {
                return;
            }
            isRefleshing = true;
            this.Text = AppName + " (Now Loading...)";
            this.Cursor = Cursors.WaitCursor;
            newTweet = new List<Tweet>();
            newPerson = new Dictionary<string, Person>();
            isOK = false;
            notException = true;
            System.Threading.Thread thread = new System.Threading.Thread(new System.Threading.ThreadStart(GetTimeLine));
            thread.Start();
            Timer timer = new Timer();
            timer.Interval = 1;
            timer.Tick += delegate
            {
                if (isOK)
                {
                    timer.Stop();
                    if (!notException)
                    {
                        this.Text = AppName + " (読み込みエラー)";
                        this.Cursor = Cursors.Arrow;
                    }
                    else
                    {
                        if (isFirst)
                        {
                            last_time = newTweet[0].time;
                            isFirst = false;
                        }
                        else if (last_time != newTweet[0].time)
                        {
                            last_time = newTweet[0].time;
                            new PopupForm("新しいツイートがあります。").Show();
                        }
                        int index = tweetList.TopIndex;
                        tweetList.ContextMenu = new ContextMenu(timelineMenu);
                        tweetList.Items.Clear();
                        tweetList.BeginUpdate();
                        foreach (Tweet tweet in newTweet)
                        {
                            tweetList.Items.Add(tweet);
                        }
                        PersonList = newPerson;
                        tweetList.EndUpdate();
                        tweetList.TopIndex = index;
                        this.Cursor = Cursors.Arrow;
                        this.Text = AppName;
                    }
                    isRefleshing = false;
                }
            };
            timer.Start();
        }

        private void GetTimeLine()
        {
            try
            {
                string data = auth.Get(home_timeline, count);
                var json = DynamicJson.Parse(data);
                foreach (var status in json)
                {
                    Application.DoEvents();

                    bool isRetweeted = false;
                    try
                    {
                        if (status.retweeted_status != null)
                        {
                            isRetweeted = true;
                        }
                    }
                    catch { }

                    if (!isRetweeted)
                    {
                        Tweet list = new Tweet();
                        list.id = status.id_str;
                        list.time = DateTime.ParseExact(status.created_at, "ddd MMM dd HH:mm:ss zz00 yyyy", CultureInfo.InvariantCulture).ToString();
                        list.text = status.text;
                        list.source = status.source;
                        list.retweet_count = status.retweet_count.ToString();
                        list.in_reply_to = status.in_reply_to_status_id_str;
                        list.screen_name = status.user.screen_name;
                        list.text = list.text.Replace('\n', ' ').Replace("&lt;", "<").Replace("&gt;", ">");
                        if (list.source.Contains("</a>"))
                        {
                            list.source = list.source.Split('>')[1].Split('<')[0];
                        }
                        list.retweeted_screen_name = null;
                        newTweet.Add(list);

                        if (!newPerson.ContainsKey(list.screen_name))
                        {
                            Person Person = new Person();
                            Person.screen_name = status.user.screen_name;
                            Person.id = status.user.id_str;
                            Person.name = status.user.name;
                            Person.location = status.user.location;
                            Person.description = status.user.description;
                            Person.imageURL = status.user.profile_image_url;
                            Person.url = status.user.url;
                            Person.followers_count = (uint)status.user.followers_count;
                            Person.friends_count = (uint)status.user.friends_count;
                            Person.statuses_count = (uint)status.user.statuses_count;
                            if (File.Exists(cache + Path.DirectorySeparatorChar + Person.id))
                            {
                                Person.image = Image.FromFile(cache + Path.DirectorySeparatorChar + Person.id);
                            }
                            else
                            {
                                Person.image = Image.FromStream(new MemoryStream(new WebClient().DownloadData(Person.imageURL)));
                                Person.image.Save(cache + Path.DirectorySeparatorChar + Person.id);
                            }
                            newPerson.Add(Person.screen_name, Person);
                        }
                    }
                    else
                    {
                        var retweeted_status = status.retweeted_status;
                        Tweet list = new Tweet();
                        list.id = retweeted_status.id_str;
                        list.time = DateTime.ParseExact(retweeted_status.created_at, "ddd MMM dd HH:mm:ss zz00 yyyy", CultureInfo.InvariantCulture).ToString();
                        list.text = retweeted_status.text;
                        list.source = retweeted_status.source;
                        list.retweet_count = retweeted_status.retweet_count.ToString();
                        list.in_reply_to = retweeted_status.in_reply_to_status_id_str;
                        list.screen_name = retweeted_status.user.screen_name;
                        list.text = list.text.Replace('\n', ' ').Replace("&lt;", "<").Replace("&gt;", ">");
                        if (list.source.Contains("</a>"))
                        {
                            list.source = list.source.Split('>')[1].Split('<')[0];
                        }
                        list.retweeted_screen_name = status.user.screen_name;
                        newTweet.Add(list);

                        if (!newPerson.ContainsKey(list.screen_name))
                        {
                            Person Person = new Person();
                            Person.screen_name = retweeted_status.user.screen_name;
                            Person.id = retweeted_status.user.id_str;
                            Person.name = retweeted_status.user.name;
                            Person.location = retweeted_status.user.location;
                            Person.description = retweeted_status.user.description;
                            Person.imageURL = retweeted_status.user.profile_image_url;
                            Person.url = retweeted_status.user.url;
                            Person.followers_count = (uint)retweeted_status.user.followers_count;
                            Person.friends_count = (uint)retweeted_status.user.friends_count;
                            Person.statuses_count = (uint)retweeted_status.user.statuses_count;
                            if (File.Exists(cache + Path.DirectorySeparatorChar + Person.id))
                            {
                                Person.image = Image.FromFile(cache + Path.DirectorySeparatorChar + Person.id);
                            }
                            else
                            {
                                Person.image = Image.FromStream(new MemoryStream(new WebClient().DownloadData(Person.imageURL)));
                                Person.image.Save(cache + Path.DirectorySeparatorChar + Person.id);
                            }
                            newPerson.Add(Person.screen_name, Person);
                        }

                        if (!newPerson.ContainsKey(status.user.screen_name))
                        {
                            Person Person = new Person();
                            Person.screen_name = status.user.screen_name;
                            Person.id = status.user.id_str;
                            Person.name = status.user.name;
                            Person.location = status.user.location;
                            Person.description = status.user.description;
                            Person.imageURL = status.user.profile_image_url;
                            Person.url = status.user.url;
                            Person.followers_count = (uint)status.user.followers_count;
                            Person.friends_count = (uint)status.user.friends_count;
                            Person.statuses_count = (uint)status.user.statuses_count;
                            if (File.Exists(cache + Path.DirectorySeparatorChar + Person.id))
                            {
                                Person.image = Image.FromFile(cache + Path.DirectorySeparatorChar + Person.id);
                            }
                            else
                            {
                                Person.image = Image.FromStream(new MemoryStream(new WebClient().DownloadData(Person.imageURL)));
                                Person.image.Save(cache + Path.DirectorySeparatorChar + Person.id);
                            }
                            newPerson.Add(Person.screen_name, Person);
                        }
                    }
                }
            }
            catch
            {
                notException = false;
            }
            isOK = true;
        }

        private void ShowAt()
        {
            if (isRefleshing)
            {
                return;
            }
            isRefleshing = true;
            if (this.timer != null && this.timer.Enabled)
            {
                this.timer.Stop();
            }
            this.Text = AppName + " (Now Loading...)";
            this.Cursor = Cursors.WaitCursor;
            newTweet = new List<Tweet>();
            newPerson = new Dictionary<string, Person>();
            isOK = false;
            notException = true;
            System.Threading.Thread thread = new System.Threading.Thread(new System.Threading.ThreadStart(GetAtTimeLine));
            thread.Start();
            Timer timer = new Timer();
            timer.Interval = 1;
            timer.Tick += delegate
            {
                if (isOK)
                {
                    timer.Stop();
                    if (!notException)
                    {
                        this.Text = AppName + " (読み込みエラー)";
                        this.Cursor = Cursors.Arrow;
                        if (this.timer != null)
                        {
                            this.timer.Start();
                        }
                    }
                    else
                    {
                        tweetList.ContextMenu = new ContextMenu(userMenu);
                        tweetList.Items.Clear();
                        tweetList.BeginUpdate();
                        foreach (Tweet tweet in newTweet)
                        {
                            tweetList.Items.Add(tweet);
                        }
                        PersonList = newPerson;
                        tweetList.EndUpdate();
                        this.Cursor = Cursors.Arrow;
                        this.Text = AppName + " (@" + screenName + ")";
                    }
                    isRefleshing = false;
                }
            };
            timer.Start();
        }

        private void GetAtTimeLine()
        {
            try
            {
                string data = auth.Get(mentions, count);
                var json = DynamicJson.Parse(data);
                foreach (var tweet in json)
                {
                    Application.DoEvents();
                    Tweet list = new Tweet();
                    list.id = tweet.id_str;
                    list.time = DateTime.ParseExact(tweet.created_at, "ddd MMM dd HH:mm:ss zz00 yyyy", CultureInfo.InvariantCulture).ToString();
                    list.text = tweet.text;
                    list.source = tweet.source;
                    list.retweet_count = tweet.retweet_count.ToString();
                    list.in_reply_to = tweet.in_reply_to_status_id_str;
                    list.screen_name = tweet.user.screen_name;
                    list.text = list.text.Replace('\n', ' ').Replace("&lt;", "<").Replace("&gt;", ">");
                    if (list.source.Contains("</a>"))
                    {
                        list.source = list.source.Split('>')[1].Split('<')[0];
                    }
                    list.retweeted_screen_name = null;
                    newTweet.Add(list);

                    if (!newPerson.ContainsKey(list.screen_name))
                    {
                        Person Person = new Person();
                        Person.screen_name = tweet.user.screen_name;
                        Person.id = tweet.user.id_str;
                        Person.name = tweet.user.name;
                        Person.location = tweet.user.location;
                        Person.description = tweet.user.description;
                        Person.imageURL = tweet.user.profile_image_url;
                        Person.url = tweet.user.url;
                        Person.followers_count = (uint)tweet.user.followers_count;
                        Person.friends_count = (uint)tweet.user.friends_count;
                        Person.statuses_count = (uint)tweet.user.statuses_count;
                        if (File.Exists(cache + Path.DirectorySeparatorChar + Person.id))
                        {
                            Person.image = Image.FromFile(cache + Path.DirectorySeparatorChar + Person.id);
                        }
                        else
                        {
                            Person.image = Image.FromStream(new MemoryStream(new WebClient().DownloadData(Person.imageURL)));
                            Person.image.Save(cache + Path.DirectorySeparatorChar + Person.id);
                        }
                        newPerson.Add(Person.screen_name, Person);
                    }
                }
            }
            catch
            {
                notException = false;
            }
            isOK = true;
        }

        private void ShowUserTimeLine(string screen_name)
        {
            if (isRefleshing)
            {
                return;
            }
            isRefleshing = true;
            if (this.timer != null && this.timer.Enabled)
            {
                this.timer.Stop();
            }
            this.Text = AppName + " (Now Loading...)";
            this.Cursor = Cursors.WaitCursor;
            newTweet = new List<Tweet>();
            newPerson = new Dictionary<string, Person>();
            isOK = false;
            notException = true;
            getScreenName = screen_name;
            System.Threading.Thread thread = new System.Threading.Thread(new System.Threading.ThreadStart(GetUserTimeLine));
            thread.Start();
            Timer timer = new Timer();
            timer.Interval = 1;
            timer.Tick += delegate
            {
                if (isOK)
                {
                    timer.Stop();
                    if (!notException)
                    {
                        this.Text = AppName + " (読み込みエラー)";
                        this.Cursor = Cursors.Arrow;
                        if (this.timer != null)
                        {
                            this.timer.Start();
                        }
                    }
                    else
                    {
                        tweetList.ContextMenu = new ContextMenu(userMenu);
                        tweetList.Items.Clear();
                        tweetList.BeginUpdate();
                        foreach (Tweet tweet in newTweet)
                        {
                            tweetList.Items.Add(tweet);
                        }
                        PersonList = newPerson;
                        tweetList.EndUpdate();
                        this.Cursor = Cursors.Arrow;
                        this.Text = AppName + " (" + screen_name + ")";
                    }
                    isRefleshing = false;
                }
            };
            timer.Start();
        }

        private void ShowUserTimeLine(object sender, EventArgs e)
        {
            GetInformationDialog information = new GetInformationDialog("スクリーンネーム");
            if (information.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            ShowUserTimeLine(information.Information);
        }

        private void GetUserTimeLine()
        {
            try
            {
                Dictionary<string, string> param = new Dictionary<string, string>();
                param.Add("screen_name", getScreenName);
                param.Add("count", "200");
                string data = auth.Get(user_timeline, param);
                var json = DynamicJson.Parse(data);
                foreach (var status in json)
                {
                    Application.DoEvents();

                    bool isRetweeted = false;
                    try
                    {
                        if (status.retweeted_status != null)
                        {
                            isRetweeted = true;
                        }
                    }
                    catch { }

                    if (!isRetweeted)
                    {
                        Tweet list = new Tweet();
                        list.id = status.id_str;
                        list.time = DateTime.ParseExact(status.created_at, "ddd MMM dd HH:mm:ss zz00 yyyy", CultureInfo.InvariantCulture).ToString();
                        list.text = status.text;
                        list.source = status.source;
                        list.retweet_count = status.retweet_count.ToString();
                        list.in_reply_to = status.in_reply_to_status_id_str;
                        list.screen_name = status.user.screen_name;
                        list.text = list.text.Replace('\n', ' ').Replace("&lt;", "<").Replace("&gt;", ">");
                        if (list.source.Contains("</a>"))
                        {
                            list.source = list.source.Split('>')[1].Split('<')[0];
                        }
                        list.retweeted_screen_name = null;
                        newTweet.Add(list);

                        if (!newPerson.ContainsKey(list.screen_name))
                        {
                            Person Person = new Person();
                            Person.screen_name = status.user.screen_name;
                            Person.id = status.user.id_str;
                            Person.name = status.user.name;
                            Person.location = status.user.location;
                            Person.description = status.user.description;
                            Person.imageURL = status.user.profile_image_url;
                            Person.url = status.user.url;
                            Person.followers_count = (uint)status.user.followers_count;
                            Person.friends_count = (uint)status.user.friends_count;
                            Person.statuses_count = (uint)status.user.statuses_count;
                            if (File.Exists(cache + Path.DirectorySeparatorChar + Person.id))
                            {
                                Person.image = Image.FromFile(cache + Path.DirectorySeparatorChar + Person.id);
                            }
                            else
                            {
                                Person.image = Image.FromStream(new MemoryStream(new WebClient().DownloadData(Person.imageURL)));
                                Person.image.Save(cache + Path.DirectorySeparatorChar + Person.id);
                            }
                            newPerson.Add(Person.screen_name, Person);
                        }
                    }
                    else
                    {
                        var retweeted_status = status.retweeted_status;
                        Tweet list = new Tweet();
                        list.id = retweeted_status.id_str;
                        list.time = DateTime.ParseExact(retweeted_status.created_at, "ddd MMM dd HH:mm:ss zz00 yyyy", CultureInfo.InvariantCulture).ToString();
                        list.text = retweeted_status.text;
                        list.source = retweeted_status.source;
                        list.retweet_count = retweeted_status.retweet_count.ToString();
                        list.in_reply_to = retweeted_status.in_reply_to_status_id_str;
                        list.screen_name = retweeted_status.user.screen_name;
                        list.text = list.text.Replace('\n', ' ').Replace("&lt;", "<").Replace("&gt;", ">");
                        if (list.source.Contains("</a>"))
                        {
                            list.source = list.source.Split('>')[1].Split('<')[0];
                        }
                        list.retweeted_screen_name = status.user.screen_name;
                        newTweet.Add(list);

                        if (!newPerson.ContainsKey(list.screen_name))
                        {
                            Person Person = new Person();
                            Person.screen_name = retweeted_status.user.screen_name;
                            Person.id = retweeted_status.user.id_str;
                            Person.name = retweeted_status.user.name;
                            Person.location = retweeted_status.user.location;
                            Person.description = retweeted_status.user.description;
                            Person.imageURL = retweeted_status.user.profile_image_url;
                            Person.url = retweeted_status.user.url;
                            Person.followers_count = (uint)retweeted_status.user.followers_count;
                            Person.friends_count = (uint)retweeted_status.user.friends_count;
                            Person.statuses_count = (uint)retweeted_status.user.statuses_count;
                            if (File.Exists(cache + Path.DirectorySeparatorChar + Person.id))
                            {
                                Person.image = Image.FromFile(cache + Path.DirectorySeparatorChar + Person.id);
                            }
                            else
                            {
                                Person.image = Image.FromStream(new MemoryStream(new WebClient().DownloadData(Person.imageURL)));
                                Person.image.Save(cache + Path.DirectorySeparatorChar + Person.id);
                            }
                            newPerson.Add(Person.screen_name, Person);
                        }

                        if (!newPerson.ContainsKey(status.user.screen_name))
                        {
                            Person Person = new Person();
                            Person.screen_name = status.user.screen_name;
                            Person.id = status.user.id_str;
                            Person.name = status.user.name;
                            Person.location = status.user.location;
                            Person.description = status.user.description;
                            Person.imageURL = status.user.profile_image_url;
                            Person.url = status.user.url;
                            Person.followers_count = (uint)status.user.followers_count;
                            Person.friends_count = (uint)status.user.friends_count;
                            Person.statuses_count = (uint)status.user.statuses_count;
                            if (File.Exists(cache + Path.DirectorySeparatorChar + Person.id))
                            {
                                Person.image = Image.FromFile(cache + Path.DirectorySeparatorChar + Person.id);
                            }
                            else
                            {
                                Person.image = Image.FromStream(new MemoryStream(new WebClient().DownloadData(Person.imageURL)));
                                Person.image.Save(cache + Path.DirectorySeparatorChar + Person.id);
                            }
                            newPerson.Add(Person.screen_name, Person);
                        }
                    }
                }
            }
            catch
            {
                notException = false;
            }
            isOK = true;
        }

        private void ShowDirectMessages(bool timerStartAndReflesh = false)
        {
            if (isRefleshing)
            {
                return;
            }
            isRefleshing = true;
            if (this.timer != null && this.timer.Enabled)
            {
                this.timer.Stop();
            }
            this.Text = AppName + " (Now Loading...)";
            this.Cursor = Cursors.WaitCursor;
            newTweet = new List<Tweet>();
            newPerson = new Dictionary<string, Person>();
            isOK = false;
            notException = true;
            System.Threading.Thread thread = new System.Threading.Thread(new System.Threading.ThreadStart(GetDirectMessages));
            thread.Start();
            Timer timer = new Timer();
            timer.Interval = 1;
            timer.Tick += delegate
            {
                if (isOK)
                {
                    timer.Stop();
                    if (!notException)
                    {
                        this.Text = AppName + " (読み込みエラー)";
                        this.Cursor = Cursors.Arrow;
                        if (this.timer != null)
                        {
                            this.timer.Start();
                        }
                    }
                    else
                    {
                        tweetList.ContextMenu = new ContextMenu(messageMenu);
                        tweetList.Items.Clear();
                        tweetList.BeginUpdate();
                        foreach (Tweet tweet in newTweet)
                        {
                            tweetList.Items.Add(tweet);
                        }
                        PersonList = newPerson;
                        tweetList.EndUpdate();
                        this.Cursor = Cursors.Arrow;
                        this.Text = AppName + " (DirectMessages)";

                        try
                        {
                            FileStream stream = new FileStream(userID, FileMode.Create);
                            StreamWriter writer = new StreamWriter(stream);
                            writer.Write(newTweet[0].id);
                            writer.Flush();
                            writer.Close();
                            stream.Close();
                        }
                        catch { }
                    }
                    isRefleshing = false;
                    if (timerStartAndReflesh)
                    {
                        if (this.timer != null && !timer.Enabled)
                        {
                            this.timer.Start();
                        }
                        RefleshTimeline();
                    }
                }
            };
            timer.Start();
        }

        private void GetDirectMessages()
        {
            try
            {
                string data = auth.Get(direct_messages, count);
                var json = DynamicJson.Parse(data);
                foreach (var tweet in json)
                {
                    Application.DoEvents();
                    Tweet list = new Tweet();
                    list.id = tweet.sender_id_str;
                    list.time = DateTime.ParseExact(tweet.created_at, "ddd MMM dd HH:mm:ss zz00 yyyy", CultureInfo.InvariantCulture).ToString();
                    list.text = tweet.text;
                    list.source = "Direct Message";
                    list.retweet_count = "0";
                    list.in_reply_to = "";
                    list.screen_name = tweet.sender.screen_name;
                    list.text = list.text.Replace('\n', ' ').Replace("&lt;", "<").Replace("&gt;", ">");
                    list.retweeted_screen_name = null;
                    newTweet.Add(list);

                    if (!newPerson.ContainsKey(list.screen_name))
                    {
                        Person Person = new Person();
                        Person.screen_name = tweet.sender.screen_name;
                        Person.id = tweet.sender.id_str;
                        Person.name = tweet.sender.name;
                        Person.location = tweet.sender.location;
                        Person.description = tweet.sender.description;
                        Person.imageURL = tweet.sender.profile_image_url;
                        Person.url = tweet.sender.url;
                        Person.followers_count = (uint)tweet.sender.followers_count;
                        Person.friends_count = (uint)tweet.sender.friends_count;
                        Person.statuses_count = (uint)tweet.sender.statuses_count;
                        if (File.Exists(cache + Path.DirectorySeparatorChar + Person.id))
                        {
                            Person.image = Image.FromFile(cache + Path.DirectorySeparatorChar + Person.id);
                        }
                        else
                        {
                            Person.image = Image.FromStream(new MemoryStream(new WebClient().DownloadData(Person.imageURL)));
                            Person.image.Save(cache + Path.DirectorySeparatorChar + Person.id);
                        }
                        newPerson.Add(Person.screen_name, Person);
                    }
                }
            }
            catch
            {
                notException = false;
            }
            isOK = true;
        }

        private void ShowFriends()
        {
            if (isRefleshing)
            {
                return;
            }
            isRefleshing = true;
            if (this.timer != null && this.timer.Enabled)
            {
                this.timer.Stop();
            }
            this.Text = AppName + " (Now Loading...)";
            this.Cursor = Cursors.WaitCursor;
            newTweet = new List<Tweet>();
            newPerson = new Dictionary<string, Person>();
            isOK = false;
            notException = true;
            System.Threading.Thread thread = new System.Threading.Thread(new System.Threading.ThreadStart(GetFriends));
            thread.Start();
            Timer timer = new Timer();
            timer.Interval = 1;
            timer.Tick += delegate
            {
                if (isOK)
                {
                    timer.Stop();
                    if (!notException)
                    {
                        this.Text = AppName + " (読み込みエラー)";
                        this.Cursor = Cursors.Arrow;
                        if (this.timer != null)
                        {
                            this.timer.Start();
                        }
                    }
                    else
                    {
                        tweetList.ContextMenu = new ContextMenu(friendsMenu);
                        tweetList.Items.Clear();
                        tweetList.BeginUpdate();
                        foreach (Tweet tweet in newTweet)
                        {
                            tweetList.Items.Add(tweet);
                        }
                        PersonList = newPerson;
                        tweetList.EndUpdate();
                        this.Cursor = Cursors.Arrow;
                        this.Text = AppName + " (friends and followers)";
                    }
                    isRefleshing = false;
                }
            };
            timer.Start();
        }

        private void GetFriends()
        {
            try
            {
                string friends_data = auth.Get(friends, cursor);
                var friends_json = DynamicJson.Parse(friends_data);
                foreach (var friends_value in friends_json.ids)
                {
                    Dictionary<string, string> id = new Dictionary<string, string>();
                    id.Add("user_id", ((uint)friends_value).ToString());
                    string json = auth.Get(lookup, id);
                    var friends_array = DynamicJson.Parse(json);
                    foreach (var friend in friends_array)
                    {
                        Tweet list = new Tweet();
                        list.id = friend.id_str;
                        list.time = "";
                        list.text = friend.description;
                        list.source = "friend";
                        list.retweet_count = "0";
                        list.in_reply_to = "";
                        list.screen_name = friend.screen_name;
                        list.text = list.text.Replace('\n', ' ').Replace("&lt;", "<").Replace("&gt;", ">");
                        list.retweeted_screen_name = null;

                        newTweet.Add(list);

                        Person person = new Person();
                        person.id = friend.id_str;
                        person.screen_name = friend.screen_name;
                        person.name = friend.name;
                        person.location = friend.location;
                        person.description = friend.description;
                        person.imageURL = friend.profile_image_url;
                        person.url = friend.url;
                        person.followers_count = (uint)friend.followers_count;
                        person.friends_count = (uint)friend.friends_count;
                        person.statuses_count = (uint)friend.statuses_count;
                        if (File.Exists(cache + Path.DirectorySeparatorChar + person.id))
                        {
                            person.image = Image.FromFile(cache + Path.DirectorySeparatorChar + person.id);
                        }
                        else
                        {
                            person.image = Image.FromStream(new MemoryStream(new WebClient().DownloadData(person.imageURL)));
                            person.image.Save(cache + Path.DirectorySeparatorChar + person.id);
                        }
                        newPerson.Add(person.screen_name, person);
                    }
                }

                List<Tweet> friend_and_follower = new List<Tweet>();

                Application.DoEvents();

                string followers_data = auth.Get(followers, cursor);
                var followers_json = DynamicJson.Parse(followers_data);

                foreach (var followers_value in followers_json.ids)
                {
                    Dictionary<string, string> id = new Dictionary<string, string>();
                    id.Add("user_id", ((uint)followers_value).ToString());
                    string json = auth.Get(lookup, id);
                    var followers_array = DynamicJson.Parse(json);
                    foreach (var follower in followers_array)
                    {
                        Tweet list = new Tweet();
                        list.id = follower.id_str;
                        list.time = "";
                        list.text = follower.description;
                        list.source = "follower";
                        list.retweet_count = "0";
                        list.in_reply_to = "";
                        list.screen_name = follower.screen_name;
                        list.text = list.text.Replace('\n', ' ').Replace("&lt;", "<").Replace("&gt;", ">");
                        list.retweeted_screen_name = null;

                        bool isContain = false;
                        foreach (Tweet tweet in newTweet)
                        {
                            if (tweet.id == list.id)
                            {
                                isContain = true;
                                Tweet t = tweet;
                                t.source = "frined and follower";
                                friend_and_follower.Add(t);
                                newTweet.Remove(tweet);
                                break;
                            }
                        }
                        if (isContain)
                        {
                            continue;
                        }

                        newTweet.Add(list);

                        Person person = new Person();
                        person.id = follower.id_str;
                        person.screen_name = follower.screen_name;
                        person.name = follower.name;
                        person.location = follower.location;
                        person.description = follower.description;
                        person.imageURL = follower.profile_image_url;
                        person.url = follower.url;
                        person.followers_count = (uint)follower.followers_count;
                        person.friends_count = (uint)follower.friends_count;
                        person.statuses_count = (uint)follower.statuses_count;
                        if (File.Exists(cache + Path.DirectorySeparatorChar + person.id))
                        {
                            person.image = Image.FromFile(cache + Path.DirectorySeparatorChar + person.id);
                        }
                        else
                        {
                            person.image = Image.FromStream(new MemoryStream(new WebClient().DownloadData(person.imageURL)));
                            person.image.Save(cache + Path.DirectorySeparatorChar + person.id);
                        }
                        newPerson.Add(person.screen_name, person);
                    }
                }

                foreach (Tweet tweet in friend_and_follower)
                {
                    newTweet.Add(tweet);
                }
            }
            catch
            {
                notException = false;
            }
            isOK = true;
        }

        private bool DirectMessagesCheck()
        {
            if (!File.Exists(userID))
            {
                return false;
            }

            try
            {
                FileStream stream = new FileStream(userID, FileMode.Open);
                StreamReader reader = new StreamReader(stream);
                string last_id = reader.ReadToEnd();
                reader.Close();
                stream.Close();
                Dictionary<string, string> since_id = new Dictionary<string, string>();
                since_id.Add("since_id", last_id);
                string data = auth.Get(direct_messages, since_id);
                var json = DynamicJson.Parse(data);
                foreach (var tweet in json)
                {
                    if (tweet.id != null)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch
            {
                return false;
            }
            return false;
        }

        private void ShowUserInformation(object sender, EventArgs e)
        {
            new InformationForm(PersonList[((Tweet)tweetList.Items[tweetList.SelectedIndex]).screen_name]).Show();
        }

        private void DoTweet(string initialize = "", string id = null, bool cursorFirst = false)
        {
            if (timer != null)
            {
                timer.Stop();
            }
            TweetDialog tweetDialog = new TweetDialog("Tweet", cursorFirst);
            tweetDialog.Tweet = initialize;
            if (tweetDialog.ShowDialog() == DialogResult.OK)
            {
                Dictionary<string, string> post = new Dictionary<string, string>();
                post.Add("status", auth.UrlEncode(tweetDialog.Tweet));
                if (id != null)
                {
                    post.Add("in_reply_to_status_id", id);
                }
                try
                {
                    auth.Post(update, post);
                }
                catch
                {
                    MessageBox.Show("Tweet出来ませんでした。", AppName);
                }
                RefleshTimeline();
            }
            if (timer != null)
            {
                timer.Start();
            }
        }

        private void Reply(object sender, EventArgs e)
        {
            if (tweetList.SelectedIndex == -1)
            {
                return;
            }

            DoTweet("@" + ((Tweet)tweetList.Items[tweetList.SelectedIndex]).screen_name + " ", ((Tweet)tweetList.Items[tweetList.SelectedIndex]).id);
        }

        private void ReTweet(object sender, EventArgs e)
        {
            if (tweetList.SelectedIndex == -1)
            {
                return;
            }

            if (MessageBox.Show("公式リツイートしますか？", AppName, MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                Dictionary<string, string> post = new Dictionary<string, string>();
                try
                {
                    auth.Post(retweet + ((Tweet)tweetList.Items[tweetList.SelectedIndex]).id + ".json", post);
                }
                catch
                {
                    MessageBox.Show("リツイート出来ませんでした。", AppName);
                    return;
                }
                MessageBox.Show("リツイートしました。", AppName);
            }
        }

        private void ReTweetWithComment(object sender, EventArgs e)
        {
            if (tweetList.SelectedIndex == -1)
            {
                return;
            }

            DoTweet(" RT @" + ((Tweet)tweetList.Items[tweetList.SelectedIndex]).screen_name + ": " + ((Tweet)tweetList.Items[tweetList.SelectedIndex]).text, ((Tweet)tweetList.Items[tweetList.SelectedIndex]).id, true);
        }

        private void QTWithComment(object sender, EventArgs e)
        {
            if (tweetList.SelectedIndex == -1)
            {
                return;
            }

            DoTweet(" QT @" + ((Tweet)tweetList.Items[tweetList.SelectedIndex]).screen_name + ": " + ((Tweet)tweetList.Items[tweetList.SelectedIndex]).text, ((Tweet)tweetList.Items[tweetList.SelectedIndex]).id, true);
        }

        private void SendDirectMessage(object sender, EventArgs e)
        {
            if (tweetList.SelectedIndex == -1)
            {
                return;
            }
            
            TweetDialog tweetDialog = new TweetDialog("DirectMessage");
            if (tweetDialog.ShowDialog() == DialogResult.OK)
            {
                Dictionary<string, string> post = new Dictionary<string, string>();
                post.Add("user", ((Tweet)tweetList.Items[tweetList.SelectedIndex]).screen_name);
                post.Add("text", auth.UrlEncode(tweetDialog.Tweet));
                try
                {
                    auth.Post(direct_message_new, post);
                }
                catch
                {
                    MessageBox.Show("送信出来ませんでした。", AppName);
                    return;
                }
                MessageBox.Show("送信しました。", AppName);
            }
        }

        private void Delete(object sender, EventArgs e)
        {
            if (tweetList.SelectedIndex == -1)
            {
                return;
            }

            Dictionary<string, string> post = new Dictionary<string, string>();
            try
            {
                auth.Post(destroy + ((Tweet)tweetList.Items[tweetList.SelectedIndex]).id + ".json", post);
            }
            catch
            {
                MessageBox.Show("削除出来ませんでした。", AppName);
                return;
            }
            isFirst = true;
            RefleshTimeline();
        }

        private void OpenURL(object sender, EventArgs e)
        {
            if (tweetList.SelectedIndex == -1)
            {
                return;
            }

            string[] split = ((Tweet)tweetList.Items[tweetList.SelectedIndex]).text.Split(' ', '　', '(', '（', '）', ')');
            List<string> stringList = new List<string>();
            foreach (string url in split)
            {
                if (url.StartsWith("http://") || url.StartsWith("https://") || url.StartsWith("ftp://"))
                {
                    stringList.Add(url);
                }
            }
            GetInformationList information = new GetInformationList("URL", stringList.ToArray());
            information.ShowDialog();
            if (information.SelectedItem != null)
            {
                Process.Start(information.SelectedItem);
            }
        }

        private void SearchHash(object sender, EventArgs e)
        {
            if (tweetList.SelectedIndex == -1)
            {
                return;
            }

            string[] split = ((Tweet)tweetList.Items[tweetList.SelectedIndex]).text.Split(' ', '　');
            List<string> stringList = new List<string>();
            foreach (string hash in split)
            {
                if (hash.StartsWith("#"))
                {
                    stringList.Add(hash);
                }
            }
            GetInformationList information = new GetInformationList("Hash", stringList.ToArray());
            information.ShowDialog();
            if (information.SelectedItem != null)
            {
                Process.Start(search + auth.UrlEncode(information.SelectedItem));
            }
        }

        private void ShowAtUserTimeLine(object sender, EventArgs e)
        {
            if (tweetList.SelectedIndex == -1)
            {
                return;
            }

            string[] split = ((Tweet)tweetList.Items[tweetList.SelectedIndex]).text.Split(' ', '　');
            List<string> stringList = new List<string>();
            foreach (string at in split)
            {
                bool removeFirst = false, removeLast = false;
                if (at.StartsWith("@"))
                {
                    if (at.StartsWith("."))
                    {
                        removeFirst = true;
                    }
                    if (at.EndsWith(":"))
                    {
                        removeLast = true;
                    }

                    if (removeFirst && removeLast)
                    {
                        stringList.Add(at.Substring(1, at.Length - 2));
                    }
                    else if (removeFirst && !removeLast)
                    {
                        stringList.Add(at.Substring(1));
                    }
                    else if (!removeFirst && removeLast)
                    {
                        stringList.Add(at.Substring(0, at.Length - 1));
                    }
                    else
                    {
                        stringList.Add(at);
                    }
                }
            }
            GetInformationList information = new GetInformationList("@", stringList.ToArray());
            information.ShowDialog();
            if (information.SelectedItem != null)
            {
                ShowUserTimeLine(information.SelectedItem.Substring(1));
            }
        }

        private void SearchInReplyTo(object sender, EventArgs e)
        {
            if (tweetList.SelectedIndex == -1 || ((Tweet)tweetList.Items[tweetList.SelectedIndex]).in_reply_to == "")
            {
                return;
            }

            for (int i = 0; i < tweetList.Items.Count; i++)
            {
                if (((Tweet)tweetList.Items[tweetList.SelectedIndex]).in_reply_to == ((Tweet)tweetList.Items[i]).id)
                {
                    tweetList.TopIndex = i;
                    break;
                }
            }
        }

        private void CreateFriend(object sender, EventArgs e)
        {
            if (tweetList.SelectedIndex == -1)
            {
                return;
            }

            Dictionary<string, string> post = new Dictionary<string, string>();
            post.Add("id", ((Tweet)tweetList.Items[tweetList.SelectedIndex]).screen_name);
            try
            {
                auth.Post(friend_create, post);
            }
            catch
            {
                MessageBox.Show("フォローに失敗しました。", AppName);
                return;
            }
            MessageBox.Show("フォローしました。", AppName);
            ShowFriends();
        }

        private void NewCreateFriend(object sender, EventArgs e)
        {
            GetInformationDialog information = new GetInformationDialog("スクリーンネーム");
            if (information.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            Dictionary<string, string> post = new Dictionary<string, string>();
            post.Add("id", information.Information);
            try
            {
                auth.Post(friend_create, post);
            }
            catch
            {
                MessageBox.Show("フォローに失敗しました。", AppName);
                return;
            }
            MessageBox.Show("フォローしました。", AppName);
            ShowFriends();
        }

        private void DestroyFriend(object sender, EventArgs e)
        {
            if (tweetList.SelectedIndex == -1)
            {
                return;
            }

            Dictionary<string, string> post = new Dictionary<string, string>();
            post.Add("id", ((Tweet)tweetList.Items[tweetList.SelectedIndex]).screen_name);
            try
            {
                auth.Post(frined_destroy, post);
            }
            catch
            {
                MessageBox.Show("フォロー解除に失敗しました。", AppName);
                return;
            }
            MessageBox.Show("フォローを解除しました。", AppName);
            ShowFriends();
        }

        private void BlockFriend(object sender, EventArgs e)
        {
            if (tweetList.SelectedIndex == -1)
            {
                return;
            }

            if (MessageBox.Show("本当にブロックしますか？", AppName, MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                Dictionary<string, string> post = new Dictionary<string, string>();
                post.Add("id", ((Tweet)tweetList.Items[tweetList.SelectedIndex]).screen_name);
                try
                {
                    auth.Post(blocks_create, post);
                }
                catch
                {
                    MessageBox.Show("ブロックに失敗しました。", AppName);
                    return;
                }
                MessageBox.Show("ブロックしました。", AppName);
                ShowFriends();
            }
        }

        private void UnBlockFriend(object sender, EventArgs e)
        {
            GetInformationDialog information = new GetInformationDialog("スクリーンネーム");
            if (information.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            Dictionary<string, string> post = new Dictionary<string, string>();
            post.Add("id", information.Information);
            try
            {
                auth.Post(blocks_destroy, post);
            }
            catch
            {
                MessageBox.Show("ブロックの解除に失敗しました。", AppName);
                return;
            }
            MessageBox.Show("ブロックを解除しました。", AppName);
            ShowFriends();
        }

        private void Copy(object sender, EventArgs e)
        {
            Clipboard.SetText(((Tweet)tweetList.Items[tweetList.SelectedIndex]).text);
        }

        public struct Tweet
        {
            public string id;
            public string time;             // created_at
            public string text;
            public string source;
            public string in_reply_to;      // in_reply_to_status_id
            public string screen_name;
            public string retweet_count;
            public string retweeted_screen_name;
        }

        public struct Person
        {
            public string screen_name;
            public string id;
            public string name;
            public string location;
            public string description;
            public string imageURL;         // profile_image_url
            public Image image;
            public string url;
            public uint followers_count;
            public uint friends_count;
            public uint statuses_count;
        }
    }
}
