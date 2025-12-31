using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Codexus.Cipher.Entities;
using Codexus.Cipher.Entities.WPFLauncher.NetGame;
using Codexus.Cipher.Entities.WPFLauncher.NetGame.Skin;
using Codexus.Cipher.Entities.WPFLauncher.NetGame.Texture;
using Codexus.Cipher.Entities.WPFLauncher.RPC;
using Codexus.Cipher.Skip32;
using Codexus.Development.SDK.Entities;
using Codexus.Development.SDK.Manager;
using Codexus.Game.Launcher.Managers;
using Codexus.Game.Launcher.Services.Java.RPC.Events;
using Codexus.Game.Launcher.Utils;
using Serilog;

namespace Codexus.Game.Launcher.Services.Java.RPC;

public sealed class GameRpcService(int port, string serverIp, string serverPort, string roleName, string userId, string userToken, EnumGameVersion gameVersion)
{
	public delegate Entity<EntityComponentDownloadInfoResponse> GetComponents(string userId, string userToken, string str);

	public delegate List<EntityUserGameTexture> GetSkinList(string userId, string userToken, EntityUserGameTextureRequest entity);

	private readonly HttpClient _httpClient = new HttpClient();

	private readonly SocketCallback _mSocketCallbackFuc = new SocketCallback();

	private readonly List<byte[]> _sendCache = new List<byte[]>();

	private readonly Skip32Cipher _skip32Cipher = new Skip32Cipher("SaintSteve"u8.ToArray());

	private string _dirSkinPath = string.Empty;

	private GetComponents? _getComponents;

	private GetSkinList? _getSkin;

	private bool _mIsLaunchIdxReady;

	private bool _mIsNormalExit;

	private TcpListener? _mMcControlListener;

	private BinaryReader? _reader;

	private BinaryWriter? _writer;

	public TcpClient? Client;

	public void Connect(string skinPath, GetSkinList getSkinListFunc, GetComponents getComponentsFunc)
	{
		_dirSkinPath = skinPath;
		_getSkin = getSkinListFunc;
		_getComponents = getComponentsFunc;
		StartControlConnection(2);
		_mSocketCallbackFuc.RegisterReceiveCallback(18, OnCheckPlayerMsg);
		_mSocketCallbackFuc.RegisterReceiveCallback(19, OnPCycEntityCheck);
		_mSocketCallbackFuc.RegisterReceiveCallback(261, HandleAuthenticationNewVersion);
		_mSocketCallbackFuc.RegisterReceiveCallback(512, OnHeartBeat);
		_mSocketCallbackFuc.RegisterReceiveCallback(517, HandleAuthentication);
		_mSocketCallbackFuc.RegisterReceiveCallback(520, HandlePlayerSkin);
		_mSocketCallbackFuc.RegisterReceiveCallback(1298, HandleMsgFilterCheck);
		_mSocketCallbackFuc.RegisterReceiveCallback(4612, HandleLoginGame);
	}

	private void StartControlConnection(int tryTimes)
	{
		try
		{
			_mMcControlListener = new TcpListener(IPAddress.Loopback, port);
			_mMcControlListener.Start();
			new Thread(ListenControlConnect).Start();
			Console.WriteLine();
			Log.Information<int>("[RPC] Control connection started on port {Port}", port);
		}
		catch (Exception ex)
		{
			if (tryTimes > 0)
			{
				StartControlConnection(tryTimes - 1);
			}
			else
			{
				CloseControlConnection();
			}
			Console.WriteLine();
			Log.Error(ex, "[RPC] Failed to start control connection after retries");
		}
	}

	public void CloseControlConnection()
	{
		_mIsNormalExit = true;
		_mMcControlListener?.Stop();
		_mMcControlListener = null;
		CloseGameCleaning();
		Log.Information("[RPC] Control connection closed");
	}

	private void HandleAuthenticationNewVersion(byte[] data)
	{
		if (gameVersion > EnumGameVersion.V_1_18)
		{
			byte[] array = SimplePack.Pack((ushort)1799, serverIp, int.Parse(serverPort), roleName);
			if (array != null)
			{
				SendControlData(array);
			}
			Log.Information("[RPC] Sent new-version authentication to {ServerIP}:{ServerPort} | User: {UserID} | Role: {RoleName} | Protocol: {GameVersion}", new object[5] { serverIp, serverPort, userId, roleName, gameVersion });
		}
	}

	private void HandleAuthentication(byte[] data)
	{
		byte[] array = SimplePack.Pack((ushort)1031, serverIp, int.Parse(serverPort), roleName, false);
		if (array != null)
		{
			SendControlData(array);
		}
		Log.Information("[RPC] Sent authentication to {ServerIP}:{ServerPort} | User: {UserID} | Role: {RoleName} | Protocol: {GameVersion}", new object[5] { serverIp, serverPort, userId, roleName, gameVersion });
	}

	private void OnHeartBeat(byte[] data)
	{
		byte[] array = SimplePack.Pack((ushort)512, "i'am wpflauncher");
		if (array != null)
		{
			SendControlData(array);
		}
		Log.Information<string>("[RPC] Heartbeat {heartBeatMsg} sent", "i'am wpflauncher");
	}

	private void OnPCycEntityCheck(byte[] data)
	{
		Log.Information<string>("[RPC] PCyc Entity {entity} sent", "[]");
	}

	private void HandlePlayerSkin(byte[] content)
	{
		Log.Information<string>("[RPC] Event received -> {event}", "Send Player Skin");
		EntityOtherEnterWorldMsg entityOtherEnterWorldMsg = new EntityOtherEnterWorldMsg();
		new SimpleUnpack(content).Unpack(ref entityOtherEnterWorldMsg);
		TaskManager.Instance.GetFactory().StartNew(() => ProcessPlayerSkin(entityOtherEnterWorldMsg));
	}

	private async Task ProcessPlayerSkin(EntityOtherEnterWorldMsg msg)
	{
		List<EntityUserGameTexture> list = _getSkin?.Invoke(userId, userToken, new EntityUserGameTextureRequest
		{
			UserId = _skip32Cipher.ComputeUserIdFromUuid(msg.Uuid).ToString(),
			ClientType = EnumGameClientType.Java
		});
		string filePath = string.Empty;
		EnumSkinMode skinMode = EnumSkinMode.Default;
		if (list != null)
		{
			foreach (EntityUserGameTexture item in list.Where((EntityUserGameTexture s) => s.SkinId.Length > 5))
			{
				skinMode = (EnumSkinMode)item.SkinMode;
				string tempPath = Path.Combine(_dirSkinPath, "skin_" + item.SkinId + ".png");
				if (File.Exists(tempPath) && FileUtil.IsFileReadable(tempPath))
				{
					filePath = tempPath;
					break;
				}
				try
				{
					EntityAvailableUser entityAvailableUser = IUserManager.Instance?.GetAvailableUser(userId);
					if (entityAvailableUser == null)
					{
						continue;
					}
					Entity<EntityComponentDownloadInfoResponse> entity = _getComponents?.Invoke(userId, entityAvailableUser.AccessToken, item.SkinId);
					if (entity != null && entity.Code != 0)
					{
						continue;
					}
					string text = entity?.Data?.SubEntities.Select((EntityComponentDownloadInfoResponseSub sub) => sub.ResUrl).FirstOrDefault();
					if (text != null)
					{
						Directory.CreateDirectory(_dirSkinPath);
						await FileUtil.WriteFileSafelyAsync(tempPath, await _httpClient.GetByteArrayAsync(text));
						if (File.Exists(tempPath) && FileUtil.IsFileReadable(tempPath))
						{
							filePath = tempPath;
							break;
						}
					}
				}
				catch (Exception ex)
				{
					Log.Error<string>(ex, "[RPC] Failed to handle skin for player {Name}", msg.Name);
					try
					{
						if (File.Exists(tempPath))
						{
							File.Delete(tempPath);
						}
					}
					catch
					{
						Log.Error<string>(ex, "[RPC] Failed to delete temp file {Path}", filePath);
					}
				}
			}
		}
		Log.Information<string, string>("[RPC] Sending skin data for {Name}: {Path}", msg.Name, filePath);
		byte[] array = SimplePack.Pack((ushort)520, msg.Name, filePath, string.Empty, skinMode);
		if (array != null)
		{
			SendControlData(array);
		}
	}

	private static void HandleLoginGame(byte[] data)
	{
		Log.Information<string>("[RPC] Event received -> {event}", "Login Game");
	}

	private void HandleMsgFilterCheck(byte[] data)
	{
		byte[] array = SimplePack.Pack((ushort)1298, false, 0L, 0L);
		if (array != null)
		{
			SendControlData(array);
		}
		Log.Information<string>("[RPC] Event received -> {event}", "Filter Message Check");
	}

	private void OnCheckPlayerMsg(byte[] data)
	{
		EntityCheckPlayerMessage content = null;
		new SimpleUnpack(data).Unpack(ref content);
		if (content != null)
		{
			byte[] array = SimplePack.Pack((ushort)18, content.Length, content.Message);
			if (array != null)
			{
				SendControlData(array);
			}
		}
		Log.Information<string, string>("[RPC] Event received -> {event} with data {Message}", "Player Message Check", content?.Message);
	}

	private void ListenControlConnect()
	{
		try
		{
			while (!_mIsNormalExit)
			{
				TcpClient tcpClient = _mMcControlListener?.AcceptTcpClient();
				object obj;
				if (tcpClient == null)
				{
					obj = null;
				}
				else
				{
					EndPoint? remoteEndPoint = tcpClient.Client.RemoteEndPoint;
					if (remoteEndPoint == null)
					{
						obj = null;
					}
					else
					{
						string? text = remoteEndPoint.ToString();
						obj = ((text != null) ? text.Split(':')[0] : null);
					}
				}
				if ((string?)obj != IPAddress.Loopback.ToString())
				{
					tcpClient?.Close();
					continue;
				}
				Client = tcpClient;
				NetworkStream stream = tcpClient.GetStream();
				_reader = new BinaryReader(stream);
				_writer = new BinaryWriter(stream);
				SendCacheControlData();
				new Thread(OnRecvControlData).Start();
				Log.Information<string>("[RPC] Accepted control connection from {RemoteEndPoint}", tcpClient.Client.RemoteEndPoint?.ToString());
			}
		}
		catch
		{
			Log.Error("[RPC] Failed to listen control connection");
		}
	}

	private void SendControlData(byte[] message)
	{
		if (_writer == null)
		{
			_sendCache.Add(message);
			return;
		}
		byte[] buffer = BitConverter.GetBytes((ushort)message.Length).Concat(message).ToArray();
		try
		{
			_writer?.Write(buffer);
			_writer?.Flush();
		}
		catch (Exception ex)
		{
			Log.Error(ex, "[RPC] Failed to send control data");
		}
	}

	private void OnRecvControlData()
	{
		while (!_mIsNormalExit)
		{
			try
			{
				if (_reader != null)
				{
					ushort count = _reader.ReadUInt16();
					byte[] message = _reader.ReadBytes(count);
					HandleMcControlMessage(message);
				}
			}
			catch (EndOfStreamException)
			{
				break;
			}
			catch (IOException)
			{
				break;
			}
			catch (Exception ex3)
			{
				Log.Error(ex3, "[RPC] Error receiving control data");
				if (!_mIsNormalExit)
				{
					CloseGameCleaning();
				}
				break;
			}
		}
	}

	private void HandleMcControlMessage(byte[] message)
	{
		ushort num = BitConverter.ToUInt16(message, 0);
		byte[] parameters = message.Skip(2).ToArray();
		if (!_mIsLaunchIdxReady && num == 261)
		{
			_mIsLaunchIdxReady = true;
			Log.Information("[RPC] Launch index ready, executed ready actions");
		}
		_mSocketCallbackFuc.InvokeCallback(num, parameters);
	}

	private void CloseGameCleaning()
	{
		_mIsLaunchIdxReady = false;
		Log.Information("[RPC] Cleaned up game resources");
	}

	private void SendCacheControlData()
	{
		foreach (byte[] item in _sendCache)
		{
			SendControlData(item);
		}
		_sendCache.Clear();
	}
}
