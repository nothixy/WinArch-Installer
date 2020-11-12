/*    WinArch installer - a Windows executable to install Archlinux on your PC
    Copyright (C) 2020  srgoti

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Management;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
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
using Path = System.IO.Path;

namespace WinArch
{
    /// <summary>
    /// Interaction logic for Slideshow.xaml
    /// </summary>
    public partial class Slideshow : Page
    {
        int numberoftasks = 6;
        double taskpercentage;
        public Slideshow()
        {
            InitializeComponent();
            downloadcomponents();
        }
        public async Task downloadcomponents()
        {
            //downloadLatestGrub();
            doOperations();
        }

        public void updateProgress(bool indeterminate, string currentAction, double? currentPercentage)
        {
            progressCurrent.IsIndeterminate = indeterminate;
            if (!indeterminate)
            {
                progressCurrent.Value = (double)currentPercentage;
            }
            textBlock.Text = currentAction;
        }
        public void updateProgressFull(int currentPercentage)
        {
            taskpercentage = (currentPercentage * 100) / 6;
            progressTotal.Value = taskpercentage;
        }
        async Task downloadCloverBootloader()
        {
            Uri downloadurl = null;
            logs.Text = logs.Text + "\nTrying to find Clover version to download";
            scroller.ScrollToBottom();
            updateProgress(true, "Finding Clover version to download", null);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.github.com/repos/CloverHackyColor/CloverBootloader/releases/latest");
            request.Headers["Accept"] = "application/vnd.github.v3+json";
            request.Headers["User-Agent"] = "request";
            request.Method = "GET";
            WebResponse responsetest = await request.GetResponseAsync();
            HttpWebResponse response = (HttpWebResponse)responsetest;
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responsejson = reader.ReadToEnd();
            reader.Close();
            dataStream.Close();
            var root = JsonSerializer.Deserialize<DocumentRoot>(responsejson);
            foreach (var obj in root.assets)
            {
                if (Regex.IsMatch(obj.name, "CloverV2-[0-9]{4}.zip"))
                {
                    downloadurl = new Uri(obj.browser_download_url);
                }
            }
            logs.Text = logs.Text + "\nDownloading file " + downloadurl.AbsoluteUri;
            scroller.ScrollToBottom();
            WebClient client = new WebClient();
            client.DownloadProgressChanged += (s, e) =>
            {
                updateProgress(false, "Download Clover zip file", e.ProgressPercentage);
            };
            client.DownloadFileCompleted += (s, e) =>
            {
                logs.Text = logs.Text + "\nDownloaded to " + Path.GetTempPath() + "clover.zip";
                scroller.ScrollToBottom();
                updateProgressFull(3);
                //doOperations();
            };
            client.DownloadFileAsync(downloadurl, Path.GetTempPath() + "clover.zip");
        }
        public class DocumentRoot
        {
            public List<assetslist> assets { get; set; }
        }
        public class assetslist
        {
            public string name { get; set; }
            public string browser_download_url { get; set; }
        }
        public void downloadArchBootstrap()
        {
            logs.Text = logs.Text + "\nReading md5s file http://mirrors.evowise.com/archlinux/iso/latest/md5sums.txt";
            scroller.ScrollToBottom();
            updateProgress(true, "Finding Archlinux archive to download", null);
            WebClient client = new WebClient();
            Stream test = client.OpenRead("http://mirrors.evowise.com/archlinux/iso/latest/md5sums.txt");
            StreamReader sr = new StreamReader(test);
            string line;
            string filetodownload = null;
            while ((line = sr.ReadLine()) != null)
            {
                if (Regex.IsMatch(line, "[0-9a-f]{32}\\s{2}archlinux-bootstrap.*"))
                {
                    filetodownload = Regex.Replace(line, "[0-9a-f]{32}\\s{2}", "");
                }
            }
            logs.Text = logs.Text + "\nFound filename to download : http://mirrors.evowise.com/archlinux/iso/latest/" + filetodownload + ", downloading";
            scroller.ScrollToBottom();
            WebClient client2 = new WebClient();
            Uri todownload = new Uri("http://mirrors.evowise.com/archlinux/iso/latest/" + filetodownload);
            client2.DownloadProgressChanged += (s, e) =>
            {
                updateProgress(false, "Download Archlinux bootstrap archive", e.ProgressPercentage);
            };
            client2.DownloadFileCompleted += (s, e) =>
            {
                logs.Text = logs.Text + "\nDownloaded to " + Path.GetTempPath() + "arch.tar.gz";
                scroller.ScrollToBottom();
                updateProgressFull(2);
                downloadCloverBootloader();
            };
            client2.DownloadFileAsync(todownload, Path.GetTempPath() + "arch.tar.gz");
        }
        async Task downloadLatestGrub()
        {
            logs.Text = logs.Text + "\nQuerying GRUB ftp server for versions";
            scroller.ScrollToBottom();
            updateProgress(true, "Finding GRUB version to download", null);
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://ftp.gnu.org/gnu/grub/");
            request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            request.Credentials = new NetworkCredential("anonymous", "");
            WebResponse webResponse = await request.GetResponseAsync();
            FtpWebResponse response = (FtpWebResponse)webResponse;
            
            Stream responseStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream);
            string[] files = reader.ReadToEnd().Split('\n');
            logs.Text = logs.Text + "\nGot response";
            scroller.ScrollToBottom();
            reader.Close();
            response.Close();
            logs.Text = logs.Text + "\nFinding version to download";
            scroller.ScrollToBottom();
            List<string> filenames = new List<string>();
            for (int i = 0; i < (files.Length - 1); i++)
            {
                if (Regex.IsMatch(Regex.Replace(Regex.Replace(files[i], ".*\\sgrub-", "grub-"), "\\s*", ""), "grub-[0-9\\.]*-for-windows.zip(?!\\.sig)"))
                {
                    filenames.Add(Regex.Replace(Regex.Replace(files[i], ".*\\sgrub-", "grub-"), "\\s*", ""));
                }
            }
            string filenamemaster = filenames[0];
            if (filenames.Count > 1)
            {
                for (int i = 1; i < filenames.Count; i++)
                {
                    if (String.Compare(filenamemaster, filenames[i]).ToString() == "-1")
                    {
                        filenamemaster = filenames[i];
                    }
                }
            }
            logs.Text = logs.Text + "\nFound version " + filenamemaster;
            scroller.ScrollToBottom();
            WebClient client = new WebClient();
            client.Credentials = new NetworkCredential("anonymous", "");
            client.DownloadProgressChanged += (s, e) =>
            {
                updateProgress(false, "Downloading GRUB file", e.ProgressPercentage);
            };
            client.DownloadFileCompleted += (s, e) =>
            {
                logs.Text = logs.Text + "\nDownloaded file to " + Path.GetTempPath() + "grub.zip";
                scroller.ScrollToBottom();
                updateProgressFull(1);
                downloadArchBootstrap();
            };
            Uri todownload = new Uri("ftp://ftp.gnu.org/gnu/grub/" + filenamemaster);
            client.DownloadFileAsync(todownload, Path.GetTempPath() + "grub.zip");
            logs.Text = logs.Text + "\nDownloading file ftp://ftp.gnu.org/gnu/grub/" + filenamemaster;
            scroller.ScrollToBottom();
        }
        public void doOperations()
        {
            Process process = new Process();
            process.StartInfo.FileName = "powershell.exe";
            process.StartInfo.Arguments = "-Command echo $(Get-ComputerInfo).BiosFirmwareType";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            process.WaitForExit();
            string errorOutput = process.StandardOutput.ReadToEnd();
            if (Regex.Replace(errorOutput, "\\s", "") == "Bios")
            {
                Debug.WriteLine("Loser legacy");
            }
            else
            {
                Debug.WriteLine("Bogoss");
            }
        }
    }
}
