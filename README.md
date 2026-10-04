# What on Earth Decompiled

This repo contains a decompilation for the earliest version of the game **Big Bang Racing** by Traplight Games. The game was originally called **What on Earth**.

The decompiled build in this repo is from a **2014 iOS TestFlight build** of the game, which happens to be the only early build I could find.

> **📦 Game Version: `0.4.0`**

[Click here to download the original IPA (32-bit)](https://archive.org/download/traplight-whatonearth/WhatOnEarth_%28com.traplight.whatonearth%29_0.4.0.ipa)

# ⚠️ Important Information Before You Start

* This game originally used **Unity 4.3.2f1**, but has been updated to **Unity 4.7.2f1** (the closest version I already had).
* This project has been updated to Unity 2018.4.8f1 for stability and better support for platforms. [Click this to go to the newer source](https://github.com/Dodonickey/WOE-Decompiled/tree/WOE-0.4.0-Unity2018)
  
* A **modified** fork of this decomp which is patched to be played offline exists [here](https://github.com/BTE-92/WOE-Offline)
  
* Currently, you can only build for **Windows** and **Android** because of required plugins.
* Android support is buggy on newer Android versions; **Android 4.4 seems to be the most stable** for Android builds.
* Some enhancements have already been made to this source, including:

  * Mouse support
  * A config file that is generated in the persistent data path, making it easier to change the server URL
  * Some changes to the resource loading system

> ⚠️ **IMPORTANT: THIS GAME IS NOT OFFLINE.**
>
> You **must** use [this server](https://github.com/BTE-92/What-On-Earth-Server) in order to actually play the game.
>
> **Without the server, the game will not work.**

# 🛠️ How to Open the Project

## 1. Install Unity 4.7.2f1

> **You do not need to install Unity if you only want to download and play the game. [Click this text to see compiled builds](https://github.com/Dodonickey/WOE-Decompiled/releases)**
>
> Unity is only required if you want to **open, modify, or build the project yourself**.

I recommend using **Unity 4.7.2f1**, as this is the exact version I used for this project.

Using a later or earlier version may cause compatibility issues.

You can find previous Unity versions here:

https://forum.unity.com/threads/early-unity-versions-downloads.1483758/

## 2. Open the Project

After installing Unity 4.7.2f1, open this repository as a Unity project.


# 🎮 Controls

If you're running the game in the **Unity Editor on PC**, these are the controls:

* **A** — Zoom in
* **Z** — Zoom out
* **Left Shift** — Draw
* **Left Alt** — Erase
* **Left / Right Arrow Keys** — Drive

That's all the important ones for now.
