#region Copyright (C) 2006 Team MediaPortal

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Xml;
using MediaPortal.Utils.Services;

namespace MediaPortal.MusicVideos.Database
{
    public class YahooSettings
    {
        private static YahooSettings instance = new YahooSettings();
        public Dictionary<string, YahooSite> moYahooSiteTable;
        //public Hashtable moYahooSiteTable;
        public List<string> moBitRateList;
        public bool mbUseVMR9 = false;
        public string msDefaultBitRate = "300";
        public string msDefaultCountryName = "USA";
        private ILog moLog;
        private YahooSettings()
        {
            ServiceProvider loServices = GlobalServiceProvider.Instance;
            moLog = loServices.Get<ILog>();
            loadSettings();
        }

        public static YahooSettings getInstance()
        {
            if (instance == null)
            {
                instance = new YahooSettings();
                  
            }
            return instance;
        }
        
        public void loadSettings()
        {
            //Read the defaults
            using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
            {
                msDefaultCountryName = xmlreader.GetValueAsString("musicvideo", "country","USA");
                msDefaultBitRate = xmlreader.GetValueAsString("musicvideo", "bitrate","768");
                mbUseVMR9 = xmlreader.GetValueAsBool("musicvideo", "useVMR9",true);                
            }
            XmlTextReader loXmlreader = null;
            try
            {
                if (moYahooSiteTable == null || moYahooSiteTable.Count < 1)
                {
                    moLog.Info("YahooSettings: loading settings");
                    //moYahooSiteTable = new Dictionary<string, YahooSite>();
                    moYahooSiteTable = new Dictionary<string, YahooSite>();
                    moBitRateList = new List<string>();
                    moLog.Info("Yahoo Settings: {0}", moBitRateList == null);
                    YahooSite loSite;
                    string lsValue;
                    using (loXmlreader = new XmlTextReader("MusicVideoSettings.xml"))
                    //using (MediaPortal.Profile.Settings loXmlreader = new MediaPortal.Profile.Settings("MusicVideoSettings.xml"))
                    {
                        while (loXmlreader.Read())
                        {
                            if (loXmlreader.NodeType == XmlNodeType.Element && loXmlreader.Name == "country")
                            {
                                loSite = new YahooSite();
                                loSite.countryName = loXmlreader.GetAttribute("name");
                                loSite.countryId = loXmlreader.GetAttribute("id");
                                loSite.NewURL = loXmlreader.GetAttribute("newURL");
                                loSite.TopURL = loXmlreader.GetAttribute("topURL");
                                loSite.SearchURL = loXmlreader.GetAttribute("searchURL");
                                loSite.GenreListURL = loXmlreader.GetAttribute("GenreListURL");
                                loSite.GenreURL = loXmlreader.GetAttribute("GenreURL");
                                moYahooSiteTable.Add(loSite.countryName, loSite);
                                //                moLog.Info("Yahoo Settings: Site created with name:{0},id={1},top={2},search={3}", loSite.countryName, loSite.countryId, loSite.TopURL, loSite.SearchURL);
                            }
                            else if (loXmlreader.Name == "bitrate")
                            {
                                //moLog.Info("node type {0}", loXmlreader.NodeType);
                                if (loXmlreader.NodeType == XmlNodeType.Element)
                                {
                                    lsValue = loXmlreader.ReadString();
                                    moBitRateList.Add(lsValue);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                moLog.Error(e);
            }
            finally
            {
                try
                {
                    loXmlreader.Close();

                    moLog.Info("Yahoo Settings: load settings closed");
                    loXmlreader = null;
                }
                catch (Exception ex)
                {
                    moLog.Info("Yahoo Settings: Exception - {0}", ex);
                }
            }
        }
        public void saveSettings()
        {
            using (MediaPortal.Profile.Settings loXmlSettings = new MediaPortal.Profile.Settings("MediaPortal.xml"))
            {
                loXmlSettings.SetValue("musicvideo", "bitrate", msDefaultBitRate);
                loXmlSettings.SetValueAsBool("musicvideo", "useVMR9", mbUseVMR9);
                loXmlSettings.SetValue("musicvideo", "country", msDefaultCountryName);
            }
        }
        public void oldSaveSettings()
        {
            XmlTextReader loReader = null;
            moLog.Info("Yahoo Settings: saving settings.");
            try
            {
                string filename = "MyMusicVideoSettings.xml";

                loReader = new XmlTextReader(filename);

                XmlDocument xmlDoc = new XmlDocument();

                try
                {
                    xmlDoc.Load(loReader);
                    loReader.Close();
                }
                catch (System.IO.FileNotFoundException)
                {
                    moLog.Info("Yahoo Settings: MyMusicVideoSettings.xml not found.");
                    //if file is not found, create a new xml file
                    //XmlTextWriter xmlWriter = new XmlTextWriter(filename, System.Text.Encoding.UTF8);
                    //xmlWriter.Formatting = Formatting.Indented;
                    //xmlWriter.WriteProcessingInstruction("xml", "version='1.0' encoding='UTF-8'");
                    //xmlWriter.WriteStartElement("FAVOURITES");

                    //xmlWriter.Close();
                    //xmlDoc.Load(filename);
                    return;
                }
                XmlNode root = xmlDoc.DocumentElement;

                XmlNodeList xmlNodeList = xmlDoc.GetElementsByTagName("country");

                XmlAttribute defaultAttribute;
                XmlAttribute nameAttribute;
                foreach (XmlNode xmlNode in xmlNodeList)
                {
                    nameAttribute = xmlNode.Attributes["name"];

                    defaultAttribute = xmlNode.Attributes["default"];

                    //moLog.Info("name attribute={0}-", nameAttribute.Value);
                    //moLog.Info("current_country={0}-", msDefaultCountryName);
                    if (nameAttribute.Value == msDefaultCountryName)
                        defaultAttribute.Value = "Y";
                    else
                        defaultAttribute.Value = "N";
                }

                xmlNodeList = xmlDoc.GetElementsByTagName("bitrate");

                string lsCurrentBitRate;
                foreach (XmlNode xmlNode in xmlNodeList)
                {
                    defaultAttribute = xmlNode.Attributes["default"];
                    lsCurrentBitRate = xmlNode.InnerXml;

                    if (lsCurrentBitRate == msDefaultBitRate)
                        defaultAttribute.Value = "Y";
                    else
                        defaultAttribute.Value = "N";
                }
                xmlDoc.Save(filename);

                //moFavoriteList.Remove(video);

            }
            catch (Exception e)
            {
                moLog.Error(e);
                moLog.Info("Yahoo Settings: save settings failed.");
            }
            finally
            {
                try
                {
                    loReader.Close();
                    moLog.Info("Yahoo Settings: save settings - reader closed");
                    loReader = null;
                }
                catch (Exception ex)
                {
                    moLog.Info("Yahoo Settings: Exception - {0}", ex);
                }
            }
        }
    }

}