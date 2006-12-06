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
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Xml;
using DirectShowLib;


using TvDatabase;
using TvControl;
using TvLibrary;
using TvLibrary.Log;
using TvLibrary.Channels;
using TvLibrary.Interfaces;
using TvLibrary.Implementations;
using DirectShowLib.BDA;

namespace SetupTv.Sections
{
  public partial class CardDvbS : SectionSettings
  {
    #region private classes
    class SatteliteContext : IComparable<SatteliteContext>
    {
      public string SatteliteName;
      public string FileName;
      public Satellite Satelite;
      public SatteliteContext()
      {
        Satelite = null;
        FileName = "";
        SatteliteName = "";
      }
      public override string ToString()
      {
        return SatteliteName;
      }
      public int CompareTo(SatteliteContext other)
      {
        return SatteliteName.CompareTo(other.SatteliteName);
      }


      #region IComparable<SatteliteContext> Members

      int IComparable<SatteliteContext>.CompareTo(SatteliteContext other)
      {
        return SatteliteName.CompareTo(other.SatteliteName);
      }

      #endregion
    }

    class Transponder : IComparable<Transponder>
    {
      public int CarrierFrequency; // frequency
      public Polarisation Polarisation;  // polarisation 0=hori, 1=vert
      public int SymbolRate; // symbol rate

      public int CompareTo(Transponder other)
      {
        if (Polarisation < other.Polarisation) return 1;
        if (Polarisation > other.Polarisation) return -1;
        if (CarrierFrequency > other.CarrierFrequency) return 1;
        if (CarrierFrequency < other.CarrierFrequency) return -1;
        if (SymbolRate > other.SymbolRate) return 1;
        if (SymbolRate < other.SymbolRate) return -1;
        return 0;
      }
      public override string ToString()
      {
        return String.Format("{0} {1} {2}", CarrierFrequency, SymbolRate, Polarisation);
      }
    }
    #endregion

    #region variables
    int _cardNumber;
    List<Transponder> _transponders = new List<Transponder>();
    int _channelCount = 0;

    int _tvChannelsNew = 0;
    int _radioChannelsNew = 0;
    int _tvChannelsUpdated = 0;
    int _radioChannelsUpdated = 0;
    bool _isScanning = false;
    bool _stopScanning = false;
    bool _enableEvents = false;
    #endregion

    #region ctors
    public CardDvbS()
      : this("DVBC")
    {
    }
    public CardDvbS(string name)
      : base(name)
    {
    }

    public CardDvbS(string name, int cardNumber)
      : base(name)
    {
      _cardNumber = cardNumber;
      InitializeComponent();
      base.Text = name;
      Init();
    }
    #endregion

    #region helper methods
    SatteliteContext LoadSatteliteName(string fileName)
    {
      SatteliteContext ts = new SatteliteContext();
      ts.FileName = @"Tuningparameters\" + fileName;
      ts.SatteliteName = fileName;

      string line;
      System.IO.TextReader tin = System.IO.File.OpenText(@"Tuningparameters\" + fileName);
      while (true)
      {
        line = tin.ReadLine();
        if (line == null) break;
        string search = line.ToLower();
        int pos = search.IndexOf("satname");
        if (pos >= 0)
        {
          pos = search.IndexOf("=");
          if (pos > 0)
          {
            ts.SatteliteName = line.Substring(pos + 1);
            ts.SatteliteName = ts.SatteliteName.Trim();
            break;
          }
        }
      }
      tin.Close();

      return ts;
    }
    void LoadTransponders(string SatteliteContextFileName)
    {
      _transponders.Clear();
      _channelCount = 0;
      string line;
      string[] tpdata;
      // load transponder list and start scan
      System.IO.TextReader tin = System.IO.File.OpenText(SatteliteContextFileName);
      int _count = 0;
      do
      {
        line = null;
        line = tin.ReadLine();
        if (line != null)
          if (line.Length > 0)
          {
            if (line.StartsWith(";"))
              continue;
            tpdata = line.Split(new char[] { ',' });
            if (tpdata.Length != 3)
              tpdata = line.Split(new char[] { ';' });
            if (tpdata.Length == 3)
            {
              try
              {

                Transponder transponder = new Transponder();
                transponder.CarrierFrequency = Int32.Parse(tpdata[0]) * 1000;
                switch (tpdata[1].ToLower())
                {
                  case "v":
                    transponder.Polarisation = Polarisation.LinearV;
                    break;
                  case "h":
                    transponder.Polarisation = Polarisation.LinearH;
                    break;
                  case "r":
                    transponder.Polarisation = Polarisation.CircularR;
                    break;
                  case "l":
                    transponder.Polarisation = Polarisation.CircularL;
                    break;
                  default:
                    transponder.Polarisation = Polarisation.LinearH;
                    break;
                }
                transponder.SymbolRate = Int32.Parse(tpdata[2]);
                _transponders.Add(transponder);
                _count += 1;
              }
              catch
              { }
            }
          }
      } while (!(line == null));
      tin.Close();
      _channelCount = _count;
      _transponders.Sort();
    }
    List<SatteliteContext> LoadSattelites()
    {
      string[] files = System.IO.Directory.GetFiles(System.IO.Directory.GetCurrentDirectory() + @"\Tuningparameters", "*.tpl");
      List<SatteliteContext> satellites = new List<SatteliteContext>();

      foreach (string file in files)
      {
        string fileName = System.IO.Path.GetFileName(file);
        SatteliteContext ts = LoadSatteliteName(fileName);
        if (ts != null)
        {
          satellites.Add(ts);
        }
      }
      satellites.Sort();
      IList dbSats = Satellite.ListAll();
      foreach (SatteliteContext ts in satellites)
      {
        foreach (Satellite dbSat in dbSats)
        {
          string name = "";
          for (int i = 0; i < ts.SatteliteName.Length; ++i)
          {
            if (ts.SatteliteName[i] >= (char)32 && ts.SatteliteName[i] < (char)127)
              name += ts.SatteliteName[i];
          }
          if (String.Compare(name, dbSat.SatelliteName, true) == 0)
          {
            ts.Satelite = dbSat;
            break;
          }
        }
        if (ts.Satelite == null)
        {
          string name = "";
          for (int i = 0; i < ts.SatteliteName.Length; ++i)
          {
            if (ts.SatteliteName[i] >= (char)32 && ts.SatteliteName[i] < (char)127)
              name += ts.SatteliteName[i];
          }
          ts.Satelite = new Satellite(name, ts.FileName);
          ts.Satelite.Persist();
        }
      }
      return satellites;
    }
    #endregion

    #region DVB-S scanning tab
    void Init()
    {
      mpTransponder1.Items.Clear();
      mpTransponder2.Items.Clear();
      mpTransponder3.Items.Clear();
      mpTransponder4.Items.Clear();
      List<SatteliteContext> satellites = LoadSattelites();

      foreach (SatteliteContext ts in satellites)
      {
        mpTransponder1.Items.Add(ts);
        mpTransponder2.Items.Add(ts);
        mpTransponder3.Items.Add(ts);
        mpTransponder4.Items.Add(ts);
      }
      if (mpTransponder1.Items.Count > 0)
        mpTransponder1.SelectedIndex = 0;
      if (mpTransponder2.Items.Count > 0)
        mpTransponder2.SelectedIndex = 0;
      if (mpTransponder3.Items.Count > 0)
        mpTransponder3.SelectedIndex = 0;
      if (mpTransponder4.Items.Count > 0)
        mpTransponder4.SelectedIndex = 0;

      mpDisEqc1.Items.Clear();
      mpDisEqc1.Items.Add(DisEqcType.None);
      mpDisEqc1.Items.Add(DisEqcType.SimpleA);
      mpDisEqc1.Items.Add(DisEqcType.SimpleB);
      mpDisEqc1.Items.Add(DisEqcType.Level1AA);
      mpDisEqc1.Items.Add(DisEqcType.Level1BA);
      mpDisEqc1.Items.Add(DisEqcType.Level1AB);
      mpDisEqc1.Items.Add(DisEqcType.Level1BB);
      mpDisEqc1.SelectedIndex = 0;

      mpDisEqc2.Items.Clear();
      mpDisEqc2.Items.Add(DisEqcType.None);
      mpDisEqc2.Items.Add(DisEqcType.SimpleA);
      mpDisEqc2.Items.Add(DisEqcType.SimpleB);
      mpDisEqc2.Items.Add(DisEqcType.Level1AA);
      mpDisEqc2.Items.Add(DisEqcType.Level1BA);
      mpDisEqc2.Items.Add(DisEqcType.Level1AB);
      mpDisEqc2.Items.Add(DisEqcType.Level1BB);
      mpDisEqc2.SelectedIndex = 0;

      mpDisEqc3.Items.Clear();
      mpDisEqc3.Items.Add(DisEqcType.None);
      mpDisEqc3.Items.Add(DisEqcType.SimpleA);
      mpDisEqc3.Items.Add(DisEqcType.SimpleB);
      mpDisEqc3.Items.Add(DisEqcType.Level1AA);
      mpDisEqc3.Items.Add(DisEqcType.Level1BA);
      mpDisEqc3.Items.Add(DisEqcType.Level1AB);
      mpDisEqc3.Items.Add(DisEqcType.Level1BB);
      mpDisEqc3.SelectedIndex = 0;

      mpDisEqc4.Items.Clear();
      mpDisEqc4.Items.Add(DisEqcType.None);
      mpDisEqc4.Items.Add(DisEqcType.SimpleA);
      mpDisEqc4.Items.Add(DisEqcType.SimpleB);
      mpDisEqc4.Items.Add(DisEqcType.Level1AA);
      mpDisEqc4.Items.Add(DisEqcType.Level1BA);
      mpDisEqc4.Items.Add(DisEqcType.Level1AB);
      mpDisEqc4.Items.Add(DisEqcType.Level1BB);
      mpDisEqc4.SelectedIndex = 0;

      TvBusinessLayer layer = new TvBusinessLayer();
      mpTransponder1.SelectedIndex = Int32.Parse(layer.GetSetting("dvbs" + _cardNumber.ToString() + "SatteliteContext1", "0").Value);
      mpTransponder2.SelectedIndex = Int32.Parse(layer.GetSetting("dvbs" + _cardNumber.ToString() + "SatteliteContext2", "0").Value);
      mpTransponder3.SelectedIndex = Int32.Parse(layer.GetSetting("dvbs" + _cardNumber.ToString() + "SatteliteContext3", "0").Value);
      mpTransponder4.SelectedIndex = Int32.Parse(layer.GetSetting("dvbs" + _cardNumber.ToString() + "SatteliteContext4", "0").Value);


      mpDisEqc1.SelectedIndex = Int32.Parse(layer.GetSetting("dvbs" + _cardNumber.ToString() + "DisEqc1", "0").Value);
      mpDisEqc2.SelectedIndex = Int32.Parse(layer.GetSetting("dvbs" + _cardNumber.ToString() + "DisEqc2", "0").Value);
      mpDisEqc3.SelectedIndex = Int32.Parse(layer.GetSetting("dvbs" + _cardNumber.ToString() + "DisEqc3", "0").Value);
      mpDisEqc4.SelectedIndex = Int32.Parse(layer.GetSetting("dvbs" + _cardNumber.ToString() + "DisEqc4", "0").Value);

      mpBand1.SelectedIndex = Int32.Parse(layer.GetSetting("dvbs" + _cardNumber.ToString() + "band1", "0").Value);
      mpBand2.SelectedIndex = Int32.Parse(layer.GetSetting("dvbs" + _cardNumber.ToString() + "band2", "0").Value);
      mpBand3.SelectedIndex = Int32.Parse(layer.GetSetting("dvbs" + _cardNumber.ToString() + "band3", "0").Value);
      mpBand4.SelectedIndex = Int32.Parse(layer.GetSetting("dvbs" + _cardNumber.ToString() + "band4", "0").Value);

      mpLNB1.Checked = (layer.GetSetting("dvbs" + _cardNumber.ToString() + "LNB1", "false").Value == "true");
      mpLNB2.Checked = (layer.GetSetting("dvbs" + _cardNumber.ToString() + "LNB2", "false").Value == "true");
      mpLNB3.Checked = (layer.GetSetting("dvbs" + _cardNumber.ToString() + "LNB3", "false").Value == "true");
      mpLNB4.Checked = (layer.GetSetting("dvbs" + _cardNumber.ToString() + "LNB4", "false").Value == "true");
      mpLNB1_CheckedChanged(null, null); ;
      mpLNB2_CheckedChanged(null, null); ;
      mpLNB3_CheckedChanged(null, null); ;
      mpLNB4_CheckedChanged(null, null); ;


      checkBoxCreateGroups.Checked = (layer.GetSetting("dvbs" + _cardNumber.ToString() + "creategroups", "true").Value == "true");


      Card card = layer.GetCardByDevicePath(RemoteControl.Instance.CardDevice(_cardNumber));
      mpComboBoxCam.SelectedIndex = card.CamType;
    }
    public override void OnSectionDeActivated()
    {
      timer1.Enabled = false;
      TvBusinessLayer layer = new TvBusinessLayer();
      base.OnSectionDeActivated();
      Setting setting;
      setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "creategroups", "false");
      setting.Value = checkBoxCreateGroups.Checked ? "true" : "false";
      setting.Persist();

      setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "SatteliteContext1", "0");
      setting.Value = mpTransponder1.SelectedIndex.ToString();
      setting.Persist();
      setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "SatteliteContext2", "0");
      setting.Value = mpTransponder2.SelectedIndex.ToString();
      setting.Persist();
      setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "SatteliteContext3", "0");
      setting.Value = mpTransponder3.SelectedIndex.ToString();
      setting.Persist();
      setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "SatteliteContext4", "0");
      setting.Value = mpTransponder4.SelectedIndex.ToString();
      setting.Persist();

      setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "DisEqc1", "0");
      setting.Value = mpDisEqc1.SelectedIndex.ToString();
      setting.Persist();
      setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "DisEqc2", "0");
      setting.Value = mpDisEqc2.SelectedIndex.ToString();
      setting.Persist();
      setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "DisEqc3", "0");
      setting.Value = mpDisEqc3.SelectedIndex.ToString();
      setting.Persist();
      setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "DisEqc4", "0");
      setting.Value = mpDisEqc4.SelectedIndex.ToString();
      setting.Persist();


      setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "band1", "0");
      setting.Value = mpBand1.SelectedIndex.ToString();
      setting.Persist();
      setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "band2", "0");
      setting.Value = mpBand2.SelectedIndex.ToString();
      setting.Persist();
      setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "band3", "0");
      setting.Value = mpBand3.SelectedIndex.ToString();
      setting.Persist();
      setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "band4", "0");
      setting.Value = mpBand4.SelectedIndex.ToString();
      setting.Persist();


      setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "LNB1", "false");
      setting.Value = mpLNB1.Checked ? "true" : "false";
      setting.Persist();

      setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "LNB2", "false");
      setting.Value = mpLNB2.Checked ? "true" : "false";
      setting.Persist();
      setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "LNB3", "false");
      setting.Value = mpLNB3.Checked ? "true" : "false";
      setting.Persist();
      setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "LNB4", "false");
      setting.Value = mpLNB4.Checked ? "true" : "false";
      setting.Persist();
    }

    void UpdateStatus(int LNB)
    {
      progressBarLevel.Value = Math.Min(100, RemoteControl.Instance.SignalLevel(_cardNumber));
      progressBarQuality.Value = Math.Min(100, RemoteControl.Instance.SignalQuality(_cardNumber));
      progressBarSatLevel.Value = Math.Min(100, RemoteControl.Instance.SignalLevel(_cardNumber));
      progressBarSatQuality.Value = Math.Min(100, RemoteControl.Instance.SignalQuality(_cardNumber));


    }

    public override void OnSectionActivated()
    {
      base.OnSectionActivated();
      UpdateStatus(1);
    }



    private void mpButtonScanTv_Click(object sender, EventArgs e)
    {
      if (_isScanning == false)
      {
        TvBusinessLayer layer = new TvBusinessLayer();
        Card card = layer.GetCardByDevicePath(RemoteControl.Instance.CardDevice(_cardNumber));
        if (card.Enabled == false)
        {
          MessageBox.Show(this, "Card is disabled, please enable the card before scanning");
          return;
        }
        Thread scanThread = new Thread(new ThreadStart(DoScan));
        scanThread.Start();
      }
      else
      {
        _stopScanning = true;
      }
    }
    void DoScan()
    {

      string buttonText = mpButtonScanTv.Text;
      try
      {
        _isScanning = true;
        _stopScanning = false;
        mpButtonScanTv.Text = "Cancel...";
        RemoteControl.Instance.EpgGrabberEnabled = false;
        mpTransponder1.Enabled = false;
        mpTransponder2.Enabled = false;
        mpTransponder3.Enabled = false;
        mpTransponder4.Enabled = false;
        mpDisEqc1.Enabled = false;
        mpDisEqc2.Enabled = false;
        mpDisEqc3.Enabled = false;
        mpDisEqc4.Enabled = false;
        mpLNB1.Enabled = false;
        mpLNB2.Enabled = false;
        mpLNB3.Enabled = false;
        mpLNB4.Enabled = false;
        mpBand1.Enabled = false;
        mpBand2.Enabled = false;
        mpBand3.Enabled = false;
        mpBand4.Enabled = false;

        listViewStatus.Items.Clear();
        _tvChannelsNew = 0;
        _radioChannelsNew = 0;
        _tvChannelsUpdated = 0;
        _radioChannelsUpdated = 0;

        if (mpLNB1.Checked)
          Scan(1, (BandType)mpBand1.SelectedIndex, (DisEqcType)mpDisEqc1.SelectedIndex, (SatteliteContext)mpTransponder1.SelectedItem);
        if (_stopScanning) return;

        if (mpLNB2.Checked)
          Scan(2, (BandType)mpBand2.SelectedIndex, (DisEqcType)mpDisEqc2.SelectedIndex, (SatteliteContext)mpTransponder2.SelectedItem);
        if (_stopScanning) return;

        if (mpLNB3.Checked)
          Scan(3, (BandType)mpBand3.SelectedIndex, (DisEqcType)mpDisEqc3.SelectedIndex, (SatteliteContext)mpTransponder3.SelectedItem);
        if (_stopScanning) return;

        if (mpLNB4.Checked)
          Scan(4, (BandType)mpBand4.SelectedIndex, (DisEqcType)mpDisEqc2.SelectedIndex, (SatteliteContext)mpTransponder4.SelectedItem);

        ListViewItem item = listViewStatus.Items.Add(new ListViewItem(String.Format("Total radio channels new:{0} updated:{1}", _radioChannelsNew, _radioChannelsUpdated)));
        item = listViewStatus.Items.Add(new ListViewItem(String.Format("Total tv channels new:{0} updated:{1}", _tvChannelsNew, _tvChannelsUpdated)));
        item = listViewStatus.Items.Add(new ListViewItem("Scan done..."));
        item.EnsureVisible();
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
      finally
      {
        RemoteControl.Instance.EpgGrabberEnabled = true;
        mpTransponder1.Enabled = true;
        mpTransponder2.Enabled = true;
        mpTransponder3.Enabled = true;
        mpTransponder4.Enabled = true;
        mpDisEqc1.Enabled = true;
        mpDisEqc2.Enabled = true;
        mpDisEqc3.Enabled = true;
        mpDisEqc4.Enabled = true;
        mpBand1.Enabled = true;
        mpBand2.Enabled = true;
        mpBand3.Enabled = true;
        mpBand4.Enabled = true;
        progressBar1.Value = 100;

        mpLNB1.Enabled = true;
        mpLNB2.Enabled = true;
        mpLNB3.Enabled = true;
        mpLNB4.Enabled = true;
        mpButtonScanTv.Text = buttonText;
        _isScanning = false;
      }
    }

    void Scan(int LNB, BandType bandType, DisEqcType disEqc, SatteliteContext SatteliteContext)
    {
      LoadTransponders(SatteliteContext.FileName);
      if (_channelCount == 0) return;


      TvBusinessLayer layer = new TvBusinessLayer();
      Card card = layer.GetCardByDevicePath(RemoteControl.Instance.CardDevice(_cardNumber));

      int position = -1;
      Setting setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "motorEnabled", "no");
      if (setting.Value == "yes")
      {
        foreach (DiSEqCMotor motor in card.ReferringDiSEqCMotor())
        {
          if (motor.IdSatellite == SatteliteContext.Satelite.IdSatellite)
          {
            position = motor.Position;
            break;
          }
        }
      }

      for (int index = 0; index < _channelCount; ++index)
      {
        if (_stopScanning) return;
        float percent = ((float)(index)) / _channelCount;
        percent *= 100f;
        if (percent > 100f) percent = 100f;
        progressBar1.Value = (int)percent;


        DVBSChannel tuneChannel = new DVBSChannel();
        tuneChannel.Frequency = _transponders[index].CarrierFrequency;
        tuneChannel.Polarisation = _transponders[index].Polarisation;
        tuneChannel.SymbolRate = _transponders[index].SymbolRate;
        tuneChannel.BandType = bandType;
        tuneChannel.SatelliteIndex = position;

        tuneChannel.DisEqc = disEqc;
        string line = String.Format("lnb:{0} {1}tp- {2} {3} {4}", LNB, 1 + index, tuneChannel.Frequency, tuneChannel.Polarisation, tuneChannel.SymbolRate);
        ListViewItem item = listViewStatus.Items.Add(new ListViewItem(line));
        item.EnsureVisible();

        if (index == 0)
        {
          RemoteControl.Instance.Tune(_cardNumber, tuneChannel);
        }
        UpdateStatus(LNB);

        IChannel[] channels = RemoteControl.Instance.Scan(_cardNumber, tuneChannel);
        UpdateStatus(LNB);

        if (channels == null || channels.Length == 0)
        {
          if (RemoteControl.Instance.TunerLocked(_cardNumber) == false)
          {
            line = String.Format("lnb:{0} {1}tp- {2} {3} {4}:No signal", LNB, 1 + index, tuneChannel.Frequency, tuneChannel.Polarisation, tuneChannel.SymbolRate);
            item.Text = line;
            item.ForeColor = Color.Red;
            continue;
          }
          else
          {
            line = String.Format("lnb:{0} {1}tp- {2} {3} {4}:Nothing found", LNB, 1 + index, tuneChannel.Frequency, tuneChannel.Polarisation, tuneChannel.SymbolRate);
            item.Text = line;
            item.ForeColor = Color.Red;
            continue;
          }

        }


        int newChannels = 0;
        int updatedChannels = 0;
        for (int i = 0; i < channels.Length; ++i)
        {
          DVBSChannel channel = (DVBSChannel)channels[i];
          Channel dbChannel = layer.GetChannelByName(channel.Name);
          bool exists = (dbChannel != null);
          if (!exists)
          {
            dbChannel = layer.AddChannel(channel.Name);
          }
          dbChannel.IsTv = channel.IsTv;
          dbChannel.IsRadio = channel.IsRadio;
          if (dbChannel.IsRadio)
          {
            dbChannel.GrabEpg = false;
          }
          dbChannel.SortOrder = 10000;
          if (channel.LogicalChannelNumber >= 1)
          {
            dbChannel.SortOrder = channel.LogicalChannelNumber;
          }
          dbChannel.Persist();

          if (checkBoxCreateGroups.Checked)
          {
            layer.AddChannelToGroup(dbChannel, channel.Provider);
          }
          layer.AddTuningDetails(dbChannel, channel);
          if (channel.IsTv)
          {
            if (exists)
            {
              _tvChannelsUpdated++;
              updatedChannels++;
            }
            else
            {
              _tvChannelsNew++;
              newChannels++;
            }
          }
          if (channel.IsRadio)
          {
            if (exists)
            {
              _radioChannelsUpdated++;
              updatedChannels++;
            }
            else
            {
              _radioChannelsNew++;
              newChannels++;
            }
          }
          layer.MapChannelToCard(card, dbChannel);
          line = String.Format("lnb:{0} {1}tp- {2} {3} {4}:New:{5} Updated:{6}",
              LNB, 1 + index, tuneChannel.Frequency, tuneChannel.Polarisation, tuneChannel.SymbolRate, newChannels, updatedChannels);
          item.Text = line;
        }
      }

      // DatabaseManager.Instance.SaveChanges();

    }

    private void mpLNB2_CheckedChanged(object sender, EventArgs e)
    {
      mpTransponder2.Enabled = mpLNB2.Checked;
      mpDisEqc2.Enabled = mpLNB2.Checked;
    }

    private void mpLNB3_CheckedChanged(object sender, EventArgs e)
    {
      mpTransponder3.Enabled = mpLNB3.Checked;
      mpDisEqc3.Enabled = mpLNB3.Checked;
    }

    private void mpLNB4_CheckedChanged(object sender, EventArgs e)
    {
      mpTransponder4.Enabled = mpLNB4.Checked;
      mpDisEqc4.Enabled = mpLNB4.Checked;
    }

    private void CardDvbS_Load(object sender, EventArgs e)
    {

    }

    private void mpComboBoxCam_SelectedIndexChanged(object sender, EventArgs e)
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      Card card = layer.GetCardByDevicePath(RemoteControl.Instance.CardDevice(_cardNumber));
      card.CamType = mpComboBoxCam.SelectedIndex;
      card.Persist();
    }

    private void mpBand1_SelectedIndexChanged(object sender, EventArgs e)
    {

    }

    private void mpComboBox1_SelectedIndexChanged(object sender, EventArgs e)
    {

    }

    private void mpComboBox1_SelectedIndexChanged_1(object sender, EventArgs e)
    {

    }

    private void mpTransponder4_SelectedIndexChanged(object sender, EventArgs e)
    {

    }

    private void mpTransponder3_SelectedIndexChanged(object sender, EventArgs e)
    {

    }

    private void mpTransponder2_SelectedIndexChanged(object sender, EventArgs e)
    {

    }

    private void mpTransponder1_SelectedIndexChanged(object sender, EventArgs e)
    {

    }

    private void mpLNB1_CheckedChanged(object sender, EventArgs e)
    {
      mpTransponder1.Enabled = mpLNB1.Checked;
      mpDisEqc1.Enabled = mpLNB1.Checked;
    }
    #endregion

    #region DiSEqC Motor tab

    void SetupMotor()
    {
      _enableEvents = false;
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "motorEnabled", "no");
      bool enabled = false;
      if (setting.Value == "yes")
      {
        enabled = true;
      }
      checkBox1.Checked = enabled;
      checkBox1_CheckedChanged(null, null);

      comboBoxStepSize.Items.Clear();
      for (int i = 1; i < 127; ++i)
        comboBoxStepSize.Items.Add(i.ToString());

      setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "motorStepSize", "10");
      int stepsize = 10;
      if (Int32.TryParse(setting.Value, out stepsize))
        comboBoxStepSize.SelectedIndex = stepsize - 1;
      else
        comboBoxStepSize.SelectedIndex = 9;

      comboBoxSat.Items.Clear();

      setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "selectedMotorSat", "0");
      int index = 0;
      Int32.TryParse(setting.Value, out index);

      List<SatteliteContext> satellites = LoadSattelites();

      foreach (SatteliteContext sat in satellites)
      {
        comboBoxSat.Items.Add(sat);
      }
      if (index >= 0 && index < satellites.Count)
        comboBoxSat.SelectedIndex = index;
      else
        comboBoxSat.SelectedIndex = 0;
      LoadMotorTransponder();
      _enableEvents = true;
    }
    private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (tabControl1.SelectedIndex == 1)
      {
        SetupMotor();
        timer1.Enabled = true;
      }
      else
      {
        timer1.Enabled = false;
      }
    }

    private void buttonMoveWest_Click(object sender, EventArgs e)
    {
      if (_enableEvents == false) return;
      //move motor west
      RemoteControl.Instance.DiSEqCDriveMotor(_cardNumber, DiSEqCDirection.West, (byte)(1 + comboBoxStepSize.SelectedIndex));
    }

    private void buttonSetWestLimit_Click(object sender, EventArgs e)
    {
      if (_enableEvents == false) return;
      //set motor west limit
      RemoteControl.Instance.DiSEqCSetWestLimit(_cardNumber);
    }

    private void tabPage2_Click(object sender, EventArgs e)
    {

    }

    private void button1_Click(object sender, EventArgs e)
    {
      if (_enableEvents == false) return;
      //goto selected sat
      if (comboBoxSat.SelectedIndex < 0) return;
      TvBusinessLayer layer = new TvBusinessLayer();
      SatteliteContext sat = (SatteliteContext)comboBoxSat.Items[comboBoxSat.SelectedIndex];

      Card card = Card.Retrieve(_cardNumber);
      IList motorSettings = card.ReferringDiSEqCMotor();
      foreach (DiSEqCMotor motor in motorSettings)
      {
        if (motor.IdSatellite == sat.Satelite.IdSatellite)
        {
          RemoteControl.Instance.DiSEqCGotoPosition(_cardNumber, (byte)motor.Position);
          MessageBox.Show("Satellite moving to position:" + motor.Position.ToString(), "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
          comboBox1_SelectedIndexChanged(null, null);
          return;
        }
      }
      MessageBox.Show("No position stored for this satellite", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
    }

    private void buttonStore_Click(object sender, EventArgs e)
    {
      if (_enableEvents == false) return;
      //store motor position..
      int index = -1;
      SatteliteContext sat = (SatteliteContext)comboBoxSat.SelectedItem;
      TvBusinessLayer layer = new TvBusinessLayer();
      Card card = Card.Retrieve(_cardNumber);
      IList motorSettings = card.ReferringDiSEqCMotor();
      foreach (DiSEqCMotor motor in motorSettings)
      {
        if (motor.IdSatellite == sat.Satelite.IdSatellite)
        {
          index = motor.Position;
          break;
        }
      }
      if (index < 0)
      {
        index = motorSettings.Count + 1;
        DiSEqCMotor motor = new DiSEqCMotor(card.IdCard, sat.Satelite.IdSatellite, index);
        motor.Persist();
      }
      RemoteControl.Instance.DiSEqCStorePosition(_cardNumber, (byte)(index));
      MessageBox.Show("Satellite position stored to:" + index.ToString(), "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

    }

    private void buttonMoveEast_Click(object sender, EventArgs e)
    {
      if (_enableEvents == false) return;
      //move motor east
      RemoteControl.Instance.DiSEqCDriveMotor(_cardNumber, DiSEqCDirection.East, (byte)(1 + comboBoxStepSize.SelectedIndex));
    }

    private void buttonSetEastLimit_Click(object sender, EventArgs e)
    {
      if (_enableEvents == false) return;
      //set motor east limit
      RemoteControl.Instance.DiSEqCSetEastLimit(_cardNumber);
    }

    private void comboBoxSat_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (_enableEvents == false) return;
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "selectedMotorSat", "0");
      setting.Value = comboBoxSat.SelectedIndex.ToString();
      setting.Persist();
      LoadMotorTransponder();
      comboBox1_SelectedIndexChanged(null, null);
    }

    private void checkBoxEnabled_CheckedChanged(object sender, EventArgs e)
    {
      if (_enableEvents == false) return;
      TvBusinessLayer layer = new TvBusinessLayer();
      if (checkBoxEnabled.Checked)
      {
        RemoteControl.Instance.DiSEqCForceLimit(_cardNumber, true);
        Setting setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "limitsEnabled", "yes");
        setting.Value = "yes";
        setting.Persist();
      }
      else
      {
        if (MessageBox.Show("Disabling the east/west limits could damage your dish!!! Are you sure?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
        {
          RemoteControl.Instance.DiSEqCForceLimit(_cardNumber, false);
          Setting setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "limitsEnabled", "yes");
          setting.Value = "no";
          setting.Persist();
        }
        else
        {
          _enableEvents = false;
          checkBoxEnabled.Checked = true;
          RemoteControl.Instance.DiSEqCForceLimit(_cardNumber, true);
          Setting setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "limitsEnabled", "yes");
          setting.Value = "yes";
          setting.Persist();
          _enableEvents = true;
        }
      }
    }

    void LoadMotorTransponder()
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "limitsEnabled", "yes");
      if (setting.Value == "yes")
        checkBoxEnabled.Checked = true;
      if (setting.Value == "no")
        checkBoxEnabled.Checked = false;
      comboBox1.Items.Clear();
      SatteliteContext sat = (SatteliteContext)comboBoxSat.SelectedItem;
      LoadTransponders(sat.FileName);
      _transponders.Sort();
      foreach (Transponder transponder in _transponders)
      {
        comboBox1.Items.Add(transponder);
      }
      if (comboBox1.Items.Count > 0)
        comboBox1.SelectedIndex = 0;
      bool eventsEnabled = _enableEvents;
      _enableEvents = true;
      comboBox1_SelectedIndexChanged(null, null);
      _enableEvents = eventsEnabled;
    }

    private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (_enableEvents == false) return;
      Transponder transponder = (Transponder)comboBox1.SelectedItem;
      TvBusinessLayer layer = new TvBusinessLayer();
      DVBSChannel tuneChannel = new DVBSChannel();
      tuneChannel.Frequency = transponder.CarrierFrequency;
      tuneChannel.Polarisation = transponder.Polarisation;
      tuneChannel.SymbolRate = transponder.SymbolRate;
      tuneChannel.BandType = BandType.Universal;
      tuneChannel.DisEqc = DisEqcType.None;
      RemoteControl.Instance.TuneScan(_cardNumber, tuneChannel);

    }

    private void buttonStop_Click(object sender, EventArgs e)
    {
      if (_enableEvents == false) return;
      RemoteControl.Instance.DiSEqCStopMotor(_cardNumber);
      comboBox1_SelectedIndexChanged(null, null);
    }

    private void buttonGotoStart_Click(object sender, EventArgs e)
    {
      if (_enableEvents == false) return;
      RemoteControl.Instance.DiSEqCGotoReferencePosition(_cardNumber);
      comboBox1_SelectedIndexChanged(null, null);

    }

    private void buttonUp_Click(object sender, EventArgs e)
    {
      if (_enableEvents == false) return;
      //move motor up
      RemoteControl.Instance.DiSEqCDriveMotor(_cardNumber, DiSEqCDirection.Up, (byte)(1 + comboBoxStepSize.SelectedIndex));
    }

    private void buttonDown_Click(object sender, EventArgs e)
    {
      if (_enableEvents == false) return;
      //move motor up
      RemoteControl.Instance.DiSEqCDriveMotor(_cardNumber, DiSEqCDirection.Down, (byte)(1 + comboBoxStepSize.SelectedIndex));
    }

    private void comboBoxStepSize_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (_enableEvents == false) return;
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "motorStepSize", "10");
      setting.Value = String.Format("{0}", (1 + comboBoxStepSize.SelectedIndex));
      setting.Persist();

    }

    private void checkBox1_CheckedChanged(object sender, EventArgs e)
    {
      comboBoxSat.Enabled = checkBox1.Checked;
      comboBox1.Enabled = checkBox1.Checked;
      buttonGoto.Enabled = checkBox1.Checked;
      comboBoxStepSize.Enabled = checkBox1.Checked;
      buttonUp.Enabled = checkBox1.Checked;
      buttonDown.Enabled = checkBox1.Checked;
      buttonMoveWest.Enabled = checkBox1.Checked;
      buttonMoveEast.Enabled = checkBox1.Checked;
      buttonStop.Enabled = checkBox1.Checked;
      checkBoxEnabled.Enabled = checkBox1.Checked;
      buttonGotoStart.Enabled = checkBox1.Checked;
      buttonStore.Enabled = checkBox1.Checked;
      buttonSetWestLimit.Enabled = checkBox1.Checked;
      buttonSetEastLimit.Enabled = checkBox1.Checked;

      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "motorEnabled", "no");
      if (checkBox1.Checked) setting.Value = "yes";
      else setting.Value = "no";
      setting.Persist();
    }

    bool reentrant = false;
    DateTime _signalTimer = DateTime.MinValue;
    private void timer1_Tick(object sender, EventArgs e)
    {
      if (reentrant) return;
      try
      {
        reentrant = true;
        TimeSpan ts = DateTime.Now - _signalTimer;
        if (ts.TotalMilliseconds > 500)
        {
          RemoteControl.Instance.UpdateSignalSate(_cardNumber);
          _signalTimer = DateTime.Now;
        }
        UpdateStatus(1);
      }
      finally
      {
        reentrant = false;
      }
    }
    #endregion

    private void buttonReset_Click(object sender, EventArgs e)
    {
      RemoteControl.Instance.DiSEqCReset(_cardNumber);
    }
  }
}