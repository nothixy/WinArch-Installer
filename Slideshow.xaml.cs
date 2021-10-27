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
        double taskpercentage;
        readonly float spaceleft_mb;
        readonly string biosmode;
        readonly bool repartition;
        readonly string volume;
        bool createExtended;
        readonly string hostname;
        readonly string language;
        readonly string keymap;
        readonly string timezone;
        readonly string password;
        readonly string uname;
        readonly string unameSys;
        readonly string desktop;
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
            GetDisksInfo(volume);
            //SetupSystem();
        }
        public void DoSlideshow()
        {
            while (true)
            {
                System.Threading.Thread.Sleep(10000);
                Dispatcher.Invoke(() =>
                {
                    tabControl.SelectedIndex = (tabControl.SelectedIndex + 1) % tabControl.Items.Count;
                });
            }
        }
        public void GetDisksInfo(string volume)
        {
            Dispatcher.Invoke(() =>
            {
                UpdateProgress(true, "Getting disks info", null);
            });
            if (biosmode == "BIOS")
            {
                Process process = new Process();
                process.Exited += (s, e) =>
                {
                    createExtended = bool.Parse(Regex.Replace(process.StandardOutput.ReadToEnd().ToLower(), "\\s", ""));
                    PartitionDisks();
                };
                process.StartInfo.FileName = "powershell.exe";
                process.StartInfo.Arguments = "if ((Get-partition -DiskNumber ((Get-Partition -DriveLetter " + volume + ").DiskNumber)).PartitionNumber -contains 0) { echo False } else { echo True }";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;
                process.EnableRaisingEvents = true;
                _ = process.Start();
                process.WaitForExit();
            }
            else
            {
                PartitionDisks();
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
                Dispatcher.Invoke(() =>
                {
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
            process.Start();
        }
        async Task DownloadLatestGrub()
        {
            UpdateProgress(true, "Finding GRUB version to download", null);
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
                    if (string.Compare(filenamemaster, filenames[i]).ToString() == "-1")
                    {
                        filenamemaster = filenames[i];
                    }
                }
            }
            WebClient client = new WebClient();
            client.Credentials = new NetworkCredential("anonymous", "");
            client.DownloadProgressChanged += (s, e) =>
            {
                UpdateProgress(false, "Downloading GRUB file", e.ProgressPercentage);
            };
            client.DownloadFileCompleted += (s, e) =>
            {
                UpdateProgressFull(2);
                DownloadArchIso();
            };
            Uri todownload = new Uri("ftp://ftp.gnu.org/gnu/grub/" + filenamemaster);
            client.DownloadFileAsync(todownload, Path.GetTempPath() + "grub.zip");
        }
        public void DownloadArchIso()
        {
            WebClient client2 = new WebClient();
            client2.DownloadProgressChanged += (s, e) =>
            {
                UpdateProgress(false, "Downloading Archlinux iso", e.ProgressPercentage);
            };
            client2.DownloadFileCompleted += (s, e) =>
            {
                UpdateProgressFull(3);
                MountArchIso();
            };
            Uri download = new Uri("https://sourceforge.net/projects/systemrescuecd/files/latest/download");
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
                Directory.CreateDirectory(target.FullName);
            }
            foreach (FileInfo fi in source.GetFiles())
            {
                Debug.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
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
            Process process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.FileName = "powershell.exe";
            process.StartInfo.Arguments = "mountvol.exe U: (Mount-DiskImage -ImagePath " + Path.GetTempPath() + "arch.iso -NoDriveLetter | Get-Volume).UniqueId";
            _ = process.Start();
            process.WaitForExit();
            DirectoryInfo dI = new DirectoryInfo(@"U:\sysresccd\");
            int i = 0;
            foreach (FileInfo file in dI.EnumerateFiles())
            {
                i++;
            }
            CopyAll(new DirectoryInfo(@"U:\sysresccd\"), new DirectoryInfo(@"L:\sysresccd\"));
            UpdateProgressFull(4);
            InstallGrub();
        }
        public void mount_efi()
        {
            Process process = new Process();
            process.StartInfo.FileName = "powershell.exe";
            process.StartInfo.Arguments = "mountvol.exe Z: ((get-partition -DiskNumber ((Get-Partition -DriveLetter L).DiskNumber)) | where-object {$_.GptType -eq \"{c12a7328-f81f-11d2-ba4b-00a0c93ec93b}\"}).AccessPaths[-1]";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.EnableRaisingEvents = true;
            process.Start();
            process.WaitForExit();
        }

        private void InstallGrub()
        {
            UpdateProgress(true, "Installing GRUB", null);
            ZipArchive archive = new ZipArchive(File.OpenRead(Path.GetTempPath() + "grub.zip"));
            if (Directory.Exists(Path.GetTempPath() + "grub"))
            {
                Directory.Delete(Path.GetTempPath() + "grub", true);
            }
            Directory.CreateDirectory(Path.GetTempPath() + "grub");
            archive.ExtractToDirectory(Path.GetTempPath() + "grub");
            string[] dirs = Directory.GetDirectories(Path.GetTempPath() + "grub");
            Process process = new Process();
            Debug.WriteLine("PATH : " + @dirs[0] + @"\grub-install.exe");
            process.StartInfo.FileName = @dirs[0] + @"\grub-install.exe";
            if (biosmode == "BIOS")
            {
                process.StartInfo.Arguments = "--boot-directory=L:\\boot --target=i386-pc //./PHYSICALDRIVE0";

            }
            else
            {
                mount_efi();
                process.StartInfo.Arguments = "--boot-directory=L:\\boot --target=x86_64-efi --removable --efi-directory=Z:\\";
            }
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.EnableRaisingEvents = true;
            process.Start();
            process.WaitForExit();
            string[] lines = {
                "menuentry 'SystenRescue' {",
                "insmod gzio",
                "insmod part_gpt",
                "insmod part_msdos",
                "insmod exfat",
                "search --no-floppy --label PREARCH --set=root",
                "linux /sysresccd/boot/x86_64/vmlinuz archisobasedir=sysresccd archisolabel=PREARCH copytoram setkmap=us ar_nowait",
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
            File.WriteAllLines(@"L:\boot\grub\grub.cfg", lines);
            if (biosmode != "BIOS")
            {
                Process process1 = new Process();
                process1.StartInfo.FileName = "powershell.exe";
                process1.StartInfo.Arguments = "$newguid = bcdedit.exe /create /d \"GRUB\"; bcdedit.exe /set "
            }
            UpdateProgressFull(5);
            SetupSystem();
        }
        public void SetupSystem()
        {
            UpdateProgress(true, "Preparing Linux autorun file", null);
            StreamReader setupfile = new StreamReader(GetType().Assembly.GetManifestResourceStream("WinArch.Resources.autorun"));
            List<string> list = new List<string>();
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
            UpdateProgressFull(6);
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
            taskpercentage = currentPercentage * 100 / 6;
            Dispatcher.Invoke(() =>
            {
                progressTotal.Value = taskpercentage;
            });
        }
    }
}
