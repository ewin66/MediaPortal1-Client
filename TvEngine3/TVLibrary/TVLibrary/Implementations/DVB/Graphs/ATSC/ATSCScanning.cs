#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using TvLibrary.Interfaces;
using TvLibrary.Interfaces.Analyzer;
using TvLibrary.Channels;
using TvLibrary.Implementations.DVB.Structures;

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// Class which implements scanning for tv/radio channels for ATSC BDA cards
  /// </summary>
  public class ATSCScanning : DvbBaseScanning, ITVScanning, IDisposable
  {
    private readonly TvCardATSC _card;

    /// <summary>
    /// Initializes a new instance of the <see cref="ATSCScanning"/> class.
    /// </summary>
    /// <param name="card">The card.</param>
    public ATSCScanning(TvCardATSC card)
      : base(card)
    {
      _card = card;
      _enableWaitForVCT = true;
    }

    /// <summary>
    /// returns the tv card used
    /// </summary>
    /// <value></value>
    public ITVCard TvCard
    {
      get { return _card; }
    }

    /// <summary>
    /// Gets the analyzer.
    /// </summary>
    /// <returns></returns>
    protected override ITsChannelScan GetAnalyzer()
    {
      return _card.StreamAnalyzer;
    }

    /// <summary>
    /// Resets the signal update.
    /// </summary>
    protected override void ResetSignalUpdate()
    {
      _card.ResetSignalUpdate();
    }

    /// <summary>
    /// Creates the new channel.
    /// </summary>
    /// <param name="info">The info.</param>
    /// <returns></returns>
    protected override IChannel CreateNewChannel(ChannelInfo info)
    {
      ATSCChannel tuningChannel = (ATSCChannel)_card.CurrentChannel;
      ATSCChannel atscChannel = new ATSCChannel();
      atscChannel.Name = info.service_name;
      atscChannel.LogicalChannelNumber = info.LCN;
      atscChannel.Provider = info.service_provider_name;
      atscChannel.ModulationType = tuningChannel.ModulationType;
      atscChannel.Frequency = tuningChannel.Frequency;
      atscChannel.PhysicalChannel = tuningChannel.PhysicalChannel;
      atscChannel.MajorChannel = info.majorChannel;
      atscChannel.MinorChannel = info.minorChannel;
      atscChannel.IsTv = (info.serviceType == (int)ServiceType.Video ||
                          info.serviceType == (int)ServiceType.Mpeg2HDStream ||
                          info.serviceType == (int)ServiceType.H264Stream ||
                          info.serviceType == (int)ServiceType.AdvancedCodecHDVideoStream ||
                          info.serviceType == (int)ServiceType.Mpeg4OrH264Stream);
      atscChannel.IsRadio = (info.serviceType == (int)ServiceType.Audio);
      atscChannel.NetworkId = info.networkID;
      atscChannel.ServiceId = info.serviceID;
      atscChannel.TransportId = info.transportStreamID;
      atscChannel.PmtPid = info.network_pmt_PID;
      atscChannel.FreeToAir = !info.scrambled;
      Log.Log.Write("atsc:Found: {0}", atscChannel);
      return atscChannel;
    }
  }
}