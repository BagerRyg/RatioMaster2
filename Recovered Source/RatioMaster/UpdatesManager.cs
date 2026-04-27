using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace RatioMaster;

public class UpdatesManager
{
	private bool DownloadUpdatesXmlInProcess;

	public static string UpdatesXmlName = "rm_updates.xml";

	public static string UpdatesXmlUrl = "";

	private Form1 _mainForm;

	public string ApplicationDir = Application.StartupPath;

	public UpdatesManager(Form1 mainForm)
	{
		_mainForm = mainForm;
	}

	public void CheckForUpdates()
	{
		_mainForm.dgvUpdates.DataSource = null;
		_mainForm.lblCheckUpdates.Text = _mainForm.lclzManager.TranslateMessage("umUpdatesDisabled", "Online updates are disabled.");
		_mainForm.AddLogLine("Online update checks are disabled.");
	}

	public void DownloadSelectedUpdates()
	{
		RequestCachePolicy cachePolicy = new RequestCachePolicy(RequestCacheLevel.Reload);
		WebClient webClient = new WebClient();
		webClient.CachePolicy = cachePolicy;
		webClient.DownloadFileCompleted += DownloadUpdateCompleted;
		_mainForm.installProgress.Visible = true;
		_mainForm.installProgress.Minimum = 0;
		_mainForm.installProgress.Maximum = ((List<UpdatesGridViewItem>)_mainForm.dgvUpdates.DataSource).Count;
		_mainForm.installProgress.Value = 0;
		foreach (UpdatesGridViewItem item in (List<UpdatesGridViewItem>)_mainForm.dgvUpdates.DataSource)
		{
			_mainForm.installProgress.Value++;
			if (!item.UpdateSelected)
			{
				continue;
			}
			DownloadUpdate(webClient, item);
			while (webClient.IsBusy)
			{
				Thread.Sleep(1);
				try
				{
					Application.DoEvents();
				}
				catch (Exception)
				{
				}
			}
		}
		webClient.Dispose();
		_mainForm.lblCheckUpdates.Text = _mainForm.lclzManager.TranslateMessage("umFinishedDownloadingUpdates", "Finished Downloading Updates");
		DownloadUpdatesXml();
	}

	private void DownloadUpdate(WebClient client, UpdatesGridViewItem gridItem)
	{
		_mainForm.lblCheckUpdates.Text = _mainForm.lclzManager.TranslateMessageWithParams("umDownloadingUpdate", "Downloading Update {0}", gridItem.UpdateFileName);
		client.DownloadFileAsync(new Uri(gridItem.Serverpath), gridItem.Localpath + "_temp", gridItem);
	}

	private void DownloadUpdateCompleted(object sender, AsyncCompletedEventArgs e)
	{
		try
		{
			if (e.UserState == null)
			{
				_mainForm.AddLogLine("UserState is null");
				return;
			}
			UpdatesGridViewItem updatesGridViewItem = (UpdatesGridViewItem)e.UserState;
			string text = updatesGridViewItem.Localpath + "_temp";
			if (e.Error != null)
			{
				_mainForm.AddLogLine("Failed to download update:" + updatesGridViewItem.UpdateFileName + " " + updatesGridViewItem.Serverpath + ". Error: " + e.Error.Message);
				if (File.Exists(text))
				{
					File.Delete(text);
				}
				return;
			}
			if (File.Exists(text))
			{
				if (File.Exists(updatesGridViewItem.Localpath))
				{
					File.Delete(updatesGridViewItem.Localpath);
				}
				File.Move(text, updatesGridViewItem.Localpath);
			}
			_mainForm.AddLogLine("Downloading update " + updatesGridViewItem.UpdateFileName + " finished successfully.");
		}
		catch (Exception ex)
		{
			_mainForm.AddLogLine("DownloadUpdateCompleted error " + ex.Message);
		}
	}

	private void DownloadUpdatesXml()
	{
		_mainForm.installUpdatesButton.Visible = false;
		_mainForm.installProgress.Visible = false;
		_mainForm.lblCheckUpdates.Text = _mainForm.lclzManager.TranslateMessage("umCheckingUpdates", "Checking for updates...");
		WebClient webClient = new WebClient();
		webClient.DownloadFileAsync(new Uri(UpdatesXmlUrl + "/" + UpdatesXmlName), ApplicationDir + "\\" + UpdatesXmlName);
		webClient.DownloadFileCompleted += FillGridViewWithData;
	}

	public void FillGridViewWithData(object sender, AsyncCompletedEventArgs e)
	{
		_mainForm.lblCheckUpdates.Text = _mainForm.lclzManager.TranslateMessage("umParsingUpdates", "Parsing Updates...");
		DownloadUpdatesXmlInProcess = false;
		if (e.Error != null)
		{
			_mainForm.ShowMessage(e.Error.Message, "Error");
			return;
		}
		List<UpdatesGridViewItem> list = new List<UpdatesGridViewItem>();
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.Load(ApplicationDir + "\\" + UpdatesXmlName);
		XmlNodeList xmlNodeList = xmlDocument.SelectNodes("/root/clients/client");
		if (xmlNodeList != null && xmlNodeList.Count > 0)
		{
			foreach (XmlElement item in xmlNodeList)
			{
				UpdatesGridViewItem gridItemFromElement = GetGridItemFromElement(item);
				if (!IsUpdateExists(gridItemFromElement))
				{
					list.Add(gridItemFromElement);
				}
			}
		}
		XmlNodeList xmlNodeList2 = xmlDocument.SelectNodes("/root/languages/language");
		if (xmlNodeList2 != null && xmlNodeList2.Count > 0)
		{
			foreach (XmlElement item2 in xmlNodeList2)
			{
				UpdatesGridViewItem gridItemFromElement2 = GetGridItemFromElement(item2);
				if (!IsUpdateExists(gridItemFromElement2))
				{
					list.Add(gridItemFromElement2);
				}
			}
		}
		_mainForm.dgvUpdates.DataSource = list;
		if (list.Count > 0)
		{
			_mainForm.lblCheckUpdates.Text = _mainForm.lclzManager.TranslateMessageWithParams("umNumUpdatesFound", "Found {0} updates", list.Count.ToString());
			_mainForm.installUpdatesButton.Visible = true;
		}
		else
		{
			_mainForm.lblCheckUpdates.Text = _mainForm.lclzManager.TranslateMessage("umNoUpdatesFound", "No new updates found");
		}
	}

	private static bool IsUpdateExists(UpdatesGridViewItem gridItem)
	{
		if (File.Exists(gridItem.Localpath))
		{
			string text = CheckSums.Md5Hash(gridItem.Localpath);
			if (text == gridItem.FileHash)
			{
				return true;
			}
		}
		return false;
	}

	private UpdatesGridViewItem GetGridItemFromElement(XmlElement xmlElement)
	{
		UpdatesGridViewItem updatesGridViewItem = new UpdatesGridViewItem();
		updatesGridViewItem.UpdateAuthor = xmlElement.GetAttribute("author");
		updatesGridViewItem.UpdateFileName = xmlElement.GetAttribute("filename");
		updatesGridViewItem.UpdateName = xmlElement.GetAttribute("name");
		updatesGridViewItem.UpdateSelected = _mainForm.ckbToggleGridSelection.Checked;
		updatesGridViewItem.UpdateType = xmlElement.Name;
		updatesGridViewItem.FileHash = xmlElement.GetAttribute("filehash");
		updatesGridViewItem.Localpath = ApplicationDir + xmlElement.GetAttribute("localpath") + "/" + updatesGridViewItem.UpdateFileName;
		updatesGridViewItem.Serverpath = UpdatesXmlUrl + xmlElement.GetAttribute("serverpath") + "/" + updatesGridViewItem.UpdateFileName;
		return updatesGridViewItem;
	}
}
