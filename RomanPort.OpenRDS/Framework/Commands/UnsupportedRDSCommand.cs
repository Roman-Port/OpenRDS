using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.OpenRDS.Framework.Commands
{
    public class UnsupportedRDSCommand : RDSCommand
    {
        //Represents just an unsupported command
        internal override void ReadCommand(int groupBSpecial, ushort groupC, ushort groupD)
        {
            //Do nothing...
        }
    }
}
