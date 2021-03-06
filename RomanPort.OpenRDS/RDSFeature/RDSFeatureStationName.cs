﻿using RomanPort.OpenRDS.Framework.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.OpenRDS.RDSFeature.StationName
{
    public class RDSFeatureStationName
    {
        /// <summary>
        /// The buffer used, may be invalid
        /// </summary>
        public char[] stationNameBuffer;

        /// <summary>
        /// The full, current station name
        /// </summary>
        public string stationName;

        /// <summary>
        /// The most common event; Notifies users when a new name is fully recieved
        /// </summary>
        public event RDSFeatureStationName_StationNameUpdatedEventArgs RDSFeatureStationName_StationNameUpdatedEvent;

        /// <summary>
        /// Notifies users when a part of the buffer is updated
        /// </summary>
        public event RDSFeatureStationName_StationBufferUpdatedEventArgs RDSFeatureStationName_StationBufferUpdatedEvent;

        /// <summary>
        /// Has the first chunk of the station name been decoded?
        /// </summary>
        private bool _firstChunkDecoded;

        public RDSFeatureStationName(RDSSession session)
        {
            stationNameBuffer = new char[8];
            _firstChunkDecoded = false;
            session.RDSFrameReceivedEvent += Session_RDSFrameReceivedEvent;
            session.RDSSessionResetEvent += Session_RDSSessionResetEvent;
        }

        private void Session_RDSSessionResetEvent(RDSSession session)
        {
            stationName = null;
            _firstChunkDecoded = false;
        }

        private void Session_RDSFrameReceivedEvent(Framework.RDSCommand frame, RDSSession session)
        {
            //Get type
            if (frame.GetType() != typeof(BasicDataRDSCommand))
                return;
            BasicDataRDSCommand cmd = (BasicDataRDSCommand)frame;

            //Set data in buffer
            stationNameBuffer[cmd.stationNameIndex] = cmd.letterA;
            stationNameBuffer[cmd.stationNameIndex+1] = cmd.letterB;

            //Set chunk flag
            if (cmd.stationNameIndex == 0)
                _firstChunkDecoded = true;

            //Update final station name, if any
            if (cmd.stationNameIndex == 6 && _firstChunkDecoded)
            {
                stationName = new string(stationNameBuffer);
                RDSFeatureStationName_StationNameUpdatedEvent?.Invoke(stationName);
            }

            //Send event
            RDSFeatureStationName_StationBufferUpdatedEvent?.Invoke(stationNameBuffer, cmd.stationNameIndex);
        }
    }

    public delegate void RDSFeatureStationName_StationNameUpdatedEventArgs(string name);
    public delegate void RDSFeatureStationName_StationBufferUpdatedEventArgs(char[] buffer, int updatePos);
}
