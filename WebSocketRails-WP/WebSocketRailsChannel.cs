using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSocketRails
{
    public class WebSocketRailsChannel
    {
	    private String eventName;
        private Dictionary<String, List<EventHandler<WebSocketRailsDataEventArgs>>> callbacks;
	    private String channelName;
	    private String token;
	    private WebSocketRailsDispatcher dispatcher;
	
	    public WebSocketRailsChannel(String channelName, WebSocketRailsDispatcher dispatcher, bool isPrivate)
	    {
            String eventName = null;
            if (isPrivate)
                eventName = "websocket_rails.subscribe_private";
            else
                eventName = "websocket_rails.subscribe";
        
            this.channelName = channelName;
            this.dispatcher = dispatcher;

            List<Object> frame = new List<Object>();
            frame.Add(eventName);

            Dictionary<String, Object> data = new Dictionary<String, Object>();
        
            Dictionary<String, Object> info = new Dictionary<String, Object>();
            info["channel"] = channelName;
        
            data["data"] = info;

            frame.Add(data);
            frame.Add(dispatcher.ConnectionId);
        
            WebSocketRailsEvent _event = new WebSocketRailsEvent(frame, null, null);

            callbacks = new Dictionary<String, List<EventHandler<WebSocketRailsDataEventArgs>>>();
    
            dispatcher.TriggerEvent(_event);
	    }

        public void Bind(String eventName, EventHandler<WebSocketRailsDataEventArgs> callback)
        {
		
	        if (! callbacks.ContainsKey(eventName))
                callbacks[eventName] = new List<EventHandler<WebSocketRailsDataEventArgs>>();
	    
	        callbacks[eventName].Add(callback);
	    }

	    public void Trigger(String eventName, Object message) 
        {
	        List<Object> frame = new List<Object>();
            frame.Add(eventName);

            Dictionary<String, Dictionary<String, Object>> data = new Dictionary<String, Dictionary<String, Object>>();
        
            Dictionary<String, Object> info = new Dictionary<String, Object>();
            info["channel"] = channelName;
            info["data"] = message;
            info["token"] = token;

            frame.Add(info);
            frame.Add(dispatcher.ConnectionId);
        
            WebSocketRailsEvent _event = new WebSocketRailsEvent(frame, null, null);
		
	        dispatcher.TriggerEvent(_event);		
	    }
	
	    public void Dispatch(String eventName, Object message) 
        {
	        if(eventName == "websocket_rails.channel_token") {
	        
	            Dictionary<String, Object> info = (message as JObject)
                    .ToObject<Dictionary<String, Object>>();
	            
                this.token = (String) info["token"];
	        }
	        else {
	            if (! callbacks.ContainsKey(eventName))
	                return;

                foreach (EventHandler<WebSocketRailsDataEventArgs> callback in callbacks[eventName])
	            {
	                callback(this, new WebSocketRailsDataEventArgs(message));
	            }
	        }		
	    }
	
	    public void Destroy() 
        {
	        String eventName = "websocket_rails.unsubscribe";
        
	        List<Object> frame = new List<Object>();
            frame.Add(eventName);

            Dictionary<String, Dictionary<String, Object>> data = new Dictionary<String, Dictionary<String, Object>>();
        
            Dictionary<String, Object> info = new Dictionary<String, Object>();
            info["channel"] = channelName;
                
            data["data"] = info;

            frame.Add(data);
            frame.Add(dispatcher.ConnectionId);
        
            WebSocketRailsEvent _event = new WebSocketRailsEvent(frame, null, null);
	    
	        dispatcher.TriggerEvent(_event);
	        callbacks.Clear();		
	    }
    }
}
