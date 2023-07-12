#nullable enable

using UnityEngine;

using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

using Cysharp.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.UnityConverters;
using Newtonsoft.Json.UnityConverters.Math;

using Dropbox.Api;
using Dropbox.Api.Files;
using System.Net;
using System.Net.Http;
using System.Diagnostics;
using UnityEngine.Networking;

public class LevelDataManager : IDisposable
{
	[Serializable]
	public record GameConfig(int LastLevel, int RequiredMultiples)
	{
		public static GameConfig Init = new(LastLevel: 1, RequiredMultiples: 3);
	}

	private const string DROPBOX_APP_KEY = "vn9mofdzn1ccx7c";
	private const string DROPBOX_APP_SECRET = "hkyyyna1ty0tedy";

	//// This loopback host is for demo purpose. If this port is not
	//// available on your machine you need to update this URL with an unused port.
	//private const string LoopbackHost = "http://localhost/";

	//// URL to receive OAuth 2 redirect from Dropbox server.
	//// You also need to register this redirect URL on https://www.dropbox.com/developers/apps.
	//private readonly Uri m_redirectUri = new Uri(LoopbackHost + "authorize");

	//// URL to receive access token from JS.
	//private readonly Uri m_jsRedirectUri = new Uri(LoopbackHost + "token");

	private const string DROPBOX_ACCESS_TOKEN = "sl.BiB1cwq_mPr0MGBsnZ4E5I0fUgHCR4u1fjjx9j1LR9p2RRClucMcrD0b57DrELO55KJB6Hgmvf7qNAYVvRIKugUkyS0Og7vQzufRfwJILCCSw_FZWho5KStzrEvkxxpDCB_NLv9msJhy";

	private readonly JsonSerializerSettings m_serializerSettings;
	private readonly Dictionary<int, LevelData> m_savedLevelDataDic;
	private readonly string m_dataPath;
	private readonly CancellationTokenSource m_cancellationTokenSource;
	private DropboxClient m_dropBox = null!;
	//private readonly AsyncReactiveProperty<bool> m_initialized = new(false);

	private string GetLevelDataFileName(int level) => $"LevelData_{level}.dat";
	private string GetGameConfigFileName() => "GameConfig.dat";

	private string GetUrl(string fileName) => $"/{m_dataPath}/{fileName}";

	private GameConfig m_gameConfig = new(1, 3);
	private LevelData? m_currentData;

	public LevelData? CurrentLevelData => m_currentData;
	public GameConfig Config => m_gameConfig;

	public LevelDataManager(string dataPath)
	{
		m_cancellationTokenSource = new();
		m_dataPath = dataPath;
		m_savedLevelDataDic = new();
		m_serializerSettings = new JsonSerializerSettings {
			Converters = new [] {
				new Vector3Converter()
			},
			Formatting = Formatting.Indented,
			ContractResolver = new UnityTypeContractResolver()
		};


		m_dropBox = new DropboxClient(DROPBOX_ACCESS_TOKEN);
		//UniTask.Void(
		//	async token => {
		//		DropboxCertHelper.InitializeCertPinning();
		//		string[] scopeList = new string[] { 
		//			"files.metadata.read", 
		//			"files.metadata.write", 
		//			"files.content.read", 
		//			"files.content.write"
		//		};
				
		//		string accessToken = await GetAccessToken();

		//		m_initialized.Value = true;
		//	},
		//	m_cancellationTokenSource.Token
		//);
	}

	public void Dispose()
	{
		m_dropBox?.Dispose();
		m_cancellationTokenSource?.Dispose();
		//m_initialized?.Dispose();
	}

	public async UniTask<LevelData?> LoadConfig()
	{
		//await UniTask.WaitUntil(() => m_initialized.Value);

		string fileName = GetGameConfigFileName();
		GameConfig newConfig = GameConfig.Init;
		
		m_gameConfig = await LoadInternal<GameConfig>(fileName, () => GameConfig.Init) ?? GameConfig.Init;
		await SaveInternal(fileName, newConfig);
		return await LoadLevelDataInternal(1);
	}

	public async UniTask<LevelData?> CreateLevelData()
	{
		int nextLevel = m_gameConfig.LastLevel + 1;
		m_gameConfig = m_gameConfig with { LastLevel = nextLevel };
		return await LoadLevelDataInternal(nextLevel);
	}

	public async UniTask<LevelData?> LoadLevelData(int level)
	{
		if (level < 0 || level >= m_gameConfig.LastLevel)
		{
			return m_currentData;
		}

		return await LoadLevelDataInternal(level);
	}

	public UniTask<LevelData?> LoadLevelDataByStep(int direction)
	{
		int selectLevel = Mathf.Max((m_currentData?.Level + direction) ?? 1, 1);
		return LoadLevelDataInternal(selectLevel);
	}

	private async UniTask<LevelData?> LoadLevelDataInternal(int level)
	{
		string fileName = GetLevelDataFileName(level);

		var data = m_savedLevelDataDic.ContainsKey(level)? 
			m_savedLevelDataDic[level] :
			await LoadInternal<LevelData>(fileName, () => LevelData.CreateData(level));
		
		if (data == null)
		{
			UnityEngine.Debug.LogException(new NullReferenceException(nameof(data)));
			return default;
		}

		if (!m_savedLevelDataDic.ContainsKey(level))
		{
			m_savedLevelDataDic.Add(level, data);
		}
		else
		{
			m_savedLevelDataDic[level] = data;
		}

		m_currentData = data;

		return data;
	}

	public UniTask SaveLevelData()
	{
		if (m_currentData == null)
		{
			return UniTask.CompletedTask;
		}

		int level = m_currentData.Level;
		string path = GetLevelDataFileName(level);

		return UniTask.WhenAll(
			SaveInternal(GetGameConfigFileName(), m_gameConfig),
			UniTask.Create(
				async () => {
					await SaveInternal(GetLevelDataFileName(level), m_currentData);
					if (!m_savedLevelDataDic.ContainsKey(level))
					{
						m_savedLevelDataDic.Add(level, m_currentData);
					}
					else
					{
						m_savedLevelDataDic[level] = m_currentData;
					}
				}
			)
		);
	}

	public void AddBoardData(out int count)
	{
		if (m_currentData?.Boards == null)
		{
			count = m_currentData?.Boards?.Count ?? 0;
			return;
		}
		List<Layer> layers = new();
		layers.Add(new Layer());

		m_currentData.Boards.Add(new Board(layers));

		count = m_currentData.Boards.Count;
	}

	public bool TryRemoveBoardData(int boardIndex, out int boardCount)
	{
		if (!m_currentData?.Boards?.HasIndex(boardIndex) ?? false)
		{
			boardCount = m_currentData?.Boards?.Count ?? 0;
			return false;
		}
		
		m_currentData?.Boards.RemoveAt(boardIndex);
		boardCount = m_currentData?.Boards?.Count ?? 0;
		return true;
	}

	// index뒤에 추가
	public bool TryAddLayerData(int boardIndex, int prevIndex, out int addIndex)
	{
		Board? board = m_currentData?[boardIndex];

		if (board?.Layers.HasIndex(prevIndex) ?? false)
		{
			addIndex = prevIndex + 1;
			board.Layers.Insert(addIndex, new Layer());
			return true;
		}

		addIndex = prevIndex;
		return false;
	}

	public void RemoveLayerData(int boardIndex, int index)
	{
		Board? board = m_currentData?[boardIndex];

		if (board?.Layers.HasIndex(index) ?? false)
		{
			board.Layers.RemoveAt(index);
		}
	}

	public bool TryAddTileData(int boardIndex, int layerIndex, Vector2 position)
	{
		Layer? layerData = m_currentData?.GetLayer(boardIndex, layerIndex);

		if (layerData?.Tiles?.All(tile => !position.Equals(tile.Position)) ?? false)
		{
			layerData.Tiles.Add(new Tile(0, position));
			return true;
		}

		return false;
	}

	public int? UpdateNumberOfTypes(int number)
	{
		if (m_currentData == null)
		{
			return default;
		}

		int clamp = Mathf.Max(number, 1);

		m_currentData = m_currentData with {
			NumberOfTileTypes = clamp
		};

		return clamp;
	}

	public bool TryRemoveTileData(int boardIndex, int layerIndex, Vector2 position)
	{
		Layer? layerData = m_currentData?.GetLayer(boardIndex, layerIndex);

		if (layerData?.Tiles is var tiles and not null)
		{
			return tiles.RemoveAll(tile => tile.Position == position) > 0;
		}

		return false;
	}

	public void ClearTileDatasInLayer(int boardIndex, int layerIndex)
	{
		Layer? layerData = m_currentData?[boardIndex]?[layerIndex];

		if (layerData != null)
		{
			layerData.Tiles.Clear();
		}
	}

	private async UniTask<T?> LoadInternal<T>(string fileName, Func<T> onCreateNew)
	{
		var list = await m_dropBox.Files
			.ListFolderAsync(
				new ListFolderArg(
					path: $"/{m_dataPath}",
					recursive: true,
					includeMediaInfo: true
				)
			)
			.AsUniTask()
			.AttachExternalCancellation(m_cancellationTokenSource.Token);
		
		if (list.Entries.All(entry => !entry.IsFile || entry.Name != fileName))
		{
			await SaveInternal(fileName, onCreateNew.Invoke());
		}

		var res = await m_dropBox.Files
			.DownloadAsync(new DownloadArg(GetUrl(fileName)))
			.AsUniTask()
			.AttachExternalCancellation(m_cancellationTokenSource.Token);

		string json = await res.GetContentAsStringAsync();
		return JsonConvert.DeserializeObject<T>(json, m_serializerSettings);
	}

	private async UniTask SaveInternal<T>(string fileName, T data)
	{
		if (data == null)
		{
			return;
		}

		string json = JsonConvert.SerializeObject(data, m_serializerSettings);
		
		using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
		{
			await m_dropBox.Files
				.UploadAsync(GetUrl(fileName), WriteMode.Overwrite.Instance, body: ms)
				.AsUniTask()
				.AttachExternalCancellation(m_cancellationTokenSource.Token);
		}
	}

	//private async UniTask<string> GetAccessToken()
	//{
	//	Console.WriteLine("Waiting for credentials.");
	//	var state = Guid.NewGuid().ToString("N");
	//	var authorizeUri = DropboxOAuth2Helper.GetAuthorizeUri(OAuthResponseType.Token, DROPBOX_APP_KEY, redirectUri: m_redirectUri, state: state, tokenAccessType: TokenAccessType.Offline);
	//	var http = new HttpListener();
	//	http.Prefixes.Add(LoopbackHost);

	//	http.Start();

	//	System.Diagnostics.Process.Start(authorizeUri.ToString());

	//	// Handle OAuth redirect and send URL fragment to local server using JS.
	//	await HandleOAuth2Redirect(http);

	//	// Handle redirect from JS and process OAuth response.
	//	var result = await HandleJSRedirect(http);

	//	if (result.State != state)
	//	{
	//		// The state in the response doesn't match the state in the request.
	//		return string.Empty;
	//	}

	//	Console.WriteLine("and back...");

	//	return result.AccessToken;

	//	/// <summary>
	//	/// Handles the redirect from Dropbox server. Because we are using token flow, the local
	//	/// http server cannot directly receive the URL fragment. We need to return a HTML page with
	//	/// inline JS which can send URL fragment to local server as URL parameter.
	//	/// </summary>
	//	/// <param name="http">The http listener.</param>
	//	/// <returns>The <see cref="Task"/></returns>
	//	async UniTask HandleOAuth2Redirect(HttpListener http)
	//	{
	//		var context = await http.GetContextAsync();

	//		// We only care about request to RedirectUri endpoint.
	//		while (context.Request.Url.AbsolutePath != m_redirectUri.AbsolutePath)
	//		{
	//			context = await http.GetContextAsync();
	//		}

	//		context.Response.ContentType = "text/html";

	//		// Respond with a page which runs JS and sends URL fragment as query string
	//		// to TokenRedirectUri.
	//		using (var file = File.OpenRead(Path.Combine(Application.dataPath, "index.html")))
	//		{
	//			file.CopyTo(context.Response.OutputStream);
	//		}

	//		context.Response.OutputStream.Close();
	//	}

	//	/// <summary>
	//	/// Handle the redirect from JS and process raw redirect URI with fragment to
	//	/// complete the authorization flow.
	//	/// </summary>
	//	/// <param name="http">The http listener.</param>
	//	/// <returns>The <see cref="OAuth2Response"/></returns>
	//	async UniTask<OAuth2Response> HandleJSRedirect(HttpListener http)
	//	{
	//		var context = await http.GetContextAsync();

	//		// We only care about request to TokenRedirectUri endpoint.
	//		while (context.Request.Url.AbsolutePath != m_jsRedirectUri.AbsolutePath)
	//		{
	//			context = await http.GetContextAsync();
	//		}

	//		var redirectUri = new Uri(context.Request.QueryString["url_with_fragment"]);

	//		var result = DropboxOAuth2Helper.ParseTokenFragment(redirectUri);

	//		return result;
	//	}

	//}
}
