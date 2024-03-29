# WinArch Installer
A Windows executable to create an Archlinux dual boot on your computer, written with WPF(C#)/XAML. You can download it [here](https://github.com/srgoti/winarch-installer/releases).

## WARNING
Disable BitLocker / Drive Encryption or save your recovery key before running. Resizing a drive with BitLocker enabled will ask for its BitLocker key on the following boot. 

## What does it support ?
WinArch works only on x86_64 UEFI machines (Any computer made after 2010 should be compatible with this software).
It requires at least 5GB of free space on a single partition, and at least 50MB on the EFI System Partition (Windows leaves this space by default).

## How does it work ?
WinArch creates an empty partition, downloads SystemRescueCD on it, boots it to ram and installs Archlinux on that same partition.

## Limitations
- You cannot choose your preferred partition layout or filesystem
- You can only choose from 6 Desktop Environments
- You have to use a user password

## Any future ideas ?
- Remove previously mentionned limitations
- Use Preloader/shim to work with Secure Boot
- Autocomplete some fields like username and hostname

## Thanks
- Microsoft for WPF
- Archlinux for its complete wiki
- SystemRescueCD for having a scriptable live system
