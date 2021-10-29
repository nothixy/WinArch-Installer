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
using System.IO.Compression;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Path = System.IO.Path;

namespace WinArch
{
    /// <summary>
    /// Interaction logic for Slideshow.xaml
    /// </summary>
    public partial class Slideshow : Page
    {
        private double taskpercentage;
        private readonly float spaceleft_mb;
        private readonly string biosmode;
        private readonly bool repartition;
        private readonly string volume;
        private readonly string hostname;
        private readonly string language;
        private readonly string keymap;
        private readonly string timezone;
        private readonly string password;
        private readonly string uname;
        private readonly string unameSys;
        private readonly string desktop;
        private bool AutoScroll = true;
        public Slideshow()
        {
            InitializeComponent();
            biosmode = (string)Application.Current.Properties["biosmode"];
            spaceleft_mb = (float)Application.Current.Properties["SpaceRequired"];
            repartition = (bool)Application.Current.Properties["Repartition"];
            volume = (string)Application.Current.Properties["Volume"];
            hostname = (string)Application.Current.Properties["Hostname"];
            language = (string)Application.Current.Properties["Language"];
            keymap = (string)Application.Current.Properties["Keymap"];
            timezone = (string)Application.Current.Properties["Timezone"];
            password = (string)Application.Current.Properties["Password"];
            uname = (string)Application.Current.Properties["Uname"];
            unameSys = (string)Application.Current.Properties["UnameSys"];
            desktop = (string)Application.Current.Properties["Desktop"];
            Downloadcomponents();
        }
        public void Downloadcomponents()
        {
            _ = Task.Run(() => DoSlideshow());
            PartitionDisks();
        }
        public void DoSlideshow()
        {
            while (tabControl.SelectedIndex != tabControl.Items.Count - 1)
            {
                System.Threading.Thread.Sleep(10000);
                Dispatcher.Invoke(() =>
                {
                    tabControl.SelectedIndex = (tabControl.SelectedIndex + 1) % (tabControl.Items.Count - 1);
                });
            }
        }
        public void PartitionDisks()
        {
            UpdateProgress(true, "Formatting disks", null);
            string diskpartfile = Path.GetTempPath() + "diskpart.txt";
            if (File.Exists(diskpartfile))
            {
                File.Delete(diskpartfile);
            }
            if (repartition)
            {
                string[] lines =
                {
                    "select volume " + volume,
                    "shrink desired=" + spaceleft_mb + " minimum=2500",
                    "create partition primary",
                    "format unit=4096 fs=fat32 quick label=ARCH",
                    "assign letter L",
                };
                File.WriteAllLines(diskpartfile, lines);
            }
            else
            {
                string[] lines =
                {
                    "select volume " + volume,
                    "delete volume",
                    "create partition primary",
                    "format unit=4096 fs=fat32 quick label=ARCH",
                    "assign letter L",
                };
                File.WriteAllLines(diskpartfile, lines);
            }
            Process process = new();
            process.Exited += (s, e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    logText.Text += "------Running Diskpart------\n";
                    logText.Text += "Arguments : " + process.StartInfo.Arguments + "\n";
                    logText.Text += process.StandardOutput.ReadToEnd() + "\n";
                    UpdateProgressFull(1);
                    _ = DownloadLatestGrub();
                });
            };
            process.StartInfo.FileName = "diskpart.exe";
            process.StartInfo.Arguments = "/s " + diskpartfile;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.EnableRaisingEvents = true;
            _ = process.Start();
        }

        private async Task DownloadLatestGrub()
        {
            UpdateProgress(true, "Finding GRUB version to download", null);
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://ftp.gnu.org/gnu/grub/");
            request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            request.Credentials = new NetworkCredential("anonymous", "");
            WebResponse webResponse = await request.GetResponseAsync();
            FtpWebResponse response = (FtpWebResponse)webResponse;
            Stream responseStream = response.GetResponseStream();
            StreamReader reader = new(responseStream);
            string[] files = reader.ReadToEnd().Split('\n');
            reader.Close();
            response.Close();
            List<string> filenames = new();
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
                    if (string.Compare(filenamemaster, filenames[i]).ToString() == "-1")
                    {
                        filenamemaster = filenames[i];
                    }
                }
            }
            WebClient client = new();
            client.Credentials = new NetworkCredential("anonymous", "");
            client.DownloadProgressChanged += (s, e) =>
            {
                UpdateProgress(false, "Downloading GRUB file", e.ProgressPercentage);
            };
            client.DownloadFileCompleted += (s, e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    logText.Text += "Downloaded\n";
                });
                UpdateProgressFull(2);
                DownloadArchIso();
            };
            Uri todownload = new("ftp://ftp.gnu.org/gnu/grub/" + filenamemaster);
            Dispatcher.Invoke(() =>
            {
                logText.Text += "------Downloading GRUB------\n";
                logText.Text += "Downloading " + todownload.AbsoluteUri + " as grub.zip" + "\n";
            });
            client.DownloadFileAsync(todownload, Path.GetTempPath() + "grub.zip");
        }
        public void DownloadArchIso()
        {
            WebClient client2 = new();
            client2.DownloadProgressChanged += (s, e) =>
            {
                UpdateProgress(false, "Downloading Archlinux iso", e.ProgressPercentage);
            };
            client2.DownloadFileCompleted += (s, e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    logText.Text += "Downloaded\n";
                });
                UpdateProgressFull(3);
                MountArchIso();
            };
            Uri download = new("https://sourceforge.net/projects/systemrescuecd/files/latest/download");
            Dispatcher.Invoke(() =>
            {
                logText.Text += "------Downloading Archlinux------\n";
                logText.Text += "Downloading " + download.AbsoluteUri + " as arch.iso\n";
            });
            client2.DownloadFileAsync(download, Path.GetTempPath() + "arch.iso");
        }
        public void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            if (source.FullName.ToLower() == target.FullName.ToLower())
            {
                return;
            }
            if (Directory.Exists(target.FullName) == false)
            {
                _ = Directory.CreateDirectory(target.FullName);
            }
            foreach (FileInfo fi in source.GetFiles())
            {
                Dispatcher.Invoke(() =>
                {
                    logText.Text += "Copying " + source.FullName + "\\" + fi.Name + "\n";
                });
                _ = fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);
            }
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }
        public void MountArchIso()
        {
            UpdateProgress(true, "Copying Archlinux files to the new partition", null);
            Dispatcher.Invoke(() =>
            {
                logText.Text += "------Copying Archlinux installation files------\n";
            });
            Process process = new();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.EnableRaisingEvents = true;
            process.StartInfo.FileName = "powershell.exe";
            process.StartInfo.Arguments = "mountvol.exe U: (Mount-DiskImage -ImagePath " + Path.GetTempPath() + "arch.iso -NoDriveLetter | Get-Volume).UniqueId";
            process.Exited += (s, e) =>
            {
                CopyAll(new DirectoryInfo(@"U:\sysresccd\"), new DirectoryInfo(@"L:\sysresccd\"));
                UpdateProgressFull(4);
                InstallGrub();
            };
            _ = process.Start();
        }
        public static void Mountefi()
        {
            Process process = new();
            process.StartInfo.FileName = "powershell.exe";
            process.StartInfo.Arguments = "mountvol.exe Z: /S";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.EnableRaisingEvents = true;
            _ = process.Start();
            process.WaitForExit();
            Debug.WriteLine(process.StandardOutput.ReadToEnd());
            Debug.WriteLine(process.StandardError.ReadToEnd());
        }
        private void InstallGrub()
        {
            Dispatcher.Invoke(() =>
            {
                logText.Text += "------Installing GRUB------\n";
                UpdateProgress(true, "Installing GRUB", null);
            });
            ZipArchive archive = new(File.OpenRead(Path.GetTempPath() + "grub.zip"));
            if (Directory.Exists(Path.GetTempPath() + "grub"))
            {
                Directory.Delete(Path.GetTempPath() + "grub", true);
            }
            _ = Directory.CreateDirectory(Path.GetTempPath() + "grub");
            archive.ExtractToDirectory(Path.GetTempPath() + "grub");
            string[] dirs = Directory.GetDirectories(Path.GetTempPath() + "grub");
            Process process = new();
            Debug.WriteLine("PATH : " + @dirs[0] + @"\grub-install.exe");
            process.StartInfo.FileName = @dirs[0] + @"\grub-install.exe";
            if (biosmode == "BIOS")
            {
                process.StartInfo.Arguments = "--boot-directory=L:\\boot --target=i386-pc //./PHYSICALDRIVE0";
            }
            else
            {
                Mountefi();
                process.StartInfo.Arguments = "--bootloader-id=GRUB --boot-directory=Z:\\ --target=x86_64-efi --efi-directory=Z:\\";
            }
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.EnableRaisingEvents = true;
            process.Exited += (s, e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    logText.Text += process.StandardOutput + "\n";
                });
                string[] lines = {
                "menuentry 'SystemRescue' {",
                "insmod gzio",
                "insmod part_gpt",
                "insmod part_msdos",
                "insmod exfat",
                "search --no-floppy --label ARCH --set=root",
                "linux /sysresccd/boot/x86_64/vmlinuz archisobasedir=sysresccd archisolabel=ARCH copytoram setkmap=us ar_nowait ar_nofail",
                "initrd /sysresccd/boot/x86_64/sysresccd.img",
                "}",
                "if [\"x${timeout}\" != \"x-1\"]; then",
                "if keystatus; then",
                "if keystatus --shift; then",
                "set timeout = -1",
                "else",
                "set timeout = 0",
                "fi",
                "else",
                "if sleep--interruptible 3; then",
                "set timeout = 0",
                "fi",
                "fi",
                "fi",
                };
                if (!Directory.Exists(@"Z:\grub"))
                {
                    _ = Directory.CreateDirectory(@"Z:\grub");
                }
                File.WriteAllLines(@"Z:\grub\grub.cfg", lines);
                UpdateProgressFull(5);
                SetupSystem();
            };
            _ = process.Start();
        }
        public void SetupSystem()
        {
            Dispatcher.Invoke(() =>
            {
                UpdateProgress(true, "Preparing Linux autorun file", null);
            });
            StreamReader setupfile = new(GetType().Assembly.GetManifestResourceStream("WinArch.Resources.autorun"));
            List<string> list = new();
            string line;
            while ((line = setupfile.ReadLine()) != null)
            {
                list.Add(line);
            }
            string[] autorun = list.ToArray();
            string[] lines =
            {
                "#!/bin/sh",
                "desktop=" + desktop,
                "hostname=" + hostname,
                "language=\"" + language + "\"",
                "keymap=" + keymap,
                "timezone=" + timezone,
                "password=" + password,
                "uname=" + uname,
                "unamesys=" + unameSys,
            };
            File.WriteAllLines(@"L:\autorun", lines);
            File.AppendAllLines(@"L:\autorun", autorun);
            Dispatcher.Invoke(() =>
            {
                logText.Text += "------Creating linux autorun file------\n";
                logText.Text += File.ReadAllText(@"L:\autorun") + "\n";
            });
            UpdateProgressFull(6);
            Cleanup();
        }
        public void Cleanup()
        {
            Dispatcher.Invoke(() =>
            {
                UpdateProgress(true, "Cleaning things up", null);
            });
            Process.Start("mountvol.exe", "Z: /D");
            Process.Start("mountvol.exe", "L: /D");
            Process.Start("mountvol.exe", "U: /D");
            Dispatcher.Invoke(() =>
            {
                logText.Text += "------Unmounted ESP, Linux Partition and Archiso------\n";
            });
            Process.Start("reg.exe", "ADD \"HKLM\\SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Power\" /V HiberbootEnabled /T REG_dWORD /D 1 /F");
            Process.Start("powercfg.exe", "/H off");
            Dispatcher.Invoke(() =>
            {
                logText.Text += "------Disabled Fast Startup and Hibernation------\n";
            });
            UpdateProgressFull(7);
            Dispatcher.Invoke(() =>
            {
                _ = NavigationService.Navigate(new Uri("Finish.xaml", UriKind.Relative));
            });
        }
        public void UpdateProgress(bool indeterminate, string currentAction, double? currentPercentage)
        {
            progressCurrent.IsIndeterminate = indeterminate;
            Dispatcher.Invoke(() =>
            {
                if (!indeterminate)
                {
                    progressCurrent.Value = (double)currentPercentage;
                }
                textBlock.Text = currentAction;
            });
        }
        public void UpdateProgressFull(float currentPercentage)
        {
            taskpercentage = currentPercentage * 100 / 7;
            Dispatcher.Invoke(() =>
            {
                progressTotal.Value = taskpercentage;
            });
        }

        private void ScrollViewer_ScrollChanged(Object sender, ScrollChangedEventArgs e)
        {
            // User scroll event : set or unset auto-scroll mode
            if (e.ExtentHeightChange == 0)
            {   // Content unchanged : user scroll event
                if (Scroller.VerticalOffset == Scroller.ScrollableHeight)
                {   // Scroll bar is in bottom
                    // Set auto-scroll mode
                    AutoScroll = true;
                }
                else
                {   // Scroll bar isn't in bottom
                    // Unset auto-scroll mode
                    AutoScroll = false;
                }
            }

            // Content scroll event : auto-scroll eventually
            if (AutoScroll && e.ExtentHeightChange != 0)
            {   // Content changed and auto-scroll mode set
                // Autoscroll
                Scroller.ScrollToVerticalOffset(Scroller.ExtentHeight);
            }
        }
    }
}
