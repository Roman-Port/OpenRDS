using RomanPort.OpenRDS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RomanPort.OpenRDSUI
{
    public partial class Form1 : Form
    {
        public FileStream stream;
        public RDSSession session;
        
        public Form1()
        {
            InitializeComponent();
            session = new RDSSession();
            stream = new FileStream(@"D:\RDS_DUMP_KZJK.bin", FileMode.Open);
            Decode();
        }

        public void Decode()
        {
            session.RDSFrameReceivedEvent += Session_RDSFrameReceivedEvent;
            FileStream o = new FileStream("D:\\RDS_FILTERED.bin", FileMode.Create);
            for(int i = 0; i<stream.Length / 8; i++)
            {
                var cmd = session.DecodeFrame(stream);
                if(cmd._header.groupType == 13)
                {
                    stream.Position -= 8;
                    byte[] buffer = new byte[8];
                    stream.Read(buffer, 0, 8);
                    o.Write(buffer, 0, 8);
                }
            }
            Console.WriteLine(session.piCode);
        }

        private void Session_RDSFrameReceivedEvent(OpenRDS.Framework.RDSCommand frame, RDSSession session)
        {
            Console.WriteLine(frame._header.groupType.ToString() + ":" + frame._header.isTypeA.ToString());
        }
    }
}
