using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSocketRails
{
    public class WebSocketRailsDispatcher
    {
	    private String state;
	    private Uri url;
	    private Dictionary<String, WebSocketRailsChannel> channels;
	    private String connectionId;
	    private Dictionary<int, WebSocketRailsEvent> queue;
        private Dictionary<String, List<EventHandler<WebSocketRailsDataEventArgs>>> callbacks;
	    private WebSocketRailsConnection connection;
	
	    public WebSocketRailsDispatcher(Uri url) 
        {
            this.url = url;
            state = "connecting";
            channels = new Dictionary<String, WebSocketRailsChannel>();
            queue = new Dictionary<int, WebSocketRailsEvent>();
            callbacks = new Dictionary<String, List<EventHandler<WebSocketRailsDataEventArgs>>>();
        
            connection = new WebSocketRailsConnection(url, this);
            connectionId = "";
	    }

	    public void NewMessage(List<Object> data)
        {
	        foreach (Object socket_message in data)
	        {
	            WebSocketRailsEvent _event = new WebSocketRailsEvent(socket_message);
	        
	            if (_event.IsResult)
	            {
	                if (queue[_event.Id] != null)
	                {
	                    queue[_event.Id].RunCallbacks(_event.IsSuccess, _event.Data);
	                    queue.Remove(_event.Id);
	                }
	            } else if (_event.IsChannel) {
	                DispatchChannel(_event);
	            } else if (_event.IsPing()) {
	                Pong();
	            } else {
	                Dispatch(_event);
	            }
	        
	            if (state == "connecting" && _event.Name == "client_connected")
	                ConnectionEstablished(_event.Data);
	        }		
	    }
	
	    public void ConnectionEstablished(Object data) 
        {
	        state = "connected";
	        if(data.GetType() == typeof(Dictionary<String, Object>)) {

                Dictionary<String, Object> infoDictionary = (Dictionary<String, Object>) data;
	    	
		        connectionId = (String) infoDictionary["connection_id"];
		        connection.FlushQueue(connectionId);
	        }
	    }

        public void Bind(String eventName, EventHandler<WebSocketRailsDataEventArgs> callback)
        {
	        if (callbacks[eventName] == null)
                callbacks[eventName] = new List<EventHandler<WebSocketRailsDataEventArgs>>();
	    
	        callbacks[eventName].Add(callback);		
	    }

        public void Trigger(String eventName, Object data, EventHandler<WebSocketRailsDataEventArgs> success, EventHandler<WebSocketRailsDataEventArgs> failure)
        {
		    List<Object> frame = new List<Object>();
		    frame.Add(eventName);
		    frame.Add(data);
		    frame.Add(connectionId);
		
	        WebSocketRailsEvent _event = new WebSocketRailsEvent(frame, success, failure);
	        queue[_event.Id] = _event;
	        connection.Trigger(_event);
	    }
	
	    public void TriggerEvent(WebSocketRailsEvent _event) 
        {
	         if (queue[_event.Id] != null && queue[_event.Id] == _event)
	             return;
	     
	         queue[_event.Id] = _event;
	         connection.Trigger(_event);		
	    }
	
	    public void Dispatch(WebSocketRailsEvent _event) 
        {
	        if (callbacks[_event.Name] == null)
	            return;

            foreach (EventHandler<WebSocketRailsDataEventArgs> callback in callbacks[_event.Name])
	        {
	            callback(this, new WebSocketRailsDataEventArgs(_event.Data));
	        }		
	    }
	
	    public WebSocketRailsChannel Subscribe(String channelName) 
        {
	        if (channels[channelName] == null)
	            return channels[channelName];
	    
	        WebSocketRailsChannel channel = new WebSocketRailsChannel(channelName, this, false);
	    
	        channels[channelName] = channel;
	    
	        return channel;
	    }
	
	    public void Unsubscribe(String channelName) 
        {
	        if (channels[channelName] == null)
	            return;
	    
	        channels[channelName].Destroy();
	        channels.Remove(channelName);		
	    }

	    private void DispatchChannel(WebSocketRailsEvent _event)
	    {
	        if (channels[_event.Channel] == null)
	            return;
	    
	        channels[_event.Channel].Dispatch(_event.Name, _event.Data);
	    }

	    private void Pong()
	    {
		    List<Object> frame = new List<Object>();
		    frame.Add("websocket_rails.pong");
		    frame.Add(new Dictionary<String, Object>());
		    frame.Add(connectionId);
		
	        WebSocketRailsEvent pong = new WebSocketRailsEvent(frame);
	        connection.Trigger(pong);
	    }

	    public void Disconnect()
	    {
	        connection.Disconnect();
	    }
	
	    public String State { get; set; }
		    
	    public Uri Uri { get; set; }
	
	    public Dictionary<String, WebSocketRailsChannel> Channels { get; set; }
	
	    public String ConnectionId { get; set; }

    }
}
