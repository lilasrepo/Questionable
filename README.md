# Questionable（繁中移植版 · TC13） / Traditional-Chinese Port

> 一個小小的任務輔助插件。<br>
> A tiny quest helper plugin.

**繁體中文**：這是 **[Questionable](https://github.com/PunishXIV/Questionable)** 的繁體中文客戶端移植版，對應 **FFXIV 7.20 / yanmucorp Dalamud API13（.NET 9）**。本專案僅做相容性移植，**非官方、非原作維護**；所有原始功能與設計著作權歸原作者 **liza、qstxiv 及眾多貢獻者**。

**English**: A Traditional-Chinese-client port of **[Questionable](https://github.com/PunishXIV/Questionable)** targeting **FFXIV 7.20 / yanmucorp Dalamud API13 (.NET 9)**. Compatibility port only — **unofficial and not maintained by the original author**. All original work © **liza, qstxiv & various contributors**.

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

## 安裝 / Installation

**繁體中文**
1. 使用 **XIVTCLauncher** 啟動繁體中文客戶端。
2. 遊戲內輸入 `/xlsettings` → 切到 **Experimental** 分頁 → **Custom Plugin Repositories（自訂插件庫）**。
3. 貼上下列網址並按 **+** 儲存：
   ```
   https://raw.githubusercontent.com/lilasrepo/DalamudPlugins/main/pluginmaster.json
   ```
4. 輸入 `/xlplugins`，搜尋 **Questionable (TC13)** → 安裝 → 啟用。

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

第三方插件，使用風險自負。**移植相關問題請回報到本 repo 的 Issues，請勿打擾上游原作者。**<br>
Third-party plugin — use at your own risk. **For port-specific issues please open an Issue here; do not contact the upstream author.**
