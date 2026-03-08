using System;
using UnityEngine;

[Serializable]
public class NoteData
{
    [SerializeField] private string noteId = "";
    [SerializeField] private string title = "Nota";
    [SerializeField] [TextArea(6, 20)] private string content = "Contenido de la nota...";

    public string NoteId => noteId;
    public string Title => title;
    public string Content => content;

    public NoteData()
    {
    }

    public NoteData(string id, string noteTitle, string noteContent)
    {
        noteId = id;
        title = string.IsNullOrWhiteSpace(noteTitle) ? "Nota" : noteTitle;
        content = string.IsNullOrWhiteSpace(noteContent) ? "Contenido de la nota..." : noteContent;
    }
}
