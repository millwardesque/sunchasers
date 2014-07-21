using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// A message that can be sent.
/// </summary>
public class Message {
	
	public GameObject MessageSource 	{get; set;}
	public string MessageName   		{get; set;}
	public string MessageValue			{get; set;}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="Message"/> class.
	/// </summary>
	/// <param name='messageSource'>
	/// The object sending the message
	/// </param>
	/// <param name='messageName'>
	/// Message name.
	/// </param>
	/// <param name='messageValue'>
	/// Message value.
	/// </param>
	public Message(GameObject messageSource, string messageName, string messageValue)
	{
		this.MessageSource = messageSource;
		this.MessageName = messageName;
		this.MessageValue = messageValue;
	}
	
	public override string ToString () {
		 return string.Format ("Name: {0} Value: {1} From: {2}", MessageName, MessageValue, MessageSource);
	}
}

/// <summary>
/// A message listener.
/// </summary>
public class Listener {
	
	public string ListenFor { get; set; }
	public GameObject Recipient { get; set; }
	public string RecipientMethod { get; set; }
	
	/// <summary>
	/// Initializes a new instance of the <see cref="Listener"/> class.
	/// </summary>
	/// <param name='messageName'>
	/// Message name to listen for.
	/// </param>
	/// <param name='recipient'>
	/// Message Recipient.
	/// </param>
	/// <param name='recipientMethod'>
	/// Recipient method to call when the message arrives.
	/// </param>
	public Listener(string messageName, GameObject recipient, string recipientMethod)
	{
		this.ListenFor = messageName;
		this.Recipient = recipient;
		this.RecipientMethod = recipientMethod;
	}
}

/// <summary>
/// Message manager.
/// </summary>
public sealed class MessageManager {
	
	// Make the message-manager a singleton
	private static readonly MessageManager instance = new MessageManager();
	private MessageManager() {}
	public static MessageManager Instance
	{
		get { return instance; }
	}
	
	private List<Listener> listeners = new List<Listener>();
	
	/// <summary>
	/// Registers the listener.
	/// </summary>
	/// <param name='listener'>
	/// The listener to register
	/// </param>
	public void RegisterListener(Listener listener)
	{
		listeners.Add(listener);
	}
	
	/// <summary>
	/// Sends a message to listeners.
	/// </summary>
	/// <param name='message'>
	/// The message to send.
	/// </param>
	public void SendToListeners(Message message)
	{
		int listenerCount = 0;
		foreach (var recipient in listeners.FindAll(listener => listener.ListenFor == message.MessageName))  
		{
		    recipient.Recipient.BroadcastMessage(recipient.RecipientMethod, message, SendMessageOptions.DontRequireReceiver);
			listenerCount++;
		}
		
		Debug.Log (string.Format ("MESSAGE <L {0}>: {1}", listenerCount, message));
	}
}
