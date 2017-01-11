using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CrossToeClient.ServiceReference;
using System.ServiceModel;
using System.Threading;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

namespace CrossToeClient
{
    public partial class MainWindow : Window
    {
        public static Action<int, string, int> actionCallback;
        ManagerGameClient managerGame;
        List<Button> listBtn;
        int[,] coordinatesLine;
        int userID;
        char symbol;
        bool isClear;

        public MainWindow()
        {
            InitializeComponent();

            listBtn = new List<Button> { bntCall1, bntCall2, bntCall3, bntCall4, bntCall5, bntCall6, bntCall7, bntCall8, bntCall9 };
            actionCallback += GetUpdate;

            coordinatesLine = new int[,]
            {
                { 0, 0, 0, 0 },
                { 0, 47, 335, 47 },
                { 0, 137, 335, 137 },
                { 0, 227, 335, 227 },
                { 57, 0, 57, 270 },
                { 167, 0, 167, 270 },
                { 277, 0, 277, 270 },
                { 0, 0, 335, 270 },
                { 0, 270, 335, 0 }
            };
        }

        private async void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                tbName.IsEnabled = false;
                btnStart.IsEnabled = false;
                tbStatus.Text = "Ожидайте соперника!";
                userID = await CallBackHendler.proxy.AddNewUserAsync(tbName.Text);
                symbol = (userID % 2 == 0) ? 'O' : 'X';
                managerGame = new ManagerGameClient();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Произошла ошибка!");
            }
        }

        private async void BtnCall_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = (Button)sender;

                if (btn.Content != null)
                    return;

                btn.Content = symbol;
                await managerGame.SetPositionAsync(userID, Convert.ToInt32(btn.Tag));
                CallsIsEnabled(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Произошла ошибка!");
            }
        }

        private void GetUpdate(int posMove, string message, int lineWin)
        {
            if (isClear)
            {
                isClear = false;
                ClearActions();
            }

            if (posMove > -1)
                listBtn[posMove].Content = (symbol == 'X') ? 'O' : 'X';

            if (message != "")
                tbStatus.Text = message;

            if (posMove != -2)
                CallsIsEnabled((lineWin >= 0) ? false : true);

            if (lineWin >= 0)
            {
                VisibleLine(lineWin);

                Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(1000);
                    Dispatcher.Invoke(ScreenShot);
                });

                isClear = true;
            }
        }

        private void CallsIsEnabled(bool val)
        {
            for (int i = 0; i < listBtn.Count; i++)
                listBtn[i].IsEnabled = val;
        }

        private void VisibleLine(int id)
        {
            lineWin.X1 = coordinatesLine[id, 0];
            lineWin.Y1 = coordinatesLine[id, 1];
            lineWin.X2 = coordinatesLine[id, 2];
            lineWin.Y2 = coordinatesLine[id, 3];
        }

        private void ClearActions()
        {
            VisibleLine(0);

            for (int i = 0; i < listBtn.Count; i++)
                listBtn[i].Content = null;
        }

        private void ScreenShot()
        {
            if (spHistory.Children.Count > 10)
                spHistory.Children.Clear();

            Bitmap bmpScreenshot = new Bitmap(335, 272, System.Drawing.Imaging.PixelFormat.Format16bppRgb555);
            Graphics gfxScreenshot = Graphics.FromImage(bmpScreenshot);
            gfxScreenshot.CopyFromScreen((int)Left + 10, (int)Top + 82, 0, 0, new System.Drawing.Size(335, 272), CopyPixelOperation.SourceCopy);

            using (MemoryStream memory = new MemoryStream())
            {
                bmpScreenshot.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                spHistory.Children.Add(new System.Windows.Controls.Image { Source = bitmapImage });
            }
        }
    }

    class CallBackHendler : IDuplexServiceCallback
    {
        static InstanceContext site = new InstanceContext(new CallBackHendler());
        public static DuplexServiceClient proxy = new DuplexServiceClient(site);

        public void GetUpdate(int posMove, string message, int lineWin)
        {
            if (MainWindow.actionCallback != null)
                MainWindow.actionCallback(posMove, message, lineWin);
        }
    }

}
