using System;

namespace API.Models.Dump;

public class ChatData
{
    public string Question { set; get; }
    public string Answer { set; get; }
    public DateTime AnsweredAt { set; get; }
    public string AnsweredBy { set; get; }
    public string AnswerOption { set; get; }
}