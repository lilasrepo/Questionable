# Questionable（繁中移植版 · TC13） / Traditional-Chinese Port

> 一個小小的任務輔助插件。<br>
> A tiny quest helper plugin.

**繁體中文**：這是 **[Questionable](https://github.com/PunishXIV/Questionable)** 的繁體中文客戶端移植版，對應 **FFXIV 7.20 / yanmucorp Dalamud API13（.NET 9）**。本專案僅做相容性移植，**非官方、非原作維護**；所有原始功能與設計著作權歸原作者 **liza、qstxiv 及眾多貢獻者**。

### Contents
* [About](#about)
* [Companion Plugins](#deps)
* [Installation](#installation)
* [Commands](#commands)
* [Contributing](CONTRIBUTING.md)

---

## 這是什麼 / About

在可行的情況下自動幫你完成任務：使用 navmesh 自動走到各任務點，並嘗試自動完成沿途步驟（不含副本、單人任務與戰鬥）。並非所有任務都支援，最新清單請見上游 Discord／儲存庫。

Automatically does quests where possible — uses navmesh to walk to quest waypoints and tries to complete each step (excluding dungeons, solo duties and combat). Not all quests are supported; see the upstream Discord/repo for the current list.

## 需要的前置插件 / Required plugins

本插件需要以下插件才能運作（本插件庫皆提供 TC13 版）：<br>
This plugin requires the following (all available as TC13 builds in this repo):

- **vnavmesh (TC13)** — 自動尋路 / navigation
- **TextAdvance (TC13)** — 對話自動推進 / dialogue automation
- **Lifestream (TC13)** — 傳送 / teleport

See [CONTRIBUTING.md](CONTRIBUTING.md)

**English**
1. Launch the Traditional-Chinese client with **XIVTCLauncher**.
2. In-game, type `/xlsettings` → **Experimental** tab → **Custom Plugin Repositories**.
3. Add this URL and save with **+**:
   ```
   https://raw.githubusercontent.com/lilasrepo/DalamudPlugins/main/pluginmaster.json
   ```
4. Type `/xlplugins`, search **Questionable (TC13)** → Install → Enable.

## 對應版本 / Compatibility

| 項目 / Item | 版本 / Version |
|---|---|
| 遊戲 / Game | FFXIV 7.20（繁中客戶端 / TC client） |
| Dalamud | yanmucorp API13（.NET 9） |
| 移植自上游 / Ported from upstream | v15.300.0.4 |

## 原作與授權 / Credits & License

本專案 fork 自 **[PunishXIV/Questionable](https://github.com/PunishXIV/Questionable)**，授權沿用上游；所有原始功能著作權歸 **liza、qstxiv 及眾多貢獻者**。<br>
Forked from **[PunishXIV/Questionable](https://github.com/PunishXIV/Questionable)**. License follows upstream; all original work © **liza, qstxiv & various contributors**.

## 免責聲明 / Disclaimer

- ### [vnavmesh](https://github.com/awgil/ffxiv_navmesh)  
Handles in-zone navigation. It enables your character to move seamlessly from one quest objective to the next.

- ### [LifeStream](https://github.com/NightmareXIV/Lifestream)  
Proper fast-travel functionality within cities using Aetherytes and Aethernet Shards.

- ### [TextAdvance](https://github.com/NightmareXIV/TextAdvance)  
Automated quest interactions, including accepting and turning in quests as well as skipping cutscenes and dialogue.

## Optional

The following plugins enable extra functionality in Questionable.

### Combat Automation

For rotation/combat automation, select one of these plugins. Questionable recommends and actively works with the developers of Boss Mod (VBM) and Wrath Combo to ensure the best experience for users of this plugin, but other options are supported.

- ### [Boss Mod (VBM)](https://github.com/awgil/ffxiv_bossmod)
A plugin that provides boss fight radar, auto-rotation, cooldown planning, and AI. All of its modules can be toggled individually.

> [!WARNING]
> Forks of Boss Mod, such as BossMod Reborn, are not supported by Questionable, and will likely lead to issues.

- ### [Wrath Combo](https://github.com/PunishXIV/WrathCombo)
Wrath Combo is a heavily enhanced version of the XIVCombo plugin, offering highly customisable features and options to allow users to have their rotations be as complex or simple as possible, even to the point of a single button; for PvE, PvP, and more.

- ### [Rotation Solver Reborn](https://github.com/FFXIV-CombatReborn/RotationSolverReborn)
RotationSolverReborn is a community-made fork of the original RotationSolver plugin for Final Fantasy XIV. This tool is designed to enhance your gameplay experience by performing your rotation as optimally as possible, including heals, interrupts, mitigations, and MP management.

### Other Features

The following plugins are recommended, but not required.

- ### [CBT (formerly known as Automaton)](https://github.com/Jaksuhn/Automaton)
CBT is a tweak collection plugin that largely focuses on automating small and frequent tasks. Questionable uses it for the "Sniper No Sniping" tweak, which automatically completes sniping tasks introduced in Stormblood.

- ### [Pandora's Box](https://github.com/PunishXIV/PandorasBox)
Pandora's Box is a tweak collection plugin. Questionable uses it for the "Auto Active Time Maneuver" tweak, which automatically completes active time maneuvers in duties.

- ### [Artisan](https://github.com/PunishXIV/Artisan)
Artisan is a plugin for automating crafting. Questionable uses it for quests that involve crafting.

- ### [Autohook](https://github.com/PunishXIV/Autohook)
Autohook is a plugin for automating fishing. Questionable uses it for quests that involve fishing.

- ### [AutoDuty](https://github.com/ffxivcode/AutoDuty)
AutoDuty is a plugin that serves as a tool to assist in the creation and following of paths through dungeons and duties. Questionable uses it to automate the completion of duties that are required for certain quests.

</section><br>

<!-- Installation -->
<section id="installation"><br>

# Installation

<img src="https://github.com/PunishXIV/WrathCombo/raw/main/res/readme_images/adding_repo.jpg" width="450" />

Open the Dalamud Settings menu in game and follow the steps below.
This can be done through the button at the bottom of the plugin installer or by
typing `/xlsettings` in the chat.

1. Under Custom Plugin Repositories, enter `https://love.puni.sh/ment.json` into the empty box at the bottom.
2. Click the "+" button.
3. Click the "Save and Close" button.

Open the Dalamud Plugin Installer menu in game and follow the steps below.
This can be done through `/xlplugins` in the chat.

1. Click the "All Plugins" tab on the left.
2. Search for "Questionable".
3. Click the "Install" button.
</section><br>

<!-- Commands -->
<section id="commands">

# Commands

<table>
<thead>
<tr>
<th align="left"><strong>Chat command</strong></th>
<th align="left"><strong>Function</strong></th>
</tr>
</thead>
<tbody>
<tr>
<td align="left"><code>/qst</code></td>
<td align="left">Opens the Questing window.</td>
</tr>
<tr>
<td align="left"><code>/qst config</code></td>
<td align="left">Opens the Configuration window.</td>
</tr>
<tr>
<td align="left"><code>/qst start</code></td>
<td align="left">Starts doing quests.</td>
</tr>
<tr>
<td align="left"><code>/qst stop</code></td>
<td align="left">Stops doing quests.</td>
</tr>
<tr>
<td align="left"><code>/qst reload</code></td>
<td align="left">Reloads all quests data.</td>
</tr>
<tr>
<td align="left"><code>/qst which</code></td>
<td align="left">Shows all quests starting with your selected target.</td>
</tr>
<tr>
<td align="left"><code>/qst zone</code></td>
<td align="left">Shows all quests starting with your current zone.<br> (<b>NOTE</b>: This only includes quests with a valid quest path and are currently visible &amp; unaccepted.)</td>
</tr>
</tbody>
</table>

</section><br>

<!-- Punish Logo & Discord -->
<div align="center">
  <a href="https://puni.sh/" alt="Puni.sh">
    <img src="https://github.com/PunishXIV/AutoHook/assets/13919114/a8a977d6-457b-4e43-8256-ca298abd9009" /></a>
<br>
  <a href="https://discord.gg/Zzrcc8kmvy" alt="Discord">
    <img src="https://discordapp.com/api/guilds/1001823907193552978/embed.png?style=banner2" /></a>
</div>
<br>
