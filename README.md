# WarnSystem
Unturned Moderation made Simple!

### Features
- Warn Rulebreaking Players with their **Name** or **SteamID**
- Automatically give out Punishments for reaching certain Warn Thresholds *(Configurable)*
- Command Execution can be Logged to Console as well as with a **Discord Webhook**
- Supports **Multiple** Databases *(See Below)*

### Commands
__**Warn**__  
`/Warn <Player> <Reason>`  
Warn Players, Warn Reason is **automatically** combined.  
**Examples:**  
*/Warn PlayerName Very Rude*  
*/Warn 76561197960287930 "Doesn't Like Apple Juice"*  
  
__**Warndelete**__  
`/Warndelete <Player> <Index>` **-** ***`/Warndel`***  
Delete Accidental Warnings, Index corresponds to the index of the List + the Offset in the Configuration File.  
Meaning with default settings index *1* is the **First** item and index *2* is the **Second** Item *etc.*  
**Examples:**  
*/Warndelete NameOfPlayer 0*  
*/Warndel 76561198169868943 7*  
  
__**Warnclear**__  
`/Warnclear <Player>` **-** ***`/Warnclr`***  
Clear **All** Warnings from a Player.  
**Examples:**  
*/Warnclear SpelarensNamn*  
*/Warnclr 76561198197527013*  
  
__**Warnings**__  
`/Warnings <Player>` **-** ***`/Warns`***  
View Your or another Player's Warnings.  
**Examples:**  
*/Warnings*  
*/Warns 76561198209939643*
#### Permissions
`WarnSystem.Warn` **-** Gives Access to; `/Warn` `/Warndelete` `/Warnclear` & Aliases  
`WarnSystem.View` **-** Gives Access to; `/Warnings` & Aliases

### Databases & Migration
__Supported Databases__  
`Json` **-** Libraries are **NOT** Required if you use this Database Type  
`MySQL` **-** Libraries **ARE** Required if you use this Database Type  
  
__Migration__  
*MigrationCommand:* `/WSMigrateDB <Old Database> <New Database>`  
1. Begin by changing your `DatabaseSystem` in your Configuration file to your desired New Database.
2. Next Load the WarnSystem Plugin.
3. Once the Database has been Loaded simply start the Database Migration by using the MigrationCommand.
4. You're Done when the Plugin Writes that the Migration is Done!  
  
<u>Example</u> **-** Migrating from **Json** to **MySQL**  
*WSMigrateDB Json MySQL*  
*Reading from JSON Database..*  
*Saving to MYSQL Database..*  
*Successfully Migrated from JSON to MYSQL!*  
