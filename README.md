WebSocketRails-WP
=================

A Windows Phone client of WebSocket-Rails

Since data exchange is JSON based, it's strongly recommended to use JSON.NET
API to deserialize data.

Here's an example:

```
public void Initialize() 
{
	WebSocketRailsDispatcher dispatcher = new WebSocketRailsDispatcher(new Uri("ws://192.168.100.109:3000/websocket"));

	channel = dispatcher.Subscribe("my_channel");

	Notification notification = new Notification();

	Contact contact = new Contact();
	contact.Id = 007;
	contact.FirstName = "James";
	contact.LastName = "Bond";
	contact.CountryCode = "+1";
	contact.PhoneNumber = "11254526";

	notification.Contact = contact;
	notification.Type = 1;
	notification.Content = "Bond, James";

	channel.Trigger("notification_event", notification);

	channel.Bind("notification_event", messageReceived);
}

private void messageReceived(object sender, WebSocketRailsDataEventArgs e)
{
	if(e.Data is JObject) 
	{
		Notification notification =JsonConvert.DeserializeObject<Notification>(e.Data);
		
		// do something
	}
}

```