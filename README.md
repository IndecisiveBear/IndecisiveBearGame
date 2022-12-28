# IndecisiveBear | Untitled Game
Repo for Untitled Game by IndecisiveBear Studios.

## Setup:
### Prerequisites:

#### Windows:
* [Unity Engine 2022.2.1](https://unity.com/releases/editor/whats-new/2022.2.1#release-notes)
* One fo the following implementations of C#:
  * [Visual Studio](https://visualstudio.microsoft.com/downloads/)
  * [Dotnet](https://www.microsoft.com/en-us/download/details.aspx?id=7029)


#### Linux (Fedora):

* Unity Engine 2022.2.1:

Follow the Red Hat Enterprise Linux (RHEL) or CentOS directions [here](https://docs.unity3d.com/hub/manual/InstallHub.html#install-hub-linux).

You will not yet be able to activate your free license or run any projects. The reason for this is that Unity requires either openssl version 1.0 or version 1.1 to run, while Fedora comes with a more recent version. To install the necessary version, use the following:
```bash
sudo dnf makecache
```
```bash
sudo dnf install openssl1.1.x86_64
```
Once complete, search for UnityHub on your machine and open it. It will guide you through the install process and license activation.

The version of the Unity Editor that UnityHub downloads is not consistent with the editor we want for the project. Download the [correct Unity Editor](https://unity.com/releases/editor/whats-new/2022.2.1#release-notes) and install it at `~/Unity/Hub/Editor/` (or just download it straight from the UnityHub GUI). Unity should now work properly.

* Dotnet

Visual Studios is not available on Linux, so we need to install the dotnet SDK directly. Download the SDK for Linux [here](https://dotnet.microsoft.com/en-us/download/dotnet/sdk-for-vs-code?utm_source=vs-code&amp;utm_medium=referral&amp;utm_campaign=sdk-install).

Next, navigate to the location of the download and run the following:
```bash
sudo chmod +x ./dotnet-install.sh
```
```bash
./dotnet-install.sh
```
Once the install is complete, we need to add dotnet to our path. Do so by editing the `~/.bashrc` file (or the rc file for whatever shell you are using) and adding the following three lines:
```vim
export DOTNET_ROOT=$HOME/.dotnet
export PATH=$PATH:$HOME/.dotnet
export PATH=$PATH:$HOME/.dotnet/tools
```
Your IDE or code editor should now recognize C#.

To get IntelliSense working for Unity packages in VS Code, install the C# v1.25.0 extension and add the following to your `settings.json` file:
```json
"omnisharp.useModernNet": false
```
You will need to install a complete version of mono to proceed. To do so, follow the instructions from the [mono website](https://www.mono-project.com/download/stable/#download-lin-fedora):
```bash
sudo rpm --import "https://keyserver.ubuntu.com/pks/lookup?op=get&search=0x3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF"
sudo su -c 'curl https://download.mono-project.com/repo/centos8-stable.repo | tee /etc/yum.repos.d/mono-centos8-stable.repo'
sudo dnf update
sudo dnf install mono-devel
```
IntelliSense should now work in VS Code.

## Notes:
This repo is in active development. The game is not yet at a working state.
