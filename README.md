# RevitMCP

RevitMCP は Revit 向け MCP サーバーの実装リポジトリです。  
このリポジトリには現在、Claude Desktop から起動できる **stdio ベースの MCP サーバーホスト** を追加しています。

## 現在使えるもの

- Claude Desktop から起動できる `RevitMcp.Server`
- MCP の `tools/list` / `tools/call` に対応した 6 個の tool
- fixture ベースの in-memory Revit モデル
- `dotnet test` で検証できるコア実装

## まだ未実装のもの

**実 Revit プロセスに接続する Windows 用 add-in ホストは未実装** です。  
つまり現時点の Claude Desktop 接続は「Revit 実データ」ではなく、同梱 fixture を使った MCP サーバーとして動作します。

理由は、実 Revit 連携には少なくとも次が必要だからです。

- Windows 上の Revit add-in プロジェクト
- `RevitAPI.dll` / `RevitAPIUI.dll` 参照
- `ExternalEvent` と Revit UI スレッド上での実行
- Claude Desktop から接続するための localhost HTTP もしくは Revit 内 stdio ではないブリッジ

このリポジトリでは、そこへ進む前段として Claude Desktop で確認できる MCP サーバー実行基盤を先に入れています。

## 前提

- .NET 8 SDK
- Claude Desktop

Claude Desktop のローカル MCP サーバー設定や `claude_desktop_config.json` の場所は、MCP 公式ドキュメントに沿っています。

- [Build an MCP server](https://modelcontextprotocol.io/quickstart/server)
- [Connect to local MCP servers](https://modelcontextprotocol.io/docs/develop/connect-local-servers)
- [MCP debugging guide](https://modelcontextprotocol.io/docs/tools/debugging)

## 1. ビルド

リポジトリ直下で実行します。

```bash
dotnet build RevitMCP.slnx
dotnet test RevitMCP.slnx
```

Claude Desktop から安定して起動するには publish しておくのが楽です。

```bash
dotnet publish src/RevitMcp.Server/RevitMcp.Server.csproj -c Release -o ./artifacts/RevitMcp.Server
```

publish 後の実行ファイルは次になります。

- DLL: `artifacts/RevitMcp.Server/RevitMcp.Server.dll`
- fixture: `artifacts/RevitMcp.Server/Fixtures/sample-project.json`

## 2. Claude Desktop 設定

Claude Desktop の設定ファイルは通常次です。

- macOS: `~/Library/Application Support/Claude/claude_desktop_config.json`
- Windows: `%APPDATA%\\Claude\\claude_desktop_config.json`

`stdio` サーバーはクライアントの作業ディレクトリに依存しないよう、**必ず絶対パス** を使ってください。これは MCP のデバッグガイドでも推奨されています。

### macOS 例

```json
{
  "mcpServers": {
    "revit-mcp": {
      "command": "dotnet",
      "args": [
        "/ABSOLUTE/PATH/TO/RevitMCP/artifacts/RevitMcp.Server/RevitMcp.Server.dll"
      ],
      "env": {}
    }
  }
}
```

### Windows 例

```json
{
  "mcpServers": {
    "revit-mcp": {
      "command": "dotnet",
      "args": [
        "C:\\ABSOLUTE\\PATH\\TO\\RevitMCP\\artifacts\\RevitMcp.Server\\RevitMcp.Server.dll"
      ],
      "env": {}
    }
  }
}
```

保存後、Claude Desktop を完全終了して再起動します。

## 3. 接続確認

Claude Desktop で次のように聞くと動作確認しやすいです。

- `revit.document.get_info を使って現在のドキュメント情報を見せて`
- `revit.selection.get を使って選択要素を見せて`
- `revit.elements.find で OST_Walls を検索して`

初期 fixture には以下が入っています。

- 1 件の wall
- 1 件の door
- 選択状態は wall と door の 2 要素

## 4. fixture を差し替える

任意の fixture JSON を使う場合は `--fixture` か `REVIT_MCP_FIXTURE` を使えます。

### Claude Desktop 設定例

```json
{
  "mcpServers": {
    "revit-mcp": {
      "command": "dotnet",
      "args": [
        "C:\\ABSOLUTE\\PATH\\TO\\RevitMCP\\artifacts\\RevitMcp.Server\\RevitMcp.Server.dll",
        "--fixture",
        "C:\\ABSOLUTE\\PATH\\TO\\custom-project.json"
      ],
      "env": {}
    }
  }
}
```

fixture の参考フォーマットは [sample-project.json](/Users/kazuhiro.takahashi/Documents/work/RevitMCP/src/RevitMcp.Server/Fixtures/sample-project.json) を見てください。

## 5. ログ確認

Claude Desktop の MCP ログは通常ここに出ます。

- macOS: `~/Library/Logs/Claude`
- Windows: `%APPDATA%\Claude\logs`

確認コマンド例:

```bash
tail -n 20 -f ~/Library/Logs/Claude/mcp*.log
```

## 6. 実 Revit 連携へ進むために必要な次の実装

本当に Revit を操作するには、次の実装が別途必要です。

1. Windows 専用の `RevitMcp.Addin` プロジェクトを作る
2. Revit の `IExternalApplication` と Ribbon を実装する
3. `ExternalEvent` 経由で `RevitExecutionService` を実 Revit API へ接続する
4. Claude Desktop からその add-in 内サーバーへ接続する transport を確定する

現実的には、Revit add-in 内で localhost MCP endpoint を起動して Claude Desktop から HTTP で接続する構成が本命です。
