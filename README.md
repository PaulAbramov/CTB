# CardTradeBot (CTB)

[![Steam Donate](https://img.shields.io/badge/steam-donate-red.svg?colorA=000000&colorB=FF0000)](https://steamcommunity.com/tradeoffer/new/?partner=40214091&token=_qILz8Ah)
[![Steam Group](https://img.shields.io/badge/steam-group-red.svg?colorA=000000&colorB=FF0000)](https://steamcommunity.com/groups/xtstc)

A robust C# core engine designed to automate Steam account interactions, specifically focusing on card farming and trade offer management.

### 🔗 Part of a Suite
This repository contains the **Core Logic**. It is designed to be managed and monitored via the [XTSCTBUI](https://github.com/PaulAbramov/XTSCTBUI) management interface.

### ✨ Key Features
* **Automated Card Farming:** Efficiently idles games in the background to trigger Steam trading card drops.
* **Smart Trade Handling:** Automatically processes specific trade offers based on user-defined rules.
* **Transparent Main Account Mode:** Unlike many other idlers, CTB can run on your main account without disconnecting you, allowing you to play games while the bot works in the background.
* **Secure Authentication:** Full support for Steam Guard and 2FA via SteamAuth integration.

### 🛠 Tech Stack
* **Language:** C#
* **Protocols:** SteamKit2 (Internal Steam Protocol)
* **Libraries:** SteamAuth, Newtonsoft.Json
