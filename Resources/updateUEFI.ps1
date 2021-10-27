$newguid = bcdedit.exe --% /copy {bootmgr} /d "GRUB";
$guid = "{" + ($newguid -split '[{}]')[1] + "}";
bcdedit.exe --% /set $guid device partition=Z:;
bcdedit --% /set %guid path \\EFI\\Boot\\BOOTX64.efi;
bcdedit --% /set {fwbootmgr} displayorder $guid /addfirst;