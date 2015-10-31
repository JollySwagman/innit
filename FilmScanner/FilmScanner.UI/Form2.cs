﻿
using System;
using System.Drawing;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
using System.Drawing.Imaging;
using System.IO;
using FilmScanner;
using System.Diagnostics;

namespace Super8Scanner.UI
{

    /// <summary>
    /// See http://www.emgu.com/wiki/index.php/Camera_Capture_in_7_lines_of_code ??
    /// </summary>
    public partial class Form2 : Form
    {

        private FilmScanner.FrameScanner m_FrameScanner;

        private bool DeviceExist = false;

        private FilterInfoCollection videoDevices;

        private VideoCaptureDevice videoSource = null;

        public Form2()
        {
            InitializeComponent();
        }

        // get the devices name
        private void getCamList()
        {
            try
            {
                videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                comboBox1.Items.Clear();
                if (videoDevices.Count == 0)
                    throw new ApplicationException();

                DeviceExist = true;
                foreach (FilterInfo device in videoDevices)
                {
                    comboBox1.Items.Add(device.Name);
                }
                comboBox1.SelectedIndex = 0; //make dafault to first cam
            }
            catch (ApplicationException)
            {
                DeviceExist = false;
                comboBox1.Items.Add("No capture device on your system");
            }
        }


        //toggle start and stop button
        private void start_Click(object sender, EventArgs e)
        {
            if (start.Text == "&Start")
            {
                if (DeviceExist)
                {
                    videoSource = new VideoCaptureDevice(videoDevices[comboBox1.SelectedIndex].MonikerString);
                    videoSource.NewFrame += new NewFrameEventHandler(video_NewFrame);
                    CloseVideoSource();
                    //videoSource.DesiredFrameSize = new Size(160, 120);
                    //videoSource.DesiredFrameRate = 10;
                    videoSource.Start();
                    label2.Text = "Device running...";
                    start.Text = "&Stop";
                    //timer1.Enabled = true;
                    btnSnapshot.Enabled = true;
                }
                else
                {
                    label2.Text = "Error: No Device selected.";
                }
            }
            else
            {
                if (videoSource.IsRunning)
                {
                    //timer1.Enabled = false;
                    CloseVideoSource();
                    label2.Text = "Device stopped.";
                    start.Text = "&Start";
                    btnSnapshot.Enabled = false;
                }
            }
        }

        private Bitmap m_image;
        private bool takeFrame = false;

        //eventhandler if new frame is ready
        private void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            if (m_image != null)
            {
                m_image.Dispose();
            }
            m_image = (Bitmap)eventArgs.Frame.Clone();

            pictureBox1.Image = m_image;

            if (takeFrame)
            {
                takeFrame = false;
                // Save the frame to disk
                var filename = new FileInfo("frame_" + DateTime.Now.Ticks + ".png");
                m_image.Save(filename.FullName, ImageFormat.Png);
                label2.Invoke((Action)(() => label2.Text = filename.Name + " taken"));
                //label2.Text = filename.Name + " taken";
            }

        }

        //close the device safely
        private void CloseVideoSource()
        {
            if (!(videoSource == null))
                if (videoSource.IsRunning)
                {
                    videoSource.SignalToStop();
                    videoSource = null;
                }
        }

        ////get total received frame at 1 second tick
        private void timer1_Tick(object sender, EventArgs e)
        {
            //    label2.Text = "Device running... " + videoSource.FramesReceived.ToString() + " FPS";
            //    // takeFrame = true;
        }

        //prevent sudden close while device is running
        private void Form2_FormClosed(object sender, FormClosedEventArgs e)
        {
            CloseVideoSource();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            getCamList();
            //m_FrameScanner.ThresholdReached
        }

        private void btnSnapshot_Click(object sender, EventArgs e)
        {
            takeFrame = true;
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
            //this.Close();
        }

        private void btnCapture_Click(object sender, EventArgs e)
        {
            btnCapture.Enabled = false;
            m_FrameScanner = new FilmScanner.FrameScanner();
            //m_FrameScanner.ThresholdReached += c_ThresholdReached;
            var testss = new TestSprocketSensor();
            var testfs = new TestFilmSensor() { State = StateType.HIGH };
            for (int i = 0; i < 10; i++)
            {
                m_FrameScanner.SeekNextFrame(testfs, testss, new TimeSpan(0, 0, 6));
                this.takeFrame = true;

                //System.Threading.Thread.Sleep(800);
                while (this.takeFrame)
                {
                    Trace.WriteLine("waiting for capture");
                    Application.DoEvents();
                    System.Threading.Thread.Sleep(20);
                }
            }
            btnCapture.Enabled = true;
        }
    }

}