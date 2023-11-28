using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;

namespace DIP
{
    public partial class Form1 : Form
    {
        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoSource;
        private Bitmap backgroundImage;
        private Bitmap foregroundImage;
        private Bitmap resultImage;
        private Color mygreen = Color.FromArgb(0, 255, 0);
        private int threshold = 5;

        public Form1()
        {
            InitializeComponent();
            InitializeWebcam();
        }

        private void InitializeWebcam()
        {
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (videoDevices.Count > 0)
            {
                videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
                videoSource.NewFrame += new NewFrameEventHandler(video_NewFrame);
                videoSource.Start();
            }
        }

        private void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            
            pictureBoxWebcam.Image = (Bitmap)eventArgs.Frame.Clone();
            pictureBoxWebcam.SizeMode = PictureBoxSizeMode.StretchImage;
        }

        private void CaptureImage()
        {
            
            if (pictureBoxWebcam.Image != null)
            {
               
                if (backgroundImage != null)
                {
                    backgroundImage.Dispose();
                }

               
                backgroundImage = new Bitmap(pictureBoxWebcam.Image);

                
                if (pictureBoxBackground.Image != null)
                {
                    pictureBoxBackground.Image.Dispose();
                }

               
                pictureBoxBackground.Image = backgroundImage;
                pictureBoxBackground.SizeMode = PictureBoxSizeMode.StretchImage;
            }
            else
            {
                MessageBox.Show("Failed to capture image from webcam.");
            }
        }


        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (videoSource != null && videoSource.IsRunning)
            {
                videoSource.SignalToStop();
                videoSource.WaitForStop();
            }
        }



        private void captureImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CaptureImage();
        }

        private void loadImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Open Image";
                dlg.Filter = "Image files (*.bmp, *.jpg, *.jpeg, *.png)|*.bmp;*.jpg;*.jpeg;*.png";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    foregroundImage = new Bitmap(dlg.FileName);
                    pictureBoxForeground.Image = foregroundImage;
                    pictureBoxForeground.SizeMode = PictureBoxSizeMode.StretchImage;
                }
            }
        }

        private void imageSubtractionToolStripMenuItem_Click(object sender, EventArgs e)
        {

            pictureBoxBackground = new PictureBox();
            pictureBoxBackground.Image = backgroundImage;
            pictureBoxForeground = new PictureBox();
            pictureBoxForeground.Image = foregroundImage;

            if (backgroundImage != null && foregroundImage != null)
            {

                if (backgroundImage.Size != foregroundImage.Size)
                {
                    foregroundImage = new Bitmap(foregroundImage, backgroundImage.Size);
                }
                Color chromaKeyColor = Color.FromArgb(0, 255, 0);
                int tolerance = 150; 
                resultImage = new Bitmap(foregroundImage.Width, foregroundImage.Height);

                for (int x = 0; x < foregroundImage.Width; x++)
                {
                    for (int y = 0; y < foregroundImage.Height; y++)
                    {
                        Color fgColor = foregroundImage.GetPixel(x, y);
                        if (IsChroma(fgColor, chromaKeyColor, tolerance))
                        {
                            
                            Color bgColor = backgroundImage.GetPixel(x, y);
                            resultImage.SetPixel(x, y, bgColor);
                        }
                        else
                        {
                            
                            resultImage.SetPixel(x, y, fgColor);
                        }
                    }
                }

                pictureBoxResult.Image = resultImage;
                pictureBoxResult.SizeMode = PictureBoxSizeMode.StretchImage;
            }
            else
            {
                MessageBox.Show("Please capture the background and load the foreground image first.");
            }
        }

        private bool IsChroma(Color color, Color chromaKeyColor, int tolerance)
        {
            return Math.Abs(color.R - chromaKeyColor.R) < tolerance &&
                   Math.Abs(color.G - chromaKeyColor.G) < tolerance &&
                   Math.Abs(color.B - chromaKeyColor.B) < tolerance;
        }
    }

}

