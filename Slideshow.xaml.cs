﻿using System;
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
using Microsoft.Win32;

namespace WinArch {
	/// <summary>
	/// Interaction logic for Slideshow.xaml
	/// </summary>
	public partial class Slideshow : Page {
		private double taskpercentage;
		private readonly float spaceleft_mb;
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
		public Slideshow() {
			InitializeComponent();
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
		public void Downloadcomponents() {
			_ = Task.Run(() => DoSlideshow());
			PartitionDisks();
		}

		public void Move() {
			System.Threading.Thread.Sleep(10000);
			Dispatcher.Invoke(() => {
				// If we are on the log page, don't move from it
				if (tabControl.SelectedIndex != tabControl.Items.Count - 1) {
					tabControl.SelectedIndex = (tabControl.SelectedIndex + 1) % (tabControl.Items.Count - 1);
				}
			});
		}

		// Every 10 seconds, change the image
		public void DoSlideshow() {
			while (true) {
				Move();
			}
		}

		// Format and create required volumes
		public void PartitionDisks() {
			UpdateProgress(true, "Formatting disks", null);
			// Create a text file containing all the instructions diskpart will execute
			string diskpartfile = Path.GetTempPath() + "diskpart.txt";
			if (File.Exists(diskpartfile)) {
				File.Delete(diskpartfile);
			}
			if (repartition) {
				string[] lines = [
					"select volume " + volume,
					"shrink desired=" + spaceleft_mb + " minimum=2500",
					"create partition primary",
					"format unit=4096 fs=exfat quick label=ARCH",
					"assign letter L",
				];
				File.WriteAllLines(diskpartfile, lines);
			} else {
				string[] lines = [
					"select volume " + volume,
					"delete volume",
					"create partition primary",
					"format unit=4096 fs=exfat quick label=ARCH",
					"assign letter L",
				];
				File.WriteAllLines(diskpartfile, lines);
			}
			Process process = new();
			// Once diskpart finished, go to next step
			process.Exited += (s, e) => {
				Dispatcher.Invoke(() => {
					logText.Text += "------Running Diskpart------\n";
					logText.Text += "Arguments : " + process.StartInfo.Arguments + "\n";
					logText.Text += process.StandardOutput.ReadToEnd() + "\n";
					UpdateProgressFull(1);
					DownloadLatestGrub();
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

		// Download GRUB from GNU's FTP server
		private async Task DownloadLatestGrub() {
			UpdateProgress(true, "Finding GRUB version to download", null);
			FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://ftp.gnu.org/gnu/grub/");
			request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
			request.Credentials = new NetworkCredential("anonymous", "");
			WebResponse webResponse = await request.GetResponseAsync();
			// Convert webResponse to a FtpWebResponse
			FtpWebResponse response = (FtpWebResponse)webResponse;
			Stream responseStream = response.GetResponseStream();
			StreamReader reader = new(responseStream);
			string[] files = reader.ReadToEnd().Split('\n');
			reader.Close();
			response.Close();
			List<string> filenames = new();
			for (int i = 0; i < (files.Length - 1); i++) {
				// Find all files that match grub*for-windows.zip
				if (Regex.IsMatch(Regex.Replace(Regex.Replace(files[i], ".*\\sgrub-", "grub-"), "\\s*", ""), "grub-[0-9\\.]*-for-windows.zip(?!\\.sig)"))
				{
					filenames.Add(Regex.Replace(Regex.Replace(files[i], ".*\\sgrub-", "grub-"), "\\s*", ""));
				}
			}
			string filenamemaster = filenames[0];
			// Find the latest version
			for (int i = 1; i < filenames.Count; i++) {
				if (string.Compare(filenamemaster, filenames[i], StringComparison.Ordinal) == -1) {
					filenamemaster = filenames[i];
				}
			}
			WebClient client = new() {
				Credentials = new NetworkCredential("anonymous", "")
			};
			client.DownloadProgressChanged += (s, e) => {
				UpdateProgress(false, "Downloading GRUB file", e.ProgressPercentage);
			};
			// On download ended, go to next step
			client.DownloadFileCompleted += (s, e) => {
				Dispatcher.Invoke(() => {
					logText.Text += "Downloaded\n";
				});
				UpdateProgressFull(2);
				DownloadArchIso();
			};
			// Download the .zip file
			Uri todownload = new("ftp://ftp.gnu.org/gnu/grub/" + filenamemaster);
			Dispatcher.Invoke(() => {
				logText.Text += "------Downloading GRUB------\n";
				logText.Text += "Downloading " + todownload.AbsoluteUri + " as grub.zip" + "\n";
			});
			client.DownloadFileAsync(todownload, Path.GetTempPath() + "grub.zip");
		}

		// Download SystemRescueCD latest version
		public void DownloadArchIso() {
			WebClient client2 = new();
			client2.DownloadProgressChanged += (s, e) => {
				UpdateProgress(false, "Downloading Archlinux iso", e.ProgressPercentage);
			};
			// On download ended, go to next step
			client2.DownloadFileCompleted += (s, e) => {
				Dispatcher.Invoke(() => {
					logText.Text += "Downloaded\n";
				});
				UpdateProgressFull(3);
				MountArchIso();
			};
			Uri download = new("https://sourceforge.net/projects/systemrescuecd/files/latest/download");
			Dispatcher.Invoke(() => {
				logText.Text += "------Downloading Archlinux------\n";
				logText.Text += "Downloading " + download.AbsoluteUri + " as arch.iso\n";
			});
			client2.DownloadFileAsync(download, Path.GetTempPath() + "arch.iso");
		}
		
		// Copy all files from SystemRescueCD to the empty volume created before
		public void CopyAll(DirectoryInfo source, DirectoryInfo target)
		{
			if (source.FullName.ToLower(System.Globalization.CultureInfo.InvariantCulture) == target.FullName.ToLower(System.Globalization.CultureInfo.InvariantCulture)) {
				return;
			}
			if (Directory.Exists(target.FullName) == false) {
				_ = Directory.CreateDirectory(target.FullName);
			}
			foreach (FileInfo fi in source.GetFiles()) {
				Dispatcher.Invoke(() => {
					logText.Text += "Copying " + source.FullName + "\\" + fi.Name + "\n";
				});
				_ = fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);
			}
			foreach (DirectoryInfo diSourceSubDir in source.GetDirectories()) {
				DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
				CopyAll(diSourceSubDir, nextTargetSubDir);
			}
		}

		// Mount SystemRescueCD
		public void MountArchIso() {
			UpdateProgress(true, "Copying Archlinux files to the new partition", null);
			Dispatcher.Invoke(() => {
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
			process.Exited += (s, e) => {
				// U = .iso file, L = empty volume created before
				CopyAll(new DirectoryInfo(@"U:\sysresccd\"), new DirectoryInfo(@"L:\sysresccd\"));
				UpdateProgressFull(4);
				InstallGrub();
			};
			_ = process.Start();
		}

		// Mount the EFI partition as Z:
		public static void Mountefi() {
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

		// Install Grub4windows
		private void InstallGrub() {
			Dispatcher.Invoke(() => {
				logText.Text += "------Installing GRUB------\n";
				UpdateProgress(true, "Installing GRUB", null);
			});
			// Extract the archive
			ZipArchive archive = new(File.OpenRead(Path.GetTempPath() + "grub.zip"));
			if (Directory.Exists(Path.GetTempPath() + "grub")) {
				Directory.Delete(Path.GetTempPath() + "grub", true);
			}
			_ = Directory.CreateDirectory(Path.GetTempPath() + "grub");
			archive.ExtractToDirectory(Path.GetTempPath() + "grub");
			string[] dirs = Directory.GetDirectories(Path.GetTempPath() + "grub");
			Mountefi();
			Process process = new();
			process.StartInfo.FileName = @dirs[0] + @"\grub-install.exe";
			process.StartInfo.Arguments = "--bootloader-id=GRUB --boot-directory=Z:\\ --target=x86_64-efi --efi-directory=Z:\\";
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.RedirectStandardError = true;
			process.StartInfo.CreateNoWindow = true;
			process.EnableRaisingEvents = true;
			process.Exited += (s, e) => {
				SetupGrub();
			};
			_ = process.Start();
		}

		// Write the GRUB config file
		private void SetupGrub() {
			Dispatcher.Invoke(() => {
				logText.Text += process.StandardOutput + "\n";
			});
			// Initialize grub video and display the entries in a menu
			string[] lines = [
				"set timeout_style=menu",
				"set timeout = 1",
				"insmod efi_gop",
				"insmod efi_uga",
				"insmod ieee1275_fb",
				"insmod vbe",
				"insmod vga",
				"insmod video_bochs",
				"insmod video_cirrus",
				"menuentry 'Finish installing' {",
				"	insmod gzio",
				"	insmod part_gpt",
				"	insmod exfat",
				"	search --no-floppy --label ARCH --set=root",
				"	linux /sysresccd/boot/x86_64/vmlinuz archisobasedir=sysresccd archisolabel=ARCH copytoram setkmap=us ar_nowait ar_nofail quiet",
				"	initrd /sysresccd/boot/x86_64/sysresccd.img",
				"}",
				"menuentry 'Return to Windows' {",
				"	insmod part_gpt",
				"	insmod fat",
				"	chainloader /EFI/Microsoft/Boot/bootmgfw.efi",
				"}",
			];
			if (!Directory.Exists(@"Z:\grub")) {
				_ = Directory.CreateDirectory(@"Z:\grub");
			}
			File.WriteAllLines(@"Z:\grub\grub.cfg", lines);
			UpdateProgressFull(5);
			SetupSystem();
		}

		// Write the autorun file with arguments
		public void SetupSystem() {
			Dispatcher.Invoke(() => {
				UpdateProgress(true, "Preparing Linux autorun file", null);
			});
			StreamReader setupfile = new(GetType().Assembly.GetManifestResourceStream("WinArch.Resources.autorun"));
			List<string> list = new();
			string line;
			while ((line = setupfile.ReadLine()) != null) {
				list.Add(line);
			}
			string[] autorun = list.ToArray();
			// Add variables
			string[] lines = [
				"#!/bin/sh",
				"desktop=\"" + desktop + "\"",
				"hostname=\"" + hostname + "\"",
				"language=\"" + language + "\"",
				"keymap=\"" + keymap + "\"",
				"timezone=\"" + timezone + "\"",
				"password=\"" + password + "\"",
				"uname=\"" + uname + "\"",
				"unamesys=\"" + unameSys + "\"",
			];
			File.WriteAllLines(@"L:\autorun", lines);
			File.AppendAllLines(@"L:\autorun", autorun);
			Dispatcher.Invoke(() => {
				logText.Text += "------Creating linux autorun file------\n";
				logText.Text += File.ReadAllText(@"L:\autorun") + "\n";
			});
			UpdateProgressFull(6);
			Cleanup();
		}

		// Unmount all volumes, disable icompatible settings
		public void Cleanup() {
			Dispatcher.Invoke(() => {
				UpdateProgress(true, "Cleaning things up", null);
			});
			char[] letters = ['L', 'U', 'Z'];
			foreach (char letter in letters) {
				Process process = new();
				process.StartInfo.FileName = "mountvol.exe";
				process.StartInfo.Arguments = letter + ": /D";
				process.StartInfo.UseShellExecute = false;
				process.StartInfo.RedirectStandardOutput = true;
				process.StartInfo.RedirectStandardError = true;
				process.StartInfo.CreateNoWindow = true;
				process.EnableRaisingEvents = true;
				process.Start();
				process.WaitForExit();
			}
			Dispatcher.Invoke(() => {
				logText.Text += "------Unmounted ESP, Linux Partition and Archiso------\n";
			});
			Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Power", "HiberbootEnabled", 0, RegistryValueKind.DWord);
			Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power", "HibernateEnabled", 0, RegistryValueKind.DWord);
			Dispatcher.Invoke(() => {
				logText.Text += "------Disabled Fast Startup and Hibernation------\n";
			});
			UpdateProgressFull(7);
			Dispatcher.Invoke(() => {
				_ = NavigationService.Navigate(new Uri("Finish.xaml", UriKind.Relative));
			});
		}

		// Update the upper progress bar
		public void UpdateProgress(bool indeterminate, string currentAction, double? currentPercentage)
		{
			progressCurrent.IsIndeterminate = indeterminate;
			Dispatcher.Invoke(() => {
				if (!indeterminate) {
					progressCurrent.Value = (double)currentPercentage;
				}
				textBlock.Text = currentAction;
			});
		}

		// Update the lower progress bar
		public void UpdateProgressFull(float currentPercentage)
		{
			taskpercentage = currentPercentage * 100 / 7;
			Dispatcher.Invoke(() => {
				progressTotal.Value = taskpercentage;
			});
		}

		// Disable automatic scrolling of log text view
		private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			if (e.ExtentHeightChange == 0) {
				AutoScroll = Scroller.VerticalOffset == Scroller.ScrollableHeight;
			}

			if (AutoScroll && e.ExtentHeightChange != 0) {
				Scroller.ScrollToVerticalOffset(Scroller.ExtentHeight);
			}
		}
	}
}
