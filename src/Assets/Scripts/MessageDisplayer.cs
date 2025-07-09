using UnityEngine;
using System.Collections;

// Use este script em objeto guiText para tem uma mensagem de status
// Basta chamar messageDisplayerObject.DisplayMessage("hello") e voce
// tera uma linha de texto que desaparece sozinha.

public class MessageDisplayer : MonoBehaviour
{
	ArrayList messages = new ArrayList();
	
	public void DisplayMessage(string message)
	{
		messages.Add(message);
		UpdateDisplay();
		Invoke("DeleteOldestMessage", 5F);
	}
	
	void DeleteOldestMessage()
	{
		// The following "if statement" protects this function from
		// getting called by SendMessage from another script and
		// crashing.
		if (messages.Count > 0)
		{
			messages.RemoveAt(0);
			UpdateDisplay();
		}
	}
	
	void UpdateDisplay()
	{
		string formattedMessages = "";
		
		foreach (string message in messages)
		{
			formattedMessages += message + "\n";
		}
		
		// For older Unity versions, guiText was a built-in property of MonoBehaviour
		guiText.text = formattedMessages;
	}
}