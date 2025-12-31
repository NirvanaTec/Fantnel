using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace Codexus.Game.Launcher.Utils;

public static class DownloadUtil
{
	private static readonly HttpClient HttpClient;

	static DownloadUtil()
	{
		HttpClient = new HttpClient(new HttpClientHandler
		{
			MaxConnectionsPerServer = 16
		})
		{
			Timeout = TimeSpan.FromMinutes(10L)
		};
	}

	public static async Task<bool> DownloadAsync(string url, string destinationPath, Action<uint>? downloadProgress = null, int maxConcurrentSegments = 8, CancellationToken cancellationToken = default(CancellationToken))
	{
		long totalSize;
		long totalRead;
		Stopwatch stopwatch;
		int lastReportedProgress;
		try
		{
			string directoryName = Path.GetDirectoryName(destinationPath);
			if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
			using HttpRequestMessage headReq = new HttpRequestMessage(HttpMethod.Head, url);
			using HttpResponseMessage headResp = await HttpClient.SendAsync(headReq, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
			headResp.EnsureSuccessStatusCode();
			long? contentLength = headResp.Content.Headers.ContentLength;
			if (contentLength.HasValue)
			{
				totalSize = contentLength.GetValueOrDefault();
				if (headResp.Headers.AcceptRanges.Contains("bytes") && maxConcurrentSegments >= 2 && totalSize >= 1048576)
				{
					MemoryMappedFile mmFile = MemoryMappedFile.CreateFromFile(destinationPath, FileMode.Create, null, totalSize, MemoryMappedFileAccess.ReadWrite);
					try
					{
						ConcurrentBag<Exception> errors = new ConcurrentBag<Exception>();
						IEnumerable<(long, long)> source = CalculateRanges(maxConcurrentSegments * 3, totalSize);
						totalRead = 0L;
						stopwatch = Stopwatch.StartNew();
						lastReportedProgress = -1;
						SemaphoreSlim semaphore = new SemaphoreSlim(maxConcurrentSegments, maxConcurrentSegments);
						try
						{
							await Task.WhenAll(source.Select<(long, long), Task>(async delegate((long Start, long End) range)
							{
								await semaphore.WaitAsync(cancellationToken);
								try
								{
									for (int attempt = 1; attempt <= 3; attempt++)
									{
										cancellationToken.ThrowIfCancellationRequested();
										try
										{
											using (HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, url))
											{
												req.Headers.Range = new RangeHeaderValue(range.Start, range.End);
												using HttpResponseMessage resp = await HttpClient.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
												resp.EnsureSuccessStatusCode();
												await using Stream netStream = await resp.Content.ReadAsStreamAsync(cancellationToken);
												await using (MemoryMappedViewStream viewStream = mmFile.CreateViewStream(range.Start, range.End - range.Start + 1, MemoryMappedFileAccess.Write))
												{
													byte[] buffer = new byte[8192];
													while (true)
													{
														int num;
														int bytesRead = (num = await netStream.ReadAsync(buffer, cancellationToken));
														if (num <= 0)
														{
															break;
														}
														await viewStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
														ReportProgressThrottled(bytesRead);
													}
												}
												break;
											}
											IL_0577:;
										}
										catch (Exception ex3) when (attempt < 3 && !(ex3 is OperationCanceledException))
										{
											await Task.Delay(500 * attempt, cancellationToken);
										}
									}
								}
								catch (Exception item)
								{
									errors.Add(item);
								}
								finally
								{
									semaphore.Release();
								}
							}));
							if (!errors.IsEmpty)
							{
								throw new AggregateException(errors);
							}
							downloadProgress?.Invoke(100u);
							return true;
						}
						finally
						{
							if (semaphore != null)
							{
								((IDisposable)semaphore).Dispose();
							}
						}
					}
					finally
					{
						if (mmFile != null)
						{
							((IDisposable)mmFile).Dispose();
						}
					}
				}
			}
			return await SingleDownloadAsync(url, destinationPath, downloadProgress, cancellationToken);
		}
		catch (OperationCanceledException)
		{
			Log.Information<string>("Download canceled: {Url}", url);
			throw;
		}
		catch (Exception ex2)
		{
			Log.Error<string>(ex2, "Download failed for {Url}", url);
			return false;
		}
		void ReportProgressThrottled(long bytesRead)
		{
			long num = Interlocked.Add(ref totalRead, bytesRead);
			if (stopwatch.ElapsedMilliseconds > 150)
			{
				stopwatch.Restart();
				int num2 = (int)((double)num * 100.0 / (double)totalSize);
				if (num2 > lastReportedProgress)
				{
					lastReportedProgress = num2;
					downloadProgress?.Invoke((uint)num2);
				}
			}
		}
	}

	private static async Task<bool> SingleDownloadAsync(string url, string destinationPath, Action<uint>? downloadProgress, CancellationToken cancellationToken)
	{
		using HttpResponseMessage resp = await HttpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
		resp.EnsureSuccessStatusCode();
		long total = resp.Content.Headers.ContentLength.GetValueOrDefault();
		long read = 0L;
		bool result;
		await using (Stream input = await resp.Content.ReadAsStreamAsync(cancellationToken))
		{
			bool flag;
			await using (FileStream output = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, useAsync: true))
			{
				byte[] buffer = new byte[8192];
				Stopwatch stopwatch = Stopwatch.StartNew();
				int lastReportedProgress = -1;
				while (true)
				{
					int num;
					int n = (num = await input.ReadAsync(buffer, cancellationToken));
					if (num <= 0)
					{
						break;
					}
					await output.WriteAsync(buffer.AsMemory(0, n), cancellationToken);
					if (total <= 0)
					{
						continue;
					}
					read += n;
					if (stopwatch.ElapsedMilliseconds > 150)
					{
						stopwatch.Restart();
						int num2 = (int)((double)read * 100.0 / (double)total);
						if (num2 > lastReportedProgress)
						{
							lastReportedProgress = num2;
							downloadProgress?.Invoke((uint)num2);
						}
					}
				}
				downloadProgress?.Invoke(100u);
				flag = true;
			}
			result = flag;
		}
		return result;
	}

	private static IEnumerable<(long Start, long End)> CalculateRanges(int segments, long totalSize)
	{
		long segmentSize = totalSize / segments;
		for (int i = 0; i < segments; i++)
		{
			long item = i * segmentSize;
			long item2 = ((i == segments - 1) ? (totalSize - 1) : ((i + 1) * segmentSize - 1));
			yield return (Start: item, End: item2);
		}
	}
}
