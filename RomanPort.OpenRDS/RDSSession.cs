using RomanPort.OpenRDS.Framework;
using RomanPort.OpenRDS.RDSFeature.RadioText;
using RomanPort.OpenRDS.RDSFeature.StationName;
using System;

namespace RomanPort.OpenRDS
{
    public class RDSSession
    {
        /// <summary>
        /// The program identification code
        /// </summary>
        public ushort? piCode;

        /// <summary>
        /// Set to true when ANY valid RDS frame is found
        /// </summary>
        public bool rdsSupported;

        /// <summary>
        /// The station name type
        /// </summary>
        public RDSFeatureStationName featureStationName;

        /// <summary>
        /// The station radio text
        /// </summary>
        public RDSFeatureRadioText featureRadioText;

        /// <summary>
        /// Called when we get any frame
        /// </summary>
        public event RDSFrameReceivedEventArgs RDSFrameReceivedEvent;

        /// <summary>
        /// Called when we reset the RDS session. Features should clear data
        /// </summary>
        public event RDSSessionResetEventArgs RDSSessionResetEvent;

        public RDSSession()
        {
            featureStationName = new RDSFeatureStationName(this);
            featureRadioText = new RDSFeatureRadioText(this);
        }

        public RDSCommand DecodeFrame(RDSFrame frame)
        {
            //Decode
            RDSCommand cmd = RDSCommand.ReadRdsFrame(frame);

            //Update PI code and supported flag
            piCode = cmd.programIdentificationCode;
            rdsSupported = true;

            //Send events
            RDSFrameReceivedEvent?.Invoke(cmd, this);

            return cmd;
        }

        /// <summary>
        /// Reads eight bytes from a stream and decodes the command
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public RDSCommand DecodeFrame(System.IO.Stream stream)
        {
            //Read information
            byte[] buffer = new byte[8];
            stream.Read(buffer, 0, 8);

            //Decode
            return DecodeFrame(buffer);
        }

        /// <summary>
        /// Decodes an 8-byte frame
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public RDSCommand DecodeFrame(byte[] buffer)
        {
            //Get the frame out
            RDSFrame frame = new RDSFrame
            {
                GroupA = BitConverter.ToUInt16(buffer, 0),
                GroupB = BitConverter.ToUInt16(buffer, 2),
                GroupC = BitConverter.ToUInt16(buffer, 4),
                GroupD = BitConverter.ToUInt16(buffer, 6)
            };

            return DecodeFrame(frame);
        }
        
        /// <summary>
        /// Clears out information, but will not stop events. This should be called when a new station is tuned to
        /// </summary>
        public void Reset()
        {
            piCode = null;
            rdsSupported = false;
            RDSSessionResetEvent?.Invoke(this);
        }
    }

    public delegate void RDSFrameReceivedEventArgs(RDSCommand frame, RDSSession session);
    public delegate void RDSSessionResetEventArgs(RDSSession session);
}
