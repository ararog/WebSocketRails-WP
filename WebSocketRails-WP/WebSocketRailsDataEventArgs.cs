using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSocketRails
{
    public class WebSocketRailsDataEventArgs : EventArgs
    {
        public WebSocketRailsDataEventArgs(Object data)
        {
            Data = data;
        }

        public Object Data { get; private set; }
    }
}
