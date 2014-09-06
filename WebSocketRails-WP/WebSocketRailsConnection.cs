using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using WebSocket4Net;

namespace WebSocketRails
{
    public class WebSocketRailsConnection
    {
        private Uri uri;
	    private WebSocketRailsDispatcher dispatcher;
	    private List<WebSocketRailsEvent> message_queue;
	    private WebSocket webSocket;
	
	    public WebSocketRailsConnection(Uri uri, WebSocketRailsDispatcher dispatcher) 
        {
            this.uri = uri;
            this.dispatcher = dispatcher;
            this.message_queue = new List<WebSocketRailsEvent>();

            webSocket = new WebSocket(uri.ToString(), "", "");
            webSocket.Closed += webSocket_Closed;
            webSocket.MessageReceived += webSocket_MessageReceived;
	    }

        void webSocket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            List<Object> list = JsonConvert.DeserializeObject<List<Object>>(e.Message);

            dispatcher.NewMessage(list);
        }

        void webSocket_Closed(object sender, EventArgs e)
        {
            List<Object> data = new List<Object>();
            data.Add("connection_closed");
            data.Add(new Dictionary<String, Object>());

            WebSocketRailsEvent closeEvent = new WebSocketRailsEvent(data);
            dispatcher.State = "disconnected";
            dispatcher.Dispatch(closeEvent);            
        }

	    public void Trigger(WebSocketRailsEvent _event) 
        {
	        if (dispatcher.State == "connected")
	            message_queue.Add(_event);
	        else
	            webSocket.Send(_event.Serialize());		
	    }
	
	    public void FlushQueue(String id) 
        {
	        foreach (WebSocketRailsEvent _event in message_queue)
	        {
	            String serializedEvent = _event.Serialize();
	            webSocket.Send(serializedEvent);
	        }		
	    }

        public void Connect()
        {
            webSocket.Open();
        }

	    public void Disconnect() 
        {
		    webSocket.Close();
	    }
    }
}
