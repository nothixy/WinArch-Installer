[![pipeline status](https://gitlab.com/srgoti/winarch-installer/badges/master/pipeline.svg)](https://gitlab.com/srgoti/winarch-installer/-/commits/master)
# WinArch Installer
A Windows executable to create an Archlinux dual boot on your computer, written with WPF(C#)/XAML.

## What does it support ?
WinArch works only on x86_46 UEFI machines (Any computer made after 2010 should be compatible with this software).
It requires at least 3.5GB of free space on a single partition, and at least 50MB on the EFI System Partition (Windows leaves this space by default).

## How does it work ?
WinArch creates an empty partition, downloads SystemRescueCD on it, boots it to ram and installs Archlinux on that same partition

## Limitations
- You cannot choose your preferred partition layout or filesystem
- You can only choose from 6 Desktop Environments
- You have to use a user password

## Any future ideas ?
- Remove previously mentionned limitations
- Use a real gui for the Linux-side installer
- Use Preloader/shim to work with Secure Boot
- Autocomplete some fields like username and hostname

## Thanks
- Microsoft for WPF
- Archlinux for its complete wiki
- SystemRescueCD for having a scriptable live system
