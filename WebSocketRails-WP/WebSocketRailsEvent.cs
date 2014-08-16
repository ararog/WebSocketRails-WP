﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSocketRails
{
    public class WebSocketRailsEvent
    {
        private bool result;

        private EventHandler<WebSocketRailsDataEventArgs> onEventSuccess;
        private EventHandler<WebSocketRailsDataEventArgs> onEventFailure;

        public WebSocketRailsEvent(Object data, EventHandler<WebSocketRailsDataEventArgs> onEventSuccess, EventHandler<WebSocketRailsDataEventArgs> onEventFailure)
	    {
		    if(data is List<Object>) 
            {
			    List<Object> listOfData = (List<Object>) data;
			
			    Name = (String) listOfData[0];
	            Attr = (Dictionary<String, Object>) listOfData[1];
	        
	            if (Attr != null)
	            {
	                if (Attr["id"] != null)
	                    Id = (int) Attr["id"];
	                else
	                    Id = (int) new Random().Next();
	            
	                if (Attr["channel"] != null)
	                    Channel = (String) Attr["channel"];
	            
	                if (Attr["data"] != null)
	                    Data = Attr["data"];
	            
	                if (Attr["token"] != null)
	                    Token = (String) Attr["token"];	            
	            
	                if (listOfData.Count > 2 && listOfData[2] != null)
	                    ConnectionId = (String) listOfData[2];
	                else
	                    ConnectionId = "";
	            
	                if (Attr["success"] != null)
	                {
	                    result = true;
	                    IsSuccess = (Boolean) Attr["success"];
	                }
	            }

                this.onEventSuccess = onEventSuccess;
                this.onEventFailure = onEventFailure;
		    }
    	}

        public WebSocketRailsEvent(Object data)
            : this(data, null, null)
        {

        }

        public bool IsPing()
        {
            return Name == "websocket_rails.ping";
        }

        public String Serialize()
        {
            List<Object> array = new List<Object>();

            array.Add(Name);
            array.Add(this.Attributes());

            return JsonConvert.SerializeObject(array);
        }

        public Object Attributes()
        {
            Dictionary<String, Object> attributes = new Dictionary<String, Object>();

            attributes["id"] = Id;
            attributes["channel"] = Channel;
            attributes["data"] = Data;
            attributes["token"] = Token;

            return attributes;
        }

        public void RunCallbacks(bool success, Object eventData)
        {
            if (success && onEventSuccess != null)
                FireEventSuccess(eventData);
            else
            {
                FireEventFailure(eventData);
            }
        }

        public String Name { get; set;}

        public Dictionary<String, Object> Attr { get; set; }

        public int Id{ get; set;}

        public String Channel{ get; set;}

        public Object Data{ get; set;}

        public Object Token { get; set; }

        public String ConnectionId { get; set; }

        public bool IsSuccess{ get; set;}

        public bool IsChannel
        {
            get
            {
                return Channel != null;
            }
        }

        public bool IsResult
        {
            get
            {
                return result;
            }
        }

        public event EventHandler<WebSocketRailsDataEventArgs> EventSuccess
        {
            add { onEventSuccess += value; }
            remove { onEventSuccess -= value; }
        }

        internal void FireEventSuccess(Object data)
        {
            if (onEventSuccess == null)
                return;

            onEventSuccess(this, new WebSocketRailsDataEventArgs(data));
        }

        public event EventHandler<WebSocketRailsDataEventArgs> EventFailure
        {
            add { onEventFailure += value; }
            remove { onEventFailure -= value; }
        }

        internal void FireEventFailure(Object data)
        {
            if (onEventFailure == null)
                return;

            onEventFailure(this, new WebSocketRailsDataEventArgs(data));
        }
    }
}
