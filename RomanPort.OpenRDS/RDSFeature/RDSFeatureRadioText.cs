using RomanPort.OpenRDS.Framework.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.OpenRDS.RDSFeature.RadioText
{
    public class RDSFeatureRadioText
    {
        /// <summary>
        /// The internal working buffer
        /// </summary>
        public char[] textBuffer;

        /// <summary>
        /// The latest full buffer
        /// </summary>
        public string radioText;

        /// <summary>
        /// The most common event. Used when we finish reading a radio text
        /// </summary>
        public event RDSFeatureRadioText_RadioTextUpdatedEventArgs RDSFeatureRadioText_RadioTextUpdatedEvent;

        /// <summary>
        /// Used when we update a segment of the buffer
        /// </summary>
        public event RDSFeatureRadioText_RadioTextBufferUpdatedEventArgs RDSFeatureRadioText_RadioTextBufferUpdatedEvent;

        /// <summary>
        /// Used when we clear the display
        /// </summary>
        public event RDSFeatureRadioText_RadioTextClearedUpdatedEventArgs RDSFeatureRadioText_RadioTextClearedUpdatedEvent;

        /// <summary>
        /// Has the first chunk of the station name been decoded?
        /// </summary>
        private bool _firstChunkDecoded;

        /// <summary>
        /// The last A/B clear flag sent. Null if unknown
        /// </summary>
        private bool? _lastClearFlag;

        public RDSFeatureRadioText(RDSSession session)
        {
            textBuffer = new char[64];
            _firstChunkDecoded = false;
            _lastClearFlag = null;
            session.RDSFrameReceivedEvent += Session_RDSFrameReceivedEvent;
            session.RDSSessionResetEvent += Session_RDSSessionResetEvent;
        }

        private void Session_RDSSessionResetEvent(RDSSession session)
        {
            radioText = null;
            _firstChunkDecoded = false;
        }

        private void Session_RDSFrameReceivedEvent(Framework.RDSCommand frame, RDSSession session)
        {
            //Get type
            if (frame.GetType() != typeof(RadioTextRDSCommand))
                return;
            RadioTextRDSCommand cmd = (RadioTextRDSCommand)frame;

            //Set if the clear flag has been changed
            //http://www.interactive-radio-system.com/docs/EN50067_RDS_Standard.pdf defines that a screen clear should happen
            if (_lastClearFlag != cmd.clear)
            {
                //Clear the buffer
                for (var i = 0; i < textBuffer.Length; i++)
                    textBuffer[i] = (char)0x00;

                //Set the buffer
                _lastClearFlag = cmd.clear;
                _firstChunkDecoded = false;

                //Send event
                RDSFeatureRadioText_RadioTextClearedUpdatedEvent?.Invoke();
            } else
            {
                //Set data in buffer
                textBuffer[cmd.offset + 0] = cmd.letterA;
                textBuffer[cmd.offset + 1] = cmd.letterB;
                textBuffer[cmd.offset + 2] = cmd.letterC;
                textBuffer[cmd.offset + 3] = cmd.letterD;

                //Set chunk flag
                if (cmd.offset == 0)
                    _firstChunkDecoded = true;

                //Update final station name, if any
                //http://www.interactive-radio-system.com/docs/EN50067_RDS_Standard.pdf defines that the final message must end with \r. We use that to determine when the message is fully written
                if ((cmd.letterA == '\r' || cmd.letterB == '\r' || cmd.letterC == '\r' || cmd.letterD == '\r') && _firstChunkDecoded)
                {
                    radioText = new string(textBuffer);
                    RDSFeatureRadioText_RadioTextUpdatedEvent?.Invoke(radioText);
                }
            }

            //Send event
            RDSFeatureRadioText_RadioTextBufferUpdatedEvent?.Invoke(textBuffer, cmd.offset, 4);
        }

        public delegate void RDSFeatureRadioText_RadioTextUpdatedEventArgs(string text);
        public delegate void RDSFeatureRadioText_RadioTextBufferUpdatedEventArgs(char[] buffer, int updatePos, int updateCount);
        public delegate void RDSFeatureRadioText_RadioTextClearedUpdatedEventArgs();
    }
}
