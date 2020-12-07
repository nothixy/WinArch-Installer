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
using SharpCompress.Readers;
using System;
using System.Collections;
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
        int numberoftasks = 6;
        int partitionsnumber = 0;
        double taskpercentage;
        double mixedpercentage = 0;
        Single spaceleft_mb;
        int packagesDone;
        string[] packages;
        string currentline;
        string biosmode;
        string kernelfile;
        string initramfsfile;
        string disktype;
        bool repartition;
        string volume;
        bool createExtended;
        public Slideshow()
        {
            InitializeComponent();
            biosmode = (string)Application.Current.Properties["biosmode"];
            spaceleft_mb = (Single)Application.Current.Properties["SpaceRequired"];
            repartition = (bool)Application.Current.Properties["Repartition"];
            volume = (string)Application.Current.Properties["Volume"];
            downloadcomponents();
        }
        public async Task downloadcomponents()
        {
            Task.Run(() => slideshow());
            //downloadLatestGrub();
            getDisksInfo(volume);
            //mountArchIso();
            //getPackageList();
            //downloadGentooStage4();
            //doOperations();
        }
        public void slideshow()
        {
            while (true)
            {
                System.Threading.Thread.Sleep(10000);
                this.Dispatcher.Invoke(() =>
                {
                    tabControl.SelectedIndex = (tabControl.SelectedIndex + 1) % tabControl.Items.Count;
                });
            }
        }
        public void getDisksInfo(string volume)
        {
            this.Dispatcher.Invoke(() =>
            {
                updateProgress(true, "Getting disks info", null);
            });
            if (biosmode == "BIOS")
            {
                Process process = new Process();
                process.Exited += (s, e) =>
                {
                    createExtended = Boolean.Parse(Regex.Replace(process.StandardOutput.ReadToEnd().ToLower(), "\\s", ""));
                    partitionDisks();
                };
                process.StartInfo.FileName = "powershell.exe";
                process.StartInfo.Arguments = "if ((Get-partition -DiskNumber ((Get-Partition -DriveLetter " + volume + ").DiskNumber)).PartitionNumber -contains 0) { echo False } else { echo True }";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;
                process.EnableRaisingEvents = true;
                process.Start();
                process.WaitForExit();
            }
            else
            {
                partitionDisks();
            }
        }
        public void partitionDisks()
        {
            updateProgress(true, "Formatting disks", null);
            string diskpartfile = Path.GetTempPath() + "diskpart.txt";
            if (File.Exists(diskpartfile))
            {
                File.Delete(diskpartfile);
            }
            if (biosmode == "BIOS")
            {
                if (repartition)
                {
                    if (createExtended)
                    {
                        string[] lines =
                        {
                            "select volume " + volume,
                            "shrink desired=" + spaceleft_mb + " minimum=2500",
                            "create partition extended",
                            "create partition logical",
                            "shrink desired=1500 minimum=1500",
                            "format unit=4096 fs=exfat quick label=Arch",
                            "create partition logical",
                            "format unit=4096 fs=fat32 quick label=PreArch",
                            "assign letter L",
                        };
                        File.WriteAllLines(diskpartfile, lines);
                    }
                    else
                    {
                        string[] lines =
                        {
                            "select volume " + volume,
                            "shrink desired=" + spaceleft_mb + " minimum=2500",
                            "shrink desired=1500 minimum=1500",
                            "format unit=4096 fs=exfat quick label=Arch",
                            "create partition logical",
                            "format unit=4096 fs=fat32 quick label=PreArch",
                            "assign letter L",
                        };
                        File.WriteAllLines(diskpartfile, lines);
                    }
                }
                else
                {
                    if (createExtended)
                    {
                        string[] lines =
                        {
                            "select volume " + volume,
                            "delete partition",
                            "create partition extended",
                            "create partition logical",
                            "shrink desired=1500 minimum=1500",
                            "format unit=4096 fs=exfat quick label=Arch",
                            "create partition logical",
                            "format unit=4096 fs=fat32 quick label=PreArch",
                            "assign letter L",
                        };
                        File.WriteAllLines(diskpartfile, lines);
                    }
                    else
                    {
                        string[] lines =
                        {
                            "select volume " + volume,
                            "shrink desired=1500 minimum=1500",
                            "format unit=4096 fs=exfat quick label=Arch",
                            "create partition logical",
                            "format unit=4096 fs=fat32 quick label=PreArch",
                            "assign letter L",
                        };
                        File.WriteAllLines(diskpartfile, lines);
                    }
                }
            }
            else
            {
                if (repartition)
                {
                    string[] lines =
                    {
                        "select volume " + volume,
                        "shrink desired=" + spaceleft_mb + " minimum=2500",
                        "create partition primary",
                        "shrink desired=1500 minimum=1500",
                        "format unit=4096 fs=exfat quick label=Arch",
                        "create partition primary",
                        "format unit=4096 fs=fat32 quick label=PreArch",
                        "assign letter L",
                    };
                    File.WriteAllLines(diskpartfile, lines);
                }
                else
                {
                    string[] lines =
                    {
                        "select volume " + volume,
                        "shrink desired=1500 minimum=1500",
                        "format unit=4096 fs=exfat quick label=Arch",
                        "create partition primary",
                        "format unit=4096 fs=fat32 quick label=PreArch",
                        "assign letter L",
                    };
                    File.WriteAllLines(diskpartfile, lines);
                }
            }
            Process process = new Process();
            process.Exited += (s, e) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    updateProgressFull(1);
                    downloadLatestGrub();
                });
            };
            process.StartInfo.FileName = "diskpart.exe";
            process.StartInfo.Arguments = "/s " + diskpartfile;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.EnableRaisingEvents = true;
            process.Start();
        }
        async Task downloadLatestGrub()
        {
            updateProgress(true, "Finding GRUB version to download", null);
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://ftp.gnu.org/gnu/grub/");
            request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            request.Credentials = new NetworkCredential("anonymous", "");
            WebResponse webResponse = await request.GetResponseAsync();
            FtpWebResponse response = (FtpWebResponse)webResponse;

            Stream responseStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream);
            string[] files = reader.ReadToEnd().Split('\n');
            reader.Close();
            response.Close();
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
            WebClient client = new WebClient();
            client.Credentials = new NetworkCredential("anonymous", "");
            client.DownloadProgressChanged += (s, e) =>
            {
                updateProgress(false, "Downloading GRUB file", e.ProgressPercentage);
            };
            client.DownloadFileCompleted += (s, e) =>
            {
                updateProgressFull(2);
                downloadGentooStage4();
            };
            Uri todownload = new Uri("ftp://ftp.gnu.org/gnu/grub/" + filenamemaster);
            client.DownloadFileAsync(todownload, Path.GetTempPath() + "grub.zip");
        }
        public void downloadGentooStage4()
        {
            updateProgress(true, "Finding Gentoo version to download", null);
            WebClient client = new WebClient();
            Stream stream = client.OpenRead("http://distfiles.gentoo.org/releases/amd64/autobuilds/latest-stage4-amd64-minimal.txt");
            StreamReader reader = new StreamReader(stream);
            while (reader.Peek() >= 0)
            {
                currentline = reader.ReadLine();
                if (Regex.IsMatch(currentline, ".*\\/stage4-amd64-minimal-.*\\.tar\\.xz.*"))
                {
                    string downloadlink = "http://distfiles.gentoo.org/releases/amd64/autobuilds/" + Regex.Replace(currentline, "\\s.*", "");
                    Uri uri = new Uri(downloadlink);
                    WebClient client2 = new WebClient();
                    client2.DownloadProgressChanged += (s, e) =>
                    {
                        updateProgress(false, "Downloading Gentoo stage4 tarball", e.ProgressPercentage);
                    };
                    client2.DownloadFileCompleted += (s, e) =>
                    {
                        updateProgressFull(3);
                        Task.Run(() =>
                        {
                            extractArchive();
                        });
                    };
                    client2.DownloadFileAsync(uri, Path.GetTempPath() + "gentoo.tar.xz");
                }
            }
        }
        public void extractArchive()
        {
            this.Dispatcher.Invoke(() =>
            {
                updateProgress(true, "Extracting Gentoo archive", null);
            });
            using (Stream stream = File.OpenRead(Path.GetTempPath() + "gentoo.tar.xz"))
            using (var reader = ReaderFactory.Open(stream))
            {
                while (reader.MoveToNextEntry())
                {
                    if (!reader.Entry.IsDirectory)
                    {
                        if (Regex.IsMatch(reader.Entry.Key, "\\./boot/vmlinuz-.*"))
                        {
                            kernelfile = Regex.Replace(reader.Entry.Key, "\\.", "");
                        }
                        if (Regex.IsMatch(reader.Entry.Key, "\\./boot/initramfs-.*\\.img"))
                        {
                            initramfsfile = Regex.Replace(reader.Entry.Key, "\\.", "");
                        }
                        /*try
                        {
                            reader.WriteEntryToDirectory(@"L:/", new ExtractionOptions()
                            {
                                ExtractFullPath = true,
                                Overwrite = true
                            });
                        }
                        catch (UnauthorizedAccessException) { }*/
                    }
                }
            }
            updateProgressFull(4);
            installGrub();
        }
        async Task installGrub()
        {
            updateProgress(true, "Installing GRUB", null);
            ZipArchive archive = new ZipArchive(File.OpenRead(Path.GetTempPath() + "grub.zip"));
            if (Directory.Exists(Path.GetTempPath() + "grub"))
            {
                Directory.Delete(Path.GetTempPath() + "grub");
            }
            Directory.CreateDirectory(Path.GetTempPath() + "grub");
            archive.ExtractToDirectory(Path.GetTempPath() + "grub");
            if (biosmode == "BIOS")
            {
                Process process = new Process();
                process.Exited += (s, e) =>
                {

                };
                process.StartInfo.FileName = Path.GetTempPath() + "grub\\grub-install.exe";
                process.StartInfo.Arguments = "--boot-directory=L:\\boot --target=i386-pc //./PHYSICALDRIVE0";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;
                process.EnableRaisingEvents = true;
                process.Start();
            }
            else
            {
                Process process = new Process();
                process.Exited += (s, e) =>
                {

                };
                process.StartInfo.FileName = Path.GetTempPath() + "grub\\grub-install.exe";
                process.StartInfo.Arguments = "--boot-directory=L:\\boot --target=x86_64-efi --removable --efi-directory=L:\\boot\\EFI";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;
                process.EnableRaisingEvents = true;
                process.Start();
            }
            if (biosmode == "BIOS")
            {
                disktype = "msdos";
            }
            else
            {
                disktype = "gpt";
            }
            string[] lines = {

                "menuentry 'Linux' {",
                "insmod exfat",
                "set root=(hd0," + disktype + "5)",
                "linux " + kernelfile + "root=LABEL=preArch",
                "initrd " + initramfsfile + "",
                "}",
                "menuentry 'Windows 10' {",
                "insmod ntfs",
                "set root=(hd0," + disktype + "1)",
                "ntldr /bootmgr",
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
            File.WriteAllLines("L:\\boot\\grub\\grub.cfg", lines);
            updateProgressFull(5);
            //setupSystem();
        }
        public void setupSystem()
        {
            updateProgress(true, "Installing configuration files", null);
            foreach (DictionaryEntry de in Application.Current.Properties)
            {
                using (StreamWriter sw = File.AppendText("L:\\config.txt"))
                {
                    sw.WriteLine(de.Key + "=" + de.Value);
                }
            }
            string[] lines =
            {
                "#!/bin/bash",
                ". /config.txt",
                "mount /dev/disk/by-label/Arch /mnt",
                "echo 'Please plug in your ethernet cable'",
                "net-setup",
                "pacstrap /mnt linux base networkmanager bluez packagekit-qt5 gnome-software-packagekit-plugin",
                "cp /config.txt /mnt/config.txt",
                "reboot"
            };
            File.WriteAllLines("L:\\initscript", lines);
            updateProgressFull(6);
            updateProgress(false, "Done", 100);
            ButtonNext.IsEnabled = true;
        }
        public void updateProgress(bool indeterminate, string currentAction, double? currentPercentage)
        {
            progressCurrent.IsIndeterminate = indeterminate;
            this.Dispatcher.Invoke(() =>
            {
                if (!indeterminate)
                {
                    progressCurrent.Value = (double)currentPercentage;
                }
                textBlock.Text = currentAction;
            });
        }
        public void updateProgressFull(float currentPercentage)
        {
            taskpercentage = (currentPercentage * 100) / 6;
            this.Dispatcher.Invoke(() =>
            {
                progressTotal.Value = taskpercentage;
            });
        }
    }
}
