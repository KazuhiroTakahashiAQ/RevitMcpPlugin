# RevitMCP

RevitMCP は、Revit add-in が localhost MCP endpoint を起動し、Claude Desktop はローカル stdio ブリッジ経由でその endpoint に接続する構成のプロジェクトです。

今の完成形は次です。

- Revit add-in: [src/RevitMcp.Addin](/Users/kazuhiro.takahashi/Documents/work/RevitMCP/src/RevitMcp.Addin)
- Claude Desktop 用 stdio ブリッジ: [src/RevitMcp.Server](/Users/kazuhiro.takahashi/Documents/work/RevitMCP/src/RevitMcp.Server)
- インストーラー: [installer/install.ps1](/Users/kazuhiro.takahashi/Documents/work/RevitMCP/installer/install.ps1)

## 導入手順

要件どおり、導入フローは次です。

1. 作成された Revit add-in をインストール
2. Revit 上で add-in を起動
3. Claude 側で MCP 設定
4. Claude Desktop から接続

以下、そのまま実行できる形で書いています。

## 0. 前提

- Windows
- Revit 2026
- .NET 8 SDK
- Claude Desktop

Revit API の add-in 登録と `IExternalApplication` / `ExternalEvent` の前提は Autodesk 公式ドキュメントに沿っています。

- [Registration of add-ins](https://help.autodesk.com/cloudhelp/2024/ENU/Revit-API/files/Revit_API_Developers_Guide/Introduction/Getting_Started/Using_the_Autodesk_Revit_API/Revit_API_Revit_API_Developers_Guide_Introduction_Getting_Started_Using_the_Autodesk_Revit_API_Registration_of_add_ins_html.html)
- [External Applications](https://help.autodesk.com/cloudhelp/2024/ENU/Revit-API/files/Revit_API_Developers_Guide/Introduction/Getting_Started/Using_the_Autodesk_Revit_API/Revit_API_Revit_API_Developers_Guide_Introduction_Getting_Started_Using_the_Autodesk_Revit_API_External_Applications_html.html)
- [Revit API installation](https://help.autodesk.com/cloudhelp/2024/PTB/Revit-API/files/Revit_API_Developers_Guide/Introduction/Getting_Started/Welcome_to_the_Revit_Platform_API/Revit_API_Revit_API_Developers_Guide_Introduction_Getting_Started_Welcome_to_the_Revit_Platform_API_Installation_html.html)

Claude Desktop のローカル MCP サーバー設定は MCP 公式ドキュメントに沿っています。

- [Connect to local MCP servers](https://modelcontextprotocol.io/docs/develop/connect-local-servers)
- [MCP debugging guide](https://modelcontextprotocol.io/docs/tools/debugging)

## 1. 作成された Revit add-in をインストール

PowerShell でリポジトリ直下へ移動して実行します。

```powershell
dotnet build RevitMCP.slnx
dotnet test RevitMCP.slnx
powershell -ExecutionPolicy Bypass -File .\installer\install.ps1
```

`install.ps1` は次を行います。

- `RevitMcp.Addin` を publish
- `RevitMcp.Server` を publish
- add-in マニフェストを `%APPDATA%\Autodesk\Revit\Addins\2026\RevitMcp.addin` に配置
- 本体ファイルを `%LOCALAPPDATA%\RevitMcp\2026\` 配下へ配置

主な配置先:

- add-in DLL: `%LOCALAPPDATA%\RevitMcp\2026\addin\RevitMcp.Addin.dll`
- Claude Desktop ブリッジ: `%LOCALAPPDATA%\RevitMcp\2026\server\RevitMcp.Server.dll`
- add-in manifest: `%APPDATA%\Autodesk\Revit\Addins\2026\RevitMcp.addin`

アンインストールする場合:

```powershell
powershell -ExecutionPolicy Bypass -File .\installer\uninstall.ps1
```

## 2. Revit 上で add-in を起動

1. Revit 2026 を起動
2. Ribbon の `AsiaQuest` タブを開く
3. `Revit MCP` パネルの `Start MCP` を押す

これで add-in 内の localhost MCP server が起動します。既定 endpoint は次です。

```text
http://127.0.0.1:4863/mcp
```

`Start MCP` 実行後、TaskDialog でも endpoint が表示されます。  
`Stop MCP` を押すとサーバー停止です。

### ここで起きていること

- Revit add-in が `HttpListener` ベースの localhost MCP endpoint を起動
- `tools/call` は Revit 実行キューへ投入
- 実際の Revit API 実行は `ExternalEvent` 経由で UI スレッド上で処理

## 3. Claude 側で MCP 設定

Claude Desktop はローカル stdio サーバーを起動するので、ここでは **Revit add-in そのもの** ではなく、**stdio ブリッジ `RevitMcp.Server`** を設定します。  
このブリッジが Revit 側の `http://127.0.0.1:4863/mcp` へ中継します。

Claude Desktop の設定ファイル:

- macOS: `~/Library/Application Support/Claude/claude_desktop_config.json`
- Windows: `%APPDATA%\Claude\claude_desktop_config.json`

Windows 例:

```json
{
  "mcpServers": {
    "revit-mcp": {
      "command": "dotnet",
      "args": [
        "C:\\Users\\YOUR_USER\\AppData\\Local\\RevitMcp\\2026\\server\\RevitMcp.Server.dll",
        "--backend",
        "remote"
      ],
      "env": {}
    }
  }
}
```

`remote` backend の既定接続先は `http://127.0.0.1:4863/mcp` です。  
別ポートにしたい場合は `--remote-url` を追加します。

```json
{
  "mcpServers": {
    "revit-mcp": {
      "command": "dotnet",
      "args": [
        "C:\\Users\\YOUR_USER\\AppData\\Local\\RevitMcp\\2026\\server\\RevitMcp.Server.dll",
        "--backend",
        "remote",
        "--remote-url",
        "http://127.0.0.1:4863/mcp"
      ],
      "env": {}
    }
  }
}
```

Bearer Token を使う場合は `REVIT_MCP_BEARER_TOKEN` を追加します。

```json
{
  "mcpServers": {
    "revit-mcp": {
      "command": "dotnet",
      "args": [
        "C:\\Users\\YOUR_USER\\AppData\\Local\\RevitMcp\\2026\\server\\RevitMcp.Server.dll",
        "--backend",
        "remote"
      ],
      "env": {
        "REVIT_MCP_BEARER_TOKEN": "YOUR_TOKEN"
      }
    }
  }
}
```

保存後、Claude Desktop を完全終了して再起動します。

## 4. Claude Desktop から接続

Claude Desktop で次のように確認できます。

- `revit.document.get_info を使って現在のドキュメント情報を見せて`
- `revit.selection.get を使って現在選択中の要素を見せて`
- `revit.elements.find で OST_Walls を検索して`

write tool の確認例:

- `revit.elements.set_parameter で elementId 12345 の Comments を更新して`
- `revit.wall.create_line で Level 1 に 3m の壁を作成して`

## 実装済みの主な構成

- Revit add-in 起動エントリ: [App.cs](/Users/kazuhiro.takahashi/Documents/work/RevitMCP/src/RevitMcp.Addin/App.cs)
- Ribbon と start/stop コマンド: [UI](/Users/kazuhiro.takahashi/Documents/work/RevitMCP/src/RevitMcp.Addin/UI), [Commands](/Users/kazuhiro.takahashi/Documents/work/RevitMCP/src/RevitMcp.Addin/Commands)
- add-in 内 MCP host ライフサイクル: [RevitAddinHost.cs](/Users/kazuhiro.takahashi/Documents/work/RevitMCP/src/RevitMcp.Addin/Runtime/RevitAddinHost.cs)
- Revit API adapter: [src/RevitMcp.Addin/Revit](/Users/kazuhiro.takahashi/Documents/work/RevitMCP/src/RevitMcp.Addin/Revit)
- Claude Desktop stdio ブリッジ: [src/RevitMcp.Server](/Users/kazuhiro.takahashi/Documents/work/RevitMCP/src/RevitMcp.Server)

## 検証

このリポジトリでは次を確認済みです。

```bash
dotnet build RevitMCP.slnx
dotnet test RevitMCP.slnx
```

補足:

- `RevitMcp.Addin` は `Autodesk.Revit.SDK` NuGet を参照してビルドしています
- この環境では実際の Revit プロセス起動までは確認していません
- add-in の実運用確認は Windows + Revit 2026 上で行ってください

## ログ

Revit add-in の監査ログ:

```text
%APPDATA%\AsiaQuest\RevitMcp\logs\
```

Claude Desktop の MCP ログ:

- macOS: `~/Library/Logs/Claude`
- Windows: `%APPDATA%\Claude\logs`
